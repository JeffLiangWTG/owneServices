using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class CodeDescriptionWithThreeGroupsRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public CodeDescriptionWithThreeGroupsRegistryItemEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo, FallbackLevel fallbackLevel)
			: base(dataType, fallbackLevel, null)
		{
			this.editorInfo = (CodeDescriptionWithThreeGroupsRegistryEditorInfo)editorInfo;
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			CodeDescriptionWithThreeGroupsControl result = new CodeDescriptionWithThreeGroupsControl();

			result.SetupGroupColumns(editorInfo.GroupColumnCaption, editorInfo.Group2ColumnCaption, editorInfo.Group3ColumnCaption, editorInfo.AreGroupColumnsVisible);
			result.SetupMainDescriptionColumn(editorInfo.MainDescriptionColumnCaption);
			result.SetupExtraDescriptionColumn(editorInfo.ExtraDescriptionColumnCaption);
			result.SetupEditMode(editorInfo.AreOnlyCodeAndGroupColumnsEditable);
			return result;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		readonly CodeDescriptionWithThreeGroupsRegistryEditorInfo editorInfo;
	}
}
