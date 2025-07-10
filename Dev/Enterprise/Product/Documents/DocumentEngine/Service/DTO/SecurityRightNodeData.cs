using System.Collections.Generic;

namespace Enterprise.DocumentEngine
{
	public class SecurityRightNodeData
	{
		public string Name { get; set; }

		public string Code { get; set; }

		public List<SecurityRightNodeData> ChildRights { get; set; }
	}
}
