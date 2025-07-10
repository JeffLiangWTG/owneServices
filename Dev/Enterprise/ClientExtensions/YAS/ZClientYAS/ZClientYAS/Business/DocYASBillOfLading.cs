using CargoWise.Types;
using Enterprise.DocumentWrappers;

namespace Enterprise.Client.YAS.Business
{
	public class DocYASBillOfLading : DocBillOfLading
	{
		public DocYASBillOfLading(DocForwardingShipment shipmentWrapper)
			: base(shipmentWrapper)
		{
		}

		protected new DocYASForwardingShipment ShipmentWrapper
		{
			get { return (DocYASForwardingShipment)base.ShipmentWrapper; }
		}

		protected override ZBool ShowTCImageCore
		{
			get
			{
				return base.ShowTCImageCore && !ShipmentWrapper.IsExportingYASBillOfLading ;
			}
		}
	}
}
