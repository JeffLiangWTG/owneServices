using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class DropDownCodeDescriptionBoolRegistryEditorInfoTest : TestCase
	{
		public void TestConstructor()
		{
			var editorInfo2 = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Caption1");
			AssertEquals("BoolColumnCaption", "Caption1", editorInfo2.BoolColumnCaption);
		}
	}
}
