using CargoWise.Types;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class JustificationDataProviderTest : BasePassarDataProviderTest<JustificationDataProvider>
{
	public void TestNew() => CombineAssertions(() =>
	{
		AssertNull(JustificationDataProvider.New(null));
		SendingObject.VOCReason = ZString.Empty;
		SendingObject.ReasonText = "reason";
		AssertNull(JustificationDataProvider.New(SendingObject));
	});

	public void TestReasonText()
	{
		SendingObject.VOCReason = UniversalReferenceConstants.PassarReasonCodes.Others;
		SendingObject.ReasonText = "reason";
		AssertEquals("Text", "reason", DataProvider.Text);
	}

	public void TestReasonCode()
	{
		SendingObject.VOCReason = UniversalReferenceConstants.PassarReasonCodes.Duplication;
		AssertEquals("Code", UniversalReferenceConstants.PassarReasonCodes.Duplication, DataProvider.Code);
	}

	protected override JustificationDataProvider CreateDataProvider() => JustificationDataProvider.New(SendingObject);
}
