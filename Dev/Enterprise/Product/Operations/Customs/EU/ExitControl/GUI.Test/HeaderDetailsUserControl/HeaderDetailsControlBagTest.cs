using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	[TestedType(typeof(HeaderDetailsControlBag))]
	sealed class HeaderDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(HeaderDetailsControlBag.BranchGuidFindBox);
				yield return nameof(HeaderDetailsControlBag.BrokerCodeFindBox);
				yield return nameof(HeaderDetailsControlBag.ExporterOrgAddressControl);
				yield return nameof(HeaderDetailsControlBag.CarrierAddressWithContactControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => HeaderDetailsControlBag.Instance;
	}
}
