using System.Collections.Generic;

namespace Enterprise.AlwaysOn.Setup
{
	static class FailoverClusterFactory
	{
		public static IFailoverCluster New(string clusterName, IEnumerable<string> nodes)
		{
			var result = new FailoverCluster(clusterName, nodes);
			return result;
		}
	}
}
