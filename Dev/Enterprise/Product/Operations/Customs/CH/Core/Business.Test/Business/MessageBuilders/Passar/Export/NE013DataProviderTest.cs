using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NE013DataProvider))]
sealed class NE013DataProviderTest : BasePassarExportDeclarationDataProviderTest<NE013DataProvider>
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

	protected override NE013DataProvider CreateDataProvider() => new NE013DataProvider(SendingObject);
}
