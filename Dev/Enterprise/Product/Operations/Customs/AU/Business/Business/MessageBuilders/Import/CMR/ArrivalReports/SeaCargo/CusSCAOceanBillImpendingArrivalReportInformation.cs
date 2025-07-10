using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAOceanBillImpendingArrivalReportInformation : CusSCAOceanBillArrivalReportInformation, ISeaImpendingArrivalReportInformation
	{
		public CusSCAOceanBillImpendingArrivalReportInformation(CusSCAOceanBill oceanBill)
			: base(oceanBill)
		{
		}

		public ZDateTime DateTimeOfDepartureUTC
		{
			get
			{
				return ZDateTime.Now.AddDays(-2);
			}
		}

		public ZString PortOfFirstArrival
		{
			get { return oceanBill.CB_RL_NKPortOfFirstArrival; }
		}

		public ZString Voyage
		{
			get
			{
				return oceanBill.CB_Voyage;
			}
		}

		public ZString LloydsNumber
		{
			get
			{
				return oceanBill.CB_LloydsIMO;
			}
		}

		public ZString[] SlotChartererIDs
		{
			get { return System.Array.Empty<ZString>(); }
		}

		public IImpendingArrivalReportLineInformation[] Lines
		{
			get { return new IImpendingArrivalReportLineInformation[] { new CusSCAOceanBillImpendingArrivalReportLineInformation(oceanBill) }; }
		}

		public IImpendingArrivalReportLineInformation[] DatabaseLines
		{
			get
			{
				var newFactory = new BusinessObjectFactory();
				var databaseOceanBill = newFactory.Load<CusSCAOceanBill>(oceanBill.PK);
				return databaseOceanBill == null ? System.Array.Empty<IImpendingArrivalReportLineInformation>() : new IImpendingArrivalReportLineInformation[] { new CusSCAOceanBillImpendingArrivalReportLineInformation(databaseOceanBill) };
			}
		}
	}
}
