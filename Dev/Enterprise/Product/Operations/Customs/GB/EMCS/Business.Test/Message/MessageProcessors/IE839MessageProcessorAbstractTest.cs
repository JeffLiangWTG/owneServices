using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GB.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE839MessageProcessor))]
	abstract class IE839MessageProcessorAbstractTest<TMessageType> : EMCSMessageProcessorAbstractTest<IE839MessageProcessor, IIE839>
	{
		public void TestEndToEndProcessing_WhenDeclarationIsNotFound()
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
					AssertMultilineASCIIEquals("EADNumber's for Declaration's that could not be found:\r\nMRN1234567", stmNote.ST_NoteText);
				});
			}
		}

		public void TestEndToEndProcessing_WhenMultipleDeclarationsAreMatched()
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
					AssertEquals("JE_EntryStatus should have been set ERJ.", EntryStatusList.Codes.ERJ, declaration1.JE_EntryStatus);
					AssertEquals("JE_EntryStatus should have been set ERJ.", EntryStatusList.Codes.ERJ, declaration2.JE_EntryStatus);
					AssertEquals("EM_Status should have been set PRS.", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);

					MessageProcessorNotificationTestHelper.AssertEmail("EMCS e-AD rejected Response for E00000810", new[] { "Your EMCS Declaration for Job E00000810 was rejected by customs. For details please follow the link to the Job." }, new string[] { "staff1@where.com" });
					MessageProcessorNotificationTestHelper.AssertEmail("EMCS e-AD rejected Response for E00000811", new[] { "Your EMCS Declaration for Job E00000811 was rejected by customs. For details please follow the link to the Job." }, new string[] { "staff1@where.com" });
				});
			}
		}

		protected override ZString MessageType => EMCSGBIncomingMessageTypeList.Codes.IE839;

		protected override IE839MessageProcessor Processor => new IE839MessageProcessor(logger, typeof(TMessageType));

		protected override void AssertProcessResult(EMCSJobDeclaration declaration, EMCSInboundEDIMessage incomingMessage)
		{
			AssertEquals("EM_Status should have been set PRS.", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			MessageProcessorNotificationTestHelper.AssertEmail("EMCS e-AD rejected", new[] { "Your EMCS Declaration for Job E00000810 was rejected by customs. For details please follow the link to the Job." }, new string[] { "staff1@where.com" });
			AssertEquals(declaration, incomingMessage.EM_LinkedObject);
		}
	}
}
