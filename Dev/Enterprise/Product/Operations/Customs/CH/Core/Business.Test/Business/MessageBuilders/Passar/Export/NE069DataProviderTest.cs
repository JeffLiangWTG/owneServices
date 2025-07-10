using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NE069DataProvider))]
sealed class NE069DataProviderTest : BasePassarExportDeclarationDataProviderTest<NE069DataProvider>
{
	public void TestJustification() => CombineAssertions(() =>
	{
		SendingObject.VOCReason = UniversalReferenceConstants.PassarReasonCodes.Others;
		SendingObject.ReasonText = "reason text";

		AssertNotNull(DataProvider.Justification);
		AssertSame("cached", DataProvider.Justification, DataProvider.Justification);

		AssertEquals("reason code", UniversalReferenceConstants.PassarReasonCodes.Others, DataProvider.Justification.Code);
		AssertEquals("reason text", "reason text", DataProvider.Justification.Text);
	});

	protected override NE069DataProvider CreateDataProvider() => new NE069DataProvider(SendingObject);
}
