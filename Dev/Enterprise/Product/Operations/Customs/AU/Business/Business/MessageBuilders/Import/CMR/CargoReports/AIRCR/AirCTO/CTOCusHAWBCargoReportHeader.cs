using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTOCusHAWBCargoReportHeader : CusHAWBAirCargoReportHeader, IAirCargoReportHeader
	{
		public CTOCusHAWBCargoReportHeader(CTOCusHAWB cTOHAWB)
			: base(cTOHAWB)
		{
		}

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

		ZString ICargoReportHeader.Loading
		{
			get { return HAWB.CS_RL_NKLoadPort; }
		}

		bool IAirCargoReportHeader.IsHVLVSpecialReporter
		{
			get { return false; }
		}

		bool IAirCargoReportHeader.IsRemailSpecialReporter
		{
			get { return false; }
		}
	}
}
