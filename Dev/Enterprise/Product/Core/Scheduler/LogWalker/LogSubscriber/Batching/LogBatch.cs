using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework.Extensions;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.LogWalker
{
	public class LogBatch : IGrouping<LogBatchKey, AppLockedItem<IQueuedLog>>
	{
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public LogBatch(LogBatchKey key, IList<AppLockedItem<IQueuedLog>> items)
		{
			this.key = key ?? throw new ArgumentNullException(nameof(key));

			if (items == null || items.Any(i => i.Item == null))
			{
				throw new ArgumentNullException(nameof(items));
			}

			this.items = items;
		}

		readonly LogBatchKey key;
		readonly IList<AppLockedItem<IQueuedLog>> items;

		#region IGrouping

		public LogBatchKey Key => key;
		public IEnumerator<AppLockedItem<IQueuedLog>> GetEnumerator() => items.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		#endregion

		#region Other members

		public int Count => items.Count;

		#endregion
	}
}
