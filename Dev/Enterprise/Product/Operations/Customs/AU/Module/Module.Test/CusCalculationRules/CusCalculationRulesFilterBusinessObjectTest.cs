using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(CusCalculationRulesFilterBusinessObject))]
	sealed class CusCalculationRulesFilterBusinessObjectTest : Customs.Module.Testing.CusCalculationRulesFilterBusinessObjectTest
	{
		public override void TestLookups()
		{
			AssertEquals("RuleType of correct type", typeof(CusCalculationRulesFilterLookups), filterBO.Lookups.GetType());
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => filterBO;

		protected override void SetUp()
		{
			base.SetUp();
			filterBO = new CusCalculationRulesFilterBusinessObject();
			filterBO.QueryObjectType = typeof(CusCalculationRule);
		}

		CusCalculationRulesFilterBusinessObject filterBO;
	}
}
