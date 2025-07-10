namespace Enterprise.AuditDataServices.UatRunner
{
	using System;
	using System.Diagnostics;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using CargoWise.Data;
	using Enterprise.AuditDataServices.Notification;
	using Enterprise.Integration;

	public partial class AuditSubscriptionForm : Form // CDC reserved column (SQL Server).
	{
		public AuditSubscriptionForm()
		{
			InitializeComponent();
		}

		bool keepRunning;
		Task notificationTask;

		void RunNotification()
		{
			using (Db.DisposableActionForDbConnection())
			{
				ILogger logger = new TextboxLogger(LogTextBox);

				try
				{
					SetButtonEnableStates(isRunning: true);
					logger.Information("--START"); // Development only form.

					RunNotificationCore();
				}
				catch (Exception ex)
				{
					logger.Error("\r\n***" + ex.Message);
				}
				finally
				{
					logger.Information("--");
					SetButtonEnableStates(isRunning: false);
				}
			}
		}

		void RunNotificationCore()
		{
			var aspTask = new AuditSubscriberProcessorTask();
			do
			{
				aspTask.RunTask(CancellationToken.None);

				var sw = Stopwatch.StartNew();

				while (keepRunning && sw.Elapsed < TimeSpan.FromMinutes(1))
				{
					Thread.Sleep(TimeSpan.FromSeconds(1));
				}

				sw.Stop();
			} while (keepRunning);
		}

		void SetButtonEnableStates(bool isRunning)
		{
			this.BeginInvoke(new MethodInvoker(
				() =>
					{
						RunNotificationOnceButton.Enabled = !isRunning;
						StartAuditNotificationButton.Enabled = !isRunning;
						StopAuditNotificationButton.Enabled = isRunning;
					}
				)
			);
		}

		void RunNotificationOnceButton_Click(object sender, EventArgs e)
		{
			if (notificationTask == null || notificationTask.IsCompleted || notificationTask.Status == TaskStatus.Created)
			{
				notificationTask = new Task(RunNotification);
				keepRunning = false;

				notificationTask.RunSynchronously();
			}
		}

		void StartAuditNotificationButton_Click(object sender, EventArgs e)
		{
			if (notificationTask == null || notificationTask.IsCompleted || notificationTask.Status == TaskStatus.Created)
			{
				notificationTask = new Task(RunNotification);
				keepRunning = true;

				notificationTask.Start();
			}
		}

		void StopAuditNotificationButton_Click(object sender, EventArgs e)
		{
			keepRunning = false;
		}
	}
}
