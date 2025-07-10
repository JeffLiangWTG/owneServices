using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class GlowUseIndexingForModuleListEditorInfoTest : TestCase
	{
		public void TestBaseDataTypeToBeEdited()
		{
			var editorInfo = new GlowUseIndexingForModuleListEditorInfo();
			AssertEquals(typeof(string[]), editorInfo.BaseDataTypeToBeEdited);
		}
	}
}
