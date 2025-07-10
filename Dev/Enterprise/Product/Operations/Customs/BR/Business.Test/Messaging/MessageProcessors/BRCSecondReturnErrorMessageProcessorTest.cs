using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCSecondReturnErrorMessageProcessorTest : BRCResponseMessageProcessorAbstractTest<BRCSecondReturnErrorMessageProcessor>
	{
		protected override IReadOnlyList<string> MessageFilterTypes => new[] { "CDE" };

		protected override IReadOnlyList<string> MessageFilterSubTypes => new[] { "ERR" };

		public void TestProcessResponseMessage_StatusRejected()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Error).ResponseMessage;
			responseMessage.EM_MessageText = ErrorReturnXML2;
			Factory.Save();

			entry.MovementReferenceNumberSetter(ZString.Empty, ZDateTime.Today);
			ExecuteMessageProcessor(responseMessage);
			Factory.Save();

			AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
			AssertEquals("CH_Status should be REJ", BRMessageStatusList.Codes.Rejected, entry.CH_Status);
			AssertEquals("Log AutoEvents.MessageRejected Created", AutoEvents.MessageRejected.Code, entry.Logs.MostRecentLog.SL_SE_NKEvent);
		}

		public void TestProcessResponseMessageError_StatusMessageFailed()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Error).ResponseMessage;
			responseMessage.EM_MessageText = @"<error></error>";
			Factory.Save();

			entry.MovementReferenceNumberSetter(ZString.Empty, ZDateTime.Today);
			var logger = ExecuteMessageProcessor(responseMessage);

			AssertEquals("EM_GB", entry.Branch.PK, responseMessage.EM_GB);
			AssertEquals("EM_Status should be FAL", EDIMessageStatusList.Codes.Failed, responseMessage.EM_Status);
			AssertEquals("Logger", "Error: \tMessage #1: Message deserialization was failed.\r\n", logger.LogMessages.ToString());
		}

		public void TestProcessResponseMessageError_WithExceptionInDeserialization()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var responseMessage = BRCResponseMessageProcessorTest.CreateResponseMessage(entry, MessageTypeList.Codes.CDE, EDIMessageSubTypeList.Codes.Error).ResponseMessage;
			responseMessage.EM_MessageText = @"aaaaaa";
			Factory.Save();

			entry.MovementReferenceNumberSetter(ZString.Empty, ZDateTime.Today);
			AssertExceptionThrown<InvalidOperationException>(() => { ExecuteMessageProcessor(responseMessage); });
		}

		public static string ErrorReturnXML = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<error>
	<message>Problemas encontrados: </message>
	<tag>[DUEX-YIXHAN2715]</tag>
	<date>2021-12-29T09:56:58</date>
	<status>422</status>
	<detail>
		<error>
			<message>Arquivo XML não atende as especificações definidas no XSD. cvc-complex-type.2.4.b: The content of element ''Destination'' is not complete. One of ''{""urn:wco:datamodel:WCO:GoodsDeclaration:1"":GoodsMeasure}'' is expected.</message>
			<code>DUEX-ER0027</code>
			<tag>[DUEX-AUJDTS2715]</tag>
			<date>2021-12-29T09:56:58</date>
			<status>422</status>
			<detail/>
			<severity>ERROR</severity>
		</error>
		<error>
			<message>Arquivo XML não atende as especificações definidas no XSD. cvc-complex-type.2.4.b: The content of element ''GovernmentAgencyGoodsItem'' is not complete. One of ''{""urn:wco:datamodel:WCO:GoodsDeclaration:1"":GovernmentProcedure}'' is expected.</message>
			<code>DUEX-ER0027</code>
			<tag>[DUEX-RFWJER2715]</tag>
			<date>2021-12-29T09:56:58</date>
			<status>422</status>
			<detail/>
			<severity>ERROR</severity>
		</error>
	</detail>
	<severity>ERROR</severity>
	<info>
		<ambiente>TRE</ambiente>
		<mnemonico>DUEX</mnemonico>
		<sistema>Declaração Única de Exportação</sistema>
		<trackerId>YR02FIHrks</trackerId>
		<url>/due/api/ext/due</url>
		<usuario>01183367708</usuario>
		<visao>PRIV</visao>
	</info>
</error>
";

		const string ErrorReturnXML2 = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<error>
	<message>Problemas encontrados: </message>
	<tag>[DUEX-YIXHAN2715]</tag>
	<date>2021-12-29T09:56:58</date>
	<status>422</status>
	<detail>
		<error>
			<message>Item DU-E 1: NF 35200400000000002720550900026408501000000000 já vinculada a  DU-E 20BR0000284120 registrada e não cancelada.</message>
			<code>DUEX-ER0148</code>
			<tag>[DUEX-OCAPWEDUEX]</tag>
			<date>2020-04-07 15:47:45</date>
			<status>422</status>
			<severity>ERROR</severity>
		</error>
	</detail>
	<severity>ERROR</severity>
	<info>
			<ambiente>INC</ambiente>
			<mnemonico>DUEX</mnemonico>
			<sistema>Declaração Única de Exportação</sistema>
			<url>/due/api/ext/due</url>
			<usuario>01183367708</usuario>
			<visao>PRIV</visao>
	</info>
</error>
";

		public static string ErrorReturnXML3 = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<error>
	<message>Arquivo XML não atende as especificações definidas no XSD. cvc-complex-type.2.4.a: Invalid content was found starting with element
Commodity
. One of
{""urn:wco:datamodel:WCO:GoodsDeclaration:1"":Destination}
 is expected.</message>
	<code>DUEX-ER0027</code>
	<tag>[DUEX-IJGFDZ2715]</tag>
	<date>2022-11-30T11:06:00</date>
	<status>422</status>
	<detail/>
	<severity>ERROR</severity>
	<info>
	<ambiente>TRE</ambiente>
	<mnemonico>DUEX</mnemonico>
	<sistema>Declaração Única de Exportação</sistema>
	<trackerId>Iy8ZTg8Uxe</trackerId>
	<url>/due/api/ext/due</url>
	<usuario>06302458609</usuario>
	<visao>PRIV</visao>
	</info>
</error>
";
	}
}
