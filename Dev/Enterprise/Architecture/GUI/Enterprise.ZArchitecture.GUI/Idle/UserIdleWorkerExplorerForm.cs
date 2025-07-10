using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.GUI
{
	internal partial class UserIdleWorkerExplorerForm : Form // This is a debugging tool only.
	{
		public UserIdleWorkerExplorerForm()
		{
			InitializeComponent();

			foreach (var workItem in UserIdleWorker.QueuedWorkItems)
			{
				OnQueued(new UserIdleWorkItem(workItem, null));
			}

			UserIdleWorker.Queued += new EventHandler<UserIdleWorkItemEventArgs>(UserIdleWorker_Queued);
			UserIdleWorker.Completed += new EventHandler<UserIdleWorkItemEventArgs>(UserIdleWorker_Completed);
			UserIdleWorker.Cancelled += new EventHandler<UserIdleWorkItemEventArgs>(UserIdleWorker_Cancelled);

			suspendedUpdateTimer = new Timer();
			suspendedUpdateTimer.Interval = 500;
			suspendedUpdateTimer.Tick += delegate { chkSuspend.Checked = UserIdleWorker.IsSuspended; };
			suspendedUpdateTimer.Start();
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			if (VisualStudioDetector.IsVisualStudio)
			{
				this.AutoScaleMode = AutoScaleMode.None;
			}
			else
			{
				this.AutoScaleMode = CargoWise.Windows.UI.ControlDpiScalingHelper.DpiScaleMode;
				this.AutoScaleDimensions = CargoWise.Windows.UI.ControlDpiScalingHelper.DpiScaleDimensions;
			}

			base.OnLayout(levent);
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				UserIdleWorker.Queued -= new EventHandler<UserIdleWorkItemEventArgs>(UserIdleWorker_Queued);
				UserIdleWorker.Completed -= new EventHandler<UserIdleWorkItemEventArgs>(UserIdleWorker_Completed);
				UserIdleWorker.Cancelled -= new EventHandler<UserIdleWorkItemEventArgs>(UserIdleWorker_Cancelled);

				suspendedUpdateTimer.Dispose();
				if (suspender != null)
				{
					suspender.Dispose();
				}
			}
		}

		#endregion

		#region OnQueued / OnCompleted / OnCancelled

		void OnQueued(UserIdleWorkItem workItem)
		{
			workItems.Add(workItem.WorkItem, workItem);
			AddToListBox(lbQueued, workItem);
		}

		void OnCompleted(UserIdleWorkItem workItem)
		{
			lbQueued.Items.Remove(workItem);
			AddToListBox(lbCompleted, workItem);
		}

		void OnCancelled(UserIdleWorkItem workItem)
		{
			lbQueued.Items.Remove(workItem);
		}

		#endregion

		#region Helper Classes

		class UserIdleWorkItem
		{
			public UserIdleWorkItem(IUserIdleWorkItem workItem, StackTrace stackTrace)
			{
				this.WorkItem = workItem;
				this.QueuedDate = DateTime.Now; // This is a debugging tool only.
				this.StackTrace = stackTrace;
			}

			public IUserIdleWorkItem WorkItem { get; private set; }
			public DateTime QueuedDate { get; private set; }
			public StackTrace StackTrace { get; private set; }

			public override string ToString()
			{
				if (asString == null)
				{
					asString = Res.GetString("f5fc132f-2710-4805-87cf-52a7526a1dc0", "<queued before this form was shown>");
					if (StackTrace != null)
					{
						foreach (var frame in StackTrace.ToString().Split('\n'))
						{
							asString = frame;
							if (!asString.Contains("UserIdleWorker"))
							{
								break;
							}
						}
						asString = QueuedDate.ToLongTimeString() + "\tStartDelay: " + WorkItem.StartDelay + "\t" + asString; // Programmatic constant
					}
				}
				return asString;
			}

			string asString;
		}

		#endregion

		#region Implementation

		readonly Dictionary<IUserIdleWorkItem, UserIdleWorkItem> workItems = new Dictionary<IUserIdleWorkItem, UserIdleWorkItem>();
		readonly Timer suspendedUpdateTimer;
		IDisposable suspender;

		void UserIdleWorker_Queued(object sender, UserIdleWorkItemEventArgs e)
		{
			OnQueued(new UserIdleWorkItem(e.WorkItem, new StackTrace()));
		}

		void UserIdleWorker_Completed(object sender, UserIdleWorkItemEventArgs e)
		{
			UserIdleWorkItem workItem = null;
			if (workItems.TryGetValue(e.WorkItem, out workItem))
			{
				OnCompleted(workItem);
			}
		}

		void UserIdleWorker_Cancelled(object sender, UserIdleWorkItemEventArgs e)
		{
			UserIdleWorkItem workItem = null;
			if (workItems.TryGetValue(e.WorkItem, out workItem))
			{
				OnCancelled(workItem);
			}
		}

		void btnClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		void WorkItem_DoubleClick(object sender, EventArgs e)
		{
			var item = ((ListBox)sender).SelectedItem as UserIdleWorkItem;
			if (item != null && item.StackTrace != null)
			{
				MessageBox.Show(item.StackTrace.ToString()); // This is a debugging tool only.
			}
		}

		void chkSuspend_CheckChanged(object sender, EventArgs e)
		{
			if (UserIdleWorker.IsSuspended != chkSuspend.Checked)
			{
				if (suspender != null)
				{
					suspender.Dispose();
					suspender = null;
				}

				if (chkSuspend.Checked)
				{
					suspender = UserIdleWorker.Suspend();
				}
				chkSuspend.Checked = UserIdleWorker.IsSuspended;
			}
		}

		void AddToListBox(ListBox listBox, UserIdleWorkItem workItem)
		{
			var scrollToBottom = listBox.Items.Count == 1 || listBox.SelectedIndex == -1 || listBox.SelectedIndex == listBox.Items.Count - 1;
			listBox.Items.Add(workItem);
			if (scrollToBottom)
			{
				listBox.SelectedIndex = listBox.Items.Count - 1;
			}
		}

		#endregion
	}
}
