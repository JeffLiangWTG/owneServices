using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.MFI.CaroTrans
{
	public class CaroTransShipmentCustomsListener : CaroTransShipmentListener
	{
		public CaroTransShipmentCustomsListener(ZString exportPath)
			: base(exportPath)
		{ }

		public override string BusinessObjectTableName
		{
			get { return JobDeclarationSchema.Constants.TableName; }
		}

		public override Type BusinessObjectType
		{
			get { return typeof(JobDeclaration); }
		}

		#region Implementation

		protected override ShipmentCollection GetShipmentsToExport(BusinessObject matchingBusinessObject)
		{
				JobDeclaration foundJobDec = (JobDeclaration)matchingBusinessObject;
				var result = new ShipmentCollection(Factory);

				if (CanExportShipment(foundJobDec.Shipment))
				{
					result.Add(foundJobDec.Shipment);
				}

				return result;
		}

		#region CanExportShipment

		bool CanExportShipment(ForwardingShipment shipment)
		{
			return shipment != null
				&& shipment.Consols.Count > 0
				&& shipment.ArrivalConsol != null
				&& shipment.ArrivalConsol.JK_RL_NKDischargePort.SubstringSafe(0, 2) == shipment.JS_RL_NKDestination.SubstringSafe(0, 2)
				&& IsSendingAgentDeclaredInTheRegistry(shipment.ArrivalConsol);
		}

		#endregion

		#endregion
	}
}
