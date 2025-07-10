using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(OrganizationControlBag))]
	sealed class OrganizationControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(OrganizationControlBag.Instance.ForeignCarrierGuidFindBox);
				yield return nameof(OrganizationControlBag.Instance.DomesticCarrierGuidFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => OrganizationControlBag.Instance;
	}
}
