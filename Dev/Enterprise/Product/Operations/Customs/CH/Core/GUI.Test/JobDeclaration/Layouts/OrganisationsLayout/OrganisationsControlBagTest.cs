using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(OrganisationsControlBag))]
class OrganisationsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(OrganisationsControlBag.ConsignorDocAddressControl);
		}
	}

	protected override ControlBag GetControlBagForTesting() => OrganisationsControlBag.Instance;
}
