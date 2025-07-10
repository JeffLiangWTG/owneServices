using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.IFC.Testing
{
	[TestedType(typeof(IFCDataRegistry))]
	internal class IFCDataRegistryTest : RegistryItemSetTestCase<IFCDataRegistry>
	{
		public void TestUserVisibleRegistryItems()
		{
			AssertEquals("Number of registry items", 2, AllItems.Count);
			AssertVisible("FSCExportDirectory");
			AssertVisible("RunTimeIntervalForBatchProcess", true);
		}

		public void TestSettingRegistryItems()
		{
			ItemSet.FSCExportDirectory = "BOB";
			AssertEquals("BOB", ItemSet.FSCExportDirectory);
			ItemSet.FSCRunTimeIntervalForBatchProcess = 5;
			AssertEquals(5, ItemSet.FSCRunTimeIntervalForBatchProcess);
		}
	}
}
