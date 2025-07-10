using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(OrganisationsControlBag))]
	sealed class OrganisationsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(OrganisationsControlBag.SupplierAddressControl);
				yield return nameof(OrganisationsControlBag.ImporterAddressControl);
				yield return nameof(OrganisationsControlBag.PayerGuidFindBox);
				yield return nameof(OrganisationsControlBag.IndustrialParkCodeCodeFindBox);
				yield return nameof(OrganisationsControlBag.ExpoterAddressControl);
				yield return nameof(OrganisationsControlBag.FinalBondedWarehouseCodeFindBox);
				yield return nameof(OrganisationsControlBag.ManufacturerGuidFindBox);
				yield return nameof(OrganisationsControlBag.ExporterGuidFindBox);
				yield return nameof(OrganisationsControlBag.StevedoreAddressControl);
				yield return nameof(OrganisationsControlBag.AuthorGroupBox);
				yield return nameof(OrganisationsControlBag.ResponsiblePersonGroupBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => OrganisationsControlBag.Instance;
	}
}
