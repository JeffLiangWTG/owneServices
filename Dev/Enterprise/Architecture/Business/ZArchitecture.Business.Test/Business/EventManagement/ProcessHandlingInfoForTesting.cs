using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business.Business.EventManagement;

namespace Enterprise.ZArchitecture.Business.EventManagement.Testing
{
	sealed class ProcessHandlingInfoForTesting : ProcessHandlingInfo
	{
		internal ProcessHandlingInfoForTesting(BusinessObject provider)
			: base(provider)
		{
		}

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			return null;
		}
	}
}
