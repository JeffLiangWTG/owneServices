using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class CommunicationStatusRegistryEditorInfoTest : TestCase
	{
		public void TestBaseDataTypeToBeEdited()
		{
			var editorInfo = new CommunicationStatusRegistryEditorInfo();
			AssertEquals(typeof(CommunicationStatusCollection), editorInfo.BaseDataTypeToBeEdited);
		}
	}
}
