using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;

namespace Enterprise.Customs.GB.CDS.Messaging.Testing
{
	public class CDSMessageStatusCalculatorTests : TestCaseWithFactory
	{
		public void TestGetMessageAwaitingStatus()
		{
			AssertEquals(MessageStatusList.Codes.AwaitingOriginal, CDSMessageStatusCalculator.GetMessageAwaitingStatus(Factory.New<CDSNewDeclarationEDIMessage>()));
			AssertEquals(MessageStatusList.Codes.AwaitingChange, CDSMessageStatusCalculator.GetMessageAwaitingStatus(Factory.New<CDSAmendDeclarationEDIMessage>()));
			AssertEquals(MessageStatusList.Codes.AwaitingDelete, CDSMessageStatusCalculator.GetMessageAwaitingStatus(Factory.New<CDSCancelDeclarationEDIMessage>()));
			AssertEquals(ZString.Empty, CDSMessageStatusCalculator.GetMessageAwaitingStatus(null));
		}

		public void TestGetMessageSentStatus()
		{
			AssertEquals(MessageStatusList.Codes.Sent, CDSMessageStatusCalculator.GetMessageSentStatus(Factory.New<CDSNewDeclarationEDIMessage>()));
			AssertEquals(MessageStatusList.Codes.Sent, CDSMessageStatusCalculator.GetMessageSentStatus(Factory.New<CDSAmendDeclarationEDIMessage>()));
			AssertEquals(MessageStatusList.Codes.Sent, CDSMessageStatusCalculator.GetMessageSentStatus(Factory.New<CDSCancelDeclarationEDIMessage>()));
			AssertEquals(ZString.Empty, CDSMessageStatusCalculator.GetMessageSentStatus(null));
		}

		public void TestGetMessageAcknowledgedStatus()
		{
			AssertEquals(MessageStatusList.Codes.AcknowledgedOriginal, CDSMessageStatusCalculator.GetMessageAcknowledgedStatus(Factory.New<CDSNewDeclarationEDIMessage>()));
			AssertEquals(MessageStatusList.Codes.AcknowledgedChange, CDSMessageStatusCalculator.GetMessageAcknowledgedStatus(Factory.New<CDSAmendDeclarationEDIMessage>()));
			AssertEquals(MessageStatusList.Codes.AcknowledgedDelete, CDSMessageStatusCalculator.GetMessageAcknowledgedStatus(Factory.New<CDSCancelDeclarationEDIMessage>()));
			AssertEquals(ZString.Empty, CDSMessageStatusCalculator.GetMessageAcknowledgedStatus(null));
		}

		public void TestGetMessageRejectedStatus()
		{
			AssertEquals(MessageStatusList.Codes.ErrorOriginal, CDSMessageStatusCalculator.GetMessageRejectedStatus(Factory.New<CDSNewDeclarationEDIMessage>()));
			AssertEquals(MessageStatusList.Codes.ErrorChange, CDSMessageStatusCalculator.GetMessageRejectedStatus(Factory.New<CDSAmendDeclarationEDIMessage>()));
			AssertEquals(MessageStatusList.Codes.ErrorDelete, CDSMessageStatusCalculator.GetMessageRejectedStatus(Factory.New<CDSCancelDeclarationEDIMessage>()));
			AssertEquals(ZString.Empty, CDSMessageStatusCalculator.GetMessageRejectedStatus(null));
		}

		public void TestGetMessageClearStatus()
		{
			AssertEquals(MessageStatusList.Codes.ClearOriginal, CDSMessageStatusCalculator.GetMessageClearStatus(Factory.New<CDSNewDeclarationEDIMessage>()));
			AssertEquals(MessageStatusList.Codes.ClearChange, CDSMessageStatusCalculator.GetMessageClearStatus(Factory.New<CDSAmendDeclarationEDIMessage>()));
			AssertEquals(MessageStatusList.Codes.ClearDelete, CDSMessageStatusCalculator.GetMessageClearStatus(Factory.New<CDSCancelDeclarationEDIMessage>()));
			AssertEquals(ZString.Empty, CDSMessageStatusCalculator.GetMessageClearStatus(null));
		}

		public void TestGetH7MessageAwaitingStatus()
		{
			CombineAssertions("Test all 3 types as well as null type and unknown type", () =>
			{
				AssertEquals(MessageStatusList.Codes.AwaitingOriginal, CDSMessageStatusCalculator.GetH7MessageAwaitingStatus(typeof(CDSNewDeclarationEDIMessage)));
				AssertEquals(MessageStatusList.Codes.AwaitingChange, CDSMessageStatusCalculator.GetH7MessageAwaitingStatus(typeof(CDSArrivalAmendmentDeclarationEDIMessage), new CusdecMessageFunction.Amended()));
				AssertEquals(MessageStatusList.Codes.AwaitingDelete, CDSMessageStatusCalculator.GetH7MessageAwaitingStatus(typeof(CDSArrivalAmendmentDeclarationEDIMessage), new CusdecMessageFunction.Deleted()));
				AssertEquals(ZString.Empty, CDSMessageStatusCalculator.GetH7MessageAwaitingStatus(null));
				AssertEquals(ZString.Empty, CDSMessageStatusCalculator.GetH7MessageAwaitingStatus(typeof(CDSEDIMessage)));
			});
		}
	}
}
