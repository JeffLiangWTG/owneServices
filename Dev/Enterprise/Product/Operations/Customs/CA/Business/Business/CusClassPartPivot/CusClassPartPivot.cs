using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using CargoWiseOne.ResourceStrings;
	using Enterprise.Customs.Business.MultiLineAddInfos;
	using Enterprise.Customs.Common;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Schema;
	using static Enterprise.Integration.Customs;

	public partial class CusClassPartPivot : AutoCusClassPartPivot
		, IHasPGARequirements
		, ICusAddInfoTypeSupporter
		, ISupportDataImporting
		, ICusCodeDataTypeSupporter
		, CA.ICusClassPartPivot
		, IPGARequirementSupporter
	{
		public CusClassPartPivot(BusinessObjectFactory factory, DataRow row)
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

		#region Schema
		public new class Schema : AutoCusClassPartPivot.Schema
		{
			public const string CI_CC_FormattedTariff = "CI_CC_FormattedTariff";
			public const string CI_CC_CA_ValueForDutyCode = "CI_CC_CA_ValueForDutyCode";
			public const string CI_CC_CA_99TariffCode = "CI_CC_CA_99TariffCode";
			public const string CI_CC_CA_AuthorityNumber = "CI_CC_CA_AuthorityNumber";
			public const string CI_CC_CA_TRSNumber = "CI_CC_CA_TRSNumber";
			public const string CI_CC_CA_TreatmentCode = "CI_CC_CA_TreatmentCode";
			public const string CI_CC_CA_GSTStatusCode = "CI_CC_CA_GSTStatusCode";
			public const string CI_CC_CA_ETExemption = "CI_CC_CA_ETExemption";
			public const string CI_CC_CA_ETRateCode = "CI_CC_CA_ETRateCode";
			public const string CCA_SIMADumpingDesc = "CCA_SIMADumpingDesc";
		}
		#endregion

		#region Overrides

		#region Duty and Tax RatesForCurrentCountry

		protected override ZString GetDutyRateForCurrentCountry()
		{
			var result = ZString.Empty;
			if (IsImportClassification)
			{
				using (var manager = GetClonedDutyAndTaxManager())
				{
					result = manager.DeriveDutyRateShortDescription();
				}
				ClearClonedBOs();
			}
			return result;
		}

		protected override ZString GetTaxRateForCurrentCountry()
		{
			var result = ZString.Empty;
			if (IsImportClassification)
			{
				using (var manager = GetClonedDutyAndTaxManager())
				{
					result = manager.DeriveTaxRateShortDescription();
				}
				ClearClonedBOs();
			}
			return result;
		}

		public override void OnSaving()
		{
			ClearClonedBOs();
			base.OnSaving();
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			CusClassPartPivot result = (CusClassPartPivot)base.CloneInternal(args);
			using (result.GetValidationSuspender())
			{
				result.Details.CopyPersistentValuesFrom(Details, new BusinessObjectCloneArgs(new[] { CusCAClassification.Schema.CCA_ParentID }));
			}
			return result;
		}

		DutyAndTaxManager GetClonedDutyAndTaxManager()
		{
			if (pivot == null || pivot.IsDeleted)
			{
				pivot = this.Clone();
			}
			return new DutyAndTaxManager(new DutyAndTaxCollection(pivot), pivot);
		}
		CusClassPartPivot pivot;

		void ClearClonedBOs()
		{
			if (pivot != null && !pivot.IsDeleted)
			{
				pivot.Delete();
			}
		}

		#endregion

		#region CI_ChildType

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.ClassificationTypes))]
		public override ZString CI_ChildType
		{
			get { return base.CI_ChildType; }
			set
			{
				var oldValue = CI_ChildType;
				base.CI_ChildType = value;
				var newValue = CI_ChildType;
				if (!IsCopying && oldValue != newValue)
				{
					if (newValue != ClassificationTypeList.Codes.HTI)
					{
						ClearPGAIndicators();
					}
					Details.MarkAsNeedingValidation();
				}
			}
		}

		public void ClearPGAIndicators()
		{
			CCA_CFIAIndicator = ZString.Empty;
			CCA_CNSCIndicator = ZString.Empty;
			CCA_DFOIndicator = ZString.Empty;
			CCA_ECCCIndicator = ZString.Empty;
			CCA_GACIndicator = ZString.Empty;
			CCA_HCIndicator = ZString.Empty;
			CCA_NRCanIndicator = ZString.Empty;
			CCA_PHACIndicator = ZString.Empty;
			CCA_TCIndicator = ZString.Empty;
		}

		#endregion

		#region CI_TariffNum

		[ReadOnlyMember(nameof(IsTariffNumReadOnly))]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.Tariffs))]
		[BusinessObjectTestExclude]
		public override ZString CI_TariffNum
		{
			get { return base.CI_TariffNum; }
			set
			{
				var oldFormattedTariffNum = CI_FormattedTariffNum;
				ZString newValue = CurrentTariffFormatter.Format(value);
				bool hasChanges = base.CI_TariffNum != newValue;
				if (hasChanges)
				{
					base.CI_TariffNum = newValue;
					if (!IsCopying)
					{
						RefreshSIMAMeasuresCollection();
						if (CI_ChildType == ClassificationTypeList.Codes.HTI && !CI_CC.IsValid)
						{
							PGARequirements.SetDefaultValueForIndicatorWhenTariffChanged();
						}
						Validation.ValidateCI_CC();
					}
					LogIfPropertyValueChange(Res.GetString("9DEBD73A-3DC1-4210-B569-025F4420019A", "Product Class. Tariff #"), oldFormattedTariffNum, CI_FormattedTariffNum);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public void RefreshSIMAMeasuresCollection()
		{
			if (!UpdatingPivotFromInvoiceLine && !CI_CC.IsValid)
			{
				CCA_SIMADumpingNumber = ZString.Empty;

				using (var dutyAndTaxManager = new DutyAndTaxManager(DutiesAndTaxes, this))
				{
					dutyAndTaxManager.RemoveNotOverriddenSIMADuties();

					SIMADumpingNumber dumpingToDefault = null;
					bool hasMultipleAdds = false;
					var isImportingData = ((ISupportDataImporting)this).IsImportingData;
					if (SIMAMeasures.Count == 1)
					{
						dumpingToDefault = SIMAMeasures[0];
					}
					else if (SIMAMeasures.Count > 1 && OnRefreshSIMAMeasureEvent != null && !isImportingData)
					{
						dumpingToDefault = OnRefreshSIMAMeasureEvent();
					}
					else if (SIMAMeasures.Count > 1 && isImportingData)
					{
						dumpingToDefault = SIMAMeasures.OfType<SIMADumpingNumber>().FirstOrDefault();
						hasMultipleAdds = true;
					}

					if (dumpingToDefault != null)
					{
						CCA_SIMADumpingNumber = dumpingToDefault.CA_DumpingNumber;
						HasMultipleADDs = hasMultipleAdds;
					}

					dutyAndTaxManager.AddSIMADutyIfNotExists();
				}
			}
		}
		public Func<SIMADumpingNumber> OnRefreshSIMAMeasureEvent;

		public TariffPropertyInfo CI_TariffNumTariffInfo
		{
			get
			{
				return new TariffPropertyInfo
				{
					TariffType = IsHTS ? TariffType.Import : TariffType.Export,
					DateForDutyRate = ZDateTime.Now
				};
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.Tariffs))]
		public override ZString CI_FormattedTariffNum
		{
			get { return base.CI_FormattedTariffNum; }
			set { base.CI_FormattedTariffNum = value; }
		}

		protected override bool UseUniversalTariff => false;

		#endregion

		#region CI_CC

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.ClassificationList))]
		[ReadOnlyMember(nameof(CI_CC_ReadOnly))]
		public override ZGuid CI_CC
		{
			get { return base.CI_CC; }
			set
			{
				bool hasChanges = base.CI_CC != value;
				if (hasChanges)
				{
					shouldRefreshSIMADataForCC = true;
					shouldRefreshPGA = true;

					var oldFormattedTariff = CI_CC_FormattedTariff;
					var oldTariffCode = CI_CC_CA_99TariffCode;
					base.CI_CC = value;
					LogIfPropertyValueChange(Res.GetString("76F4C82E-14D2-445E-B0DE-1863A875A652", "Classification Lookup Class. Tariff #"), oldFormattedTariff, CI_CC_FormattedTariff);
					LogIfPropertyValueChange(Res.GetString("2AE0DEA5-F190-480F-BA0A-1FD9CB00D979", "Classification Lookup Tariff Code"), oldTariffCode, CI_CC_CA_99TariffCode);
					if (!IsCopying)
					{
						Validation.ValidateCI_TariffNum();
					}

					PGARequirements.RefreshBinding();
					PGARequirements.RefreshBindingForDeclaredPGAHeaders();
					DutiesAndTaxesForCC.RefreshBinding();
				}
			}
		}

		public override ZString CCA_TreatmentCode
		{
			get { return base.CCA_TreatmentCode; }
			set
			{
				var oldTreatmentCode = CCA_TreatmentCode;
				base.CCA_TreatmentCode = value;
				LogIfPropertyValueChange(Res.GetString("FB0A0D56-E172-45A3-8EA8-95471302E74D", "TT"), oldTreatmentCode, CCA_TreatmentCode);
			}
		}

		#endregion

		#region CI_LastAuditedDate

		[ReadOnlyMember(nameof(AlwaysReturnTrue))]
		public override ZDateTime CI_LastAuditedDate
		{
			get { return base.CI_LastAuditedDate; }
		}

		#endregion

		#region CCA_CFIAUSStateOfOrigin

		[ReadOnlyMember(nameof(CCA_CFIAUSStateOfOrigin_ReadOnly))]
		public override ZString CCA_CFIAUSStateOfOrigin
		{
			get => base.CCA_CFIAUSStateOfOrigin;
			set => base.CCA_CFIAUSStateOfOrigin = value;
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

		[ReadOnlyMember(nameof(CCA_ProvinceOfOrigin_ReadOnly))]
		public override ZString CCA_ProvinceOfOrigin
		{
			get => base.CCA_ProvinceOfOrigin;
			set
			{
				base.CCA_ProvinceOfOrigin = value;
				RefreshBindingForDeclaredPGAHeaders();
			}
		}

		#endregion

		#region CCA_RN_NKSource
		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.DefaultOrigins))]
		public override ZString CCA_RN_NKSource
		{
			get { return base.CCA_RN_NKSource; }
			set
			{
				var oldValue = base.CCA_RN_NKSource;
				base.CCA_RN_NKSource = value;
				if (!IsCopying && oldValue != CCA_RN_NKSource)
				{
					if (!CCA_StateOfSource.IsEmpty && CCA_StateOfSource_ReadOnly)
					{
						CCA_StateOfSource = ZString.Empty;
					}
					RefreshBindingForDeclaredPGAHeaders();
				}
			}
		}

		#endregion

		#region CCA_StateOfSource
		[List(nameof(CAClassificationLookups) + "." + nameof(CusCAClassificationLookups.StatesOfExport))]
		[ReadOnlyMember(nameof(CCA_StateOfSource_ReadOnly))]
		public override ZString CCA_StateOfSource
		{
			get { return base.CCA_StateOfSource; }
			set
			{
				Details.CCA_StateOfSource = value;
				RefreshBindingForDeclaredPGAHeaders();
			}
		}

		bool CCA_StateOfSource_ReadOnly
		{
			get
			{
				return CCA_RN_NKSource != Enterprise.Core.Constants.CountryCodes.UnitedStates;
			}
		}
		#endregion

		public bool AlwaysReturnTrue
		{
			get { return true; }
		}

		#endregion

		#region New Properties

		#region CI_CC_FormattedTariff

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.Tariffs))]
		public ZString CI_CC_FormattedTariff
		{
			get { return Classification != null ? Classification.CC_FormattedTariffNum : ZString.Empty; }
		}

		public ZPropertyInfo CI_CC_FormattedTariffInfo
		{
			get { return GetZPropertyInfo(Schema.CI_CC_FormattedTariff); }
		}

		ZString CI_CC_TariffNum
		{
			get { return Classification != null ? Classification.CC_TariffNum : ZString.Empty; }
		}

		#endregion

		#region CI_CC_CA_ValueForDutyCode

		[MaxLength(AutoCAAddInfo.Schema.CA_ValueForDutyCodeMaxLength)]
		public ZString CI_CC_CA_ValueForDutyCode
		{
			get { return Classification != null ? Classification.CCA_ValueForDutyCode : ZString.Empty; }
		}

		public ZPropertyInfo CI_CC_CA_ValueForDutyCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CI_CC_CA_ValueForDutyCode); }
		}

		#endregion

		#region CI_CC_CA_GSTStatusCode

		[MaxLength(AutoCAAddInfo.Schema.CA_GSTStatusCodeMaxLength)]
		public ZString CI_CC_CA_GSTStatusCode
		{
			get { return Classification != null ? Classification.CCA_GSTStatusCode : ZString.Empty; }
		}

		public ZPropertyInfo CI_CC_CA_GSTStatusCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CI_CC_CA_GSTStatusCode); }
		}

		#endregion

		#region CI_CC_CA_ETExemption

		[MaxLength(AutoCAAddInfo.Schema.CA_ETExemptionMaxLength)]
		public ZString CI_CC_CA_ETExemption
		{
			get { return Classification != null ? Classification.CCA_ETExemption : ZString.Empty; }
		}

		public ZPropertyInfo CI_CC_CA_ETExemptionInfo
		{
			get { return GetZPropertyInfo(Schema.CI_CC_CA_ETExemption); }
		}

		#endregion

		#region CI_CC_CA_ETRateCode

		[MaxLength(AutoCAAddInfo.Schema.CA_ETRateCodeMaxLength)]
		public ZString CI_CC_CA_ETRateCode
		{
			get { return Classification != null ? Classification.CCA_ETRateCode : ZString.Empty; }
		}

		public ZPropertyInfo CI_CC_CA_ETRateCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CI_CC_CA_ETRateCode); }
		}

		#endregion

		#region CI_CC_CA_99TariffCode

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.Tariffs))]
		[MaxLength(AutoCAAddInfo.Schema.CA_99TariffCodeMaxLength)]
		public ZString CI_CC_CA_99TariffCode
		{
			get { return Classification != null ? Classification.CCA_99TariffCode : ZString.Empty; }
		}

		public ZPropertyInfo CI_CC_CA_99TariffCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CI_CC_CA_99TariffCode); }
		}

		#endregion

		#region CI_CC_CA_AuthorityNumber

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.AuthorityNumberList))]
		[MaxLength(AutoCAAddInfo.Schema.CA_AuthorityNumberMaxLength)]
		[ResourceStringData("CusClassPartPivot|CI_CC_CA_AuthorityNumber", Caption = "Special Authority/Permit", ShortCaption = "Auth/Pmt", MediumCaption = "Special Auth/Pmt",
			FullDescription = "A permit number or Order in Council (OIC) authorization number to import goods under special conditions.")]
		public ZString CI_CC_CA_AuthorityNumber
		{
			get { return Classification != null ? Classification.CCA_AuthorityNumber : ZString.Empty; }
		}

		public ZPropertyInfo CI_CC_CA_AuthorityNumberInfo
		{
			get { return GetZPropertyInfo(Schema.CI_CC_CA_AuthorityNumber); }
		}

		#endregion

		#region

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.AuthorityNumberList))]
		[MaxLength(CusCAClassification.Schema.CCA_AuthorityNumberMaxLength)]
		[ResourceStringData("CusClassPartPivot|CA_AuthorityNumber", Caption = "Special Authority/Permit", ShortCaption = "Auth/Pmt", MediumCaption = "Special Auth/Pmt",
			FullDescription = "A permit number or Order in Council (OIC) authorization number to import goods under special conditions.")]
		public override ZString CCA_AuthorityNumber
		{
			get { return base.CCA_AuthorityNumber; }
			set { base.CCA_AuthorityNumber = value; }
		}

		#endregion

		#region CI_CC_CA_TRSNumber

		[MaxLength(AutoCAAddInfo.Schema.CA_TRSNumberMaxLength)]
		public ZString CI_CC_CA_TRSNumber
		{
			get { return Classification != null ? Classification.CCA_TRSNumber : ZString.Empty; }
		}

		public ZPropertyInfo CI_CC_CA_TRSNumberInfo
		{
			get { return GetZPropertyInfo(Schema.CI_CC_CA_TRSNumber); }
		}

		#endregion

		#region CI_CC_CA_TreatmentCode

		[MaxLength(AutoCAAddInfo.Schema.CA_TreatmentCodeMaxLength)]
		public ZString CI_CC_CA_TreatmentCode
		{
			get { return Classification != null ? Classification.CCA_TreatmentCode : ZString.Empty; }
		}

		public ZPropertyInfo CI_CC_CA_TreatmentCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CI_CC_CA_TreatmentCode); }
		}

		#endregion

		#region CCA_SIMADumpingDesc

		public ZString CCA_SIMADumpingDesc
		{
			get
			{
				if (Classification is CusClassification classification)
				{
					return DutyAndTaxManager.GetSIMADumpingDescription(Factory, classification.CC_TariffNum, classification.CCA_SIMADumpingNumber, ZDateTime.Today);
				}
				else
				{
					return DutyAndTaxManager.GetSIMADumpingDescription(Factory, CI_TariffNum.IsEmpty ? CI_CC_TariffNum : CI_TariffNum, CCA_SIMADumpingNumber, ZDateTime.Today);
				}
			}
		}

		public virtual ZPropertyInfo CCA_SIMADumpingDescInfo
		{
			get { return GetZPropertyInfo(Schema.CCA_SIMADumpingDesc); }
		}

		#endregion

		#region CCA_SIMADumpingNumber

		public override ZString CCA_SIMADumpingNumber
		{
			get
			{
				if (Classification is CusClassification classification)
				{
					return classification.CCA_SIMADumpingNumber;
				}
				else
				{
					return base.CCA_SIMADumpingNumber;
				}
			}
			set
			{
				if (Classification == null)
				{
					base.CCA_SIMADumpingNumber = value;
				}
			}
		}

		#endregion

		public ZBool IsSIMADutyRequired
		{
			get { return DutyAndTaxManager.IsSIMADutyRequired(Factory, IsHTS, CI_TariffNum, CCA_RN_NKOrigin, CCA_SIMADumpingNumber, ZDateTime.Today, true); }
		}

		#region CFIARegistrationNumbers

		[ChildEditable(true)]
		public CFIARegistrationNumberCollection CFIARegistrationNumbers
		{
			get
			{
				if (fCFIARegistrationNumbers == null)
				{
					fCFIARegistrationNumbers = new CFIARegistrationNumberCollection(this);
					fCFIARegistrationNumbers.Load();
					RegisterEditableChildObject(fCFIARegistrationNumbers);
				}
				return fCFIARegistrationNumbers;
			}
		}
		CFIARegistrationNumberCollection fCFIARegistrationNumbers;

		#endregion

		#region SITTCertificationNumbers

		[ChildEditable(true)]
		public SITTCertificationNumberCollection SITTCertificationNumbers
		{
			get
			{
				if (fSITTCertificationNumbers == null)
				{
					fSITTCertificationNumbers = new SITTCertificationNumberCollection(this);
					fSITTCertificationNumbers.Load();
					RegisterEditableChildObject(fSITTCertificationNumbers);
				}
				return fSITTCertificationNumbers;
			}
		}
		SITTCertificationNumberCollection fSITTCertificationNumbers;

		#endregion

		public new CusAttributeFilterCollection Attributes1
		{
			get { return (CusAttributeFilterCollection)base.Attributes1; }
		}

		public new CusAttributeFilterCollection Attributes2
		{
			get { return (CusAttributeFilterCollection)base.Attributes2; }
		}

		public new CusAttributeFilterCollection Attributes3
		{
			get { return (CusAttributeFilterCollection)base.Attributes3; }
		}

		protected override Customs.Business.CusAttributeFilterCollection GetNewAttributeFilterCollection(ZString attributeType)
		{
			return new CusAttributeFilterCollection(this, attributeType);
		}

		public new CusClassPartPivot Parent
		{
			get { return (CusClassPartPivot)base.Parent; }
		}

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
					base.CCA_OA_Manufacturer = value;
					fCCA_OA_Manufacturer_ZAddress = null;
					RefreshBindingForDeclaredPGAHeaders();
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

		public DutyAndTaxForDisplayCollection DutiesAndTaxesForCC
		{
			get
			{
				if (fDutiesAndTaxesForCC == null)
				{
					fDutiesAndTaxesForCC = new DutyAndTaxForDisplayCollection(this.Factory);
					fDutiesAndTaxesForCC.SetReadOnlyIncludingChildren(true);
				}
				if (shouldRefreshSIMADataForCC)
				{
					fDutiesAndTaxesForCC.RemoveAll();
					if (Classification is CusClassification classification)
					{
						fDutiesAndTaxesForCC.AddRange(classification.DutiesAndTaxes.ToArray());
					}
					shouldRefreshSIMADataForCC = false;
				}

				return fDutiesAndTaxesForCC;
			}
		}
		DutyAndTaxForDisplayCollection fDutiesAndTaxesForCC;

		bool shouldRefreshSIMADataForCC = true;

		#region SIMA Measures
		public SIMADumpingNumberCollection SIMAMeasures
		{
			get
			{
				if (fSIMAMeasures == null)
				{
					fSIMAMeasures = new CachedProperty<SIMADumpingNumberCollection>(Factory, () =>
					{
						var result = new SIMADumpingNumberCollection(Factory);
						if (!CI_CC.IsValid && IsHTS && !CI_TariffNum.IsEmpty && !CCA_RN_NKOrigin.IsEmpty)
						{
							var dumpingNumbers = DutyAndTaxManager.GetSIMADumpingNumbersFromTariffAndCountry(Factory, CI_TariffNum, CCA_RN_NKOrigin, ZDateTime.Today);
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
		#endregion

		internal ZBool UpdatingPivotFromInvoiceLine
		{
			get { return fUpdatingPivotFromInvoiceLine; }
			set { fUpdatingPivotFromInvoiceLine = value; }
		}
		ZBool fUpdatingPivotFromInvoiceLine;

		internal bool HasMultipleADDs { get; set; }

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

		#region PGARequirements

		[ChildEditable]
		public PGARequirementCollection PGARequirements
		{
			get
			{
				if (pgaAgencyRequirements == null)
				{
					var provider = GetPGARequirementProvider();
					pgaAgencyRequirements = new PGARequirementCollection(provider);
					pgaAgencyRequirements.Populate();
					SetPGARequirementsToReadOnlyOrNot(pgaAgencyRequirements);
					RegisterEditableChildObject(pgaAgencyRequirements);
				}
				if (shouldRefreshPGA)
				{
					shouldRefreshPGA = false;
					var provider = GetPGARequirementProvider();
					pgaAgencyRequirements.PGAProvider = provider;
					pgaAgencyRequirements.Populate();
					pgaAgencyRequirements.PGATabCollectionNeedToBeRefreshed = true;
					SetPGARequirementsToReadOnlyOrNot(pgaAgencyRequirements);
				}
				return pgaAgencyRequirements;
			}
		}

		PGARequirementCollection pgaAgencyRequirements;

		void SetPGARequirementsToReadOnlyOrNot(PGARequirementCollection pgaAgencyRequirements)
		{
			if (Classification is CusClassification classification)
			{
				pgaAgencyRequirements.SetReadOnlyIncludingChildren(true);
				foreach (var pgaRequirement in pgaAgencyRequirements.Cast<PGARequirement>())
				{
					pgaRequirement.PGAHeader.SetPGAReadOnly();
				}
				var requirementsParent = (IHasPGARequirements)classification;
				((IZPropertyInfoObsolete)requirementsParent.JI_BrandNameInfo).ReadOnly = true;
				((IZPropertyInfoObsolete)requirementsParent.JI_ModelInfo).ReadOnly = true;
				((IZPropertyInfoObsolete)requirementsParent.OA_ManufacturerInfo).ReadOnly = true;
				((IZPropertyInfoObsolete)requirementsParent.RN_NKCountryOfOriginInfo).ReadOnly = true;
				((IZPropertyInfoObsolete)requirementsParent.RN_NKCountryOfSourceInfo).ReadOnly = true;
				((IZPropertyInfoObsolete)requirementsParent.RW_NKCountryOfSourceStateInfo).ReadOnly = true;
				((IZPropertyInfoObsolete)requirementsParent.RW_NKOriginStateInfo).ReadOnly = true;
				((IZPropertyInfoObsolete)requirementsParent.TariffInfo).ReadOnly = true;
			}
			else
			{
				pgaAgencyRequirements.SetReadOnlyIncludingChildren(false);
			}
		}

		PGARequirementProvider GetPGARequirementProvider()
		{
			if (Classification is CusClassification classification)
			{
				return new PGARequirementProvider(classification);
			}
			else
			{
				return new PGARequirementProvider(this);
			}
		}

		bool shouldRefreshPGA;

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

		void RefreshBindingForDeclaredPGAHeaders()
		{
			if (!CI_CC.IsValid)
			{
				PGARequirements.RefreshBindingForDeclaredPGAHeaders();
			}
		}

		#endregion

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
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

		#region IHasPGARequirements Members

		ZGuid IHasPGARequirements.OA_Manufacturer
		{
			get => CCA_OA_Manufacturer;
			set => CCA_OA_Manufacturer = value;
		}

		ZPropertyInfo IHasPGARequirements.OA_ManufacturerInfo => CCA_OA_ManufacturerInfo;

		ZString IHasPGARequirements.RN_NKCountryOfOrigin
		{
			get => CCA_RN_NKOrigin;
			set => CCA_RN_NKOrigin = value;
		}

		ZPropertyInfo IHasPGARequirements.RN_NKCountryOfOriginInfo => CCA_RN_NKOriginInfo;

		ZString IHasPGARequirements.RW_NKOriginState
		{
			get => CCA_ProvinceOfOrigin;
			set => CCA_ProvinceOfOrigin = value;
		}

		ZPropertyInfo IHasPGARequirements.RW_NKOriginStateInfo => CCA_ProvinceOfOriginInfo;

		ZString IHasPGARequirements.RN_NKCountryOfSource
		{
			get => CCA_RN_NKSource;
			set => CCA_RN_NKSource = value;
		}

		ZPropertyInfo IHasPGARequirements.RN_NKCountryOfSourceInfo => CCA_RN_NKSourceInfo;

		ZString IHasPGARequirements.RW_NKCountryOfSourceState
		{
			get => CCA_StateOfSource;
			set => CCA_StateOfSource = value;
		}

		ZPropertyInfo IHasPGARequirements.RW_NKCountryOfSourceStateInfo => CCA_StateOfSourceInfo;

		ZAddress IHasPGARequirements.OA_ManufacturerAddress_ZAddress => CCA_OA_Manufacturer_ZAddress;

		OrgHeaderCollection IHasPGARequirements.ManufacturersLookup => Lookups.Manufacturers;

		RefCountryCollection IHasPGARequirements.CountryOfOriginsLookup => CAClassificationLookups.Origins;

		CodeDescriptionPairList IHasPGARequirements.StateCodeListLookup => CAClassificationLookups.StatesOfOrigin;

		RefCountryCollection IHasPGARequirements.CountryOfSourceLookup => CAClassificationLookups.CFIAOrigins;
		CodeDescriptionPairList IHasPGARequirements.CountryOfSourceStateLookup => CAClassificationLookups.CFIAStatesOfOrigin;

		ZString IHasPGARequirements.JI_BrandName
		{
			get
			{
				return Part?.OP_Brand ?? ZString.Empty;
			}
			set
			{
				if (Part is OrgSupplierPart part)
				{
					part.OP_Brand = value;
				}
			}
		}

		ZPropertyInfo IHasPGARequirements.JI_BrandNameInfo => Part?.OP_BrandInfo;

		ZString IHasPGARequirements.JI_Model
		{
			get
			{
				return Part?.OP_Model ?? ZString.Empty;
			}
			set
			{
				if (Part is OrgSupplierPart part)
				{
					part.OP_Model = value;
				}
			}
		}

		ZPropertyInfo IHasPGARequirements.JI_ModelInfo => Part?.OP_ModelInfo;
		ZString IHasPGARequirements.Tariff => CI_TariffNum;
		ZPropertyInfo IHasPGARequirements.TariffInfo => CI_TariffNumInfo;

		#endregion

		#region ISupportDataImporting Members

		bool ISupportDataImporting.IsImportingData
		{
			get { return fIsImportingData; }
			set { fIsImportingData = value; }
		}
		bool fIsImportingData;

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : PivotFetchStrategy
		{
			public Strategy(CusClassPartPivot pivot)
				: base(pivot)
			{
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				var pivot = (CusClassPartPivot)BusinessObject;
				Factory.AddFetchHint(OrgSupplierPartSchema.PK, pivot.CI_OP);
			}
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.CFIANumber, typeof(CFIARegistrationNumber));
			result.Add(CusCodeDataTypeList.Codes.SITTNumber, typeof(SITTCertificationNumber));
			return result;
		}

		#endregion

		#region IPGARequirementSupporter Members

		ZBool IPGARequirementSupporter.IsDeleted => IsDeleted;
		ZString IPGARequirementSupporter.Tariff => CI_TariffNum;
		ZDateTime IPGARequirementSupporter.EffectiveDate => ZDate.Today;
		ZBool IPGARequirementSupporter.IsPGARequirementEffective => IsImport;
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

		public override ZDecimal CCA_AMMVPerUnit
		{
			get => base.CCA_AMMVPerUnit;
			set
			{
				var oldValue = CCA_AMMVPerUnit;
				base.CCA_AMMVPerUnit = value;
				var newValue = CCA_AMMVPerUnit;
				if (!IsCopying && oldValue != newValue)
				{
					if (newValue.IsEmpty || CCA_AMMVPercentage.IsEmpty)
					{
						CCA_AMMVPercentageInfo.RefreshBinding();
					}
					else
					{
						CCA_AMMVPercentage = ZDecimal.Zero;
					}
				}
			}
		}

		bool CCA_AMMVPerUnit_ReadOnly => !CCA_AMMVPercentage.IsEmpty && CCA_AMMVPerUnit.IsEmpty;

		[ReadOnlyMember(nameof(CCA_AMMVPerUnit_ReadOnly))]
		public override ZString CCA_AMMVPerUnitCurrency { get => base.CCA_AMMVPerUnitCurrency; set => base.CCA_AMMVPerUnitCurrency = value; }

		[ReadOnlyMember(nameof(CCA_AMMVPercentage_ReadOnly))]
		public override ZDecimal CCA_AMMVPercentage
		{
			get => base.CCA_AMMVPercentage;
			set
			{
				var oldValue = CCA_AMMVPercentage;
				base.CCA_AMMVPercentage = value;
				var newValue = CCA_AMMVPercentage;
				if (!IsCopying && oldValue != newValue)
				{
					if (newValue.IsEmpty || CCA_AMMVPerUnit.IsEmpty)
					{
						CCA_AMMVPerUnitInfo.RefreshBinding();
					}
					else
					{
						CCA_AMMVPerUnit = ZDecimal.Zero;
						CCA_AMMVPerUnitCurrency = ZString.Empty;
					}
				}
			}
		}

		bool CCA_AMMVPercentage_ReadOnly => !CCA_AMMVPerUnit.IsEmpty && CCA_AMMVPercentage.IsEmpty;

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CI_ChildType = ClassificationTypeList.Codes.HTI;
		}

		ZBool IsHTE => CI_ChildType == ClassificationTypeList.Codes.HTE;
		ZBool IsSHB => CI_ChildType == ClassificationTypeList.Codes.SHB;

		protected override bool IsExportClassificationCore => IsHTE || IsSHB;

		protected override IEnumerable<string> GetSetterSuspenderSupportedFields()
		{
			return base.GetSetterSuspenderSupportedFields().Union(new[]
			{
				Schema.CCA_OA_Manufacturer,
				Schema.CCA_HCIndicator,
				Schema.CCA_PHACIndicator,
				Schema.CCA_NRCanIndicator,
				Schema.CCA_DFOIndicator,
				Schema.CCA_GACIndicator,
				Schema.CCA_CFIAIndicator,
				Schema.CCA_CNSCIndicator,
				Schema.CCA_ECCCIndicator,
				Schema.CCA_TCIndicator
			});
		}

		#endregion
	}
}
