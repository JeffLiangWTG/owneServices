using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Accounting.GUI.Res;

namespace Enterprise.Accounting.Registry.GUI
{
	public class GLPresentationJournalCategoryRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public GLPresentationJournalCategoryRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
			this.editorInfo = new GLPresentationJournalCategoryRegistryEditorInfo(Res.GetString("f09c4e7f-8e89-481a-8dfe-df681944c29f", "Active"), Res.GetString("70834806-ed46-4e14-8295-f0258948c8f5", "Elimination"),
																					Res.GetString("87a6eaac-83dc-4ed7-99a0-94ebf20eed27", "Closing"), Res.GetString("f9f5d5ae-f565-45a3-8e02-57bd4b7431a3", "Opening"));
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			GLPresentationJournalCategoryControl result = new GLPresentationJournalCategoryControl();
			if (editorInfo != null)
			{
				result.SetupBoolColumn(editorInfo.BoolColumnCaption, editorInfo.Bool2ColumnCaption, editorInfo.Bool3ColumnCaption, editorInfo.Bool4ColumnCaption);
				result.SetupEditMode(editorInfo.IsOnlyBoolColumnEditable);
			}
			return result;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		readonly GLPresentationJournalCategoryRegistryEditorInfo editorInfo;
	}
}
