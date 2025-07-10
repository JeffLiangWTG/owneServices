using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class AutoRatingRequiredFieldsRegistryEditorInfoTest : TestCase
	{
		public void TestConstructor()
		{
			AutoRatingRequiredFieldsRegistryEditorInfo editorInfo = new AutoRatingRequiredFieldsRegistryEditorInfo(false);
			AssertEquals("ShowIncoterm", false, editorInfo.ShowIncoterm);

			editorInfo = new AutoRatingRequiredFieldsRegistryEditorInfo(true);
			AssertEquals("ShowIncoterm", true, editorInfo.ShowIncoterm);
		}

		public void TestBaseDataTypeToBeEdited()
		{
			AutoRatingRequiredFieldsRegistryEditorInfo editorInfo = new AutoRatingRequiredFieldsRegistryEditorInfo(true);
			AssertEquals("BaseDataTypeToBeEdited", typeof(BinaryRegistryDataType), editorInfo.BaseDataTypeToBeEdited);
		}
	}
}
