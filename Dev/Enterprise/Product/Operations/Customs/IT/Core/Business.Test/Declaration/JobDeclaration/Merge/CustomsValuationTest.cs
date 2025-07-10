using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CustomsValuationTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("entryHeader is required", () => new CustomsValuation(entryHeader: null));
		AssertNoExceptionThrown(() => new CustomsValuation(entryHeader: Factory.New<CusEntryHeader>()));
	}

	public void TestFreightAdjustment()
	{
		invoiceLine1.JI_Tariff = "400";
		invoiceLine1.Charges.AddNewWithTestValues("OFT", amount: 1000.14m, isStatisticalValueApplicable: true, isIncludedInLines: false);
		invoiceLine1.Charges.AddNewWithTestValues("ADD", 111m, isStatisticalValueApplicable: true, isIncludedInLines: false);
		invoiceLine1.Charges.AddNewWithTestValues("OFT", 222m, isStatisticalValueApplicable: true, isIncludedInLines: true);

		invoiceLine2.JI_Tariff = "500";
		invoiceLine2.Charges.AddNewWithTestValues("OFT", 555.12m, isStatisticalValueApplicable: false, isIncludedInLines: true);
		invoiceLine2.Charges.AddNewWithTestValues("ADD", 333m, isStatisticalValueApplicable: false, isIncludedInLines: true);

		declaration.DoMerge();

		AssertEquals("Entry Headers count", 1, declaration.CustomsEntryHeaders.Count);
		var entryHeader = declaration.CustomsEntryHeaders[0];
		AssertEquals("Entry Lines count", 2, entryHeader.MergedLines.Count);
		AssertEquals("Freight Adjustment", 445.02m, entryHeader.CH_FreightAdjustment);
	}

	public void TestInvoiceAmount()
	{
		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invoiceLine1.JI_Tariff = "400";
		invoiceLine1.JI_LinePrice = 10m;

		invoice2.JZ_RX_NKInvoice_Currency = "EUR";
		invoiceLine2.JI_Tariff = "500";
		invoiceLine2.JI_LinePrice = 100m;

		declaration.DoMerge();

		AssertEquals("Entry Headers count", 1, declaration.CustomsEntryHeaders.Count);
		var entryHeader = declaration.CustomsEntryHeaders[0];

		AssertEquals("Entry Lines count", 2, entryHeader.MergedLines.Count);
		var entryLine400 = entryHeader.MergedLines.Cast<CusEntryLine>().Single(x => x.Tariff == "400");
		var entryLine500 = entryHeader.MergedLines.Cast<CusEntryLine>().Single(x => x.Tariff == "500");

		CombineAssertions(() =>
		{
			AssertEquals("Entry Line 400 Lines Value", 10m, entryLine400.ZG_LinesValue);
			AssertEquals("Entry Line 500 Lines Value", 100m, entryLine500.ZG_LinesValue);
			AssertEquals("Entry Header Invoice Amount", 10m + 100m, entryHeader.InvoiceAmount);
		});
	}

	public void TestAdjustmentAmount()
	{
		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invoice2.JZ_RX_NKInvoice_Currency = "EUR";
		invoiceLine1.JI_LinePrice = 1m;

		invoiceLine1.Charges.AddNewWithTestValues("OFT", amount: 10m, isStatisticalValueApplicable: true, isIncludedInLines: false);
		invoiceLine1.Charges.AddNewWithTestValues("ADD", amount: 25m, isStatisticalValueApplicable: true, isIncludedInLines: false);
		invoiceLine1.Charges.AddNewWithTestValues("OFT", 0.1m, isStatisticalValueApplicable: false, isIncludedInLines: true);

		declaration.DoMerge();

		AssertEquals("Entry Headers count", 1, declaration.CustomsEntryHeaders.Count);
		var entryHeader = declaration.CustomsEntryHeaders[0];

		AssertEquals("Entry Lines count", 1, entryHeader.MergedLines.Count);
		var entryLine = entryHeader.MergedLines[0];

		CombineAssertions(() =>
		{
			AssertEquals("Statistical Value", 35.9m, entryLine.CL_StatisticalValue);
			AssertEquals("Lines Value", 1m, entryLine.ZG_LinesValue);
			AssertEquals("AdjustmentAmount", 25m, entryLine.ZG_AdjustmentAmount);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		invoice1 = declaration.Invoices.AddNew();
		invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoice2 = declaration.Invoices.AddNew();
		invoiceLine2 = invoice2.InvoiceLines.AddNew();
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoice1;
	JobComInvoiceHeader invoice2;
	JobComInvoiceLine invoiceLine1;
	JobComInvoiceLine invoiceLine2;
}
