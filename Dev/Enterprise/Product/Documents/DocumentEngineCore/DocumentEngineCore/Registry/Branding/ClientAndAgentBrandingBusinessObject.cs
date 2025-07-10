using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public abstract class ClientAndAgentBrandingBusinessObject : RegistryBusinessObject
	{
		#region Schema

		protected new class Schema : RegistryBusinessObject.Schema
		{
			public const string BrandName = "BrandName";
			public const string BrandEmailAddress = "BrandEmailAddress";
			public const string UseGeneric = "UseGeneric";
			public const string ReplaceDomainNames = "ReplaceDomainNames";

			public const int BrandNameMaxLength = 50;
			public const int BrandEmailAddressMaxLength = 128;
		}

		#endregion

		public ClientAndAgentBrandingBusinessObject()
		{
		}

		public ClientAndAgentBrandingBusinessObject(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Overrides

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			using (clone.GetValidationSuspender())
			{
				((ClientAndAgentBrandingBusinessObject)clone).Image = Image;
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateRow();
		}

		#endregion

		#region Property Overrides

		public override ZString Code
		{
			get { return base.Code; }
			set
			{
				base.Code = value;

				if (!IsCopying && CodeList != null)
				{
					Description = CodeList.GetMultilingualDescriptionFromCode(Code);
				}
			}
		}

		[ReadOnly(true)]
		public override MultilingualString Description
		{
			get { return base.Description; }
			set { base.Description = value; }
		}

		protected override int MaxDescriptionLength
		{
			get { return 100; }
		}

		#endregion

		#region Validation Overrides

		protected override void ValidateCodeCore()
		{
			if (CodeList != null && CodeList.Count > 0)
			{
				ListValidation.ErrorIfInvalidCode(CodeInfo, CodeList);
			}
		}

		void CheckBrandingOptionIsEnabled()
		{
			if (CurrentFallbackLevel != null)
			{
				if (BrandingOptionRegistryItem != null)
				{
					RegistryItemProposedValueAccessor retriever =
						new RegistryItemProposedValueAccessor(BrandingOptionRegistryItem, CurrentFallbackLevel);

					if (!(bool)retriever.GetFallBackValue().Value)
					{
						AddRowError(Res.GetString("72a408b5-207d-4e24-903a-03d742ed1a79", "{0} is currently disabled. Please enable {0} first before modifying this Registry Item.", BrandingOptionTitle));
					}
				}
			}
		}

		public void ValidateRow()
		{
			ClearRowNotifications();

			CheckBrandingOptionIsEnabled();

			if (Image == null)
			{
				ValidateImageExists();
			}
			else
			{
				ValidateImageSize(Image);
			}
		}

		protected virtual void ValidateImageExists()
		{
			AddRowError(Res.GetString("880e1657-c03c-4279-b257-15b6a31212e5", "Please select an Image."));
		}

		void ValidateImageSize(Image image)
		{
			using (var stream = new MemoryStream())
			{
				image.Save(stream, ImageFormat.Gif);
				if (stream.Length > ImageMaxSize)
				{
					AddRowError(Res.GetString("5836c0a4-3ded-44d6-a040-b1b29f86a92a", "Image can't be more than 50 Mb"));
				}
			}
		}

#if DEBUG
		protected virtual
#endif
 long ImageMaxSize
		{ get { return 1024 * 1024 * 50; } }

		#endregion

		#region Bound Properties

		#region BrandName

		[MaxLength(Schema.BrandNameMaxLength)]
		public ZString BrandName
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return brandName; }
			set
			{
				CheckMaximumLength(BrandNameInfo, value);
				SetNonPersistentPropertyValue(BrandNameInfo, ref brandName, value);

				if (!IsValidationSuspended)
				{
					ValidateBrandName();
				}
			}
		}

		public ZPropertyInfo BrandNameInfo
		{
			get { return GetZPropertyInfo(Schema.BrandName); }
		}

		public virtual void ValidateBrandName()
		{
		}

		ZString brandName;

		#endregion

		#region BrandEmailAddress

		[MaxLength(Schema.BrandEmailAddressMaxLength)]
		public ZString BrandEmailAddress
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return brandEmailAddress; }
			set
			{
				CheckMaximumLength(BrandEmailAddressInfo, value);
				SetNonPersistentPropertyValue(BrandEmailAddressInfo, ref brandEmailAddress, value);

				if (!IsValidationSuspended)
				{
					ValidateBrandEmailAddress();
				}
			}
		}

		public ZPropertyInfo BrandEmailAddressInfo
		{
			get { return GetZPropertyInfo(Schema.BrandEmailAddress); }
		}

		public virtual void ValidateBrandEmailAddress()
		{
		}

		ZString brandEmailAddress;

		#endregion

		#region UseGeneric

		public ZBool UseGeneric
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return useGeneric; }
			set { SetNonPersistentPropertyValue(UseGenericInfo, ref useGeneric, value); }
		}

		public ZPropertyInfo UseGenericInfo
		{
			get { return GetZPropertyInfo(Schema.UseGeneric); }
		}

		ZBool useGeneric;

		#endregion

		#region ReplaceDomainNames

		public ZBool ReplaceDomainNames
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return replaceDomainNames; }
			set { SetNonPersistentPropertyValue(ReplaceDomainNamesInfo, ref replaceDomainNames, value); }
		}

		public ZPropertyInfo ReplaceDomainNamesInfo
		{
			get { return GetZPropertyInfo(Schema.ReplaceDomainNames); }
		}

		ZBool replaceDomainNames = true;

		#endregion

		#region Image

		public Image Image
		{
			get { return fImage; }
			set
			{
				fImage = value;
				RefreshBinding();

				if (!IsValidationSuspended)
				{
					ValidateRow();
				}
			}
		}

		Image fImage;

		#endregion

		#endregion

		#region Code List

		public CodeDescriptionPairList CodeList
		{
			get
			{
				if (fCodeList == null)
				{
					fCodeList = GetNewCodeList();
				}

				return fCodeList;
			}
		}

		protected abstract CodeDescriptionPairList GetNewCodeList();

		protected void ClearCodeList()
		{
			fCodeList = null;
		}

		CodeDescriptionPairList fCodeList;

		#endregion

		#region Read/Write Xml

		protected override void WriteMoreElements(XmlWriter writer)
		{
			WriteImageElement(writer);
			WriteReplaceDomainNamesElement(writer);
		}

		void WriteImageElement(XmlWriter writer)
		{
			var output = Array.Empty<byte>();
			if (Image != null)
			{
				using (var temp = TempFile.New())
				using (var file = File.Create(temp.Filename))
				{
					Image.Save(file, ImageFormat.Gif); // Save as a gif to reduce memory and disk space used
					file.Flush();
					file.Position = 0;

					output = file.ReadFully();
				}
			}

			writer.WriteElementString("Image", Convert.ToBase64String(output));
		}

		void WriteReplaceDomainNamesElement(XmlWriter writer)
		{
			writer.WriteElementString("ReplaceDomainNames", ReplaceDomainNames ? bool.TrueString : bool.FalseString);
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			ReadImageElement(reader);
			ReadReplaceDomainNamesElement(reader);
		}

		void ReadReplaceDomainNamesElement(XmlReader reader)
		{
			while (string.IsNullOrWhiteSpace(reader.Name))
			{
				reader.Read();
			}

			if (reader.Name == "ReplaceDomainNames")
			{
				bool result;
				if (bool.TryParse(reader.ReadElementContentAsString(), out result))
				{
					ReplaceDomainNames = result;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "String comparison")]
		void ReadImageElement(XmlReader reader)
		{
			if (reader.Name == "Image" || reader.ReadToFollowing("Image"))
			{
				const int bufferSize = 4096;
				var buffer = new byte[bufferSize];
				var output = new MemoryStream(bufferSize);

				int read;
				while ((read = reader.ReadElementContentAsBase64(buffer, 0, bufferSize)) > 0)
				{
					output.Write(buffer, 0, read);
				}

				output.Position = 0;
				Image = output.Length > 0 ? Bitmap.FromStream(output) : null; // read back from gif (or, if created when we still saved as BMPs, from BMP) and let .net decompress however it sees fit
			}
		}

		#endregion

		protected abstract IRegistryItem BrandingOptionRegistryItem { get; }
		protected abstract string BrandingOptionTitle { get; }
		internal IRegistryItem BrandingOptionRegistryItemInternal => BrandingOptionRegistryItem;
		internal string BrandingOptionTitleInternal => BrandingOptionTitle;
		internal void CopyValuesToCloneInternal(RegistryBusinessObjectTemplate clone) => CopyValuesToClone(clone);
	}
}
