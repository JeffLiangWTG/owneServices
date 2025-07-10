namespace Enterprise.Customs.CA.Business
{
	using System.Data;
	using System.Linq;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Core;
	using Enterprise.Customs.Common;
	using Enterprise.ZArchitecture.Schema;

	public abstract class AutoCusClassPartPivot : Customs.Business.BaseCusClassPartPivot
	{
		protected AutoCusClassPartPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : Customs.Business.BaseCusClassPartPivot.Schema
		{
			public const string CCA_RN_NKOrigin = CusCAClassificationSchema.Constants.CCA_RN_NKOrigin;
			public const string CCA_ProvinceOfOrigin = CusCAClassificationSchema.Constants.CCA_ProvinceOfOrigin;
			public const string CCA_99TariffCode = CusCAClassificationSchema.Constants.CCA_99TariffCode;
			public const string CCA_ValueForDutyCode = CusCAClassificationSchema.Constants.CCA_ValueForDutyCode;
			public const string CCA_TreatmentCode = CusCAClassificationSchema.Constants.CCA_TreatmentCode;
			public const string CCA_AuthorityNumber = CusCAClassificationSchema.Constants.CCA_AuthorityNumber;
			public const string CCA_TRSNumber = CusCAClassificationSchema.Constants.CCA_TRSNumber;
			public const string CCA_RequirementID = CusCAClassificationSchema.Constants.CCA_RequirementID;
			public const string CCA_RequirementVersion = CusCAClassificationSchema.Constants.CCA_RequirementVersion;
			public const string CCA_AirsCode = CusCAClassificationSchema.Constants.CCA_AirsCode;
			public const string CCA_DestinationProvince = CusCAClassificationSchema.Constants.CCA_DestinationProvince;
			public const string CCA_EndUse = CusCAClassificationSchema.Constants.CCA_EndUse;
			public const string CCA_MiscID = CusCAClassificationSchema.Constants.CCA_MiscID;
			public const string CCA_RN_NKCFIAOrigin = CusCAClassificationSchema.Constants.CCA_RN_NKCFIAOrigin;
			public const string CCA_CFIAUSStateOfOrigin = CusCAClassificationSchema.Constants.CCA_CFIAUSStateOfOrigin;
			public const string CCA_ImportReasonCode = CusCAClassificationSchema.Constants.CCA_ImportReasonCode;
			public const string CCA_ModelNumber = CusCAClassificationSchema.Constants.CCA_ModelNumber;
			public const string CCA_TypeSize = CusCAClassificationSchema.Constants.CCA_TypeSize;
			public const string CCA_TIIN = CusCAClassificationSchema.Constants.CCA_TIIN;
			public const string CCA_CompliantCompletion = CusCAClassificationSchema.Constants.CCA_CompliantCompletion;
			public const string CCA_CompliantImportDateIndicator = CusCAClassificationSchema.Constants.CCA_CompliantImportDateIndicator;
			public const string CCA_GSTStatusCode = CusCAClassificationSchema.Constants.CCA_GSTStatusCode;
			public const string CCA_SIMADumpingNumber = CusCAClassificationSchema.Constants.CCA_SIMADumpingNumber;
			public const string CCA_ETExemption = CusCAClassificationSchema.Constants.CCA_ETExemption;
			public const string CCA_ETRateCode = CusCAClassificationSchema.Constants.CCA_ETRateCode;
			public const string CCA_OA_Manufacturer = CusCAClassificationSchema.Constants.CCA_OA_Manufacturer;
			public const string CCA_CFIAIndicator = CusCAClassificationSchema.Constants.CCA_CFIAIndicator;
			public const string CCA_CNSCIndicator = CusCAClassificationSchema.Constants.CCA_CNSCIndicator;
			public const string CCA_DFOIndicator = CusCAClassificationSchema.Constants.CCA_DFOIndicator;
			public const string CCA_ECCCIndicator = CusCAClassificationSchema.Constants.CCA_ECCCIndicator;
			public const string CCA_GACIndicator = CusCAClassificationSchema.Constants.CCA_GACIndicator;
			public const string CCA_HCIndicator = CusCAClassificationSchema.Constants.CCA_HCIndicator;
			public const string CCA_NRCanIndicator = CusCAClassificationSchema.Constants.CCA_NRCanIndicator;
			public const string CCA_PHACIndicator = CusCAClassificationSchema.Constants.CCA_PHACIndicator;
			public const string CCA_TCIndicator = CusCAClassificationSchema.Constants.CCA_TCIndicator;
			public const string CCA_Model = CusCAClassificationSchema.Constants.CCA_Model;
			public const string CCA_BrandName = CusCAClassificationSchema.Constants.CCA_BrandName;
			public const string CCA_RN_NKSource = CusCAClassificationSchema.Constants.CCA_RN_NKSource;
			public const string CCA_StateOfSource = CusCAClassificationSchema.Constants.CCA_StateOfSource;
			public const string CCA_AMMVPerUnit = CusCAClassificationSchema.Constants.CCA_AMMVPerUnit;
			public const string CCA_AMMVPerUnitCurrency = CusCAClassificationSchema.Constants.CCA_AMMVPerUnitCurrency;
			public const string CCA_AMMVPercentage = CusCAClassificationSchema.Constants.CCA_AMMVPercentage;
		}

		#endregion

		#region CusCAClassification wrapper Properties

		#region CCA_RN_NKOrigin
		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.Origins))]
		public virtual ZString CCA_RN_NKOrigin
		{
			get { return Details.CCA_RN_NKOrigin; }
			set
			{
				var oldValue = CCA_RN_NKOrigin;
				Details.CCA_RN_NKOrigin = value;
				if (!IsCopying && oldValue != CCA_RN_NKOrigin && CCA_ProvinceOfOrigin_ReadOnly)
				{
					CCA_ProvinceOfOrigin = ZString.Empty;
				}
				CCA_ProvinceOfOriginInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CCA_RN_NKOriginInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_RN_NKOrigin, x => Details.CCA_RN_NKOriginInfo); }
		}
		#endregion

		#region CCA_ProvinceOfOrigin
		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.StatesOfOrigin))]
		public virtual ZString CCA_ProvinceOfOrigin
		{
			get { return Details.CCA_ProvinceOfOrigin; }
			set { Details.CCA_ProvinceOfOrigin = value; }
		}

		public virtual ZPropertyInfo CCA_ProvinceOfOriginInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_ProvinceOfOrigin, x => Details.CCA_ProvinceOfOriginInfo); }
		}

		protected bool CCA_ProvinceOfOrigin_ReadOnly
		{
			get
			{
				return (IsImport && CCA_RN_NKOrigin != Constants.CountryCodes.UnitedStates);
			}
		}

		#endregion

		#region CCA_RN_NKSource
		public virtual ZString CCA_RN_NKSource
		{
			get { return Details.CCA_RN_NKSource; }
			set { Details.CCA_RN_NKSource = value; }
		}

		public virtual ZPropertyInfo CCA_RN_NKSourceInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_RN_NKSource, x => Details.CCA_RN_NKSourceInfo); }
		}
		#endregion

		#region CCA_StateOfSource
		public virtual ZString CCA_StateOfSource
		{
			get { return Details.CCA_StateOfSource; }
			set { Details.CCA_StateOfSource = value; }
		}

		public virtual ZPropertyInfo CCA_StateOfSourceInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_StateOfSource, x => Details.CCA_StateOfSourceInfo); }
		}

		#endregion

		#region CCA_99TariffCode
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.Tariffs))]
		public virtual ZString CCA_99TariffCode
		{
			get { return Details.CCA_99TariffCode; }
			set { Details.CCA_99TariffCode = value; }
		}

		public virtual ZPropertyInfo CCA_99TariffCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_99TariffCode, x => Details.CCA_99TariffCodeInfo); }
		}

		public TariffPropertyInfo CCA_99TariffCodeTariffInfo
		{
			get { return new TariffPropertyInfo(TariffType.Import, ZDate.Today, "99"); }
		}
		#endregion

		#region CCA_AirsCode
		public virtual ZString CCA_AirsCode
		{
			get { return Details.CCA_AirsCode; }
			set { Details.CCA_AirsCode = value; }
		}

		public virtual ZPropertyInfo CCA_AirsCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_AirsCode, x => Details.CCA_AirsCodeInfo); }
		}
		#endregion

		#region CCA_AuthorityNumber
		public virtual ZString CCA_AuthorityNumber
		{
			get { return Details.CCA_AuthorityNumber; }
			set { Details.CCA_AuthorityNumber = value; }
		}

		public virtual ZPropertyInfo CCA_AuthorityNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_AuthorityNumber, x => Details.CCA_AuthorityNumberInfo); }
		}
		#endregion

		#region CCA_CFIAUSStateOfOrigin
		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.CFIAStatesOfOrigin))]
		public virtual ZString CCA_CFIAUSStateOfOrigin
		{
			get { return Details.CCA_CFIAUSStateOfOrigin; }
			set { Details.CCA_CFIAUSStateOfOrigin = value; }
		}

		public virtual ZPropertyInfo CCA_CFIAUSStateOfOriginInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_CFIAUSStateOfOrigin, x => Details.CCA_CFIAUSStateOfOriginInfo); }
		}

		protected bool CCA_CFIAUSStateOfOrigin_ReadOnly
		{
			get { return CCA_RN_NKCFIAOrigin != Constants.CountryCodes.UnitedStates; }
		}
		#endregion

		#region CCA_DestinationProvince
		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.CanadianProvinces))]
		public virtual ZString CCA_DestinationProvince
		{
			get { return Details.CCA_DestinationProvince; }
			set { Details.CCA_DestinationProvince = value; }
		}

		public virtual ZPropertyInfo CCA_DestinationProvinceInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_DestinationProvince, x => Details.CCA_DestinationProvinceInfo); }
		}
		#endregion

		#region CCA_EndUse
		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.CFIAEndUseCodes))]
		public virtual ZString CCA_EndUse
		{
			get { return Details.CCA_EndUse; }
			set { Details.CCA_EndUse = value; }
		}

		public virtual ZPropertyInfo CCA_EndUseInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_EndUse, x => Details.CCA_EndUseInfo); }
		}
		#endregion

		#region CCA_MiscID
		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.CFIAMiscIDCodes))]
		public virtual ZString CCA_MiscID
		{
			get { return Details.CCA_MiscID; }
			set { Details.CCA_MiscID = value; }
		}

		public virtual ZPropertyInfo CCA_MiscIDInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_MiscID, x => Details.CCA_MiscIDInfo); }
		}
		#endregion

		#region CCA_RequirementID
		public virtual ZString CCA_RequirementID
		{
			get { return Details.CCA_RequirementID; }
			set { Details.CCA_RequirementID = value; }
		}

		public virtual ZPropertyInfo CCA_RequirementIDInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_RequirementID, x => Details.CCA_RequirementIDInfo); }
		}
		#endregion

		#region CCA_RequirementVersion
		public virtual ZString CCA_RequirementVersion
		{
			get { return Details.CCA_RequirementVersion; }
			set { Details.CCA_RequirementVersion = value; }
		}

		public virtual ZPropertyInfo CCA_RequirementVersionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_RequirementVersion, x => Details.CCA_RequirementVersionInfo); }
		}
		#endregion

		#region CCA_RN_NKCFIAOrigin
		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.CFIAOrigins))]
		public virtual ZString CCA_RN_NKCFIAOrigin
		{
			get { return Details.CCA_RN_NKCFIAOrigin; }
			set
			{
				var oldValue = CCA_RN_NKCFIAOrigin;
				Details.CCA_RN_NKCFIAOrigin = value;
				if (!IsCopying && oldValue != CCA_RN_NKCFIAOrigin && CCA_RN_NKCFIAOrigin != Constants.CountryCodes.UnitedStates)
				{
					CCA_CFIAUSStateOfOrigin = ZString.Empty;
				}
			}
		}

		public virtual ZPropertyInfo CCA_RN_NKCFIAOriginInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_RN_NKCFIAOrigin, x => Details.CCA_RN_NKCFIAOriginInfo); }
		}
		#endregion

		#region CCA_TRSNumber
		public virtual ZString CCA_TRSNumber
		{
			get { return Details.CCA_TRSNumber; }
			set { Details.CCA_TRSNumber = value; }
		}

		public virtual ZPropertyInfo CCA_TRSNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_TRSNumber, x => Details.CCA_TRSNumberInfo); }
		}
		#endregion

		#region CCA_ValueForDutyCode
		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.ValueForDutyCodes))]
		public virtual ZString CCA_ValueForDutyCode
		{
			get { return Details.CCA_ValueForDutyCode; }
			set { Details.CCA_ValueForDutyCode = value; }
		}

		public virtual ZPropertyInfo CCA_ValueForDutyCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_ValueForDutyCode, x => Details.CCA_ValueForDutyCodeInfo); }
		}
		#endregion

		#region CCA_TreatmentCode
		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.TreatmentCodes))]
		public virtual ZString CCA_TreatmentCode
		{
			get { return Details.CCA_TreatmentCode; }
			set { Details.CCA_TreatmentCode = value; }
		}

		public virtual ZPropertyInfo CCA_TreatmentCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_TreatmentCode, x => Details.CCA_TreatmentCodeInfo); }
		}
		#endregion

		#region CCA_ImportReasonCode
		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.ImportReasonCodes))]
		public virtual ZString CCA_ImportReasonCode
		{
			get { return Details.CCA_ImportReasonCode; }
			set { Details.CCA_ImportReasonCode = value; }
		}

		public virtual ZPropertyInfo CCA_ImportReasonCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_ImportReasonCode, x => Details.CCA_ImportReasonCodeInfo); }
		}
		#endregion

		#region CCA_ModelNumber
		public virtual ZString CCA_ModelNumber
		{
			get { return Details.CCA_ModelNumber; }
			set { Details.CCA_ModelNumber = value; }
		}

		public virtual ZPropertyInfo CCA_ModelNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_ModelNumber, x => Details.CCA_ModelNumberInfo); }
		}
		#endregion

		#region CCA_TypeSize
		public virtual ZString CCA_TypeSize
		{
			get { return Details.CCA_TypeSize; }
			set { Details.CCA_TypeSize = value; }
		}

		public virtual ZPropertyInfo CCA_TypeSizeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_TypeSize, x => Details.CCA_TypeSizeInfo); }
		}
		#endregion

		#region CCA_TIIN
		public virtual ZString CCA_TIIN
		{
			get { return Details.CCA_TIIN; }
			set { Details.CCA_TIIN = value; }
		}

		public virtual ZPropertyInfo CCA_TIINInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_TIIN, x => Details.CCA_TIINInfo); }
		}
		#endregion

		#region CCA_CompliantCompletion
		public virtual ZBool CCA_CompliantCompletion
		{
			get { return Details.CCA_CompliantCompletion; }
			set { Details.CCA_CompliantCompletion = value; }
		}

		public virtual ZPropertyInfo CCA_CompliantCompletionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_CompliantCompletion, x => Details.CCA_CompliantCompletionInfo); }
		}
		#endregion

		#region CCA_CompliantImportDateIndicator
		public virtual ZBool CCA_CompliantImportDateIndicator
		{
			get { return Details.CCA_CompliantImportDateIndicator; }
			set { Details.CCA_CompliantImportDateIndicator = value; }
		}

		public virtual ZPropertyInfo CCA_CompliantImportDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_CompliantImportDateIndicator, x => Details.CCA_CompliantImportDateIndicatorInfo); }
		}
		#endregion

		#region CCA_GSTExemption
		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.GSTStatusCodes))]
		public virtual ZString CCA_GSTStatusCode
		{
			get { return Details.CCA_GSTStatusCode; }
			set { Details.CCA_GSTStatusCode = value; }
		}

		public virtual ZPropertyInfo CCA_GSTStatusCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_GSTStatusCode, x => Details.CCA_GSTStatusCodeInfo); }
		}

		#endregion

		#region CCA_SIMACode
		public virtual ZString CCA_SIMADumpingNumber
		{
			get { return Details.CCA_SIMADumpingNumber; }
			set { Details.CCA_SIMADumpingNumber = value; }
		}

		public virtual ZPropertyInfo CCA_SIMADumpingNumInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_SIMADumpingNumber, x => Details.CCA_SIMADumpingNumberInfo); }
		}

		#endregion

		#region CCA_ETExemption
		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.ETExemptionCodes))]
		public virtual ZString CCA_ETExemption
		{
			get { return Details.CCA_ETExemption; }
			set { Details.CCA_ETExemption = value; }
		}

		public virtual ZPropertyInfo CCA_ETExemptionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_ETExemption, x => Details.CCA_ETExemptionInfo); }
		}

		#endregion

		#region CCA_ETRateCode
		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.ExciseTaxRateCodes))]
		public virtual ZString CCA_ETRateCode
		{
			get { return Details.CCA_ETRateCode; }
			set { Details.CCA_ETRateCode = value; }
		}

		public virtual ZPropertyInfo CCA_ETRateCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_ETRateCode, x => Details.CCA_ETRateCodeInfo); }
		}

		#endregion

		#region CCA_OA_Manufacturer

		public virtual ZGuid CCA_OA_Manufacturer
		{
			get { return Details.CCA_OA_Manufacturer; }
			set { Details.CCA_OA_Manufacturer = value; }
		}

		public virtual ZPropertyInfo CCA_OA_ManufacturerInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_OA_Manufacturer, x => Details.CCA_OA_ManufacturerInfo); }
		}

		#endregion

		#region CCA_CFIAIndicator

		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.CCA_PGAIndicatorList))]
		public virtual ZString CCA_CFIAIndicator
		{
			get { return Details.CCA_CFIAIndicator; }
			set { Details.CCA_CFIAIndicator = value; }
		}

		public virtual ZPropertyInfo CCA_CFIAIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_CFIAIndicator, x => Details.CCA_CFIAIndicatorInfo); }
		}

		#endregion

		#region CCA_CNSCIndicator

		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.CCA_PGAIndicatorList))]
		public virtual ZString CCA_CNSCIndicator
		{
			get { return Details.CCA_CNSCIndicator; }
			set { Details.CCA_CNSCIndicator = value; }
		}

		public virtual ZPropertyInfo CCA_CNSCIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_CNSCIndicator, x => Details.CCA_CNSCIndicatorInfo); }
		}

		#endregion

		#region CCA_DFOIndicator

		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.CCA_PGAIndicatorList))]
		public virtual ZString CCA_DFOIndicator
		{
			get { return Details.CCA_DFOIndicator; }
			set { Details.CCA_DFOIndicator = value; }
		}

		public virtual ZPropertyInfo CCA_DFOIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_DFOIndicator, x => Details.CCA_DFOIndicatorInfo); }
		}

		#endregion

		#region CCA_ECCCIndicator

		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.CCA_PGAIndicatorList))]
		public virtual ZString CCA_ECCCIndicator
		{
			get { return Details.CCA_ECCCIndicator; }
			set { Details.CCA_ECCCIndicator = value; }
		}

		public virtual ZPropertyInfo CCA_ECCCIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_ECCCIndicator, x => Details.CCA_ECCCIndicatorInfo); }
		}

		#endregion

		#region CCA_GACIndicator

		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.CCA_PGAIndicatorList))]
		public virtual ZString CCA_GACIndicator
		{
			get { return Details.CCA_GACIndicator; }
			set { Details.CCA_GACIndicator = value; }
		}

		public virtual ZPropertyInfo CCA_GACIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_GACIndicator, x => Details.CCA_GACIndicatorInfo); }
		}

		#endregion

		#region CCA_HCIndicator

		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.CCA_PGAIndicatorList))]
		public virtual ZString CCA_HCIndicator
		{
			get { return Details.CCA_HCIndicator; }
			set { Details.CCA_HCIndicator = value; }
		}

		public virtual ZPropertyInfo CCA_HCIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_HCIndicator, x => Details.CCA_HCIndicatorInfo); }
		}

		#endregion

		#region CCA_NRCanIndicator

		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.CCA_PGAIndicatorList))]
		public virtual ZString CCA_NRCanIndicator
		{
			get { return Details.CCA_NRCanIndicator; }
			set { Details.CCA_NRCanIndicator = value; }
		}

		public virtual ZPropertyInfo CCA_NRCanIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_NRCanIndicator, x => Details.CCA_NRCanIndicatorInfo); }
		}

		#endregion

		#region CCA_PHACIndicator

		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.CCA_PGAIndicatorList))]
		public virtual ZString CCA_PHACIndicator
		{
			get { return Details.CCA_PHACIndicator; }
			set { Details.CCA_PHACIndicator = value; }
		}

		public virtual ZPropertyInfo CCA_PHACIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_PHACIndicator, x => Details.CCA_PHACIndicatorInfo); }
		}

		#endregion

		#region CCA_TCIndicator

		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.CCA_PGAIndicatorList))]
		public virtual ZString CCA_TCIndicator
		{
			get { return Details.CCA_TCIndicator; }
			set { Details.CCA_TCIndicator = value; }
		}

		public virtual ZPropertyInfo CCA_TCIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_TCIndicator, x => Details.CCA_TCIndicatorInfo); }
		}

		#endregion

		#region CCA_Model

		public virtual ZString CCA_Model
		{
			get { return Details.CCA_Model; }
			set { Details.CCA_Model = value; }
		}

		public virtual ZPropertyInfo CCA_ModelInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_Model, x => Details.CCA_ModelInfo); }
		}

		#endregion

		#region CCA_BrandName

		public virtual ZString CCA_BrandName
		{
			get { return Details.CCA_BrandName; }
			set { Details.CCA_BrandName = value; }
		}

		public virtual ZPropertyInfo CCA_BrandNameInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_BrandName, x => Details.CCA_BrandNameInfo); }
		}

		#endregion

		#region CCA_AMMVPerUnit
		[DecimalPlaces(4)]
		public virtual ZDecimal CCA_AMMVPerUnit
		{
			get { return Details.CCA_AMMVPerUnit; }
			set { Details.CCA_AMMVPerUnit = value; }
		}

		public virtual ZPropertyInfo CCA_AMMVPerUnitInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_AMMVPerUnit, x => Details.CCA_AMMVPerUnitInfo); }
		}

		#endregion

		#region CCA_AMMVPerUnitCurrency

		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.Currencies))]
		public virtual ZString CCA_AMMVPerUnitCurrency
		{
			get { return Details.CCA_AMMVPerUnitCurrency; }
			set { Details.CCA_AMMVPerUnitCurrency = value; }
		}

		public virtual ZPropertyInfo CCA_AMMVPerUnitCurrencyInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_AMMVPerUnitCurrency, x => Details.CCA_AMMVPerUnitCurrencyInfo); }
		}

		#endregion

		#region CCA_AMMVPercentage
		public virtual ZDecimal CCA_AMMVPercentage
		{
			get { return Details.CCA_AMMVPercentage; }
			set { Details.CCA_AMMVPercentage = value; }
		}

		public virtual ZPropertyInfo CCA_AMMVPercentageInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_AMMVPercentage, x => Details.CCA_AMMVPercentageInfo); }
		}
		#endregion

		#endregion AddInfo Properties

		#region implementation
		public bool IsHTS
		{
			get { return CI_ChildType == ClassificationTypeList.Codes.HTE || CI_ChildType == ClassificationTypeList.Codes.HTI; }
		}

		public bool IsImport
		{
			get { return CI_ChildType == ClassificationTypeList.Codes.HTI; }
		}

		public bool IsExport
		{
			get { return CI_ChildType != ClassificationTypeList.Codes.HTI; }
		}
		#endregion

		#region ClassificationDetails

		public CusCAClassificationValidation CAClassificationValidation => Details.Validation;

		public CusCAClassificationLookups CAClassificationLookups => Details.Lookups;

		[ChildEditable(true)]
		CusCAClassificationCollection CusCAClassifications
		{
			get
			{
				if (cusCAClassifications == null)
				{
					cusCAClassifications = new CusCAClassificationCollection(this);
					cusCAClassifications.Load();
					RegisterEditableChildObject(cusCAClassifications);
				}
				return cusCAClassifications;
			}
		}
		CusCAClassificationCollection cusCAClassifications;

		public CusCAClassification Details
		{
			get
			{
				if (fDetails == null || fDetails.IsDeleted)
				{
					if (IsDeleted)
					{
						fDetails = Factory.GetNull<CusCAClassification>();
					}
					else
					{
						fDetails = CusCAClassifications.Cast<CusCAClassification>().OrderBy(x => x.PK).FirstOrDefault() ?? CusCAClassifications.AddNew();
						fDetails.HasChangesChanged -= ClearAuditOnChanged;
						fDetails.HasChangesChanged += ClearAuditOnChanged;
					}
				}
				return fDetails;
			}
		}
		CusCAClassification fDetails;

		public override void Delete()
		{
			if (!IsDeleted)
			{
				CusCAClassifications.RemoveAndDeleteAll();
			}
			base.Delete();
		}

		#endregion
	}
}
