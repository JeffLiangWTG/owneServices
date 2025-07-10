using System.Collections.Generic;

namespace Enterprise.DataTransfer.Native.Common
{
	public class CriteriaGroup
	{
		public string Type { get; set; }
		public IEnumerable<EntityCriteria> Criterias { get; set; }
	}
}
