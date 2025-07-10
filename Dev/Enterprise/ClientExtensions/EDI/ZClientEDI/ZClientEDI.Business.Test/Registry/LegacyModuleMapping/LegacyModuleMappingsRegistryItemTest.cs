using Enterprise.CustomerService.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(LegacyModuleMappingsRegistryItem))]
	class LegacyModuleMappingsRegistryItemTest : StronglyTypedRegistryItemTestCase<LegacyModuleMappingCollection, LegacyModuleMappingCollection>
	{
		protected override StronglyTypedRegistryItem<LegacyModuleMappingCollection, LegacyModuleMappingCollection> GetNewRegistryItem()
		{
			return new LegacyModuleMappingsRegistryItem("", null, null, null, new LegacyModuleMappingsRegistryEditorInfo("Category"), RegistryStorageFlags.System, ModuleListType.MenuSection);
		}
	}

	[TestedType(typeof(LegacyModuleMappingsRegistryDataType))]
	class LegacyModuleMappingsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<LegacyModuleMappingsRegistryDataType>
	{
		#region Implementation

		protected override LegacyModuleMappingsRegistryDataType GetNewDataType()
		{
			return new LegacyModuleMappingsRegistryDataType(ModuleListType.MenuSection);
		}

		protected override string ExpectedEditorName
		{
			get { return "LegacyModuleMappingsRegistryEditor"; }
		}

		// Using custom EditorInfo.
		protected override bool HasEditor
		{
			get { return false; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			LegacyModuleMappingCollection collection = new LegacyModuleMappingCollection(ModuleListType.MenuSection);
			collection.AddNew("COR", "Core", "", MandatoryCustomerServiceMenuSectionList.Codes.Other);
			collection.AddNew("XXX", "XXX", "CR8", Cr8ModuleList.Codes.OtherComplianceIssue);

			byte[] byteArrayValue = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,76,0,101,0,103,0,97,0,99,0,121,0,77,0,111,0,100,0,117,0,108,0,101,0,77,0,97,0,112,0,112,0,105,0,110,0,103,
0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,
0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,
0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,
0,62,0,60,0,76,0,101,0,103,0,97,0,99,0,121,0,77,0,111,0,100,0,117,0,108,0,101,0,77,0,97,0,112,0,112,0,105,0,110,0,103,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,67,0,79,0,82,0,60,0,47,
0,67,0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,67,0,111,0,114,0,101,0,60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,
0,111,0,110,0,62,0,60,0,67,0,114,0,105,0,116,0,105,0,99,0,97,0,108,0,105,0,116,0,121,0,77,0,97,0,112,0,112,0,105,0,110,0,103,0,32,0,47,0,62,0,60,0,77,0,111,0,100,0,117,0,108,0,101,0,77,
0,97,0,112,0,112,0,105,0,110,0,103,0,62,0,79,0,84,0,72,0,60,0,47,0,77,0,111,0,100,0,117,0,108,0,101,0,77,0,97,0,112,0,112,0,105,0,110,0,103,0,62,0,60,0,67,0,111,0,117,0,110,0,116,0,114,
0,121,0,77,0,97,0,112,0,112,0,105,0,110,0,103,0,32,0,47,0,62,0,60,0,47,0,76,0,101,0,103,0,97,0,99,0,121,0,77,0,111,0,100,0,117,0,108,0,101,0,77,0,97,0,112,0,112,0,105,0,110,0,103,0,62,
0,60,0,76,0,101,0,103,0,97,0,99,0,121,0,77,0,111,0,100,0,117,0,108,0,101,0,77,0,97,0,112,0,112,0,105,0,110,0,103,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,88,0,88,0,88,0,60,0,47,0,67,
0,111,0,100,0,101,0,62,0,60,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,0,62,0,88,0,88,0,88,0,60,0,47,0,68,0,101,0,115,0,99,0,114,0,105,0,112,0,116,0,105,0,111,0,110,
0,62,0,60,0,67,0,114,0,105,0,116,0,105,0,99,0,97,0,108,0,105,0,116,0,121,0,77,0,97,0,112,0,112,0,105,0,110,0,103,0,62,0,67,0,82,0,56,0,60,0,47,0,67,0,114,0,105,0,116,0,105,0,99,0,97,
0,108,0,105,0,116,0,121,0,77,0,97,0,112,0,112,0,105,0,110,0,103,0,62,0,60,0,77,0,111,0,100,0,117,0,108,0,101,0,77,0,97,0,112,0,112,0,105,0,110,0,103,0,62,0,79,0,67,0,73,0,60,0,47,0,77,
0,111,0,100,0,117,0,108,0,101,0,77,0,97,0,112,0,112,0,105,0,110,0,103,0,62,0,60,0,67,0,111,0,117,0,110,0,116,0,114,0,121,0,77,0,97,0,112,0,112,0,105,0,110,0,103,0,32,0,47,0,62,0,60,0,47,
0,76,0,101,0,103,0,97,0,99,0,121,0,77,0,111,0,100,0,117,0,108,0,101,0,77,0,97,0,112,0,112,0,105,0,110,0,103,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,76,0,101,0,103,0,97,
0,99,0,121,0,77,0,111,0,100,0,117,0,108,0,101,0,77,0,97,0,112,0,112,0,105,0,110,0,103,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}
