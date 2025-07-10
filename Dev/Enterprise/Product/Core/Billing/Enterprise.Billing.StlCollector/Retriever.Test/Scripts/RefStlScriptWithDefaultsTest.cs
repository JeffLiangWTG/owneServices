using System;
using CargoWise.Billing.Collectors;
using Enterprise.Integration.Billing;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts
{
	[TestsSubclassesOf(typeof(RefStlScriptWithDefaults))]
	abstract class RefStlScriptWithDefaultsTest : StlScriptTest
	{
		public override void TestCollectorType()
		{
			var item = ScriptToTest;
			AssertEquals("Incorrect value for property CollectorType", StlCollectorType.Dynamic, item.CollectorType);
		}

		public override void TestScriptTextContainsComment()
		{
			var item = ScriptToTest;
			Assert("Comment with collector details not found in script", item.ScriptText.Contains($"-- STL Collector query for FeatureCode={item.Code}, CollectorType=Dynamic"));
		}

		public override void TestFieldMaxSizes()
		{
			AssertEquals("ActiveOn <= 3", true, ScriptToTest.ActiveOn.Length <= 3);
			AssertEquals("AdditionalRefs <= 1000", true, ScriptToTest.AdditionalRefs.Length <= 1000);
			AssertEquals("BillingReference1 <= 1000", true, ScriptToTest.Reference1.Length <= 1000);
			AssertEquals("BillingReference2 <= 1000", true, ScriptToTest.Reference2.Length <= 1000);
			AssertEquals("BillingReference3 <= 1000", true, ScriptToTest.Reference3.Length <= 1000);
			AssertEquals("BillingReference4 <= 1000", true, ScriptToTest.Reference4.Length <= 1000);
			AssertEquals("BranchCode <= 1000", true, ScriptToTest.Branch.Length <= 1000);
			AssertEquals("CompanyCode <= 1000", true, ScriptToTest.Company.Length <= 1000);
			AssertEquals("CreatingUserCode <= 1000", true, ScriptToTest.User.Length <= 1000);
			AssertEquals("FeatureCode.Length <= 3", true, ScriptToTest.Code.Length <= 3);
			AssertEquals("FeatureName.Length <= 75", true, ScriptToTest.Feature.Length <= 75);
			AssertEquals("FunctionName.Length <= 50", true, ScriptToTest.Function.Length <= 50);
			AssertEquals("GuidReference.Length <= 1000", true, ScriptToTest.GuidReference.Length <= 1000);
			AssertEquals("MaxCW1Version.Length <= 20", true, ScriptToTest.MaxCW1Version.Length <= 20);
			AssertEquals("MinCW1Version.Length <= 20", true, ScriptToTest.MinCW1Version.Length <= 20);
			AssertEquals("ModuleName.Length <= 50", true, ScriptToTest.Module.Length <= 50);
			AssertEquals("RoleName.Length <= 50", true, ScriptToTest.Role.Length <= 50);
			AssertEquals("TransactionCount.Length <= 1000", true, ScriptToTest.BillableCount.Length <= 1000);
			AssertEquals("TransactionDateUtc.Length <= 1000", true, ScriptToTest.TransactionDateUtc.Length <= 1000);
		}

		override protected IStlScript ScriptToTest
		{
			get
			{
				Type[] types = Array.Empty<Type>();
				var constructorInfo = TestedTypeHelper.GetTestedType(GetType()).GetConstructor(types);
				if (scriptToTest_DoNotUseDirectly == null && constructorInfo != null)
				{
					scriptToTest_DoNotUseDirectly = (RefStlScriptWithDefaults)constructorInfo.Invoke(Array.Empty<object>());
				}
				return new RefStlScriptRetriever(scriptToTest_DoNotUseDirectly);
			}
		}

		RefStlScriptWithDefaults scriptToTest_DoNotUseDirectly;
	}
}
