using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class WarehouseWrapperTest : Customs.Business.Testing.DataProviderTestCase<WarehouseWrapper>
	{
		protected override WarehouseWrapper GetProvider()
		{
			var permit = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			permit.CPH_Number = "12314";
			var rule = permit.CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = "OTH";
			rule.CPR_ValueFrom = "OTH";
			var rule2 = permit.CusAuthorisationRules.AddNew();
			rule2.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.USE;
			rule2.CPR_ValueFrom = "ESU";
			return WarehouseWrapper.New(permit);
		}

		public void TestType()
		{
			AssertEquals("Type should be equal to CPR_ValueFrom where CPR_RuleCode = USE.", "ESU", Provider.Type);
		}

		public void TestCcQualifier()
		{
			AssertEquals("CcQualifier mapping is not done yet and should be empty.", ZString.Empty, Provider.CcQualifier);
		}

		public void TestIdentifier()
		{
			AssertEquals("Identifier should be equal to CPH_Number.", "12314", Provider.Identifier);
		}
	}
}
