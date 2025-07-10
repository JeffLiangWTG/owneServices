using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class ElectronicDocumentTypeListTest : NUnit.Framework.TestCase
	{
		public void TestSupportsAmendment()
		{
			Assert("830 - export dec, 5AS is an amendment", ElectronicDocumentTypeList.SupportsAmendment(ElectronicDocumentTypeList.Codes._830));
			Assert("5WN", !ElectronicDocumentTypeList.SupportsAmendment(ElectronicDocumentTypeList.Codes._5WN));
			Assert("929 - import dec, 5FE is an amendment", ElectronicDocumentTypeList.SupportsAmendment(ElectronicDocumentTypeList.Codes._929));
			Assert(!ElectronicDocumentTypeList.SupportsAmendment(ElectronicDocumentTypeList.Codes._5FE));
			Assert(ElectronicDocumentTypeList.SupportsAmendment(ElectronicDocumentTypeList.Codes._5BA));
			Assert(!ElectronicDocumentTypeList.SupportsAmendment(ElectronicDocumentTypeList.Codes._008));
			Assert(ElectronicDocumentTypeList.SupportsAmendment(ElectronicDocumentTypeList.Codes._5SC));
			Assert(!ElectronicDocumentTypeList.SupportsAmendment(ElectronicDocumentTypeList.Codes._105));
			Assert(ElectronicDocumentTypeList.SupportsAmendment(ElectronicDocumentTypeList.Codes._DHR));
			Assert(!ElectronicDocumentTypeList.SupportsAmendment(ElectronicDocumentTypeList.Codes._5AA));
			Assert(ElectronicDocumentTypeList.SupportsAmendment(ElectronicDocumentTypeList.Codes._5DQ));
			Assert(!ElectronicDocumentTypeList.SupportsAmendment(ElectronicDocumentTypeList.Codes._5DR));
			Assert(ElectronicDocumentTypeList.SupportsAmendment(ElectronicDocumentTypeList.Codes._5DP));
			Assert(!ElectronicDocumentTypeList.SupportsAmendment(ElectronicDocumentTypeList.Codes._5DS));
		}

		public void TestSupportsCancellation()
		{
			Assert("830 - export dec, 5AS is a cancellation", ElectronicDocumentTypeList.SupportsCancellation(ElectronicDocumentTypeList.Codes._830));
			Assert("5WN", !ElectronicDocumentTypeList.SupportsCancellation(ElectronicDocumentTypeList.Codes._5WN));
			Assert("929 - import dec, 5BF is a cancellation", ElectronicDocumentTypeList.SupportsCancellation(ElectronicDocumentTypeList.Codes._929));
			Assert("929 - import dec, 5BF is a cancellation", !ElectronicDocumentTypeList.SupportsCancellation(ElectronicDocumentTypeList.Codes._5BF));
			Assert("929 - import dec, 5FE is an amendment", !ElectronicDocumentTypeList.SupportsCancellation(ElectronicDocumentTypeList.Codes._5FE));
			Assert(!ElectronicDocumentTypeList.SupportsCancellation(ElectronicDocumentTypeList.Codes._5BA));
			Assert(!ElectronicDocumentTypeList.SupportsCancellation(ElectronicDocumentTypeList.Codes._008));
			Assert(!ElectronicDocumentTypeList.SupportsCancellation(ElectronicDocumentTypeList.Codes._5SC));
			Assert(!ElectronicDocumentTypeList.SupportsCancellation(ElectronicDocumentTypeList.Codes._105));
			Assert(!ElectronicDocumentTypeList.SupportsCancellation(ElectronicDocumentTypeList.Codes._DHR));
			Assert(!ElectronicDocumentTypeList.SupportsCancellation(ElectronicDocumentTypeList.Codes._5AA));
			Assert(ElectronicDocumentTypeList.SupportsCancellation(ElectronicDocumentTypeList.Codes._5DQ));
			Assert("This message itself is a cancellation", !ElectronicDocumentTypeList.SupportsCancellation(ElectronicDocumentTypeList.Codes._5DR));
			Assert(ElectronicDocumentTypeList.SupportsCancellation(ElectronicDocumentTypeList.Codes._5DP));
			Assert("This message itself is a cancellation", !ElectronicDocumentTypeList.SupportsCancellation(ElectronicDocumentTypeList.Codes._5DS));
		}

		public void TestIsAmendment()
		{
			Assert(!ElectronicDocumentTypeList.IsAmendment(ElectronicDocumentTypeList.Codes._830));
			Assert(!ElectronicDocumentTypeList.IsAmendment(ElectronicDocumentTypeList.Codes._929));
			Assert(!ElectronicDocumentTypeList.IsAmendment(ElectronicDocumentTypeList.Codes._5BA));
			Assert(!ElectronicDocumentTypeList.IsAmendment(ElectronicDocumentTypeList.Codes._5SC));
			Assert(!ElectronicDocumentTypeList.IsAmendment(ElectronicDocumentTypeList.Codes._DHR));
			Assert(!ElectronicDocumentTypeList.IsAmendment(ElectronicDocumentTypeList.Codes._5DQ));
			Assert(!ElectronicDocumentTypeList.IsAmendment(ElectronicDocumentTypeList.Codes._5DP));

			Assert(ElectronicDocumentTypeList.IsAmendment(ElectronicDocumentTypeList.Codes._5AS));
			Assert(ElectronicDocumentTypeList.IsAmendment(ElectronicDocumentTypeList.Codes._5FE));
			Assert(ElectronicDocumentTypeList.IsAmendment(ElectronicDocumentTypeList.Codes._5BB));
			Assert(ElectronicDocumentTypeList.IsAmendment(ElectronicDocumentTypeList.Codes._105));
			Assert(ElectronicDocumentTypeList.IsAmendment(ElectronicDocumentTypeList.Codes._DHS));
			Assert(ElectronicDocumentTypeList.IsAmendment(ElectronicDocumentTypeList.Codes._5DS));
			Assert(ElectronicDocumentTypeList.IsAmendment(ElectronicDocumentTypeList.Codes._5DR));
		}

		public void TestIsCancellation()
		{
			Assert(!ElectronicDocumentTypeList.IsCancellation(ElectronicDocumentTypeList.Codes._830));
			Assert(!ElectronicDocumentTypeList.IsCancellation(ElectronicDocumentTypeList.Codes._929));
			Assert(!ElectronicDocumentTypeList.IsCancellation(ElectronicDocumentTypeList.Codes._5BA));
			Assert(!ElectronicDocumentTypeList.IsCancellation(ElectronicDocumentTypeList.Codes._5SC));
			Assert(!ElectronicDocumentTypeList.IsCancellation(ElectronicDocumentTypeList.Codes._DHR));
			Assert(!ElectronicDocumentTypeList.IsCancellation(ElectronicDocumentTypeList.Codes._5DQ));
			Assert(!ElectronicDocumentTypeList.IsCancellation(ElectronicDocumentTypeList.Codes._5DP));

			Assert(ElectronicDocumentTypeList.IsCancellation(ElectronicDocumentTypeList.Codes._DKJ));
			Assert(!ElectronicDocumentTypeList.IsCancellation(ElectronicDocumentTypeList.Codes._5FE));
			Assert(!ElectronicDocumentTypeList.IsCancellation(ElectronicDocumentTypeList.Codes._5BB));
			Assert(!ElectronicDocumentTypeList.IsCancellation(ElectronicDocumentTypeList.Codes._105));
			Assert(!ElectronicDocumentTypeList.IsCancellation(ElectronicDocumentTypeList.Codes._DHS));
			Assert(ElectronicDocumentTypeList.IsCancellation(ElectronicDocumentTypeList.Codes._5DS));
			Assert(ElectronicDocumentTypeList.IsCancellation(ElectronicDocumentTypeList.Codes._5DR));
			Assert(ElectronicDocumentTypeList.IsCancellation(ElectronicDocumentTypeList.Codes._5BF));
		}

		public void TestGetOriginalType()
		{
			AssertEquals("830 itself is an origianl type so no original type exists", "", ElectronicDocumentTypeList.GetOriginalType(ElectronicDocumentTypeList.Codes._830));
			AssertEquals(ElectronicDocumentTypeList.Codes._830, ElectronicDocumentTypeList.GetOriginalType(ElectronicDocumentTypeList.Codes._5AS));
			AssertEquals(ElectronicDocumentTypeList.Codes._830, ElectronicDocumentTypeList.GetOriginalType(ElectronicDocumentTypeList.Codes._DKJ));
			AssertEquals(ElectronicDocumentTypeList.Codes._929, ElectronicDocumentTypeList.GetOriginalType(ElectronicDocumentTypeList.Codes._5FE));
			AssertEquals(ElectronicDocumentTypeList.Codes._929, ElectronicDocumentTypeList.GetOriginalType(ElectronicDocumentTypeList.Codes._5BF));
			AssertEquals(ElectronicDocumentTypeList.Codes._5BA, ElectronicDocumentTypeList.GetOriginalType(ElectronicDocumentTypeList.Codes._5BB));
			AssertEquals(ElectronicDocumentTypeList.Codes._5SC, ElectronicDocumentTypeList.GetOriginalType(ElectronicDocumentTypeList.Codes._105));
			AssertEquals(ElectronicDocumentTypeList.Codes._DHR, ElectronicDocumentTypeList.GetOriginalType(ElectronicDocumentTypeList.Codes._DHS));
			AssertEquals(ElectronicDocumentTypeList.Codes._5DQ, ElectronicDocumentTypeList.GetOriginalType(ElectronicDocumentTypeList.Codes._5DS));
			AssertEquals(ElectronicDocumentTypeList.Codes._5DP, ElectronicDocumentTypeList.GetOriginalType(ElectronicDocumentTypeList.Codes._5DR));
		}

		public void TestIsSupplementaryOutgoingMessage()
		{
			Assert(!ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._830));
			Assert(!ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._5AS));
			Assert(ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._5AC));

			Assert(!ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._5DP));
			Assert(!ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._5DQ));
			Assert(!ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._5DR));
			Assert(!ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._5DS));
			Assert(ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._DF3));

			Assert(!ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._929));
			Assert(!ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._5FE));
			Assert(ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._934));
			Assert(ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._5FN));
			Assert(ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._5SC));
			Assert(ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._105));
			Assert(ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._DHR));
			Assert(ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._DHS));
			Assert(!ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._5BF));
			Assert(ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._5GW));
			Assert(ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._5BD));
			Assert(ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._5SG));
			Assert(ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._5SI));
			Assert(!ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._008));
			Assert(ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._5BA));
			Assert(ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._5BB));
			Assert(!ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._5SM));
			Assert(ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._5TE));
			Assert(ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._5TM));
			Assert(ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._5UL));
			Assert(ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._D72));
			Assert(!ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._D87));
			Assert(!ElectronicDocumentTypeList.IsSupplementaryOutgoingMessage(ElectronicDocumentTypeList.Codes._008));
		}

		public void TestIsOriginalOrSupplementaryOriginalMessage()
		{
			Assert(ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._830));
			Assert(!ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._5AS));
			Assert(ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._5AC));

			Assert(ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._5DP));
			Assert(ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._5DQ));
			Assert(!ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._5DR));
			Assert(!ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._5DS));
			Assert(ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._DF3));

			Assert(ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._929));
			Assert(!ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._5FE));
			Assert(ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._934));
			Assert(ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._5FN));
			Assert(ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._5SC));
			Assert(!ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._105));
			Assert(ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._DHR));
			Assert(!ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._DHS));
			Assert(!ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._5BF));
			Assert(ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._5GW));
			Assert(ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._5BD));
			Assert(ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._5SG));
			Assert(ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._5SI));
			Assert(ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._008));
			Assert(ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._5BA));
			Assert(!ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._5BB));
			Assert(ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._5SM));
			Assert(ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._5TE));
			Assert(ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._5TM));
			Assert(ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._5UL));
			Assert(ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._D72));
			Assert(ElectronicDocumentTypeList.IsOriginalOrSupplementaryOriginalMessage(ElectronicDocumentTypeList.Codes._D87));
		}

		public void TestGetMainOriginalMessageTypeFor()
		{
			AssertEquals(string.Empty, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._830));
			AssertEquals(string.Empty, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._5AS));
			AssertEquals(ElectronicDocumentTypeList.Codes._830, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._5AC));

			AssertEquals(string.Empty, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._5DP));
			AssertEquals(string.Empty, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._5DQ));
			AssertEquals(string.Empty, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._5DR));
			AssertEquals(string.Empty, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._5DS));
			AssertEquals(ElectronicDocumentTypeList.Codes._5DQ, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._DF3));

			AssertEquals(string.Empty, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._929));
			AssertEquals(string.Empty, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._5FE));
			AssertEquals(ElectronicDocumentTypeList.Codes._929, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._934));
			AssertEquals(ElectronicDocumentTypeList.Codes._929, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._5FN));
			AssertEquals(ElectronicDocumentTypeList.Codes._929, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._5SC));
			AssertEquals(string.Empty, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._105));
			AssertEquals(ElectronicDocumentTypeList.Codes._929, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._DHR));
			AssertEquals(string.Empty, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._DHS));
			AssertEquals(string.Empty, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._5BF));
			AssertEquals(ElectronicDocumentTypeList.Codes._929, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._5GW));
			AssertEquals(ElectronicDocumentTypeList.Codes._929, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._5BD));
			AssertEquals(ElectronicDocumentTypeList.Codes._929, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._5SG));
			AssertEquals(ElectronicDocumentTypeList.Codes._929, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._5SI));
			AssertEquals(string.Empty, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._008));
			AssertEquals(ElectronicDocumentTypeList.Codes._929, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._5BA));
			AssertEquals(string.Empty, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._5BB));
			AssertEquals(string.Empty, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._5SM));
			AssertEquals(ElectronicDocumentTypeList.Codes._929, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._5TE));
			AssertEquals(ElectronicDocumentTypeList.Codes._929, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._5TM));
			AssertEquals(ElectronicDocumentTypeList.Codes._929, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._5UL));
			AssertEquals(ElectronicDocumentTypeList.Codes._929, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._D72));
			AssertEquals(string.Empty, ElectronicDocumentTypeList.GetMainOriginalMessageTypeFor(ElectronicDocumentTypeList.Codes._D87));
		}

		public void TestCanSendBeforeDeclarationIsAccepted()
		{
			Assert(!ElectronicDocumentTypeList.CanSendBeforeDeclarationIsAccepted(ElectronicDocumentTypeList.Codes._5UL));
			Assert(ElectronicDocumentTypeList.CanSendBeforeDeclarationIsAccepted(ElectronicDocumentTypeList.Codes._934));
			Assert(!ElectronicDocumentTypeList.CanSendBeforeDeclarationIsAccepted(ElectronicDocumentTypeList.Codes._5TM));
		}

		public void TestGetMainMessageTypeReqCustomsReviewMessage()
		{
			var result = ElectronicDocumentTypeList.GetMainMessageTypeLeadingToCustomsReviewMessage(ElectronicDocumentTypeList.Codes._5DR);
			AssertEquals(1, result.Length);
			AssertEquals("To send 5DR, RR3 should be there for 5DP", ElectronicDocumentTypeList.Codes._5DP, result[0]);

			result = ElectronicDocumentTypeList.GetMainMessageTypeLeadingToCustomsReviewMessage(ElectronicDocumentTypeList.Codes._5DS);
			AssertEquals(1, result.Length);
			AssertEquals("To send 5DS, RR3 should be there for 5DQ", ElectronicDocumentTypeList.Codes._5DQ, result[0]);

			result = ElectronicDocumentTypeList.GetMainMessageTypeLeadingToCustomsReviewMessage(ElectronicDocumentTypeList.Codes._DF3);
			AssertEquals(4, result.Length);
			AssertEquals(ElectronicDocumentTypeList.Codes._5DP, result[0]);
			AssertEquals(ElectronicDocumentTypeList.Codes._5DQ, result[1]);
			AssertEquals(ElectronicDocumentTypeList.Codes._5DS, result[2]);
			AssertEquals(ElectronicDocumentTypeList.Codes._5DR, result[3]);

			result = ElectronicDocumentTypeList.GetMainMessageTypeLeadingToCustomsReviewMessage(ElectronicDocumentTypeList.Codes._5FN);
			AssertEquals(2, result.Length);
			AssertEquals(ElectronicDocumentTypeList.Codes._5FE, result[0]);
			AssertEquals(ElectronicDocumentTypeList.Codes._5BF, result[1]);

			result = ElectronicDocumentTypeList.GetMainMessageTypeLeadingToCustomsReviewMessage(ElectronicDocumentTypeList.Codes._5SC);
			AssertEquals(2, result.Length);
			AssertEquals(ElectronicDocumentTypeList.Codes._5FE, result[0]);
			AssertEquals(ElectronicDocumentTypeList.Codes._5BF, result[1]);

			result = ElectronicDocumentTypeList.GetMainMessageTypeLeadingToCustomsReviewMessage(ElectronicDocumentTypeList.Codes._DHR);
			AssertEquals(2, result.Length);
			AssertEquals(ElectronicDocumentTypeList.Codes._5FE, result[0]);
			AssertEquals(ElectronicDocumentTypeList.Codes._5BF, result[1]);

			result = ElectronicDocumentTypeList.GetMainMessageTypeLeadingToCustomsReviewMessage(ElectronicDocumentTypeList.Codes._5BD);
			AssertEquals(2, result.Length);
			AssertEquals(ElectronicDocumentTypeList.Codes._5FE, result[0]);
			AssertEquals(ElectronicDocumentTypeList.Codes._5BF, result[1]);

			result = ElectronicDocumentTypeList.GetMainMessageTypeLeadingToCustomsReviewMessage(ElectronicDocumentTypeList.Codes._5SG);
			AssertEquals(2, result.Length);
			AssertEquals(ElectronicDocumentTypeList.Codes._5FE, result[0]);
			AssertEquals(ElectronicDocumentTypeList.Codes._5BF, result[1]);

			result = ElectronicDocumentTypeList.GetMainMessageTypeLeadingToCustomsReviewMessage(ElectronicDocumentTypeList.Codes._5SI);
			AssertEquals(2, result.Length);
			AssertEquals(ElectronicDocumentTypeList.Codes._5FE, result[0]);
			AssertEquals(ElectronicDocumentTypeList.Codes._5BF, result[1]);

			result = ElectronicDocumentTypeList.GetMainMessageTypeLeadingToCustomsReviewMessage(ElectronicDocumentTypeList.Codes._5BA);
			AssertEquals(2, result.Length);
			AssertEquals(ElectronicDocumentTypeList.Codes._5FE, result[0]);
			AssertEquals(ElectronicDocumentTypeList.Codes._5BF, result[1]);

			result = ElectronicDocumentTypeList.GetMainMessageTypeLeadingToCustomsReviewMessage(ElectronicDocumentTypeList.Codes._5TE);
			AssertEquals(2, result.Length);
			AssertEquals(ElectronicDocumentTypeList.Codes._5FE, result[0]);
			AssertEquals(ElectronicDocumentTypeList.Codes._5BF, result[1]);

			result = ElectronicDocumentTypeList.GetMainMessageTypeLeadingToCustomsReviewMessage(ElectronicDocumentTypeList.Codes._5TM);
			AssertEquals(2, result.Length);
			AssertEquals(ElectronicDocumentTypeList.Codes._5FE, result[0]);
			AssertEquals(ElectronicDocumentTypeList.Codes._5BF, result[1]);

			result = ElectronicDocumentTypeList.GetMainMessageTypeLeadingToCustomsReviewMessage(ElectronicDocumentTypeList.Codes._5UL);
			AssertEquals(2, result.Length);
			AssertEquals(ElectronicDocumentTypeList.Codes._5FE, result[0]);
			AssertEquals(ElectronicDocumentTypeList.Codes._5BF, result[1]);

			result = ElectronicDocumentTypeList.GetMainMessageTypeLeadingToCustomsReviewMessage(ElectronicDocumentTypeList.Codes._D72);
			AssertEquals(2, result.Length);
			AssertEquals(ElectronicDocumentTypeList.Codes._5FE, result[0]);
			AssertEquals(ElectronicDocumentTypeList.Codes._5BF, result[1]);

			result = ElectronicDocumentTypeList.GetMainMessageTypeLeadingToCustomsReviewMessage(ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(1, result.Length);
			AssertEquals(ElectronicDocumentTypeList.Codes._5BF, result[0]);

			result = ElectronicDocumentTypeList.GetMainMessageTypeLeadingToCustomsReviewMessage(ElectronicDocumentTypeList.Codes._5BF);
			AssertEquals(1, result.Length);
			AssertEquals(ElectronicDocumentTypeList.Codes._5FE, result[0]);
		}

		public void TestGetOutgoingMessagesToReceiveCustomsReviewMessage()
		{
			var outgoingMessages = ElectronicDocumentTypeList.GetOutgoingMessagesToReceiveCustomsReviewMessage(ElectronicDocumentTypeList.Codes._RR3);
			AssertEquals(4, outgoingMessages.Length);
			foreach (var messageType in outgoingMessages)
			{
				AssertEquals(ElectronicDocumentTypeList.Codes._RR3, ElectronicDocumentTypeList.MandatoryCustomsApprovalMessageFor(messageType));
			}

			outgoingMessages = ElectronicDocumentTypeList.GetOutgoingMessagesToReceiveCustomsReviewMessage(ElectronicDocumentTypeList.Codes._5DT);
			AssertEquals(2, outgoingMessages.Length);
			foreach (var messageType in outgoingMessages)
			{
				AssertEquals(ElectronicDocumentTypeList.Codes._5DT, ElectronicDocumentTypeList.MandatoryCustomsApprovalMessageFor(messageType));
			}

			outgoingMessages = ElectronicDocumentTypeList.GetOutgoingMessagesToReceiveCustomsReviewMessage(ElectronicDocumentTypeList.Codes._5FK);
			AssertEquals(1, outgoingMessages.Length);
			foreach (var messageType in outgoingMessages)
			{
				AssertEquals(ElectronicDocumentTypeList.Codes._5FK, ElectronicDocumentTypeList.MandatoryCustomsApprovalMessageFor(messageType));
			}

			outgoingMessages = ElectronicDocumentTypeList.GetOutgoingMessagesToReceiveCustomsReviewMessage(ElectronicDocumentTypeList.Codes._106);
			AssertEquals(2, outgoingMessages.Length);
			foreach (var messageType in outgoingMessages)
			{
				AssertEquals(ElectronicDocumentTypeList.Codes._106, ElectronicDocumentTypeList.MandatoryCustomsApprovalMessageFor(messageType));
			}

			outgoingMessages = ElectronicDocumentTypeList.GetOutgoingMessagesToReceiveCustomsReviewMessage(ElectronicDocumentTypeList.Codes._5BG);
			AssertEquals(1, outgoingMessages.Length);
			foreach (var messageType in outgoingMessages)
			{
				AssertEquals(ElectronicDocumentTypeList.Codes._5BG, ElectronicDocumentTypeList.MandatoryCustomsApprovalMessageFor(messageType));
			}

			outgoingMessages = ElectronicDocumentTypeList.GetOutgoingMessagesToReceiveCustomsReviewMessage(ElectronicDocumentTypeList.Codes._5SH);
			AssertEquals(1, outgoingMessages.Length);
			foreach (var messageType in outgoingMessages)
			{
				AssertEquals(ElectronicDocumentTypeList.Codes._5SH, ElectronicDocumentTypeList.MandatoryCustomsApprovalMessageFor(messageType));
			}

			outgoingMessages = ElectronicDocumentTypeList.GetOutgoingMessagesToReceiveCustomsReviewMessage(ElectronicDocumentTypeList.Codes._5BC);
			AssertEquals(1, outgoingMessages.Length);
			foreach (var messageType in outgoingMessages)
			{
				AssertEquals(ElectronicDocumentTypeList.Codes._5BC, ElectronicDocumentTypeList.MandatoryCustomsApprovalMessageFor(messageType));
			}

			outgoingMessages = ElectronicDocumentTypeList.GetOutgoingMessagesToReceiveCustomsReviewMessage(ElectronicDocumentTypeList.Codes._5TF);
			AssertEquals(1, outgoingMessages.Length);
			foreach (var messageType in outgoingMessages)
			{
				AssertEquals(ElectronicDocumentTypeList.Codes._5TF, ElectronicDocumentTypeList.MandatoryCustomsApprovalMessageFor(messageType));
			}

			outgoingMessages = ElectronicDocumentTypeList.GetOutgoingMessagesToReceiveCustomsReviewMessage(ElectronicDocumentTypeList.Codes._RCA);
			AssertEquals(1, outgoingMessages.Length);
			foreach (var messageType in outgoingMessages)
			{
				AssertEquals(ElectronicDocumentTypeList.Codes._RCA, ElectronicDocumentTypeList.MandatoryCustomsApprovalMessageFor(messageType));
			}

			outgoingMessages = ElectronicDocumentTypeList.GetOutgoingMessagesToReceiveCustomsReviewMessage(ElectronicDocumentTypeList.Codes._R43);
			AssertEquals(1, outgoingMessages.Length);
			foreach (var messageType in outgoingMessages)
			{
				AssertEquals(ElectronicDocumentTypeList.Codes._R43, ElectronicDocumentTypeList.MandatoryCustomsApprovalMessageFor(messageType));
			}
		}

		public void TestIsExportOrLocalExportOutgoingMessage()
		{
			Assert(ElectronicDocumentTypeList.IsExportOrLocalExportOutgoingMessage(ElectronicDocumentTypeList.Codes._830));
			Assert(ElectronicDocumentTypeList.IsExportOrLocalExportOutgoingMessage(ElectronicDocumentTypeList.Codes._5AS));
			Assert(ElectronicDocumentTypeList.IsExportOrLocalExportOutgoingMessage(ElectronicDocumentTypeList.Codes._DKJ));
			Assert(ElectronicDocumentTypeList.IsExportOrLocalExportOutgoingMessage(ElectronicDocumentTypeList.Codes._5DP));
			Assert(ElectronicDocumentTypeList.IsExportOrLocalExportOutgoingMessage(ElectronicDocumentTypeList.Codes._5DQ));
			Assert(ElectronicDocumentTypeList.IsExportOrLocalExportOutgoingMessage(ElectronicDocumentTypeList.Codes._5DR));
			Assert(ElectronicDocumentTypeList.IsExportOrLocalExportOutgoingMessage(ElectronicDocumentTypeList.Codes._5DS));
			Assert(ElectronicDocumentTypeList.IsExportOrLocalExportOutgoingMessage(ElectronicDocumentTypeList.Codes._5AC));
			Assert(ElectronicDocumentTypeList.IsExportOrLocalExportOutgoingMessage(ElectronicDocumentTypeList.Codes._DF3));
			Assert(!ElectronicDocumentTypeList.IsExportOrLocalExportOutgoingMessage(ElectronicDocumentTypeList.Codes._929));
			Assert(!ElectronicDocumentTypeList.IsExportOrLocalExportOutgoingMessage(ElectronicDocumentTypeList.Codes._5GW));
		}

		public void TestGetMessageTypeSettingEntryToCancellationApprovedByCustoms()
		{
			AssertEquals(ElectronicDocumentTypeList.Codes._RR3, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationApprovedByCustoms(ElectronicDocumentTypeList.Codes._5DQ));
			AssertEquals(ElectronicDocumentTypeList.Codes._RR3, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationApprovedByCustoms(ElectronicDocumentTypeList.Codes._5DP));
			AssertEquals(ElectronicDocumentTypeList.Codes._5DT, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationApprovedByCustoms(JobMessageTypeList.Codes.Export));
			AssertEquals(ElectronicDocumentTypeList.Codes._5BG, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationApprovedByCustoms(JobMessageTypeList.Codes.Import));
			AssertEquals(ElectronicDocumentTypeList.Codes._5SN, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationApprovedByCustoms(ElectronicDocumentTypeList.Codes._5SM));
			AssertEquals(string.Empty, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationApprovedByCustoms(ElectronicDocumentTypeList.Codes._830));
			AssertEquals(string.Empty, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationApprovedByCustoms(ElectronicDocumentTypeList.Codes._929));
		}

		public void TestGetMessageTypeSettingEntryToCancellationByCustoms()
		{
			AssertEquals(ElectronicDocumentTypeList.Codes._RR3, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationByCustoms(ElectronicDocumentTypeList.Codes._5DQ));
			AssertEquals(ElectronicDocumentTypeList.Codes._RR3, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationByCustoms(ElectronicDocumentTypeList.Codes._5DP));
			AssertEquals(string.Empty, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationByCustoms(JobMessageTypeList.Codes.Export));
			AssertEquals(ElectronicDocumentTypeList.Codes._023, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationByCustoms(JobMessageTypeList.Codes.Import));
			AssertEquals(string.Empty, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationByCustoms(ElectronicDocumentTypeList.Codes._5SM));
			AssertEquals(string.Empty, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationByCustoms(ElectronicDocumentTypeList.Codes._830));
			AssertEquals(string.Empty, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationByCustoms(ElectronicDocumentTypeList.Codes._929));
		}
	}
}
