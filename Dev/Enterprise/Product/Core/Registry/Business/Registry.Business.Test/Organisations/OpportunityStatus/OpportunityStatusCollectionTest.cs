using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OpportunityStatusCollection))]
	sealed class OpportunityStatusCollectionTest : CodeDescriptionBoolCollectionAbstractTest<OpportunityStatusCollection>
	{
		public void TestGetEffectiveAgreementFromCode()
		{
			var collection = Collection;
			var element1 = collection.AddNew();
			element1.Code = "AAA";
			element1.EffectiveAgreement = true;
			element1.Enabled = false;
			var element2 = collection.AddNew();
			element2.Code = "ZZZ";
			element2.EffectiveAgreement = false;
			element2.Enabled = true;

			AssertEquals(true, collection.GetEffectiveAgreementFromCode("AAA"));
			AssertEquals(false, collection.GetEffectiveAgreementFromCode("ZZZ"));
			AssertEquals(false, collection.GetEffectiveAgreementFromCode("ASD"));
		}

		public void TestGetTradeStatusFromCode()
		{
			var collection = Collection;
			var element1 = collection.AddNew();
			element1.Code = "AAA";
			element1.TradeStatus = OpportunityTradeStatus.Codes.Successful;
			var element2 = collection.AddNew();
			element2.Code = "ZZZ";
			element2.TradeStatus = OpportunityTradeStatus.Codes.Active;

			AssertEquals(OpportunityTradeStatus.Codes.Successful, collection.GetTradeStatusFromCode("AAA"));
			AssertEquals(OpportunityTradeStatus.Codes.Active, collection.GetTradeStatusFromCode("ZZZ"));
			AssertEquals(ZString.Empty, collection.GetTradeStatusFromCode("ASD"));
		}

		#region Implementation

		public override void TestDefaultBoolForNewChild()
		{
			AssertEquals(false, new OpportunityStatusCollection(false, false).AddNew().Bool);
			AssertEquals(false, new OpportunityStatusCollection(true, false).AddNew().Bool);
			AssertEquals(true, new OpportunityStatusCollection(false, true).AddNew().Bool);
			AssertEquals(true, new OpportunityStatusCollection(true, true).AddNew().Bool);
			AssertEquals(false, new OpportunityStatusCollection().AddNew().Bool);
		}

		public void TestEnabledStatuses()
		{
			var opportunities = new OpportunityStatusCollection();
			opportunities.Add("CRT", (NoResString)"Current", false, false, true, "");
			opportunities.Add("ARB", (NoResString)"Arbitrary", false, false, false, "");
			opportunities.Add("TST", (NoResString)"Test", false, false, true, "");
			int trueCount = 0;
			int falseCount = 0;

			foreach (OpportunityStatus t in opportunities)
			{
				if (t.Enabled)
				{
					trueCount++;
				}
				else if (!t.Enabled)
				{
					falseCount++;
				}
			}
			AssertEquals("1 Value should be false", 1, falseCount);
			AssertEquals("2 Values are enabled, so the count should be 2", 2, trueCount);
		}

		protected override OpportunityStatusCollection GetCollectionToTest()
		{
			return new OpportunityStatusCollection();
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OpportunityStatus();
		}

		#endregion
	}
}
