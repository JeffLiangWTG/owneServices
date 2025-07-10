using System;

namespace Enterprise.Semaphores.Common
{
	public interface IHeartbeatInfo
	{
		/// <summary>
		/// Unique identifier of heartbeat row in db.
		/// </summary>
		Guid HeartbeatId { get; }

		/// <summary>
		/// Full domain name of the computer hosting the session.
		/// </summary>
		string HostName { get; }

		/// <summary>
		/// A unique identifier for the client machine.
		/// Expected format is "{GUID}/{HostName}" where the HostName is the name of the machine that the client app is running on.
		/// </summary>
		string ClientIdentifier { get; }

		/// <summary>
		/// User PK.
		/// </summary>
		Guid UserPk { get; }

		/// <summary>
		/// Full User Name.
		/// </summary>
		string FullUserName { get; }

		/// <summary>
		/// Application Process ID.
		/// </summary>
		int ProcessId { get; }

		/// <summary>
		/// Extra session info.
		/// </summary>
		string SessionReference { get; }

		/// <summary>
		/// Staff Code for a GlbStaff, Organisation Code for an OrgContact
		/// Not guaranteed to be unique
		/// </summary>
		string LogonIdentificationCode { get; }

		/// <summary>
		/// Email address
		/// </summary>
		string EmailAddress { get; }

		/// <summary>
		/// Logon Type
		/// </summary>
		LogonType LogonType { get; }

		/// <summary>
		/// Type of application Heartbeat is for (Enterprise, RF Warehouse, etc.)
		/// </summary>
		string HeartbeatType { get; }
	}
}
