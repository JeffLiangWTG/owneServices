using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.CA;

namespace Enterprise.Customs.CA.Business
{
	public class ForwardingShipmentCustomsStatusProvider : CusSCAHouseForwardingShipmentCustomsStatusProvider, IForwardingShipmentCustomsStatusProvider
	{
		public ForwardingShipmentCustomsStatusProvider(ForwardingShipment shipment)
			: base(shipment)
		{
			shipment.Factory.AddFetchHint(typeof(CusCAeMHHouse), CusCAeMHHouseSchema.BW_ParentID, shipment.PK);
		}

		protected override ZString[] ApplicationCodes
		{
			get { return CusSCAOceanBill.ApplicationCodes; }
		}

		public IReleaseStatusWrapper GetReleaseStatusWrapper()
		{
			var ediMessage = EDIReleaseMessage.GetLastReleaseStatusMessage(base.Shipment.Messages, RNSMessagingBO.ReleaseSubTypesToIgnore);
			return ediMessage != null ? new ReleaseStatusDocumentWrapper(ediMessage) : new ReleaseStatusDocumentWrapper();
		}

		CusCAeMHHouse HouseBill
		{
			get { return houseBill ?? (houseBill = Shipment.Factory.LoadTop1<CusCAeMHHouse>(new ZQuery(CusCAeMHHouseSchema.BW_ParentID, Shipment.PK))); }
		}
		CusCAeMHHouse houseBill;

		public override ZString CustomsCargoStatus()
		{
			var houseBillCache = HouseBill;
			return houseBillCache != null ? houseBillCache.BW_CustomsStatus : base.CustomsCargoStatus();
		}

		public override ZString CustomsMessageStatus()
		{
			var houseBillCache = HouseBill;
			return houseBillCache != null ? houseBillCache.BW_MessageStatus : base.CustomsMessageStatus();
		}

		public override ZString EManifestCargoStatus()
		{
			var houseBillCache = HouseBill;
			return houseBillCache != null ? houseBillCache.BW_CustomsStatus : base.EManifestCargoStatus();
		}

		public override ZString EManifestMessageStatus()
		{
			var houseBillCache = HouseBill;
			return houseBillCache != null ? houseBillCache.BW_MessageStatus : base.EManifestMessageStatus();
		}

		public override ZString ACICargoStatus()
		{
			BaseCusSCAHouse house = this.House;
			return house != null ? house.CA_ShipmentStatus : base.ACICargoStatus();
		}

		public override ZString ACIMessageStatus()
		{
			BaseCusSCAHouse house = this.House;
			return house != null ? house.CA_MessageStatus : base.ACIMessageStatus();
		}
	}
}
