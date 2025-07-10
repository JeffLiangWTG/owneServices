namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class CustomsMessageStatusTypeListTest : NUnit.Framework.TestCase
	{
		public void TestIsMessageAccepted()
		{
			Assert(CustomsMessageStatusTypeList.IsMessageAccepted(CustomsMessageStatusTypeList.Codes.OriginalAccepted));
			Assert(!CustomsMessageStatusTypeList.IsMessageAccepted(CustomsMessageStatusTypeList.Codes.AmendmentRejected));
			Assert(CustomsMessageStatusTypeList.IsMessageAccepted(CustomsMessageStatusTypeList.Codes.AmendmentAccepted));
			Assert(!CustomsMessageStatusTypeList.IsMessageAccepted(CustomsMessageStatusTypeList.Codes.AmendmentSent));
			Assert(CustomsMessageStatusTypeList.IsMessageAccepted(CustomsMessageStatusTypeList.Codes.CancellationAccepted));
			Assert(!CustomsMessageStatusTypeList.IsMessageAccepted(CustomsMessageStatusTypeList.Codes.CancellationRejected));
		}

		public void TestIsMessageRejected()
		{
			Assert(CustomsMessageStatusTypeList.IsMessageRejectedOrFailToDeliver(CustomsMessageStatusTypeList.Codes.ErrorSendingOriginal));
			Assert(!CustomsMessageStatusTypeList.IsMessageRejectedOrFailToDeliver(CustomsMessageStatusTypeList.Codes.OriginalSent));
			Assert(CustomsMessageStatusTypeList.IsMessageRejectedOrFailToDeliver(CustomsMessageStatusTypeList.Codes.OriginalRejected));
			Assert(!CustomsMessageStatusTypeList.IsMessageRejectedOrFailToDeliver(CustomsMessageStatusTypeList.Codes.OriginalAccepted));
			Assert(!CustomsMessageStatusTypeList.IsMessageRejectedOrFailToDeliver(CustomsMessageStatusTypeList.Codes.AmendmentSent));
			Assert(CustomsMessageStatusTypeList.IsMessageRejectedOrFailToDeliver(CustomsMessageStatusTypeList.Codes.AmendmentRejected));
			Assert(!CustomsMessageStatusTypeList.IsMessageRejectedOrFailToDeliver(CustomsMessageStatusTypeList.Codes.AmendmentAccepted));
			Assert(!CustomsMessageStatusTypeList.IsMessageRejectedOrFailToDeliver(CustomsMessageStatusTypeList.Codes.CancellationSent));
			Assert(CustomsMessageStatusTypeList.IsMessageRejectedOrFailToDeliver(CustomsMessageStatusTypeList.Codes.CancellationRejected));
			Assert(!CustomsMessageStatusTypeList.IsMessageRejectedOrFailToDeliver(CustomsMessageStatusTypeList.Codes.CancellationAccepted));
			Assert(CustomsMessageStatusTypeList.IsMessageRejectedOrFailToDeliver(CustomsMessageStatusTypeList.Codes.ErrorSendingAmendment));
			Assert(CustomsMessageStatusTypeList.IsMessageRejectedOrFailToDeliver(CustomsMessageStatusTypeList.Codes.ErrorSendingCancellation));
		}

		public void TestIsCancellationStatus()
		{
			Assert(CustomsMessageStatusTypeList.IsCancellationStatus(CustomsMessageStatusTypeList.Codes.CancellationSent));
			Assert(!CustomsMessageStatusTypeList.IsCancellationStatus(CustomsMessageStatusTypeList.Codes.ErrorSendingOriginal));
			Assert(CustomsMessageStatusTypeList.IsCancellationStatus(CustomsMessageStatusTypeList.Codes.CancellationRejected));
			Assert(!CustomsMessageStatusTypeList.IsCancellationStatus(CustomsMessageStatusTypeList.Codes.OriginalSent));
			Assert(CustomsMessageStatusTypeList.IsCancellationStatus(CustomsMessageStatusTypeList.Codes.CancellationAccepted));
			Assert(!CustomsMessageStatusTypeList.IsCancellationStatus(CustomsMessageStatusTypeList.Codes.OriginalRejected));
			Assert(CustomsMessageStatusTypeList.IsCancellationStatus(CustomsMessageStatusTypeList.Codes.ErrorSendingCancellation));
		}

		public void TestIsOriginalMessageAllowed()
		{
			Assert(!CustomsMessageStatusTypeList.IsOriginalMessageAllowed(CustomsMessageStatusTypeList.Codes.OriginalSent));
			Assert(CustomsMessageStatusTypeList.IsOriginalMessageAllowed(CustomsMessageStatusTypeList.Codes.OriginalRejected));
			Assert(!CustomsMessageStatusTypeList.IsOriginalMessageAllowed(CustomsMessageStatusTypeList.Codes.OriginalAccepted));
			Assert(CustomsMessageStatusTypeList.IsOriginalMessageAllowed(CustomsMessageStatusTypeList.Codes.ErrorSendingOriginal));
			Assert(!CustomsMessageStatusTypeList.IsOriginalMessageAllowed(CustomsMessageStatusTypeList.Codes.AmendmentAccepted));
			Assert(CustomsMessageStatusTypeList.IsOriginalMessageAllowed(""));
		}

		public void TestIsAmendmentOrCancellationMessageAllowed()
		{
			Assert(!CustomsMessageStatusTypeList.IsAmendmentOrCancellationMessageAllowed(CustomsMessageStatusTypeList.Codes.OriginalSent));
			Assert(CustomsMessageStatusTypeList.IsAmendmentOrCancellationMessageAllowed(CustomsMessageStatusTypeList.Codes.OriginalAccepted));
			Assert(!CustomsMessageStatusTypeList.IsAmendmentOrCancellationMessageAllowed(CustomsMessageStatusTypeList.Codes.OriginalRejected));
			Assert(CustomsMessageStatusTypeList.IsAmendmentOrCancellationMessageAllowed(CustomsMessageStatusTypeList.Codes.ErrorSendingAmendment));
			Assert(!CustomsMessageStatusTypeList.IsAmendmentOrCancellationMessageAllowed(CustomsMessageStatusTypeList.Codes.CancellationApprovedByCustoms));
			Assert(CustomsMessageStatusTypeList.IsAmendmentOrCancellationMessageAllowed(CustomsMessageStatusTypeList.Codes.ErrorSendingCancellation));
			Assert(!CustomsMessageStatusTypeList.IsAmendmentOrCancellationMessageAllowed(CustomsMessageStatusTypeList.Codes.ErrorSendingOriginal));
			Assert(CustomsMessageStatusTypeList.IsAmendmentOrCancellationMessageAllowed(CustomsMessageStatusTypeList.Codes.AmendmentAccepted));
			Assert(CustomsMessageStatusTypeList.IsAmendmentOrCancellationMessageAllowed(CustomsMessageStatusTypeList.Codes.CancellationAccepted));
			Assert(CustomsMessageStatusTypeList.IsAmendmentOrCancellationMessageAllowed(CustomsMessageStatusTypeList.Codes.AmendmentRejected));
			Assert(!CustomsMessageStatusTypeList.IsAmendmentOrCancellationMessageAllowed(CustomsMessageStatusTypeList.Codes.CancellationSent));
			Assert(CustomsMessageStatusTypeList.IsAmendmentOrCancellationMessageAllowed(CustomsMessageStatusTypeList.Codes.CancellationRejected));
		}

		public void TestIsWaitingForResponse()
		{
			Assert(CustomsMessageStatusTypeList.IsWaitingForResponse(CustomsMessageStatusTypeList.Codes.OriginalSent));
			Assert(!CustomsMessageStatusTypeList.IsWaitingForResponse(CustomsMessageStatusTypeList.Codes.OriginalRejected));
			Assert(!CustomsMessageStatusTypeList.IsWaitingForResponse(CustomsMessageStatusTypeList.Codes.OriginalAccepted));
			Assert(CustomsMessageStatusTypeList.IsWaitingForResponse(CustomsMessageStatusTypeList.Codes.AmendmentSent));
			Assert(!CustomsMessageStatusTypeList.IsWaitingForResponse(CustomsMessageStatusTypeList.Codes.AmendmentRejected));
			Assert(!CustomsMessageStatusTypeList.IsWaitingForResponse(CustomsMessageStatusTypeList.Codes.AmendmentAccepted));
			Assert(CustomsMessageStatusTypeList.IsWaitingForResponse(CustomsMessageStatusTypeList.Codes.CancellationSent));
			Assert(!CustomsMessageStatusTypeList.IsWaitingForResponse(CustomsMessageStatusTypeList.Codes.CancellationRejected));
			Assert(!CustomsMessageStatusTypeList.IsWaitingForResponse(CustomsMessageStatusTypeList.Codes.CancellationAccepted));
		}

		public void TestGetMainStatusReqCustomsReviewMessageOnMainMessage()
		{
			var result = CustomsMessageStatusTypeList.GetMainStatusLeadingToCustomsReviewMessageOnMainMessage(ElectronicDocumentTypeList.Codes._5DR);
			AssertEquals(3, result.Length);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalAccepted, result[0]);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, result[1]);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationAccepted, result[2]);

			result = CustomsMessageStatusTypeList.GetMainStatusLeadingToCustomsReviewMessageOnMainMessage(ElectronicDocumentTypeList.Codes._5DS);
			AssertEquals(3, result.Length);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalAccepted, result[0]);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, result[1]);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationAccepted, result[2]);

			result = CustomsMessageStatusTypeList.GetMainStatusLeadingToCustomsReviewMessageOnMainMessage(ElectronicDocumentTypeList.Codes._DF3);
			AssertEquals(3, result.Length);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalAccepted, result[0]);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, result[1]);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationAccepted, result[2]);

			result = CustomsMessageStatusTypeList.GetMainStatusLeadingToCustomsReviewMessageOnMainMessage(ElectronicDocumentTypeList.Codes._5FN);
			AssertEquals(2, result.Length);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, result[0]);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationAccepted, result[1]);

			result = CustomsMessageStatusTypeList.GetMainStatusLeadingToCustomsReviewMessageOnMainMessage(ElectronicDocumentTypeList.Codes._5SC);
			AssertEquals(2, result.Length);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, result[0]);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationAccepted, result[1]);

			result = CustomsMessageStatusTypeList.GetMainStatusLeadingToCustomsReviewMessageOnMainMessage(ElectronicDocumentTypeList.Codes._DHR);
			AssertEquals(2, result.Length);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, result[0]);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationAccepted, result[1]);

			result = CustomsMessageStatusTypeList.GetMainStatusLeadingToCustomsReviewMessageOnMainMessage(ElectronicDocumentTypeList.Codes._5BD);
			AssertEquals(2, result.Length);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, result[0]);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationAccepted, result[1]);

			result = CustomsMessageStatusTypeList.GetMainStatusLeadingToCustomsReviewMessageOnMainMessage(ElectronicDocumentTypeList.Codes._5SG);
			AssertEquals(2, result.Length);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, result[0]);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationAccepted, result[1]);

			result = CustomsMessageStatusTypeList.GetMainStatusLeadingToCustomsReviewMessageOnMainMessage(ElectronicDocumentTypeList.Codes._5SI);
			AssertEquals(2, result.Length);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, result[0]);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationAccepted, result[1]);

			result = CustomsMessageStatusTypeList.GetMainStatusLeadingToCustomsReviewMessageOnMainMessage(ElectronicDocumentTypeList.Codes._5BA);
			AssertEquals(2, result.Length);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, result[0]);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationAccepted, result[1]);

			result = CustomsMessageStatusTypeList.GetMainStatusLeadingToCustomsReviewMessageOnMainMessage(ElectronicDocumentTypeList.Codes._5TE);
			AssertEquals(2, result.Length);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, result[0]);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationAccepted, result[1]);

			result = CustomsMessageStatusTypeList.GetMainStatusLeadingToCustomsReviewMessageOnMainMessage(ElectronicDocumentTypeList.Codes._5TM);
			AssertEquals(2, result.Length);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, result[0]);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationAccepted, result[1]);

			result = CustomsMessageStatusTypeList.GetMainStatusLeadingToCustomsReviewMessageOnMainMessage(ElectronicDocumentTypeList.Codes._5UL);
			AssertEquals(2, result.Length);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, result[0]);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationAccepted, result[1]);

			result = CustomsMessageStatusTypeList.GetMainStatusLeadingToCustomsReviewMessageOnMainMessage(ElectronicDocumentTypeList.Codes._D72);
			AssertEquals(2, result.Length);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, result[0]);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationAccepted, result[1]);

			result = CustomsMessageStatusTypeList.GetMainStatusLeadingToCustomsReviewMessageOnMainMessage(ElectronicDocumentTypeList.Codes._5AS);
			AssertEquals(1, result.Length);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationAccepted, result[0]);

			result = CustomsMessageStatusTypeList.GetMainStatusLeadingToCustomsReviewMessageOnMainMessage(ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(1, result.Length);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationAccepted, result[0]);

			result = CustomsMessageStatusTypeList.GetMainStatusLeadingToCustomsReviewMessageOnMainMessage(ElectronicDocumentTypeList.Codes._DKJ);
			AssertEquals(1, result.Length);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, result[0]);

			result = CustomsMessageStatusTypeList.GetMainStatusLeadingToCustomsReviewMessageOnMainMessage(ElectronicDocumentTypeList.Codes._5BF);
			AssertEquals(1, result.Length);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, result[0]);
		}

		public void TestGetErrorStatus()
		{
			AssertEquals(CustomsMessageStatusTypeList.Codes.ErrorSendingOriginal, CustomsMessageStatusTypeList.GetErrorStatus(CustomsMessageStatusTypeList.Codes.OriginalSent));
			AssertEquals(CustomsMessageStatusTypeList.Codes.ErrorSendingAmendment, CustomsMessageStatusTypeList.GetErrorStatus(CustomsMessageStatusTypeList.Codes.AmendmentSent));
			AssertEquals(CustomsMessageStatusTypeList.Codes.ErrorSendingCancellation, CustomsMessageStatusTypeList.GetErrorStatus(CustomsMessageStatusTypeList.Codes.CancellationSent));
			AssertEquals(string.Empty, CustomsMessageStatusTypeList.GetErrorStatus(CustomsMessageStatusTypeList.Codes.ErrorSendingCancellation));
			AssertEquals(string.Empty, CustomsMessageStatusTypeList.GetErrorStatus(CustomsMessageStatusTypeList.Codes.CancellationApprovedByCustoms));
		}
	}
}
