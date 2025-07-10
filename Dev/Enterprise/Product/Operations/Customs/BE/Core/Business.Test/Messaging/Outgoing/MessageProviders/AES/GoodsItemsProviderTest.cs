using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class GoodsItemsProviderTest : Customs.Business.Testing.DataProviderTestCase<GoodsItemsProvider>
{
	public void TestAdditionalInformations()
	{
		var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
		additionalInfo.CSI_SubType = BEAdditionalDocTypeList.Codes.AdditionalInformation;
		AssertEquals(1, provider.AdditionalInformations.Count);
	}

	public void TestAdditionalReferences()
	{
		var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
		additionalInfo.CSI_SubType = BEAdditionalDocTypeList.Codes.AdditionalReference;
		AssertEquals(1, provider.AdditionalReferences.Count);
	}

	public void TestAdditionalSupplyChainActors()
	{
		invoiceLine.CusSupplyChainActorReferences.AddNew();
		AssertEquals(1, provider.AdditionalSupplyChainActors.Count);
	}

	public void TestAuthorisations()
	{
		invoiceLine.CusAuthorizationUsages.AddNew();
		AssertEquals(1, provider.Authorisations.Count);
	}

	public void TestCommodity()
	{
		AssertNotNull(provider.Commodity);
	}

	public void TestConsignee()
	{
		var consigneeAddress = Factory.New<OrgAddress>();
		invoiceLine.JI_OA_ConsigneeAddress = consigneeAddress.PK;
		AssertNotNull(provider.Consignee);
	}

	public void TestConsignor()
	{
		var exporterAddress = Factory.New<OrgAddress>();
		invoiceLine.JI_OA_ExporterAddress = exporterAddress.PK;
		AssertNotNull(provider.Consignor);
	}

	public void TestCountryOfDestination()
	{
		invoiceLine.ZG_CountryOfDestination = "BE";
		AssertEquals("BE", provider.CountryOfDestination);
	}

	public void TestCountryOfExport()
	{
		invoiceLine.JI_RN_NKCountryOfExport = "BE";
		AssertEquals("BE", provider.CountryOfExport);
	}

	public void TestDeclarationGoodsItemNumber()
	{
		invoiceLine.CusEntryLine.CL_LineNumber = 1423;
		AssertEquals("1423", provider.DeclarationGoodsItemNumber);
	}

	public void TestNatureOfTransaction()
	{
		invoiceLine.InvoiceHeader.JZ_ValuationCode = "VA";

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
		{
			AssertEquals("VA", provider.NatureOfTransaction);
		}

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
		{
			AssertEquals(string.Empty, provider.NatureOfTransaction);
		}
	}

	public void TestOrigin()
	{
		AssertNotNull(provider.Origin);
	}

	public void TestPackagings()
	{
		invoiceLine.PackagesPivot.AddNew();
		AssertEquals(1, provider.Packagings.Count);
	}

	public void TestPreviousDocuments()
	{
		invoiceLine.PreviousDocuments.AddNew();
		AssertEquals(1, provider.PreviousDocuments.Count);
	}

	public void TestProcedure()
	{
		AssertNotNull(provider.Procedure);
	}

	public void TestReferenceNumberUCR()
	{
		invoiceLine.InvoiceHeader.JZ_UCR = "UCR";
		AssertEquals("UCR", provider.ReferenceNumberUCR);
	}

	public void TestStatisticalValue()
	{
		AssertEquals(invoiceLine.JI_Calc_StatisticalValue, provider.StatisticalValue);
	}

	public void TestSupportingDocuments()
	{
		invoiceLine.SupportingDocuments.AddNew();
		AssertEquals(1, provider.SupportingDocuments.Count);
	}

	public void TestTransportCharges()
	{
		AssertNotNull(provider.TransportCharges);
	}

	public void TestTransportDocuments()
	{
		var transportDocument = Factory.New<CusSupportingInfo>();
		transportDocument.CSI_ParentID = invoiceLine.PK;
		transportDocument.CSI_Type = "OTH";
		transportDocument.CSI_ParentTableCode = "JI";
		transportDocument.CSI_SubType = "TRA";

		Factory.Save();

		provider = new GoodsItemsProvider(invoiceLine);

		AssertEquals(1, provider.TransportDocuments.Count);
	}

	public void TestAcceptanceDate()
	{
		AssertNull(provider.AcceptanceDate);
	}

	public void TestAdditionalFiscalReferences()
	{
		var orgHeader = Factory.New<OrgHeader>();
		_ = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;

		var reference1 = invoiceLine.FiscalReferences.AddNew();
		reference1.CFR_Code = "CD1";
		reference1.CFR_OA_Owner = orgAddress.PK;

		var reference2 = invoiceLine.FiscalReferences.AddNew();
		reference2.CFR_Code = "CD2";

		CombineAssertions(() =>
		{
			AssertType<IAdditionalFiscalReference[]>("Type", Provider.AdditionalFiscalReferences);
			AssertEquals("AdditionalFiscalReferences", 2, Provider.AdditionalFiscalReferences.Count);
			AssertContainsExactElementsInExactOrder("Additional Fiscal Reference Elements", new string[] { "1|CD1|123", "2|CD2|" }, Provider.AdditionalFiscalReferences.Select(x => $"{x.SequenceNumber}|{x.Role}|{x.VatIdentificationNumber}"));
		});
	}

	public void TestBuyer()
	{
		var buyer = Factory.NewWithValidTestData<OrgHeader>();
		invoiceLine.BuyerDocAddress.OrganisationPK = buyer.PK;
		AssertNotNull(provider.Buyer);
	}

	public void TestCountryOfDispatch()
	{
		invoiceLine.ZG_CountryOfDispatch = "BE";
		AssertEquals("BE", Provider.CountryOfDispatch);
	}
	public void TestCustomsValuation()
	{
		AssertType<CustomsValuationProvider>(Provider.CustomsValuation);
	}

	public void TestDestination()
	{
		invoiceLine.ZG_CountryOfDestination = "FR";
		invoiceLine.ZG_RegionOfDestination = "7";
		var destination = Provider.Destination;
		AssertEquals("CountryOfDestination", "FR", destination.CountryOfDestination);
		AssertEquals("RegionOfDestination", "7", destination.RegionOfDestination);
		AssertSame("Cached", destination, Provider.Destination);
	}

	public void TestExporter()
	{
		var exporter = Factory.NewWithValidTestData<OrgHeader>();
		invoiceLine.JI_OA_ExporterAddress = exporter.MainAddress.PK;
		AssertNotNull(provider.Exporter);
	}

	public void TestIntristicValue()
	{
		AssertNull(provider.IntristicValue);
	}

	public void TestPostalValue()
	{
		AssertNull(provider.PostalValue);
	}

	public void TestSeller()
	{
		var seller = Factory.NewWithValidTestData<OrgHeader>();
		invoiceLine.SellerDocAddress.OrganisationPK = seller.PK;
		AssertNotNull(provider.Buyer);
	}

	public void TestSequenceNumber()
	{
		invoiceLine.CusEntryLine.CL_LineNumber = 1423;
		AssertEquals("1423", provider.SequenceNumber);
	}

	public void TestTransportAndInsuranceCostsToTheDestination()
	{
		AssertNull(provider.TransportAndInsuranceCostsToTheDestination);
	}

	public void TestValuationAdjustment()
	{
		AssertEquals("0000", Provider.ValuationAdjustment);
		invoice.RelatedIndicator = true;
		AssertEquals("1000", new GoodsItemsProvider(invoiceLine).ValuationAdjustment);
		invoice.RelatedIndicator3 = true;
		AssertEquals("1010", new GoodsItemsProvider(invoiceLine).ValuationAdjustment);
		invoice.RelatedIndicator3 = false;
		invoice.RelatedIndicator4 = true;
		AssertEquals("1001", new GoodsItemsProvider(invoiceLine).ValuationAdjustment);

		invoiceLine.JI_RelatedIndicator = ValuationIndicatorCodeList.Codes.No;
		AssertEquals("0001", new GoodsItemsProvider(invoiceLine).ValuationAdjustment);
		invoiceLine.JI_RelatedIndicator = ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader;
		AssertEquals("1001", new GoodsItemsProvider(invoiceLine).ValuationAdjustment);

		invoiceLine.ZG_RelatedIndicator2 = ValuationIndicatorCodeList.Codes.Yes;
		AssertEquals("1101", new GoodsItemsProvider(invoiceLine).ValuationAdjustment);
		invoiceLine.ZG_RelatedIndicator4 = ValuationIndicatorCodeList.Codes.No;
		AssertEquals("1100", new GoodsItemsProvider(invoiceLine).ValuationAdjustment);
		invoiceLine.ZG_RelatedIndicator4 = ValuationIndicatorCodeList.Codes.SameAsInvoiceHeader;
		AssertEquals("1101", new GoodsItemsProvider(invoiceLine).ValuationAdjustment);
	}

	protected override GoodsItemsProvider GetProvider() => provider;
	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		invoice = declaration.Invoices.AddNew();

		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;

		var lineMerger = new EU.Business.Declaration.LineMerger(declaration);
		lineMerger.DoMerge();

		provider = new GoodsItemsProvider(invoiceLine);
	}

	JobComInvoiceLine invoiceLine;
	JobComInvoiceHeader invoice;
	GoodsItemsProvider provider;
}
