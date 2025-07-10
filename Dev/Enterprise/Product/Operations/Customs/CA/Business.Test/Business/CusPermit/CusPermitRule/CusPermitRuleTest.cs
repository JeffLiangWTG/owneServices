using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusPermitRule))]
	sealed class CusPermitRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var permitHeader = Factory.New<CusPermitHeader>();
			var rule = permitHeader.CusPermitRules.AddNew();

			AssertEquals("Default Rule Code", CAPermitRuleCodeList.Codes.TAR, rule.CPR_RuleCode);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var permitHeader = Factory.New<CusPermitHeader>();
			return permitHeader.CusPermitRules.AddNew();
		}

		#endregion
	}
}
