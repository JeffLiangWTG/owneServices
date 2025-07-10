using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core.Encryption;
using Ganss.Xss;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public enum CharacterCase { Lower, Normal, Upper }

	public class StringRegistryDataType : RegistryDataType<string>
	{
		public StringRegistryDataType()
			: this(string.Empty)
		{
		}

		public StringRegistryDataType(string defaultValue)
			: base(RegistryDataTypes.Codes.String, defaultValue)
		{
			MaxLength = int.MaxValue;
			CharacterCase = CharacterCase.Normal;
			IsEncrypted = false;
		}

		public StringRegistryDataType(bool isEncrypted)
			: this()
		{
			IsEncrypted = isEncrypted;
		}

		public StringRegistryDataType(int minLength, int maxLength)
			: this()
		{
			MinLength = minLength;
			MaxLength = maxLength;
		}

		public StringRegistryDataType(int minLength, int maxLength, bool isEncrypted)
			: this()
		{
			MinLength = minLength;
			MaxLength = maxLength;
			IsEncrypted = isEncrypted;
		}

		public StringRegistryDataType(CharacterCase characterCase)
			: this()
		{
			CharacterCase = characterCase;
		}

		public StringRegistryDataType(CharacterCase characterCase, bool isEncrypted)
			: this()
		{
			CharacterCase = characterCase;
			IsEncrypted = isEncrypted;
		}

		public StringRegistryDataType(CharacterCase characterCase, int minLength, int maxLength)
			: this()
		{
			CharacterCase = characterCase;
			MinLength = minLength;
			MaxLength = maxLength;
		}

		public StringRegistryDataType(CharacterCase characterCase, int minLength, int maxLength, bool isEncrypted)
			: this()
		{
			CharacterCase = characterCase;
			MinLength = minLength;
			MaxLength = maxLength;
			IsEncrypted = isEncrypted;
		}

		public StringRegistryDataType(bool readOnly, string overrideValue)
			: this()
		{
			ReadOnly = readOnly;
			OverrideValue = overrideValue;
		}

		public override bool IsNullDataRepresentation(object value)
		{
			if (value is byte[])
			{
				var asString = DeserialiseAndDecrypt((byte[])value);

				return asString == MagicNullString;
			}

			return false;
		}

		string DeserialiseAndDecrypt(byte[] value)
		{
			var asString = Encoding.Unicode.GetString(value);

			Exception reportException = null;
			if (IsEncrypted)
			{
				try
				{
					asString = Encoder.Decrypt(asString);
				}
				catch (CryptographicException ex) when (!ex.IsCriticalException())
				{
					reportException = ex;
				}
				catch (FormatException ex) when (!ex.IsCriticalException())
				{
					reportException = ex;
				}

				if (reportException != null)
				{
					ErrorReporter.ReportOnce("BadEncryptedRegistryValue", $"Cannot decrypt value of the encrypted registry", reportException);
					asString = string.Empty;
				}
			}

			return asString;
		}

		public override bool IsDefaultValueImmutable => true;
		public int MinLength { get; set; }
		public int MaxLength { get; set; }
		public CharacterCase CharacterCase { get; set; }
		public bool IsEncrypted { get; private set; }

		public bool ReadOnly { get; }
		public string OverrideValue { get; }

		TwoWayEncoder Encoder => encoder ?? (encoder = TwoWayEncoder.NewWithStandardInitialisationVector());
		TwoWayEncoder encoder;

		protected override byte[] SerialiseCore(string value)
		{
			value = value ?? MagicNullString;
			if (IsEncrypted)
			{
				value = Encoder.Encrypt(value);
			}

			return Encoding.Unicode.GetBytes(value);
		}

		protected override string DeserialiseCore(byte[] value)
		{
			string asString = DeserialiseAndDecrypt(value);

			return (asString == MagicNullString) ? null : asString;
		}

		protected override bool ValuesAreEqualCore(string a, string b)
		{
			return a == b;
		}

		protected override bool AllowNullCore
		{
			get { return true; }
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new TextRegistryEditorInfo(TextEditorType.TextBox);
		}

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			if (string.IsNullOrEmpty(proposedValue) && IsValueMandatory(registryItem))
			{
				throw new RegistryValidationException(Res.GetString("3dd28d1f-9003-4c11-bb4e-46d955c93a87", "Value cannot be empty."));
			}

			int length = (proposedValue == null) ? 0 : proposedValue.Length;
			if (length < MinLength || length > MaxLength)
			{
				string errorMessage;
				if (MinLength == MaxLength)
				{
					errorMessage = Res.GetString("038d05a4-08a4-433f-92d2-d60592f41bfb", "Length must be {0} characters.", MinLength);
				}
				else
				{
					errorMessage = Res.GetString("529aabe4-0e4a-401d-a95b-41c41d7db235", "Length must be between {0} and {1}.", MinLength, MaxLength);
				}
				throw new RegistryValidationException(errorMessage);
			}

			if (!string.IsNullOrEmpty(proposedValue))
			{
				var editorInfo = GetEditorInfo(registryItem);

				if (editorInfo != null)
				{
					if (editorInfo.EditorType == TextEditorType.DirectoryBrowser)
					{
						foreach (char invalidPathChar in Path.GetInvalidPathChars())
						{
							if (proposedValue.Contains(invalidPathChar.ToString()))
							{
								string invalidChar = (invalidPathChar == '\t') ? "TabSpace" : invalidPathChar.ToString();
								throw new RegistryValidationException(Res.GetString("1d601bf3-53c3-4ff7-9dd8-d0a6eb2c1a3a", "Illegal character '{0}' is not allowed in a directory path.", invalidChar));
							}
						}

						// NET8.0 - Test Exception Catches
						// This check is needed due to the difference in behavior between .NET 4.8 and .NET 8.0 version of DirectoryInfo constructor regarding FTP paths.
						// NOTE:
						//		This code isn't being encapsulated because assosiated tests are being inherited by NET 8 Projects
						//		Once this project is multi-targeted and building, this code can be encapsulated in #if NET
						if (proposedValue.StartsWith("ftp:", StringComparison.OrdinalIgnoreCase))
						{
							throw new RegistryValidationException(Res.GetString("InvalidFtpPath", "FTP paths are not allowed."));
						}

						try
						{
							DirectoryInfo directoryInfo = new DirectoryInfo(proposedValue);
						}
						catch (PathTooLongException e)
						{
							throw new RegistryValidationException(e.Message);
						}
						catch (ArgumentException e)
						{
							throw new RegistryValidationException(e.Message);
						}
						catch (NotSupportedException e)
						{
							throw new RegistryValidationException(e.Message);
						}
					}
					else if (editorInfo.EditorType == TextEditorType.Url)
					{
						if (!UrlValidation.IsValidAbsoluteHttpOrHttpsUrl(proposedValue))
						{
							throw new RegistryValidationException(Res.GetString("36371888-a965-40ec-8741-be2ed9efc649", "The URL is invalid, please input a valid URL that must be HTTPS or HTTP."));
						}
					}
					else if (editorInfo.EditorType == TextEditorType.HTML)
					{
						ValidateHTMLMarkup(proposedValue);
					}
					else if (editorInfo.EditorType.HasFlag(TextEditorType.Guid))
					{
						if (!Guid.TryParse(proposedValue, out _))
						{
							throw new RegistryValidationException(Res.GetString("7AE7B348-5A26-4604-BA16-85169C51197E", "The value is invalid, please input a valid GUID value."));
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html element")]
		static void ValidateHTMLMarkup(string proposedMarkup)
		{
			var sanitizer = new HtmlSanitizer();
			sanitizer.AllowDataAttributes = true;
			sanitizer.AllowedAttributes.Clear();
			sanitizer.AllowedTags.Clear();
			foreach (var allowedAttribute in new[] { "class", "href", "src", "target" })
			{
				sanitizer.AllowedAttributes.Add(allowedAttribute);
			}

			foreach (var allowedTag in new[] { "i", "b", "u", "ul", "li", "br", "h1", "h2", "h3", "h4", "h5", "h6", "span", "div", "p", "a", "img", "strong", "ol", "em" })
			{
				sanitizer.AllowedTags.Add(allowedTag);
			}

			var sanitizedMarkup = sanitizer.Sanitize(proposedMarkup);

			var decodedProposed = HttpUtility.HtmlDecode(proposedMarkup);
			var decodedSanitized = HttpUtility.HtmlDecode(sanitizedMarkup);

			var normalizedProposed = Regex.Replace(decodedProposed, @"\s", string.Empty);
			var normalizedSanitized = Regex.Replace(decodedSanitized, @"\s", string.Empty);

			if (!normalizedProposed.Equals(normalizedSanitized, StringComparison.OrdinalIgnoreCase))
			{
				throw new RegistryValidationException(Res.GetString("4e051968-1e46-4d67-8f23-3e50dbdc2943", "The HTML markup is invalid. Please only use the following allowed tags and attributes:{0}{0}{1}{0}{2}", System.Environment.NewLine, string.Join(", ", sanitizer.AllowedTags), string.Join(", ", sanitizer.AllowedAttributes)));
			}
		}

		TextRegistryEditorInfo GetEditorInfo(IRegistryItem registryItem)
		{
			if (registryItem is StringRegistryItem || registryItem is MultilingualStringRegistryItem)
			{
				return registryItem.EditorInfo as TextRegistryEditorInfo;
			}
			else if (HasDefaultEditorInfo)
			{
				return DefaultEditorInfo as TextRegistryEditorInfo;
			}

			return null;
		}

		bool IsValueMandatory(IRegistryItem registryItem)
		{
			return (registryItem != null) && registryItem.IsValueMandatory;
		}

		protected override void CheckConfigurationValidCore()
		{
			if (MinLength < 0)
			{
				throw new ArgumentException("MinLength can't be less than zero.");
			}
			if (MaxLength < 0)
			{
				throw new ArgumentException("MaxLength can't be less than zero.");
			}
			if (MinLength > MaxLength)
			{
				throw new ArgumentException("The minimum length should not be greater than the maximum length.");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Special NULL String")]
		internal const string MagicNullString = "*** NULL ***";
	}
}
