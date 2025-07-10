using Enterprise.Integration;
using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public class WebSecurityMappingRegistryEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public WebSecurityMappingRegistryEditor(IRegistryDataType dataType)
			: base(dataType, null, null)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			var result = new WebSecurityMappingControl();
			return result;
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
