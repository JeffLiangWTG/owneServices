using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module.Testing
{
	[TestedType(typeof(LicenceKeyController))]
	public class LicenceKeyControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return Modules.ClientControllerRegistration.LicenseKey;
		}

		public void TestGetForm()
		{
			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			LicenceDatabase database1 = org.LicCompany.LicDatabases.AddNew();
			database1.FillWithValidTestData();
			database1.LD_HostedLocation = "SYD";
			database1.LicEnterprise.LE_EnterpriseCode = "ABC";
			Factory.Save();
			var licHeader = org.LicCompany.GetHeader(database1);
			ZController controller = ZControllerFactory.Create(ClientControllerRegistration.LicenseKey);
			using (EDIOrganisationForm form = (EDIOrganisationForm)controller.ShowEditForm(licHeader))
			{
				form.Show();
				AssertEquals(typeof(EDIOrganisationForm), controller.LastShownForm.GetType());
				AssertEquals("LicenceKeyBuilderTabPage", form.OrganisationsTabControl.SelectedTab.Name);
				AssertEquals(database1.PK, form.BuilderTabPage.SelectedLicenceDatabase.PK);
			}
		}
	}
}
