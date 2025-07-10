using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(SupplyChainActorControlBag))]
	sealed class SupplyChainActorControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(SupplyChainActorUserControl.RoleDropEdit);
				yield return nameof(SupplyChainActorUserControl.ReferenceTextBox);
				yield return nameof(SupplyChainActorUserControl.OwnerOrganisationFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => SupplyChainActorControlBag.Instance;
	}
}
