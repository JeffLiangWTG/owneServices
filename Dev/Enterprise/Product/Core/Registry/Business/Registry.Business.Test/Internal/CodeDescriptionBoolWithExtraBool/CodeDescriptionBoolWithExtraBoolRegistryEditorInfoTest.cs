using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class CodeDescriptionBoolWithExtraBoolRegistryEditorInfoTest : TestCase
	{
		public void TestConstructor()
		{
			CodeDescriptionBoolWithExtraBoolRegistryEditorInfo editorInfo = new CodeDescriptionBoolWithExtraBoolRegistryEditorInfo("Caption1", "Caption2");
			AssertEquals("BoolColumnCaption", "Caption1", editorInfo.BoolColumnCaption);
			AssertEquals("Bool2ColumnCaption", "Caption2", editorInfo.Bool2ColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionBoolWithExtraBoolCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("IsBoolColumnVisible", true, editorInfo.IsBoolColumnVisible);
			AssertNull("IsBoolColumnVisibleCondition", editorInfo.IsBoolColumnVisibleCondition);
			AssertEquals("IsOnlyBoolColumnEditable", false, editorInfo.IsOnlyBoolColumnEditable);

			editorInfo = new CodeDescriptionBoolWithExtraBoolRegistryEditorInfo("Caption2", false, "Caption3", true);
			AssertEquals("BoolColumnCaption", "Caption2", editorInfo.BoolColumnCaption);
			AssertEquals("Bool2ColumnCaption", "Caption3", editorInfo.Bool2ColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionBoolWithExtraBoolCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("IsBoolColumnVisible", false, editorInfo.IsBoolColumnVisible);
			AssertEquals("IsBool2ColumnVisible", true, editorInfo.IsBool2ColumnVisible);
			AssertNull("IsBoolColumnVisibleCondition", editorInfo.IsBoolColumnVisibleCondition);
			AssertEquals("IsOnlyBoolColumnEditable", false, editorInfo.IsOnlyBoolColumnEditable);

			editorInfo = new CodeDescriptionBoolWithExtraBoolRegistryEditorInfo("Caption3", true, "Caption4", false);
			AssertEquals("BoolColumnCaption", "Caption3", editorInfo.BoolColumnCaption);
			AssertEquals("Bool2ColumnCaption", "Caption4", editorInfo.Bool2ColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionBoolWithExtraBoolCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("IsBoolColumnVisible", true, editorInfo.IsBoolColumnVisible);
			AssertEquals("IsBool2ColumnVisible", false, editorInfo.IsBool2ColumnVisible);
			AssertNull("IsBoolColumnVisibleCondition", editorInfo.IsBoolColumnVisibleCondition);
			AssertEquals("IsOnlyBoolColumnEditable", false, editorInfo.IsOnlyBoolColumnEditable);

			var mock = new Mock<ICondition>();
			mock.Setup(m => m.IsMet).Returns(false);

			editorInfo = new CodeDescriptionBoolWithExtraBoolRegistryEditorInfo("Caption4", mock.Object);
			AssertEquals("BoolColumnCaption", "Caption4", editorInfo.BoolColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionBoolWithExtraBoolCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("IsBoolColumnVisible", false, editorInfo.IsBoolColumnVisible);
			AssertEquals("IsBoolColumnVisibleCondition", mock.Object, editorInfo.IsBoolColumnVisibleCondition);
			AssertEquals("IsOnlyBoolColumnEditable", false, editorInfo.IsOnlyBoolColumnEditable);

			mock.VerifyAll();

			editorInfo = new CodeDescriptionBoolWithExtraBoolRegistryEditorInfo("Caption5", true, true);
			AssertEquals("BoolColumnCaption", "Caption5", editorInfo.BoolColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionBoolWithExtraBoolCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("IsBoolColumnVisible", true, editorInfo.IsBoolColumnVisible);
			AssertNull("IsBoolColumnVisibleCondition", editorInfo.IsBoolColumnVisibleCondition);
			AssertEquals("IsOnlyBoolColumnEditable", true, editorInfo.IsOnlyBoolColumnEditable);

			mock = new Mock<ICondition>();
			mock.Setup(m => m.IsMet).Returns(true);
			mock.Setup(m => m.IsMet).Returns(true);

			editorInfo = new CodeDescriptionBoolWithExtraBoolRegistryEditorInfo("Caption6", mock.Object, true);
			AssertEquals("BoolColumnCaption", "Caption6", editorInfo.BoolColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionBoolWithExtraBoolCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("IsBoolColumnVisible", true, editorInfo.IsBoolColumnVisible);
			AssertEquals("IsBoolColumnVisibleCondition", mock.Object, editorInfo.IsBoolColumnVisibleCondition);
			AssertEquals("IsOnlyBoolColumnEditable", true, editorInfo.IsOnlyBoolColumnEditable);

			mock.VerifyAll();
		}
	}
}
