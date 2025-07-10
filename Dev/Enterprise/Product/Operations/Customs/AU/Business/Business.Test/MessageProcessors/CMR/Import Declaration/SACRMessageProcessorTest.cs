using System;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SACRMessageProcessorTest : CMRHeaderChargesResponseProcessorTest
	{
		public void TestEntryHeaderNumberIsPopulatedForSACR()
		{
			CMRSACRMessage message = Factory.New<CMRSACRMessage>();
			message.EM_MessageText = CMRImportDeclarationTestData.SACRHeld;
			processor.ProcessMessage(message);
			AssertEquals("Entry number on invoice header is populated", "AAAA6RHTE", declaration.CustomsEntryHeaders[0].EntryNumber);
		}

		public void TestProcessHeaderLevelOnlyAndNotOverrideLineLevelCharges()
		{
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_CustomsValue = 300m;
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_CustomsValue = 350m;
			entryLine1.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 186.44m);
			entryLine2.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 100m);

			incomingMessage.EM_MessageText = CMRImportDeclarationTestData.SACRTransactionRejected;
			processor.ProcessMessage(incomingMessage);
			AssertEquals("Entry line Customs value stays", 300m, entryLine1.CL_CustomsValue);
			AssertEquals("Entry line Customs value stays", 350m, entryLine2.CL_CustomsValue);

			AssertEquals("Entry line DutyAmount stays", 186.44m, entryLine1.DutyAmount);
			AssertEquals("Entry line DutyAmount stays", 100m, entryLine2.DutyAmount);
			AssertEquals("ErrorEmailCount", 1, processor.ErrorEmailSendCount);
			AssertEquals("AcknowledgementEmailCount", 0, processor.AcknowledgementEmailSendCount);
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.SAC;

		protected override ZString GetExpectedMessageName() => "Self Assessed Clearance Declaration Response (SACR)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => new SACRMessageProcessor(logger);

		protected override Type IncomingMessageType => typeof(CMRSACRMessage);
	}
}
