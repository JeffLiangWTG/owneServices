#if NETFRAMEWORK // Fix after WI00756415 (refactor BackgroundAppDomainWorker to not use AppDomains) is completed
using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class BackgroundAppDomainWorkerNotifierTest : TestCase
	{
#if !WINZOR
		[ExpectNoExceptions]
		public void TestDispose_WorkItemEventsDontBlowUp()
		{
			TestBackgroundAppDomainWorkerNotifier notifier = new TestBackgroundAppDomainWorkerNotifier();
			IAsyncResult workItem = BackgroundAppDomainWorker.QueueWorkItem("Work item", delegate
			{ });
			notifier.Dispose();
			Application.DoEvents();
			WaitForTaskToComplete(workItem);
		}

		public void TestNotifyIconDisposed()
		{
			bool disposed = false;

			TestBackgroundAppDomainWorkerNotifier notifier = new TestBackgroundAppDomainWorkerNotifier();
			notifier.NotifyIcon.Disposed += delegate
			{ disposed = true; };

			notifier.Dispose();
			Assert(disposed);
		}

		public void TestBalloonGUIShownOnQueueWorkItem()
		{
			AssertEquals(false, Notifier.Visible);
			IAsyncResult async1 = BackgroundAppDomainWorker.QueueWorkItem("Work item #1 - BitVectors rule(d)", delegate
			{ Thread.Sleep(1000); });
			IAsyncResult async2 = BackgroundAppDomainWorker.QueueWorkItem("Work item #2", delegate
			{ Thread.Sleep(1000); });

			WaitForNotifierToBecomeVisible(Notifier);
			AssertEquals(true, Notifier.Visible);
			AssertEquals("The following tasks are running or scheduled to run:\r\n\r\nWork item #1 - BitVectors rule(d)\r\nWork item #2\r\n", Notifier.LastToolTipTextShown);

			WaitForTaskToComplete(async1);
			WaitForTaskToComplete(async2);
		}

		public void TestBalloonGUIShownOnWorkItemCompletion()
		{
			AssertEquals(false, Notifier.Visible);
			IAsyncResult async = BackgroundAppDomainWorker.QueueWorkItem("Work item", delegate
			{ Thread.Sleep(1000); });
			IBackgroundAppDomainWorkItem workItem = BackgroundAppDomainWorker.WorkItemsInProgress[0];

			WaitForNotifierToBecomeVisible(Notifier);
			WaitForTaskToComplete(async);

			string expectedText = "Work item, submitted at " + workItem.SubmittedTime.ToLongTimeString() + ", has been completed.";
			AssertEquals(expectedText, Notifier.LastToolTipTextShown);
		}

		public void TestDetailsWindowShownOnClick()
		{
			IAsyncResult async1 = BackgroundAppDomainWorker.QueueWorkItem("Work item", delegate
			{ Thread.Sleep(3000); });
			IBackgroundAppDomainWorkItem workItem1 = BackgroundAppDomainWorker.WorkItemsInProgress[0];

			WaitForNotifierToBecomeVisible(Notifier);
			Notifier.OnClick();

			string expectedText = $"{Enterprise.Core.Constants.ProductName} Background Tasks:\r\n\r\n" + workItem1.SubmittedTime.ToLongTimeString() + "\t\t" + workItem1.Status.ToString() + "\t\tWork item\r\n";
			AssertEquals(expectedText, UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			WaitForTaskToComplete(async1);
			Notifier.OnClick();
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(false, Notifier.Visible);
		}

		#region Test Classes

		class TestBackgroundAppDomainWorkerNotifier : BackgroundAppDomainWorkerNotifier
		{
			protected override void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon)
			{
				base.ShowBalloonTip(timeout, tipTitle, tipText, tipIcon);
				LastToolTipTextShown = tipText;
			}

			public string LastToolTipTextShown;

			public void OnClick()
			{
				NotifyIcon.GetType().GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NotifyIcon, new object[] { EventArgs.Empty });
			}

			public new NotifyIcon NotifyIcon
			{
				get { return base.NotifyIcon; }
			}
		}

		#endregion

		#region Implementation

		TestBackgroundAppDomainWorkerNotifier Notifier;

		void WaitForTaskToComplete(IAsyncResult async)
		{
			while (!async.IsCompleted)
			{
				Thread.Sleep(0);
			}
			Application.DoEvents();
		}

		void WaitForNotifierToBecomeVisible(BackgroundAppDomainWorkerNotifier notifier)
		{
			for (int i = 0; i < 1000; i++)
			{
				Application.DoEvents();
				if (!notifier.Visible)
				{
					Thread.Sleep(10);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Notifier = new TestBackgroundAppDomainWorkerNotifier();
		}

		protected override void TearDown()
		{
			Notifier.Dispose();
			base.TearDown();
		}

		#endregion
#endif 
	}
}
#endif
