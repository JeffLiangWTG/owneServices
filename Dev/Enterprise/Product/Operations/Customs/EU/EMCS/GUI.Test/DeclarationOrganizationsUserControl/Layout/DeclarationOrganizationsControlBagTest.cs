using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	[TestedType(typeof(DeclarationOrganizationsControlBag))]
	sealed class DeclarationOrganizationsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(DeclarationOrganizationsUserControl.ConsigneeDocAddressControl);
				yield return nameof(DeclarationOrganizationsUserControl.ConsignorDocAddressControl);
				yield return nameof(DeclarationOrganizationsUserControl.OwnerDocAddressUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => DeclarationOrganizationsControlBag.Instance;
	}
}
