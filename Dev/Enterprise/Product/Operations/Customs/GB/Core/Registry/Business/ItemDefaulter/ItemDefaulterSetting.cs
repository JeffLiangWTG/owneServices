using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.GB.Registry.XmlSerializers")]
	public class ItemDefaulterSetting : RegistryBusinessObjectTemplate
	{
		#region schema and constructors
		protected abstract class Schema
		{
			public const string SourceType = "SourceType";
			public const string SourceValue = "SourceValue";
			public const string TargetType = "TargetType";
			public const string TargetOrgAddress = "TargetOrgAddress";
			public const string TargetCode = "TargetCode";
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ItemDefaulterSetting(fallbackLevel, factory);
		}

		public ItemDefaulterSetting()
		{
		}

		public ItemDefaulterSetting(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ItemDefaulterSetting(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}
		#endregion

		#region SourceType
		[MaxLength(5)]
		[List(nameof(SourceTypesList))]
		public ZString SourceType  // DOC or CPC
		{
			get { return fSourceType; }
			set
			{
				SetNonPersistentPropertyValue(SourceTypeInfo, ref fSourceType, value.ToUpper());
				if (!IsValidationSuspended)
				{
					ValidateSourceType();
				}
			}
		}
		ZString fSourceType;

		public ZPropertyInfo SourceTypeInfo
		{
			get { return GetZPropertyInfo(Schema.SourceType); }
		}

		public void ValidateSourceType()
		{
			SourceTypeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode((NoResString)"Enter a valid source type", SourceTypeInfo);
		}

		public CodeDescriptionPairList SourceTypesList
		{
			get { return CurrentFactory.GetCachedValue<SourceTypesList>(); }
		}
		#endregion

		#region SourceValue
		[MaxLength(7)]
		public ZString SourceValue  // 4100000 or C601
		{
			get { return fSourceValue; }
			set
			{
				SetNonPersistentPropertyValue(SourceValueInfo, ref fSourceValue, value.ToUpper());
				if (!IsValidationSuspended)
				{
					ValidateSourceValue();
				}
			}
		}
		ZString fSourceValue;

		public ZPropertyInfo SourceValueInfo
		{
			get { return GetZPropertyInfo(Schema.SourceValue); }
		}

		public void ValidateSourceValue()
		{
			SourceValueInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(SourceValueInfo, "source value");
			if (SourceType == Registry.SourceTypesList.Codes.SupportingDocumentBox44)
			{
				CheckSupportingDocumentCodeIsLegal(SourceValueInfo);
			}
		}

		#endregion

		#region TargetType
		[MaxLength(5)]
		[List(nameof(TargetTypesList))]
		public ZString TargetType // DOC or SPOFF
		{
			get { return fTargetType; }
			set
			{
				SetNonPersistentPropertyValue(TargetTypeInfo, ref fTargetType, value.ToUpper());
				if (!IsValidationSuspended)
				{
					ValidateTargetType();
				}
			}
		}
		ZString fTargetType;

		public ZPropertyInfo TargetTypeInfo
		{
			get { return GetZPropertyInfo(Schema.TargetType); }
		}

		public void ValidateTargetType()
		{
			TargetTypeInfo.ClearAllNotifications();
			switch (TargetType)
			{
				case GB.Registry.TargetTypesList.Codes.SetReferenceFromInvoiceNumber:
					if (SourceType != GB.Registry.SourceTypesList.Codes.SupportingDocumentBox44)
					{
						TargetTypeInfo.AddError("This target type is only allowed with source type SUPPD");
					}
					break;
				default:
					ListValidation.ErrorIfInvalidCode((NoResString)"Enter a valid target type", TargetTypeInfo);
					MandatoryValidation.CheckEntered(TargetTypeInfo, "Enter a valid target type");
					break;
			}
		}

		public CodeDescriptionPairList TargetTypesList
		{
			get { return CurrentFactory.GetCachedValue<TargetTypesList>(); }
		}

		#endregion

		#region TargetOrgAddress
		[List(nameof(OrgAddressesList))]
		public ZGuid TargetOrgAddress // PK to an OrgAddress
		{
			get { return fTargetOrgAddress; }
			set
			{
				SetNonPersistentPropertyValue(TargetOrgAddressInfo, ref fTargetOrgAddress, value);
				if (!IsValidationSuspended)
				{
					ValidateTargetOrgAddress();
				}
			}
		}
		ZGuid fTargetOrgAddress;

		public ZPropertyInfo TargetOrgAddressInfo
		{
			get { return GetZPropertyInfo(Schema.TargetOrgAddress); }
		}

		public void ValidateTargetOrgAddress()
		{
			TargetOrgAddressInfo.ClearAllNotifications();
			if (TargetType != GB.Registry.TargetTypesList.Codes.SupervisingOfficeBox44)
			{
				if (!TargetOrgAddress.IsEmpty)
				{
					TargetOrgAddressInfo.AddError("This field should only be set when the target type is SPOFF");
				}
			}
			else
			{
				MandatoryValidation.CheckEntered(TargetOrgAddressInfo, "supervising office address");
			}
		}

		public OrgAddressCollection OrgAddressesList
		{
			get { return new OrgAddressCollection(CurrentFactory); }
		}

		#endregion

		#region TargetCode
		[MaxLength(14)] // == GBCStandingDataSchema.US_Code.MaxLength
		public ZString TargetCode  // e.g C601
		{
			get { return fTargetCode; }
			set
			{
				SetNonPersistentPropertyValue(TargetCodeInfo, ref fTargetCode, value.ToUpper());
				if (!IsValidationSuspended)
				{
					ValidateTargetCode();
				}
			}
		}
		ZString fTargetCode;

		public ZPropertyInfo TargetCodeInfo
		{
			get { return GetZPropertyInfo(Schema.TargetCode); }
		}

		public void ValidateTargetCode()
		{
			TargetCodeInfo.ClearAllNotifications();
			switch (TargetType)
			{
				case GB.Registry.TargetTypesList.Codes.SupervisingOfficeBox44:
					if (!TargetCode.IsEmpty)
					{
						TargetCodeInfo.AddError("This field should be blank when the target type is SPOFF");
					}

					break;
				case GB.Registry.TargetTypesList.Codes.RegistrationNumberFromExporter:
				case GB.Registry.TargetTypesList.Codes.RegistrationNumberFromImporter:
					if (!new OrgCodeLists().CustomsCodes_List(UK).ContainsCode(TargetCode))
					{
						TargetCodeInfo.AddError("That registration code type does not exist for UK companies");
					}

					break;
				case GB.Registry.TargetTypesList.Codes.SupportingDocumentBox44:
					CheckSupportingDocumentCodeIsLegal(TargetCodeInfo);
					break;
				case GB.Registry.TargetTypesList.Codes.AdditionalInformationStatementBox44:
					CheckAICodeIsLegal(TargetCodeInfo);
					break;
				case GB.Registry.TargetTypesList.Codes.ClientEoriForDucrTickbox:
					// Any value is fine, the value is ignored
					break;
				case GB.Registry.TargetTypesList.Codes.SetReferenceFromInvoiceNumber:
					if (!TargetCode.IsEmpty)
					{
						TargetCodeInfo.AddError("This field should be blank when target type is INVNO");
					}
					break;
				default:
					TargetCodeInfo.AddError("Target type does not allow this value.");
					break;
			}
		}

		void CheckAICodeIsLegal(ZPropertyInfo zPropertyInfo)
		{
			var aiTypes = CurrentFactory.GetAdditionalInformationList(Core.Constants.CountryCodes.UnitedKingdom, UniversalReferenceConstants.RefCusCodeListLevelType.Both, UniversalReferenceConstants.RefCusCodeListDirectionType.Both);
			ListValidation.ErrorIfInvalidCode(zPropertyInfo, aiTypes);
		}

		void CheckSupportingDocumentCodeIsLegal(ZPropertyInfo zPropertyInfo)
		{
			var sdTypes = CurrentFactory.GetSupportingDocumentList(Core.Constants.CountryCodes.UnitedKingdom, UniversalReferenceConstants.RefCusCodeListLevelType.Both, UniversalReferenceConstants.RefCusCodeListDirectionType.Both);
			ListValidation.ErrorIfInvalidCode(zPropertyInfo, sdTypes);
		}

		RefCountry uK;
		RefCountry UK
		{
			get { return uK ?? (uK = RefCountry.LoadFromCountryCode(CurrentFactory, Core.Constants.CountryCodes.UnitedKingdom)); }
		}

		#endregion

		#region XML Reading and Writing
		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.SourceType, SourceType);
			writer.WriteElementString(Schema.SourceValue, SourceValue);
			writer.WriteElementString(Schema.TargetType, TargetType);
			writer.WriteElementString(Schema.TargetOrgAddress, TargetOrgAddress.ToString());
			writer.WriteElementString(Schema.TargetCode, TargetCode);
		}

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			SourceType = reader.ReadElementString(Schema.SourceType);
			SourceValue = reader.ReadElementString(Schema.SourceValue);
			TargetType = reader.ReadElementString(Schema.TargetType);
			TargetOrgAddress = new ZGuid(reader.ReadElementString(Schema.TargetOrgAddress));
			TargetCode = reader.ReadElementString(Schema.TargetCode);
		}
		#endregion
	}
}
