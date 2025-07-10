using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Business
{
	public class IsStartableMap
	{
		public IsStartableMap(ConcurrentHashSet<ZGuid> startableTasks, ImmutableHashSet<ZGuid> tasksBlockedByRpl, ConcurrentHashSet<ZGuid> nonStartableTasks)
		{
			StartableTasks = startableTasks;
			TasksBlockedByRpl = tasksBlockedByRpl;
			NonStartableTasks = nonStartableTasks;
		}

		internal ConcurrentHashSet<ZGuid> StartableTasks { get; }
		internal ImmutableHashSet<ZGuid> TasksBlockedByRpl { get; }
		ConcurrentHashSet<ZGuid> NonStartableTasks { get; }

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "It's okay if the contents change between calls.")]
		internal bool IsStartable(IProcessTask task, Func<IProcessTask, bool> calculateIsStartableFunc)
		{
			var pk = task.PK;

			if (!StartableTasks.Contains(pk) && !NonStartableTasks.Contains(pk))
			{
				if (calculateIsStartableFunc(task))
				{
					StartableTasks.TryAdd(task.PK);
				}
				else
				{
					NonStartableTasks.TryAdd(task.PK);
				}
			}

			return StartableTasks.Contains(pk) && !TasksBlockedByRpl.Contains(pk);
		}
	}
}
