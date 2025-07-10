using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class AccTaxRateListRegistryEditorInfoTest : TestCase
	{
		public void TestFilter()
		{
			AccTaxRateListRegistryEditorInfo editorInfo = new AccTaxRateListRegistryEditorInfo(RegistryFindBoxFilter.None);
			AssertEquals("Filter", RegistryFindBoxFilter.None, editorInfo.Filter);
		}

		public void TestBaseDataTypeToBeEdited()
		{
			AccTaxRateListRegistryEditorInfo editorInfo = new AccTaxRateListRegistryEditorInfo(RegistryFindBoxFilter.None);
			AssertEquals("BaseDataTypeToBeEdited", typeof(StringRegistryDataType), editorInfo.BaseDataTypeToBeEdited);
		}
	}
}
