using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class HBLDeliveryPriorityRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public HBLDeliveryPriorityRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
			=> new HBLDeliveryPriorityControl();

		protected override RegistryItemEditor.EditorPaneAnchor Anchor
			=> EditorPaneAnchor.All;
	}
}
