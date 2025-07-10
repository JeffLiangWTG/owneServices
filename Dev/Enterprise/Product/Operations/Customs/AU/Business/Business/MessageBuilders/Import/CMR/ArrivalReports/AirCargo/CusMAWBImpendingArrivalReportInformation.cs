using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusMAWBImpendingArrivalReportInformation : CusMAWBArrivalReportInformation, IAirImpendingArrivalReportInformation
	{
		public CusMAWBImpendingArrivalReportInformation(CusMAWB mAWB)
			: base(mAWB)
		{
		}

		public ZString PortOfFirstArrival
		{
			get { return mAWB.CM_RL_NKFirstArrivalPort; }
		}

		public ZString FlightNo
		{
			get { return mAWB.CM_FlightNo; }
		}

		public IImpendingArrivalReportLineInformation[] Lines
		{
			get { return new IImpendingArrivalReportLineInformation[] { new CusMAWBImpendingArrivalReportLineInformation(mAWB) }; }
		}

		public IImpendingArrivalReportLineInformation[] DatabaseLines
		{
			get
			{
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				CusMAWB databaseMAWB = newFactory.Load<CusMAWB>(mAWB.PK);
				return databaseMAWB == null ? System.Array.Empty<IImpendingArrivalReportLineInformation>() : new IImpendingArrivalReportLineInformation[] { new CusMAWBImpendingArrivalReportLineInformation(databaseMAWB) };
			}
		}
	}
}
