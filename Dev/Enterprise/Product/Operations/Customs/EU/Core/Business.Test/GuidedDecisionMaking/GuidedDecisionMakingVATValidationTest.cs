using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class GuidedDecisionMakingVATValidationTest : BusinessObjectValidationTestCase
	{
		public void TestParent()
		{
			var gdmBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);
			var gdmVAT = new GuidedDecisionMakingVAT(gdmBasic);
			var validation = new GuidedDecisionMakingVATValidation(gdmVAT);

			AssertType<GuidedDecisionMakingVAT>("Parent of GuidedDecisionMakingVATValidation should be of type GuidedDecisionMakingVAT.", validation.Parent);
		}
	}
}
