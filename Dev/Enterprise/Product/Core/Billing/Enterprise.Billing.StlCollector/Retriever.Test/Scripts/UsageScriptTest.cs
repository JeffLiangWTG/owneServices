using CargoWise.Application;
using Enterprise.Billing.Integration;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Integration.Billing;
using Enterprise.Integration.Licensing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Testing.Scripts
{
	[TestsSubclassesOf(typeof(UsageScript))]
	abstract class UsageScriptTest : BaseStlScriptTest
	{
		public override void TestCollectorType()
		{
			var item = ScriptToTest;
			AssertEquals("Incorrect value for property CollectorType", StlCollectorType.Custom, item.CollectorType);
		}

		public override void TestScriptTextContainsComment()
		{
			var item = ScriptToTest;
			Assert("Comment with collector details not found in script", item.ScriptText.Contains($"-- STL Collector query for FeatureCode={item.Code}, CollectorType=Custom"));
		}

		public override void TestFieldMaxSizes()
		{
			AssertEquals("FeatureCode.Length <= 3", true, ScriptToTest.Code.Length <= 3);
			AssertEquals("RoleName.Length <= 50", true, ScriptToTest.Role.Length <= 50);
			AssertEquals("ModuleName.Length <= 50", true, ScriptToTest.Module.Length <= 50);
			AssertEquals("FunctionName.Length <= 50", true, ScriptToTest.Function.Length <= 50);
			AssertEquals("FeatureName.Length <= 75", true, ScriptToTest.Feature.Length <= 75);
			AssertEquals("MaxCW1Version.Length <= 20", true, ScriptToTest.MaxCW1Version.Length <= 20);
			AssertEquals("MinCW1Version.Length <= 20", true, ScriptToTest.MinCW1Version.Length <= 20);
		}

		protected void AssertEnvironmentProperties(UsageTransaction usageTransaction)
		{
			var productKey = ObjectFactory.Get<IProductRegistration>().Key;
			AssertEquals("EnterpriseCode", productKey.EnterpriseCode, usageTransaction.EnterpriseCode);
			AssertEquals("ServerCode", productKey.ServerCode, usageTransaction.ServerCode);
			AssertEquals("Environment", productKey.DatabaseType, usageTransaction.Environment);
			AssertEquals("BranchCode", "CBR", usageTransaction.BranchCode);
			AssertEquals("CompanyCode", "WTG", usageTransaction.GetCompanyCode());
			AssertEquals("CompanyName", "Wisetech", usageTransaction.CompanyName);
		}
	}
}
