using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

internal class EdecRefinementDataProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Argument == null", () => new EdecRefinementDataProvider(null));
			AssertNotNull("Not Null", EdecGoodsItemOriginDataProvider.New(entryLine));
		});
	}

	public void TestDirection()
	{
		CombineAssertions(() =>
		{
			invoiceLine.InAndOutwardProcessingDirection = "2";
			var messageBuilder = EdecRefinementDataProvider.New(entryLine);
			AssertEquals($"{nameof(invoiceLine.InAndOutwardProcessingDirection)}={invoiceLine.InAndOutwardProcessingDirection}", "2", messageBuilder.Direction);
		});
	}

	public void TestRefinementType()
	{
		CombineAssertions(() =>
		{
			invoiceLine.InAndOutwardProcessingRefinementType = "3";
			var messageBuilder = EdecRefinementDataProvider.New(entryLine);
			AssertEquals($"{nameof(invoiceLine.InAndOutwardProcessingRefinementType)}={invoiceLine.InAndOutwardProcessingRefinementType}", "3", messageBuilder.RefinementType);
		});
	}

	public void TestProcessType()
	{
		CombineAssertions(() =>
		{
			invoiceLine.InAndOutwardProcessingProcessType = "4";
			var messageBuilder = EdecRefinementDataProvider.New(entryLine);
			AssertEquals($"{nameof(invoiceLine.InAndOutwardProcessingProcessType)}={invoiceLine.InAndOutwardProcessingProcessType}", "4", messageBuilder.ProcessType);
		});
	}

	public void TestBillingType()
	{
		CombineAssertions(() =>
		{
			invoiceLine.InAndOutwardProcessingBillingType = "5";
			var messageBuilder = EdecRefinementDataProvider.New(entryLine);
			AssertEquals($"{nameof(invoiceLine.InAndOutwardProcessingBillingType)}={invoiceLine.InAndOutwardProcessingBillingType}", "5", messageBuilder.BillingType);
		});
	}

	public void TestRepairReason()
	{
		CombineAssertions(() =>
		{
			invoiceLine.InAndOutwardProcessingRepairReason = "6";
			var messageBuilder = EdecRefinementDataProvider.New(entryLine);
			AssertEquals($"{nameof(invoiceLine.InAndOutwardProcessingRepairReason)}={invoiceLine.InAndOutwardProcessingRepairReason}", "6", messageBuilder.RepairReason);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.AllEntryLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine);
	}
	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;
}
