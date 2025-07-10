using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyProcessHandlingInfo : ProcessHandlingInfo
	{
		public DummyProcessHandlingInfo(DummyProcessHandlingInfoProviderBizo parent)
			: base(parent)
		{
			this.dummy = parent;
		}

		readonly DummyProcessHandlingInfoProviderBizo dummy;

		public void OverrideParametersToPropagate(string[] parametersToPropagate)
		{
			this.parametersToPropagate = parametersToPropagate;
		}

		protected override IEnumerable<PropagationLink> PopulatePropagationTargets()
		{
			var dummyParent = LogParent.Factory.Load<DummyEnterpriseBusinessObject>(dummy.Z0_Guid);
			var dummySiblings = LogParent.Factory.Load<DummyProcessHandlingInfoProviderBizo>(new ZQuery(DummyBizoSchema.Z0_Guid, dummy.Z0_Guid));

			return new[] { new PropagationLink(dummyParent, dummySiblings, "Dummy Siblings") };
		}

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			return null;
		}

		protected override IEnumerable<string> PopulateEventParametersToMatchDuringPropagation(ZString eventCode)
		{
			return parametersToPropagate ?? base.PopulateEventParametersToMatchDuringPropagation(eventCode);
		}

		string[] parametersToPropagate;
	}
}
