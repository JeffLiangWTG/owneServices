using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManTranHeadSeaImpendingArrivalReportInformation : ISeaImpendingArrivalReportInformation
	{
		public CusSeaManTranHeadSeaImpendingArrivalReportInformation(CusSeaManTranHead header)
		{
			this.header = header;
		}

		public ZString LastOverseasPortOfDeparture
		{
			get { return header.BT_RL_NKPortOfLastForeignPort; }
		}

		public ZString LloydsNumber
		{
			get { return header.Vessel != null ? header.Vessel.RV_LloydsNumber : ZString.Empty; }
		}

		public ZString Voyage
		{
			get { return header.BT_VoyageNum; }
		}

		public ZDateTime DateTimeOfDepartureUTC
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				RefUNLOCO lastForeignPort = header.PortOfLastForeignPort;
				if (lastForeignPort != null && !header.BT_PortOfLastForeignPortATD.IsEmpty && header.BT_PortOfLastForeignPortATD.IsValid)
				{
					if (lastForeignPort.TimeZoneSet != null)
					{
						result = lastForeignPort.TimeZoneSet.GetCalculationTimeZone().ToUniversalTime(header.BT_PortOfLastForeignPortATD.ToDateTime());
					}
					else
					{
						result = header.BT_PortOfLastForeignPortATD;
					}
				}
				return result;
			}
		}

		public ZString[] SlotChartererIDs
		{
			get
			{
				ArrayList result = new ArrayList();
				foreach (CusSeaManSlotOrg slotPivot in header.SlotCharterers)
				{
					ZString iD = slotPivot.ABNOrCCID;
					if (!iD.IsEmpty)
					{
						result.Add(iD);
					}
				}
				return (ZString[])result.ToArray(typeof(ZString));
			}
		}

		public ZString PortOfFirstArrival
		{
			get { return header.FirstPortOfArrival; }
		}

		public IImpendingArrivalReportLineInformation[] Lines
		{
			get
			{
				ArrayList result = new ArrayList();
				foreach (CusSeaManArrivalPort arrival in header.Arrivals)
				{
					result.Add(new CusSeaManArrivalPortImpendingArrivalReportLineInformation(arrival));
				}
				return (IImpendingArrivalReportLineInformation[])result.ToArray(typeof(IImpendingArrivalReportLineInformation));
			}
		}

		public IImpendingArrivalReportLineInformation[] DatabaseLines
		{
			get
			{
				ArrayList result = new ArrayList();

				CusSeaManTranHead databaseHeader = new BusinessObjectFactory().Load<CusSeaManTranHead>(header.PK);
				if (databaseHeader != null)
				{
					foreach (CusSeaManArrivalPort arrival in databaseHeader.Arrivals)
					{
						result.Add(new CusSeaManArrivalPortImpendingArrivalReportLineInformation(arrival));
					}
				}
				return (IImpendingArrivalReportLineInformation[])result.ToArray(typeof(IImpendingArrivalReportLineInformation));
			}
		}

		public ZString ResponsiblePartyID
		{
			get { return header.BT_ResponsiblePartyID; }
		}

		#region Implementation

		readonly CusSeaManTranHead header;

		#endregion
	}
}
