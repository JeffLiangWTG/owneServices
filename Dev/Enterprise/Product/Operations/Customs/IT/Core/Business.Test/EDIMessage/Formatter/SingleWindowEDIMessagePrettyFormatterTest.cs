using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SingleWindowEDIMessagePrettyFormatterTest : TestCaseWithFactory
{
	public void TestPrettifyMessage()
	{
		var formatter = new SingleWindowEDIMessagePrettyFormatter(Factory);

		var originalText =
			"<note>" +
				"<to>Tove</to>" +
				"<from>Jani</from>" +
				"<heading>Reminder</heading>" +
				"<body>Don't forget me this weekend!</body>" +
			"</note>";

		var expectedFormattedText = "<?xml version=\"1.0\"?>" + originalText;
		AssertEquals("Prettified text", expectedFormattedText, formatter.GetFormattedText(originalText));
	}
}
