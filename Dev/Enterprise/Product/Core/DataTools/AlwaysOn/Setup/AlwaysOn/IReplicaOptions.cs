	using System;

namespace Enterprise.AlwaysOn.Setup
{
	public enum AvailabilityMode
	{
		ASYNCHRONOUS_COMMIT = 0,
		SYNCHRONOUS_COMMIT = 1,
	}

	public enum FailoverMode
	{
		AUTOMATIC = 0,
		MANUAL = 1,
	}

	public enum AllowConnection
	{
		NO = 0,
		READ_ONLY = 1,
		ALL = 2,
		READ_WRITE = 3,
	}

	public interface IReplicaOptions : IComparable<IReplicaOptions>
	{
		AvailabilityMode CommitMode { get; }
		FailoverMode Failover { get; }
		AllowConnection SecondaryAllowConnection { get; }
		string SecondaryReadOnlyRoutingUrl { get; }
		bool Validate(out string validationError);
	}
}
