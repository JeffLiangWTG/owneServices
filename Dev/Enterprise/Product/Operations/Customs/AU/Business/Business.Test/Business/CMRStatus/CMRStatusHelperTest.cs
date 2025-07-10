using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CMRStatusHelperTest : TestCaseWithFactory
	{
		public void TestNoMessageIsCurrent()
		{
			Assert(CMRStatusHelper.NoMessageIsCurrent(""));
			Assert(!CMRStatusHelper.NoMessageIsCurrent("ZZZ"));
			Assert(!CMRStatusHelper.NoMessageIsCurrent(CMRBaseStatuses.Codes.OriginalAccepted));
			Assert(!CMRStatusHelper.NoMessageIsCurrent(CMRBaseStatuses.Codes.AwaitingResponseToOriginal));
			Assert(!CMRStatusHelper.NoMessageIsCurrent(CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal));
			Assert(!CMRStatusHelper.NoMessageIsCurrent(CMRBaseStatuses.Codes.AmendmentRejected));
			Assert(!CMRStatusHelper.NoMessageIsCurrent(CMRBaseStatuses.Codes.WithdrawalRejected));
			Assert(CMRStatusHelper.NoMessageIsCurrent(CMRBaseStatuses.Codes.WithdrawalAccepted));
			Assert(CMRStatusHelper.NoMessageIsCurrent(CMRBaseStatuses.Codes.NotSent));
			Assert(CMRStatusHelper.NoMessageIsCurrent(CMRBaseStatuses.Codes.OriginalRejected));
		}

		public void TestCanDelete()
		{
			AssertEquals(true, CMRStatusHelper.CanDelete(""));
			AssertEquals(true, CMRStatusHelper.CanDelete("ZZZ"));
			AssertEquals(false, CMRStatusHelper.CanDelete(CMRBaseStatuses.Codes.OriginalAccepted));
			AssertEquals(false, CMRStatusHelper.CanDelete(CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal));
			AssertEquals(true, CMRStatusHelper.CanDelete(CMRBaseStatuses.Codes.WithdrawalAccepted));
			AssertEquals(true, CMRStatusHelper.CanDelete(CMRBaseStatuses.Codes.NotSent));
			AssertEquals(true, CMRStatusHelper.CanDelete(CMRBaseStatuses.Codes.OriginalRejected));
		}

		public void TestGetAcceptableStatusesForKeyValueChange()
		{
			var list1 = CMRStatusHelper.GetAcceptableStatusesForKeyValueChange(Factory);
			AssertEquals(3, list1.Count);
			AssertEquals(CMRBaseStatuses.Descriptions.WithdrawalAccepted, list1.GetDescriptionFromCode(CMRBaseStatuses.Codes.WithdrawalAccepted));
			AssertEquals(CMRBaseStatuses.Descriptions.OriginalRejected, list1.GetDescriptionFromCode(CMRBaseStatuses.Codes.OriginalRejected));
			AssertEquals(CMRBaseStatuses.Descriptions.NotSent, list1.GetDescriptionFromCode(CMRBaseStatuses.Codes.NotSent));
			AssertEquals(list1, CMRStatusHelper.GetAcceptableStatusesForKeyValueChange(Factory));
		}

		public void TestIsMessagingBeingNotLodged()
		{
			AssertEquals(true, CMRStatusHelper.IsMessagingBeingNotLodged(""));
			AssertEquals(true, CMRStatusHelper.IsMessagingBeingNotLodged(CMRBaseStatuses.Codes.WithdrawalAccepted));
			AssertEquals(true, CMRStatusHelper.IsMessagingBeingNotLodged(CMRBaseStatuses.Codes.OriginalRejected));
			AssertEquals(true, CMRStatusHelper.IsMessagingBeingNotLodged(CMRBaseStatuses.Codes.NotSent));
		}
	}
}
