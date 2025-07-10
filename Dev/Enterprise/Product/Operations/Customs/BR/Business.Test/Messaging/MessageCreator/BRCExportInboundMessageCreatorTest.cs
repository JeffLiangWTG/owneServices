using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCExportInboundMessageCreatorTest : BRCInboundMessageCreatorAbstractTest
	{
		protected override ZString InterchangeType => MessageTypeList.Codes.CDE;

		public void TestGenerateMessageFromInterchange_CDC()
		{
			var jsonMessage = @"
					{
						""message"":""Operação realizada com sucesso"",
						""code"":""PUCX-ER0101"",
						""tag"":""[081454RXF]"",
						""status"":200,
						""severity"":""""
					}";

			AssertCreateMessageFromInterchange(jsonMessage, jsonMessage, MessageTypeList.Codes.CDC);
			AssertCreateMessageFromInterchange(CreateUniversalInterchangeXml(responseMessage: jsonMessage), jsonMessage, MessageTypeList.Codes.CDC);
		}

		public void TestGenerateMessageFromInterchange_CDEERR()
		{
			var xmlError = @"<error>
								<message>Item DU-E 1: NF 35200400000000002720550900026408501000000000 já vinculada a  DU-E 20BR0000284120 registrada e não cancelada.</message>
								<code>DUEX-ER0148</code>
								<tag>[DUEX-OCAPWEDUEX]</tag>
								<date>2020-04-07 15:47:45</date>
								<status>422</status>
								<severity>ERROR</severity>
								<info>
									<ambiente>INC</ambiente>
									<mnemonico>DUEX</mnemonico>
									<sistema>Declaração Única de Exportação</sistema>
									<url>/due/api/ext/due</url>
									<usuario>00000000000</usuario>
									<visao>PRIV</visao>
								</info>
							</error>";

			AssertCreateMessageFromInterchange(CreateUniversalInterchangeXml(responseMessage: xmlError), xmlError, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Error);
			AssertCreateMessageFromInterchange(xmlError, xmlError, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Error);
		}

		public void TestGenerateMessageFromInterchange_CDESUC()
		{
			var xmlSuccess = @"<pucomexReturn>
									<message>Operação realizada com sucesso.</message>
									<due>20BR0000274180</due>
									<ruc>0BR00000000200000000000000000020403</ruc>
									<chaveDeAcesso>20RQN000613376</chaveDeAcesso>
									<date>2020-04-07 16:07:06</date>
									<cpf>00000000000</cpf>
								</pucomexReturn>";
			AssertCreateMessageFromInterchange(xmlSuccess, xmlSuccess, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success);
			AssertCreateMessageFromInterchange(CreateUniversalInterchangeXml(responseMessage: xmlSuccess), xmlSuccess, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Success);
		}

		public void TestGenerateMessageFromInterchange_CDE_COM()
		{
			var jsonMessage = BRCExportCompleteConsultMessageProcessorTest.JsonMessage;
			AssertCreateMessageFromInterchange(jsonMessage, jsonMessage, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.CompleteConsult);
			AssertCreateMessageFromInterchange(CreateUniversalInterchangeXml(responseMessage: jsonMessage), jsonMessage, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.CompleteConsult);
		}

		public void TestGenerateMessageFromInterchange_Invalid()
		{
			var invalidResponse = @"<response></response>";
			var interchange = ProcessEDIInterchange(invalidResponse);
			CombineAssertions(() =>
			{
				AssertEquals("Should have been 0 message extracted from interchange", 0, interchange.ContainedMessages.Count);
				AssertContains("Logger", "Can not determine Message Type for the Interchange Message", logger.Logs.ElementAt(0).ToString());
			});
		}
	}
}
