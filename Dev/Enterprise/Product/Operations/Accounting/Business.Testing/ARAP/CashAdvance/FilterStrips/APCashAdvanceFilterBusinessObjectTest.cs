using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(APCashAdvanceFilterBusinessObject))]
	public class APCashAdvanceFilterBusinessObjectTest : CashAdvanceFilterBusinessObjectTest
	{
		protected override string LedgerType { set; get; } = LedgerTypes.AccountsPayable;

		public void TestCollection_ShouldOnlyContainAPCashAdvances()
		{
			CashAdvanceRequestHeader2.CAH_Ledger = LedgerTypes.AccountsReceivable;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals(1, FilterCollection.Count);
			AssertCollectionContains(CashAdvanceRequestHeader1, FilterCollection);
			AssertCollectionNotContains(CashAdvanceRequestHeader2, FilterCollection);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new APCashAdvanceFilterBusinessObject();
		}
	}
}
