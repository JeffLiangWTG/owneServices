using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaCargoDepotGatePass : SeaCargoDepotBusinessObject
	{
		protected SeaCargoDepotGatePass(GatePassShipment shipment)
			: base(shipment)
		{
			fShipment = shipment;
		}

		public static SeaCargoDepotGatePass Load(GatePassShipment shipment)
		{
			return (SeaCargoDepotGatePass)Load(typeof(SeaCargoDepotGatePass), shipment);
		}

		#region Properties

		public override CFSLoadListConsol ParentConsol
		{
			get
			{
				CFSLoadListConsol result = null;
				if (Shipment != null)
				{
					result = Shipment.ArrivalConsol as CFSLoadListConsol;
				}
				return result;
			}
		}

		public GatePassShipment Shipment
		{
			get
			{
				return fShipment;
			}
		}

		public CommonContainer Container
		{
			get
			{
				CommonContainer result = null;
				foreach (PackLine packLine in Shipment.OuterPackLines)
				{
					result = packLine.GetContainer(ParentConsol);
					if (result != null)
					{
						break;
					}
				}
				return result;
			}
		}

		public CusSCADepotHouseCollection ReportedHouseBills
		{
			get
			{
				if (fReportedHouseBills == null)
				{
					fReportedHouseBills = new CusSCADepotHouseCollection(Shipment, Factory);
					fReportedHouseBills.Load();
				}
				//fReportedHouseBills.Sort(// On EventTime)
				return fReportedHouseBills;
			}
		}

		#endregion

		#region Implementation

		readonly GatePassShipment fShipment;
		CusSCADepotHouseCollection fReportedHouseBills;

		protected ZString DeliveriesExist()
		{
			ZString result = ZString.Empty;
			if (Shipment.OuterPackLines.Count == 0 || Shipment.OuterPackLines[0].Containers.Count == 0)
			{
				result = "Unable to locate the container associated with this house bill";
			}
			return result;
		}

		#endregion

	}
}
