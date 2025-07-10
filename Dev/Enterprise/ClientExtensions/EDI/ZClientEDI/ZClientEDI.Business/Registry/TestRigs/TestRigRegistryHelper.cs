using System.Linq;

namespace Enterprise.Client.EDI.Registry.Business
{
	static class TestRigRegistryHelper
	{
		public static TestRigRegistryOptions GetOptionsForWorkItemCombination(string product, string productArea, string module, string changeType, TestRigRegistryCollection collection)
		{
			return collection.GetOptionsForWorkItemCombination(product, productArea, module, changeType).SingleOrDefault();
		}

		public static TestRigRegistryOptions GetOptionsForWorkItemCombination(string product, string productArea, string module, string changeType)
		{
			return GetOptionsForWorkItemCombination(product, productArea, module, changeType, GetTestRigOptions());
		}

		public static TestRigRegistryCollection GetTestRigOptions()
		{
			return EDIDataRegistry.Instance.TestRigOptions.Value.OptionsCollection;
		}
	}
}

