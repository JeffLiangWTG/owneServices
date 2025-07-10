using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public class AvailabilityMenuItem : ZToolStripMenuItem
	{
		public AvailabilityMenuItem(GlbStaff resource, BMComponent buffer)
			: base(Res.GetString("cfd06403-c596-452d-9671-e8b907b9227d", "Availability"))
		{
			this.Name = "Availability";
			this.resource = resource;
			this.bufferPK = buffer.PK;

			AddTodayAndTomorrowMenuItems();
			AddAdvancedMenuItem();
		}

		void AddTodayAndTomorrowMenuItems()
		{
			var todayMenuItem = new ZToolStripMenuItem(Res.GetString("45b68d7b-4c1d-4198-9847-be8707a4g6982", "Today"));
			todayMenuItem.Name = "Today";
			this.DropDownItems.Add(todayMenuItem);
			todayMenuItem.DropDownItems.Add(GetAtWorkMenuItemForSpecificDate(ZDateTime.Now));
			todayMenuItem.DropDownItems.Add(GetAwayMenuItemForSpecificDate(ZDateTime.Now));

			var tomorrowMenuItem = new ZToolStripMenuItem(Res.GetString("fd7350ee-f647-4874-9eaa-2abaab0adf08", "Tomorrow"));
			tomorrowMenuItem.Name = "Tomorrow";
			this.DropDownItems.Add(tomorrowMenuItem);
			tomorrowMenuItem.DropDownItems.Add(GetAtWorkMenuItemForSpecificDate(ZDateTime.Now.AddDays(1)));
			tomorrowMenuItem.DropDownItems.Add(GetAwayMenuItemForSpecificDate(ZDateTime.Now.AddDays(1)));
		}

		void AddAdvancedMenuItem()
		{
			var advancedMenuItem = new ZToolStripMenuItem(Res.GetString("115fb0b3-5f42-42b9-b12f-b5500af686d1", "Advanced..."));
			advancedMenuItem.Name = "Advanced";
			advancedMenuItem.Click += advancedMenuItem_Click;

			this.DropDownItems.Add(advancedMenuItem);
		}

		void advancedMenuItem_Click(object sender, EventArgs e)
		{
			if (IsStaffAllowed())
			{
				AdvancedAvailabilityForm.ShowAdvancedAvailabilityForm(resource);
			}
		}

		ZToolStripMenuItem GetAtWorkMenuItemForSpecificDate(ZDateTime dateTime)
		{
			var atWorkMenuItem = new ZToolStripMenuItem(Res.GetString("ce7147e1-7641-49eb-b3e1-aaddcebd0de2", "At Work"));
			atWorkMenuItem.Name = "AtWork";
			atWorkMenuItem.Click += AtWorkMenuItem_Click(dateTime);

			return atWorkMenuItem;
		}

		ZToolStripMenuItem GetAwayMenuItemForSpecificDate(ZDateTime dateTime)
		{
			var awayMenuItem = new ZToolStripMenuItem(Res.GetString("3bd62f49-cc2d-45cf-8063-48c60243fabc", "Away"));
			awayMenuItem.Name = "Away";
			awayMenuItem.Click += AwayMenuItem_Click(dateTime);

			return awayMenuItem;
		}

		readonly GlbStaff resource;
		readonly ZGuid bufferPK;

		EventHandler AtWorkMenuItem_Click(ZDateTime dateTimeToTest)
		{
			return (s, e) =>
			{
				if (IsStaffAllowed())
				{
					var bmsLeave = GetBMSLeaveRecord(resource.HolidaysIncBMSLeave, dateTimeToTest);
					var leave = GetLeaveRecord(resource.HolidaysIncBMSLeave, dateTimeToTest);

					if (bmsLeave == null)
					{
						string message;

						if (leave == null)
						{
							message = resource.IsWorkingRightNow ? ResourceAlreadyAtWorkMessage : ResourceOutsideOfWorkingHoursMessage;
						}
						else
						{
							message = Res.GetString("00c8d9b9-1a6a-4743-8d5f-f9ac18f7347e", "A leave record of type: {0} exists.", leave.GA_WorkHolidayType);
						}

						Globals.Message.Show(message, Res.GetString("5e256482-b395-45f6-a07c-f83c83580f9e", "Unable to change Availability"), MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
					else
					{
						resource.HolidaysIncBMSLeave.Remove(bmsLeave);
						bmsLeave.Delete();
						bmsLeave.Factory.SaveHandlingZSaveExceptions();
					}
				}
			};
		}

		public static string ResourceAlreadyAtWorkMessage
		{
			get { return Res.GetString("8b7c18ad-43ad-4c4b-85b9-34eb95613726", "This resource cannot be marked as 'At Work' because there is no BMS Leave record to remove for the day specified."); }
		}

		public static string ResourceOutsideOfWorkingHoursMessage
		{
			get { return Res.GetString("6e0328bd-d9a8-4af2-bf63-a624e11e0a88", "It is currently outside the resource's defined working hours, which cannot be changed here. Please update the resource's record directly."); }
		}

		void CreateAvailabilityOverride(ZDateTime dateTimeForLeave)
		{
			var factory = resource.Factory;
			var availabilityOverride = factory.New<BMSResourceAvailabilityOverride>();
			availabilityOverride.GA_DaysLeaveTaken = 1.0m;
			availabilityOverride.GA_AvailabilityPercentage = 0;
			availabilityOverride.GA_ParentID = bufferPK;
			availabilityOverride.GA_GS = resource.PK;
			availabilityOverride.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			availabilityOverride.GA_StartTime = new ZDateTime(dateTimeForLeave.Date);
			availabilityOverride.GA_EndTime = new ZDateTime(dateTimeForLeave.AddDays(1).Date);
			factory.SaveHandlingZSaveExceptions();
			resource.HolidaysIncBMSLeave.Add(availabilityOverride);
		}

		EventHandler AwayMenuItem_Click(ZDateTime dateTimeToTest)
		{
			return (s, e) =>
			{
				if (IsStaffAllowed())
				{
					var leave = GetLeaveRecord(resource.HolidaysIncBMSLeave, dateTimeToTest);
					if (leave != null)
					{
						Globals.Message.Show(Res.GetString("00c8d9b9-1a6a-4743-8d5f-f9ac18f7347e", "A leave record of type: {0} exists.", leave.GA_WorkHolidayType), Res.GetString("5e256482-b395-45f6-a07c-f83c83580f9e", "Unable to change Availability"), MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
					else
					{
						CreateAvailabilityOverride(dateTimeToTest);
					}
				}
			};
		}

		GlbStaffHoliday GetLeaveRecord(GlbStaffHolidayIncBMSLeaveCollection holidays, ZDateTime dateTimeToTest)
		{
			return holidays.Cast<GlbStaffHoliday>().FirstOrDefault(h => h.GA_StartTime <= dateTimeToTest && h.GA_EndTime > dateTimeToTest);
		}

		GlbStaffHoliday GetBMSLeaveRecord(GlbStaffHolidayIncBMSLeaveCollection holidays, ZDateTime dateTimeToTest)
		{
			return holidays.Cast<GlbStaffHoliday>().FirstOrDefault(h => h.GA_RecordType == BMConstants.BMSLeaveType && h.GA_StartTime <= dateTimeToTest && h.GA_EndTime > dateTimeToTest);
		}

		bool IsStaffAllowed()
		{
			if (Env.CurrentUser.PK == resource.PK)
			{
				if (Env.Security.BMBoardChangeBMSAvailabilityForCurrentUser.IsAllowed)
				{
					return true;
				}
				else
				{
					Env.Security.BMBoardChangeBMSAvailabilityForCurrentUser.ShowError();
					return false;
				}
			}
			else
			{
				if (Env.Security.BMBoardChangeBMSAvailabilityForOtherUsers.IsAllowed)
				{
					return true;
				}
				else
				{
					Env.Security.BMBoardChangeBMSAvailabilityForOtherUsers.ShowError();
					return false;
				}
			}
		}
	}
}
