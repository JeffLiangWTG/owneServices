using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NctsGuaranteeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestApplyRuleC086()
		{
			var nctsGuarantee = Factory.New<NctsGuarantee>();
			var validation = new NctsGuaranteeValidationForTest(nctsGuarantee);
			AssertEquals("ApplyRuleC086 is not applicable", false, validation.ApplyRuleC086Exposed);
		}

		class NctsGuaranteeValidationForTest : NctsGuaranteeValidation
		{
			public NctsGuaranteeValidationForTest(NctsGuarantee cusBondDetail) : base(cusBondDetail)
			{
			}

			public bool ApplyRuleC086Exposed => base.ApplyRuleC086;
		}
	}
}
