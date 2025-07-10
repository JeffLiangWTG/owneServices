using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.Types;
using Enterprise.Customs.Business;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

class ConsignmentItemDataProviderTest : BasePassarDataProviderTest<ConsignmentItemDataProvider>
{
	public void TestConstructorNullArgument()
	{
		AssertNull(ConsignmentItemDataProvider.New(null));
	}

	public void TestProvider()
	{
		EntryLine.CL_LineNumber = 1;
		AssertEquals("GoodsItemNumber", 1, DataProvider.GoodsItemNumber);
		AssertNull("ReferenceNumberUCR", DataProvider.ReferenceNumberUCR);
	}

	public void TestCommodity()
	{
		EntryLine.CL_Description = "DESC";
		AssertType<CommodityDataProvider>(DataProvider.Commodity);
		AssertSame("cached", DataProvider.Commodity, DataProvider.Commodity);
		AssertEquals("DESC", DataProvider.Commodity.DescriptionOfGoods);
	}

	public void TestUnusedProperties()
	{
		AssertNull("CountryOfDispatch", DataProvider.CountryOfDispatch);
		AssertNull("CountryOfDestination", DataProvider.CountryOfDestination);
		AssertNull("DeclarationType", DataProvider.DeclarationType);
		AssertNull("AdditionalReference", DataProvider.AdditionalReferences);
		AssertNull("AdditionalInformation", DataProvider.AdditionalInformations);
		AssertNull("AdditionalSupplyChainActors", DataProvider.AdditionalSupplyChainActors);
		AssertNull("UnloadingRemarkCode", DataProvider.UnloadingRemarkCode);
		AssertNull("UnloadingRemarkText", DataProvider.UnloadingRemarkText);
	}

	public void TestSupportingDocuments() => CombineAssertions(() =>
	{
		var invoiceLine1 = InvoiceLine;
		var invoiceLine2 = CreateInvoiceLine(InvoiceHeader);
		AddDocument(invoiceLine1.SupportingDocuments, "1", "01");
		AddDocument(invoiceLine1.SupportingDocuments, "2", "02");
		AddDocument(invoiceLine2.SupportingDocuments, "1", "01");
		AddDocument(invoiceLine2.SupportingDocuments, "3", "03");
		AddDocument(invoiceLine2.SupportingDocuments, "4", "01");
		AddDocument(invoiceLine2.SupportingDocuments, "1", "05");

		AddDocument(InvoiceHeader.SupportingDocuments, "9", "IH");
		AddDocument(EntryInstruction.SupportingDocuments, "9", "EI");

		Declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
		Declaration.DoMergeForTesting();

		AssertNotNull(DataProvider.SupportingDocuments);
		AssertContainsExactElementsInAnyOrder(new[] { "101", "202", "303", "401", "105" }, DataProvider.SupportingDocuments.Select(x => x.Type + x.ReferenceNumber));
		AssertSame("cached", DataProvider.SupportingDocuments, DataProvider.SupportingDocuments);
	});

	public void TestPreviousDocuments() => CombineAssertions(() =>
	{
		var invoiceLine1 = InvoiceLine;
		var invoiceLine2 = CreateInvoiceLine(InvoiceHeader);
		AddDocument(invoiceLine1.PreviousDocuments, "1", "01");
		AddDocument(invoiceLine1.PreviousDocuments, "2", "02");
		AddDocument(invoiceLine2.PreviousDocuments, "1", "01");
		AddDocument(invoiceLine2.PreviousDocuments, "3", "03");
		AddDocument(invoiceLine2.PreviousDocuments, "4", "01");
		AddDocument(invoiceLine2.PreviousDocuments, "1", "05");

		AddDocument(InvoiceHeader.PreviousDocuments, "9", "IH");
		AddDocument(EntryInstruction.PreviousDocuments, "9", "EI");

		Declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
		Declaration.DoMergeForTesting();

		AssertNotNull(DataProvider.PreviousDocuments);
		AssertContainsExactElementsInAnyOrder(new[] { "101", "202", "303", "401", "105" }, DataProvider.PreviousDocuments.Select(x => x.Type + x.ReferenceNumber));
		AssertSame("cached", DataProvider.PreviousDocuments, DataProvider.PreviousDocuments);
	});

	public void TestEmptyRefinement() => AssertNull(DataProvider.Refinement);

	public void TestRefinement()
	{
		InvoiceLine.InAndOutwardProcessingRefinementType = InAndOutwardProcessingRefinementTypesEdec.CommercialProcessing;

		AssertType<ConsignmentItemRefinementDataProvider>(DataProvider.Refinement);
		AssertSame("cached", DataProvider.Refinement, DataProvider.Refinement);
	}

	public void TestPackagings()
	{
		var invoice = Declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		EntryLine.InvoiceLines.Add(invoiceLine);

		var packaging = Declaration.Packages.AddNew();
		packaging.CW_PackType = "ABC";
		packaging.CW_MarksAndNos = "MARK AND NOS 1";

		var packageInvoiceLine = invoiceLine.PackagesPivot.AddNew();
		packageInvoiceLine.CHC_CW = packaging.PK;
		packageInvoiceLine.CHC_NumberOfPacks = 5;

		CombineAssertions(() =>
		{
			AssertType<ConsignmentItemPackagingDataProvider>(DataProvider.Packagings.First());
			AssertEquals("Count", 1, DataProvider.Packagings.Count);
			AssertSame("cached", DataProvider.Packagings, DataProvider.Packagings);
		});
	}

	public void TestRefund() => CombineAssertions(() =>
	{
		InvoiceLine.JI_RefundType = RefundType.ReturnedGoodsWithRefundRequest;
		InvoiceLine.JI_RefundReferenceNumber = "24CH12345678901238";
		InvoiceLine.JI_RefundGoodsItemNumber = 1;
		InvoiceLine.JI_RefundReason = "Reason Text";

		AssertNotNull("instance", DataProvider.Refund);
		AssertType<RefundDataProvider>(DataProvider.Refund);
		AssertSame("cached", DataProvider.Refund, DataProvider.Refund);

		ResetDataProvider();
		InvoiceLine.JI_RefundType = RefundType.Refund;
		AssertNull(DataProvider.Refund);

		ResetDataProvider();
		InvoiceLine.JI_RefundType = RefundType.ReturnedGoodsWithRefundRequest;
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertNull(DataProvider.Refund);
	});

	public void TestReferenceNumberUCR()
	{
		var invoice1 = InvoiceHeader;
		invoice1.InvoiceLines[0].JI_Tariff = "1";
		var invoice2 = CreateInvoiceHeader();
		invoice2.InvoiceLines[0].JI_Tariff = "2";

		Declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
		Declaration.DoMergeForTesting();
		AssertEquals("Pre-check: # of entry lines", 2, EntryHeader.AllEntryLines.Count);

		AssertReferenceNumberUCR(null, ZString.Empty, ZString.Empty, ZString.Empty);
		AssertReferenceNumberUCR(null, "UCR1", ZString.Empty, ZString.Empty);
		AssertReferenceNumberUCR("UCR1", "UCR1", ZString.Empty, "UCR3");
		AssertReferenceNumberUCR("UCR2", "UCR1", "UCR2", ZString.Empty);
		AssertReferenceNumberUCR("UCR2", ZString.Empty, "UCR2", ZString.Empty);
		AssertReferenceNumberUCR(null, ZString.Empty, ZString.Empty, "UCR3");

		invoice1.JZ_UCR = "UCR1";
		AssertEquals("Cached (not recalculated)", null, DataProvider.ReferenceNumberUCR);

		void AssertReferenceNumberUCR(string expectedUCR, ZString declarationUCR, ZString invoice1UCR, ZString invoice2UCR, [CallerLineNumber] int callerLineNumber = 0)
		{
			Declaration.JE_UCR = declarationUCR;
			invoice1.JZ_UCR = invoice1UCR;
			invoice2.JZ_UCR = invoice2UCR;
			ResetDataProvider();
			AssertEquals($"[{callerLineNumber}] DeclarationUCR={declarationUCR} Invoice1UCR={invoice1UCR} Invoice2UCR={invoice2UCR}", expectedUCR, DataProvider.ReferenceNumberUCR);
		}
	}

	protected override ConsignmentItemDataProvider CreateDataProvider() => ConsignmentItemDataProvider.New(EntryLine);
}
