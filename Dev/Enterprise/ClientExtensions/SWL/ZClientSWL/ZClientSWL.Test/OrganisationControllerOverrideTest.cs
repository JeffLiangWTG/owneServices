using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.SWL.Testing
{
	[TestedType(typeof(SWLOrganisationControllerOverrideForTest))]
	public class OrganisationControllerOverrideTest : OrganisationControllerTest
	{
		public void TestGetForm()
		{
			var controller = new SWLOrganisationControllerOverrideForTest();
			var carrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrg.OH_IsShippingProvider = true;
			using (var form = controller.GetForm(carrierOrg))
			{
				AssertEquals("Type of controller's form", typeof(SWLOrganisationForm), form.GetType());
			}
		}

		public class SWLOrganisationControllerOverrideForTest : SWLOrganisationControllerOverride
		{
			public new IZForm GetForm(IBusiness businessEntity)
			{
				return base.GetForm(businessEntity);
			}
		}
	}
}
