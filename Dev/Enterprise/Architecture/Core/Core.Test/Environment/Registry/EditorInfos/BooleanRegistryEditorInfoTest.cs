using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class BooleanRegistryEditorInfoTest : TestCase
	{
		public void TestBaseDataTypeToBeEditedAndCaption()
		{
			BooleanRegistryEditorInfo editorInfo = new BooleanRegistryEditorInfo();
			AssertEquals("BaseDataTypeToBeEdited", typeof(BooleanRegistryDataType), editorInfo.BaseDataTypeToBeEdited);
			editorInfo = new BooleanRegistryEditorInfo((NoResString)"What?");
			AssertEquals("Caption", "What?", editorInfo.Caption);
		}
	}
}
