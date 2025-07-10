using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;
using static Enterprise.ZArchitecture.GUI.UserIdleWorker;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class UserIdleWorkerTest : TestCaseWithDummy
	{
		public void TestWorkItemIsDisposedAfterDequeueAndRun()
		{
			var control = new Control();
			var worker = new TestUserIdleWorker(100);
			worker.ThrowOnTimerStart = false;
			var wr = RunTestAndGetWeakReference(worker, control);
			GC.Collect();
			Assert(!wr.IsAlive);
		}

		WeakReference RunTestAndGetWeakReference(TestUserIdleWorker worker, Control parentControl)
		{
			var workItemRan = false;
			var workItem = (WorkItem)worker.QueueWorkItemCore(parentControl, "", UserIdleWorkItemOptions.None, new MethodInvoker(delegate { workItemRan = true; }), null);
			AssertEquals("Should not run yet", false, workItemRan);

			Thread.Sleep(200);
			Application.DoEvents();
			AssertEquals("Should run on idle", true, workItemRan);
			return new WeakReference(workItem);
		}

		public void TestIsActive()
		{
			AssertEquals(false, UserIdleWorker.IsActive);
			UserIdleWorker.QueueWorkItem(null, new MethodInvoker(DoWork));
			AssertEquals(true, UserIdleWorker.IsActive);
			UserIdleWorker.Flush();
			AssertEquals(false, UserIdleWorker.IsActive);
		}

		public void TestQueuedWorkItemCount()
		{
			AssertEquals(0, UserIdleWorker.QueuedWorkItemCount);
			UserIdleWorker.QueueWorkItem(null, new MethodInvoker(DoWork));
			AssertEquals(1, UserIdleWorker.QueuedWorkItemCount);
			UserIdleWorker.Flush();
			AssertEquals(0, UserIdleWorker.QueuedWorkItemCount);
		}

		public void TestCleanupAfterWorkItemsCompleted()
		{
			UserIdleWorker.QueueWorkItem(null, new MethodInvoker(DoWork));
			UserIdleWorker.Flush();
			AssertEquals("UserIdleWorker.IsActive", false, UserIdleWorker.IsActive);
			AssertEquals("UserIdleDetecter.IsActive", false, UserIdleDetecter.IsActive);
		}

		public void TestWorkItemRunOnApplicationIdle()
		{
			var worker = new TestUserIdleWorker(10);
			worker.SetRunOnApplicationIdle(true);

			worker.QueueWorkItemCore(null, "", UserIdleWorkItemOptions.None, new MethodInvoker(DoWork), null);
			Thread.Sleep(100);
			Application.DoEvents();
			AssertEquals("Work item not run until Application.Idle fired", 0, DoWorkCount);

			FireApplicationIdle();
			AssertEquals("Work item run after Application.Idle fired", 1, DoWorkCount);
		}

		public void TestTimerShouldBeDisabledDuringDbUpgrade()
		{
			var worker = new TestUserIdleWorker(10);
			worker.SetRunOnApplicationIdle(true);

			worker.QueueWorkItemCore(null, "", UserIdleWorkItemOptions.None, new MethodInvoker(DoWork), null);
			Thread.Sleep(100);
			using (DbEnv.Instance.DisableTimerDuringDbUpgrade())
			{
				Application.DoEvents();
				AssertEquals("Work item not run until Application.Idle fired", 0, DoWorkCount);

				FireApplicationIdle();
				AssertEquals("Timer should not tick", 0, DoWorkCount);
			}
		}

		void FireApplicationIdle()
		{
			var form = new Form();

			var timer = new System.Windows.Forms.Timer();
			timer.Tick += delegate { form.Dispose(); Application.DoEvents(); };
			timer.Interval = 200;
			timer.Start();

			form.ShowDialog();
			timer.Dispose();
		}

		#region QueueWorkItem

		public void TestQueueWorkItem()
		{
			Try10Times(delegate
			{
				UserIdleWorker.QueueWorkItem(null, new MethodInvoker(DoWork));
				UserIdleWorker.QueueWorkItem(null, new MethodInvoker(DoWork));
				Application.DoEvents();
				AssertEquals("No work items executed until 1 second of idle", 0, DoWorkCount);

				Thread.Sleep(500);
				Application.DoEvents();
				AssertEquals("No work items executed until 1 second of idle", 0, DoWorkCount);

				Thread.Sleep(1000);
				Application.DoEvents();
				AssertEquals("Both work items executed", 2, DoWorkCount);
			});
		}

		public void TestQueueWorkItem_With2WorkItemsWithDifferentIntervals()
		{
			Try10Times(delegate
			{
				var workItem1Run = false;
				var workItem2Run = false;
				UserIdleWorker.QueueWorkItem(null, "", 100, new MethodInvoker(delegate { workItem1Run = true; }));
				UserIdleWorker.QueueWorkItem(null, "", 300, new MethodInvoker(delegate { workItem2Run = true; }));

				Thread.Sleep(200);
				Application.DoEvents();
				AssertEquals("WorkItem1 run after 100ms", true, workItem1Run);
				AssertEquals("WorkItem1 not run after 100ms", false, workItem2Run);

				Thread.Sleep(200);
				Application.DoEvents();
				AssertEquals("WorkItem2 run after 300ms", true, workItem2Run);
			});
		}

		public void TestQueueWorkItem_ForShorterInterval()
		{
			Try10Times(delegate
			{
				UserIdleWorker.QueueWorkItem(null, "", 300, new MethodInvoker(DoWork));
				UserIdleWorker.QueueWorkItem(null, "", 300, new MethodInvoker(DoWork));
				Application.DoEvents();
				AssertEquals("No work items executed until 300ms of idle", 0, DoWorkCount);

				Thread.Sleep(200);
				Application.DoEvents();
				AssertEquals("No work items executed until 300ms of idle", 0, DoWorkCount);

				Thread.Sleep(200);
				Application.DoEvents();
				AssertEquals("Both work items executed", 2, DoWorkCount);
			});
		}

		public void TestQueueWorkItem_CheckForUserActivityBetweenWorkItems()
		{
			Try10Times(delegate
			{
				UserIdleWorker.QueueWorkItem(null, "", 100, new MethodInvoker(DoWork), Array.Empty<object>());
				UserIdleWorker.QueueWorkItem(null, "", 100, new MethodInvoker(DoWork), Array.Empty<object>());
				Application.DoEvents();
				AssertEquals("No work items executed until 1 second of idle", 0, DoWorkCount);

				PressKeyDuringDoWork = true;
				Thread.Sleep(200);
				Application.DoEvents();
				AssertEquals("1 work item executed. User activity occurred during work item, wait for idle again before running the 2nd", 1, DoWorkCount);

				Thread.Sleep(200);
				Application.DoEvents();
				AssertEquals("Second work item executed", 2, DoWorkCount);
			});
		}

		public void TestQueueWorkItem_DontRunIfRegistryItemNotEnabled()
		{
			SystemDataRegistryForTest.Get().UserIdleWorkerEnabled = false;
			UserIdleWorker.QueueWorkItem(null, new MethodInvoker(DoWork));
			AssertEquals("Work item run immediately when UserIdleWorker disabled", 1, DoWorkCount);
		}

		public void TestQueueWorkItem_DontRunIfOwnerDisposed()
		{
			var control = new ZTextBox();
			control.Dispose();

			UserIdleWorker.QueueWorkItem(control, new MethodInvoker(DoWork));
			AssertEquals("Work item not queued when owner control disposed", false, UserIdleWorker.IsActive);
			AssertEquals("Work item not queued when owner control disposed", false, UserIdleDetecter.IsActive);
		}

		public void TestQueueWorkItem_DontRunAfterOwnerDisposed()
		{
			using (var control = new ZTextBox())
			{
				UserIdleWorker.QueueWorkItem(control, "", 100, new MethodInvoker(DoWork));
			}

			Thread.Sleep(200);
			Application.DoEvents();
			AssertEquals("Work item not run after owner control disposed", 0, DoWorkCount);
		}

		public void TestQueueWorkItem_DontRunDuringEditingOfAControl()
		{
			Try10Times(delegate
			{
				using (var form = new ZTestForm(Dummy))
				{
					form.Show();
					Application.DoEvents();

					UserIdleWorker.ActiveForm = form;
					form.TextBox.Focus();
					form.TextBox.Text = "Changed";
					UserIdleWorker.QueueWorkItem(form.CalcEdit, "", 100, new MethodInvoker(DoWork));

					Thread.Sleep(200);
					Application.DoEvents();
					AssertEquals("Work item not run during edit of control", 0, DoWorkCount);

					form.CalcEdit.Focus();
					UserIdleDetecter.FireUserActivity();
					Thread.Sleep(200);
					Application.DoEvents();
					Thread.Sleep(200);
					Application.DoEvents();
					AssertEquals("Work item run after no control is in edit", 1, DoWorkCount);
				}
			});
		}

		public void TestQueueWorkItem_AllowDuringEditOfAControlIfPermitted()
		{
			Try10Times(delegate
			{
				using (var form = new ZTestForm(Dummy))
				{
					form.Show();
					Application.DoEvents();

					UserIdleWorker.ActiveForm = form;
					form.TextBox.Focus();
					form.TextBox.Text = "Changed";
					UserIdleWorker.QueueWorkItem(form.CalcEdit, "", 100, UserIdleWorkItemOptions.AllowDuringEdit, new MethodInvoker(DoWork));

					Thread.Sleep(200);
					Application.DoEvents();
					AssertEquals("Work item run during edit of control", 1, DoWorkCount);
				}
			});
		}

		public void TestQueueWorkItem_AllowWhenFormInactiveIfPermitted()
		{
			Try10Times(delegate
			{
				using (var form = new ZTestForm(Dummy))
				{
					form.Show();
					Application.DoEvents();

					UserIdleWorker.ActiveForm = null;
					UserIdleWorker.QueueWorkItem(form.CalcEdit, "", 100, UserIdleWorkItemOptions.AllowWhenFormInactive, new MethodInvoker(DoWork));

					Thread.Sleep(200);
					Application.DoEvents();
					AssertEquals("Work item run when form not active", 1, DoWorkCount);
				}
			});
		}

		public void TestQueueWorkItem_DontRunWhenFormInactiveNotPermitted()
		{
			Try10Times(delegate
			{
				using (var form = new ZTestForm(Dummy))
				{
					form.Show();
					Application.DoEvents();

					UserIdleWorker.ActiveForm = null;
					UserIdleWorker.QueueWorkItem(form.CalcEdit, "", 100, new MethodInvoker(DoWork));

					Thread.Sleep(200);
					Application.DoEvents();
					AssertEquals("Work item not run when form not active", 0, DoWorkCount);
				}
			});
		}

		public void TestQueueWorkItem_PreserveCurrentSelectionDuringBrowseOfAListControl()
		{
			Try10Times(delegate
			{
				using (var form = new ZTestForm(Dummy))
				{
					form.Show();
					Application.DoEvents();
					form.Grid.Focus();
					((BusinessObjectCollection)form.Grid.List).AddNew();
					((BusinessObjectCollection)form.Grid.List).AddNew();
					form.Grid.ListManager.Position = 1;
					UserIdleWorker.QueueWorkItem(form.CalcEdit, "", 100, new MethodInvoker(delegate { form.Grid.ListManager.Position = 0; }));

					Thread.Sleep(200);
					Application.DoEvents();
					AssertEquals("Current grid cell restored after work item completed", 1, form.Grid.ListManager.Position);
				}
			});
		}

		public void TestQueueWorkItem_DisposeCancelsWorkItem()
		{
			IDisposable workItem = UserIdleWorker.QueueWorkItem(null, "", 0, new MethodInvoker(DoWork));
			workItem.Dispose();
			AssertEquals("Work item should not run after it has been disposed", 0, DoWorkCount);
			UserIdleWorker.Flush();
		}

		public void TestQueueWorkItem_Win32ExceptionOccursOnTimer_WorkItemRunImmediately()
		{
			var worker = new TestUserIdleWorker(100);
			worker.ThrowOnTimerStart = true;

			var workItemRan = false;
			worker.QueueWorkItemCore(null, "", UserIdleWorkItemOptions.None, new MethodInvoker(delegate { workItemRan = true; }), null);
			AssertEquals("Work item should run immediately if the timer can't be started", true, workItemRan);
		}

		public void TestQueueWorkItem_QueueEmptied_WorkerDisposed()
		{
			var worker = new TestUserIdleWorker(100);

			var workItemRan = false;
			worker.QueueWorkItemCore(null, "", UserIdleWorkItemOptions.None, new MethodInvoker(delegate { workItemRan = true; }), null);

			Thread.Sleep(200);
			Application.DoEvents();

			AssertEquals("Work item should have run so the queue will be clear", true, workItemRan);
			Assert("TestUserIdleWorker should be disposed when the message queue is emptied", worker.Disposed);
		}

		public void TestQueueWorkItem_WorkerDisposed_RequeueThrowsObjectDisposedException()
		{
			var worker = new TestUserIdleWorker(100);
			var workItemRan = false;

			worker.QueueWorkItemCore(null, "", UserIdleWorkItemOptions.None, new MethodInvoker(delegate { workItemRan = true; }), null);

			Thread.Sleep(200);
			Application.DoEvents();

			AssertEquals("Work item should have run so the queue will be clear", true, workItemRan);
			AssertExceptionThrown<ObjectDisposedException>(() => worker.QueueWorkItemCore(null, "", UserIdleWorkItemOptions.None, new MethodInvoker(delegate { }), null));
		}

		#endregion

		#region Suspend

		public void TestSuspend()
		{
			Try10Times(delegate
			{
				UserIdleWorker.QueueWorkItem(null, "", 100, new MethodInvoker(DoWork));

				using (UserIdleWorker.Suspend())
				{
					Thread.Sleep(200);
					Application.DoEvents();
					AssertEquals("No work items executed while suspended", 0, DoWorkCount);
				}

				Thread.Sleep(200);
				Application.DoEvents();
				AssertEquals("Work item executed after resume", 1, DoWorkCount);
			});
		}

		#endregion

		#region Flush

		public void TestFlush()
		{
			UserIdleWorker.QueueWorkItem(null, new MethodInvoker(DoWork));
			UserIdleWorker.QueueWorkItem(null, new MethodInvoker(DoWork));
			UserIdleWorker.Flush();
			AssertEquals("All work items executed", 2, DoWorkCount);
		}

		#endregion

		#region ErrorLogger

		public void TestErrorLogger()
		{
			var logger = new ErrorLogger();
			AssertEquals("Precondition: Logger should initially contain no messages", string.Empty, logger.GetLoggedMessages());

			logger.Log("Hello");
			AssertEquals("Logger should return the string logged", "Hello", logger.GetLoggedMessages());

			logger.Log("Goodbye");
			var expected = string.Join(System.Environment.NewLine, new string[] { "Hello", "Goodbye" });
			AssertEquals("Logger should join multiple logged messages with a new line", expected, logger.GetLoggedMessages());

			logger.Clear();
			AssertEquals("Logger should return empty string after being cleared", string.Empty, logger.GetLoggedMessages());
		}

		#endregion

		#region Queued / Completed / Cancelled

		public void TestQueuedEvent()
		{
			var queuedFired = false;
			UserIdleWorker.Queued += delegate { queuedFired = true; };
			var workItem = UserIdleWorker.QueueWorkItem(null, new MethodInvoker(delegate { }));
			workItem.Dispose();
			AssertEquals("Queued fired", true, queuedFired);
		}

		public void TestCompleted()
		{
			var completedFired = false;
			UserIdleWorker.Completed += delegate { completedFired = true; };
			var workItem = UserIdleWorker.QueueWorkItem(null, "", 100, new MethodInvoker(delegate { }));

			AssertEquals("Completed not fired until completed", false, completedFired);
			Thread.Sleep(200);
			Application.DoEvents();
			FireApplicationIdle();
			AssertEquals("Completed fired", true, completedFired);
		}

		public void TestCancelled()
		{
			var cancelledFired = false;
			UserIdleWorker.Cancelled += delegate { cancelledFired = true; };
			var workItem = UserIdleWorker.QueueWorkItem(null, new MethodInvoker(delegate { }));

			AssertEquals("Cancelled not fired until cancelled", false, cancelledFired);
			workItem.Dispose();
			AssertEquals("Completed fired", true, cancelledFired);
		}

		#endregion

		#region Test Classes

		class TestUserIdleWorker : UserIdleWorker
		{
			public TestUserIdleWorker(int startDelay)
				: base(startDelay)
			{
			}

			public new IDisposable QueueWorkItemCore(Control workItemOwner, string description, UserIdleWorkItemOptions options, Delegate method, object[] args)
			{
				return base.QueueWorkItemCore(workItemOwner, description, options, method, args);
			}

			public void SetRunOnApplicationIdle(bool value)
			{
				runOnApplicationIdle = value;
			}

			protected override bool RunOnApplicationIdle
			{
				get { return runOnApplicationIdle ?? base.RunOnApplicationIdle; }
			}
			bool? runOnApplicationIdle;

			public bool ThrowOnTimerStart { get; set; }

			protected override IWindowsTimer CreateTimer()
			{
				return new TestTimer(this);
			}
		}

		class TestTimer : ProxyWindowsTimer, IWindowsTimer
		{
			public TestTimer(TestUserIdleWorker owner)
			{ this.owner = owner; }

			public bool ThrowOnTimerStart { get; set; }

			bool IWindowsTimer.Enabled
			{
				get { return timer.Enabled; }
				set
				{
					timer.Enabled = value;
					if (value && owner.ThrowOnTimerStart)
					{
						throw new Win32Exception();
					}
				}
			}

			readonly TestUserIdleWorker owner;
		}

		#endregion

		#region Implementation

		int DoWorkCount;
		bool PressKeyDuringDoWork;

		void DoWork()
		{
			if (PressKeyDuringDoWork)
			{
				UserIdleDetecter.FireUserActivity();
			}
			DoWorkCount++;
		}

		void Try10Times(MethodInvoker method)
		{
			var i = 1;
			while (true)
			{
				try
				{
					method();
					break;
				}
				catch (AssertionFailedError)
				{
					if (i == 10)
					{
						throw;
					}
					TearDown();
					SetUp();
					Thread.Sleep(1000);
				}
				i++;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			UserIdleWorker.AllowWhenFormInactiveForTest = false;
		}

		protected override void TearDown()
		{
			base.TearDown();
			UserIdleWorker.Flush();
			DoWorkCount = 0;
			PressKeyDuringDoWork = false;
			UserIdleWorker.ActiveForm = Form.ActiveForm;
			UserIdleWorker.AllowWhenFormInactiveForTest = true;
		}

		#endregion
	}
}
