using System;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	class StartupShowStaffForm : IPostLoginTask
	{
		public string TaskDescription
		{
			get { return Res.GetString("b742509c-0a31-48a0-a5c0-9322e0672780", "Staff Form Displayer"); }
		}

		public bool ShouldExecute()
		{
			return true;
		}

		public void Execute()
		{
			ShowStaffFormEveryXDays();
		}

		protected void ShowStaffFormEveryXDays()
		{
			if (ShouldShowStaffFormEveryXDays() && HasBeenXDays())
			{
				ShowStaffForm();
			}
		}

		bool ShouldShowStaffFormEveryXDays()
		{
			var currentUser = GlbStaff.CurrentUser;

			return !currentUser.GS_IsSystemAccount
				&& !currentUser.GS_IsRobot
				&& (Env.Security.StaffOwnDetails.IsAllowed || Env.Security.StaffDetails.IsAllowed)
				&& (Env.Security.StaffView.IsAllowed || Env.Security.StaffEdit.IsAllowed)
				&& Env.Security.FindOrCreateAccessModuleCheckPoint(Env.Security.Staff).IsAllowed
				&& Env.Registry.StaffDetailsUpdateFrequency > 0;
		}

		bool HasBeenXDays()
		{
			DateTime dateToday = Env.Time.CurrentLocalDateTime;
			DateTime staffFormShouldShown = dateToday.AddDays(1);
			try
			{
				int frequency = Env.Registry.StaffDetailsUpdateFrequency;
				staffFormShouldShown = Env.Registry.DateTimeStaffFormWasLastShown.AddDays(frequency);
			}
			catch (ArgumentOutOfRangeException)
			{
				return false;
			}

			return staffFormShouldShown <= dateToday;
		}

		void ShowStaffForm()
		{
			ShowAlertThatStaffFormIsShownEveryXDays();

			var staffPK = Env.Instance.CurrentUserPK;
			var (controller, bizo) = ZControllerFactory.GetCorrectControllerAndBusinessObject(ControllerIDs.GlbStaff, staffPK, true);

			var form = (GlbStaffForm)controller.ShowEditForm(bizo);
			form.DisableNewAction();

			Env.Registry.DateTimeStaffFormWasLastShown = Env.Time.CurrentLocalDateTime;
		}

		void ShowAlertThatStaffFormIsShownEveryXDays()
		{
			int frequency = Env.Registry.StaffDetailsUpdateFrequency;

			string showStaffFormMsg = Res.GetString("3f37d9d8-2755-49aa-9048-60e5d0d21157", "Your staff profile is displayed every {0} days to ensure that the details are kept up to date. Please make sure your details are still current.", frequency);
			Globals.Message.Show(showStaffFormMsg,
				Res.GetString("9f68a5d9-b4db-450e-afec-ae699ffa4058", "Staff Details"),
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);
		}
	}
}
