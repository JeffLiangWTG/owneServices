using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusEntryHeaderStatusTest : NUnit.Framework.TestCase
	{
		public void TestGetFirstClearStatusFor()
		{
			var list = new MessageStatusList();
			AssertEquals(MessageStatusList.Codes.Sent, list.GetFirstClearStatusFor(MessageType.DataLoadingModule));
			AssertEquals("", list.GetFirstClearStatusFor(MessageType.Undefined));
		}

		public void TestIsMessageStatusAllowCancellation()
		{
			Assert(MessageStatusList.IsMessageStatusAllowCancellation(MessageStatusList.Codes.ErrorOriginal));
			Assert(MessageStatusList.IsMessageStatusAllowCancellation(MessageStatusList.Codes.NotSent));
			Assert(MessageStatusList.IsMessageStatusAllowCancellation(MessageStatusList.Codes.ClearDelete));
			Assert(!MessageStatusList.IsMessageStatusAllowCancellation(MessageStatusList.Codes.AwaitingDelete));
			Assert(!MessageStatusList.IsMessageStatusAllowCancellation(MessageStatusList.Codes.AwaitingOriginal));
			Assert(!MessageStatusList.IsMessageStatusAllowCancellation(MessageStatusList.Codes.AcknowledgedOriginal));
			Assert(!MessageStatusList.IsMessageStatusAllowCancellation(MessageStatusList.Codes.ClearOriginal));
		}

		public void TestIsMessageAccepted()
		{
			Assert(MessageStatusList.IsMessageAccepted(MessageStatusList.Codes.ClearOriginal));
			Assert(MessageStatusList.IsMessageAccepted(MessageStatusList.Codes.ClearDelete));
			Assert(MessageStatusList.IsMessageAccepted(MessageStatusList.Codes.ClearChange));
			Assert(!MessageStatusList.IsMessageAccepted(MessageStatusList.Codes.AcknowledgedOriginal));
			Assert(!MessageStatusList.IsMessageAccepted(MessageStatusList.Codes.AcknowledgedDelete));
			Assert(!MessageStatusList.IsMessageAccepted(MessageStatusList.Codes.AcknowledgedChange));
			Assert(!MessageStatusList.IsMessageAccepted(MessageStatusList.Codes.AwaitingOriginal));
			Assert(!MessageStatusList.IsMessageAccepted(MessageStatusList.Codes.AwaitingDelete));
			Assert(!MessageStatusList.IsMessageAccepted(MessageStatusList.Codes.AwaitingChange));
			Assert(!MessageStatusList.IsMessageAccepted(MessageStatusList.Codes.ErrorOriginal));
			Assert(!MessageStatusList.IsMessageAccepted(MessageStatusList.Codes.ErrorDelete));
			Assert(!MessageStatusList.IsMessageAccepted(MessageStatusList.Codes.ErrorChange));
		}

		public void TestGetAppropriateErrorCode()
		{
			AssertEquals(MessageStatusList.Codes.ErrorOriginal, MessageStatusList.GetAppropriateErrorCode(MessageStatusList.Codes.AwaitingOriginal));
			AssertEquals(MessageStatusList.Codes.ErrorOriginal, MessageStatusList.GetAppropriateErrorCode(MessageStatusList.Codes.ClearOriginal));

			AssertEquals(MessageStatusList.Codes.ErrorChange, MessageStatusList.GetAppropriateErrorCode(MessageStatusList.Codes.AwaitingChange));
			AssertEquals(MessageStatusList.Codes.ErrorChange, MessageStatusList.GetAppropriateErrorCode(MessageStatusList.Codes.ClearChange));

			AssertEquals(MessageStatusList.Codes.ErrorDelete, MessageStatusList.GetAppropriateErrorCode(MessageStatusList.Codes.AwaitingDelete));
			AssertEquals(MessageStatusList.Codes.ErrorDelete, MessageStatusList.GetAppropriateErrorCode(MessageStatusList.Codes.ClearDelete));

			AssertEquals(MessageStatusList.Codes.ErrorReplace, MessageStatusList.GetAppropriateErrorCode(MessageStatusList.Codes.AwaitingReplace));
			AssertEquals(MessageStatusList.Codes.ErrorReplace, MessageStatusList.GetAppropriateErrorCode(MessageStatusList.Codes.ClearReplace));

			AssertEquals(ZString.Empty, MessageStatusList.GetAppropriateErrorCode(MessageStatusList.Codes.ErrorChange));
		}

		public void TestGetAppropriateClearCode()
		{
			AssertEquals(MessageStatusList.Codes.ClearOriginal, MessageStatusList.GetAppropriateClearCode(MessageStatusList.Codes.AwaitingOriginal));
			AssertEquals(MessageStatusList.Codes.ClearChange, MessageStatusList.GetAppropriateClearCode(MessageStatusList.Codes.AwaitingChange));
			AssertEquals(MessageStatusList.Codes.ClearReplace, MessageStatusList.GetAppropriateClearCode(MessageStatusList.Codes.AwaitingReplace));
			AssertEquals(MessageStatusList.Codes.ClearDelete, MessageStatusList.GetAppropriateClearCode(MessageStatusList.Codes.AwaitingDelete));
		}
	}
}
