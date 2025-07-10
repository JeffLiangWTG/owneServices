using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GB.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE829MessageProcessor))]
	abstract class IE829MessageProcessorAbstractTest<TMessageType> : EMCSMessageProcessorAbstractTest<IE829MessageProcessor, IIE829>
	{
		public void TestEndToEndProcessing_WhenDeclarationNotFound()
		{
			var incomingMessage = CreateNewIncomingMessage();
			using (incomingMessage.Factory.AddDisposableService())
			{
				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);
				CombineAssertions("Process", () =>
				{
					AssertEquals("EM_Status should have been set PRS.", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
					var stmNote = GetStmNote(incomingMessage);
					AssertMultilineASCIIEquals("EADNumber's for which a Declaration could not be found:\r\nMRN1234567", stmNote.ST_NoteText);
				});
			}
		}

		public void TestEndToEndProcessing_WhenMultipleDeclarationsMatched()
		{
			var declaration1 = Factory.New<EMCSJobDeclaration>();
			declaration1.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			declaration1.JE_DeclarationReference = "E00000810";
			CreateNewMrnNumber(declaration1, "MRN1234567", "1");

			var declaration2 = Factory.New<EMCSJobDeclaration>();
			declaration2.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			declaration2.JE_DeclarationReference = "E00000811";
			CreateNewMrnNumber(declaration2, "MRN7654321", "1");

			var incomingMessage = CreateNewIncomingMessage();
			incomingMessage.EM_MessageText = MessageTextMRN;
			var outgoingMessage1 = CreateNewOutgoingMessage();
			outgoingMessage1.EM_ApplicationReference = ZString.Empty;
			var outgoingMessage2 = CreateNewOutgoingMessage();
			outgoingMessage2.EM_ApplicationReference = ZString.Empty;
			declaration1.Messages.Add(outgoingMessage1);
			declaration2.Messages.Add(outgoingMessage2);
			Factory.Save();

			using (incomingMessage.Factory.AddDisposableService())
			{
				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);
				CombineAssertions("Process", () =>
				{
					AssertEquals("JE_EntryStatus should have been set EXP.", EntryStatusList.Codes.EXP, declaration1.JE_EntryStatus);
					AssertEquals("JE_EntryStatus should have been set EXP.", EntryStatusList.Codes.EXP, declaration2.JE_EntryStatus);
					AssertEquals("EM_Status should have been set PRS.", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
					AssertNull("Message should not be linked when multiple declarations matched", incomingMessage.EM_LinkedObject);

					MessageProcessorNotificationTestHelper.AssertEmail("EMCS Notification of Accepted Export Response for E00000810", new[] { "Your EMCS Declaration for Job E00000810 received a notification of accepted export. For details please follow the link to the Job." }, new string[] { "staff1@where.com" });
					MessageProcessorNotificationTestHelper.AssertEmail("EMCS Notification of Accepted Export Response for E00000811", new[] { "Your EMCS Declaration for Job E00000811 received a notification of accepted export. For details please follow the link to the Job." }, new string[] { "staff1@where.com" });
				});
			}
		}

		protected override ZString MessageType => EMCSGBIncomingMessageTypeList.Codes.IE829;

		protected override IE829MessageProcessor Processor => new IE829MessageProcessor(logger, typeof(TMessageType));

		protected override void AssertProcessResult(EMCSJobDeclaration declaration, EMCSInboundEDIMessage incomingMessage)
		{
			AssertEquals("JE_EntryStatus should have been set EXP.", EntryStatusList.Codes.EXP, declaration.JE_EntryStatus);
			AssertEquals("EM_Status should have been set PRS.", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			MessageProcessorNotificationTestHelper.AssertEmail("EMCS Notification of Accepted Export", new[] { "Your EMCS Declaration for Job E00000810 received a notification of accepted export. For details please follow the link to the Job." }, new string[] { "staff1@where.com" });
		}
	}
}
