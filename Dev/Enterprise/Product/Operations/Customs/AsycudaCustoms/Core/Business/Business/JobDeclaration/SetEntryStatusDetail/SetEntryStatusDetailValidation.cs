using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class SetEntryStatusDetailValidation : AutoSetEntryStatusDetailValidation
	{
		public SetEntryStatusDetailValidation(AutoSetEntryStatusDetail parent)
			: base(parent)
		{
		}

		protected override void CheckCusEntryHeaderPK()
		{
			base.CheckCusEntryHeaderPK();
			var cusEntryHeaderPKInfo = Parent.CusEntryHeaderPKInfo;
			MandatoryValidation.CheckEntered(cusEntryHeaderPKInfo);
			ListValidation.ErrorIfInvalidPK(cusEntryHeaderPKInfo);
		}

		protected override void CheckEventTime()
		{
			base.CheckEventTime();
			var parent = Parent;
			var parentEventTimeInfo = parent.EventTimeInfo;
			MandatoryValidation.CheckEntered(parentEventTimeInfo);
			if (parent.EventTime.IsInTheFuture())
			{
				parentEventTimeInfo.AddError(Res.GetString("3f4712e9-67bc-4f24-a9d1-4154cd3e6c4c6", "Event time should not be in the future."));
			}
		}

		protected override void CheckEntryStatus()
		{
			base.CheckEntryStatus();
			var parent = Parent;
			var parentEntryStatusInfo = parent.EntryStatusInfo;
			MandatoryValidation.CheckEntered(parentEntryStatusInfo);
			ListValidation.ErrorIfInvalidCode(parentEntryStatusInfo);

			if ((parent as SetEntryStatusDetail)?.EntryHeader is CusEntryHeader entryHeader)
			{
				var entryHeaderEntryStatus = entryHeader.CH_EntryStatus;
				if (parent.EntryStatus.Equals(entryHeaderEntryStatus))
				{
					var notificationType = NotificationType.Error;
					if (entryHeader.ShouldLogEntryStatus)
					{
						var filter = new ZQuery(StmALogSchema.SL_Parent, entryHeader.PK);
						filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);
						filter.OrderBy = StmALogSchema.Constants.SL_EventTime + OrderByClause.Descending;
						if (parent.Factory.Load<StmALog>(filter).FirstOrDefault() is StmALog mostRecentLog && mostRecentLog.IsCancelled && mostRecentLog.SL_Reference == entryHeaderEntryStatus)
						{
							notificationType = NotificationType.Warning;
						}
					}

					parentEntryStatusInfo.AddNotification(notificationType, Res.GetString("A74F03A7-DE24-4CF9-AE97-9AFB3111D906", "Entry Status must be changed."));
				}
			}
		}
	}
}
