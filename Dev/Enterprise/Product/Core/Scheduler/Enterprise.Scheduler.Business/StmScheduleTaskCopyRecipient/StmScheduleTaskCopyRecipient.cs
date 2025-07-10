using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Scheduler.Business
{
	public class StmScheduleTaskCopyRecipient : AutoStmScheduleTaskCopyRecipient
	{
		public StmScheduleTaskCopyRecipient(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region SCR_EmailAddress

		[List("Lookups.SCR_AvailableEmailAddress_List")]
		[EmailAddress]
		public override ZString SCR_EmailAddress
		{
			get { return base.SCR_EmailAddress; }
			set { base.SCR_EmailAddress = value; }
		}

		#endregion

		#region Recipient

		public virtual StmScheduleTaskRecipient Recipient
		{
			get { return Factory.Load<StmScheduleTaskRecipient>(SCR_S6); }
		}

		[RelatedBusinessObject("Recipient")]
		public override ZGuid SCR_S6
		{
			get { return base.SCR_S6; }
			set { base.SCR_S6 = value; }
		}

		#endregion
	}
}
