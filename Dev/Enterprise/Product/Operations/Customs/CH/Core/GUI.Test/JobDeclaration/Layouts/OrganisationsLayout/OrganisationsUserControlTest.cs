using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.GUI.Testing;

class OrganisationsUserControlTest : TestCaseWithFactory
{
	public void TestBindings()
	{
		using (var control = new OrganisationsUserControl())
		{
			AssertEquals("ConsignorDcoAddressControl.BindTo", nameof(JobDeclaration.ConsignorDocAddress), control.ConsignorDocAddressControl.BindTo);
			AssertEquals("ConsignorDcoAddressControl.BindToOrganisations", $"{nameof(JobDeclaration.Lookups)}+{nameof(JobDeclarationLookups.ConsignorList)}", control.ConsignorDocAddressControl.BindToOrganisations);
		}
	}
}
