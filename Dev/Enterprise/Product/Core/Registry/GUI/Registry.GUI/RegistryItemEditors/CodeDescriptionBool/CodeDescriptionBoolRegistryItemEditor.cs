using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class CodeDescriptionBoolRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public CodeDescriptionBoolRegistryItemEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo, FallbackLevel fallbackLevel)
			: base(dataType, fallbackLevel, null)
		{
			this.editorInfo = (CodeDescriptionBoolRegistryEditorInfo)editorInfo;
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			CodeDescriptionBoolControl result = new CodeDescriptionBoolControl(editorInfo.IsDescriptionColumnTranslatable);
			result.SetupColumns(editorInfo.BoolColumnCaption, editorInfo.IsBoolColumnVisible, editorInfo.IsCodeColumnVisible);
			result.SetupEditMode(editorInfo.IsOnlyBoolColumnEditable);
			result.SetupCodeColumnCaption(editorInfo.CodeColumnCaption);
			return result;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		readonly CodeDescriptionBoolRegistryEditorInfo editorInfo;
	}
}
