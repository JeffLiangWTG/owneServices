using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CusMAWBBaseOutturnReportHeaderInformation : CusUnderbondOutturnReportHeaderInformation, IAirOutturnReportHeaderInformation
	{
		public CusMAWBBaseOutturnReportHeaderInformation(CusMAWBBase mAWB, CusUnderbond underbond)
			: base(underbond)
		{
			this.mAWB = mAWB;
		}

		public ZDateTime DateTimeOfOutturn
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (!Underbond.C4_Outurned.IsEmpty && Underbond.C4_Outurned.IsValid)
				{
					if (OutturnHeaderWithSameKeyInfoExists)
					{
						Underbond.C4_Outurned = Underbond.C4_Outurned.AddMinutes(1);
					}

					var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
					var closestPort = orgProxy != null ? orgProxy.ClosestPort : null;
					var timeZone = closestPort != null ? closestPort.TimeZoneSet : null;

					if (timeZone != null)
					{
						result = timeZone.GetCalculationTimeZone().ToUniversalTime(Underbond.C4_Outurned.ToDateTime());
					}
				}

				return result;
			}
		}

		bool OutturnHeaderWithSameKeyInfoExists
		{
			get
			{
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				ZDBOnlyQuery mAWBQuery = new CusMAWBBase.Loader(newFactory).GetMAWBQuery(mAWB.CM_FlightNo, mAWB.CM_ArrivalDate);
				ZDBOnlySubQuery underbondsQuery = new ZDBOnlySubQuery(typeof(CusUnderbond), CusUnderbondSchema.C4_ParentID);
				underbondsQuery.AddToFilter(CusUnderbondSchema.C4_Outurned, Underbond.C4_Outurned);
				mAWBQuery.AddSubQuery(underbondsQuery, JoinCondition.And);

				CusMAWBBase[] mAWBs = newFactory.Load<CusMAWBBase>(mAWBQuery);
				return mAWBs.Length > 1 || (mAWBs.Length == 1 && mAWBs[0].PK != mAWB.PK);
			}
		}

		public ZString FlightNumber
		{
			get { return Underbond.C4_FlightNo.IsEmpty ? mAWB.CM_FlightNo : Underbond.C4_FlightNo; }
		}

		public ZDateTime EstimatedDateOfArrival
		{
			get { return Underbond.C4_ArrivalDate.IsEmpty ? mAWB.CM_ArrivalDate : Underbond.C4_ArrivalDate; }
		}

		public IAirOutturnReportLineInformation[] Lines
		{
			get { return GetLines(Underbond); }
		}

		public IAirOutturnReportLineInformation[] DatabaseLines
		{
			get
			{
				CusUnderbond underbondInNewFactory = new BusinessObjectFactory().Load<CusUnderbond>(Underbond.PK);
				return GetLines(underbondInNewFactory);
			}
		}

		#region Implementation

		protected abstract IAirOutturnReportLineInformation[] GetLines(CusUnderbond underbond);

		readonly CusMAWBBase mAWB;

		#endregion
	}
}
