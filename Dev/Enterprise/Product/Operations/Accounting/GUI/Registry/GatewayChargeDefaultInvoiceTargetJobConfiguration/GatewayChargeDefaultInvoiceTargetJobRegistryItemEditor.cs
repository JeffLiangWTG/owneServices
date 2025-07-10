using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.GUI
{
	public class GatewayChargeDefaultInvoiceTargetJobRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public GatewayChargeDefaultInvoiceTargetJobRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new GatewayChargeDefaultInvoiceTargetJobConfigurationControl();
		}

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}
