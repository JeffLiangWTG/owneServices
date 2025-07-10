using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.ActiveDirectory;
using CargoWise.Application;
using CargoWise.BrandManager;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using FormsTimer = System.Windows.Forms.Timer;

namespace Enterprise.Security.ActiveDirectory.GUI
{
	public class ADPasswordExpiryChecker
	{
		public static ADPasswordExpiryChecker Instance
		{
			get { return instance ?? (instance = new ADPasswordExpiryChecker()); }
		}
		[ThreadStatic]
		static ADPasswordExpiryChecker instance;

		public void Enable()
		{
			if (!isEnabled && IsIntegrationEnabled)
			{
				ReminderTimer.Tick += (sender, eventArgs) => Run();
				ReminderTimer.Enabled = true;
				isEnabled = true;
				Run();
			}
		}
		bool isEnabled;

		FormsTimer ReminderTimer
		{
			get
			{
				if (reminderTimer == null)
				{
					reminderTimer = new FormsTimer();
					reminderTimer.Interval = (int)TimeSpan.FromDays(1).TotalMilliseconds;
				}

				return reminderTimer;
			}
		}
		FormsTimer reminderTimer;

		public void Run()
		{
			var enabled = ReminderTimer.Enabled;
			ReminderTimer.Enabled = false;
			try
			{
				try
				{
					var staff = GlbStaff.CurrentUser;
					if (IsIntegrationEnabled && AllowADPasswordChange && ShouldDisplayReminder(staff))
					{
						var notifyIcon = new NotifyIcon();
						notifyIcon.Icon = BrandingFactory.Instance.ProductIcon;
						notifyIcon.Click += (s, e) => { ShowChangePasswordForm(staff); };
						notifyIcon.BalloonTipClicked += (s, e) => { ShowChangePasswordForm(staff); };
						notifyIcon.BalloonTipClosed += (s, e) => { notifyIcon.Visible = false; };
						notifyIcon.Visible = true;
						notifyIcon.ShowBalloonTip(showTimeMilliseconds,
							Res.GetString("a3add5d8-0769-41d7-9dda-bbd5f142045d", "Password about to expire"),
							Res.GetString("c6b16a15-4477-455a-afde-83d1123ff9c0", "Your Active Directory password will expire in {0} days.\r\nClick here to change it now.", staff.GetADNumberOfDaysTillPasswordExpiry()),
							ToolTipIcon.Info);
						Task.Factory.StartNew(() =>
						{
							Thread.Sleep(showTimeMilliseconds);
							if (notifyIcon != null)
							{
								notifyIcon.Visible = false;
								notifyIcon.Icon = null;
								notifyIcon.Dispose();
								notifyIcon = null;
							}
						}, CancellationToken.None, TaskCreationOptions.None, ObjectFactory.Get<TaskScheduler>());

						FireNotificationDisplayed();
					}
				}
				finally
				{
					ReminderTimer.Enabled = enabled;
				}
			}
			catch (NoDomainPrivilegeException)
			{
				// Just ignore when it can't access to the domain.
				// This is to avoid crash during login when the Domain User Credentials is incorrect/outdated.
			}
		}

		bool IsIntegrationEnabled => ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled;

		bool AllowADPasswordChange => !ObjectFactory.Get<IADRegistry>().DisableADPasswordChange;

		bool ShouldDisplayReminder(GlbStaff staff) => staff != null && staff.IsADLinked && staff.GetADNumberOfDaysTillPasswordExpiry() <= DataRegistry.Instance.PromptPasswordChangeBeforeExpireDays;

		void ShowChangePasswordForm(GlbStaff staff) => ChangePasswordDialog.ChangePassword(staff);

		public event EventHandler NotificationDisplayed;

		public void FireNotificationDisplayed()
		{
			var handler = this.NotificationDisplayed;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		const int showTimeMilliseconds = 10000;
	}
}
