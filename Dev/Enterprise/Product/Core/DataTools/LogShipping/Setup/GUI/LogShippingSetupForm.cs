#define SuppressResourceStringsCheckRegion

using System;
using System.Threading;
using System.Windows.Forms;

namespace Enterprise.LogShipping.Setup.GUI
{
	public partial class LogShippingSetupForm : Form
	{
		public LogShippingSetupForm()
		{
			InitializeComponent();
			InitializeScreenAttributes();
		}

		void InitializeScreenAttributes()
		{
			screenTitleLabels = new TaskStatusControl[CountOfSteps];
			screenTitleLabels[(int)ScreenEnum.SecondaryServer] = TitleLabel_SecondaryServer;
			screenTitleLabels[(int)ScreenEnum.SecondaryDatabase] = TitleLabel_SecondaryDatabase;
			screenTitleLabels[(int)ScreenEnum.PrimaryServer] = TitleLabel_PrimaryServer;
			screenTitleLabels[(int)ScreenEnum.PrimaryDatabase] = TitleLabel_PrimaryDatabase;
			screenTitleLabels[(int)ScreenEnum.PrimaryEDocsDatabases] = TitleLabel_EDocsDatabases;
			screenTitleLabels[(int)ScreenEnum.SourceDirectory] = TitleLabel_BkpSourceDir;
			screenTitleLabels[(int)ScreenEnum.LocalCopyDirectory] = TitleLabel_BkpLocalCopyDir;
			screenTitleLabels[(int)ScreenEnum.InitializeSecondaryDb] = TitleLabel_InitSecondaryDb;
			screenTitleLabels[(int)ScreenEnum.FinalSetupScreen] = TitleLabel_FinalSetUp;

			screens = new Control[CountOfSteps];
			screens[(int)ScreenEnum.SecondaryServer] = secondaryServerControl;
			screens[(int)ScreenEnum.SecondaryDatabase] = secondaryDatabaseControl;
			screens[(int)ScreenEnum.PrimaryServer] = primaryServerControl;
			screens[(int)ScreenEnum.PrimaryDatabase] = primaryDatabaseControl;
			screens[(int)ScreenEnum.PrimaryEDocsDatabases] = primaryEDocsDatabaseControl;
			screens[(int)ScreenEnum.SourceDirectory] = sourceDirectoryControl;
			screens[(int)ScreenEnum.LocalCopyDirectory] = localCopyDirectoryControl;
			screens[(int)ScreenEnum.InitializeSecondaryDb] = initializeSecondaryDbControl;
			screens[(int)ScreenEnum.FinalSetupScreen] = finalSetupScreenControl;

			Navigator = new ScreenNavigator();
			Navigator.OnShowMessage += new NotificationDelegate(navigator_OnShowMessage);
			Navigator.OnSuccess += new ActionResultDelegate(navigator_OnSuccess);
			Navigator.OnFailure += new ActionResultDelegate(navigator_OnFailure);

			SetCurrentScreenContext();
		}

		#region Internal

		internal void Setup()
		{
			NextStep = false;
			SetWaitingFormStatus();
			Thread thread = new Thread(Navigator.Setup);
			thread.Start();
		}

		internal void SetCurrentScreenStatus(TaskStatus status)
		{
			var currentScreen = (int)Navigator.CurrentScreen;

			if (currentScreen < screenTitleLabels.Length)
			{
				var currentScreenControl = screenTitleLabels[currentScreen];

				if (currentScreenControl != null)
				{
					currentScreenControl.Status = status;
				}
			}
		}

		#endregion

		#region Events

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Cannot use Globals.Message because external tool")]
		void LogShippingSetupForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!completed)
			{
				string massage = waiting ? "Setup tool is in the middle of process, you may break something if close application now. Are you sure you want to exit?"
					: "Are you sure you want to exit without finishing setup?";
				DialogResult userResponse = MessageBox.Show(massage, "Exit Setup", MessageBoxButtons.YesNo);

				if (userResponse == DialogResult.No)
				{
					e.Cancel = true;
				}
			}
		}

		void BackButton_Click(object sender, EventArgs e)
		{
			NextStep = false;
			SetPreviousScreenInvisibleWhenMoveBack();
			Navigator.MoveBack();
			SetCurrentScreenContext();
		}

		void NextButton_Click(object sender, EventArgs e)
		{
			var currentScreen = (int)Navigator.CurrentScreen;

			if (currentScreen < screens.Length)
			{
				var currentScreenControl = screens[currentScreen] as UserAreaControl;

				if (currentScreenControl != null)
				{
					NextStep = true;
					SetWaitingFormStatus();
					Thread thread = new Thread(Navigator.MoveNext);
					thread.Start(currentScreenControl.Value);
				}
			}
		}

		void ExitButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		void navigator_OnSuccess()
		{
			IAsyncResult result = this.BeginInvoke(new ActionResultDelegate(SetScreenSuccess));

			if (result != null)
			{
				result.AsyncWaitHandle.WaitOne();
			}
		}

		void navigator_OnFailure()
		{
			IAsyncResult result = this.BeginInvoke(new ActionResultDelegate(SetScreenFailure));

			if (result != null)
			{
				result.AsyncWaitHandle.WaitOne();
			}
		}

		void navigator_OnShowMessage(string message)
		{
			IAsyncResult result = this.BeginInvoke(new NotificationDelegate(ShowCurrentScreenMessage), message);

			if (result != null)
			{
				result.AsyncWaitHandle.WaitOne();
			}
		}

		#endregion

		#region Implementation

		void SetupCompletedSuccessfully()
		{
			completed = true;
			ExitButton.ImageIndex = 1;
			finalSetupScreenControl.DisableSetup();
			SetCurrentScreenStatus(TaskStatus.Success);
			BackButton.Enabled = false;
		}

		void ShowCurrentScreenMessage(string message)
		{
			var currentScreen = (int)Navigator.CurrentScreen;

			if (currentScreen < screens.Length)
			{
				var userAreaControl = screens[currentScreen] as UserAreaControl;

				if (userAreaControl != null)
				{
					userAreaControl.ShowMessage(message);
				}
			}
		}

		void SetScreenSuccess()
		{
			if (Navigator.CurrentScreen == ScreenEnum.FinalSetupScreen && !NextStep)
			{
				SetActiveFormStatus(Navigator.CurrentScreen);
				SetupCompletedSuccessfully();
			}
			else
			{
				SetActiveFormStatus(Navigator.PreviousScreen);
				SetPreviousScreenInvisibleWhenMoveNext();
				SetCurrentScreenContext();
			}
		}

		void SetScreenFailure()
		{
			SetActiveFormStatus(Navigator.CurrentScreen);
			SetCurrentScreenStatus(TaskStatus.Failure);
		}

		void SetActiveFormStatus(ScreenEnum screen)
		{
			var screenAsInt = (int)screen;

			if (screenAsInt < screens.Length)
			{
				var userAreaControl = screens[screenAsInt] as UserAreaControl;

				if (userAreaControl != null)
				{
					userAreaControl.EnableControls();
					userAreaControl.Cursor = Cursors.Default;
					this.BackButton.Enabled = screen != 0;
					this.NextButton.Enabled = screen != (ScreenEnum)(CountOfSteps - 1);
					this.ExitButton.Enabled = true;
					this.Cursor = Cursors.Default;
					waiting = false;
				}
			}
		}

		void SetWaitingFormStatus()
		{
			int currentScreen = (int)Navigator.CurrentScreen;

			if (currentScreen < screens.Length)
			{
				waiting = true;

				var userAreaControl = screens[currentScreen] as UserAreaControl;

				if (userAreaControl != null)
				{
					userAreaControl.DisableControls();
					userAreaControl.Cursor = Cursors.WaitCursor;
					this.NextButton.Enabled = false;
					this.BackButton.Enabled = false;
					this.ExitButton.Enabled = false;
					this.Cursor = Cursors.WaitCursor;
				}
			}
		}

		void SetPreviousScreenInvisibleWhenMoveNext()
		{
			var previousScreen = (int)Navigator.PreviousScreen;
			var currentScreen = (int)Navigator.CurrentScreen;

			if (previousScreen < screens.Length && screens[previousScreen] != null)
			{
				screens[previousScreen].Visible = false;
			}

			if (previousScreen < screenTitleLabels.Length && screenTitleLabels[previousScreen] != null)
			{
				screenTitleLabels[previousScreen].Status = TaskStatus.Success;
			}

			if (currentScreen <= screenTitleLabels.Length)
			{
				for (int i = previousScreen + 1; i < currentScreen; i++)
				{
					if (screenTitleLabels[i] != null)
					{
						screenTitleLabels[i].Status = TaskStatus.Skipped;
					}
				}
			}
		}

		void SetPreviousScreenInvisibleWhenMoveBack()
		{
			var previousScreen = (int)Navigator.PreviousScreen;
			var currentScreen = (int)Navigator.CurrentScreen;

			if (currentScreen < screens.Length)
			{
				var currentScreenControl = screens[currentScreen];

				if (currentScreenControl != null)
				{
					currentScreenControl.Visible = false;
					((UserAreaControl)currentScreenControl).ClearOutput();
				}
			}

			if (currentScreen < screenTitleLabels.Length)
			{
				for (int i = previousScreen + 1; i <= currentScreen; i++)
				{
					if (screenTitleLabels[i] != null)
					{
						screenTitleLabels[i].Status = TaskStatus.Initial;
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void SetCurrentScreenContext()
		{
			var currentScreen = (int)Navigator.CurrentScreen;

			if (currentScreen < screens.Length)
			{
				var currentScreenControl = screens[currentScreen];

				if (currentScreenControl != null)
				{
					stepsGroupBox.Text = string.Format("Step {0} of {1}", currentScreen + 1, CountOfSteps);
					SetCurrentScreenStatus(TaskStatus.Setup);
					currentScreenControl.Visible = true;
					BackButton.Enabled = Navigator.CurrentScreen != 0;
					NextButton.Enabled = Navigator.CurrentScreen != (ScreenEnum)(CountOfSteps - 1);
				}
			}
		}

		#endregion

		bool completed;
		bool waiting;
		internal ScreenNavigator Navigator { get; private set; }
		internal bool NextStep { get; private set; }
		TaskStatusControl[] screenTitleLabels;
		Control[] screens;
		const int CountOfSteps = 9;
	}
}
