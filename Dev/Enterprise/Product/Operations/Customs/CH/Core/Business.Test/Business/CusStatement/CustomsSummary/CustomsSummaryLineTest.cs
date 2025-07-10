using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CustomsSummaryLine))]
sealed class CustomsSummaryLineTest : EnterpriseBusinessObjectTestCase
{
	public void TestCaptions() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions<CustomsSummaryLine>(nameof(CustomsSummaryLine.B3_EntryNum), caption: "Entry Number");
		CaptionTestHelper.AssertCaptions<CustomsSummaryLine>(nameof(CustomsSummaryLine.B3_Status), caption: "eVV Document Received Status");
		CaptionTestHelper.AssertCaptions<CustomsSummaryLine>(nameof(CustomsSummaryLine.StatusDescription), caption: "eVV Document Received Status Desc.");
		CaptionTestHelper.AssertCaptions<CustomsSummaryLine>(nameof(CustomsSummaryLine.B3_BrokerReference), caption: "Trader Reference");
		CaptionTestHelper.AssertCaptions<CustomsSummaryLine>(nameof(CustomsSummaryLine.ChargeTypeDescription), caption: "eVV Document Type Desc.");
		CaptionTestHelper.AssertCaptions<CustomsSummaryLine>(nameof(CustomsSummaryLine.StatementNumber), caption: "Summary Number");
		CaptionTestHelper.AssertCaptions<CustomsSummaryLine>(nameof(CustomsSummaryLine.ProcessDate), caption: "Summary Date");
		CaptionTestHelper.AssertCaptions<CustomsSummaryLine>(nameof(CustomsSummaryLine.AccountNo), caption: "Account Number");
		CaptionTestHelper.AssertCaptions<CustomsSummaryLine>(nameof(CustomsSummaryLine.ChargeType), caption: "eVV Document Type");
		CaptionTestHelper.AssertCaptions<CustomsSummaryLine>(nameof(CustomsSummaryLine.ChargeAmount), caption: "Amount");
		CaptionTestHelper.AssertCaptions<CustomsSummaryLine>(nameof(CustomsSummaryLine.ReferenceNumber), caption: "Customs Office Number");
	});

	public void TestDocumentTypeDescription() => CombineAssertions(() =>
	{
		AssertDescription("VVZ", "Duties");
		AssertDescription("VVM", "VAT");
		AssertDescription("RBZ", "Duties Refund");
		AssertDescription("RBM", "VAT Refund");

		void AssertDescription(ZString chargeType, ZString expectedDescription)
		{
			SummaryLine.LineCharge.B4_ChargeType = chargeType;
			AssertEquals($"B4_ChargeType={chargeType}", expectedDescription, SummaryLine.ChargeTypeDescription);
		}
	});

	public void TestStatusDescription() => CombineAssertions(() =>
	{
		AssertDescription("SNT", "Sent");
		AssertDescription("RCV", "Received");
		AssertDescription("ERR", "Send Failure");
		AssertDescription("REJ", "Rejected by Customs");
		AssertDescription("SKP", "Skipped");

		void AssertDescription(ZString status, ZString expectedDescription)
		{
			SummaryLine.B3_Status = status;
			AssertEquals($"B3_Status={status}", expectedDescription, SummaryLine.StatusDescription);
		}
	});

	protected override BusinessObject GetNewBusinessObject() => CustomsSummaryTestHelper.CreateSummaryLine(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => CustomsSummaryTestHelper.CreateSummaryLine(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => CustomsSummaryTestHelper.CreateSummaryLine(factory);

	CustomsSummaryLine SummaryLine => summaryLine ??= CustomsSummaryTestHelper.CreateSummaryLine(Factory);
	CustomsSummaryLine summaryLine;
}

[TestedType(typeof(CustomsSummaryLine.Loader))]
sealed class CustomsSummaryLineLoaderTest : LoaderTestCase
{
	protected override BusinessObject.Loader GetNewLoaderToTest() => new CustomsSummaryLine.Loader(Factory);

	public void TestLoadCustomsSummaryLine()
	{
		var loader = new CustomsSummaryLine.Loader(Factory);
		var summaryHeader = Factory.New<CustomsSummaryHeader>();

		_ = AddCustomsSummaryLine("4321", "xyz");
		_ = AddCustomsSummaryLine("1234", "zyx");
		_ = AddCustomsSummaryLine("1111", "zzz");
		var summaryLine = AddCustomsSummaryLine("1234", "xyz");

		Factory.Save();

		AssertEquals(summaryLine, loader.LoadCustomsSummaryLine("1234", "xyz"));

		CustomsSummaryLine AddCustomsSummaryLine(string entryNum, string chargeType)
		{
			var summaryLine = summaryHeader.SummaryLines.AddNew();
			summaryLine.B3_EntryNum = entryNum;
			summaryLine.LineCharge.B4_ChargeType = chargeType;
			return summaryLine;
		}
	}
}
