using Enterprise.Integration;
using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public class LegacyModuleMappingsRegistryEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public LegacyModuleMappingsRegistryEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo)
			: base(dataType, null, null)
		{
			this.editorInfo = editorInfo as LegacyModuleMappingsRegistryEditorInfo;
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			var result = new LegacyModuleMappingsControl();
			result.ModuleMappingCaption = editorInfo.ModuleMappingCaption;
			return result;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		readonly LegacyModuleMappingsRegistryEditorInfo editorInfo;
	}
}
