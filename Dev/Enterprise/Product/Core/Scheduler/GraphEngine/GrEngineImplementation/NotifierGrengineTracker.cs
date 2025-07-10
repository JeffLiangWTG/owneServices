using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.GraphEngine;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Scheduler.GraphEngine
{
	class NotifierGrengineTracker<TKey, TEntity> : IGrEngineTracker<TKey, TEntity>
	{
		INotifications notifications;

		internal IDisposable WithNotifications(INotifications newNotifications)
		{
			var old = notifications;
			notifications = newNotifications;
			return new DisposableAction(() => notifications = old);
		}

		public void Loaded(IEnumerable<TEntity> entities)
		{
			if (entities.Any())
			{
				notifications?.Add(NotificationType.Information, string.Format(CultureInfo.InvariantCulture, (NoResString)"Entities loaded ({0})", entities.Count()));
			}
		}

		public void VetexAdded(IEnumerable<TEntity> entities)
		{
			if (entities.Any())
			{
				notifications?.Add(NotificationType.Information, string.Format(CultureInfo.InvariantCulture, (NoResString)"Entities added ({0})", entities.Count()));
			}
		}

		public void VetexRemoved(IEnumerable<TEntity> entities, int removedFromMiddle)
		{
			if (entities.Any())
			{
				notifications?.Add(NotificationType.Information, string.Format(CultureInfo.InvariantCulture, (NoResString)"Entities removed ({0})", entities.Count()));
			}

			if (removedFromMiddle > 0)
			{
				notifications?.Add(NotificationType.Information, FormattableString.Invariant($"Prereqs reassigned ({removedFromMiddle})"));
			}
		}
	}
}
