using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngineCore.Registry
{
	#region Label Names

	public static class LabelNames
	{
		public const string DeliveryLabel = "DEL";
		public const string EParcelLabel = "EPL";
		public const string StarTrackLabel = "STK";
	}

	public class LabelNameList : CodeDescriptionPairList
	{
		public LabelNameList()
		{
			AddPair(LabelNames.DeliveryLabel, ResString.GetMultilingualString("257641f4-d21a-4525-b74d-f97aaa76be61", "GS1 Delivery Label"));
			AddPair(LabelNames.EParcelLabel, ResString.GetMultilingualString("7835f17a-2b0a-4fd6-98b7-72807fd0e336", "eParcel Label"));
			AddPair(LabelNames.StarTrackLabel, ResString.GetMultilingualString("03af1cb0-71a8-485d-87c3-62054b3b1a50", "Star Track Label"));
		}
	}

	#endregion

	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class LocalTransportCompanyBranding : ClientAndAgentBrandingBusinessObject
	{
		protected new class Schema : ClientAndAgentBrandingBusinessObject.Schema
		{
			public const string LocalTransportCompanyPK = "LocalTransportCompanyPK";
			public const string LabelName = "LabelName";

			public const int LabelTypeMaxLength = 3;
		}

		public LocalTransportCompanyBranding() { }

		public LocalTransportCompanyBranding(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		#region Overrides

		protected override int CodeMaxLengthDefaultValue
		{
			get { return OrgHeaderSchema.OH_Code.MaxLength; }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new LocalTransportCompanyBranding(fallbackLevel, factory);
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			WriteMoreElements(writer);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ReadMoreElements(reader);
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(Schema.LocalTransportCompanyPK, LocalTransportCompanyPK.ToString());
			writer.WriteElementString(Schema.LabelName, LabelName);
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			LocalTransportCompanyPK = new ZGuid(reader.ReadElementString(Schema.LocalTransportCompanyPK));
			LabelName = reader.ReadElementString(Schema.LabelName);
		}

		#endregion

		protected override CodeDescriptionPairList GetNewCodeList()
		{
			return new CodeDescriptionPairList();
		}

		protected override IRegistryItem BrandingOptionRegistryItem
		{
			get { return null; }
		}

		protected override string BrandingOptionTitle
		{
			get { return string.Empty; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("e7bdea9f-0abb-4c00-8179-d086e3ef01a6", "Local Transport Company Label"); }
		}

		protected override void ValidateImageExists() { }

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateLocalTransportCompany();
			ValidateLabelName();
		}

		#endregion

		#region Properties

		#region LocalTransportCompany

		public ZGuid LocalTransportCompanyPK
		{
			get { return localTransportCompanyPK; }
			set
			{
				if (localTransportCompanyPK != value)
				{
					localTransportCompanyPK = value;

					var localTransportCompany = CurrentFactory.Load<IOrgHeader>(value);
					Code = localTransportCompany != null ? localTransportCompany.OH_Code : ZString.Empty;

					if (!IsValidationSuspended)
					{
						ValidateLocalTransportCompany();
					}
				}

				LocalTransportCompanyPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LocalTransportCompanyPKInfo
		{
			get { return GetZPropertyInfo(Schema.LocalTransportCompanyPK); }
		}

		public void ValidateLocalTransportCompany()
		{
			LocalTransportCompanyPKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(LocalTransportCompanyPKInfo, Res.GetString("{3d495249-5b9c-4924-b0d5-b6bfa3fccd4f", "Local Transport Company"));
			ListValidation.ErrorIfInvalidPK(LocalTransportCompanyPKInfo, LocalTransportCompanyCollection, ResString.GetMultilingualString("461aba78-e347-4065-84a4-f2f6e819b83a", "Enter a valid Local Transport Company."));

			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(LocalTransportCompanyPKInfo, Res.GetString("783294b0-4b6f-4760-abd3-1a997e341b1d", "Local Transport Company already exists in the list."));
			}
		}

		ZGuid localTransportCompanyPK;

		#endregion

		#region LabelName

		[List("LabelNameList")]
		[MaxLength(Schema.LabelTypeMaxLength)]
		public ZString LabelName
		{
			get { return labelName; }
			set
			{
				CheckMaximumLength(LabelNameInfo, value);
				SetNonPersistentPropertyValue(LabelNameInfo, ref labelName, value);

				if (!IsValidationSuspended)
				{
					ValidateLabelName();
				}
			}
		}

		public ZPropertyInfo LabelNameInfo
		{
			get { return GetZPropertyInfo(Schema.LabelName); }
		}

		public void ValidateLabelName()
		{
			LabelNameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(LabelNameInfo, Res.GetString("c6ee71d2-bc04-4134-aa78-a862198233a5", "Label Name"));
			ListValidation.ErrorIfInvalidCode(LabelNameInfo, LabelNameList, ResString.GetMultilingualString("5e74b595-ac3c-4555-a21c-9089744a33f0", "Label Name"));
		}

		ZString labelName;

		#endregion

		#endregion

		#region Lookups

		public BusinessObjectCollection LocalTransportCompanyCollection
		{
			get { return localTransportCompanyCollection ?? (localTransportCompanyCollection = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<ILocalTransportCompanyCollection>(), CurrentFactory)); }
		}

		BusinessObjectCollection localTransportCompanyCollection;

		public LabelNameList LabelNameList
		{
			get { return labelNameList ?? (labelNameList = new LabelNameList()); }
		}

		LabelNameList labelNameList;

		#endregion

	}
}
