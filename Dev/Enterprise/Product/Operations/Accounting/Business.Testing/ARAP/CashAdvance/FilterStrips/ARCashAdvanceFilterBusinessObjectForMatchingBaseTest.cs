using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(ARCashAdvanceFilterBusinessObjectForMatchingBase))]
	public class ARCashAdvanceFilterBusinessObjectForMatchingBaseTest : CashAdvanceFilterBusinessObjectForMatchingBaseTest
	{
		protected override string LedgerType { set; get; } = LedgerTypes.AccountsReceivable;

		protected override MatchingFilterBusinessObject GetMatchingFilterBusinessObject() => new ARMatchingFilterBusinessObject();

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ARCashAdvanceFilterBusinessObjectForMatchingBase(MatchingFilterBizO as ARMatchingFilterBusinessObject);
	}
}
