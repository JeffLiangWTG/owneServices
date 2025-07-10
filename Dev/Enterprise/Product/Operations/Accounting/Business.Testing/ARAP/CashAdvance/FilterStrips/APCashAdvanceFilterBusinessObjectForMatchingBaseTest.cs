using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Matching.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(APCashAdvanceFilterBusinessObjectForMatchingBase))]
	public class APCashAdvanceFilterBusinessObjectForMatchingBaseTest : CashAdvanceFilterBusinessObjectForMatchingBaseTest
	{
		protected override string LedgerType { set; get; }  = LedgerTypes.AccountsPayable;

		protected override MatchingFilterBusinessObject GetMatchingFilterBusinessObject() => new APMatchingFilterBusinessObject();

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new APCashAdvanceFilterBusinessObjectForMatchingBase(MatchingFilterBizO as APMatchingFilterBusinessObject);

		protected override void SetUp()
		{
			ObjectCreator.LocalClient.OH_IsCreditor = true;
			Factory.Save();
			base.SetUp();
		}
	}
}
