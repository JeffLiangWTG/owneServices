using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Mail.Business
{
	public class EDIMailItem : MailItem
	{
		public EDIMailItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString LastStatusChangedBy
		{
			get
			{
				ZQuery query = new ZQuery(StmALogSchema.SL_Parent, PK);
				query.AddToFilter(StmALogSchema.SL_IsCancelled, ZBool.False.ToString());
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.EditedARecordCode);
				query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + " DESC";
				StmALog log = Factory.LoadTop1<StmALog>(query);
				return (log != null) ? log.SL_UserNameAndInitials : ZString.Empty;
			}
		}

		public ZPropertyInfo LastStatusChangedByInfo
		{
			get { return GetZPropertyInfo(nameof(LastStatusChangedBy)); }
		}

		public override void OnSaving()
		{
			if (IsInDatabase && MI_StatusInfo.HasChanges && MI_Application == EDIMailApplication.CustomerService)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
			base.OnSaving();
		}
	}
}

