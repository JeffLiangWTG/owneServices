using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class CodeDescriptionBoolRegistryEditorInfoTest : TestCase
	{
		public void TestConstructor()
		{
			CodeDescriptionBoolRegistryEditorInfo editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Caption1");
			AssertEquals("BoolColumnCaption", "Caption1", editorInfo.BoolColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionBoolCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("IsBoolColumnVisible", true, editorInfo.IsBoolColumnVisible);
			AssertNull("IsBoolColumnVisibleCondition", editorInfo.IsBoolColumnVisibleCondition);
			AssertEquals("IsOnlyBoolColumnEditable", false, editorInfo.IsOnlyBoolColumnEditable);

			editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Caption2", false);
			AssertEquals("BoolColumnCaption", "Caption2", editorInfo.BoolColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionBoolCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("IsBoolColumnVisible", false, editorInfo.IsBoolColumnVisible);
			AssertNull("IsBoolColumnVisibleCondition", editorInfo.IsBoolColumnVisibleCondition);
			AssertEquals("IsOnlyBoolColumnEditable", false, editorInfo.IsOnlyBoolColumnEditable);

			editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Caption3", true);
			AssertEquals("BoolColumnCaption", "Caption3", editorInfo.BoolColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionBoolCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("IsBoolColumnVisible", true, editorInfo.IsBoolColumnVisible);
			AssertNull("IsBoolColumnVisibleCondition", editorInfo.IsBoolColumnVisibleCondition);
			AssertEquals("IsOnlyBoolColumnEditable", false, editorInfo.IsOnlyBoolColumnEditable);

			editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Caption3", true, true, true);
			Assert(editorInfo.IsDescriptionColumnTranslatable);

			editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Caption3", true, true, false);
			Assert(!editorInfo.IsDescriptionColumnTranslatable);
			AssertEquals("CodeColumnCaption (default)", "", editorInfo.CodeColumnCaption);

			var mock = new Mock<ICondition>();
			mock.Setup(m => m.IsMet).Returns(false);

			editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Caption4", mock.Object);
			AssertEquals("BoolColumnCaption", "Caption4", editorInfo.BoolColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionBoolCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("IsBoolColumnVisible", false, editorInfo.IsBoolColumnVisible);
			AssertEquals("IsBoolColumnVisibleCondition", mock.Object, editorInfo.IsBoolColumnVisibleCondition);
			AssertEquals("IsOnlyBoolColumnEditable", false, editorInfo.IsOnlyBoolColumnEditable);

			mock.VerifyAll();

			editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Caption5", true, true);
			AssertEquals("BoolColumnCaption", "Caption5", editorInfo.BoolColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionBoolCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("IsBoolColumnVisible", true, editorInfo.IsBoolColumnVisible);
			AssertNull("IsBoolColumnVisibleCondition", editorInfo.IsBoolColumnVisibleCondition);
			AssertEquals("IsOnlyBoolColumnEditable", true, editorInfo.IsOnlyBoolColumnEditable);

			mock = new Mock<ICondition>();
			mock.Setup(m => m.IsMet).Returns(true);
			mock.Setup(m => m.IsMet).Returns(true);

			editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Caption6", mock.Object, true);
			AssertEquals("BoolColumnCaption", "Caption6", editorInfo.BoolColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionBoolCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("IsBoolColumnVisible", true, editorInfo.IsBoolColumnVisible);
			AssertEquals("IsBoolColumnVisibleCondition", mock.Object, editorInfo.IsBoolColumnVisibleCondition);
			AssertEquals("IsOnlyBoolColumnEditable", true, editorInfo.IsOnlyBoolColumnEditable);

			mock.VerifyAll();

			editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"BoolCaptionTest", (NoResString)"CodeCaptionTest", null, true, true, true);
			AssertEquals("BoolColumnCaption", "BoolCaptionTest", editorInfo.BoolColumnCaption);
			AssertEquals("CodeColumnCaption", "CodeCaptionTest", editorInfo.CodeColumnCaption);
		}

		public void TestIsCodeColumnVisible()
		{
			var editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"", null, true, true, true);
			AssertEquals("Constructor sets IsCodeColumnVisible correctly", true, editorInfo.IsCodeColumnVisible);

			editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"", null, true, true, false);
			AssertEquals("Constructor sets IsCodeColumnVisible correctly", false, editorInfo.IsCodeColumnVisible);

			editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"", null, true, false, true);
			AssertEquals("Constructor sets IsCodeColumnVisible correctly", true, editorInfo.IsCodeColumnVisible);

			editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"", null, true, false, false);
			AssertEquals("Constructor sets IsCodeColumnVisible correctly", true, editorInfo.IsCodeColumnVisible);

			editorInfo = new CodeDescriptionBoolRegistryEditorInfo((NoResString)"", null, false, true, false);
			AssertEquals("Constructor sets IsCodeColumnVisible correctly", true, editorInfo.IsCodeColumnVisible);
		}
	}
}
