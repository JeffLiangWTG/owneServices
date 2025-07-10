using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;

namespace Enterprise.Client.TGE.PMS
{
	public class TGEConsolValueObjectDataAdapter : ForwardingConsolValueObjectDataAdapter
	{
		protected override ShipmentValueObjectDataAdapter<ForwardingShipment> GetNewShipmentValueObjectDataAdapter(ForwardingConsol existingConsol)
		{
			return new TGEShipmentValueObjectDataAdapter(existingConsol);
		}

		protected override void OnUserDeclinedImport(ForwardingConsol bizObj, Enterprise.DataTransfer.Xml.XsdVersion1.Consol valueObj, IValueObjectImportContext context)
		{
			ImportConsolShipmentsAndDeclarations(bizObj, valueObj, context);
		}
	}
}
