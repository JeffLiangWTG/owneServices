using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Controls.Internal.Testing
{
	sealed class ControlTrackerTest : TestCase
	{
		public void TestStopTrackingWithUndisposedControl()
		{
			var tracker = new ControlTracker();

			var control = new Control();
			control.CreateControl();
			tracker.TrackInstanceForLeak(control);

			AssertEquals(1, tracker.ControlCount);
			tracker.StopTrackingInstanceForLeak(control);
			AssertEquals("Calling Stop should not dispose the 'good' control", false, control.IsDisposed);
			AssertEquals(0, tracker.ControlCount);
		}

		public void TestLeakyReference()
		{
			var tracker = new ControlTracker();
			var trackedObject = CreateTrackedObject(tracker);

			GC.Collect();

			Assert("Tracked object is gone", !trackedObject.TryGetTarget(out var _));

			tracker.StopTrackingInstanceForLeak(null);
			AssertEquals("Control count is zero", 0, tracker.ControlCount);
		}

		WeakReference<Control> CreateTrackedObject(ControlTracker tracker)
		{
			var c = new Control();
			tracker.TrackInstanceForLeak(c);
			return new WeakReference<Control>(c);
		}

		public void TestStopTrackingWithMultipleUndisposedControls()
		{
			var tracker = new ControlTracker();

			// this is the OK one
			var control = new Control();
			control.CreateControl();
			control.Name = "OK";
			tracker.TrackInstanceForLeak(control);

			// this is the leaky one
			AddToTracker(tracker, false);
			AssertEquals(2, tracker.ControlCount);
			tracker.StopTrackingInstanceForLeak(control);

			AssertEquals(0, tracker.ControlCount);
			AssertContains(
@"Leak detected. Please check ""Previous Exceptions thrown"" section for the root cause.
Name = Collectable
TopParent is null
Parent NOT Disposed is null
Parent Changes: 
Path: Collectable (Control)
Stack: Not collected", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestIncludesAllLeakedControls()
		{
			var tracker = new ControlTracker();
			using (var leakyOne = new TrackedBox(tracker) { Name = "LeakyOne" })
			using (var leakyTwo = new TrackedBox(tracker) { Name = "LeakyTwo" })
			using (var noleak = new TrackedBox(tracker) { Name = "Third" })
			{
				ForceHandleCreation(leakyOne, leakyTwo, noleak);

				noleak.Dispose();

				CombineAssertions("The error report should contain all of the leaked controls", () =>
				{
					AssertContains("LeakyOne", ErrorReporter.LastMessageReported);
					AssertContains("LeakyTwo", ErrorReporter.LastMessageReported);
				});

				ErrorReporter.Clear();
			}
		}

		public void TestNoLeakDetectedWhenOutOfMemoryExceptionThrown()
		{
			ErrorReporter.Clear();

			var toolStrip = new KToolStripDisposedWithOutOfMemoryException { Name = "tool strip 1" };
			var toolStrip2 = new KToolStrip { Name = "tool strip 2" };
			ForceHandleCreation(toolStrip, toolStrip2);

			var tracker = new ControlTracker();
			tracker.TrackInstanceForLeak(toolStrip);
			tracker.TrackInstanceForLeak(toolStrip2);
			AssertExceptionThrown(typeof(OutOfMemoryException), toolStrip.Dispose);
			tracker.StopTrackingInstanceForLeak(toolStrip2);

			AssertEquals("There should be no further exception thrown", "You ran out of memory", ErrorReporter.LastMessageReported);
		}

		class KToolStripDisposedWithOutOfMemoryException : KToolStrip
		{
			int RunDisposeNumber;
			protected override void Dispose(bool disposing)
			{
				RunDisposeNumber++;
				if (RunDisposeNumber == 1)
				{
					var ex = new OutOfMemoryException("Test out of memory exception");
					ErrorReporter.ReportOnce("You ran out of memory", ex);
					throw ex;
				}
				else
				{
					base.Dispose(disposing);
				}
			}
		}

		public void TestNoLeakDetectedWhenShutdownSqlExceptionThrown()
		{
			ErrorReporter.Clear();
			var toolStrip = new KToolStrip { Name = "tool strip 1" };
			var toolStrip2 = new KToolStrip { Name = "tool strip 2" };
			ForceHandleCreation(toolStrip, toolStrip2);

			var tracker = new ControlTracker();
			tracker.TrackInstanceForLeak(toolStrip);
			tracker.TrackInstanceForLeak(toolStrip2);
			ErrorReporter.ReportOnce("Shutting down", SqlExceptionBuilder.CreateSqlException(6005, "SHUTDOWN is in progress.\nLogin failed for user 'xxx'. Only administrators may connect at this time."));
			tracker.StopTrackingInstanceForLeak(toolStrip2);

			AssertEquals("There should be no further exception thrown", "Shutting down", ErrorReporter.LastMessageReported);
		}

		public void TestLeakDetectedWhenNonShutdownSqlExceptionThrown()
		{
			ErrorReporter.Clear();
			var toolStrip = new KToolStrip { Name = "tool strip 1" };
			var toolStrip2 = new KToolStrip { Name = "tool strip 2" };
			ForceHandleCreation(toolStrip, toolStrip2);

			var tracker = new ControlTracker();
			tracker.TrackInstanceForLeak(toolStrip);
			tracker.TrackInstanceForLeak(toolStrip2);
			ErrorReporter.ReportOnce("Non-shutdown sql", SqlExceptionBuilder.CreateSqlException(7303, "Cannot initialize the data source object of OLE DB provider"));
			tracker.StopTrackingInstanceForLeak(toolStrip2);

			AssertContains("There should be a leak exception thrown", "Leak detected", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestLeakDetectedWhenNonSqlShutdownExceptionThrown()
		{
			ErrorReporter.Clear();
			var toolStrip = new KToolStrip { Name = "tool strip 1" };
			var toolStrip2 = new KToolStrip { Name = "tool strip 2" };
			ForceHandleCreation(toolStrip, toolStrip2);

			var tracker = new ControlTracker();
			tracker.TrackInstanceForLeak(toolStrip);
			tracker.TrackInstanceForLeak(toolStrip2);
			ErrorReporter.ReportOnce("Non-sql shutdown", new Exception("SHUTDOWN is in progress.\nLogin failed for user 'xxx'. Only administrators may connect at this time."));
			tracker.StopTrackingInstanceForLeak(toolStrip2);

			AssertContains("There should be a leak exception thrown", "Leak detected", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestNoLeakDetectedWhenShutdownSqlAndOtherExceptionsThrown()
		{
			ErrorReporter.Clear();
			var toolStrip = new KToolStrip { Name = "tool strip 1" };
			var toolStrip2 = new KToolStrip { Name = "tool strip 2" };
			ForceHandleCreation(toolStrip, toolStrip2);

			var tracker = new ControlTracker();
			tracker.TrackInstanceForLeak(toolStrip);
			tracker.TrackInstanceForLeak(toolStrip2);
			ErrorReporter.ReportOnce("Shutting down", SqlExceptionBuilder.CreateSqlException(6005, "SHUTDOWN is in progress.\nLogin failed for user 'xxx'. Only administrators may connect at this time."));
			ErrorReporter.ReportOnce("Non-sql shutdown", new Exception("SHUTDOWN is in progress.\nLogin failed for user 'xxx'. Only administrators may connect at this time."));
			ErrorReporter.ReportOnce("Non-shutdown sql", SqlExceptionBuilder.CreateSqlException(7303, "Cannot initialize the data source object of OLE DB provider"));
			tracker.StopTrackingInstanceForLeak(toolStrip2);

			AssertEquals("Should be no leak reported", false, ErrorReporter.LastExceptionsReported().
				Any(exceptionText => exceptionText.IndexOf("Leak detected", StringComparison.InvariantCulture) >= 0));

			ErrorReporter.Clear();
		}

		public void TestNoLeakDetectedWhenNetworkSqlExceptionThrown()
		{
			ErrorReporter.Clear();
			var toolStrip = new KToolStrip { Name = "tool strip 1" };
			var toolStrip2 = new KToolStrip { Name = "tool strip 2" };
			ForceHandleCreation(toolStrip, toolStrip2);

			var tracker = new ControlTracker();
			tracker.TrackInstanceForLeak(toolStrip);
			tracker.TrackInstanceForLeak(toolStrip2);
			ErrorReporter.ReportOnce("The server was not found or was not accessible", SqlExceptionBuilder.CreateSqlException(53, "A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)"));
			tracker.StopTrackingInstanceForLeak(toolStrip2);

			AssertEquals("There should be no further exceptions thrown", "The server was not found or was not accessible", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestLeakDetectedWhenNonSQLNetworkExceptionThrown()
		{
			ErrorReporter.Clear();
			var toolStrip = new KToolStrip { Name = "tool strip 1" };
			var toolStrip2 = new KToolStrip { Name = "tool strip 2" };
			ForceHandleCreation(toolStrip, toolStrip2);

			var tracker = new ControlTracker();
			tracker.TrackInstanceForLeak(toolStrip);
			tracker.TrackInstanceForLeak(toolStrip2);
			ErrorReporter.ReportOnce("The server was not found or was not accessible", new Exception("A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)"));
			tracker.StopTrackingInstanceForLeak(toolStrip2);

			AssertContains("There should be a leak exception thrown", "Leak detected", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestNoLeakDetectedWhenTCPConnectionFailureThrown()
		{
			ErrorReporter.Clear();
			var toolStrip = new KToolStrip { Name = "tool strip 1" };
			var toolStrip2 = new KToolStrip { Name = "tool strip 2" };
			ForceHandleCreation(toolStrip, toolStrip2);
			var tracker = new ControlTracker();
			tracker.TrackInstanceForLeak(toolStrip);
			tracker.TrackInstanceForLeak(toolStrip2);
			ErrorReporter.ReportOnce("Specified network name is no longer available", SqlExceptionBuilder.CreateSqlException(233, "A connection was successfully established with the server, but then an error occurred during the pre-login handshake. (provider: TCP Provider, error: 0 - The specified network name is no longer available.)"));

			tracker.StopTrackingInstanceForLeak(toolStrip2);
			AssertEquals("There should be no further exceptions thrown", "Specified network name is no longer available", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
		public void TestNoLeakDetectedWhenNonSQLTCPConnectionFailureThrown()
		{
			ErrorReporter.Clear();
			var toolStrip = new KToolStrip { Name = "tool strip 1" };
			var toolStrip2 = new KToolStrip { Name = "tool strip 2" };
			ForceHandleCreation(toolStrip, toolStrip2);
			var tracker = new ControlTracker();
			tracker.TrackInstanceForLeak(toolStrip);
			tracker.TrackInstanceForLeak(toolStrip2);
			ErrorReporter.ReportOnce("Specified network name is no longer available", new Exception("A connection was successfully established with the server, but then an error occurred during the pre-login handshake. (provider: TCP Provider, error: 0 - The specified network name is no longer available.)"));
			tracker.StopTrackingInstanceForLeak(toolStrip2);
			AssertContains("There should be a leak exception thrown", "Leak detected", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		void ForceHandleCreation(params Control[] controls)
			=> controls.Select(c => c.Handle).ToArray();

		public void TestIncludesParentChanges()
		{
			var tracker = new ControlTracker();
			using (var parent = new Form { Name = "Bob" })
			using (var t1 = new TrackedBox(tracker) { Name = "T1", Dock = DockStyle.Top })
			using (var t2 = new TrackedBox(tracker) { Name = "T2", Dock = DockStyle.Bottom })
			{
				parent.Controls.Add(t1);
				parent.Controls.Add(t2);
				parent.Show();

				ThisShouldBeInTheRemovedStack(t1);
				parent.Dispose();

				AssertContains(nameof(ThisShouldBeInTheRemovedStack), ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		void ThisShouldBeInTheRemovedStack(Control c)
			=> c.Parent.Controls.Remove(c);

		public void TestStopTrackingWithDisposedControl()
		{
			var tracker = new ControlTracker();

			var control = new Control();
			control.CreateControl();
			tracker.TrackInstanceForLeak(control);

			AssertEquals(1, tracker.ControlCount);
			control.Dispose();
			tracker.StopTrackingInstanceForLeak(control);
			AssertEquals(0, tracker.ControlCount);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		void AddToTracker(ControlTracker tracker, bool dispose)
		{
			var control = new Control();
			control.CreateControl();
			control.Name = "Collectable";
			tracker.TrackInstanceForLeak(control);
			if (dispose)
			{
				control.Dispose();
			}
		}

		class TrackedBox : TextBox
		{
			readonly ControlTracker tracker;

			public TrackedBox(ControlTracker tracker)
				=> this.tracker = tracker;

			protected override void OnHandleCreated(EventArgs e)
			{
				base.OnHandleCreated(e);
				tracker.TrackInstanceForLeak(this);
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				tracker.StopTrackingInstanceForLeak(this);
			}
		}
	}
}
