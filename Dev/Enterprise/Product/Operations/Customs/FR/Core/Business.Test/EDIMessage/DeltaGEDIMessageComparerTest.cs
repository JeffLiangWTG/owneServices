using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	public class DeltaGEDIMessageComparerTest : TestCaseWithFactory
	{
		public void TestDeltaGMessageOrder()
		{
			AssertProcessOrder("1.VLD.1", "1.BAE.1");
			AssertProcessOrder("2.VALIDE.1", "2.BAE.1");
			AssertProcessOrder("3.VAL.1", "3.BAE.1");
			AssertProcessOrder("1.VALIDE-CONTINGENT-CRITIQUE.1", "1.BAE-CONTINGENT-CRITIQUE.1");
			AssertProcessOrder("1.VALIDE -CC.1", "1.BAE-CC.1");
		}

		void AssertProcessOrder(string expectedFirstProcessedInterchangeNumber, string expectedLastProcessedInterchangeNumber)
		{
			var messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCExportErrorMessage.xml");
			var intchg1 = SetupInterchange(expectedLastProcessedInterchangeNumber, messageText);
			var intchg2 = SetupInterchange(expectedFirstProcessedInterchangeNumber, messageText);
			Factory.Save();

			var processor = new DeltaGIncomingMessageProcessor(new BatchProcessor.LoggingInformation());
			processor.ProcessInterchangesAndExecuteBatch(new System.Threading.CancellationToken());

			var msg1 = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg1.PK));
			var msg2 = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg2.PK));
			AssertEquals(msg1.EM_ReceiveTransmit, msg2.EM_ReceiveTransmit);
			AssertEquals("EM_InterchangeNumber", expectedLastProcessedInterchangeNumber, msg1.EM_InterchangeNumber);
			AssertEquals("EM_InterchangeNumber", expectedFirstProcessedInterchangeNumber, msg2.EM_InterchangeNumber);

			var messages = new List<EDIMessage>();
			messages.Add(msg1);
			messages.Add(msg2);
			var sortedMessages = DeltaGEDIMessageComparer.GetSortedMessages(messages, ListSortDirection.Ascending);
			AssertEquals(expectedLastProcessedInterchangeNumber + " message should be processed last", expectedLastProcessedInterchangeNumber, sortedMessages[sortedMessages.Length - 1].EM_InterchangeNumber);
		}

		EDIInterchange SetupInterchange(string interchangeNum, string messageText)
		{
			var intchg = Factory.NewWithValidTestData<EDIInterchange>();
			intchg.EI_From = "FRCUS";
			intchg.EI_To = "TEST";
			intchg.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;
			intchg.EI_InterchangeNum = interchangeNum;
			intchg.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg.EI_Status = EDIInterchange.Status.Queued;
			intchg.EI_IsActive = true;
			intchg.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg.EI_BodyText = messageText;
			return intchg;
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
