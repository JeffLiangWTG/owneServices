using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(OrganisationsControlBag))]
	sealed class OrganisationsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(OrganisationsControlBag.DeclarationConsignorAddressControl);
				yield return nameof(OrganisationsControlBag.DeclarationConsigneeAddressControl);
				yield return nameof(OrganisationsControlBag.CarrierGroupBox);
				yield return nameof(OrganisationsControlBag.AttorneyForCustomsProcedureGroupBox);
				yield return nameof(OrganisationsControlBag.InspectionWitnessGroupBox);
				yield return nameof(OrganisationsControlBag.ExternalBrokerGroupBox);
				yield return nameof(OrganisationsControlBag.AirCargoAgentGroupBox);
				yield return nameof(OrganisationsControlBag.ForwarderGroupBox);
			}
		}

		protected override ControlBag GetControlBagForTesting()
		{
			return OrganisationsControlBag.Instance;
		}
	}
}
