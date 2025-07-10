using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.GUI.Testing
{
	[TestedType(typeof(DeclarationOrganizationControlBag))]
	sealed class DeclarationOrganizationsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(DeclarationOrganizationControlBag.CertificateIdentifierGroupBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => DeclarationOrganizationControlBag.Instance;
	}
}
