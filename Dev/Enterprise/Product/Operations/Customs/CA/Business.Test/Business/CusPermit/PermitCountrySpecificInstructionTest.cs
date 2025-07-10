using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class PermitCountrySpecificInstructionTest : TestCaseWithFactory
	{
		public void TestGetTypeList()
		{
			AssertType<PermitTypeList>(countrySpecificInstruction.GetTypeList());
		}

		public void TestGetRuleCodeList()
		{
			AssertType<CAPermitRuleCodeList>(countrySpecificInstruction.GetRuleCodeListForModule());
			AssertType<CAPermitRuleCodeList>(countrySpecificInstruction.GetRuleCodeList("", ""));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var permitHeader = Factory.New<CusPermitHeader>();
			countrySpecificInstruction = (PermitCountrySpecificInstruction)permitHeader.CountrySpecificInstruction;
		}

		PermitCountrySpecificInstruction countrySpecificInstruction;

		#endregion

	}
}
