using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseLoadingObject : NonPersistentBusinessObject
	{
		public ImportLicenseLoadingObject(ImportLicenseLoadingObjectParent parent) : base(parent.Factory)
		{
			Parent = Argument.NotNull(parent, nameof(parent));
		}

		public static class Schema
		{
			public const string ImportLicenseNo = "ImportLicenseNo";
			public const string RegistrationDate = "RegistrationDate";
			public const string InvoiceHeaderPK = "InvoiceHeaderPK";
			public const string Incoterm = "Incoterm";
			public const string VMLE = "VMLE";
			public const string VMCV = "VMCV";
			public const string NetWeight = "NetWeight";
			public const string UQ = "UQ";
			public const string ImportLicenseType = "ImportLicenseType";
			public const string ImportLicenseAuthorizationDate = "ImportLicenseAuthorizationDate";
			public const string ImportLicenseFeeType = "ImportLicenseFeeType";
			public const string Tariff = "Tariff";
			public const string Currency = "Currency";
			public const string ExchangeHedging = "ExchangeHedging";
			public const string NcmCode = "NcmCode";
			public const string ManufacturerIndicator = "ManufacturerIndicator";
			public const string NaladiHs = "NaladiHs";
			public const string GoodsCondition = "GoodsCondition";
			public const string DutyTaxRegime = "DutyTaxRegime";
			public const string DutyLegalBase = "DutyLegalBase";
			public const string ExchangeHedgeFinancialInstitution = "ExchangeHedgeFinancialInstitution";
			public const string ExchangeHedgeReason = "ExchangeHedgeReason";
			public const string GoodsOrigin = "GoodsOrigin";
			public const string AdditionalTariffsType = "AdditionalTariffsType";
			public const string SupplierAddressPK = "SupplierAddressPK";
			public const string ManufacturerAddressPK = "ManufacturerAddressPK";
		}

		public JobDeclaration Declaration { get; set; }

		public ImportLicenseLoadingObjectParent Parent { get; private set; }

		#region Lookups

		public ImportLicenseLoadingObjectLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = new ImportLicenseLoadingObjectLookups(this);
				}
				return fLookups;
			}
		}

		ImportLicenseLoadingObjectLookups fLookups;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public ImportLicenseLoadingObjectValidation Validation
		{
			get { return new ImportLicenseLoadingObjectValidation(this); }
		}

		protected ImportLicenseLoadingObjectValidation GetNewValidation()
		{
			return new ImportLicenseLoadingObjectValidation(this);
		}

		#endregion

		#region ImportLicenseNo

		[ReadOnly(true)]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.ImportLicenseLoadingObject|ImportLicenseNo", Caption = "Import License No.")]
		public ZString ImportLicenseNo
		{
			get { return fImportLicenseNo; }
			set { SetNonPersistentPropertyValue(ImportLicenseNoInfo, ref fImportLicenseNo, value); }
		}

		ZString fImportLicenseNo;

		public ZPropertyInfo ImportLicenseNoInfo => GetZPropertyInfo(Schema.ImportLicenseNo);

		#endregion

		#region RegistrationDate

		[ReadOnly(true)]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.ImportLicenseLoadingObject|RegistrationDate", Caption = "Registration Date")]
		public ZDateTime RegistrationDate
		{
			get { return fRegistrationDate; }
			set { SetNonPersistentPropertyValue(RegistrationDateInfo, ref fRegistrationDate, value); }
		}

		ZDateTime fRegistrationDate;

		public ZPropertyInfo RegistrationDateInfo => GetZPropertyInfo(Schema.RegistrationDate);

		#endregion

		#region InvoiceHeaderPK

		[List(nameof(Lookups) + "." + nameof(ImportLicenseLoadingObjectLookups.InvoiceHeaderList))]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.ImportLicenseLoadingObject|InvoiceHeaderPK", Caption = "Invoice No.")]
		public ZGuid InvoiceHeaderPK
		{
			get { return fInvoiceHeaderPK; }
			set
			{
				SetNonPersistentPropertyValue(InvoiceHeaderPKInfo, ref fInvoiceHeaderPK, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateInvoiceHeaderPK();
				}
			}
		}

		ZGuid fInvoiceHeaderPK;

		public ZPropertyInfo InvoiceHeaderPKInfo => GetZPropertyInfo(Schema.InvoiceHeaderPK);

		#endregion

		#region Incoterm

		[ReadOnly(true)]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.ImportLicenseLoadingObject|Incoterm", Caption = "Incoterm")]
		public ZString Incoterm
		{
			get { return fIncoterm; }
			set { SetNonPersistentPropertyValue(IncotermInfo, ref fIncoterm, value); }
		}

		ZString fIncoterm;

		public ZPropertyInfo IncotermInfo => GetZPropertyInfo(Schema.Incoterm);

		#endregion

		#region Currency

		[ReadOnly(true)]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.ImportLicenseLoadingObject|Currency", Caption = "Currency")]
		public ZString Currency
		{
			get { return fCurrency; }
			set { SetNonPersistentPropertyValue(CurrencyInfo, ref fCurrency, value); }
		}

		ZString fCurrency;

		public ZPropertyInfo CurrencyInfo => GetZPropertyInfo(Schema.Currency);

		#endregion

		#region VMLE

		[ReadOnly(true)]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.ImportLicenseLoadingObject|VMLE", Caption = "VMLE")]
		public ZDecimal VMLE
		{
			get { return fVMLE; }
			set { SetNonPersistentPropertyValue(VMLEInfo, ref fVMLE, value); }
		}

		ZDecimal fVMLE;

		public ZPropertyInfo VMLEInfo => GetZPropertyInfo(Schema.VMLE);

		#endregion

		#region VMCV

		[ReadOnly(true)]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.ImportLicenseLoadingObject|VMCV", Caption = "VMCV")]
		public ZDecimal VMCV
		{
			get { return fVMCV; }
			set { SetNonPersistentPropertyValue(VMCVInfo, ref fVMCV, value); }
		}

		ZDecimal fVMCV;

		public ZPropertyInfo VMCVInfo => GetZPropertyInfo(Schema.VMCV);

		#endregion

		#region NetWeight

		[ReadOnly(true)]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.ImportLicenseLoadingObject|NetWeight", Caption = "Net Weight")]
		public ZDecimal NetWeight
		{
			get { return fNetWeight; }
			set { SetNonPersistentPropertyValue(NetWeightInfo, ref fNetWeight, value); }
		}

		ZDecimal fNetWeight;

		public ZPropertyInfo NetWeightInfo => GetZPropertyInfo(Schema.NetWeight);

		#endregion

		#region UQ

		[ReadOnly(true)]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.ImportLicenseLoadingObject|UQ", Caption = "UQ")]
		public ZString UQ
		{
			get { return fUQ; }
			set { SetNonPersistentPropertyValue(UQInfo, ref fUQ, value); }
		}

		ZString fUQ;

		public ZPropertyInfo UQInfo => GetZPropertyInfo(Schema.UQ);

		#endregion

		#region ImportLicenseType

		[List(nameof(Lookups) + "." + nameof(ImportLicenseLoadingObjectLookups.ImportLicenseTypeList))]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.ImportLicenseLoadingObject|ImportLicenseType", Caption = "Import License Type")]
		public ZString ImportLicenseType
		{
			get { return fImportLicenseType; }
			set { SetNonPersistentPropertyValue(ImportLicenseTypeInfo, ref fImportLicenseType, value); }
		}

		ZString fImportLicenseType;

		public ZPropertyInfo ImportLicenseTypeInfo => GetZPropertyInfo(Schema.ImportLicenseType);

		#endregion

		#region ImportLicenseAuthorizationDate

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.ImportLicenseLoadingObject|ImportLicenseAuthorizationDate", Caption = "Import License Concession Date")]
		public ZDateTime ImportLicenseAuthorizationDate
		{
			get { return fImportLicenseAuthorizationDate; }
			set { SetNonPersistentPropertyValue(ImportLicenseAuthorizationDateInfo, ref fImportLicenseAuthorizationDate, value); }
		}

		ZDateTime fImportLicenseAuthorizationDate;

		public ZPropertyInfo ImportLicenseAuthorizationDateInfo => GetZPropertyInfo(Schema.ImportLicenseAuthorizationDate);

		#endregion

		#region ImportLicenseFeeType

		[List(nameof(Lookups) + "." + nameof(ImportLicenseLoadingObjectLookups.ImportLicenseFeeTypeList))]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.ImportLicenseLoadingObject|ImportLicenseFeeType", Caption = "Import License Fee Type")]
		public ZString ImportLicenseFeeType
		{
			get { return fImportLicenseFeeType; }
			set { SetNonPersistentPropertyValue(ImportLicenseFeeTypeInfo, ref fImportLicenseFeeType, value); }
		}

		ZString fImportLicenseFeeType;

		public ZPropertyInfo ImportLicenseFeeTypeInfo => GetZPropertyInfo(Schema.ImportLicenseFeeType);

		#endregion

		#region GoodsOrigin

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.ImportLicenseLoadingObject|GoodsOrigin", Caption = "Goods Origin")]
		public ZString GoodsOrigin
		{
			get { return fGoodsOrigin; }
			set { SetNonPersistentPropertyValue(GoodsOriginInfo, ref fGoodsOrigin, value); }
		}

		ZString fGoodsOrigin;

		public ZPropertyInfo GoodsOriginInfo => GetZPropertyInfo(Schema.GoodsOrigin);

		#endregion

		#region AdditionalTariffsType

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.ImportLicenseLoadingObject|AdditionalTariffsType", Caption = "Additional Tariffs Type")]
		public ZString AdditionalTariffsType
		{
			get { return fAdditionalTariffsType; }
			set { SetNonPersistentPropertyValue(AdditionalTariffsTypeInfo, ref fAdditionalTariffsType, value); }
		}

		ZString fAdditionalTariffsType;

		public ZPropertyInfo AdditionalTariffsTypeInfo => GetZPropertyInfo(Schema.AdditionalTariffsType);

		#endregion

		#region Tariff

		public ZString Tariff
		{
			get { return fTariff; }
			set { SetNonPersistentPropertyValue(TariffInfo, ref fTariff, value); }
		}

		ZString fTariff;

		public ZPropertyInfo TariffInfo => GetZPropertyInfo(Schema.Tariff);

		#endregion

		#region ExchangeHedging

		public ZString ExchangeHedging
		{
			get { return fExchangeHedging; }
			set { SetNonPersistentPropertyValue(ExchangeHedgingInfo, ref fExchangeHedging, value); }
		}

		ZString fExchangeHedging;

		public ZPropertyInfo ExchangeHedgingInfo => GetZPropertyInfo(Schema.ExchangeHedging);

		#endregion

		#region NcmCode

		public ZString NcmCode
		{
			get { return fNcmCode; }
			set { SetNonPersistentPropertyValue(NcmCodeInfo, ref fNcmCode, value); }
		}

		ZString fNcmCode;

		public ZPropertyInfo NcmCodeInfo => GetZPropertyInfo(Schema.NcmCode);

		#endregion

		#region ImportLicenseLoadingObjectNcmDetailsCollection

		[ChildEditable(true)]
		public ImportLicenseLoadingObjectNcmDetailsCollection ImportLicenseLoadingObjectNcmDetailsCollection
		{
			get
			{
				if (fImportLicenseLoadingObjectNcmDetailsCollection == null)
				{
					fImportLicenseLoadingObjectNcmDetailsCollection = new ImportLicenseLoadingObjectNcmDetailsCollection(this);
					RegisterEditableChildObject(fImportLicenseLoadingObjectNcmDetailsCollection);
				}
				return fImportLicenseLoadingObjectNcmDetailsCollection;
			}
		}

		ImportLicenseLoadingObjectNcmDetailsCollection fImportLicenseLoadingObjectNcmDetailsCollection;

		#endregion

		#region ManufacturerIndicator

		public ZString ManufacturerIndicator
		{
			get { return fManufacturerIndicator; }
			set { SetNonPersistentPropertyValue(ManufacturerIndicatorInfo, ref fManufacturerIndicator, value); }
		}

		ZString fManufacturerIndicator;

		public ZPropertyInfo ManufacturerIndicatorInfo => GetZPropertyInfo(Schema.ManufacturerIndicator);

		#endregion

		#region NaladiHs

		public ZString NaladiHs
		{
			get { return fNaladiHs; }
			set { SetNonPersistentPropertyValue(NaladiHsInfo, ref fNaladiHs, value); }
		}

		ZString fNaladiHs;

		public ZPropertyInfo NaladiHsInfo => GetZPropertyInfo(Schema.NaladiHs);

		#endregion

		#region GoodsCondition

		public ZString GoodsCondition
		{
			get { return fGoodsCondition; }
			set { SetNonPersistentPropertyValue(GoodsConditionInfo, ref fGoodsCondition, value); }
		}

		ZString fGoodsCondition;

		public ZPropertyInfo GoodsConditionInfo => GetZPropertyInfo(Schema.GoodsCondition);

		#endregion

		#region DutyTaxRegime

		public ZString DutyTaxRegime
		{
			get { return fDutyTaxRegime; }
			set { SetNonPersistentPropertyValue(DutyTaxRegimeInfo, ref fDutyTaxRegime, value); }
		}

		ZString fDutyTaxRegime;

		public ZPropertyInfo DutyTaxRegimeInfo => GetZPropertyInfo(Schema.DutyTaxRegime);

		#endregion

		#region DutyLegalBase

		public ZString DutyLegalBase
		{
			get { return fDutyLegalBase; }
			set { SetNonPersistentPropertyValue(DutyLegalBaseInfo, ref fDutyLegalBase, value); }
		}

		ZString fDutyLegalBase;

		public ZPropertyInfo DutyLegalBaseInfo => GetZPropertyInfo(Schema.DutyLegalBase);

		#endregion

		#region ExchangeHedgeFinancialInstitution

		public ZString ExchangeHedgeFinancialInstitution
		{
			get { return fExchangeHedgeFinancialInstitution; }
			set { SetNonPersistentPropertyValue(ExchangeHedgeFinancialInstitutionInfo, ref fExchangeHedgeFinancialInstitution, value); }
		}

		ZString fExchangeHedgeFinancialInstitution;

		public ZPropertyInfo ExchangeHedgeFinancialInstitutionInfo => GetZPropertyInfo(Schema.ExchangeHedgeFinancialInstitution);

		#endregion

		#region ExchangeHedgeReason

		public ZString ExchangeHedgeReason
		{
			get { return fExchangeHedgeReason; }
			set { SetNonPersistentPropertyValue(ExchangeHedgeReasonInfo, ref fExchangeHedgeReason, value); }
		}

		ZString fExchangeHedgeReason;

		public ZPropertyInfo ExchangeHedgeReasonInfo => GetZPropertyInfo(Schema.ExchangeHedgeReason);

		#endregion

		#region SupplierAddressPK

		public ZGuid SupplierAddressPK
		{
			get { return fSupplierAddressPK; }
			set { SetNonPersistentPropertyValue(SupplierAddressPKInfo, ref fSupplierAddressPK, value); }
		}

		ZGuid fSupplierAddressPK;

		public ZPropertyInfo SupplierAddressPKInfo => GetZPropertyInfo(Schema.SupplierAddressPK);

		#endregion

		#region ManufacturerAddressPK

		public ZGuid ManufacturerAddressPK
		{
			get { return fManufacturerAddressPK; }
			set { SetNonPersistentPropertyValue(ManufacturerAddressPKInfo, ref fManufacturerAddressPK, value); }
		}

		ZGuid fManufacturerAddressPK;

		public ZPropertyInfo ManufacturerAddressPKInfo => GetZPropertyInfo(Schema.ManufacturerAddressPK);

		#endregion
	}
}
