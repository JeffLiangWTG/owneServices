using System;

namespace Enterprise.AlwaysOn.Setup
{
	public class ReplicaOptions : IReplicaOptions
	{
		public ReplicaOptions(AvailabilityMode commitMode, FailoverMode failover, AllowConnection secRoleAllowConnection, string secReadOnlyRoutingUrl)
		{
			this.commitMode = commitMode;
			this.failover = failover;
			secondaryRoleAllowConnection = (secRoleAllowConnection == AllowConnection.NO ? AllowConnection.READ_ONLY : secRoleAllowConnection);
			secondaryReadOnlyRoutingUrl = secReadOnlyRoutingUrl;
		}

		#region Factory Methods

		public static ReplicaOptions NewFromRawData(int commitMode, int failover, int secRoleAllowConnection, string secReadOnlyRoutingUrl)
		{
			return new ReplicaOptions((AvailabilityMode)commitMode, (FailoverMode)failover, (AllowConnection)secRoleAllowConnection, secReadOnlyRoutingUrl);
		}

		public static IReplicaOptions NewFromTextOptions(string commitModeText, string failoverModeText, string allowConnectionText, string secReadOnlyRoutingUrl)
		{
			AvailabilityMode commitMode;
			FailoverMode failoverMode;
			AllowConnection allowConnection;

			switch (commitModeText.ToUpper())
			{
				case "SYNCHRONOUS":
					commitMode = AvailabilityMode.SYNCHRONOUS_COMMIT;
					break;
				default:
					commitMode = AvailabilityMode.ASYNCHRONOUS_COMMIT;
					break;
			}

			switch (failoverModeText.ToUpper())
			{
				case "AUTOMATIC":
					failoverMode = FailoverMode.AUTOMATIC;
					break;
				default:
					failoverMode = FailoverMode.MANUAL;
					break;
			}

			switch (allowConnectionText.ToUpper())
			{
				case "NO":
					allowConnection = AllowConnection.NO;
					break;
				case "READ-ONLY":
					allowConnection = AllowConnection.READ_ONLY;
					break;
				case "READ-WRITE":
					allowConnection = AllowConnection.READ_WRITE;
					break;
				default:
					allowConnection = AllowConnection.ALL;
					break;
			}

			return new ReplicaOptions(commitMode, failoverMode, allowConnection, secReadOnlyRoutingUrl);
		}

		#endregion // Factory Methods

		#region IReplicaOptions Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		bool IReplicaOptions.Validate(out string validationError)
		{
			validationError = null;

			if (failover == FailoverMode.AUTOMATIC && commitMode != AvailabilityMode.SYNCHRONOUS_COMMIT)
			{
				validationError = "Automatic failover requires synchronous-commit availability. Please either change the failover mode to manual or the availability mode to synchronous commit.";
			}

			return (validationError == null);
		}

		AvailabilityMode IReplicaOptions.CommitMode
		{
			get { return commitMode; }
		}
		readonly AvailabilityMode commitMode;

		FailoverMode IReplicaOptions.Failover
		{
			get { return failover; }
		}
		readonly FailoverMode failover;

		AllowConnection IReplicaOptions.SecondaryAllowConnection
		{
			get { return secondaryRoleAllowConnection; }
		}
		readonly AllowConnection secondaryRoleAllowConnection;

		string IReplicaOptions.SecondaryReadOnlyRoutingUrl
		{
			get { return secondaryReadOnlyRoutingUrl; }
		}
		readonly string secondaryReadOnlyRoutingUrl;

		int IComparable<IReplicaOptions>.CompareTo(IReplicaOptions otherReplicaOptions)
		{
			if (otherReplicaOptions == null)
			{
				throw new InvalidOperationException("otherReplicaOptions cannot be null");
			}

			return (
				commitMode.CompareTo(otherReplicaOptions.CommitMode) * 10000
				+ failover.CompareTo(otherReplicaOptions.Failover) * 1000
				+ secondaryRoleAllowConnection.CompareTo(otherReplicaOptions.SecondaryAllowConnection) * 100
				+ secondaryReadOnlyRoutingUrl.CompareTo(otherReplicaOptions.SecondaryReadOnlyRoutingUrl) * 10
			);
		}

		#endregion // IReplicaOptions Members
	}
}
