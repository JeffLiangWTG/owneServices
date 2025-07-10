using CargoWise.Types;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class JustificationDataProviderTest : BaseDepartureDataProviderTest<JustificationDataProvider, NctsHeaderDepartureMessageSendingObject>
{
	public void TestNew()
	{
		AssertNull(JustificationDataProvider.New(null));
		MessageSendingObject.ReasonCode = ZString.Empty;
		MessageSendingObject.ReasonText = "reason";
		AssertNull(JustificationDataProvider.New(MessageSendingObject));
	}

	public void TestProvider() => CombineAssertions(() =>
	{
		MessageSendingObject.ReasonCode = "12";
		MessageSendingObject.ReasonText = "reason";

		AssertEquals("Code", "12", DataProvider.Code);
		AssertEquals("Text", "reason", DataProvider.Text);
	});

	protected override JustificationDataProvider CreateDataProvider() => JustificationDataProvider.New(MessageSendingObject);
}
