using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NE014DataProvider))]
sealed class NE014DataProviderTest : BasePassarMessageDataProviderTest<NE014DataProvider>
{
	public void TestExportOperation() => CombineAssertions(() =>
	{
		AssertNotNull(DataProvider.ExportOperation);
		AssertSame("cached", DataProvider.ExportOperation, DataProvider.ExportOperation);
	});

	public void TestJustification()
	{
		SendingObject.VOCReason = CH.Business.UniversalReferenceConstants.PassarReasonCodes.Others;
		AssertNotNull(DataProvider.Justification);
		AssertSame("cached", DataProvider.Justification, DataProvider.Justification);
	}

	protected override NE014DataProvider CreateDataProvider() => new NE014DataProvider(SendingObject);
}
