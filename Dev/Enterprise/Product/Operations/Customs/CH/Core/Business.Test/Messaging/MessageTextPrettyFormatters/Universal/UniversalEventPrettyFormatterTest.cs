using CargoWise.EntityFramework.Testing;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.CH.Business.Testing;

class UniversalEventPrettyFormatterTest : TestCaseWithFactory
{
	public void TestGetFormattedText() => CombineAssertions(() =>
	{
		const string reason = @"{""fault"":""error""}";

		var universalEventData = new UniversalEventWrapper(UniversalEventTestDataHelper.CreateUniversalEventXml(messageType: "", responseType: "", reason: reason, responseMessage: ""));

		var formatter = new UniversalEventPrettyFormatter(universalEventData);
		var formattedText = formatter.GetFormattedText();
		Assert("Title", formattedText.Contains("<h2>Send Failure</h2>"));
		Assert("Reason", formattedText.Contains($"<p>{reason.Replace(@"""", "&quot;")}</p>"));
	});

	public void TestGetFormattedText_NoData()
	{
		var universalEventData = new UniversalEventWrapper(UniversalEventTestDataHelper.CreateUniversalEventXml(messageType: "", responseType: "", reason: "", responseMessage: ""));

		var formatter = new UniversalEventPrettyFormatter(universalEventData);
		var formattedText = formatter.GetFormattedText();
		Assert("Empty <reason>", formattedText.Contains("<h2>Message is empty</h2>"));
	}
}
