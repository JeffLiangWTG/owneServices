using System.Linq;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(GlowOpportunityStatusCollection))]
	sealed class GlowOpportunityStatusCollectionTest : CodeDescriptionBoolCollectionAbstractTest<GlowOpportunityStatusCollection>
	{
		public void TestEnabledStatuses()
		{
			var statuses = new GlowOpportunityStatusCollection();
			statuses.Add("CRT", (NoResString)"Current", true, "");
			statuses.Add("ARB", (NoResString)"Arbitrary", false, "");
			statuses.Add("TST", (NoResString)"Test", true, "");

			var statusList = statuses.Cast<GlowOpportunityStatus>();
			AssertEquals("1 Value should be false", 1, statusList.Count(opp => !opp.Bool));
			AssertEquals("2 Values are enabled, so the count should be 2", 2, statusList.Count(opp => opp.Bool));
		}

		public void TestTradeStatusesAreSet()
		{
			var statuses = new GlowOpportunityStatusCollection();
			var activeStatus = statuses.Add("CRT", (NoResString)"Current", true, OpportunityTradeStatus.Codes.Active);
			var successfulStatus = statuses.Add("ARB", (NoResString)"Arbitrary", false, OpportunityTradeStatus.Codes.Successful);
			var unsuccessfulStatus = statuses.Add("TST", (NoResString)"Test", true, OpportunityTradeStatus.Codes.Unsuccessful);
			var unprovidedStatus = statuses.Add("ABA", (NoResString)"Abandoned", true);

			AssertEquals(OpportunityTradeStatus.Codes.Active, activeStatus.TradeStatus);
			AssertEquals(OpportunityTradeStatus.Codes.Successful, successfulStatus.TradeStatus);
			AssertEquals(OpportunityTradeStatus.Codes.Unsuccessful, unsuccessfulStatus.TradeStatus);
			AssertEquals(OpportunityTradeStatus.Codes.Active, ((GlowOpportunityStatus)unprovidedStatus).TradeStatus);
		}

		#region Implementation

		public override void TestDefaultBoolForNewChild()
		{
			AssertEquals(true, new GlowOpportunityStatusCollection().AddNew().Bool);
		}

		protected override GlowOpportunityStatusCollection GetCollectionToTest()
		{
			return new GlowOpportunityStatusCollection();
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GlowOpportunityStatus();
		}

		#endregion
	}
}
