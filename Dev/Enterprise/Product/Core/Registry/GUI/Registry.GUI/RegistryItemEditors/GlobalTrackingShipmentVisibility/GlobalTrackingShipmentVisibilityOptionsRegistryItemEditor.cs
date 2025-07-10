using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class GlobalTrackingShipmentVisibilityOptionsRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public GlobalTrackingShipmentVisibilityOptionsRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new GlobalTrackingShipmentVisibilityOptionsRegistryItemControl();
		}

		protected override EditorPaneAnchor Anchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
