using System.Collections.Generic;

namespace Enterprise.AlwaysOn.Setup
{
	class FailoverCluster : IFailoverCluster
	{
		public FailoverCluster(string name, IEnumerable<string> nodes)
		{
			this.name = name;
			this.nodes = nodes;
		}

		string IFailoverCluster.Name
		{
			get { return name; }
		}
		readonly string name;

		IEnumerable<string> IFailoverCluster.Nodes
		{
			get { return nodes; }
		}
		readonly IEnumerable<string> nodes;
	}
}
