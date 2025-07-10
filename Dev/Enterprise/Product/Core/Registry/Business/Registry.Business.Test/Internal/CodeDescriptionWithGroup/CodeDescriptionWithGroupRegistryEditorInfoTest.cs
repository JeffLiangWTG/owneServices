using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class CodeDescriptionWithGroupRegistryEditorInfoTest : TestCase
	{
		public void TestConstructor()
		{
			CodeDescriptionWithGroupRegistryEditorInfo editorInfo = new CodeDescriptionWithGroupRegistryEditorInfo((NoResString)"Caption1");
			AssertEquals("GroupColumnCaption", "Caption1", editorInfo.GroupColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionWithGroupCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("IsGroupColumnVisible", true, editorInfo.IsGroupColumnVisible);
			AssertNull("IsGroupColumnVisibleCondition", editorInfo.IsGroupColumnVisibleCondition);
			AssertEquals("IsOnlyGroupColumnEditable", false, editorInfo.IsOnlyGroupColumnEditable);

			editorInfo = new CodeDescriptionWithGroupRegistryEditorInfo((NoResString)"Caption2", false);
			AssertEquals("GroupColumnCaption", "Caption2", editorInfo.GroupColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionWithGroupCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("IsGroupColumnVisible", false, editorInfo.IsGroupColumnVisible);
			AssertNull("IsGroupColumnVisibleCondition", editorInfo.IsGroupColumnVisibleCondition);
			AssertEquals("IsOnlyGroupColumnEditable", false, editorInfo.IsOnlyGroupColumnEditable);

			editorInfo = new CodeDescriptionWithGroupRegistryEditorInfo((NoResString)"Caption3", true);
			AssertEquals("GroupColumnCaption", "Caption3", editorInfo.GroupColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionWithGroupCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("IsGroupColumnVisible", true, editorInfo.IsGroupColumnVisible);
			AssertNull("IsGroupColumnVisibleCondition", editorInfo.IsGroupColumnVisibleCondition);
			AssertEquals("IsOnlyGroupColumnEditable", false, editorInfo.IsOnlyGroupColumnEditable);

			editorInfo = new CodeDescriptionWithGroupRegistryEditorInfo((NoResString)"Caption3", true, true, true);
			Assert(editorInfo.IsDescriptionColumnTranslatable);

			editorInfo = new CodeDescriptionWithGroupRegistryEditorInfo((NoResString)"Caption3", true, true, false);
			Assert(!editorInfo.IsDescriptionColumnTranslatable);

			var mock = new Mock<ICondition>();
			mock.Setup(m => m.IsMet).Returns(false);

			editorInfo = new CodeDescriptionWithGroupRegistryEditorInfo((NoResString)"Caption4", mock.Object);
			AssertEquals("GroupColumnCaption", "Caption4", editorInfo.GroupColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionWithGroupCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("IsGroupColumnVisible", false, editorInfo.IsGroupColumnVisible);
			AssertEquals("IsGroupColumnVisibleCondition", mock.Object, editorInfo.IsGroupColumnVisibleCondition);
			AssertEquals("IsOnlyGroupColumnEditable", false, editorInfo.IsOnlyGroupColumnEditable);

			mock.VerifyAll();

			editorInfo = new CodeDescriptionWithGroupRegistryEditorInfo((NoResString)"Caption5", true, true);
			AssertEquals("GroupColumnCaption", "Caption5", editorInfo.GroupColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionWithGroupCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("IsGroupColumnVisible", true, editorInfo.IsGroupColumnVisible);
			AssertNull("IsGroupColumnVisibleCondition", editorInfo.IsGroupColumnVisibleCondition);
			AssertEquals("IsOnlyGroupColumnEditable", true, editorInfo.IsOnlyGroupColumnEditable);

			mock = new Mock<ICondition>();
			mock.Setup(m => m.IsMet).Returns(true);
			mock.Setup(m => m.IsMet).Returns(true);

			editorInfo = new CodeDescriptionWithGroupRegistryEditorInfo((NoResString)"Caption6", mock.Object, true);
			AssertEquals("GroupColumnCaption", "Caption6", editorInfo.GroupColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionWithGroupCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("IsGroupColumnVisible", true, editorInfo.IsGroupColumnVisible);
			AssertEquals("IsGroupColumnVisibleCondition", mock.Object, editorInfo.IsGroupColumnVisibleCondition);
			AssertEquals("IsOnlyGroupColumnEditable", true, editorInfo.IsOnlyGroupColumnEditable);

			mock.VerifyAll();
		}
	}
}
