using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class CodeDescriptionWithThreeGroupsRegistryEditorInfoTest : TestCase
	{
		public void TestConstructor()
		{
			CodeDescriptionWithThreeGroupsRegistryEditorInfo editorInfo =
				new CodeDescriptionWithThreeGroupsRegistryEditorInfo((NoResString)"Caption1", (NoResString)"Caption2", (NoResString)"Caption3", (NoResString)"CaptionExtra", (NoResString)"CaptionDescription", true, false);
			AssertEquals("GroupColumnCaption", "Caption1", editorInfo.GroupColumnCaption);
			AssertEquals("Group2ColumnCaption", "Caption2", editorInfo.Group2ColumnCaption);
			AssertEquals("Group3ColumnCaption", "Caption3", editorInfo.Group3ColumnCaption);
			AssertEquals("ExtraDescriptionColumnCaption", "CaptionExtra", editorInfo.ExtraDescriptionColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionWithThreeGroupsCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("AreGroupColumnsVisible", true, editorInfo.AreGroupColumnsVisible);
			AssertNull("AreGroupColumnsVisibleCondition", editorInfo.AreGroupColumnsVisibleCondition);
			AssertEquals("AreOnlyCodeAndGroupColumnsEditable", false, editorInfo.AreOnlyCodeAndGroupColumnsEditable);
			Assert(!editorInfo.IsDescriptionColumnTranslatable);

			editorInfo = new CodeDescriptionWithThreeGroupsRegistryEditorInfo((NoResString)"Caption1", (NoResString)"Caption2", (NoResString)"Caption3", (NoResString)"CaptionExtra", (NoResString)"CaptionDescription", false, false);
			AssertEquals("GroupColumnCaption", "Caption1", editorInfo.GroupColumnCaption);
			AssertEquals("Group2ColumnCaption", "Caption2", editorInfo.Group2ColumnCaption);
			AssertEquals("Group3ColumnCaption", "Caption3", editorInfo.Group3ColumnCaption);
			AssertEquals("ExtraDescriptionColumnCaption", "CaptionExtra", editorInfo.ExtraDescriptionColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionWithThreeGroupsCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("AreGroupColumnsVisible", false, editorInfo.AreGroupColumnsVisible);
			AssertNull("AreGroupColumnsVisibleCondition", editorInfo.AreGroupColumnsVisibleCondition);
			AssertEquals("AreOnlyCodeAndGroupColumnsEditable", false, editorInfo.AreOnlyCodeAndGroupColumnsEditable);
			Assert(!editorInfo.IsDescriptionColumnTranslatable);

			var mock = new Mock<ICondition>();
			mock.Setup(m => m.IsMet).Returns(false);

			editorInfo = new CodeDescriptionWithThreeGroupsRegistryEditorInfo((NoResString)"Caption1", (NoResString)"Caption2", (NoResString)"Caption3", (NoResString)"CaptionExtra", (NoResString)"CaptionDescription", mock.Object, true, false, true);
			AssertEquals("GroupColumnCaption", "Caption1", editorInfo.GroupColumnCaption);
			AssertEquals("Group2ColumnCaption", "Caption2", editorInfo.Group2ColumnCaption);
			AssertEquals("Group3ColumnCaption", "Caption3", editorInfo.Group3ColumnCaption);
			AssertEquals("ExtraDescriptionColumnCaption", "CaptionExtra", editorInfo.ExtraDescriptionColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionWithThreeGroupsCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("AreGroupColumnsVisible", false, editorInfo.AreGroupColumnsVisible);
			AssertEquals("AreGroupColumnsVisibleCondition", mock.Object, editorInfo.AreGroupColumnsVisibleCondition);
			AssertEquals("AreOnlyCodeAndGroupColumnsEditable", false, editorInfo.AreOnlyCodeAndGroupColumnsEditable);
			Assert(editorInfo.IsDescriptionColumnTranslatable);

			mock.VerifyAll();

			editorInfo = new CodeDescriptionWithThreeGroupsRegistryEditorInfo((NoResString)"Caption1", (NoResString)"Caption2", (NoResString)"Caption3", (NoResString)"CaptionExtra", (NoResString)"CaptionDescription", true, true);
			AssertEquals("GroupColumnCaption", "Caption1", editorInfo.GroupColumnCaption);
			AssertEquals("Group2ColumnCaption", "Caption2", editorInfo.Group2ColumnCaption);
			AssertEquals("Group3ColumnCaption", "Caption3", editorInfo.Group3ColumnCaption);
			AssertEquals("ExtraDescriptionColumnCaption", "CaptionExtra", editorInfo.ExtraDescriptionColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionWithThreeGroupsCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("AreGroupColumnsVisible", true, editorInfo.AreGroupColumnsVisible);
			AssertNull("AreGroupColumnsVisibleCondition", editorInfo.AreGroupColumnsVisibleCondition);
			AssertEquals("AreOnlyCodeAndGroupColumnsEditable", true, editorInfo.AreOnlyCodeAndGroupColumnsEditable);
			Assert(!editorInfo.IsDescriptionColumnTranslatable);

			mock = new Mock<ICondition>();
			mock.Setup(m => m.IsMet).Returns(true);
			mock.Setup(m => m.IsMet).Returns(true);

			editorInfo = new CodeDescriptionWithThreeGroupsRegistryEditorInfo((NoResString)"Caption1", (NoResString)"Caption2", (NoResString)"Caption3", (NoResString)"CaptionExtra", (NoResString)"CaptionDescription", mock.Object, true, true, false);
			AssertEquals("GroupColumnCaption", "Caption1", editorInfo.GroupColumnCaption);
			AssertEquals("Group2ColumnCaption", "Caption2", editorInfo.Group2ColumnCaption);
			AssertEquals("Group3ColumnCaption", "Caption3", editorInfo.Group3ColumnCaption);
			AssertEquals("ExtraDescriptionColumnCaption", "CaptionExtra", editorInfo.ExtraDescriptionColumnCaption);
			AssertEquals("BaseDataTypeToBeEdited", typeof(CodeDescriptionWithThreeGroupsCollection), editorInfo.BaseDataTypeToBeEdited);
			AssertEquals("AreGroupColumnsVisible", true, editorInfo.AreGroupColumnsVisible);
			AssertEquals("AreGroupColumnsVisibleCondition", mock.Object, editorInfo.AreGroupColumnsVisibleCondition);
			AssertEquals("AreOnlyCodeAndGroupColumnsEditable", true, editorInfo.AreOnlyCodeAndGroupColumnsEditable);
			Assert(!editorInfo.IsDescriptionColumnTranslatable);

			mock.VerifyAll();
		}
	}
}
