using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.CommissionManagement.Business.Test
{
	[TestedType(typeof(AccAmbiguousCommission))]
	internal class AccAmbiguousCommissionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAC0_CA0_SelectedAgreement_ConcurrencyPolicy()
		{
			var ambiguousCommission = Factory.New<AccAmbiguousCommission>();
			AssertEquals(ConcurrencyPolicy.Strict, ambiguousCommission.AC0_CA0_SelectedAgreementInfo.ConcurrencyPolicy);
		}
	}
}
