using System;
using System.Collections;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.IN;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(JobComInvoiceHeader))]
sealed class JobComInvoiceHeaderTest : BaseJobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
{
	public override void TestLocalCurrencyCodeCoreOverride()
	{
		AssertEquals("LocalCurrencyCode", Core.Constants.CurrencyCodes.India, Factory.New<JobComInvoiceHeader>().LocalCurrencyCode);
	}

	protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<InvoiceApportionCharge>);

	protected override Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<InvoiceCharge>);

	public void TestJZ_IncoTermPlace_Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(Factory.New<JobComInvoiceHeader>().JZ_IncoTermPlaceInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Incoterm Place", captionResourceString.Caption);
			AssertEquals("Inco Place", captionResourceString.MediumCaption);
			AssertEquals("Place", captionResourceString.ShortCaption);
		});
	}

	public void TestJZ_OA_ExporterAddress_Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(Factory.New<JobComInvoiceHeader>().JZ_OA_ExporterAddressInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Third Party", captionResourceString.Caption);
			AssertEquals("Third Party", captionResourceString.MediumCaption);
			AssertEquals("T. Party", captionResourceString.ShortCaption);
		});
	}

	public void TestExchangeRateType()
	{
		declaration.AutoCreateChargesBasedOnIncoTerm = false;
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		CombineAssertions(() =>
		{
			AssertEquals("Should be CUS for Import", ExchangeRateType.Customs, ((MasterFiles.Business.ICurrencyConverterDataProvider)invoiceHeader).RateType);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			AssertEquals("Should be CUE for export", ExchangeRateType.CustomsSecondary, ((MasterFiles.Business.ICurrencyConverterDataProvider)invoiceHeader).RateType);
		});
	}

	public void TestJZ_ExporterContractNumber()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(Factory.New<JobComInvoiceHeader>().JZ_ExporterContractNumberInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Exporter Contract No.", captionResourceString.Caption);
			AssertEquals("Exp. Contract No.", captionResourceString.MediumCaption);
			AssertEquals("Exp. Contr. No.", captionResourceString.ShortCaption);
		});
	}

	public void TestJZ_PaymentDays_Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(Factory.New<JobComInvoiceHeader>().JZ_PaymentDaysInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Payment Days.", captionResourceString.Caption);
			AssertEquals("Pay. Days.", captionResourceString.MediumCaption);
			AssertEquals("Pay. Days.", captionResourceString.ShortCaption);
			AssertEquals("Max Length", 3, invoiceHeader.JZ_PaymentDaysInfo.MaxLength);
		});
	}

	public void TestJZ_PaymentMethod_Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(Factory.New<JobComInvoiceHeader>().JZ_PaymentMethodInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Nature Of Payment", captionResourceString.Caption);
			AssertEquals("MediumCaption", "Nat. Of Pay.", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Pay. Nat.", captionResourceString.ShortCaption);
		});
	}

	public void TestBuyingPartyOrgPK_Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(Factory.New<JobComInvoiceHeader>().BuyingPartyOrgPKInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Buyer", captionResourceString.Caption);
			AssertEquals("MediumCaption", "Buyer", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Buyer", captionResourceString.ShortCaption);
		});
	}

	public void TestBuyingPartyAddressPK_Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(Factory.New<JobComInvoiceHeader>().BuyingPartyAddressPKInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Buyer Address", captionResourceString.Caption);
			AssertEquals("MediumCaption", "Buyer Addr.", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Buyer Addr.", captionResourceString.ShortCaption);
		});
	}

	public void TestBuyerDocAddress()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		var buyerDocAddress = invoice.BuyerDocAddress;

		CombineAssertions(() =>
		{
			AssertNotNull("docAddresses.GetDocAddress(DocAddressType.BuyerDocAddress)", buyerDocAddress);

			AssertEquals(DocAddressType.BuyingParty, buyerDocAddress.DocAddressType);
			AssertEquals(ContactType.NoContactType, buyerDocAddress.DefaultContactType);
			AssertEquals("BYP", buyerDocAddress.E2_AddressType);
			AssertEquals("JZ", buyerDocAddress.E2_ParentTableCode);
			AssertEquals(true, buyerDocAddress.CanOverride);
			AssertEquals(invoice.PK, buyerDocAddress.E2_ParentID);
		});
	}

	public void TestSWControlCollection()
	{
		var collection = Factory.New<JobComInvoiceHeader>().SWControls;
		AssertType<SWControlCollection>(collection);
	}

	public void TestICusSupportingInfoTypeSupporter()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		Integration.Customs.ICusSupportingInfoTypeSupporter supporter = invoiceHeader;
		var supportingInfoTypes = supporter.GetCusSupportingInfoTypes();

		AssertEquals(typeof(SWControl), supportingInfoTypes[CusSupportingInfoTypeList.Codes.SingleWindowControl]);
		AssertEquals(typeof(SupportingDocument), supportingInfoTypes[CusSupportingInfoTypeList.Codes.SupportingDocument]);

		AssertType<CusSupportingInfoTypeSupporterFetchStrategy>(supporter.GetFetchStrategies().First());
	}

	public void TestGetSequenceNumberGenerator()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		var supportingInfoParent = (ICusSupportingInfoWithSerialNoParent)invoiceHeader;

		var expectedValues = new[]
		{
			invoiceHeader.SWControlsLineNumberGenerator,
			invoiceHeader.SupportingDocumentLineNumberGenerator,
		};

		var actualValues = supportingInfoParent.GetCusSupportingInfoTypes().Keys.Select(type => supportingInfoParent.GetSequenceNumberGenerator(type));

		AssertEquals("Count", expectedValues.Length, actualValues.Count());
		AssertContainsExactElementsInExactOrder("Sequence Number Generators", expectedValues, actualValues);
	}

	public void TestBuyerDocAddressAndBuyingPartyAddressPKInSync()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		var buyerDocAddress = invoice.BuyerDocAddress;
		var orgHeader1 = Factory.New<OrgHeader>();
		var address1 = orgHeader1.MainAddress;
		var orgHeader2 = Factory.New<OrgHeader>();
		var address2 = orgHeader2.MainAddress;

		invoice.BuyingPartyAddressPK = address1.PK;
		AssertEquals("When Buyer address set", address1.PK, buyerDocAddress.E2_OA_Address);

		invoice.BuyingPartyAddressPK = ZGuid.Empty;
		AssertEquals("When Buyer address set to empty", ZGuid.Empty, buyerDocAddress.E2_OA_Address);

		buyerDocAddress.E2_OA_Address = address2.PK;
		AssertEquals("When BuyerDocAddress set", address2.PK, invoice.BuyingPartyAddressPK);

		buyerDocAddress.E2_OA_Address = ZGuid.Empty;
		AssertEquals("When BuyerDocAddress set to empty", ZGuid.Empty, invoice.BuyingPartyAddressPK);
	}

	public void TestJZ_OA_BuyerAddress()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var buyerDocAddress = invoice.BuyerDocAddress;
		var orgHeader1 = Factory.New<OrgHeader>();
		var address1 = orgHeader1.MainAddress;
		var orgHeader2 = Factory.New<OrgHeader>();
		var address2 = orgHeader2.MainAddress;
		buyerDocAddress.DocAddressType = DocAddressType.None;
		buyerDocAddress.E2_OA_Address = address1.PK;
		AssertEquals("JZ_OA_BuyerAddress when DocAddressType - None", ZGuid.Empty, invoice.JZ_OA_BuyerAddress);

		buyerDocAddress.DocAddressType = DocAddressType.BuyingParty;
		buyerDocAddress.E2_OA_Address = ZGuid.Empty;
		AssertEquals("JZ_OA_BuyerAddress when DocAddressType - BuyingParty and E2_OA_Address Empty", ZGuid.Empty, invoice.JZ_OA_BuyerAddress);

		buyerDocAddress.E2_OA_Address = address2.PK;
		AssertEquals("JZ_OA_BuyerAddress when DocAddressType - BuyingParty", address2.PK, invoice.JZ_OA_BuyerAddress);
	}

	public void TestAuthorizedEconomicOperatorOrgPK_Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(Factory.New<JobComInvoiceHeader>().AuthorizedEconomicOperatorOrgPKInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Authorized Economic Operator", captionResourceString.Caption);
			AssertEquals("MediumCaption", "AEO", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "AEO", captionResourceString.ShortCaption);
		});
	}

	public void TestAuthorizedEconomicOperatorOrgPK()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		var authorizedEconomicOperatorAddress = invoice.AuthorizedEconomicOperatorAddress;

		authorizedEconomicOperatorAddress.OrganisationPK = ZGuid.BrettsGuid;
		AssertEquals("When Buyer address set", ZGuid.BrettsGuid, invoice.AuthorizedEconomicOperatorOrgPK);

		authorizedEconomicOperatorAddress.OrganisationPK = ZGuid.Empty;
		AssertEquals("When Buyer address set to empty", ZGuid.Empty, invoice.AuthorizedEconomicOperatorOrgPK);
	}

	public void TestDefaultAuthorizedEconomicOperatorFromSupplier()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();

		var org1 = Factory.NewWithValidTestData<OrgHeader>();
		org1.MainAddress.OA_RN_NKCountryCode = "IN";
		var addressIN1 = org1.Addresses.AddNew();
		OrgCusCode cusCode1 = org1.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.BSN, "1234");
		cusCode1.OK_OA_PremisesAddress = addressIN1.PK;
		var org2 = Factory.NewWithValidTestData<OrgHeader>();
		org2.MainAddress.OA_RN_NKCountryCode = "IN";
		var addressIN2 = org2.Addresses.AddNew();
		OrgCusCode cusCode2 = org2.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.AEO, "1234");
		cusCode2.OK_OA_PremisesAddress = addressIN2.PK;
		var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
		supplierDocumentaryAddress.OrganisationPK = ZGuid.BrettsGuid;

		supplierDocumentaryAddress.E2_OA_Address = addressIN1.PK;
		AssertNotEquals("When AEO empty and MainSupplier field with org of BSN cusCode, Export declaration", supplierDocumentaryAddress.OrganisationPK, invoice.AuthorizedEconomicOperatorOrgPK);

		invoice.AuthorizedEconomicOperatorOrgPK = ZGuid.BrettsGuid;
		supplierDocumentaryAddress.E2_OA_Address = addressIN2.PK;
		AssertNotEquals("When AEO not empty and MainSupplier field with org of AEO cusCode, Export declaration", supplierDocumentaryAddress.OrganisationPK, invoice.AuthorizedEconomicOperatorOrgPK);

		invoice.AuthorizedEconomicOperatorOrgPK = ZGuid.Empty;
		supplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
		supplierDocumentaryAddress.E2_OA_Address = addressIN2.PK;
		AssertEquals("When AEO empty and MainSupplier field with org of AEO cusCode, Export declaration", supplierDocumentaryAddress.OrganisationPK, invoice.AuthorizedEconomicOperatorOrgPK);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		invoice.AuthorizedEconomicOperatorOrgPK = ZGuid.Empty;
		supplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
		supplierDocumentaryAddress.E2_OA_Address = addressIN2.PK;
		AssertNotEquals("When AEO empty and MainSupplier field with org of AEO cusCode, Import declaration", supplierDocumentaryAddress.OrganisationPK, invoice.AuthorizedEconomicOperatorOrgPK);
	}

	public void TestAuthorizedEconomicOperatorCountry_Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(Factory.New<JobComInvoiceHeader>().AuthorizedEconomicOperatorCountryInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Authorized Economic Operator Country", captionResourceString.Caption);
			AssertEquals("MediumCaption", "AEO Country", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Country", captionResourceString.ShortCaption);
		});
	}

	public void TestAuthorizedEconomicOperatorCountry()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		var authorizedEconomicOperatorAddress = invoice.AuthorizedEconomicOperatorAddress;

		var org1 = Factory.NewWithValidTestData<OrgHeader>();
		org1.MainAddress.OA_RN_NKCountryCode = "IN";
		var addressIN1 = org1.Addresses.AddNew();
		var org2 = Factory.NewWithValidTestData<OrgHeader>();
		org2.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
		var addressIN2 = org2.Addresses.AddNew();

		authorizedEconomicOperatorAddress.E2_OA_Address = addressIN1.PK;
		AssertEquals("When CountryCode set", "IN", invoice.AuthorizedEconomicOperatorCountry);

		authorizedEconomicOperatorAddress.E2_OA_Address = addressIN2.PK;
		AssertEquals("When CountryCode empty set", ZString.Empty, invoice.AuthorizedEconomicOperatorCountry);

		authorizedEconomicOperatorAddress.E2_OA_Address = ZGuid.Empty;
		AssertEquals("When E2_OA_Address empty set", ZString.Empty, invoice.AuthorizedEconomicOperatorCountry);
	}

	public void TestAuthorizedEconomicOperatorCode_Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(Factory.New<JobComInvoiceHeader>().AuthorizedEconomicOperatorCodeInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Authorized Economic Operator Code", captionResourceString.Caption);
			AssertEquals("MediumCaption", "AEO Code", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Code", captionResourceString.ShortCaption);
		});
	}

	public void TestAuthorizedEconomicOperatorCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		var authorizedEconomicOperatorAddress = invoice.AuthorizedEconomicOperatorAddress;

		var org1 = Factory.NewWithValidTestData<OrgHeader>();
		var addressIN1 = org1.Addresses.AddNew();
		OrgCusCode cusCode1 = org1.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.AEO, "1234");
		cusCode1.OK_OA_PremisesAddress = addressIN1.PK;
		var org2 = Factory.NewWithValidTestData<OrgHeader>();
		var addressIN2 = org2.Addresses.AddNew();
		OrgCusCode cusCode2 = org2.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.BSN, "5678");
		cusCode2.OK_OA_PremisesAddress = addressIN2.PK;

		authorizedEconomicOperatorAddress.E2_OA_Address = addressIN1.PK;
		AssertEquals("When CusCode AEO set", "1234", invoice.AuthorizedEconomicOperatorCode);

		authorizedEconomicOperatorAddress.E2_OA_Address = addressIN2.PK;
		AssertEquals("When CusCode BSN set", ZString.Empty, invoice.AuthorizedEconomicOperatorCode);

		authorizedEconomicOperatorAddress.E2_OA_Address = ZGuid.Empty;
		AssertEquals("When E2_OA_Address empty set", ZString.Empty, invoice.AuthorizedEconomicOperatorCode);
	}

	public void TestAuthorizedEconomicOperatorRole_Caption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(Factory.New<JobComInvoiceHeader>().JZ_AuthorizedEconomicOperatorRoleInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Authorized Economic Operator Role", captionResourceString.Caption);
			AssertEquals("MediumCaption", "AEO Role", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "Role", captionResourceString.ShortCaption);
		});
	}

	public void TestAuthorizedEconomicOperatorAddress()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		var authorizedEconomicOperatorAddress = invoice.AuthorizedEconomicOperatorAddress;

		CombineAssertions(() =>
		{
			AssertNotNull("docAddresses.GetDocAddress(DocAddressType.AuthorizedEconomicOperatorAddress)", authorizedEconomicOperatorAddress);

			AssertEquals(DocAddressType.AuthorizedEconomicOperatorAddress, authorizedEconomicOperatorAddress.DocAddressType);
			AssertEquals(ContactType.NoContactType, authorizedEconomicOperatorAddress.DefaultContactType);
			AssertEquals("AEO", authorizedEconomicOperatorAddress.E2_AddressType);
			AssertEquals("JZ", authorizedEconomicOperatorAddress.E2_ParentTableCode);
			AssertEquals(invoice.PK, authorizedEconomicOperatorAddress.E2_ParentID);
		});
	}

	public void TestSupportingDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		var collection = invoice.SupportingDocuments;
		AssertType<SupportingDocumentCollection>(collection);
	}

	protected override Hashtable ExpectedDocAddressTypes
	{
		get
		{
			if (expectedDocAddressTypes == null)
			{
				expectedDocAddressTypes = base.ExpectedDocAddressTypes;
				expectedDocAddressTypes.Add(DocAddressTypes.Codes.BuyingParty, DocAddressType.BuyingParty);
				expectedDocAddressTypes.Add(DocAddressTypes.Codes.AuthorizedEconomicOperatorAddress, DocAddressType.AuthorizedEconomicOperatorAddress);
			}
			return expectedDocAddressTypes;
		}
	}
	Hashtable expectedDocAddressTypes;

	public void TestJZ_RX_NKInvoice_Currency()
	{
		var standardCurrencyCode = RefDataSetupTestHelper.SetupStandardCurrencyCodes(Factory);
		var nonStandardCurrencyCode = "MNT";
		AssertEquals(0, declaration.NonStandardExchangeRates.Count);

		invoiceHeader.JZ_RX_NKInvoice_Currency = nonStandardCurrencyCode;
		AssertEquals(1, declaration.NonStandardExchangeRates.Count);
		AssertEquals(nonStandardCurrencyCode, declaration.NonStandardExchangeRates.First().CSI_RX_NKCurrency);

		invoiceHeader.JZ_RX_NKInvoice_Currency = standardCurrencyCode;
		AssertEquals(0, declaration.NonStandardExchangeRates.Count);

		invoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;
		AssertEquals(0, declaration.NonStandardExchangeRates.Count);
	}

	public void TestJZ_InvoiceCurrExRate()
	{
		AssertEquals(4, invoiceHeader.JZ_InvoiceCurrExRateInfo.GetAttribute<DecimalPlacesAttribute>().DecimalPlaces);
	}

	public void TestIsNonStandardCurrency()
	{
		var standardCurrency = RefDataSetupTestHelper.SetupStandardCurrencyCodes(Factory);
		var nonStandardCurrency = "ZZZ";

		invoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;
		Assert(!invoiceHeader.IsNonStandardCurrency);

		invoiceHeader.JZ_RX_NKInvoice_Currency = standardCurrency;
		Assert(!invoiceHeader.IsNonStandardCurrency);

		invoiceHeader.JZ_RX_NKInvoice_Currency = nonStandardCurrency;
		Assert(invoiceHeader.IsNonStandardCurrency);
	}

	public override void TestIWeightApportioneeRoundingIssue()
	{
		RefDataSetupTestHelper.SetupStandardCurrencyCodes(Factory, "XYZ");
		base.TestIWeightApportioneeRoundingIssue();
	}

	new JobDeclaration declaration => (JobDeclaration)base.declaration;

	new JobComInvoiceHeader invoiceHeader => (JobComInvoiceHeader)base.invoiceHeader;

	protected override bool ShouldBOGetSavedWithDetachedInvoiceHeader(BaseJobComInvoiceHeader invoice, Type invoiceLineAddInfoChildType, BusinessObject bO)
	{
		return base.ShouldBOGetSavedWithDetachedInvoiceHeader(invoice, invoiceLineAddInfoChildType, bO) || bO is NonStandardExchangeRate;
	}

	public void TestJZ_GSTPaymentStatus()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		CaptionTestHelper.AssertCaptions(invoiceHeader.JZ_GSTPaymentStatusInfo, "IGST Payment", "IGST Pay.", "IGST Pay.");
		AssertEquals("Max Length", 3, invoiceHeader.JZ_GSTPaymentStatusInfo.MaxLength);

		invoiceHeader.JZ_GSTPaymentStatus = IGSTPaymentStatusCodeList.Codes.NotApplicable;
		AssertEquals(invoiceLine1.JI_GSTPayNotApplicable, true);
		AssertEquals(invoiceLine2.JI_GSTPayNotApplicable, true);

		invoiceHeader.JZ_GSTPaymentStatus = IGSTPaymentStatusCodeList.Codes.ExportAgainstPayment;
		AssertEquals(invoiceLine1.JI_GSTPayNotApplicable, false);
		AssertEquals(invoiceLine2.JI_GSTPayNotApplicable, false);
	}

	public void TestPMVAndTotalPMV_UpdatesOnInvoiceCurrExRateChange()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var foreignCurrency = "xyz";
		RefDataSetupTestHelper.SetupStandardCurrencyCodes(Factory, foreignCurrency);
		RefDataSetupTestHelper.SetExchangeRates(Factory, foreignCurrency, 8.00m);

		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_ValuationMarkup = 110.00m;
		invoiceLine.JI_UnitPrice = 10.00m;
		invoiceLine.JI_InvoiceQuantity = 10;

		invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.India;
		AssertEquals(invoiceHeader.JZ_InvoiceCurrExRate, 1.00m);
		AssertEquals("PMV (INR)", 11.00m, invoiceLine.JI_PMV);
		AssertEquals("Total PMV", 110.00m, invoiceLine.JI_TotalPMV);

		invoiceHeader.JZ_RX_NKInvoice_Currency = foreignCurrency;
		AssertEquals(invoiceHeader.JZ_InvoiceCurrExRate, 8.00m);
		AssertEquals("PMV (INR)", 88.00m, invoiceLine.JI_PMV);
		AssertEquals("Total PMV", 880.00m, invoiceLine.JI_TotalPMV);
	}

	public void TestIGSTPaymentNotApplicable()
	{
		var invoiceHeader = Factory.New<JobComInvoiceHeader>();
		invoiceHeader.JZ_GSTPaymentStatus = IGSTPaymentStatusCodeList.Codes.NotApplicable;
		AssertEquals(invoiceHeader.IGSTPaymentNotApplicable, true);

		invoiceHeader.JZ_GSTPaymentStatus = IGSTPaymentStatusCodeList.Codes.ExportAgainstPayment;
		AssertEquals(invoiceHeader.IGSTPaymentNotApplicable, false);
	}

	public void TestJZ_InvoiceNumber_MaxLength()
	{
		AssertEquals("MaxLength", 17, Factory.New<JobComInvoiceHeader>().JZ_InvoiceNumberInfo.MaxLength);
	}

	[TestDate(2024, 6, 13)]
	public void TestIDateOfValuationProvider()
	{
		var provider = (IDateOfValuationProvider)invoiceHeader;
		AssertEquals("Declaration without ValuationDate set", new ZDate(2024, 6, 13), provider.DateOfValuation);
		declaration.JE_ValuationDate = new ZDate(2022, 3, 16);
		AssertEquals("Declaration with ValuationDate set", new ZDate(2022, 3, 16), provider.DateOfValuation);
	}

	public void TestGetStandaloneIncoTermAndChargeFactoryCountryContext()
	{
		CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			declaration.MakeNonPersistent();

			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<CommonIncoTermAndCustomsChargeFactory>(invoice.IncoTermAndChargeFactory);

			invoice.JZ_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportIncoTermAndCustomsChargeFactory>(invoice.IncoTermAndChargeFactory);
		});
	}
}

