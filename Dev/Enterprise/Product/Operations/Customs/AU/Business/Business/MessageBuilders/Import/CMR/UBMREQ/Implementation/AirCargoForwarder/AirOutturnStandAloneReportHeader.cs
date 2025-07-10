using System.Collections;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirOutturnStandAloneReportHeader : IAirOutturnReportHeaderInformation
	{
		public AirOutturnStandAloneReportHeader(CusUnderbond underbond)
		{
			Argument.NotNull(underbond, "Underbond");

			this.underbond = underbond;
		}

		readonly CusUnderbond underbond;

		#region IAirOutturnReportHeaderInformation Members

		ZDateTime IAirOutturnReportHeaderInformation.DateTimeOfOutturn
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (!underbond.C4_Outurned.IsEmpty && underbond.C4_Outurned.IsValid)
				{
					if (OutturnHeaderWithSameKeyInfoExists)
					{
						underbond.C4_Outurned = underbond.C4_Outurned.AddMinutes(1);
					}

					result = GlbBranch.CurrentBranch.OrgProxy.ClosestPort.TimeZoneSet.GetCalculationTimeZone().ToUniversalTime(underbond.C4_Outurned.ToDateTime());
				}

				return result;
			}
		}

		bool OutturnHeaderWithSameKeyInfoExists
		{
			get
			{
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				ZDBOnlyQuery mAWBQuery = new CusMAWBBase.Loader(newFactory).GetMAWBQuery(underbond.C4_FlightNo, underbond.C4_ArrivalDate);
				ZDBOnlySubQuery underbondsQuery = new ZDBOnlySubQuery(typeof(CusUnderbond), CusUnderbondSchema.C4_ParentID);
				underbondsQuery.AddToFilter(CusUnderbondSchema.C4_Outurned, underbond.C4_Outurned);
				mAWBQuery.AddSubQuery(underbondsQuery, JoinCondition.And);

				CusMAWBBase[] mAWBs = newFactory.Load<CusMAWBBase>(mAWBQuery);
				return (mAWBs.Length > 0) || StandAloneOutturnHeaderWithSameKeyInfoExists;
			}
		}

		bool StandAloneOutturnHeaderWithSameKeyInfoExists
		{
			get
			{
				ZDBOnlyQuery standAloneUnderbondsQuery = new ZDBOnlyQuery(typeof(CusUnderbond));
				standAloneUnderbondsQuery.AddToFilter(CusUnderbondSchema.C4_ArrivalDate, underbond.C4_ArrivalDate);
				standAloneUnderbondsQuery.AddToFilter(CusUnderbondSchema.C4_FlightNo, underbond.C4_FlightNo);
				standAloneUnderbondsQuery.AddToFilter(CusUnderbondSchema.C4_Outurned, underbond.C4_Outurned);
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				CusUnderbond[] underbonds = newFactory.Load<CusUnderbond>(standAloneUnderbondsQuery);
				return underbonds.Length > 1 || (underbonds.Length == 1 && underbonds[0].PK != underbond.PK);
			}
		}

		ZString IAirOutturnReportHeaderInformation.FlightNumber
		{
			get { return underbond.C4_FlightNo; }
		}

		ZDateTime IAirOutturnReportHeaderInformation.EstimatedDateOfArrival
		{
			get { return underbond.C4_ArrivalDate; }
		}

		IAirOutturnReportLineInformation[] IAirOutturnReportHeaderInformation.Lines
		{
			get { return GetLines(underbond); }
		}

		IAirOutturnReportLineInformation[] IAirOutturnReportHeaderInformation.DatabaseLines
		{
			get
			{
				CusUnderbond underbondInNewFactory = new BusinessObjectFactory().Load<CusUnderbond>(underbond.PK);
				return GetLines(underbondInNewFactory);
			}
		}

		IAirOutturnReportLineInformation[] GetLines(CusUnderbond underbond)
		{
			ArrayList result = new ArrayList();
			if (underbond != null)
			{
				foreach (CusOutturn outturn in underbond.Outturns)
				{
					result.Add(new AirOutturnStandAloneReportLine(outturn));
				}
			}
			return (IAirOutturnReportLineInformation[])result.ToArray(typeof(IAirOutturnReportLineInformation));
		}

		ZString IOutturnReportHeaderInformation.ResponsiblePartyID
		{
			get { return underbond.C4_Calculated_ResponsiblePartyID; }
		}

		ZString IOutturnReportHeaderInformation.EstablishmentID
		{
			get { return underbond.C4_DestinationPremiseID; }
		}

		#endregion
	}
}
