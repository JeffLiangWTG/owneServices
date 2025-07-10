using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public partial class CusClassification : AutoCusClassification, ICusAddInfoTypeSupporter, IHasPGARequirements, IPGARequirementSupporter
	{
		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public CusClassification Load(string lookupCode, string classificationType)
			{
				var classFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, lookupCode);
				classFilter.AddToFilter(CusClassificationSchema.CC_ClassificationType, classificationType);
				classFilter.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, Core.Constants.CountryCodes.Canada);
				return (CusClassification)Factory.LoadTop1(GetTypeOfBusinessObjectToLoad(), classFilter);
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusClassification);
			}
		}

		#endregion

		public CusClassification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			pgaHeaders = new CusAddInfoChildrenCollection(this);
			pgaHeaders.Register<HCPGAHeader>();
			pgaHeaders.Register<PHACPGAHeader>();
			pgaHeaders.Register<NRCanPGAHeader>();
			pgaHeaders.Register<DFOPGAHeader>();
			pgaHeaders.Register<GACPGAHeader>();
			pgaHeaders.Register<CFIAPGAHeader>();
			pgaHeaders.Register<CNSCPGAHeader>();
			pgaHeaders.Register<ECCCPGAHeader>();
			pgaHeaders.Register<TCPGAHeader>();
		}

		#region Duty and Tax RatesForCurrentCountry

		protected override ZString GetDutyRateForCurrentCountry()
		{
			var result = ZString.Empty;
			if (IsHTS)
			{
				using (var manager = GetClonedDutyAndTaxManager())
				{
					result = manager.DeriveDutyRateShortDescription();
				}
				ClearClonedBOs();
			}
			return result;
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		public override void OnSaving()
		{
			ClearClonedBOs();
			base.OnSaving();
		}

		DutyAndTaxManager GetClonedDutyAndTaxManager()
		{
			allowClone = true;
			if (classification == null || classification.IsDeleted)
			{
				classification = this.Clone();
			}
			allowClone = false;
			return new DutyAndTaxManager(new DutyAndTaxCollection(classification), classification);
		}
		CusClassification classification;

		void ClearClonedBOs()
		{
			if (classification != null && !classification.IsDeleted)
			{
				classification.Delete();
			}
		}

		protected override bool SupportsCloneCore()
		{
			return allowClone;
		}

		ZBool allowClone;

		#endregion

		#region CC_TariffNum

		[List(nameof(Lookups) + "." + nameof(CusClassificationLookups.Tariffs))]
		public override ZString CC_TariffNum
		{
			get { return base.CC_TariffNum; }
			set
			{
				var oldValue = CC_TariffNum;
				base.CC_TariffNum = value;
				if (oldValue != value && !IsCopying)
				{
					RefreshSIMAMeasuresCollection();
					PGARequirements.SetDefaultValueForIndicatorWhenTariffChanged();
				}
				var newTariffDescription = TariffDescriptionHelper.GetTariffDescription(Factory, CC_TariffNum, !IsHTS);
				if (CC_Description.IsEmpty && !newTariffDescription.IsEmpty)
				{
					CC_Description = newTariffDescription.Left(CC_DescriptionInfo.MaxLength);
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusClassificationLookups.Tariffs))]
		public override ZString CC_FormattedTariffNum
		{
			get { return base.CC_FormattedTariffNum; }
			set { base.CC_FormattedTariffNum = value; }
		}

		public TariffPropertyInfo CC_FormattedTariffNumTariffInfo
		{
			get { return new TariffPropertyInfo { TariffType = IsHTS ? TariffType.Import : TariffType.Export }; }
		}

		#endregion

		#region CA_AuthorityNumber

		[List(nameof(Lookups) + "." + nameof(CusClassificationLookups.AuthorityNumberList))]
		[MaxLength(AutoCAAddInfo.Schema.CA_AuthorityNumberMaxLength)]
		[ResourceStringData("CusClassification|CA_AuthorityNumber", Caption = "Special Authority/Permit", ShortCaption = "Auth/Pmt", MediumCaption = "Special Auth/Pmt",
			FullDescription = "A permit number or Order in Council (OIC) authorization number to import goods under special conditions.")]
		public override ZString CCA_AuthorityNumber
		{
			get { return base.CCA_AuthorityNumber; }
			set { base.CCA_AuthorityNumber = value; }
		}

		#endregion

		#region CCA_OA_Manufacturer

		[RelatedBusinessObject("ManufacturerAddress")]
		[List(nameof(CCA_OA_Manufacturer_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid CCA_OA_Manufacturer
		{
			get { return base.CCA_OA_Manufacturer; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CCA_OA_Manufacturer))
				{
					var oldValue = CCA_OA_Manufacturer;
					base.CCA_OA_Manufacturer = value;
					if (oldValue != CCA_OA_Manufacturer && !IsCopying)
					{
						fCCA_OA_Manufacturer_ZAddress = null;
						PGARequirements.RefreshBindingForDeclaredPGAHeaders();
					}
				}
			}
		}

		public OrgAddress ManufacturerAddress
		{
			get { return Factory.Load<OrgAddress>(CCA_OA_Manufacturer); }
		}

		public ZAddress CCA_OA_Manufacturer_ZAddress
		{
			get
			{
				if (fCCA_OA_Manufacturer_ZAddress == null)
				{
					fCCA_OA_Manufacturer_ZAddress = new ZAddress(CCA_OA_ManufacturerInfo);
					fCCA_OA_Manufacturer_ZAddress.IsOrgVisible = true;
					fCCA_OA_Manufacturer_ZAddress.DefaultAddressType = AddressType.NoDefault;
					fCCA_OA_Manufacturer_ZAddress.GetDefaultAddress = header => GetMainAddressPK(header as OrgHeader);
					RegisterEditableChildObject(fCCA_OA_Manufacturer_ZAddress);
				}
				return fCCA_OA_Manufacturer_ZAddress;
			}
		}
		ZAddress fCCA_OA_Manufacturer_ZAddress;

		ZGuid GetMainAddressPK(OrgHeader header)
		{
			return header != null ? header.MainAddress.PK : ZGuid.Empty;
		}

		#endregion

		#region CCA_RN_NKOrigin

		public override ZString CCA_RN_NKOrigin
		{
			get => base.CCA_RN_NKOrigin;
			set
			{
				var oldValue = CCA_RN_NKOrigin;
				base.CCA_RN_NKOrigin = value;
				if (oldValue != CCA_RN_NKOrigin && !IsCopying)
				{
					RefreshSIMAMeasuresCollection();
					PGARequirements.RefreshBindingForDeclaredPGAHeaders();
				}
			}
		}

		#endregion

		#region CCA_ProvinceOfOrigin

		public override ZString CCA_ProvinceOfOrigin
		{
			get => base.CCA_ProvinceOfOrigin;
			set
			{
				var oldValue = CCA_ProvinceOfOrigin;
				base.CCA_ProvinceOfOrigin = value;
				if (oldValue != CCA_ProvinceOfOrigin && !IsCopying)
				{
					PGARequirements.RefreshBindingForDeclaredPGAHeaders();
				}
			}
		}

		#endregion

		#region CCA_RN_NKSource
		public override ZString CCA_RN_NKSource
		{
			get { return base.CCA_RN_NKSource; }
			set
			{
				var oldValue = base.CCA_RN_NKSource;
				base.CCA_RN_NKSource = value;
				if (oldValue != CCA_RN_NKSource && !IsCopying)
				{
					PGARequirements.RefreshBindingForDeclaredPGAHeaders();
				}
			}
		}

		#endregion

		#region CCA_StateOfSource

		public override ZString CCA_StateOfSource
		{
			get { return base.CCA_StateOfSource; }
			set
			{
				var oldValue = base.CCA_StateOfSource;
				base.CCA_StateOfSource = value;
				if (oldValue != CCA_StateOfSource && !IsCopying)
				{
					PGARequirements.RefreshBindingForDeclaredPGAHeaders();
				}
			}
		}

		#endregion

		#region PGA Indicators

		public override ZString CCA_HCIndicator
		{
			get { return base.CCA_HCIndicator; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CCA_HCIndicator))
				{
					base.CCA_HCIndicator = value;
				}
			}
		}

		public override ZString CCA_PHACIndicator
		{
			get { return base.CCA_PHACIndicator; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CCA_PHACIndicator))
				{
					base.CCA_PHACIndicator = value;
				}
			}
		}

		public override ZString CCA_NRCanIndicator
		{
			get { return base.CCA_NRCanIndicator; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CCA_NRCanIndicator))
				{
					base.CCA_NRCanIndicator = value;
				}
			}
		}

		public override ZString CCA_DFOIndicator
		{
			get { return base.CCA_DFOIndicator; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CCA_DFOIndicator))
				{
					base.CCA_DFOIndicator = value;
				}
			}
		}

		public override ZString CCA_GACIndicator
		{
			get { return base.CCA_GACIndicator; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CCA_GACIndicator))
				{
					base.CCA_GACIndicator = value;
				}
			}
		}

		public override ZString CCA_CFIAIndicator
		{
			get { return base.CCA_CFIAIndicator; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CCA_CFIAIndicator))
				{
					base.CCA_CFIAIndicator = value;
				}
			}
		}

		public override ZString CCA_CNSCIndicator
		{
			get { return base.CCA_CNSCIndicator; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CCA_CNSCIndicator))
				{
					base.CCA_CNSCIndicator = value;
				}
			}
		}

		public override ZString CCA_ECCCIndicator
		{
			get { return base.CCA_ECCCIndicator; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CCA_ECCCIndicator))
				{
					base.CCA_ECCCIndicator = value;
				}
			}
		}

		public override ZString CCA_TCIndicator
		{
			get { return base.CCA_TCIndicator; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CCA_TCIndicator))
				{
					base.CCA_TCIndicator = value;
				}
			}
		}

		#endregion

		public bool IsHTS
		{
			get { return CC_ClassificationType == ClassificationType.IMP; }
		}

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.CADutyAndTax, typeof(DutyAndTax));
			result.Add(CusAddInfoTypeAttribute.Codes.CAHCPGAHeader, typeof(HCPGAHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.CAPHACPGAHeader, typeof(PHACPGAHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.CANRCanPGAHeader, typeof(NRCanPGAHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.CADFOPGAHeader, typeof(DFOPGAHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.CAGACPGAHeader, typeof(GACPGAHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.CACFIAPGAHeader, typeof(CFIAPGAHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.CACNSCPGAHeader, typeof(CNSCPGAHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.CAECCCPGAHeader, typeof(ECCCPGAHeader));
			result.Add(CusAddInfoTypeAttribute.Codes.CATCPGAHeader, typeof(TCPGAHeader));
			return result;
		}

		#endregion

		#region IHasPGARequirements

		[ChildEditable]
		public PGARequirementCollection PGARequirements
		{
			get
			{
				if (pgaAgencyRequirements == null)
				{
					pgaAgencyRequirements = new PGARequirementCollection(new PGARequirementProvider(this));
					pgaAgencyRequirements.Populate();
					RegisterEditableChildObject(pgaAgencyRequirements);
				}
				return pgaAgencyRequirements;
			}
		}

		PGARequirementCollection pgaAgencyRequirements;

		#region OA_Manufacturer

		ZGuid IHasPGARequirements.OA_Manufacturer
		{
			get => CCA_OA_Manufacturer;
			set => CCA_OA_Manufacturer = value;
		}

		ZPropertyInfo IHasPGARequirements.OA_ManufacturerInfo => CCA_OA_ManufacturerInfo;

		ZAddress IHasPGARequirements.OA_ManufacturerAddress_ZAddress => CCA_OA_Manufacturer_ZAddress;

		OrgHeaderCollection IHasPGARequirements.ManufacturersLookup => Lookups.Manufacturers;

		#endregion

		#region RN_NKCountryOfOrigin

		ZString IHasPGARequirements.RN_NKCountryOfOrigin
		{
			get => CCA_RN_NKOrigin;
			set => CCA_RN_NKOrigin = value;
		}

		ZPropertyInfo IHasPGARequirements.RN_NKCountryOfOriginInfo => CCA_RN_NKOriginInfo;

		RefCountryCollection IHasPGARequirements.CountryOfOriginsLookup => CAClassificationLookups.Origins;

		#endregion

		#region RW_NKOriginState

		ZString IHasPGARequirements.RW_NKOriginState
		{
			get => CCA_ProvinceOfOrigin;
			set => CCA_ProvinceOfOrigin = value;
		}

		ZPropertyInfo IHasPGARequirements.RW_NKOriginStateInfo => CCA_ProvinceOfOriginInfo;

		CodeDescriptionPairList IHasPGARequirements.StateCodeListLookup => CAClassificationLookups.StatesOfOrigin;

		#endregion

		#region RN_NKCountryOfSource

		ZString IHasPGARequirements.RN_NKCountryOfSource
		{
			get => CCA_RN_NKSource;
			set => CCA_RN_NKSource = value;
		}

		ZPropertyInfo IHasPGARequirements.RN_NKCountryOfSourceInfo => CCA_RN_NKSourceInfo;

		RefCountryCollection IHasPGARequirements.CountryOfSourceLookup => CAClassificationLookups.CFIAOrigins;

		#endregion

		#region RW_NKCountryOfSourceState

		ZString IHasPGARequirements.RW_NKCountryOfSourceState
		{
			get => CCA_StateOfSource;
			set => CCA_StateOfSource = value;
		}

		ZPropertyInfo IHasPGARequirements.RW_NKCountryOfSourceStateInfo => CCA_StateOfSourceInfo;

		CodeDescriptionPairList IHasPGARequirements.CountryOfSourceStateLookup => CAClassificationLookups.CFIAStatesOfOrigin;

		#endregion

		#region JI_BrandName

		ZString IHasPGARequirements.JI_BrandName
		{
			get => CCA_BrandName;
			set => CCA_BrandName = value;
		}

		ZPropertyInfo IHasPGARequirements.JI_BrandNameInfo => CCA_BrandNameInfo;

		#endregion

		#region JI_Model

		ZString IHasPGARequirements.JI_Model
		{
			get => CCA_Model;
			set => CCA_Model = value;
		}

		ZPropertyInfo IHasPGARequirements.JI_ModelInfo => CCA_ModelInfo;

		#endregion

		ZString IHasPGARequirements.Tariff => CC_TariffNum;
		ZPropertyInfo IHasPGARequirements.TariffInfo => CC_TariffNumInfo;

		#endregion

		#region IPGARequirementSupporter

		ZBool IPGARequirementSupporter.IsDeleted => IsDeleted;
		ZString IPGARequirementSupporter.Tariff => CC_TariffNum;
		ZDateTime IPGARequirementSupporter.EffectiveDate => ZDate.Today;
		ZBool IPGARequirementSupporter.IsPGARequirementEffective => IsHTS;
		ZPropertyInfo IPGARequirementSupporter.HCIndInfo => CCA_HCIndicatorInfo;
		ZPropertyInfo IPGARequirementSupporter.PHACIndInfo => CCA_PHACIndicatorInfo;
		ZPropertyInfo IPGARequirementSupporter.NRCanIndInfo => CCA_NRCanIndicatorInfo;
		ZPropertyInfo IPGARequirementSupporter.DFOIndInfo => CCA_DFOIndicatorInfo;
		ZPropertyInfo IPGARequirementSupporter.GACIndInfo => CCA_GACIndicatorInfo;
		ZPropertyInfo IPGARequirementSupporter.ECCCIndInfo => CCA_ECCCIndicatorInfo;
		ZPropertyInfo IPGARequirementSupporter.CNSCIndInfo => CCA_CNSCIndicatorInfo;
		ZPropertyInfo IPGARequirementSupporter.TCIndInfo => CCA_TCIndicatorInfo;
		ZPropertyInfo IPGARequirementSupporter.CFIAIndInfo => CCA_CFIAIndicatorInfo;
		IPGAProgramRequirementProvider IPGARequirementSupporter.CFIARequirementProvider => CFIAPGAHeader;
		IPGAProgramRequirementProvider IPGARequirementSupporter.CNSCRequirementProvider => CNSCPGAHeader;
		IPGAProgramRequirementProvider IPGARequirementSupporter.DFORequirementProvider => DFOPGAHeader;
		IPGAProgramRequirementProvider IPGARequirementSupporter.ECCCRequirementProvider => ECCCPGAHeader;
		IPGAProgramRequirementProvider IPGARequirementSupporter.GACRequirementProvider => GACPGAHeader;
		IPGAProgramRequirementProvider IPGARequirementSupporter.HCRequirementProvider => HCPGAHeader;
		IPGAProgramRequirementProvider IPGARequirementSupporter.NRCanRequirementProvider => NRCanPGAHeader;
		IPGAProgramRequirementProvider IPGARequirementSupporter.PHACRequirementProvider => PHACPGAHeader;
		IPGAProgramRequirementProvider IPGARequirementSupporter.TCRequirementProvider => TCPGAHeader;
		SetterSuspender IPGARequirementSupporter.SetterSuspender => SetterSuspender;

		#endregion

		#region PGA

		public HCPGAHeader HCPGAHeader => YesNoList.IsYesOrNo(CCA_HCIndicator) ? pgaHeaders.Load<HCPGAHeader>() : Factory.GetNull<HCPGAHeader>();
		public PHACPGAHeader PHACPGAHeader => YesNoList.IsYesOrNo(CCA_PHACIndicator) ? pgaHeaders.Load<PHACPGAHeader>() : Factory.GetNull<PHACPGAHeader>();
		public NRCanPGAHeader NRCanPGAHeader => YesNoList.IsYesOrNo(CCA_NRCanIndicator) ? pgaHeaders.Load<NRCanPGAHeader>() : Factory.GetNull<NRCanPGAHeader>();
		public DFOPGAHeader DFOPGAHeader => YesNoList.IsYesOrNo(CCA_DFOIndicator) ? pgaHeaders.Load<DFOPGAHeader>() : Factory.GetNull<DFOPGAHeader>();
		public GACPGAHeader GACPGAHeader => YesNoList.IsYesOrNo(CCA_GACIndicator) ? pgaHeaders.Load<GACPGAHeader>() : Factory.GetNull<GACPGAHeader>();
		public CFIAPGAHeader CFIAPGAHeader => YesNoList.IsYesOrNo(CCA_CFIAIndicator) ? pgaHeaders.Load<CFIAPGAHeader>() : Factory.GetNull<CFIAPGAHeader>();
		public CNSCPGAHeader CNSCPGAHeader => YesNoList.IsYesOrNo(CCA_CNSCIndicator) ? pgaHeaders.Load<CNSCPGAHeader>() : Factory.GetNull<CNSCPGAHeader>();
		public ECCCPGAHeader ECCCPGAHeader => YesNoList.IsYesOrNo(CCA_ECCCIndicator) ? pgaHeaders.Load<ECCCPGAHeader>() : Factory.GetNull<ECCCPGAHeader>();
		public TCPGAHeader TCPGAHeader => YesNoList.IsYesOrNo(CCA_TCIndicator) ? pgaHeaders.Load<TCPGAHeader>() : Factory.GetNull<TCPGAHeader>();

		internal readonly CusAddInfoChildrenCollection pgaHeaders;

		#endregion

		#region SIMA

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public void RefreshSIMAMeasuresCollection()
		{
			CCA_SIMADumpingNumber = ZString.Empty;

			using (var dutyAndTaxManager = new DutyAndTaxManager(DutiesAndTaxes, this))
			{
				dutyAndTaxManager.RemoveNotOverriddenSIMADuties();

				bool hasMultipleAdds = false;
				SIMADumpingNumber dumpingToDefault = null;
				if (SIMAMeasures.Count == 1)
				{
					dumpingToDefault = SIMAMeasures[0];
				}
				else if (SIMAMeasures.Count > 1 && OnRefreshSIMAMeasureEvent != null)
				{
					dumpingToDefault = OnRefreshSIMAMeasureEvent();
					hasMultipleAdds = true;
				}

				if (dumpingToDefault != null)
				{
					CCA_SIMADumpingNumber = dumpingToDefault.CA_DumpingNumber;
					CCA_SIMADumpingDescInfo.RefreshBinding();
					HasMultipleADDs = hasMultipleAdds;
				}

				dutyAndTaxManager.AddSIMADutyIfNotExists();
			}
		}

		internal bool HasMultipleADDs { get; set; }

		public Func<SIMADumpingNumber> OnRefreshSIMAMeasureEvent;

		public SIMADumpingNumberCollection SIMAMeasures
		{
			get
			{
				if (fSIMAMeasures == null)
				{
					fSIMAMeasures = new CachedProperty<SIMADumpingNumberCollection>(Factory, () =>
					{
						var result = new SIMADumpingNumberCollection(Factory);
						if (IsHTS && !CC_TariffNum.IsEmpty && !CCA_RN_NKOrigin.IsEmpty)
						{
							var dumpingNumbers = DutyAndTaxManager.GetSIMADumpingNumbersFromTariffAndCountry(Factory, CC_TariffNum, CCA_RN_NKOrigin, ZDateTime.Today);
							if (dumpingNumbers != null)
							{
								dumpingNumbers.ForEach(d => result.AddNew(d.Item1, d.Item2));
							}
						}
						return result;
					});
				}

				return fSIMAMeasures.Value;
			}
		}
		CachedProperty<SIMADumpingNumberCollection> fSIMAMeasures;

		[ChildEditable(true)]
		public DutyAndTaxCollection DutiesAndTaxes
		{
			get
			{
				if (fDutiesAndTaxes == null)
				{
					fDutiesAndTaxes = new DutyAndTaxCollection(this);
					RegisterEditableChildObject(fDutiesAndTaxes);
				}

				return fDutiesAndTaxes;
			}
		}
		DutyAndTaxCollection fDutiesAndTaxes;

		public ZBool IsSIMADutyRequired
		{
			get { return DutyAndTaxManager.IsSIMADutyRequired(Factory, IsHTS, CC_TariffNum, CCA_RN_NKOrigin, CCA_SIMADumpingNumber, ZDateTime.Today, true); }
		}

		#endregion

		public SetterSuspender SetterSuspender => setterSuspender ?? (setterSuspender = new SetterSuspender());
		SetterSuspender setterSuspender;
	}
}
