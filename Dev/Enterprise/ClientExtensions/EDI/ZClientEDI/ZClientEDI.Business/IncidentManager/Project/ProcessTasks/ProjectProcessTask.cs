using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class ProjectProcessTask : ProcessManagement.Business.ProjectProcessTask
	{
		public ProjectProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(EDIProject); }
		}

		public new EDIProject Parent
		{
			get { return (EDIProject)base.Parent; }
		}

		public override ZString P9_Status
		{
			get { return base.P9_Status; }
			set
			{
				if (base.P9_Status != value)
				{
					base.P9_Status = value;
					if (IsRowCommittedAndHasParent)
					{
						Parent.CalculateStatus();
					}
				}
			}
		}

		bool IsRowCommittedAndHasParent
		{
			get { return ((INeedRow)this).Row.RowState != DataRowState.Detached && Parent != null; }
		}

		#region Reminder Time Zone

		protected override ITimeZone ReminderTimeZone
		{
			get
			{
				if (this.AssignedStaffMember == null || this.AssignedStaffMember.HomeBranch == null)
				{
					return base.ReminderTimeZone;
				}

				RefUNLOCO loco = this.AssignedStaffMember.HomeBranch.HomePort;

				if (loco != null && loco.TimeZoneSet != null)
				{
					return loco.TimeZoneSet.GetCalculationTimeZone();
				}
				else
				{
					return base.ReminderTimeZone;
				}
			}
		}

		#endregion
	}
}

