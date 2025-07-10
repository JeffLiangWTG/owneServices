using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Registry.GUI
{
	class ShippingAgentRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public ShippingAgentRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new ShippingAgentControl();

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}
