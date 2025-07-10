using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class DeltaCResponseEDIMessagePrettierTest : TestCaseWithFactory
	{
		public void TestGetOutgoingMessageInterpretation()
		{
			var messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportValidResponseMessage.xml");
			var ediMessage = Factory.New<DeltaCImportFREDIMessage>();
			ediMessage.EM_MessageType = MessageTypeList.Codes.IMC;
			ediMessage.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			ediMessage.EM_MessageText = messageText;
			ediMessage.EM_ReceiveTransmit = "TRX";

			AssertEquals("Message interpretation of outgoing messages should reflect the raw message text", messageText, ediMessage.EM_MessageInterpretation);

			var ediMessage2 = Factory.New<DeltaCExportFREDIMessage>();
			ediMessage2.EM_MessageType = MessageTypeList.Codes.EXC;
			ediMessage2.EM_MessageSubType = MessageSubTypeList.Codes.EXC;
			ediMessage2.EM_MessageText = messageText;
			ediMessage2.EM_ReceiveTransmit = "TRX";

			AssertEquals("Message interpretation of outgoing messages should reflect the raw message text", messageText, ediMessage2.EM_MessageInterpretation);
		}

		public void TestGetMessageInterpretation_Valid()
		{
			var messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportValidResponseMessage.xml");

			var ediMessage = Factory.New<DeltaCImportFREDIMessage>();
			ediMessage.EM_MessageType = MessageTypeList.Codes.IMC;
			ediMessage.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			ediMessage.EM_MessageText = messageText;

			AssertEquals(@"<H1>Delta response</H1><br /><H3>TransactionID: AAAAAAAAA+BBB+0000000001</H3><H3>Entry Status : VALIDE</H3><H3>Entry Status date: 13/02/2019</H3><H3>Entry Status hour: 04:04</H3><H3>Entry Evenement tag: Demande de validation</H3><H3>Entry Reference: 9000-B00177613</H3><H3>Delta Reference: 1901204207</H3><br />", ediMessage.EM_MessageInterpretation);

			messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DaltaCExportValidResponseMessage.xml");

			var ediMessage2 = Factory.New<DeltaCExportFREDIMessage>();
			ediMessage2.EM_MessageType = MessageTypeList.Codes.EXC;
			ediMessage2.EM_MessageSubType = MessageSubTypeList.Codes.EXC;
			ediMessage2.EM_MessageText = messageText;

			AssertEquals(@"<H1>Delta response</H1><br /><H3>TransactionID: AAAAAAAAA+BBB+0000000001</H3><H3>Entry Status : ANT</H3><H3>Entry Status date: 16/09/2019</H3><H3>Entry Status hour: 10:23</H3><H3>Entry Reference: 9000-B00177613</H3><H3>Delta Reference: 1907396130</H3><br />", ediMessage2.EM_MessageInterpretation);
		}

		public void TestGetMessageInterpretation_BAE()
		{
			var messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");

			var ediMessage = Factory.New<DeltaCImportFREDIMessage>();
			ediMessage.EM_MessageType = MessageTypeList.Codes.IMC;
			ediMessage.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			ediMessage.EM_MessageText = messageText;

			AssertEquals(@"<H1>Delta response</H1><br /><H3>TransactionID: AAAAAAAAA+BBB+0000000001</H3><H3>Entry Status : BAE</H3><H3>Entry Status date: 13/02/2019</H3><H3>Entry Status hour: 04:04</H3><H3>Entry Evenement tag: Expiration du timer</H3><H3>Entry Reference: 9000-B00177613</H3><H3>Delta Reference: 1901204207</H3><br />", ediMessage.EM_MessageInterpretation);

			messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportBAEResponseMessage.xml");

			var ediMessage2 = Factory.New<DeltaCExportFREDIMessage>();
			ediMessage2.EM_MessageType = MessageTypeList.Codes.EXC;
			ediMessage2.EM_MessageSubType = MessageSubTypeList.Codes.EXC;
			ediMessage2.EM_MessageText = messageText;

			AssertEquals(@"<H1>Delta response</H1><br /><H3>TransactionID: AAAAAAAAA+BBB+0000000001</H3><H3>Entry Status : BAE</H3><H3>Entry Status date: 13/02/2019</H3><H3>Entry Status hour: 04:04</H3><H3>Entry Reference: 9000-B00177613</H3><H3>Delta Reference: 1907396130</H3><br />", ediMessage2.EM_MessageInterpretation);
		}

		public void TestGetMessageInterpretation_Erreur()
		{
			var messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportErreurResponseMessage.xml");

			var ediMessage = Factory.New<DeltaCImportFREDIMessage>();
			ediMessage.EM_MessageType = MessageTypeList.Codes.IMC;
			ediMessage.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			ediMessage.EM_MessageText = messageText;

			AssertEquals(@"<H1>Delta response</H1><br /><H3>TransactionID: AAAAAAAAA+BBB+0000000001</H3><H3>Entry Reference: 9000-B00177613</H3><br /><table width=""100%""><tr><td>Error Code</td><td>Error Description</td></tr><tr><td>COR1500</td><td>This is error</td></tr></table>", ediMessage.EM_MessageInterpretation);

			messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportErrorMessage.xml");

			var ediMessage2 = Factory.New<DeltaCExportFREDIMessage>();
			ediMessage2.EM_MessageType = MessageTypeList.Codes.EXC;
			ediMessage2.EM_MessageSubType = MessageSubTypeList.Codes.EXC;
			ediMessage2.EM_MessageText = messageText;

			AssertEquals(@"<H1>Delta response</H1><br /><H3>TransactionID: AAAAAAAAA+BBB+0000000001</H3><H3>Entry Reference: 9000-B00177613</H3><br /><table width=""100%""><tr><td>Error Code</td><td>Error Description</td></tr><tr><td>DELTA_FONCTL</td><td>Message error</td></tr></table>", ediMessage2.EM_MessageInterpretation);
		}

		public void TestGetMessageInterpretation_Alerts()
		{
			var messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportResponseMessageWithAlerts.xml");

			var ediMessage = Factory.New<DeltaCImportFREDIMessage>();
			ediMessage.EM_MessageType = MessageTypeList.Codes.IMC;
			ediMessage.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			ediMessage.EM_MessageText = messageText;

			AssertEquals(@"<H1>Delta response</H1><br /><H3>TransactionID: 0000000001</H3><H3>Entry Reference: 9000-B00177613</H3><H3>Alerts:</H3><ul><H4><li>[Article n°2] - C401 - FR2007502637-R - N° ligne 2 - F030 : N° de ligne non conforme au document</H4><H4><li>[Article n°3] - C225 - FR2007502637-R - F030 : Quantité non conforme au document</H4></ul><br />", ediMessage.EM_MessageInterpretation);

			messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportResponseMessageWithAlerts.xml");

			var ediMessage2 = Factory.New<DeltaCExportFREDIMessage>();
			ediMessage2.EM_MessageType = MessageTypeList.Codes.EXC;
			ediMessage2.EM_MessageSubType = MessageSubTypeList.Codes.EXC;
			ediMessage2.EM_MessageText = messageText;

			AssertEquals(@"<H1>Delta response</H1><br /><H3>TransactionID: 0000000001</H3><H3>Entry Reference: 9000-B00177613</H3><H3>Alerts:</H3><ul><H4><li>[Article n°2] - C401 - FR2007502637-R - N° ligne 2 - F030 : N° de ligne non conforme au document</H4><H4><li>[Article n°3] - C225 - FR2007502637-R - F030 : Quantité non conforme au document</H4></ul><br />", ediMessage.EM_MessageInterpretation);
		}

		public void TestGetMessageInterpretation_RequestStatus()
		{
			var messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportResponseMessageWithRequestStatus.xml");

			var ediMessage = Factory.New<DeltaCImportFREDIMessage>();
			ediMessage.EM_MessageType = MessageTypeList.Codes.IMC;
			ediMessage.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			ediMessage.EM_MessageText = messageText;

			AssertEquals(@"<H1>Delta response</H1><br /><H3>TransactionID: 0000000001</H3><H3>Entry Reference: 9000-B00177613</H3><H3>Response to amendment/cancellation request:</H3><ul><H4><li>Demand type: Amendment</H4><H4><li>Your reason: Changement poids et colisage</H4><H4><li>Your justification: Article 173 du CDU</H4><H4><li>Your comment: Packing list initiale erronnée</H4><H4><li>Your new destination: Declaration 123654455</H4><H4><li>Rejection reason: Rectification inutile</H4><H4><li>Request number: 2000090954</H4><H4><li>Agent name: Jean-Paul GRONIER</H4><H4><li>Agent office: FR001290</H4></ul><br />", ediMessage.EM_MessageInterpretation);

			messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportResponseMessageWithRequestStatus.xml");

			var ediMessage2 = Factory.New<DeltaCExportFREDIMessage>();
			ediMessage2.EM_MessageType = MessageTypeList.Codes.EXC;
			ediMessage2.EM_MessageSubType = MessageSubTypeList.Codes.EXC;
			ediMessage2.EM_MessageText = messageText;

			AssertEquals(@"<H1>Delta response</H1><br /><H3>TransactionID: 0000000001</H3><H3>Entry Reference: 9000-B00177613</H3><H3>Response to amendment/cancellation request:</H3><ul><H4><li>Demand type: Amendment</H4><H4><li>Your reason: Changement poids et colisage</H4><H4><li>Your justification: Article 173 du CDU</H4><H4><li>Your comment: Packing list initiale erronnée</H4><H4><li>Your new destination: Declaration 123654455</H4><H4><li>Rejection reason: Rectification inutile</H4><H4><li>Request number: 2000090954</H4><H4><li>Agent name: Jean-Paul GRONIER</H4><H4><li>Agent office: FR001290</H4></ul><br />", ediMessage.EM_MessageInterpretation);
		}

		public void TestGetMessageInterpretation_ECS()
		{
			var messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportECSResponseMessage.xml");

			var ediMessage2 = Factory.New<DeltaCExportFREDIMessage>();
			ediMessage2.EM_MessageType = MessageTypeList.Codes.EXC;
			ediMessage2.EM_MessageSubType = MessageSubTypeList.Codes.EXC;
			ediMessage2.EM_MessageText = messageText;

			AssertEquals(@"<H1>Delta response</H1><br /><H3>TransactionID: AAAAAAAAA+BBB+0000000001</H3><H3>Entry ECS status: EEC</H3><H3>Entry Status date: 09/12/2020</H3><H3>Entry Status hour: 10:59</H3><H3>Entry Reference: 0FR33159700500064-B183287</H3><H3>Delta Reference: 2000026199</H3><br />", ediMessage2.EM_MessageInterpretation);
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
