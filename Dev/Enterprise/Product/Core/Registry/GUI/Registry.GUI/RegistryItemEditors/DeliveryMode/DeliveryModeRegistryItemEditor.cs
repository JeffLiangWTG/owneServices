namespace Enterprise.Registry.GUI
{
	using CargoWise.EntityFramework;
	using Enterprise.Integration;
	using Enterprise.ZArchitecture.Environment;

	public class DeliveryModeRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public DeliveryModeRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new DeliveryModeRegistryControl();
		}

		protected override EditorPaneAnchor Anchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
