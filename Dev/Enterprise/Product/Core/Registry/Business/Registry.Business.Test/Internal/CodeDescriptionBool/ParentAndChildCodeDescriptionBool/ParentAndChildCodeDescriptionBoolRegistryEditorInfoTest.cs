using System;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	class ParentAndChildCodeDescriptionBoolRegistryEditorInfoTest : TestCase
	{
		public void TestConstructor()
		{
			CodeDescriptionBoolRegistryEditorInfo parentListEditorInfo = new CodeDescriptionBoolRegistryEditorInfo(null);
			CodeDescriptionBoolRegistryEditorInfo childListEditorInfo = new CodeDescriptionBoolRegistryEditorInfo(null);

			ParentAndChildCodeDescriptionBoolRegistryEditorInfo editorInfo = (ParentAndChildCodeDescriptionBoolRegistryEditorInfo)Activator.CreateInstance(EditorInfoType, new object[] { (NoResString)"P1", (NoResString)"C1", parentListEditorInfo, childListEditorInfo });
			AssertEquals("ParentListCaption", "P1", editorInfo.ParentListCaption);
			AssertEquals("ChildListCaption", "C1", editorInfo.ChildListCaption);
			AssertEquals("ParentListEditorInfo", parentListEditorInfo, editorInfo.ParentListEditorInfo);
			AssertEquals("ChildListEditorInfo", childListEditorInfo, editorInfo.ChildListEditorInfo);
			Assert("IsParentListReadOnly should be false by default", !editorInfo.IsParentListReadOnly);
			AssertEquals("BaseDataTypeToBeEdited", typeof(ParentCodeDescriptionBoolCollection), editorInfo.BaseDataTypeToBeEdited);

			editorInfo = (ParentAndChildCodeDescriptionBoolRegistryEditorInfo)Activator.CreateInstance(EditorInfoType, new object[] { (NoResString)"P1", (NoResString)"C1", parentListEditorInfo, childListEditorInfo, true });
			AssertEquals("ParentListCaption", "P1", editorInfo.ParentListCaption);
			AssertEquals("ChildListCaption", "C1", editorInfo.ChildListCaption);
			AssertEquals("ParentListEditorInfo", parentListEditorInfo, editorInfo.ParentListEditorInfo);
			AssertEquals("ChildListEditorInfo", childListEditorInfo, editorInfo.ChildListEditorInfo);
			Assert("IsParentListReadOnly", editorInfo.IsParentListReadOnly);
			AssertEquals("BaseDataTypeToBeEdited", typeof(ParentCodeDescriptionBoolCollection), editorInfo.BaseDataTypeToBeEdited);

			editorInfo = (ParentAndChildCodeDescriptionBoolRegistryEditorInfo)Activator.CreateInstance(EditorInfoType, new object[] { (NoResString)"P1", (NoResString)"C1", parentListEditorInfo, childListEditorInfo, false });
			AssertEquals("ParentListCaption", "P1", editorInfo.ParentListCaption);
			AssertEquals("ChildListCaption", "C1", editorInfo.ChildListCaption);
			AssertEquals("ParentListEditorInfo", parentListEditorInfo, editorInfo.ParentListEditorInfo);
			AssertEquals("ChildListEditorInfo", childListEditorInfo, editorInfo.ChildListEditorInfo);
			Assert("IsParentListReadOnly", !editorInfo.IsParentListReadOnly);
			AssertEquals("BaseDataTypeToBeEdited", typeof(ParentCodeDescriptionBoolCollection), editorInfo.BaseDataTypeToBeEdited);
		}

		#region Implementation

		protected virtual Type EditorInfoType
		{
			get { return typeof(ParentAndChildCodeDescriptionBoolRegistryEditorInfo); }
		}

		#endregion
	}
}
