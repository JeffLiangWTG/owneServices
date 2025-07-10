using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(ARCashAdvanceFilterBusinessObject))]
	public class ARCashAdvanceFilterBusinessObjectTest : CashAdvanceFilterBusinessObjectTest
	{
		protected override string LedgerType { set; get; } = LedgerTypes.AccountsReceivable;

		public void TestCollectionContainsARCashAdvancesOnly()
		{
			CashAdvanceRequestHeader2.CAH_Ledger = LedgerTypes.AccountsPayable;
			FilterCollection.Load(FilterBO.Filter);
			AssertEquals(1, FilterCollection.Count);
			AssertCollectionContains(CashAdvanceRequestHeader1, FilterCollection);
			AssertCollectionNotContains(CashAdvanceRequestHeader2, FilterCollection);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ARCashAdvanceFilterBusinessObject();
		}
	}
}
