using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTOCusPartShipCargoReportHeader : CTOCusHAWBCargoReportHeader, IAirCargoReportHeader
	{
		public CTOCusPartShipCargoReportHeader(CusPartShip partShip, CTOCusHAWB cTOHAWB)
			: base(cTOHAWB)
		{
			this.partShip = partShip;
		}

		// v2 of the runtime seems to be calling the non-explicit implementation of this in the base class
		// instead of the explicit implementation

		ZString IAirCargoReportHeader.MAWB
		{
			get { return HAWB.CS_HAWB; }
		}

		ZString IAirCargoReportHeader.MasterHouseBill
		{
			get { return ZString.Empty; }
		}

		ZString IAirCargoReportHeader.HAWBNum
		{
			get { return ZString.Empty; }
		}

		ZString IAirCargoReportHeader.MatchConsignmentReference
		{
			get { return ZString.Empty; }
		}

		ZString IAirCargoReportHeader.FlightNo
		{
			get { return partShip.CG_FlightNo; }
		}

		ZDateTime IAirCargoReportHeader.ArivalDate
		{
			get { return partShip.CG_ArrivalDate; }
		}

		ZString ICargoReportHeader.Loading
		{
			get { return partShip.CG_RL_NKLoadPort; }
		}

		ZString ICargoReportHeader.Discharge
		{
			get { return partShip.CG_RL_NKDischargePort; }
		}

		int IAirCargoReportHeader.PackageCount
		{
			get { return partShip.CG_PiecesLanded; }
		}

		ZString[] ICargoReportHeader.Routings
		{
			get { return null; }
		}

		bool IAirCargoReportHeader.IsHVLVSpecialReporter
		{
			get { return false; }
		}

		bool IAirCargoReportHeader.IsRemailSpecialReporter
		{
			get { return false; }
		}

		#region Implementation

		readonly CusPartShip partShip;

		#endregion
	}
}
