using CargoWise.ComponentModel;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.TGE.PMS
{
	public class TGEShipmentValueObjectDataAdapter : ForwardingShipmentValueObjectDataAdapter
	{
		public TGEShipmentValueObjectDataAdapter(ForwardingConsol existingConsol)
			: base(existingConsol)
		{
		}

		protected override bool ShouldUpdateExistingObject(ForwardingShipment bizObj, INotifications notifications)
		{
			notifications.Notify(new ErrorNotification(ErrorType.Error, "Shipment with HAWB " + bizObj.JS_HouseBill + " already exists"));
			return base.ShouldUpdateExistingObject(bizObj, notifications);
		}
	}
}
