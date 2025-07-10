using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Common.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture
{
	public class BackgroundAppDomainWorkerNotifier : IDisposable
	{
		public BackgroundAppDomainWorkerNotifier()
		{
			BackgroundAppDomainWorker.WorkItemsInProgressChanged += new EventHandler(BackgroundAppDomainWorker_WorkItemsInProgressChanged);

			NotifyIcon.Icon = BrandingFactory.Instance.ProductIcon;
			NotifyIcon.Click += new EventHandler(NotifyIcon_Click);
			NotifyIcon.BalloonTipClicked += new EventHandler(NotifyIcon_Click);
			NotifyIcon.BalloonTipClosed += new EventHandler(NotifyIcon_BalloonTipClosed);

			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		#region Visible

		public bool Visible
		{
			get
			{
				CheckNotDisposed();
				return NotifyIcon.Visible;
			}
		}

		#endregion

		#region Click

		void NotifyIcon_Click(object sender, EventArgs e)
		{
			CheckNotDisposed();
			if (BackgroundAppDomainWorker.WorkItemsInProgress.Length == 0)
			{
				NotifyIcon.Visible = false;
			}
			else
			{
				StringBuilder text = new StringBuilder("");
				text.Append(Res.GetString("ef088f62-a651-41af-8761-598ceb3ff7de", "{0} Background Tasks:", ProductName) + "\r\n\r\n");

				foreach (IBackgroundAppDomainWorkItem workItem in BackgroundAppDomainWorker.WorkItemsInProgress)
				{
					text.Append(workItem.SubmittedTime.ToLongTimeString() + "\t\t");
					text.Append(workItem.Status.ToString() + "\t\t");
					text.Append(workItem.Description + "\r\n");
				}

				Globals.Message.Show(text.ToString(), Res.GetString("bf030324-5111-4d19-a340-ae7507070002", "{0} Background Tasks", ProductName), ZMessageBoxButtons.OK, ZMessageBoxIcon.Information);
			}
		}

		void NotifyIcon_BalloonTipClosed(object sender, EventArgs e)
		{
			if (BackgroundAppDomainWorker.WorkItemsInProgress.Length == 0)
			{
				NotifyIcon.Visible = false;
			}
		}

		#endregion

		#region Balloon

		protected virtual void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon)
		{
			CheckNotDisposed();
			NotifyIcon.ShowBalloonTip(timeout, tipTitle, tipText, tipIcon);
		}

		void ShowBalloon()
		{
			CheckNotDisposed();
			NotifyIcon.Visible = true;

			IBackgroundAppDomainWorkItem completedItem = GetRecentlyCompletedWorkItem();
			if (completedItem != null)
			{
				ShowBalloonTip(10000,
					Res.GetString("879683bb-ed94-4ec7-b25c-5cfa8b03f994", "{0} Background Task Completion", ProductName),
					Res.GetString("f637127d-2dda-4ef3-8e71-0d54d9eb1f4c", "{0}, submitted at {1}, has been completed.", completedItem.Description, completedItem.SubmittedTime.ToLongTimeString()),
					ToolTipIcon.Info);
			}
			else if (BackgroundAppDomainWorker.WorkItemsInProgress.Length > 0)
			{
				StringBuilder text = new StringBuilder(Res.GetString("e77a9ee2-3f0b-4593-b202-909c86964347", "The following tasks are running or scheduled to run:") + "\r\n\r\n");
				foreach (IBackgroundAppDomainWorkItem workItem in BackgroundAppDomainWorker.WorkItemsInProgress)
				{
					text.Append(workItem.Description + "\r\n");
				}
				ShowBalloonTip(10000, Res.GetString("bf030324-5111-4d19-a340-ae7507070002", "{0} Background Tasks", ProductName), text.ToString(), ToolTipIcon.Info);
			}

			LastSetOfTasks = BackgroundAppDomainWorker.WorkItemsInProgress;
		}

		IBackgroundAppDomainWorkItem GetRecentlyCompletedWorkItem()
		{
			if (LastSetOfTasks != null)
			{
				foreach (IBackgroundAppDomainWorkItem workItem in LastSetOfTasks)
				{
					if (!((IList<IBackgroundAppDomainWorkItem>)BackgroundAppDomainWorker.WorkItemsInProgress).Contains(workItem))
					{
						return workItem;
					}
				}
			}
			return null;
		}

		IBackgroundAppDomainWorkItem[] LastSetOfTasks;

		#endregion

		#region Exit

		public static bool NotifyWorkItemsInProgress()
		{
			var workItemsInProgress = BackgroundAppDomainWorker.WorkItemsInProgress;
			var result = workItemsInProgress.Length > 0;
			if (result)
			{
				Globals.Message.ShowInformation(GetBackgroundWorkItemsInProgressMessage(workItemsInProgress));
			}
			return result;
		}

		static string GetBackgroundWorkItemsInProgressMessage(IEnumerable<IBackgroundAppDomainWorkItem> workItemsInProgress)
		{
			var message = new StringWriter();
			message.WriteLine("");
			message.WriteLine(Res.GetString("2281a424-bc28-4efc-a987-080c489a5145", "You cannot exit {0} as the following background jobs are still running:", ProductName));
			message.WriteLine("");
			foreach (var workItem in workItemsInProgress)
			{
				message.WriteLine(workItem.Description);
			}
			message.WriteLine("");
			message.WriteLine(Res.GetString("a17dbc00-eebc-4be8-8574-534176570155", "Please try again later."));

			return message.GetStringBuilder().ToString();
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			IsDisposed = true;
			NotifyIcon.Dispose();
			BackgroundAppDomainWorker.WorkItemsInProgressChanged -= new EventHandler(BackgroundAppDomainWorker_WorkItemsInProgressChanged);
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}

		void CheckNotDisposed()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		bool IsDisposed;

		#endregion

		#region Implementation

		static string ProductName => !string.IsNullOrEmpty(BrandingFactory.Instance.ProductName) ? BrandingFactory.Instance.ProductName : Res.GetString("BackgroundAppDomainWorkerNotifier.Application", "the Application");

		readonly SynchronizationContext uiThread = SynchronizationContext.Current;

		protected NotifyIcon NotifyIcon
		{
			get { return notifyIcon; }
		}

		readonly NotifyIcon notifyIcon = new NotifyIcon();

		void BackgroundAppDomainWorker_WorkItemsInProgressChanged(object sender, EventArgs e)
		{
			uiThread.Post(delegate
			{
				if (!IsDisposed)
				{
					ShowBalloon();
				}
			}, null);
		}

		#endregion
	}
}
