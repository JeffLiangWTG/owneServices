using System;
using System.Drawing;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Controls.Internal.Testing
{
	sealed class GCTrackerTest : TestCase
	{
		public void TestGCTrackerDoesNotReportLeakWhenSqlShutdownThrown()
		{
			ErrorReporter.Clear();

			var testControl = new KToolStrip();
			var otherTestControl = new KToolStrip();
			var testTracker = GCTracker.Track("Test key", testControl);
			testTracker.NotifyDisposed(false);

			var reportingTestTracker = GCTracker.Track("Test key", otherTestControl);
			ErrorReporter.ReportOnce("Shutting down", SqlExceptionBuilder.CreateSqlException(6005, "SHUTDOWN is in progress.\nLogin failed for user 'xxx'. Only administrators may connect at this time."));
			reportingTestTracker.NotifyDisposed(false);

			AssertEquals("Finaliser warning should not come out", "Shutting down", ErrorReporter.LastMessageReported);
		}

		public void TestGCTrackerReportsLeakWhenNonShutdownSqlExceptionThrown()
		{
			ErrorReporter.Clear();

			var testControl = new KToolStrip();
			var otherTestControl = new KToolStrip();
			var testTracker = GCTracker.Track("Test key", testControl);
			testTracker.NotifyDisposed(false);

			var reportingTestTracker = GCTracker.Track("Test key", otherTestControl);
			ErrorReporter.ReportOnce("Non-shutdown sql", SqlExceptionBuilder.CreateSqlException(7303, "Cannot initialize the data source object of OLE DB provider"));
			reportingTestTracker.NotifyDisposed(false);

			AssertContains("Finaliser warning should come out", "A finalizer has been called", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestGCTrackerReportsLeakWhenNonSqlShutdownExceptionThrown()
		{
			ErrorReporter.Clear();

			var testControl = new KToolStrip();
			var otherTestControl = new KToolStrip();
			var testTracker = GCTracker.Track("Test key", testControl);
			testTracker.NotifyDisposed(false);

			var reportingTestTracker = GCTracker.Track("Test key", otherTestControl);
			ErrorReporter.ReportOnce("Non-sql shutdown", new Exception("SHUTDOWN is in progress.\nLogin failed for user 'xxx'. Only administrators may connect at this time."));
			reportingTestTracker.NotifyDisposed(false);

			AssertContains("Finaliser warning should come out", "A finalizer has been called", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestGCTrackerDoesNotReportLeakWhenShutdownSqlAndOtherExceptionsThrown()
		{
			ErrorReporter.Clear();

			var testControl = new KToolStrip();
			var otherTestControl = new KToolStrip();
			var testTracker = GCTracker.Track("Test key", testControl);
			testTracker.NotifyDisposed(false);

			var reportingTestTracker = GCTracker.Track("Test key", otherTestControl);
			ErrorReporter.ReportOnce("Non-sql shutdown", new Exception("SHUTDOWN is in progress.\nLogin failed for user 'xxx'. Only administrators may connect at this time."));
			ErrorReporter.ReportOnce("Non-shutdown sql", SqlExceptionBuilder.CreateSqlException(7303, "Cannot initialize the data source object of OLE DB provider"));
			ErrorReporter.ReportOnce("Shutting down", SqlExceptionBuilder.CreateSqlException(6005, "SHUTDOWN is in progress.\nLogin failed for user 'xxx'. Only administrators may connect at this time."));
			reportingTestTracker.NotifyDisposed(false);

			AssertEquals("Finaliser warning should not come out", false, ErrorReporter.LastExceptionsReported().
				Any(exceptionText => exceptionText.IndexOf("A finalizer has been called on", StringComparison.InvariantCulture) >= 0));

			ErrorReporter.Clear();
		}

		public void TestGCTrackerDoesNotReportLeakWhenSqlNetworkExceptionThrown()
		{
			ErrorReporter.Clear();

			var testControl = new KToolStrip();
			var otherTestControl = new KToolStrip();
			var testTracker = GCTracker.Track("Test key", testControl);
			testTracker.NotifyDisposed(false);

			var reportingTestTracker = GCTracker.Track("Test key", otherTestControl);
			ErrorReporter.ReportOnce("The server was not found or was not accessible", SqlExceptionBuilder.CreateSqlException(53, "A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)"));
			reportingTestTracker.NotifyDisposed(false);

			AssertEquals("Finaliser warning should not come out", "The server was not found or was not accessible", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestGCTrackerReportsLeakWhenNonSqlNetworkExceptionThrown()
		{
			ErrorReporter.Clear();

			var testControl = new KToolStrip();
			var otherTestControl = new KToolStrip();
			var testTracker = GCTracker.Track("Test key", testControl);
			testTracker.NotifyDisposed(false);

			var reportingTestTracker = GCTracker.Track("Test key", otherTestControl);
			ErrorReporter.ReportOnce("The server was not found or was not accessible", new Exception("A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)"));
			reportingTestTracker.NotifyDisposed(false);

			AssertContains("Finaliser warning should come out", "A finalizer has been called", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestGCTrackerDoesNotReportLeakWhenSqlAccessExceptionThrown()
		{
			ErrorReporter.Clear();

			var testControl = new KToolStrip();
			var otherTestControl = new KToolStrip();
			var testTracker = GCTracker.Track("Test key", testControl);
			testTracker.NotifyDisposed(false);

			var reportingTestTracker = GCTracker.Track("Test key", otherTestControl);
			ErrorReporter.ReportOnce("The server was not found or was not accessible", SqlExceptionBuilder.CreateSqlException(53, "Unable to access availability database 'CargoWiseOneDFOBNJPRO' because the database replica is not in the PRIMARY or SECONDARY role. Connections to an availability database is permitted only when the database replica is in the PRIMARY or SECONDARY role. Try the operation again later."));
			reportingTestTracker.NotifyDisposed(false);

			AssertEquals("Finaliser warning should not come out", "The server was not found or was not accessible", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestGCTrackerReportsLeakWhenNonSqlAccessExceptionThrown()
		{
			ErrorReporter.Clear();

			var testControl = new KToolStrip();
			var otherTestControl = new KToolStrip();
			var testTracker = GCTracker.Track("Test key", testControl);
			testTracker.NotifyDisposed(false);

			var reportingTestTracker = GCTracker.Track("Test key", otherTestControl);
			ErrorReporter.ReportOnce("The server was not found or was not accessible", new Exception("Unable to access availability database 'CargoWiseOneDFOBNJPRO' because the database replica is not in the PRIMARY or SECONDARY role. Connections to an availability database is permitted only when the database replica is in the PRIMARY or SECONDARY role. Try the operation again later."));
			reportingTestTracker.NotifyDisposed(false);

			AssertContains("Finaliser warning should come out", "A finalizer has been called", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestGCTrackerDoesNotReportLeakWhenTCPConnectionFailureThrown()
		{
			ErrorReporter.Clear();

			var testControl = new KToolStrip();
			var otherTestControl = new KToolStrip();
			var testTracker = GCTracker.Track("Test key", testControl);
			testTracker.NotifyDisposed(false);

			var reportingTestTracker = GCTracker.Track("Test key", otherTestControl);
			ErrorReporter.ReportOnce("Specified network name is no longer available", SqlExceptionBuilder.CreateSqlException(233, "A connection was successfully established with the server, but then an error occurred during the pre-login handshake. (provider: TCP Provider, error: 0 - The specified network name is no longer available.)"));
			reportingTestTracker.NotifyDisposed(false);

			AssertEquals("Finaliser warning should not come out", "Specified network name is no longer available", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestGCTrackerReportsLeakWhenNonSQLTCPConnectionFailureThrown()
		{
			ErrorReporter.Clear();

			var testControl = new KToolStrip();
			var otherTestControl = new KToolStrip();
			var testTracker = GCTracker.Track("Test key", testControl);
			testTracker.NotifyDisposed(false);

			var reportingTestTracker = GCTracker.Track("Test key", otherTestControl);
			ErrorReporter.ReportOnce("Specified network name is no longer available", new Exception("A connection was successfully established with the server, but then an error occurred during the pre-login handshake. (provider: TCP Provider, error: 0 - The specified network name is no longer available.)"));
			reportingTestTracker.NotifyDisposed(false);

			AssertContains("Finaliser warning should come out", "A finalizer has been called", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1105:Do not use System.Windows.Forms.UserControl or System.Windows.Forms.KUserControl Class", Justification = "Unit Test")]
		public void TestGCTrackerDoesNotReportLeakWhenArgumentExceptionThrownWithinSystemDrawing()
		{
			ErrorReporter.Clear();
			using var testControl = new KUserControl();
			var testTracker = GCTracker.Track("Test key", testControl);
			testTracker.NotifyDisposed(false);

			using var otherTestControl = new KUserControl();
			var reportingTestTracker = GCTracker.Track("Test key", otherTestControl);
			var exception = CreateArgumentException();

			ErrorReporter.ReportOnce("The system graphical resource is exhausted", exception);
			reportingTestTracker.NotifyDisposed(false);

			AssertNotContains("Finaliser warning should not come out", "A finalizer has been called", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		ArgumentException CreateArgumentException()
		{
			try
			{
				var buffer = new byte[] { 1 };
				_ = new Bitmap(stream: new MemoryStream(buffer));
				return null;
			}
			catch (ArgumentException ex)
			{
				return ex;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			GCTracker.ClearListForTest();
		}

		protected override void TearDown()
		{
			GCTracker.ClearListForTest();
			base.TearDown();
		}
	}
}
