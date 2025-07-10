using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC225C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC225CProcessor))]
	class CC225CProcessorTest : NCTSLinkedGuaranteeMessageProcessorAbstractTest<CC225CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC225CProvider>
	{
		public void TestLinkedNoGuaranteeFound() => CombineAssertions(() =>
		{
			_ = CreateCusGuaranteeHeader(Core.Constants.CountryCodes.Latvia, grn, ZDate.BrettsBirthday, ZDate.BrettsBirthday.AddYears(1));

			using (Factory.AddDisposableService())
			{
				var processor = Processor;
				var incomingMessage = CreateNewIncomingMessage();
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);
				AssertEquals("incomingMessage.EM_LinkUniqueID", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
				AssertEquals("incomingMessage.EM_LinkTable", ZString.Empty, incomingMessage.EM_LinkTable);
				AssertEquals("Message should have been set PRS.", "PRS", incomingMessage.EM_Status);
			}
		});

		public void TestMultipleGuarantees() => CombineAssertions(() =>
		{
			var header1 = CreateCusGuaranteeHeader(Core.Constants.CountryCodes.Ireland, "GRN001", ZDate.BrettsBirthday, ZDate.BrettsBirthday.AddYears(1));
			var header2 = CreateCusGuaranteeHeader(Core.Constants.CountryCodes.Ireland, "GRN002", ZDate.BrettsBirthday, ZDate.BrettsBirthday.AddYears(1));

			using (Factory.AddDisposableService())
			{
				var processor = Processor;
				var messgageText = InterchangeProcessorTestHelper.GetStandardCC225CText(
					("GRN001", new DateTime(2001, 1, 1), new DateTime(2001, 12, 31)),
					("GRN002", new DateTime(2002, 1, 1), new DateTime(2002, 12, 31)));
				var incomingMessage = CreateNewIncomingMessage(InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, messgageText, includeResponseWrap: false));

				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				AssertEquals("GRN001 CPH_StartDate", new ZDate(2001, 1, 1), header1.CPH_StartDate);
				AssertEquals("GRN001 CPH_EndDate", new ZDate(2001, 12, 31), header1.CPH_EndDate);
				AssertEquals("GRN002 CPH_StartDate", new ZDate(2002, 1, 1), header2.CPH_StartDate);
				AssertEquals("GRN002 CPH_EndDate", new ZDate(2002, 12, 31), header2.CPH_EndDate);
			}
		});

		protected override void AssertProcessResultCore(CusGuaranteeHeader messageAttachee, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("CPH_StartDate", validityDate, messageAttachee.CPH_StartDate);
			AssertEquals("CPH_EndDate", invalidityDate, messageAttachee.CPH_EndDate);
		}

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE225;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC225CText((grn, validityDate, invalidityDate));

		protected override ZString MessageFriendlyName => "CC225C: GUARANTEE UPDATE NOTIFICATION";

		protected override CC225CProcessor Processor => new CC225CProcessor(logger, typeof(Cc225CType));

		readonly DateTime? validityDate = new DateTime(2023, 01, 01);
		readonly DateTime? invalidityDate = new DateTime(2024, 12, 31);
	}
}
