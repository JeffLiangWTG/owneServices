using System.Collections.Generic;

namespace Enterprise.AlwaysOn.Setup
{
	public interface IFailoverCluster
	{
		/// <summary>
		/// SELECT * FROM sys.dm_hadr_cluster
		/// cluster_name
		/// </summary>
		string Name { get; }

		/// <summary>
		/// SELECT * FROM sys.dm_hadr_cluster_members WHERE member_type = 0;
		/// member_type: 0 = WSFC node, 1 = Disk witness, 2 = File share witness
		/// member_name,
		/// member_state: 0 = Offline, 1 = Online
		/// </summary>
		IEnumerable<string> Nodes { get; }
	}
}
