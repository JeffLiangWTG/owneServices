using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.GUI.Testing
{
	[TestedType(typeof(OrganisationDetailsControlBag))]
	sealed class OrganisationDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(OrganisationDetailsControlBag.ConsigneeOrganisationControl);
				yield return nameof(OrganisationDetailsControlBag.SupplierOrganisationControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => OrganisationDetailsControlBag.Instance;
	}
}
