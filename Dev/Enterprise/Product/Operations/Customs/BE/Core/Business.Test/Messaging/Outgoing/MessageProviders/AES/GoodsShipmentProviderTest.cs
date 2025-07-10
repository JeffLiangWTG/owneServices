using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.BE.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.BE.Business.Declaration.CusEntryInstruction;
using JobDeclaration = Enterprise.Customs.BE.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class GoodsShipmentProviderTest : Customs.Business.Testing.DataProviderTestCase<GoodsShipmentProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new GoodsShipmentProvider(null));
	}

	public void TestNatureOfTransaction()
	{
		invoice.JZ_ValuationCode = "11";
		invoice2.JZ_ValuationCode = "11";
		AssertEquals("11", provider.NatureOfTransaction);
	}

	public void TestCountryOfExport()
	{
		declaration.JE_RL_NKOrigin = "BE";
		AssertEquals("BE", provider.CountryOfExport);
	}

	public void TestCountryOfDestination()
	{
		declaration.JE_GoodsDestination = "NL";
		AssertEquals("NL", provider.CountryOfDestination);
	}

	public void TestCountryOfExport_TransitionPeriod()
	{
		declaration.JE_RL_NKOrigin = "BE";
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
		{
			AssertEquals(string.Empty, provider.CountryOfExport);
		}
	}

	public void TestCountryOfDestination_TransitionPeriod()
	{
		declaration.JE_GoodsDestination = "NL";
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
		{
			AssertEquals(string.Empty, provider.CountryOfDestination);
		}
	}

	public void TestAdditionalSupplyChainActors()
	{
		entryInstruction.CusSupplyChainActorReferences.AddNew();
		entryInstruction.CusSupplyChainActorReferences.AddNew();
		AssertEquals("2 CusSupplyChainActorReferences", 2, provider.AdditionalSupplyChainActors.Count);
	}

	public void TestWarehouseIdentifier()
	{
		var warehouse = Factory.NewWithValidTestData<OrgHeader>();

		declaration.WarehouseDocAddress.OrganisationPK = warehouse.PK;

		var cusPermitHeader = Factory.New<CusAuthorisationHeader>();
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
		cusPermitHeader.CPH_OH_PermitHolder = warehouse.PK;
		cusPermitHeader.CPH_Number = "WHS1";
		cusPermitHeader.CPH_StartDate = ZDate.Today;

		var provider = new GoodsShipmentProvider(entryHeader);
		AssertNotNull("WHS1", provider.WarehouseIdentifier);
	}

	public void TestWarehouseType()
	{
		var warehouse = Factory.NewWithValidTestData<OrgHeader>();

		declaration.WarehouseDocAddress.OrganisationPK = warehouse.PK;

		var cusPermitHeader = Factory.New<CusAuthorisationHeader>();
		cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
		cusPermitHeader.CPH_OH_PermitHolder = warehouse.PK;
		cusPermitHeader.CPH_Number = "WHS1";
		cusPermitHeader.CPH_StartDate = ZDate.Today;

		var provider = new GoodsShipmentProvider(entryHeader);

		CombineAssertions(() =>
		{
			cusPermitHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.TemporaryStorage;
			AssertNotNull("V", provider.WarehouseType);
			cusPermitHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			AssertNotNull("R", provider.WarehouseType);
			cusPermitHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
			AssertNotNull("S", provider.WarehouseType);
			cusPermitHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			AssertNotNull("U", provider.WarehouseType);
		});
	}

	public void TestPreviousDocuments()
	{
		entryInstruction.PreviousDocuments.AddNew();
		AssertEquals("1 document", 1, provider.PreviousDocuments.Count);
	}

	public void TestSupportingDocuments()
	{
		entryInstruction.SupportingDocuments.AddNew();
		AssertEquals("1 document", 1, provider.SupportingDocuments.Count);
	}

	public void TestAdditionalReferences()
	{
		var additionalReference = entryInstruction.AdditionalInfos.AddNew();
		additionalReference.CSI_SubType = BEAdditionalDocTypeList.Codes.AdditionalReference;
		AssertEquals("1 reference", 1, provider.AdditionalReferences.Count);
	}

	public void TestAdditionalInformations()
	{
		var additionalInformation = entryInstruction.AdditionalInfos.AddNew();
		additionalInformation.CSI_SubType = BEAdditionalDocTypeList.Codes.AdditionalInformation;
		AssertEquals("1 additional info", 1, provider.AdditionalInformations.Count);
	}

	public void TestConsignment() => AssertNotNull(provider.Consignment);

	public void TestGoodsItems()
	{
		AssertEquals(2, provider.GoodsItems.Count);
	}

	public void TestDeliveryTerms()
	{
		AssertNotNull(provider.DeliveryTerms);
	}

	public void TestTotalAmountInvoiced()
	{
		invoice.JZ_InvoiceAmount = 1;
		invoice2.JZ_InvoiceAmount = 2;
		AssertEquals(3m, provider.TotalAmountInvoiced);
	}

	public void TestInvoiceCurrency()
	{
		invoice.JZ_RX_NKInvoice_Currency = "GER";
		AssertEquals("GER", provider.InvoiceCurrency);
	}

	public void TestExchangeRate()
	{
		invoice.JZ_InvoiceCurrExRate = 1.39m;
		AssertEquals(1.39m, provider.ExchangeRate);
	}

	public void TestCountryOfDispatch()
	{
		declaration.JE_GoodsOrigin = "BE";
		AssertEquals("BE", provider.CountryOfDispatch);
	}

	public void TestBuyer()
	{
		var buyer = Factory.New<OrgHeader>();
		invoice.JZ_OA_BuyerAddress = buyer.MainAddress.PK;
		AssertNotNull(provider.Buyer);
	}

	public void TestSeller()
	{
		var seller = Factory.New<OrgHeader>();
		invoice.JZ_OA_SellerAddress = seller.MainAddress.PK;
		AssertNotNull(provider.Seller);
	}

	public void TestExporter()
	{
		var exporter = Factory.New<OrgHeader>();
		invoice.JZ_OA_ExporterAddress = exporter.MainAddress.PK;
		AssertNotNull(provider.Exporter);
	}

	public void TestDestination()
	{
		declaration.JE_GoodsDestination = "CX";
		declaration.ZG_RegionOfDestination = "N";

		CombineAssertions(() =>
		{
			AssertEquals("CX", provider.Destination.CountryOfDestination);
			AssertEquals("N", provider.Destination.RegionOfDestination);
		});
	}

	public void TestAdditionsAndDeductions()
	{
		AssertType<List<IAdditionsAndDeductions>>(provider.AdditionsAndDeductions);
	}

	public void TestAdditionalFiscalReferences()
	{
		AssertType<List<IAdditionalFiscalReference>>(provider.AdditionalFiscalReferences);
	}

	public void TestAcceptanceDate()
	{
		AssertEquals(default(DateTime), provider.AcceptanceDate);
	}

	protected override GoodsShipmentProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_Description = "invoice line 1";
		invoice2 = declaration.Invoices.AddNew();
		invoiceLine2 = invoice2.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = entryInstruction.PK;

		var lineMerger = new LineMerger(declaration);
		lineMerger.DoMerge();
		entryHeader = declaration.CustomsEntryHeaders.FirstOrDefault();

		provider = new GoodsShipmentProvider(entryHeader);
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	JobComInvoiceLine invoiceLine;
	JobComInvoiceLine invoiceLine2;
	JobComInvoiceHeader invoice;
	JobComInvoiceHeader invoice2;
	CusEntryInstruction entryInstruction;
	GoodsShipmentProvider provider;
}
