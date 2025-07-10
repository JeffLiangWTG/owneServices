using System;
using System.Linq;

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.MFI.CaroTrans
{
	public class CaroTransConsolAvailableListener : CaroTransShipmentListener
	{
		public CaroTransConsolAvailableListener(ZString exportPath)
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
			return IsDCFOrCAVevent(log.SL_SE_NKEvent);
		}

		bool IsDCFOrCAVevent(ZString @event)
		{
			return @event == Events.DeliveryCartageCompleteFinalisedCode || @event == Events.CargoAvailableCode;
		}

		#region GetShipmentsToExport

		protected override ShipmentCollection GetShipmentsToExport(BusinessObject matchingBusinessObject)
		{
			ForwardingConsol foundConsol = (ForwardingConsol)matchingBusinessObject;
			var result = new ShipmentCollection(Factory);

			if (IsSendingAgentDeclaredInTheRegistry(foundConsol))
			{
				foreach (CommonContainer container in foundConsol.Containers)
				{
					if (container.JC_FCLAvailable.IsValid || container.JC_LCLAvailable.IsValid)
					{
						foreach (ForwardingShipment shipment in foundConsol.Shipments.Cast<ForwardingShipment>().
																Where(s => s.ArrivalConsol != null && s.ArrivalConsol.PK == foundConsol.PK))
						{
							if (!result.Contains(shipment) && shipment.Containers.Contains(container))
							{
								result.Add(shipment);
							}
						}
					}
				}
			}

			return result;
		}

		#endregion

		#endregion
	}
}
