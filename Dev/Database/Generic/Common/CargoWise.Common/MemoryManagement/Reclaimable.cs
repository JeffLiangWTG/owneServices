using System;

namespace CargoWise.Common.MemoryManagement.Internal
{
	class Reclaimable : IReclaimable
	{
		internal Reclaimable(WeakReference targetWeakReference, ReclaimWrapper<object> cleanupAction)
		{
			ConstructionTime = DateTime.Now;	// DEBUGGING ONLY - DON'T WANT DB HIT HERE...
			TargetWeakReference = targetWeakReference;
			this.cleanupAction = cleanupAction;
		}

		public string Description { get; internal set; }
		public DateTime ConstructionTime { get; private set; }

		public readonly WeakReference TargetWeakReference;
		readonly ReclaimWrapper<object> cleanupAction;

		public bool IsAlive
		{
			get { return TargetWeakReference == null || TargetWeakReference.IsAlive; }
		}

		public FlushResult CleanUp(FlushAction action)
		{
			if (cleanupAction != null)
			{
				if (TargetWeakReference == null)
				{
					return cleanupAction(null, action);
				}
				else
				{
					object target = TargetWeakReference.Target;
					return target != null ? cleanupAction(target, action) : FlushResult.NotRequired;
				}
			}
			else
			{
				return FlushResult.NotRequired;
			}
		}
	}

	sealed class Reclaimable<T> : Reclaimable
	{
		internal Reclaimable(T target, ReclaimWrapper<T> cleanupAction)
			: base(target != null ? new WeakReference(target) : null, (lambdaTarget, action) => cleanupAction((T)lambdaTarget, action))
		{
		}
	}
}
