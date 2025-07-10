using Enterprise.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI
{
	public class ParentAndChildCodeDescriptionBoolRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public ParentAndChildCodeDescriptionBoolRegistryItemEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo)
			: base(dataType, null, null)
		{
			this.editorInfo = (ParentAndChildCodeDescriptionBoolRegistryEditorInfo)editorInfo;
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new ParentAndChildCodeDescriptionBoolControl(EditorInfo);
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		protected ParentAndChildCodeDescriptionBoolRegistryEditorInfo EditorInfo
		{
			get { return editorInfo; }
		}

		readonly ParentAndChildCodeDescriptionBoolRegistryEditorInfo editorInfo;
	}
}
