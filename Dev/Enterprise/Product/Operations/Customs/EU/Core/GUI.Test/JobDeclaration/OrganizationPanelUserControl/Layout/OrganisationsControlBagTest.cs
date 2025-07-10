using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(OrganisationsControlBag))]
	sealed class OrganisationsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(OrganisationsControlBag.DefermentPartyDocAddressControl);
				yield return nameof(OrganisationsControlBag.ExporterDocAddressControl);
				yield return nameof(OrganisationsControlBag.ContractualPartnerDocAddressControl);
				yield return nameof(OrganisationsControlBag.CarrierEUBorderDocAddressControl);
				yield return nameof(OrganisationsControlBag.DutyPayerGuidFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => OrganisationsControlBag.Instance;
	}
}
