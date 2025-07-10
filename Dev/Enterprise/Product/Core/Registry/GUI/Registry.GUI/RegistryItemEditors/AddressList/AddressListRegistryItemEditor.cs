using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class AddressListRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public AddressListRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new AddressListControl();

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}
