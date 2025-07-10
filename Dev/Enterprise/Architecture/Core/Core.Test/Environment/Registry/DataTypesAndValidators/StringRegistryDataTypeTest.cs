using System;
using System.IO;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core.Encryption;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(StringRegistryDataType))]
	public class StringRegistryDataTypeTest : RegistryDataTypeTestCase<StringRegistryDataType>
	{
		public void TestIsNullDataRepresentation()
		{
			var registryItem = new StringRegistryItem("dummy", null, null, null, new StringRegistryDataType(true), RegistryStorageFlags.System);
			var dataType = (StringRegistryDataType)registryItem.DataType;
			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			AssertEquals(false, dataType.IsNullDataRepresentation(Encoding.Unicode.GetBytes(encoder.Encrypt("abc"))));
			AssertEquals(false, dataType.IsNullDataRepresentation(Encoding.Unicode.GetBytes(encoder.Encrypt("null"))));
			AssertEquals(false, dataType.IsNullDataRepresentation(Encoding.Unicode.GetBytes(encoder.Encrypt(""))));

			AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
			AssertEquals(false, dataType.IsNullDataRepresentation(Encoding.Unicode.GetBytes(StringRegistryDataType.MagicNullString)));
			AssertEquals("BadEncryptedRegistryValue", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();

			AssertEquals(false, dataType.IsNullDataRepresentation(Encoding.Unicode.GetBytes("hello")));
			AssertEquals("BadEncryptedRegistryValue", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();

			AssertEquals(true, dataType.IsNullDataRepresentation(Encoding.Unicode.GetBytes(encoder.Encrypt(StringRegistryDataType.MagicNullString))));
			AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
		}

		public void TestEncryptedPassword()
		{
			var registryItem = new StringRegistryItem("dummy", null, null, null, new StringRegistryDataType(true), RegistryStorageFlags.System);
			registryItem.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
			var dataType = (StringRegistryDataType)registryItem.DataType;

			AssertSerialiseDeserialise(dataType, "");
			AssertSerialiseDeserialise(dataType, "abc");
			AssertSerialiseDeserialise(dataType, "123");
			AssertSerialiseDeserialise(dataType, "AAAbbbCCC");
			AssertSerialiseDeserialise(dataType, "!@#$%^&*()_");
		}

		void AssertSerialiseDeserialise(StringRegistryDataType dataType, string value)
		{
			var bytes = dataType.Serialise(value);
			var normalBytes = Encoding.Unicode.GetBytes(value ?? StringRegistryDataType.MagicNullString);

			AssertNotEquals(bytes, normalBytes);

			var value1 = dataType.Deserialise(bytes);

			string value2 = Encoding.Unicode.GetString(normalBytes);
			value2 = (value2 == StringRegistryDataType.MagicNullString) ? null : value2;

			AssertEquals(value, value1);
			AssertEquals(value, value2);
		}

		public void TestEqualsDoesntBlowUpWithZStrings()
		{
			const string s1 = "First", s2 = "Second";
			ZString zs1 = new ZString(s1), zs2 = new ZString(s2);

			var dataType = new StringRegistryDataType();

			AssertValuesAreEqual(dataType, s1, zs1, true);
			AssertValuesAreEqual(dataType, zs1, s1, true);

			AssertValuesAreEqual(dataType, s2, zs1, false);
			AssertValuesAreEqual(dataType, zs1, s2, false);

			AssertValuesAreEqual(dataType, zs1, zs1, true);
			AssertValuesAreEqual(dataType, zs1, zs2, false);
		}

		void AssertValuesAreEqual(StringRegistryDataType dataType, object a, object b, bool shouldBeEqual)
		{
			const string zStringFormat = "ZString(\"{0}\")";
			const string stringFormat = "\"{0}\"";

			var aName = string.Format(a is string ? stringFormat : zStringFormat, a);
			var bName = string.Format(b is string ? stringFormat : zStringFormat, b);
			var equals = shouldBeEqual ? "==" : "!=";

			Assert(string.Join(" ", aName, equals, bName), dataType.ValuesAreEqual(a, b) == shouldBeEqual);
		}

		public void TestDirectoryBrowseValdation()
		{
			var registryItem = new StringRegistryItem("dummy", null, null, null, RegistryStorageFlags.System);
			registryItem.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
			var dataType = (StringRegistryDataType)registryItem.DataType;
			foreach (char invalidPathChar in Path.GetInvalidPathChars())
			{
				string invalidChar = (invalidPathChar == '\t') ? "TabSpace" : invalidPathChar.ToString();
				AssertExceptionThrown(typeof(RegistryValidationException), string.Format("Illegal character '{0}' is not allowed in a directory path.", invalidChar), delegate
				{
					dataType.Validate(registryItem, "dummy" + invalidPathChar.ToString(), Guid.Empty, Guid.Empty, Guid.Empty);
				});
			}
			AssertExceptionThrown(typeof(RegistryValidationException), delegate
			{
				dataType.Validate(registryItem, "dummy".PadRight(short.MaxValue, 'a'), Guid.Empty, Guid.Empty, Guid.Empty);
			});
			AssertExceptionThrown(typeof(RegistryValidationException), delegate
			{
				dataType.Validate(registryItem, "ftp:\\", Guid.Empty, Guid.Empty, Guid.Empty);
			});
		}

		public void TestHTMLMarkupValdation()
		{
			var registryItem = new MultilingualStringRegistryItem("dummy", null, null, null, RegistryStorageFlags.System);
			registryItem.EditorInfo = new TextRegistryEditorInfo(TextEditorType.HTML);
			var dataType = (StringRegistryDataType)registryItem.DataType;

			var plainText = @"
				Some text
				or
				conditions
			";
			AssertNoExceptionThrown("No exception for plain text", () => dataType.Validate(registryItem, plainText, Guid.Empty, Guid.Empty, Guid.Empty));

			var validMarkup = @"
			<div class=""someclass"">Some text with &</div>
			<img src=""somesource"">
			<a href=""somelink"" target=""_blank"">Some link description</a>
			<span>More text</span>
			<i>Italic text</i>
			<b>Bold text</b>
			<u>Unarticulated  text</u>
			<ul>
				<li>Some unordered option 1</li>
				<li>Some unordered option 2</li>
			</ul>
			<ol>
				<li>Some ordered option 1</li>
				<li>Some ordered option 2</li>
			</ol>
			<br>
			<h1>Heading 1</h1>
			<h2>Heading 2</h2>
			<h3>Heading 3</h3>
			<h4>Heading 4</h4>
			<h5>Heading 5</h5>
			<h6>Heading 6</h6>
			<strong>Strong text</strong>
			<p>Some paragraph</p>
			<em>Some emphasized text</em>
			";
			AssertNoExceptionThrown("No exception for valid markup", () => dataType.Validate(registryItem, validMarkup, Guid.Empty, Guid.Empty, Guid.Empty));

			var allowedAttributes = "class, href, src, target";
			var allowedTags = "i, b, u, ul, li, br, h1, h2, h3, h4, h5, h6, span, div, p, a, img, strong, ol, em";
			var expectedExceptionMessage = $"The HTML markup is invalid. Please only use the following allowed tags and attributes:{System.Environment.NewLine}{System.Environment.NewLine}{allowedTags}{System.Environment.NewLine}{allowedAttributes}";
			var invalidMarkup = $@"{validMarkup}
			<iframe src=""somesource"" />
			";
			AssertExceptionThrown(typeof(RegistryValidationException), expectedExceptionMessage, () =>
			{
				dataType.Validate(registryItem, invalidMarkup, Guid.Empty, Guid.Empty, Guid.Empty);
			});

			invalidMarkup = $@"{validMarkup}
			<br />
			";
			AssertExceptionThrown(typeof(RegistryValidationException), expectedExceptionMessage, () =>
			{
				dataType.Validate(registryItem, invalidMarkup, Guid.Empty, Guid.Empty, Guid.Empty);
			});
		}

		public void TestCharacterCase()
		{
			var stringRegistryDataType = new StringRegistryDataType();
			AssertEquals(CharacterCase.Normal, stringRegistryDataType.CharacterCase);

			stringRegistryDataType = new StringRegistryDataType(CharacterCase.Upper);
			AssertEquals(CharacterCase.Upper, stringRegistryDataType.CharacterCase);

			stringRegistryDataType = new StringRegistryDataType(CharacterCase.Normal);
			AssertEquals(CharacterCase.Normal, stringRegistryDataType.CharacterCase);

			stringRegistryDataType = new StringRegistryDataType(CharacterCase.Lower);
			AssertEquals(CharacterCase.Lower, stringRegistryDataType.CharacterCase);

			stringRegistryDataType.CharacterCase = CharacterCase.Upper;
			AssertEquals(CharacterCase.Upper, stringRegistryDataType.CharacterCase);

			stringRegistryDataType = new StringRegistryDataType(CharacterCase.Upper, 0, 4);
			AssertEquals(CharacterCase.Upper, stringRegistryDataType.CharacterCase);
		}

		[ExpectNoExceptions]
		public void TestValidate_OptinionalWithEmptyString()
		{
			MandatoryDataType.Validate(
					new StringRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.All),
					string.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

			MandatoryDataType.Validate(
					new StringRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.All, RegistryOptions.IsValueOptional),
					string.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		public void TestValidate_MandatoryWithEmptyString()
		{
			AssertExceptionThrown<RegistryValidationException>("Value cannot be empty.", () =>
			{
				MandatoryDataType.Validate(
					new StringRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.All, RegistryOptions.IsValueMandatory),
					string.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			});
		}

		#region implementation

		static public object GetNullStringRepresentation()
		{
			return Encoding.Unicode.GetBytes(StringRegistryDataType.MagicNullString);
		}

		protected override object GetNullRepresentation()
		{
			return GetNullStringRepresentation();
		}

		protected override StringRegistryDataType GetNewDataType()
		{
			return new StringRegistryDataType(0, 10);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(null, Encoding.Unicode.GetBytes("*** NULL ***")),
				new ValidSampleAndBinaryValueInDB("", Encoding.Unicode.GetBytes("")),
				new ValidSampleAndBinaryValueInDB("splaty", Encoding.Unicode.GetBytes("splaty"))
			};
		}

		protected override object[] GetInvalidSamples()
		{
			return new object[] { "1234567890!" };
		}

		protected override void AssertValuesEqualForCheckingDefault(string message, object lhs, object rhs)
		{
			if (lhs is string && rhs == null || rhs is string && lhs == null)
			{
			}
			else
			{
				base.AssertValuesEqual(message, lhs, rhs);
			}
		}

		StringRegistryDataType MandatoryDataType
		{
			get
			{
				return mandatoryDataType ?? (mandatoryDataType = (StringRegistryDataType)new StringRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.IsValueMandatory).DataType);
			}
		}
		StringRegistryDataType mandatoryDataType;

		#endregion

		public void TestValidate_Url()
		{
			var registryItem = new StringRegistryItem("dummy", null, null, null, RegistryStorageFlags.System);
			registryItem.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Url);
			var dataType = (StringRegistryDataType)registryItem.DataType;

			var exceptionMessage = "The URL is invalid, please input a valid URL that must be HTTPS or HTTP.";

			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, "https://", Guid.Empty, Guid.Empty, Guid.Empty));

			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, "http://", Guid.Empty, Guid.Empty, Guid.Empty));

			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, "ftp://", Guid.Empty, Guid.Empty, Guid.Empty));

			AssertNoExceptionThrown(() => dataType.Validate(registryItem, "https://bl.ah", Guid.Empty, Guid.Empty, Guid.Empty));

			AssertNoExceptionThrown(() => dataType.Validate(registryItem, "https://bl.", Guid.Empty, Guid.Empty, Guid.Empty));

			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, "https://.ah", Guid.Empty, Guid.Empty, Guid.Empty));

			AssertNoExceptionThrown(() => dataType.Validate(registryItem, "http://bl.ah", Guid.Empty, Guid.Empty, Guid.Empty));

			AssertNoExceptionThrown(() => dataType.Validate(registryItem, "http://bl.", Guid.Empty, Guid.Empty, Guid.Empty));

			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, "http://.ah", Guid.Empty, Guid.Empty, Guid.Empty));

			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, "ftp://bl.ah", Guid.Empty, Guid.Empty, Guid.Empty));

			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, "ftp://bl.", Guid.Empty, Guid.Empty, Guid.Empty));

			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, "ftp://.ah", Guid.Empty, Guid.Empty, Guid.Empty));

			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, "bl.ah", Guid.Empty, Guid.Empty, Guid.Empty));

			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, "bl.", Guid.Empty, Guid.Empty, Guid.Empty));

			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, ".ah", Guid.Empty, Guid.Empty, Guid.Empty));

			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, "ah", Guid.Empty, Guid.Empty, Guid.Empty));

			AssertExceptionThrown(typeof(RegistryValidationException), exceptionMessage, () => dataType.Validate(registryItem, " ", Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestValidateGuid()
		{
			var registryItem = new StringRegistryItem("dummy", null, null, null, RegistryStorageFlags.System)
			{
				EditorInfo = new TextRegistryEditorInfo(TextEditorType.Guid | TextEditorType.Password)
			};
			var dataType = (StringRegistryDataType)registryItem.DataType;

			AssertExceptionThrown<RegistryValidationException>("Invalid GUID", "The value is invalid, please input a valid GUID value.", () => dataType.Validate(registryItem, "asdf", Guid.Empty, Guid.Empty, Guid.Empty));
			AssertNoExceptionThrown(() => dataType.Validate(registryItem, "EECA8079-A23D-4474-9993-6EF580FEFA77", Guid.Empty, Guid.Empty, Guid.Empty));
		}
	}
}
