using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class DCGResponsePrettierTest : TestCaseWithFactory
	{
		public void TestGetOutgoingMessageInterpretation()
		{
			var messageText = "Any string here is ok";
			var ediMessage = Factory.New<DCGResponseFREDIMessage>();
			ediMessage.EM_MessageType = MessageTypeList.Codes.DCG;
			ediMessage.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			ediMessage.EM_MessageText = messageText;
			ediMessage.EM_ReceiveTransmit = "TRX";

			AssertEquals("Message interpretation of outgoing messages should reflect the raw message text", messageText, ediMessage.EM_MessageInterpretation);
		}

		public void TestGetMessageInterpretationWhenErrors()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustomsWithErrors.xml");

			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();

			var processor = new DCGResponseMessageProcessor(new LoggingInformation());

			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals(@"<H1>DCG Status</H1><br /><b>Reference: </b>19014110 / DCG_REFERENCE<br /><b>Period: </b> - <br /><br /><b>Errors:</b><br /><font color=""red"">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;DELTA_FONCTL : Erreur 1</font><br /><font color=""red"">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;DELTA_FONCTL : Erreur 2</font>", message.EM_MessageInterpretation);
		}

		public void TestGetMessageInterpretationWhenAnomalies()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustomsWithAnomalies.xml");

			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();

			var processor = new DCGResponseMessageProcessor(new LoggingInformation());

			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals(@"<H1>DCG Status</H1><br /><b>Reference: </b>19014110 / DCG_REFERENCE<br /><b>Period: </b> - <br /><br /><b>Anomalies:</b><br /><font color=""orange"">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;DELTA_FONCTL : Anomalie 1</font><br /><font color=""orange"">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;DELTA_FONCTL : Anomalie 2</font>", message.EM_MessageInterpretation);
		}

		public void TestGetMessageInterpretation()
		{
			var importMessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DCGResponseFromCustoms.xml");

			var message = Factory.New<DCGResponseFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.DCG;
			message.EM_MessageSubType = MessageSubTypeList.Codes.DCG;
			message.EM_MessageText = importMessageText;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();

			var processor = new DCGResponseMessageProcessor(new LoggingInformation());

			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals(@"<H1>DCG Status</H1><br /><b>Reference: </b>19014110 / DCG_REFERENCE<br /><b>Period: </b>01/07/2019 - 31/07/2019<br /><br /><b>Taxes:</b><br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;A445 / B00 = 101 079,00 €<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;U165 / A00 = 160 254,00 €<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;A445 / B00 = 597 310,00 €<br /><br /><b>Entries:</b><br /><font color=""green"">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;1906142283 / 0738/19</font><br /><font color=""green"">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;1906135971 / 0892/19</font><br /><font color=""green"">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;1906117547 / 0736/19</font><br /><font color=""green"">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;1906100402 / 0809/19</font><br /><font color=""green"">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;1906096292 / 0961/19</font>", message.EM_MessageInterpretation);
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
