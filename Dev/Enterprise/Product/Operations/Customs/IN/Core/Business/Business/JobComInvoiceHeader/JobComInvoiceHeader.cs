using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.IN;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IN.Business;

public partial class JobComInvoiceHeader : AutoINJobComInvoiceHeader, Integration.Customs.ICusSupportingInfoTypeSupporter, ICusSupportingInfoWithSerialNoParent
	, ICurrencyConverterDataProvider
	, IDateOfValuationProvider
{
	public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	#region Schema

	public new class Schema : AutoINJobComInvoiceHeader.Schema
	{
		public const string BuyingPartyAddressPK = nameof(JobComInvoiceHeader.BuyingPartyAddressPK);
		public const string BuyingPartyOrgPK = nameof(JobComInvoiceHeader.BuyingPartyOrgPK);
		public const string AuthorizedEconomicOperatorOrgPK = nameof(JobComInvoiceHeader.AuthorizedEconomicOperatorOrgPK);
		public const string AuthorizedEconomicOperatorCountry = nameof(JobComInvoiceHeader.AuthorizedEconomicOperatorCountry);
		public const string AuthorizedEconomicOperatorCode = nameof(JobComInvoiceHeader.AuthorizedEconomicOperatorCode);
	}

	#endregion

	protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.India;

	protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext()
	{
		return base.GetStandaloneIncoTermAndChargeFactoryCountryContext() + this.GetCustomsChargeTypeListCacheKey();
	}

	[ResourceStringData("5c8e1309-ad14-42fb-aa4c-966579657fc6", Caption = "Incoterm Place", MediumCaption = "Inco Place", ShortCaption = "Place")]
	public override ZString JZ_IncoTermPlace { get => base.JZ_IncoTermPlace; set => base.JZ_IncoTermPlace = value; }

	[ResourceStringData("2A646229-39CB-4E6D-B46D-63E84A70AC76", Caption = "Third Party", MediumCaption = "Third Party", ShortCaption = "T. Party")]
	public override ZGuid JZ_OA_ExporterAddress { get => base.JZ_OA_ExporterAddress; set => base.JZ_OA_ExporterAddress = value; }

	[ResourceStringData("4A831B07-5724-4A16-9B4C-BEB46658E67B", Caption = "Exporter Contract No.", MediumCaption = "Exp. Contract No.", ShortCaption = "Exp. Contr. No.")]
	public override ZString JZ_ExporterContractNumber { get => base.JZ_ExporterContractNumber; set => base.JZ_ExporterContractNumber = value; }

	[MaxLength(3)]
	[ResourceStringData("CF49DACE-F4A8-4CCB-AE63-345E67E86D4D", Caption = "Payment Days.", MediumCaption = "Pay. Days.", ShortCaption = "Pay. Days.")]
	public override ZInt JZ_PaymentDays { get => base.JZ_PaymentDays; set => base.JZ_PaymentDays = value; }

	protected override ExchangeRateType RateTypeCore => IsImport ? ExchangeRateType.Customs : IsExport ? ExchangeRateType.CustomsSecondary : base.RateTypeCore;

	[ResourceStringData("0A4CD8C9-E363-4358-8CAB-0EC1118A41C1", Caption = "Nature Of Payment", MediumCaption = "Nat. Of Pay.", ShortCaption = "Pay. Nat.")]
	public override ZString JZ_PaymentMethod { get => base.JZ_PaymentMethod; set => base.JZ_PaymentMethod = value; }

	[MaxLength(17)]
	public override ZString JZ_InvoiceNumber { get => base.JZ_InvoiceNumber; set => base.JZ_InvoiceNumber = value; }

	#region BuyerDocAddress

	[List(nameof(BuyerDocAddress) + "." + nameof(JobDocAddress.Lookups) + "." + nameof(JobDocAddressLookups.Address_List))]
	[ResourceStringData("C8E29C97-ABB7-4F71-8CC9-A5E87AE971B1", Caption = "Buyer Address", MediumCaption = "Buyer Addr.", ShortCaption = "Buyer Addr.")]
	public ZGuid BuyingPartyAddressPK
	{
		get => BuyerDocAddress.E2_OA_Address;
		set => BuyerDocAddress.E2_OA_Address = value;
	}

	public ZPropertyInfo BuyingPartyAddressPKInfo => GetWrappedZPropertyInfo(Schema.BuyingPartyAddressPK, x => BuyerDocAddress.E2_OA_AddressInfo);

	[List(nameof(Lookups) + "." + nameof(Lookups.Buyers))]
	[ResourceStringData("47A37AB2-B14F-4A2C-98F0-9A25EAF75266", Caption = "Buyer", MediumCaption = "Buyer", ShortCaption = "Buyer")]
	public ZGuid BuyingPartyOrgPK
	{
		get => BuyerDocAddress.OrganisationPK;
		set => BuyerDocAddress.OrganisationPK = value;
	}

	[DecimalPlaces(4)]
	public override ZDecimal JZ_InvoiceCurrExRate
	{
		get => base.JZ_InvoiceCurrExRate;
		set
		{
			var oldValue = JZ_InvoiceCurrExRate;
			base.JZ_InvoiceCurrExRate = value;
			if (!IsCopying && oldValue != JZ_InvoiceCurrExRate)
			{
				InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.CalculatePMV());
			}
		}
	}

	public ZPropertyInfo BuyingPartyOrgPKInfo => GetWrappedZPropertyInfo(Schema.BuyingPartyOrgPK, x => BuyerDocAddress.OrganisationPKInfo);

	public JobDocAddress BuyerDocAddress
	{
		get
		{
			if (buyerDocAddress == null || buyerDocAddress.IsDeleted)
			{
				buyerDocAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.BuyingParty);
			}
			return buyerDocAddress;
		}
	}
	JobDocAddress buyerDocAddress;

	#endregion

	#region AuthorizedEconomicOperator

	[List(nameof(Lookups) + "." + nameof(Lookups.AuthorizedEconomicOperatorsList))]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceHeader|AuthorizedEconomicOperatorOrgPK", Caption = "Authorized Economic Operator", MediumCaption = "AEO", ShortCaption = "AEO")]
	public ZGuid AuthorizedEconomicOperatorOrgPK
	{
		get => AuthorizedEconomicOperatorAddress.OrganisationPK;
		set => AuthorizedEconomicOperatorAddress.OrganisationPK = value;
	}

	public ZPropertyInfo AuthorizedEconomicOperatorOrgPKInfo => GetWrappedZPropertyInfo(Schema.AuthorizedEconomicOperatorOrgPK, x => AuthorizedEconomicOperatorAddress.OrganisationPKInfo);

	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceHeader|AuthorizedEconomicOperatorCountry", Caption = "Authorized Economic Operator Country", MediumCaption = "AEO Country", ShortCaption = "Country")]
	public ZString AuthorizedEconomicOperatorCountry => AuthorizedEconomicOperatorAddress.Organisation?.MainAddress.OA_RN_NKCountryCode ?? ZString.Empty;

	public ZPropertyInfo AuthorizedEconomicOperatorCountryInfo => GetZPropertyInfo(Schema.AuthorizedEconomicOperatorCountry);

	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceHeader|AuthorizedEconomicOperatorCode", Caption = "Authorized Economic Operator Code", MediumCaption = "AEO Code", ShortCaption = "Code")]
	public ZString AuthorizedEconomicOperatorCode => AuthorizedEconomicOperatorAddress.Organisation.GetAEONumber();

	public ZPropertyInfo AuthorizedEconomicOperatorCodeInfo => GetZPropertyInfo(Schema.AuthorizedEconomicOperatorCode);

	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceHeader|JZ_AuthorizedEconomicOperatorRole", Caption = "Authorized Economic Operator Role", MediumCaption = "AEO Role", ShortCaption = "Role")]
	public override ZString JZ_AuthorizedEconomicOperatorRole { get => base.JZ_AuthorizedEconomicOperatorRole; set => base.JZ_AuthorizedEconomicOperatorRole = value; }

	public JobDocAddress AuthorizedEconomicOperatorAddress
	{
		get
		{
			if (authorizedEconomicOperatorAddress == null || authorizedEconomicOperatorAddress.IsDeleted)
			{
				authorizedEconomicOperatorAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.AuthorizedEconomicOperatorAddress);
				authorizedEconomicOperatorAddress.MarkParentAsNeedingValidation = true;
			}
			return authorizedEconomicOperatorAddress;
		}
	}
	JobDocAddress authorizedEconomicOperatorAddress;

	public void DefaultAuthorizedEconomicOperatorFromSupplier()
	{
		if (AuthorizedEconomicOperatorOrgPK.IsEmpty && PersistentDeclaration?.Supplier is OrgHeader supplier && !supplier.GetAEONumber().IsEmpty)
		{
			AuthorizedEconomicOperatorOrgPK = supplier.PK;
		}
	}

	#endregion

	protected override void DocAddressChangedCore(JobDocAddress docAddress)
	{
		base.DocAddressChangedCore(docAddress);
		if (docAddress.DocAddressType == DocAddressType.BuyingParty)
		{
			JZ_OA_BuyerAddress = docAddress.E2_OA_Address;
		}
	}

	public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => new JobComInvoiceHeaderJobDocAddressValidation(addressToValidate, this);

	protected override DocAddressType[] SupportedAddressTypesCore() => base.SupportedAddressTypesCore().Concat(new[] { DocAddressType.BuyingParty, DocAddressType.AuthorizedEconomicOperatorAddress }).ToArray();

	protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobComInvoiceHeaderFetchStrategy(this);

	protected override ZDateTime EffectiveValuationDateCore => CusEntryInstructions.FirstOrDefault(x => x.CEI_DateForDuty is { IsValid: true })?.CEI_DateForDuty ?? base.EffectiveValuationDateCore;

	protected override MasterFiles.Business.CurrencyConverter GetNewCurrencyConverter() => new CurrencyConverter(Factory, this);

	public bool IsNonStandardCurrency => (CurrencyConverter as CurrencyConverter).IsNonStandardCurrency(JZ_RX_NKInvoice_Currency);

	NonStandardExchangeRateCollection ICurrencyConverterDataProvider.NonStandardExchangeRates => JobDeclaration?.NonStandardExchangeRates;

	[MaxLength(Schema.JZ_GSTPaymentStatusMaxLength)]
	[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.IGSTPaymentStatusCodeList))]
	[ResourceStringData("B3B9702F-E9B7-46CE-BF3B-3536C206FCC8", Caption = "IGST Payment", MediumCaption = "IGST Pay.", ShortCaption = "IGST Pay.")]
	public override ZString JZ_GSTPaymentStatus
	{
		get => base.JZ_GSTPaymentStatus;
		set
		{
			var wasNotApplicable = IGSTPaymentNotApplicable;
			base.JZ_GSTPaymentStatus = value;
			if (!IsCopying && wasNotApplicable != IGSTPaymentNotApplicable)
			{
				InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.RefreshGSTPayNotApplicable());
			}
		}
	}

	public override ZDateTime JZ_ValuationDateOverride
	{
		get => base.JZ_ValuationDateOverride;
		set
		{
			var oldValue = JZ_ValuationDateOverride;
			base.JZ_ValuationDateOverride = value;
			if (!IsCopying && oldValue != JZ_ValuationDateOverride)
			{
				JobComInvoiceLines.MarkAsNeedingValidation();
			}
		}
	}

	[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
	[ChildEditable(true)]
	public SWControlCollection SWControls
	{
		get
		{
			if (fSWControls == null)
			{
				fSWControls = new SWControlCollection(this);
				fSWControls.Load();
				RegisterEditableChildObject(fSWControls);
			}
			return fSWControls;
		}
	}

	SWControlCollection fSWControls;

	public HugeSequenceNumberGenerator SWControlsLineNumberGenerator => fSWControlsLineNumberGenerator ??= new HugeSequenceNumberGenerator(() => SWControls);
	HugeSequenceNumberGenerator fSWControlsLineNumberGenerator;

	#region ICusSupportingInfoTypeSupporter

	IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => new Dictionary<ZString, Type>
	{
		{ CusSupportingInfoTypeList.Codes.SingleWindowControl, typeof(SWControl) },
		{ CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(SupportingDocument) }
	};

	IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
	{
		yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
	}

	#endregion

	HugeSequenceNumberGenerator ICusSupportingInfoWithSerialNoParent.GetSequenceNumberGenerator(string type)
	{
		return type switch
		{
			CusSupportingInfoTypeList.Codes.SingleWindowControl => SWControlsLineNumberGenerator,
			CusSupportingInfoTypeList.Codes.SupportingDocument => SupportingDocumentLineNumberGenerator,
			_ => null
		};
	}

	public bool IGSTPaymentNotApplicable => JZ_GSTPaymentStatus == IGSTPaymentStatusCodeList.Codes.NotApplicable;

	[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
	[ChildEditable(true)]
	public SupportingDocumentCollection SupportingDocuments
	{
		get
		{
			if (fSupportingDocument == null)
			{
				fSupportingDocument = new SupportingDocumentCollection(this);
				fSupportingDocument.Load();
				RegisterEditableChildObject(fSupportingDocument);
			}
			return fSupportingDocument;
		}
	}

	SupportingDocumentCollection fSupportingDocument;

	public HugeSequenceNumberGenerator SupportingDocumentLineNumberGenerator => fSupportingDocumentLineNumberGenerator ??= new HugeSequenceNumberGenerator(() => SupportingDocuments);
	HugeSequenceNumberGenerator fSupportingDocumentLineNumberGenerator;

	ZDateTime IDateOfValuationProvider.DateOfValuation => EffectiveValuationDate;
}
