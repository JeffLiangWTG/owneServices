using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class SplitterLayoutRegistryTest : TransactionedTestCase
	{
		public void TestRegistry()
		{
			var registry = new SplitterLayoutRegistry();

			var testSplitterKey = "TestForm|TestSplitter1";
			var result = registry.GetSplitterLayout(testSplitterKey);
			AssertEquals("Initially", 0m, result);

			registry.SetSplitterLayout(testSplitterKey, 100m);
			result = registry.GetSplitterLayout(testSplitterKey);
			AssertEquals("GetSplitterLayout", 100m, result);

			registry.ClearSplitterLayout(testSplitterKey);
			result = registry.GetSplitterLayout(testSplitterKey);
			AssertEquals("After Clearing", 0m, result);
		}

		[ExpectNoExceptions("SetFormLocation and GetFormLocation should not throw an exception if the environment isn't setup")]
		public void TestRegistryWithNoEnvironment()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(null))
			{
				var registry = new SplitterLayoutRegistry();

				var testSplitterKey = "TestForm|TestSplitter1";
				var result = registry.GetSplitterLayout(testSplitterKey);
				AssertEquals("Initially", 0m, result);

				registry.SetSplitterLayout(testSplitterKey, 100m);
				result = registry.GetSplitterLayout(testSplitterKey);
				AssertEquals("GetSplitterLayout - Should be default value, unable to save with no user.", 0m, result);

				registry.ClearSplitterLayout(testSplitterKey);
				result = registry.GetSplitterLayout(testSplitterKey);
				AssertEquals("After Clearing", 0m, result);
			}
		}
	}
}
