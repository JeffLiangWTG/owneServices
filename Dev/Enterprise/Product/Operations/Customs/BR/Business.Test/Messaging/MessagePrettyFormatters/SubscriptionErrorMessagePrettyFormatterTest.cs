using System.Text.Json;
using CargoWise.Customs.BR.MessageDefinitions.Subscription.Incoming;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	class SubscriptionErrorMessagePrettyFormatterTest : TestCaseWithFactory
	{
		public void TestGetFormattedMessageText()
		{
			var error = JsonSerializer.Deserialize<ErrorNotification>(JsonError);
			var actualHtml = new SubscriptionErrorMessagePrettyFormatter(error).GetFormattedMessageText();

			AssertMultilineASCIIEquals("EM_MessageInterpretation must be equal to HTML",
				@"<H3>Customs Error</H3>" +
				"<table border=\"1\" cellpadding=\"2\" cellspacing=\"0\" class=\"table\" style=\"white-space:pre\" width=\"100%\">" +
				"<tr><td><strong>Info</strong></td><td><strong>Description</strong></td></tr>" +
				@"<tr><td>code</td><td>PLAT-ER9100</td></tr>" +
				@"<tr><td>message</td><td>TEST SUB</td></tr>" +
				@"<tr><td>ambiente</td><td>TRE</td></tr>" +
				@"<tr><td>usuario</td><td>00302993738</td></tr>" +
				"</table>",
				actualHtml);
		}

		public const string JsonError = @"{
	""message"": ""TEST SUB"",
	""code"": ""PLAT-ER9100"",
	""field"": ""evento"",
	""path"": null,
	""tag"": ""[PLAT-IQAIGS2TKK]"",
	""date"": ""2023-11-28T10:08:37"",
	""detail"": [],
	""severity"": ""ERROR"",
	""info"": {
		""mnemonico"": ""PLAT"",
		""sistema"": ""Plataforma [Módulo Notificação - Serviços de Notificação de Usuários e Webhooks]"",
		""ambiente"": ""TRE"",
		""visao"": ""PRIV"",
		""usuario"": ""00302993738"",
		""url"": ""/plat-notificacao/api/ext/webhook"",
		""fluxo"": null,
		""trackerId"": ""N7rsM1F0bM""
	}
}";
	}
}
