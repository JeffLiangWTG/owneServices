using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public class NeoUpgradeLicencesRegistryEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public NeoUpgradeLicencesRegistryEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory) { }

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new NeoUpgradeLicencesControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
