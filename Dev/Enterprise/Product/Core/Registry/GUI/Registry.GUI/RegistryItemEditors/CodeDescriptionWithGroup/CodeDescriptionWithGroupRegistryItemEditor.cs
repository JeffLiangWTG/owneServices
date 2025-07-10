using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class CodeDescriptionWithGroupRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public CodeDescriptionWithGroupRegistryItemEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo, FallbackLevel fallbackLevel)
			: base(dataType, fallbackLevel, null)
		{
			this.editorInfo = (CodeDescriptionWithGroupRegistryEditorInfo)editorInfo;
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			CodeDescriptionWithGroupControl result = new CodeDescriptionWithGroupControl(editorInfo.IsDescriptionColumnTranslatable);
			result.SetupGroupColumn(editorInfo.GroupColumnCaption, editorInfo.IsGroupColumnVisible);
			result.SetupEditMode(editorInfo.IsOnlyGroupColumnEditable);
			return result;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		readonly CodeDescriptionWithGroupRegistryEditorInfo editorInfo;
	}
}
