using System;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls.Internal;
using CargoWise.Windows.UI.Design;
using Enterprise.ZArchitecture.Core;

#if WINZOR
using BArchitecture;
using Enterprise.ZArchitecture.GUI.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
#endif

namespace Enterprise.Environment
{
	[DesignerSerializer(typeof(ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class UpgradeInProgressForm : Form // We do not have access to the database
	{
		public UpgradeInProgressForm()
			: this(startTimers: true)
		{
		}

		internal UpgradeInProgressForm(bool startTimers)
		{
			using (Db.DisableSchemaVersionCheckOnCurrentThread())
			{
				InitializeComponent();
				this.Icon = BrandingFactory.Instance.ProductIcon;
				this.ControlBox = false;
				SetText();
				if (startTimers)
				{
					StartTimers();
				}
			}
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			this.AutoScaleMode = ControlDpiScalingHelper.DpiScaleMode;
			this.AutoScaleDimensions = ControlDpiScalingHelper.DpiScaleDimensions;
			base.OnLayout(levent);
		}

		void SetText()
		{
			switch (Db.GetLockoutReason())
			{
				case LockoutReason.Upgrade:
					this.Text = Env.Instance.DbUpgradeCaptions.UpgradeInProgressTitle;
					this.messageTextBox.Text = Env.Instance.DbUpgradeCaptions.UpgradeInProgressMessage;
					break;
				case LockoutReason.Purge:
					this.Text = Env.Instance.DbUpgradeCaptions.PurgeInProgressTitle;
					this.messageTextBox.Text = Env.Instance.DbUpgradeCaptions.PurgeInProgressMessage;
					break;
			}
			this.exitButton.Text = Env.Instance.DbUpgradeCaptions.Exit;
		}

		void StartTimers()
		{
			backgroundTimer = new System.Threading.Timer(BackgroundTimerCallback, null, 30000, 30000);
			foregroundTimer = new System.Windows.Forms.Timer();
			foregroundTimer.Interval = 1000;
			foregroundTimer.Enabled = true;
			foregroundTimer.Tick += ForegroundTimer_Tick;
		}

		void BackgroundTimerCallback(object state)
		{
			if (Interlocked.CompareExchange(ref isBackgroundCheckInProgress, 1, 0) != 0)
			{
				return;
			}

			try
			{
				using (Db.DisableSchemaVersionCheckOnCurrentThread())
				{
					DoBackgroundCheck();
				}
			}
			finally
			{
				isBackgroundCheckInProgress = 0;
			}
		}
		int isBackgroundCheckInProgress;

		protected void DoBackgroundCheck()
		{
			if (IsDisposed)
			{
				return;
			}
			using (Db.DisposableActionForDbConnection())
			{
				try
				{
					if (DbLockout.HasLockout(Db.Connection))
					{
						return;
					}
					if (upgradeChecker.HasBeenUpgraded())
					{
						upgradedException = new DatabaseUpgradedException();
					}
					CanClose = true;
				}
				catch (DatabaseUpgradedException ex)
				{
					upgradedException = ex;
					CanClose = true;
				}
				catch (DatabaseUpgradeException)
				{
				}
				catch (System.Data.Common.DbException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.LockTimeoutExpired)
				{
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					GCTracker.StopTracking();
					try
					{
						HandleUnhandledException(ex);
					}
					finally
					{
						GCTracker.StartTracking();
					}
				}

				if (CanClose)
				{
					backgroundTimer?.Dispose();
				}
			}
		}

#if WINZOR
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			InvokeRenderDispatcher(async () =>
				await CargoWiseClientServices.JSRuntime.InvokeAsync<object>("enableRestartOnDisconnect", Array.Empty<object>()));
		}
#endif

		protected virtual void HandleUnhandledException(Exception ex)
			=> ExceptionReporter.Instance.HandleUnhandledException(ex);

		protected void ForegroundTimer_Tick(object sender, EventArgs e)
		{
			if (CanClose)
			{
				if (upgradedException != null)
				{
					using (Db.DisableSchemaVersionCheckOnCurrentThread())
					using (Db.DisposableActionForDbConnection())
					{
						DbEnv.Instance.ConnectionGuiPlugin.HandleDatabaseUpgradeException(upgradedException);
					}
				}
				Close();
			}
		}

		void exitButton_Click(object sender, EventArgs e)
		{
			ExceptionReporter.SuppressGui();
			GCTracker.StopTracking();
#if WINZOR
			var logger = GetService<ILogger<Form>>();
			var circuitId = GetService<ICircuitIdProvider>()?.CircuitId;
			logger?.LogInformation($"Exit Button is clicked, {GetType().Name} on circuit {circuitId}");
			Application.Exit();
#else
			Process.GetCurrentProcess().Kill();
#endif
		}

		System.Threading.Timer backgroundTimer;
		protected System.Windows.Forms.Timer foregroundTimer;
		protected virtual IUpgradeCheckHelper upgradeChecker { get; } = new UpgradeCheckHelper();
		protected volatile DatabaseUpgradedException upgradedException;
		volatile bool canClose;

		protected virtual bool CanClose
		{
			get { return canClose; }
			set { canClose = value; }
		}
	}
}
