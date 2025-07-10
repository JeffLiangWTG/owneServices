using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class OthersElementStrategyTest : TestCaseWithFactory
	{
		public void TestIsMandatory()
		{
			AssertEquals("IsMandatory", false, new OthersElementStrategy().IsMandatory);
			AssertEquals("IsMandatory", false, new GTINElementStrategy().IsMandatory);
			AssertEquals("IsMandatory", false, new CASElementStrategy().IsMandatory);
		}

		public void TestIsMergeKey()
		{
			AssertEquals("IsMergeKey", false, new OthersElementStrategy().IsMergeKey);
			AssertEquals("IsMergeKey", true, new GTINElementStrategy().IsMergeKey);
			AssertEquals("IsMergeKey", true, new CASElementStrategy().IsMergeKey);
		}
	}
}
