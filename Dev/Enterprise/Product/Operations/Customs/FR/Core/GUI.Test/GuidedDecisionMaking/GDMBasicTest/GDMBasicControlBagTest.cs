using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.GDM.Testing
{
	[TestedType(typeof(GDMBasicControlBag))]
	sealed class GDMBasicControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(GDMBasicControlBag.RegionOrTerritoryOfDestinationDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => GDMBasicControlBag.Instance;
	}
}
