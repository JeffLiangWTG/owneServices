using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManArrivalPortCargoListReportHeader : ICargoListReportHeader
	{
		public CusSeaManArrivalPortCargoListReportHeader(CusSeaManArrivalPort arrivalPort)
		{
			this.arrivalPort = arrivalPort;
		}

		public ZString CargoResponsiblePartyID
		{
			get { return arrivalPort.Header.BT_ResponsiblePartyID; }
		}

		public ZString LloydsNumber
		{
			get { return arrivalPort.Header.BT_LloydsIMO; }
		}

		public ZString VoyageNumber
		{
			get { return arrivalPort.Header.BT_VoyageNum; }
		}

		public ZString DischargePort
		{
			get { return arrivalPort.BA_RL_NKArrivalPort; }
		}

		public ICargoListReportLine[] Lines
		{
			get { return GetLines(arrivalPort); }
		}

		public ICargoListReportLine[] DatabaseLines
		{
			get
			{
				CusSeaManArrivalPort dBArrivalPort = new BusinessObjectFactory().Load<CusSeaManArrivalPort>(arrivalPort.PK);
				return GetLines(dBArrivalPort);
			}
		}

		#region Implemention

		readonly CusSeaManArrivalPort arrivalPort;

		ICargoListReportLine[] GetLines(CusSeaManArrivalPort arrival)
		{
			ArrayList result = new ArrayList();
			if (arrival != null)
			{
				foreach (CusSeaManOBLDetailCargoLine detail in arrival.GetCargoListDetails())
				{
					result.Add(new CusSeaManOBLDetailCargoListReportLine(detail));
				}
			}
			return (ICargoListReportLine[])result.ToArray(typeof(ICargoListReportLine));
		}

		#endregion
	}
}
