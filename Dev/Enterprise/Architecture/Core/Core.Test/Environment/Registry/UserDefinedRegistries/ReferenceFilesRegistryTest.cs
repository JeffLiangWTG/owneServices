using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class ReferenceFilesRegistryTest : TransactionedTestCase
	{
		public void TestEquipmentGroup()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("AAA", "A A A");
			list.AddPair("BBB", "B B B");
			list.AddPair("CCC", "C C C");
			DataRegistry registry = new DataRegistry();
			AssertEquals(0, registry.ReferenceFiles.EquipmentGroup.Count);
			registry.ReferenceFiles.EquipmentGroup = list;
			AssertEquals(registry.ReferenceFiles.EquipmentGroup.ToString(), 3, registry.ReferenceFiles.EquipmentGroup.Count);
			AssertEquals(true, registry.ReferenceFiles.EquipmentGroup.ContainsCode("AAA"));
			AssertEquals(true, registry.ReferenceFiles.EquipmentGroup.ContainsCode("BBB"));
			AssertEquals(true, registry.ReferenceFiles.EquipmentGroup.ContainsCode("CCC"));
			AssertEquals(false, registry.ReferenceFiles.EquipmentGroup.ContainsCode("DDD"));
		}

		[ExpectNoExceptions]
		public void TestMaxLengthOfEquipmentGroupCode()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("AAAA", "A A A A");
			DataRegistry registry = new DataRegistry();
			AssertEquals("Precondition", 0, registry.ReferenceFiles.EquipmentGroup.Count);
			try
			{
				registry.ReferenceFiles.EquipmentGroup = list;
				Fail("Expected validation exception for value '" + list[0].Code + "'");
			}
			catch (RegistryValidationException)
			{
			}
		}
	}
}
