using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business.EventManagement
{
	public class PropagationDeferrer
	{
		PropagationDeferrer(BusinessObjectFactory factory)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
		}

		BusinessObjectFactory Factory { get; }

		internal static PropagationDeferrer GetInstance(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));
			return factory.GetCachedValue(nameof(PropagationDeferrer), () => new PropagationDeferrer(factory));
		}

		internal bool IsDeferred => deferPropagationIndex > 0;
		int deferPropagationIndex;

		internal void AddLogToPropagate(ProcessHandlingInfo processHandlingInfo, IStmALog logBeingAdded)
		{
			DeferredPropagations.Add((processHandlingInfo, logBeingAdded));
		}

		void PropagateEventsOnDisposal(IPropagationHandler handler)
		{
			var selected = DeferredPropagations
						.Where(p => !((p.Log as BusinessObject)?.IsDeleted == true) && !p.Log.SL_IsCancelled)
						.SelectMany(
							p => p.ProcessHandlingInfo.GetPropagationTargets(p.Log)
									.Select(l => new { p.Log, p.ProcessHandlingInfo, l.Target.LogsParentPK }));

#if NETFRAMEWORK
			var uniqueDeferredPropagations = selected.DistinctBy(p => (p.LogsParentPK,
			p.Log.SL_SE_NKEvent, p.Log.SL_Reference, p.Log.SL_IsEstimate));
#else
			var uniqueDeferredPropagations = Enumerable.DistinctBy(selected, p => (p.LogsParentPK,
			p.Log.SL_SE_NKEvent, p.Log.SL_Reference, p.Log.SL_IsEstimate));
#endif

			foreach (var deferredPropagation in uniqueDeferredPropagations)
			{
				using (EventRecursionHandler.WithEventRecursionDetection())
				{
					handler.Propagate(deferredPropagation.ProcessHandlingInfo, deferredPropagation.Log);
				}
			}

			DeferredPropagations.Clear();
		}

		List<(ProcessHandlingInfo ProcessHandlingInfo, IStmALog Log)> DeferredPropagations
		{
			get { return deferredPropagations ?? (deferredPropagations = new List<(ProcessHandlingInfo, IStmALog)>()); }
		}
		List<(ProcessHandlingInfo ProcessHandlingInfo, IStmALog Log)> deferredPropagations;

		/// <summary>
		/// Enable deferral of event propagation, this is designed solely for use with tight loops where uniform events are being added on line objects that will propagate to a parent.
		/// This avoids propagation having to check each child object with each subsequent log add, when propagation can only occur on the final child.
		/// 
		/// This must *NOT* be used in areas that may fire many different or unknown events (e.g. Factory.Save or the LogWalker).
		/// This is because it is not possible to guarantee any de-duplication logic is completely robust.
		/// 
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="propagationHandler"></param>
		/// <returns></returns>
		public static IDisposable DeferEventPropagation(BusinessObjectFactory factory, IPropagationHandler propagationHandler = null)
		{
			if (propagationHandler == null)
			{
				propagationHandler = new PropagationHandler(factory);
			}

			var deferrer = GetInstance(factory);

			return new DisposableAction(
				() => deferrer.deferPropagationIndex++,
				() =>
				{
					if (--deferrer.deferPropagationIndex == 0)
					{
						deferrer.PropagateEventsOnDisposal(propagationHandler);
					}
				}
			);
		}
	}
}
