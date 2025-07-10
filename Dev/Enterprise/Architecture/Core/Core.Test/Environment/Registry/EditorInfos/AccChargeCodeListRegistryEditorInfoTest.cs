using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class AccChargeCodeListRegistryEditorInfoTest : TestCase
	{
		public void TestFilter()
		{
			AccChargeCodeListRegistryEditorInfo editorInfo = new AccChargeCodeListRegistryEditorInfo(RegistryFindBoxFilter.None);
			AssertEquals("Filter", RegistryFindBoxFilter.None, editorInfo.Filter);

			editorInfo = new AccChargeCodeListRegistryEditorInfo(RegistryFindBoxFilter.FreightChargeCode);
			AssertEquals("Filter", RegistryFindBoxFilter.FreightChargeCode, editorInfo.Filter);
		}

		public void TestBaseDataTypeToBeEdited()
		{
			AccChargeCodeListRegistryEditorInfo editorInfo = new AccChargeCodeListRegistryEditorInfo(RegistryFindBoxFilter.None);
			AssertEquals("BaseDataTypeToBeEdited", typeof(StringRegistryDataType), editorInfo.BaseDataTypeToBeEdited);
		}
	}
}
