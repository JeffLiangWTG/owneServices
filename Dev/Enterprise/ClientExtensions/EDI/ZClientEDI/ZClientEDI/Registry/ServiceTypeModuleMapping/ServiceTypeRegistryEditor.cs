using Enterprise.Integration;
using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public class ServiceTypeRegistryEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public ServiceTypeRegistryEditor(IRegistryDataType dataType, IRegistryEditorInfo editorInfo)
			: base(dataType, null, null)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new ServiceTypeRegistryControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
