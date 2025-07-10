using System;
using System.Collections.Generic;

namespace ServiceManager.Integration.Abstractions
{
	/// <summary>
	/// Indicates the current status of a service task ranked in order from worst status (0) upwards
	/// </summary>
	public enum ServiceTaskStatus
	{
		/// <summary>
		/// No such task was found in the database. 
		/// </summary>
		NoSuchTaskIsInstalledInThisDb = 0,

		/// <summary>
		/// Service task exists but is in the 'inactive' state. The Windows NT service is running. 
		/// </summary>
		ServiceTaskIsInactive = 1,

		/// <summary>
		/// Task is listed but has no physical machine on which to run itself
		/// </summary>
		NoMachineUponWhichToRunTheTask = 2,

		/// <summary>
		/// No successful response was received from any hosts.
		/// </summary>
		NoAvailableHosts = 3,

		/// <summary>
		/// Service task is running healthily on at least one machine
		/// </summary>
		AtLeastOneHostIsRunningHealthily = 4,
	}

	public interface IServiceManagerQuerier
	{
		ServiceTaskStatus CheckStateOfNamedServiceTask(string codeOfServiceTaskToCheck);
		IEnumerable<string> GetServiceTasksByCategory(string category);
		bool TryGetServiceTaskNextRunTime(string serviceTaskCode, out DateTimeOffset? nextRunTime);
		bool TryGetServiceTaskBranchPK(string serviceTaskCode, out Guid branch);
	}
}
