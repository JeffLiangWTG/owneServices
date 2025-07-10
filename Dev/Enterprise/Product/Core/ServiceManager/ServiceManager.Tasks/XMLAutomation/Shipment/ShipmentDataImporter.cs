namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class ShipmentDataImporter : AdditionalActionsXmlDataImporter
	{
		public ShipmentDataImporter()
			: base(new BatchForwardingShipmentValueObjectDataAdapter())
		{
		}
	}
}
