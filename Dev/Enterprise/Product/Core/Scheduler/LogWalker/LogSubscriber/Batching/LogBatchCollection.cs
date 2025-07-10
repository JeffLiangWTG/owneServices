using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Enterprise.LogWalker
{
	public class LogBatchCollection : Collection<LogBatch>
	{
		public LogBatchCollection(IEnumerable<LogBatch> logs)
			: base(logs.ToList())
		{
		}
	}
}
