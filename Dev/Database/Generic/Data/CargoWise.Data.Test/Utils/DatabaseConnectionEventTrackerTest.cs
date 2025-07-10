using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DatabaseConnectionEventTrackerTest : TestCase
	{
		public void TestGetInstance()
		{
			AssertNotNull("Valid Instance should be returned", DatabaseConnectionEventTracker.Instance);
			AssertEquals("Is enabled by default", true, DatabaseConnectionEventTracker.Instance.IsEnabled);
		}

		[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "this assembly has no reference to Enterprise libraries")]
		public void TestClear()
		{
			using (var connection = new SqlConnection(@"Server=(localdb)\V11.0"))
			{
				DatabaseConnectionEventTracker.Instance.Clear();
				DatabaseConnectionEventTracker.Instance.AddConnectionEvent(connection);

				AssertEquals("PreCondition: Event Added", true, DatabaseConnectionEventTracker.Instance.HasConnections);

				DatabaseConnectionEventTracker.Instance.Clear();
				AssertEquals("Events Cleared", false, DatabaseConnectionEventTracker.Instance.HasConnections);
			}
		}

		public void TestConnectionEventTrapping()
		{
			DatabaseConnectionEventTracker.Instance.Clear();
			AssertEquals("PreCondition: Events Cleared", false, DatabaseConnectionEventTracker.Instance.HasConnections);

			using (var conn = Db.NewAdminConnection())
			{
				AssertEquals("Event Added", false, DatabaseConnectionEventTracker.Instance.HasConnections);
				DatabaseConnectionEventTracker.Instance.Clear();
			}

			AssertEquals("Event Added", false, DatabaseConnectionEventTracker.Instance.HasConnections);
		}
	}
}
