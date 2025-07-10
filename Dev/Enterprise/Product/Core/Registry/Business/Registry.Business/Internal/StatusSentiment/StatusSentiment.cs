using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class StatusSentiment : RegistryBusinessObjectTemplate
	{
		public StatusSentiment() : base()
		{
		}

		public StatusSentiment(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		#region Schema

		public static class Schema
		{
			public const string Code = "Code";
			public const string Description = "Description";
			public const string Sentiment = "Sentiment";
			public const string Variant = "Variant";
		}

		#endregion

		#region Bound Properties

		#region Code

		[ResourceStringData("ea6a296f-0e75-4f21-9707-afa1f3c67157", Caption = "Code")]
		[MaxLength(3)]
		public ZString Code
		{
			get => code;
			set
			{
				SetNonPersistentPropertyValue(CodeInfo, ref code, value);
				if (!IsValidationSuspended)
				{
					ValidateStatusCode();
				}
			}
		}
		ZString code;

		public ZPropertyInfo CodeInfo => GetZPropertyInfo(Schema.Code);

		#endregion

		#region Description

		[ResourceStringData("146ccab5-4b64-468a-b6c0-1ca00d840d81", Caption = "Description")]
		public ZString Description
		{
			get => description;
			set
			{
				SetNonPersistentPropertyValue(DescriptionInfo, ref description, value);
				if (!IsValidationSuspended)
				{
					ValidateStatusDescription();
				}
			}
		}
		ZString description;

		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(Schema.Description);

		#endregion

		#region Sentiment

		[List(nameof(SentimentTypeList))]
		[ResourceStringData("ac2dc168-490e-41ba-a126-92571677fa38", Caption = "Sentiment")]
		public ZString Sentiment
		{
			get => sentiment;
			set
			{
				SetNonPersistentPropertyValue(SentimentInfo, ref sentiment, value);
				if (!IsValidationSuspended)
				{
					ValidateSentiment();
				}
			}
		}
		ZString sentiment;

		public ZPropertyInfo SentimentInfo => GetZPropertyInfo(Schema.Sentiment);

		#endregion

		#region Variant

		[List(nameof(VariantTypeList))]
		[ResourceStringData("f2b42ead-3285-44e0-9c6a-9a95d4234150", Caption = "Variant")]
		public ZString Variant
		{
			get => variant;
			set
			{
				SetNonPersistentPropertyValue(VariantInfo, ref variant, value);
				if (!IsValidationSuspended)
				{
					ValidateVariant();
				}
			}
		}
		ZString variant;

		public ZPropertyInfo VariantInfo => GetZPropertyInfo(Schema.Variant);

		#endregion

		#endregion

		#region ParentCollection

		StatusSentimentCollection ParentCollection
		{
			get { return (StatusSentimentCollection)GetParentCollection(this, typeof(StatusSentimentCollection)); }
		}

		#endregion

		#region Validation

		public void ValidateStatusCode()
		{
			CodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CodeInfo);
			if (ParentCollection != null)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(CodeInfo, ParentCollection, true);
			}
		}

		public void ValidateStatusDescription()
		{
			DescriptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DescriptionInfo);
		}

		public void ValidateSentiment()
		{
			SentimentInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(SentimentInfo, SentimentTypeList);
		}

		public void ValidateVariant()
		{
			VariantInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(VariantInfo, VariantTypeList);
		}

		#endregion

		#region SentimentTypes

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public static class SentimentTypes
		{
			public const string Success = "Success";
			public const string Info = "Info";
			public const string Primary = "Primary";
			public const string Warning = "Warning";
			public const string Critical = "Critical";
		}

		#endregion

		#region SentimentTypeList

		public CodeDescriptionPairList SentimentTypeList
		{
			get
			{
				if (sentimentTypeList == null)
				{
					sentimentTypeList = new CodeDescriptionPairList();
					sentimentTypeList.AddPair(SentimentTypes.Success, ResString.GetMultilingualString("effe0888-1c45-404b-b6dd-b25e6143b8f1", "Success"));
					sentimentTypeList.AddPair(SentimentTypes.Info, ResString.GetMultilingualString("b410abc1-f3ba-41d2-8bce-f123eab108c3", "Info"));
					sentimentTypeList.AddPair(SentimentTypes.Primary, ResString.GetMultilingualString("f84f8fa9-3904-4993-8695-dfee9318a362", "Primary"));
					sentimentTypeList.AddPair(SentimentTypes.Warning, ResString.GetMultilingualString("1c2907cd-7bff-4cd6-84a3-3a2e7752810a", "Warning"));
					sentimentTypeList.AddPair(SentimentTypes.Critical, ResString.GetMultilingualString("17f35f9e-8ff0-4c89-afef-bb4aedc9ae41", "Critical"));
				}

				return sentimentTypeList;
			}
		}
		CodeDescriptionPairList sentimentTypeList;

		#endregion

		#region VariantTypes

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public static class VariantTypes
		{
			public const string Fill = "Fill";
			public const string Outline = "Outline";
		}

		#endregion

		#region VariantTypeList
		public CodeDescriptionPairList VariantTypeList
		{
			get
			{
				if (variantTypeList == null)
				{
					variantTypeList = new CodeDescriptionPairList();
					variantTypeList.AddPair(VariantTypes.Fill, ResString.GetMultilingualString("83723ab9-059d-4120-bdf4-97a7cb4ceaa2", "Fill"));
					variantTypeList.AddPair(VariantTypes.Outline, ResString.GetMultilingualString("0a1796e6-7a47-4baf-b8a1-567b6289701a", "Outline"));
				}

				return variantTypeList;
			}
		}
		CodeDescriptionPairList variantTypeList;

		#endregion

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new StatusSentiment(fallbackLevel);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Code = reader.ReadElementString(Schema.Code);
			Description = reader.ReadElementString(Schema.Description);
			Sentiment = reader.ReadElementString(Schema.Sentiment);
			Variant = reader.ReadElementString(Schema.Variant);
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Code, Code);
			writer.WriteElementString(Schema.Description, Description);
			writer.WriteElementString(Schema.Sentiment, Sentiment);
			writer.WriteElementString(Schema.Variant, Variant);
		}

		#endregion
	}
}
