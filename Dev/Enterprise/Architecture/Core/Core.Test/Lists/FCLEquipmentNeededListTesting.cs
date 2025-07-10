using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class FCLEquipmentNeededListTesting : TransactionedTestCase
	{
		public void TestParameterlessConstructor()
		{
			var list = new FCLEquipmentNeededList();
			AssertEquals("List.Count", 6, list.Count);
			AssertEquals("Contains 'ASK'", true, list.ContainsCode(Constants.EquipmentNeeded.Ask));
			AssertEquals("Contains 'ANY'", true, list.ContainsCode(Constants.EquipmentNeeded.Any));
		}

		public void TestRatingConstructor()
		{
			var list = new FCLEquipmentNeededList(false);
			AssertEquals("List.Count", 5, list.Count);
			AssertEquals("Do not contain 'ASK'", false, list.ContainsCode(Constants.EquipmentNeeded.Ask));
			AssertEquals("Contains 'ANY'", true, list.ContainsCode(Constants.EquipmentNeeded.Any));
		}

		public void TestDefaultCodesArePreservedWhenRegistryIsOverriden()
		{
			CodeDescriptionPairList beforeList = new FCLEquipmentNeededList();
			Assert("Contains WaitForUnpack", beforeList.ContainsCode(Enterprise.Core.Constants.FCLEquipmentNeeded.WaitForUnpack));

			CodeDescriptionPairList registryList = new CodeDescriptionPairList();
			registryList.AddPair("ABC", "Test ABC");

			ReadOnlyCodeDescriptionPairList storedList = EnvProxy.Instance.Registry.FCLEquipmentNeededList;
			try
			{
				EnvProxy.Instance.Registry.FCLEquipmentNeededList = registryList;

				CodeDescriptionPairList afterList = new FCLEquipmentNeededList();
				Assert("Contains WaitForUnpack", afterList.ContainsCode(Enterprise.Core.Constants.FCLEquipmentNeeded.WaitForUnpack));
				Assert("Contains ABC", afterList.ContainsCode("ABC"));
			}
			finally
			{
				EnvProxy.Instance.Registry.FCLEquipmentNeededList = storedList;
			}
		}
	}
}
