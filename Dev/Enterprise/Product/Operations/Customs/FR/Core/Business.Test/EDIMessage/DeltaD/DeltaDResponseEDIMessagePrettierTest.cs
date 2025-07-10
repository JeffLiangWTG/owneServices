using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class DeltaDResponseEDIMessagePrettierTest : TestCaseWithFactory
	{
		public void TestGetOutgoingMessageInterpretation()
		{
			var messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaDImportErrorResponseMessage.xml");

			var ediMessage = Factory.New<DeltaDImportFREDIMessage>();
			ediMessage.EM_MessageType = MessageTypeList.Codes.IMD;
			ediMessage.EM_MessageSubType = MessageSubTypeList.Codes.IMD;
			ediMessage.EM_MessageText = messageText;
			ediMessage.EM_ReceiveTransmit = "TRX";

			AssertEquals("Message interpretation of outgoing messages should reflect the raw message text", messageText, ediMessage.EM_MessageInterpretation);

			var ediMessage2 = Factory.New<DeltaDExportFREDIMessage>();
			ediMessage2.EM_MessageType = MessageTypeList.Codes.EXD;
			ediMessage2.EM_MessageSubType = MessageSubTypeList.Codes.EXD;
			ediMessage2.EM_MessageText = messageText;
			ediMessage2.EM_ReceiveTransmit = "TRX";

			AssertEquals("Message interpretation of outgoing messages should reflect the raw message text", messageText, ediMessage2.EM_MessageInterpretation);
		}

		public void TestGetMessageInterpretation_Alerts()
		{
			var messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaDImportResponseMessageWithAlerts.xml");

			var ediMessage = Factory.New<DeltaDImportFREDIMessage>();
			ediMessage.EM_MessageType = MessageTypeList.Codes.IMD;
			ediMessage.EM_MessageSubType = MessageSubTypeList.Codes.IMD;
			ediMessage.EM_MessageText = messageText;

			AssertEquals(@"<H1>Delta response</H1><br /><H3>TransactionID: 0000000001</H3><H3>Entry Reference: 9000-B00177613</H3><H3>Alerts:</H3><ul><H4><li>[Article n°2] - C401 - FR2007502637-R - N° ligne 2 - F030 : N° de ligne non conforme au document</H4><H4><li>[Article n°3] - C225 - FR2007502637-R - F030 : Quantité non conforme au document</H4></ul><br />", ediMessage.EM_MessageInterpretation);

			messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaDExportResponseMessageWithAlerts.xml");

			var ediMessage2 = Factory.New<DeltaDExportFREDIMessage>();
			ediMessage2.EM_MessageType = MessageTypeList.Codes.EXD;
			ediMessage2.EM_MessageSubType = MessageSubTypeList.Codes.EXD;
			ediMessage2.EM_MessageText = messageText;

			AssertEquals(@"<H1>Delta response</H1><br /><H3>TransactionID: 0000000001</H3><H3>Entry Reference: 9000-B00177613</H3><H3>Alerts:</H3><ul><H4><li>[Article n°2] - C401 - FR2007502637-R - N° ligne 2 - F030 : N° de ligne non conforme au document</H4><H4><li>[Article n°3] - C225 - FR2007502637-R - F030 : Quantité non conforme au document</H4></ul><br />", ediMessage.EM_MessageInterpretation);
		}

		public void TestGetMessageInterpretation_RequestStatus()
		{
			var messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaDImportResponseMessageWithRequestStatus.xml");

			var ediMessage = Factory.New<DeltaDImportFREDIMessage>();
			ediMessage.EM_MessageType = MessageTypeList.Codes.IMD;
			ediMessage.EM_MessageSubType = MessageSubTypeList.Codes.IMD;
			ediMessage.EM_MessageText = messageText;

			AssertEquals(@"<H1>Delta response</H1><br /><H3>TransactionID: 0000000001</H3><H3>Entry Reference: 9000-B00177613</H3><H3>Response to amendment/cancellation request:</H3><ul><H4><li>Request status: Rejected</H4><H4><li>Status granted on: 02-Jul-20</H4><H4><li>Your reason: Changement poids et colisage</H4><H4><li>Your justification: Article 173 du CDU</H4><H4><li>Your comment: Packing list initiale erronnÃ©e</H4><H4><li>Your new destination: Declaration 123654455</H4><H4><li>Rejection reason: Rectification inutile</H4><H4><li>Request number: 2000090954</H4><H4><li>Agent name: Jean-Paul GRONIER</H4><H4><li>Agent office: FR001290</H4></ul><br />", ediMessage.EM_MessageInterpretation);

			messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaDExportResponseMessageWithRequestStatus.xml");

			var ediMessage2 = Factory.New<DeltaDExportFREDIMessage>();
			ediMessage2.EM_MessageType = MessageTypeList.Codes.EXD;
			ediMessage2.EM_MessageSubType = MessageSubTypeList.Codes.EXD;
			ediMessage2.EM_MessageText = messageText;

			AssertEquals(@"<H1>Delta response</H1><br /><H3>TransactionID: 0000000001</H3><H3>Entry Reference: 9000-B00177613</H3><H3>Response to amendment/cancellation request:</H3><ul><H4><li>Request status: Rejected</H4><H4><li>Status granted on: 02-Jul-20</H4><H4><li>Your reason: Changement poids et colisage</H4><H4><li>Your justification: Article 173 du CDU</H4><H4><li>Your comment: Packing list initiale erronnÃ©e</H4><H4><li>Your new destination: Declaration 123654455</H4><H4><li>Rejection reason: Rectification inutile</H4><H4><li>Request number: 2000090954</H4><H4><li>Agent name: Jean-Paul GRONIER</H4><H4><li>Agent office: FR001290</H4></ul><br />", ediMessage2.EM_MessageInterpretation);
		}

		public void TestGetMessageInterpretation_ECS()
		{
			var messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaDExportECSResponseMessage.xml"); //à créer

			var ediMessage = Factory.New<DeltaDExportFREDIMessage>();
			ediMessage.EM_MessageType = MessageTypeList.Codes.EXD;
			ediMessage.EM_MessageSubType = MessageSubTypeList.Codes.EXD;
			ediMessage.EM_MessageText = messageText;

			AssertEquals(@"<H1>Delta response</H1><br /><H3>TransactionID: AAAAAAAAA+BBB+0000000001</H3><H3>Entry ECS status: EEC</H3><H3>Entry Status date: 01/02/2021</H3><H3>Entry Status hour: 09:43</H3><H3>Entry Reference: 9000-B00177613</H3><H3>Delta Reference: 1907396130</H3><br />", ediMessage.EM_MessageInterpretation);
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
