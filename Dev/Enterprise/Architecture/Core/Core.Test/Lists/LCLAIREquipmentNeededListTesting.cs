using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class LCLAIREquipmentNeededListTesting : TransactionedTestCase
	{
		public void TestParameterlessConstructor()
		{
			var list = new LCLAIREquipmentNeededList();
			AssertEquals("List.Count", 6, list.Count);
			AssertEquals("Contains 'ASK'", true, list.ContainsCode(Constants.EquipmentNeeded.Ask));
			AssertEquals("Contains 'ANY'", true, list.ContainsCode(Constants.EquipmentNeeded.Any));
		}

		public void TestRatingConstructor()
		{
			var list = new LCLAIREquipmentNeededList(false);
			AssertEquals("List.Count", 5, list.Count);
			AssertEquals("Do not contain 'ASK'", false, list.ContainsCode(Constants.EquipmentNeeded.Ask));
			AssertEquals("Contains 'ANY'", true, list.ContainsCode(Constants.EquipmentNeeded.Any));
		}

		public void TestDefaultCodesArePreservedWhenRegistryIsOverriden()
		{
			CodeDescriptionPairList beforeList = new LCLAIREquipmentNeededList();
			Assert("Contains Premise", beforeList.ContainsCode(Constants.LCLAIREquipmentNeeded.Premise));

			CodeDescriptionPairList registryList = new CodeDescriptionPairList();
			registryList.AddPair("ABC", "Test ABC");

			ReadOnlyCodeDescriptionPairList storedList = EnvProxy.Instance.Registry.LCLAIREquipmentNeededList;
			try
			{
				EnvProxy.Instance.Registry.LCLAIREquipmentNeededList = registryList;

				CodeDescriptionPairList afterList = new LCLAIREquipmentNeededList();
				Assert("Contains Premise", afterList.ContainsCode(Constants.LCLAIREquipmentNeeded.Premise));
				Assert("Contains ABC", afterList.ContainsCode("ABC"));
			}
			finally
			{
				EnvProxy.Instance.Registry.LCLAIREquipmentNeededList = storedList;
			}
		}
	}
}
