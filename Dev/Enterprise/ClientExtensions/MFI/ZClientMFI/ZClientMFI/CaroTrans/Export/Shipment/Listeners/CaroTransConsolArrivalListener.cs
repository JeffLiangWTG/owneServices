using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.MFI.CaroTrans
{
	public class CaroTransConsolArrivalListener : CaroTransShipmentListener
	{
		public CaroTransConsolArrivalListener(ZString exportPath)
			: base(exportPath)
		{ }

		public override string BusinessObjectTableName
		{
			get { return JobConsolSchema.Constants.TableName; }
		}

		public override Type BusinessObjectType
		{
			get { return typeof(ForwardingConsol); }
		}

		#region Implementation

		protected override bool AdditionalMatching(StmALog log)
		{
			return (log.SL_SE_NKEvent == Events.Arrival.Code && !log.SL_IsEstimate);
		}

		#region GetShipmentsToExport

		protected override ShipmentCollection GetShipmentsToExport(BusinessObject matchingBusinessObject)
		{
			ForwardingConsol foundConsol = (ForwardingConsol)matchingBusinessObject;
			ShipmentCollection result = new ShipmentCollection(Factory);

			if (IsSendingAgentDeclaredInTheRegistry(foundConsol))
			{
				foreach (ForwardingShipment shipment in foundConsol.Shipments.Cast<ForwardingShipment>().
														Where(s => s.ArrivalConsol != null
															&& s.ArrivalConsol.JK_RL_NKDischargePort.SubstringSafe(0, 2) == s.JS_RL_NKDestination.SubstringSafe(0, 2)
															&& s.ArrivalConsol.PK == foundConsol.PK))
				{
					var lastLeg = new TransportOrderHelper(foundConsol.Transports).LastLeg;
					if (lastLeg != null && lastLeg.JW_ATA.IsValid && !result.Contains(shipment))
					{
						result.Add(shipment);
					}
				}
			}

			return result;
		}

		#endregion

		#endregion
	}
}
