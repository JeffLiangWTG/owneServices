using Enterprise.AlwaysOn.Setup;
using NUnit.Framework;

namespace Enterprise.AlwaysOn.Testing
{
	class ReplicaOptionsTest : TestCase
	{
		public void TestAttributes()
		{
			IReplicaOptions replicaOptions = NewReplicaOptionsForTesting(AvailabilityMode.SYNCHRONOUS_COMMIT, FailoverMode.MANUAL);
			AssertEquals("CommitMode", AvailabilityMode.SYNCHRONOUS_COMMIT, replicaOptions.CommitMode);
			AssertEquals("Failover", FailoverMode.MANUAL, replicaOptions.Failover);
			AssertEquals("SecondaryRole", AllowConnection.ALL, replicaOptions.SecondaryAllowConnection);
			AssertEquals("SecondaryReadOnlyRoutingUrl", "", replicaOptions.SecondaryReadOnlyRoutingUrl);

			replicaOptions = ReplicaOptions.NewFromRawData(0, 1, 1, "TestUrl");
			AssertEquals("CommitMode", AvailabilityMode.ASYNCHRONOUS_COMMIT, replicaOptions.CommitMode);
			AssertEquals("Failover", FailoverMode.MANUAL, replicaOptions.Failover);
			AssertEquals("SecondaryRole", AllowConnection.READ_ONLY, replicaOptions.SecondaryAllowConnection);
			AssertEquals("SecondaryReadOnlyRoutingUrl", "TestUrl", replicaOptions.SecondaryReadOnlyRoutingUrl);

			replicaOptions = ReplicaOptions.NewFromRawData(0, 0, 1, "TestUrl");
			AssertEquals("CommitMode", AvailabilityMode.ASYNCHRONOUS_COMMIT, replicaOptions.CommitMode);
			AssertEquals("Failover", FailoverMode.AUTOMATIC, replicaOptions.Failover);
			AssertEquals("SecondaryRole", AllowConnection.READ_ONLY, replicaOptions.SecondaryAllowConnection);
			AssertEquals("SecondaryReadOnlyRoutingUrl", "TestUrl", replicaOptions.SecondaryReadOnlyRoutingUrl);
		}

		public void TestValidate()
		{
			string validationError = null;

			IReplicaOptions replicaOptions = NewReplicaOptionsForTesting(AvailabilityMode.ASYNCHRONOUS_COMMIT, FailoverMode.MANUAL);
			AssertEquals("Are Replica Options valid?", true, replicaOptions.Validate(out validationError));
			AssertNull("Validation Error Message should be null", validationError);

			replicaOptions = NewReplicaOptionsForTesting(AvailabilityMode.ASYNCHRONOUS_COMMIT, FailoverMode.AUTOMATIC);
			AssertEquals("Are Replica Options valid?", false, replicaOptions.Validate(out validationError));
			AssertEquals("Validation Error Message", "Automatic failover requires synchronous-commit availability. Please either change the failover mode to manual or the availability mode to synchronous commit.", validationError);

			replicaOptions = NewReplicaOptionsForTesting(AvailabilityMode.SYNCHRONOUS_COMMIT, FailoverMode.AUTOMATIC);
			AssertEquals("Are Replica Options valid?", true, replicaOptions.Validate(out validationError));
			AssertNull("Validation Error Message should be null", validationError);
		}

		public void TestNoConnectionBecomesReadOnly()
		{
			IReplicaOptions replicaOptions = NewReplicaOptionsForTesting(AvailabilityMode.SYNCHRONOUS_COMMIT, FailoverMode.AUTOMATIC, AllowConnection.NO);

			AssertNotEquals("Allow connection should never be NO", AllowConnection.NO, replicaOptions.SecondaryAllowConnection);
		}

		public static IReplicaOptions NewReplicaOptionsForTesting(AvailabilityMode commitMode, FailoverMode failover, AllowConnection secRoleAllowConnection = AllowConnection.ALL, string secReadOnlyRoutingUrl = "")
		{
			return new ReplicaOptions(commitMode, failover, secRoleAllowConnection, secReadOnlyRoutingUrl);
		}
	}
}
