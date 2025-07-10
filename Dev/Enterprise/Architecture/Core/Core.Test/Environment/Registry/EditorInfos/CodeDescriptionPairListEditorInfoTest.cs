using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class CodeDescriptionPairListEditorInfoTest : TestCase
	{
		public void TestConstructor()
		{
			CodeDescriptionPairListEditorInfo editorInfo = new CodeDescriptionPairListEditorInfo();
			AssertEquals("ShowCodeColumn", true, editorInfo.ShowCodeColumn);
			AssertEquals("ShowDescriptionColumn", true, editorInfo.ShowDescriptionColumn);
			AssertEquals("CodeFieldCasing", CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, editorInfo.CodeFieldCasing);
			AssertEquals("DescriptionFieldCasing", CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, editorInfo.DescriptionFieldCasing);
			AssertEquals("CodeColumnCaption", "Code", editorInfo.CodeColumnCaption);
			AssertEquals("DescriptionColumnCaption", "Description", editorInfo.DescriptionColumnCaption);

			editorInfo = new CodeDescriptionPairListEditorInfo((NoResString)"Gah");
			AssertEquals("ShowCodeColumn", true, editorInfo.ShowCodeColumn);
			AssertEquals("ShowDescriptionColumn", true, editorInfo.ShowDescriptionColumn);
			AssertEquals("CodeFieldCasing", CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, editorInfo.CodeFieldCasing);
			AssertEquals("DescriptionFieldCasing", CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, editorInfo.DescriptionFieldCasing);
			AssertEquals("CodeColumnCaption", "Gah", editorInfo.CodeColumnCaption);
			AssertEquals("DescriptionColumnCaption", "Description", editorInfo.DescriptionColumnCaption);

			editorInfo = new CodeDescriptionPairListEditorInfo(CodeDescriptionPairListEditorInfo.CharacterCasing.Upper);
			AssertEquals("ShowCodeColumn", true, editorInfo.ShowCodeColumn);
			AssertEquals("ShowDescriptionColumn", true, editorInfo.ShowDescriptionColumn);
			AssertEquals("CodeFieldCasing", CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, editorInfo.CodeFieldCasing);
			AssertEquals("DescriptionFieldCasing", CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, editorInfo.DescriptionFieldCasing);
			AssertEquals("CodeColumnCaption", "Code", editorInfo.CodeColumnCaption);
			AssertEquals("DescriptionColumnCaption", "Description", editorInfo.DescriptionColumnCaption);

			editorInfo = new CodeDescriptionPairListEditorInfo(true, false);
			AssertEquals("ShowCodeColumn", true, editorInfo.ShowCodeColumn);
			AssertEquals("ShowDescriptionColumn", false, editorInfo.ShowDescriptionColumn);
			AssertEquals("CodeFieldCasing", CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, editorInfo.CodeFieldCasing);
			AssertEquals("DescriptionFieldCasing", CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, editorInfo.DescriptionFieldCasing);
			AssertEquals("CodeColumnCaption", "Code", editorInfo.CodeColumnCaption);
			AssertEquals("DescriptionColumnCaption", "Description", editorInfo.DescriptionColumnCaption);

			editorInfo = new CodeDescriptionPairListEditorInfo(true, false, CodeDescriptionPairListEditorInfo.CharacterCasing.Upper);
			AssertEquals("ShowCodeColumn", true, editorInfo.ShowCodeColumn);
			AssertEquals("ShowDescriptionColumn", false, editorInfo.ShowDescriptionColumn);
			AssertEquals("CodeFieldCasing", CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, editorInfo.CodeFieldCasing);
			AssertEquals("DescriptionFieldCasing", CodeDescriptionPairListEditorInfo.CharacterCasing.Upper, editorInfo.DescriptionFieldCasing);
			AssertEquals("CodeColumnCaption", "Code", editorInfo.CodeColumnCaption);
			AssertEquals("DescriptionColumnCaption", "Description", editorInfo.DescriptionColumnCaption);

			editorInfo = new CodeDescriptionPairListEditorInfo(false, true, CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, CodeDescriptionPairListEditorInfo.CharacterCasing.Lower);
			AssertEquals("ShowCodeColumn", false, editorInfo.ShowCodeColumn);
			AssertEquals("ShowDescriptionColumn", true, editorInfo.ShowDescriptionColumn);
			AssertEquals("CodeFieldCasing", CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, editorInfo.CodeFieldCasing);
			AssertEquals("DescriptionFieldCasing", CodeDescriptionPairListEditorInfo.CharacterCasing.Lower, editorInfo.DescriptionFieldCasing);
			AssertEquals("CodeColumnCaption", "Code", editorInfo.CodeColumnCaption);
			AssertEquals("DescriptionColumnCaption", "Description", editorInfo.DescriptionColumnCaption);

			editorInfo = new CodeDescriptionPairListEditorInfo(false, true, CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, CodeDescriptionPairListEditorInfo.CharacterCasing.Lower, (NoResString)"Blah Code", (NoResString)"Blah Description");
			AssertEquals("ShowCodeColumn", false, editorInfo.ShowCodeColumn);
			AssertEquals("ShowDescriptionColumn", true, editorInfo.ShowDescriptionColumn);
			AssertEquals("CodeFieldCasing", CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, editorInfo.CodeFieldCasing);
			AssertEquals("DescriptionFieldCasing", CodeDescriptionPairListEditorInfo.CharacterCasing.Lower, editorInfo.DescriptionFieldCasing);
			AssertEquals("CodeColumnCaption", "Blah Code", editorInfo.CodeColumnCaption);
			AssertEquals("DescriptionColumnCaption", "Blah Description", editorInfo.DescriptionColumnCaption);
		}

		public void TestBaseDataTypeToBeEdited()
		{
			CodeDescriptionPairListEditorInfo editorInfo = new CodeDescriptionPairListEditorInfo();
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionPairListRegistryDataType), editorInfo.BaseDataTypeToBeEdited);
		}
	}
}
