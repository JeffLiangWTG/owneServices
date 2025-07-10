using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Customs
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class EntryChargeTypeSetting : RegistryBusinessObjectTemplate
	{
		protected abstract class Schema
		{
			public const string ChargeType = "ChargeType";
			public const string ChargeTypeDescription = "ChargeTypeDescription";
			public const string AC_ChargeCode = "AC_ChargeCode";
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EntryChargeTypeSetting(fallbackLevel, factory, null);
		}

		public EntryChargeTypeSetting()
			: base()
		{
		}

		public EntryChargeTypeSetting(FallbackLevel fallbackLevel, BusinessObjectFactory factory, EntryChargeTypeSettingCollection parentCollection)
			: base(fallbackLevel, factory)
		{
			SetParentCollection(parentCollection);
		}

		public EntryChargeTypeList ChargeType_List => GetChargeType_ListCore();

		protected virtual EntryChargeTypeList GetChargeType_ListCore()
		{
			var countryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CurrentCountryCode);

			return CurrentFactory.GetCachedValue(countryCode + ".ChargeType_List", () =>
			{
				var result = EntryChargeTypeList.GetList(countryCode);
				result.FilterChargeTypesForRegistry();

				if (result is AU.EntryChargeTypeList chargeTypesOfAU)
				{
					chargeTypesOfAU.RemoveWhere(c => c.Code == AU.EntryChargeTypeList.Codes.AQISServicePaymentAmount);
				}

				return result;
			});
		}

		public ZString CurrentCountryCode
		{
			get
			{
				var currentFallbackLevel = CurrentFallbackLevel ?? ParentCollection?.CurrentFallbackLevel;
				var companyPK = currentFallbackLevel?.CompanyPK(false) ?? ZGuid.Empty;

				return CurrentFactory.GetCachedValue(companyPK + ".CurrentCountryCode"
					, () => companyPK.IsEmpty ? ZString.Empty : CurrentFactory.Load<IGlbCompany>(companyPK)?.GC_RN_NKCountryCode ?? ZString.Empty);
			}
		}

		[BusinessObjectTestExclude]
		public EntryChargeTypeSettingCollection ParentCollection { get; private set; }

		public void SetParentCollection(EntryChargeTypeSettingCollection parentCollection)
		{
			ParentCollection = parentCollection;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateChargeType();
			ValidateAC_ChargeCode();
		}

		#region ChargeType
		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString ChargeType
		{
			get { return chargeType; }
			set
			{
				CheckMaximumLength(ChargeTypeInfo, value);
				SetNonPersistentPropertyValue<ZString>(ChargeTypeInfo, ref chargeType, value);

				if (!IsValidationSuspended)
				{
					ValidateChargeType();
				}
			}
		}
		ZString chargeType;

		public ZPropertyInfo ChargeTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeType); }
		}

		public void ValidateChargeType()
		{
			ChargeTypeInfo.ClearAllNotifications();
			CheckASPChargeType();
			if (ChargeType.IsEmpty)
			{
				ChargeTypeInfo.AddWarning(ErrorMustHaveChargeType);
			}
			else
			{
				EntryChargeType chargeType = ChargeType_List[ChargeType];
				if (chargeType == null)
				{
					ChargeTypeInfo.AddWarning(ErrorMustHaveChargeType);
				}
				else
				{
					if (chargeType.IsRolledUpIntoAnotherChargeCodeAsGSTAtTimeOfInvoicing)
					{
						ChargeTypeInfo.AddWarning(string.Format(ErrorUseANonGSTChargeType, chargeType.ParentCodeForGSTOnARInvoice));
					}
					else if (ParentCollection != null)
					{
						bool codeIsDuplicated = false;
						foreach (EntryChargeTypeSetting chargeTypeSetting in ParentCollection)
						{
							if (chargeTypeSetting != this && chargeTypeSetting.ChargeType == this.ChargeType)
							{
								codeIsDuplicated = true;
								break;
							}
						}
						if (codeIsDuplicated)
						{
							ChargeTypeInfo.AddWarning(ErrorDuplicateChargeType);
						}
					}
				}
			}
		}

		void CheckASPChargeType()
		{
			if (this.ChargeType_List is AU.EntryChargeTypeList)
			{
				var chargeTypeSetting = ParentCollection.Cast<EntryChargeTypeSetting>().FirstOrDefault(x => x.ChargeType == AU.EntryChargeTypeList.Codes.AQISServicePaymentAmount);
				if (chargeTypeSetting != null)
				{
					chargeTypeSetting.ChargeTypeInfo.AddError(ErrorCantHaveASPChargeType);
				}
			}
		}

		public static string ErrorCantHaveASPChargeType
		{
			get { return Res.GetString("172420D9-7564-48CD-BC92-C725BEB92D6B", "Please remove the ASP Charge Type."); }
		}
		public static string ErrorMustHaveChargeType
		{
			get { return Res.GetString("d5b14c5b-b752-4874-9a4e-236c9ddfab3e", "Please enter a valid Charge Type to allocate a Charge Code against."); }
		}
		public static string ErrorDuplicateChargeType
		{
			get { return Res.GetString("b446429d-3a78-4d16-a12d-b0b0ff29fc29", "Charge Type has already been allocated. Cannot allocate the same Charge Type twice."); }
		}
		public static string ErrorUseANonGSTChargeType
		{
			get { return Res.GetString("6ca59622-d2a7-4dd4-86fb-1bd0ed82767d", "This Charge Type is added as GST to Charge Type [{0:G}] at the time of invoicing. If you want to assign a Charge Code for this Charge Type, assign it to [{0:G}] to cover both."); }
		}

		#endregion

		#region ChargeTypeDescription
		[BusinessObjectTestExclude]
		public ZString ChargeTypeDescription
		{
			get { return ChargeType_List.GetDescriptionFromCode(ChargeType); }
		}

		public ZPropertyInfo ChargeTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeTypeDescription); }
		}
		#endregion

		#region AC_ChargeCode
		public ZGuid AC_ChargeCode
		{
			get { return fAC_ChargeCode; }
			set
			{
				SetNonPersistentPropertyValue<ZGuid>(AC_ChargeCodeInfo, ref fAC_ChargeCode, value);
				if (!IsValidationSuspended)
				{
					ValidateAC_ChargeCode();
				}
			}
		}
		ZGuid fAC_ChargeCode;

		public ZPropertyInfo AC_ChargeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.AC_ChargeCode); }
		}

		public void ValidateAC_ChargeCode()
		{
			AC_ChargeCodeInfo.ClearAllNotifications();
			CheckASPChargeType();
			if (AC_ChargeCode.IsEmpty)
			{
				AC_ChargeCodeInfo.AddError(ErrorMustHaveChargeCode);
			}
			else if (CurrentFallbackLevel != null)
			{
				ListValidation.ErrorIfInvalidPK(AC_ChargeCodeInfo, ChargeCode_List);
			}
		}
		public static string ErrorMustHaveChargeCode
		{
			get { return Res.GetString("33cfa858-7da7-43c5-be80-d48650dd7511", "You must have a valid Charge Code for each Charge Type."); }
		}

		public BusinessObjectCollection ChargeCode_List
		{
			get
			{
				object[] parameters = (CurrentFallbackLevel == null) ? new object[] { CurrentFactory } : new object[] { CurrentFactory, new ZQuery(), CurrentFallbackLevel.CompanyPK(false) };
				return (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IAccChargeCodeCollection>(), parameters);
			}
		}
		#endregion

		#region XML Reading and Writing
		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ChargeType, ChargeType);
			writer.WriteElementString(Schema.AC_ChargeCode, AC_ChargeCode.ToString());
		}

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			ChargeType = reader.ReadElementString(Schema.ChargeType);
			AC_ChargeCode = new ZGuid(reader.ReadElementString(Schema.AC_ChargeCode));
		}
		#endregion
	}
}
