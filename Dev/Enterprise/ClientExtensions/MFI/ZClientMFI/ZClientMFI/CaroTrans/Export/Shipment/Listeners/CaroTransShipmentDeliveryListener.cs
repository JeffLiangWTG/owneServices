using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.MFI.CaroTrans
{
	public class CaroTransShipmentDeliveryListener : CaroTransShipmentListener
	{
		public CaroTransShipmentDeliveryListener(ZString exportPath)
			: base(exportPath)
		{ }

		public override string BusinessObjectTableName
		{
			get { return JobShipmentSchema.Constants.TableName; }
		}

		public override Type BusinessObjectType
		{
			get { return typeof(ForwardingShipment); }
		}

		#region Implementation

		protected override ShipmentCollection GetShipmentsToExport(BusinessObject matchingBusinessObject)
		{
			var result = new ShipmentCollection(Factory);

			var foundShipment = (ForwardingShipment)matchingBusinessObject;

			if (CanExportShipment(foundShipment))
			{
				result.Add(foundShipment);
			}

			return result;
		}

		protected override bool AdditionalMatching(StmALog log)
		{
			return IsCAVOrDCFEvent(log.SL_SE_NKEvent);
		}

		bool IsCAVOrDCFEvent(ZString eventCode)
		{
			return eventCode == Events.CargoAvailableCode || eventCode == Events.DeliveryCartageCompleteFinalisedCode;
		}

		#region CanExportShipment

		bool CanExportShipment(ForwardingShipment shipment)
		{
			return (!shipment.IsDeleted
				&& (shipment.DocsAndCartage.JP_LCLAvailable.IsValid || shipment.DocsAndCartage.JP_DeliveryCartageCompleted.IsValid)
				&& shipment.ArrivalConsol != null
				&& shipment.ArrivalConsol.JK_RL_NKDischargePort.SubstringSafe(0, 2) == shipment.JS_RL_NKDestination.SubstringSafe(0, 2)
				&& IsSendingAgentDeclaredInTheRegistry(shipment.ArrivalConsol));
		}

		#endregion

		#endregion
	}
}
