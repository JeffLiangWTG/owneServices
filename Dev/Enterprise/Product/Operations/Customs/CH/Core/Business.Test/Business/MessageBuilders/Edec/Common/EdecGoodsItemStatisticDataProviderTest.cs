
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business.Testing;

public class EdecGoodsItemStatisticDataProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertNull("Null", EdecGoodsItemStatisticDataProvider.New(null));
			AssertNotNull("Not Null", EdecGoodsItemStatisticDataProvider.New(entryLine));
		});
	}

	public void TestStatisticalValue()
	{
		var messageBuilder = EdecGoodsItemStatisticDataProvider.New(entryLine);
		AssertEquals(nameof(messageBuilder.StatisticalValue), 0m, messageBuilder.StatisticalValue);

		entryLine.CL_StatisticalValue = 1000.6m;
		AssertEquals(nameof(messageBuilder.StatisticalValue), 1000m, messageBuilder.StatisticalValue);
	}

	public void TestStatisticalValueConfirmation()
	{
		var messageBuilder = EdecGoodsItemStatisticDataProvider.New(entryLine);
		AssertEquals(nameof(messageBuilder.StatisticalValueConfirmation), false, messageBuilder.StatisticalValueConfirmation);

		invoiceLine.JI_StatisticalValueConfirmation = true;
		AssertEquals(nameof(messageBuilder.StatisticalValueConfirmation), true, messageBuilder.StatisticalValueConfirmation);
	}

	public void TestRepair()
	{
		CombineAssertions(() =>
		{
			var messageBuilder = EdecGoodsItemStatisticDataProvider.New(entryLine);
			AssertEquals($"{nameof(invoiceLine.InAndOutwardProcessingRepair)}={invoiceLine.InAndOutwardProcessingRepair}", false, messageBuilder.Repair);

			invoiceLine.InAndOutwardProcessingRepair = true;
			messageBuilder = EdecGoodsItemStatisticDataProvider.New(entryLine);
			AssertEquals($"{nameof(invoiceLine.InAndOutwardProcessingRepair)}={invoiceLine.InAndOutwardProcessingRepair}", true, messageBuilder.Repair);
		});
	}

	public void TestCommercialGood()
	{
		var messageBuilder = EdecGoodsItemStatisticDataProvider.New(entryLine);
		AssertEquals(nameof(messageBuilder.CommercialGood), "1", messageBuilder.CommercialGood);

		invoiceLine.JI_NonTradingGoods = true;
		messageBuilder = EdecGoodsItemStatisticDataProvider.New(entryLine);
		AssertEquals(nameof(messageBuilder.CommercialGood), "2", messageBuilder.CommercialGood);
	}

	public void TestCustomsClearanceType()
	{
		var messageBuilder = EdecGoodsItemStatisticDataProvider.New(entryLine);
		AssertEquals("Default value should be: ", "0", messageBuilder.CustomsClearanceType);

		invoiceLine.JI_Procedure = "01";
		messageBuilder = EdecGoodsItemStatisticDataProvider.New(entryLine);
		AssertEquals(nameof(messageBuilder.CustomsClearanceType), "01", messageBuilder.CustomsClearanceType);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.AllEntryLines.AddNew();
		entryLine.InvoiceLines.Add(invoiceLine);
	}

	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;
}
