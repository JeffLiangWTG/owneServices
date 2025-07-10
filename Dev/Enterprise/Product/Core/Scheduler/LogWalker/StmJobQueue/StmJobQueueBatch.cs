using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.EntityFramework.Extensions;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.LogWalker
{
	public sealed class StmJobQueueBatch : Disposable
	{
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public StmJobQueueBatch(IList<AppLockedItem<IQueuedLog>> values)
			: base(isListened: false)
		{
			Values = values;
		}

		internal IList<AppLockedItem<IQueuedLog>> Values { get; }
		internal int ItemsLoaded { get; set; }

		protected override void Dispose(bool isDisposing)
		{
			foreach (var value in Values)
			{
				value.Dispose();
			}
		}
	}
}
