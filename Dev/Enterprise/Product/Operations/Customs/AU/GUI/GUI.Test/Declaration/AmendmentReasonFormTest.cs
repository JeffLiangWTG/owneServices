using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(AmendmentReasonForm))]
	sealed class AmendmentReasonFormTest : Customs.GUI.Testing.AmendmentReasonFormTest
	{
		protected override AmendmentWithdrawalReason GetNewAmendmentWithdrawalReason() => new CMRAmendmentWithdrawalReason();
	}
}
