using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	class ShipmentBranchLocatorTest : BranchLocatorTest
	{
		protected override StronglyTypedRegistryItem<ImportBranchRule> ImportBranchRuleRegistryItem
		{
			get { return SystemDataRegistry.Instance.ShipmentImportBranchRules; }
		}

		protected override BranchLocatorObjectWrapper GetTestObjectWrapper()
		{
			var shipmentXsd = new Xsd.Shipment();
			shipmentXsd.ShipmentDetails.PortofDestination = new Xsd.Movement() { Port = Xsd.UNLOCO.FromPortCode(Factory, "YEBLH") };
			shipmentXsd.ShipmentDetails.PortOfOrigin = new Xsd.Movement() { Port = Xsd.UNLOCO.FromPortCode(Factory, "ZAAMZ") };

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			return new BranchLocatorObjectWrapper(context, shipmentXsd);
		}

		protected override BranchLocatorObjectWrapper GetTestObjectWrapperWithEmptyPorts()
		{
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			return new BranchLocatorObjectWrapper(context, new Xsd.Shipment());
		}
	}
}
