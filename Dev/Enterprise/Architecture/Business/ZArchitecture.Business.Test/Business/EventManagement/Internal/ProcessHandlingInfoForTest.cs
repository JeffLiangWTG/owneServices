using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business.Business.EventManagement;

namespace Enterprise.ZArchitecture.Business.EventManagement.Testing
{
	sealed class ProcessHandlingInfoForTest : ProcessHandlingInfo
	{
		internal ProcessHandlingInfoForTest(BusinessObject logParent, IEnumerable<BusinessObject> children)
			: this(logParent, children, Array.Empty<PropagationLink>())
		{
		}

		internal ProcessHandlingInfoForTest(BusinessObject logParent, IEnumerable<IBaseTrigger> parentTriggers)
			: this(logParent, l => parentTriggers)
		{
		}

		internal ProcessHandlingInfoForTest(BusinessObject logParent, IEnumerable<BusinessObject> children, IEnumerable<PropagationLink> propagationTargets)
			: base(logParent)
		{
			this.propagationTargets = propagationTargets;
		}

		internal ProcessHandlingInfoForTest(BusinessObject logParent, IEnumerable<BusinessObject> children, IEnumerable<PropagationLink> propagationTargets, IEnumerable<CascadingLink> cascadingTargets)
			: this(logParent, children, propagationTargets)
		{
			this.cascadingTargets = cascadingTargets;
		}

		internal ProcessHandlingInfoForTest(BusinessObject logParent, Func<IStmALog, IEnumerable<IBaseTrigger>> getParentTriggers)
			: base(logParent)
		{
			this.getParentTriggers = getParentTriggers;
		}

		readonly IEnumerable<PropagationLink> propagationTargets;
		readonly IEnumerable<CascadingLink> cascadingTargets;

		readonly Func<IStmALog, IEnumerable<IBaseTrigger>> getParentTriggers;

		protected override IEnumerable<PropagationLink> PopulatePropagationTargets()
		{
			return propagationTargets;
		}

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			return cascadingTargets;
		}

		protected override IEnumerable<IBaseTrigger> PopulateParentTriggers(IStmALog logBeingAdded)
		{
			return getParentTriggers?.Invoke(logBeingAdded);
		}
	}
}
