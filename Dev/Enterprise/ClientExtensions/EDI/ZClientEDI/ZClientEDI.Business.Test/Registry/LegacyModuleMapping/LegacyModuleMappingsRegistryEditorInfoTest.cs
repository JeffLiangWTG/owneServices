using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Test
{
	sealed class LegacyModuleMappingsRegistryEditorInfoTest : TestCase
	{
		public void TestConstructor()
		{
			var editorInfo = new LegacyModuleMappingsRegistryEditorInfo("Category");
			AssertEquals("ModuleMappingCaption", "Category", editorInfo.ModuleMappingCaption);
		}
	}
}