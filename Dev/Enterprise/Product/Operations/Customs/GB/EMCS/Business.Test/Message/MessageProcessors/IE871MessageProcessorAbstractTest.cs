using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GB.EMCS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE871MessageProcessor))]
	abstract class IE871MessageProcessorAbstractTest<TMessageType> : EMCSMessageProcessorAbstractTest<IE871MessageProcessor, IIE871>
	{
		protected abstract void UpdateAdministrativeReferenceCode();

		public void TestEndToEndProcessing_WhenNoDeclarationMatched()
		{
			UpdateAdministrativeReferenceCode();
			var incomingMessage = CreateNewIncomingMessage();
			Processor.PreProcessMessage(incomingMessage);
			ProcessMessage(incomingMessage);
			CreateSetupData();
			CombineAssertions(() =>
			{
				AssertEquals("Entry Status", ZString.Empty, declaration.JE_EntryStatus);
				AssertEquals("Message Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Error, incomingMessage.EM_Status);
			});
		}

		public void TestArrivalInformation()
		{
			CreateSetupData();
			var line1 = declaration.InvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			var line2 = declaration.InvoiceLines.AddNew();
			line2.JI_LineNo = 2;

			Processor.PreProcessMessage(incomingMessage);
			ProcessMessage(incomingMessage);

			var line3 = declaration.InvoiceLines.Cast<EMCSJobComInvoiceLine>().Single(x => x.JI_LineNo == 1);
			AssertEquals("1st line, ExciseProductCode", "B000", line3.ZG_ExciseProductCode);
			AssertEquals("1st line, Explanation", "Shortage explanation", line3.Outturn.C5_OutturnResultReason);

			var line4 = declaration.InvoiceLines.Cast<EMCSJobComInvoiceLine>().Single(x => x.JI_LineNo == 2);
			AssertEquals("2nd line, ExciseProductCode", "S200", line4.ZG_ExciseProductCode);
			AssertEquals("2nd line, Explanation", "Excess explanation", line4.Outturn.C5_OutturnResultReason);
		}

		protected abstract void UpdateGlobalExplanation();
		public void TestGenerateEmail_GlobalExplanationEmpty()
		{
			UpdateGlobalExplanation();
			CreateSetupData();
			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EMCSInboundEDIMessage>(declaration, incomingMessage.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = Staff.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);
			ProcessMessage(incomingMessage);
			MessageProcessorNotificationTestHelper.AssertEmailNotContains("EMCS Shortage or Excess Explanation Received", new[] { "Global Explanation" });
		}

		protected abstract void UpdateBodyAnalysis();
		public void TestGenerateEmail_LinesEmpty()
		{
			UpdateBodyAnalysis();
			CreateSetupData();
			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EMCSInboundEDIMessage>(declaration, incomingMessage.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = Staff.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);
			ProcessMessage(incomingMessage);
			MessageProcessorNotificationTestHelper.AssertEmailNotContains("EMCS Shortage or Excess Explanation Received", new[] { "Analysis Detail" });
		}

		protected override ZString MessageType => EMCSGBIncomingMessageTypeList.Codes.IE871;

		protected override ZString MessageText => EMCSXmlObjectSerializer.Serialize(ie871);

		protected override IE871MessageProcessor Processor => new IE871MessageProcessor(logger, typeof(TMessageType));

		protected override void AssertProcessResult(EMCSJobDeclaration declaration, EMCSInboundEDIMessage incomingMessage)
		{
			AssertEquals("JE_MessageStatus should have been set RCV.", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, declaration.JE_MessageStatus);
			AssertEquals("JE_EntryStatus should have been set SHR.", EntryStatusList.Codes.SHR, declaration.JE_EntryStatus);
			AssertEquals("EM_Status should have been set PRS.", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
			MessageProcessorNotificationTestHelper.AssertEmail("EMCS Shortage or Excess Explanation Received", new[] { $"Your EMCS Declaration for Job {declaration.JE_DeclarationReference} received a Shortage or Excess Explanation. For details please follow the Link to the Job." }, new string[] { "staff1@where.com" });
		}

		protected abstract TMessageType CreateDefaultIE871Type();
		protected TMessageType ie871;

		protected override void SetUp()
		{
			base.SetUp();
			ie871 = CreateDefaultIE871Type();
		}
	}
}
