using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class InternetAddressListRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public InternetAddressListRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new InternetAddressListRegistryControl();
		}

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}
