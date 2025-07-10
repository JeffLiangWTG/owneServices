using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(GLPresentationJournalCategoryRegistryItemEditor))]
	public class GLPresentationJournalCategoryRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new GLPresentationJournalCategoryRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((GLPresentationJournalCategoryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(GLPresentationJournalCategoryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new GLPresentationJournalCategoryRegistryItem("", null, null, null, RegistryStorageFlags.System/*, new GLPresentationJournalCategoryCollection()*/);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new GLPresentationJournalCategoryCollection();
			var copy = collection.AddNew();

			copy.Code = "TST";
			copy.Description = (NoResString)"Desc";
			copy.Bool = true;

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
