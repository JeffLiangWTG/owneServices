namespace Enterprise.Customs.CA.Business
{
	using System.Data;
	using System.Linq;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Common;
	using Enterprise.ZArchitecture.Schema;

	public abstract class AutoCusClassification : BaseCusClassification
	{
		protected AutoCusClassification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : BaseCusClassification.Schema
		{
			public const string CCA_99TariffCode = CusCAClassificationSchema.Constants.CCA_99TariffCode;
			public const string CCA_ValueForDutyCode = CusCAClassificationSchema.Constants.CCA_ValueForDutyCode;
			public const string CCA_AuthorityNumber = CusCAClassificationSchema.Constants.CCA_AuthorityNumber;
			public const string CCA_TRSNumber = CusCAClassificationSchema.Constants.CCA_TRSNumber;
			public const string CCA_TreatmentCode = CusCAClassificationSchema.Constants.CCA_TreatmentCode;
			public const string CCA_OA_Manufacturer = CusCAClassificationSchema.Constants.CCA_OA_Manufacturer;
			public const string CCA_RN_NKOrigin = CusCAClassificationSchema.Constants.CCA_RN_NKOrigin;
			public const string CCA_ProvinceOfOrigin = CusCAClassificationSchema.Constants.CCA_ProvinceOfOrigin;
			public const string CCA_RN_NKSource = CusCAClassificationSchema.Constants.CCA_RN_NKSource;
			public const string CCA_StateOfSource = CusCAClassificationSchema.Constants.CCA_StateOfSource;
			public const string CCA_Model = CusCAClassificationSchema.Constants.CCA_Model;
			public const string CCA_BrandName = CusCAClassificationSchema.Constants.CCA_BrandName;
			public const string CCA_CFIAIndicator = CusCAClassificationSchema.Constants.CCA_CFIAIndicator;
			public const string CCA_CNSCIndicator = CusCAClassificationSchema.Constants.CCA_CNSCIndicator;
			public const string CCA_DFOIndicator = CusCAClassificationSchema.Constants.CCA_DFOIndicator;
			public const string CCA_ECCCIndicator = CusCAClassificationSchema.Constants.CCA_ECCCIndicator;
			public const string CCA_GACIndicator = CusCAClassificationSchema.Constants.CCA_GACIndicator;
			public const string CCA_HCIndicator = CusCAClassificationSchema.Constants.CCA_HCIndicator;
			public const string CCA_NRCanIndicator = CusCAClassificationSchema.Constants.CCA_NRCanIndicator;
			public const string CCA_PHACIndicator = CusCAClassificationSchema.Constants.CCA_PHACIndicator;
			public const string CCA_TCIndicator = CusCAClassificationSchema.Constants.CCA_TCIndicator;
			public const string CCA_GSTStatusCode = CusCAClassificationSchema.Constants.CCA_GSTStatusCode;
			public const string CCA_ETExemption = CusCAClassificationSchema.Constants.CCA_ETExemption;
			public const string CCA_ETRateCode = CusCAClassificationSchema.Constants.CCA_ETRateCode;
			public const string CCA_SIMADumpingNumber = CusCAClassificationSchema.Constants.CCA_SIMADumpingNumber;
			public const string CCA_SIMADumpingDesc = "CCA_SIMADumpingDesc";
		}

		#endregion

		#region CusCAClassification wrapper Properties

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
					}
				}
				return fDetails;
			}
		}
		CusCAClassification fDetails;

		#region CCA_99TariffCode
		[List(nameof(Lookups) + "." + nameof(CusClassificationLookups.Tariffs))]
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
		[ReadOnlyMember(nameof(CCA_ProvinceOfOrigin_ReadOnly))]
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
				return (IsImport && CCA_RN_NKOrigin != Core.Constants.CountryCodes.UnitedStates);
			}
		}

		#endregion

		#region CCA_RN_NKSource
		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.DefaultOrigins))]
		public virtual ZString CCA_RN_NKSource
		{
			get { return Details.CCA_RN_NKSource; }
			set
			{
				var oldValue = CCA_RN_NKSource;
				Details.CCA_RN_NKSource = value;
				if (!IsCopying && oldValue != CCA_RN_NKSource && CCA_StateOfSource_ReadOnly)
				{
					CCA_StateOfSource = ZString.Empty;
				}
				CCA_StateOfSourceInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo CCA_RN_NKSourceInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_RN_NKSource, x => Details.CCA_RN_NKSourceInfo); }
		}
		#endregion

		#region CCA_StateOfSource

		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.StatesOfExport))]
		[ReadOnlyMember(nameof(CCA_StateOfSource_ReadOnly))]
		public virtual ZString CCA_StateOfSource
		{
			get { return Details.CCA_StateOfSource; }
			set { Details.CCA_StateOfSource = value; }
		}

		public virtual ZPropertyInfo CCA_StateOfSourceInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_StateOfSource, x => Details.CCA_StateOfSourceInfo); }
		}

		protected bool CCA_StateOfSource_ReadOnly
		{
			get
			{
				return CCA_RN_NKSource != Enterprise.Core.Constants.CountryCodes.UnitedStates;
			}
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

		#region CCA_GSTStatusCode

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

		#region CCA_SIMADumpingNumber

		public virtual ZString CCA_SIMADumpingNumber
		{
			get { return Details.CCA_SIMADumpingNumber; }
			set { Details.CCA_SIMADumpingNumber = value; }
		}

		public virtual ZPropertyInfo CCA_SIMADumpingNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CCA_SIMADumpingNumber, x => Details.CCA_SIMADumpingNumberInfo); }
		}

		#endregion

		#endregion

		#region New Properties

		#region CCA_SIMADumpingDesc

		public ZString CCA_SIMADumpingDesc
		{
			get { return DutyAndTaxManager.GetSIMADumpingDescription(Factory, CC_TariffNum, CCA_SIMADumpingNumber, ZDateTime.Today); }
		}

		public virtual ZPropertyInfo CCA_SIMADumpingDescInfo
		{
			get { return GetZPropertyInfo(Schema.CCA_SIMADumpingDesc); }
		}

		#endregion

		#endregion

		public override void Delete()
		{
			if (!IsDeleted)
			{
				CusCAClassifications.RemoveAndDeleteAll();
			}
			base.Delete();
		}
	}
}
