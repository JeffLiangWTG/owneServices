using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(TraderDetailsControlBag))]
	sealed class TraderDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TraderDetailsControlBag.PrincipalDocAddressControl);
				yield return nameof(TraderDetailsControlBag.ConsignorDocAddressControl);
				yield return nameof(TraderDetailsControlBag.ConsigneeDocAddressControl);
				yield return nameof(TraderDetailsControlBag.RepresentativeDocAddressControl);
				yield return nameof(TraderDetailsControlBag.FromWarehouseGroupBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TraderDetailsControlBag.Instance;
	}
}
