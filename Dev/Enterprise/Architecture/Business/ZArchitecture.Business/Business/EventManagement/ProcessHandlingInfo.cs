using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.EventManagement
{
	public abstract class ProcessHandlingInfo
	{
		protected ProcessHandlingInfo(BusinessObject logParent)
		{
			this.logParent = Argument.NotNull(logParent, "logParent");

			if (!(logParent is IProcessHandlingInfoProvider))
			{
				throw new ArgumentException("The logParent must implement " + nameof(IProcessHandlingInfoProvider) + " so that WorkFlow will know it has to propagate or cascade.");
			}

			if (!(logParent is IStmALogParent))
			{
				throw new ArgumentException("The logParent must implement " + nameof(IStmALogParent) + " so that WorkFlow can locate its logs");
			}
		}
		internal readonly BusinessObject logParent;

		public IEnumerable<CascadingLink> GetCascadingTargets(IStmALog logBeingAdded)
		{
			if (cascadingTargets == null
				&& ParentStateAllowsCascading
				&& logBeingAdded != null
				&& !logBeingAdded.SL_SE_NKEvent.IsEmpty
				&& IsEventLogApplicableForCascading(logBeingAdded))
			{
				cascadingTargets = PopulateCascadingTargets(logBeingAdded);
			}

			return cascadingTargets;
		}

		IEnumerable<CascadingLink> cascadingTargets;

		protected virtual bool ParentStateAllowsCascading
		{
			get { return logParent.IsInDatabase; }
		}

		protected virtual bool IsEventLogApplicableForCascading(IStmALog logBeingAdded)
		{
			return true;
		}

		protected abstract IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded);

		protected internal BusinessObject LogParent
		{
			get { return logParent; }
		}

		public IEnumerable<PropagationLink> GetPropagationTargets(IStmALog logBeingAdded)
		{
			var logBO = logBeingAdded as BusinessObject;
			var isDeletedLogBO = logBO != null && logBO.IsDeleted;

			return (logBeingAdded != null && !isDeletedLogBO && IsEventLogApplicableForPropagation(logBeingAdded))
				? PopulatePropagationTargets()
				: Enumerable.Empty<PropagationLink>();
		}

		protected virtual IEnumerable<PropagationLink> PopulatePropagationTargets()
		{
			return Array.Empty<PropagationLink>();
		}

		protected virtual bool IsEventLogApplicableForPropagation(IStmALog logBeingAdded)
		{
			return true;
		}

		public IEnumerable<string> GetEventParametersToMatchDuringPropagation(ZString eventCode)
		{
			return PopulateEventParametersToMatchDuringPropagation(eventCode);
		}

		protected virtual IEnumerable<string> PopulateEventParametersToMatchDuringPropagation(ZString eventCode)
		{
			return AllEventReferenceParametersCodes;
		}

		public IEnumerable<IBaseTrigger> GetParentTriggers(IStmALog logBeingAdded)
		{
			return PopulateParentTriggers(logBeingAdded);
		}

		protected virtual IEnumerable<IBaseTrigger> PopulateParentTriggers(IStmALog logBeingAdded)
		{
			yield break;
		}

		protected internal virtual Event[] StartingEventTypes
		{
			get { return Array.Empty<Event>(); }
		}

		protected internal virtual Event[] FinishingEventTypes
		{
			get { return Array.Empty<Event>(); }
		}

		protected internal virtual bool IsEventExcludedFromCascadingOrPropagation(ZString eventCode)
		{
			return !LogParent.Factory.LoadFromNaturalKey<StmEvent>(StmEventSchema.SE_Code, eventCode)?.SE_PropagateToParent ?? false;
		}

		static string[] AllEventReferenceParametersCodes
		{
			get
			{
				if (allEventParameters == null)
				{
					var parameters = typeof(CargoWise.EventReference.Constants.EventReferenceParameters.Codes).GetFields();

					allEventParameters = parameters.Select(p => p.GetRawConstantValue().ToString()).ToArray();
				}

				return allEventParameters;
			}
		}

		[ThreadStatic]
		static string[] allEventParameters;
	}
}
