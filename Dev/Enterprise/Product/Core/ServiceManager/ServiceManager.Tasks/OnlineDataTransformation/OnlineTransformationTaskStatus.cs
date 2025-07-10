using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.ServiceManager.Tasks.OnlineDataTransformation
{
	public class OnlineTransformationTaskStatus
	{
		public OnlineTransformationTaskStatus(IEnumerable<string> completed, IEnumerable<string> pending)
		{
			Argument.NotNull(completed, nameof(completed));
			Argument.NotNull(pending, nameof(pending));

			Completed = completed.Distinct(StringComparer.Ordinal).ToArray();
			Pending = pending.Distinct(StringComparer.Ordinal).ToArray();
		}

		public IEnumerable<string> Completed { get; }
		public IEnumerable<string> Pending { get; }
	}
}
