using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CARSTAndUnderbondStatusCombinerTest : TestCase
	{
		public void TestCombinedStatus()
		{
			var combiner = new CARSTAndUnderbondStatusCombiner();
			AssertEquals("when both empty", ZString.Empty, combiner.GetCombinedStatus(ZString.Empty, ZString.Empty));
			AssertEquals("when only CARST", CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl, combiner.GetCombinedStatus(CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl, ZString.Empty));
			AssertEquals("when only Underbond", CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived, combiner.GetCombinedStatus(ZString.Empty, CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived));
			AssertEquals("prefer CARST status over underbond", CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl, combiner.GetCombinedStatus(CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl, CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived));
			AssertEquals("prefer Underbond approval over any CARST status", CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived, combiner.GetCombinedStatus(CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived));
		}
	}
}
