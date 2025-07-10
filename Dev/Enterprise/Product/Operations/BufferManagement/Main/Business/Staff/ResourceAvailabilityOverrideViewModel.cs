using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class ResourceAvailabilityOverrideViewModel : NonPersistentBusinessObject
	{
		public ResourceAvailabilityOverrideViewModel(GlbStaff staff)
			: base(staff.Factory)
		{
			this.staff = staff;
		}

		readonly GlbStaff staff;

		public BMSResourceAvailabilityOverrideCollection BMSLeave
		{
			get
			{
				if (bmsLeave == null)
				{
					var query = new ZQuery(GlbStaffHolidaySchema.GA_RecordType, BMConstants.BMSLeaveType);
					bmsLeave = new BMSResourceAvailabilityOverrideCollection(staff, query);
				}
				return bmsLeave;
			}
		}

		BMSResourceAvailabilityOverrideCollection bmsLeave;

		public FutureLeaveCollection NearFutureLeave
		{
			get
			{
				if (nearFutureLeave == null)
				{
					var query = new ZQuery(GlbStaffHolidaySchema.GA_RecordType, SQLComparisonOperator.Equal, GlbStaffHolidayLookups.RecordTypes.Leave);
					query.AddToFilter(FutureLeaveCollection.GetFilterForFutureLeaveWithinXDays(30));
					nearFutureLeave = new FutureLeaveCollection(staff, query);
					nearFutureLeave.SetReadOnlyIncludingChildren(true);
				}
				return nearFutureLeave;
			}
		}

		FutureLeaveCollection nearFutureLeave;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "This is a viewmodel")]
		public void Save()
		{
			try
			{
				foreach (var leave in BMSLeave)
				{
					leave.Validation.ValidateAll();
				}

				if (!BMSLeave.Any(l => l.HasErrors))
				{
					Factory.Save();

					if (Closed != null)
					{
						Closed(this, EventArgs.Empty);
					}
				}
				else
				{
					var errors = BMSLeave.SelectMany(l => l.Notifications).Where(n => n.Type == NotificationType.Error).Select(n => n.Message);
					Globals.Message.ShowError(string.Join(System.Environment.NewLine, errors)); // This is a viewModel
				}
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		public event EventHandler Closed;
	}
}
