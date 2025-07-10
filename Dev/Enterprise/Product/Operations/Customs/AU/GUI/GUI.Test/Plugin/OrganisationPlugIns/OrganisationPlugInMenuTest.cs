using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI.Testing
{
	sealed class OrganisationPlugInMenuTest : TestCaseWithFactory
	{
		public void TestSendClientRegistrationRequest()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var organisation = new OrgHeaderWrapper(orgHeader);
			using (var plugIn = new OrganisationPlugIn(organisation.OrgHeader))
			using (MasterFiles.GUI.BaseOrganisationsForm form = new MasterFiles.GUI.BaseOrganisationsForm(organisation.OrgHeader))
			{
				var mainMenu = (OrganisationPlugInMenu)plugIn.TopLevelMenu;
				using (var menu = new OrganisationPlugInMenu(organisation, mainMenu.licenceLogIn))
				{
					menu.sendMessageOverrideFortesting = true;
					form.Menu = new MainMenu(new MenuItem[] { menu });
					orgHeader.CustomsCodes.RemoveAndDeleteAll();
					menu.clientRegistrationRequest.PerformClick();
					AssertEquals(OrganisationPlugInMenu.SaveOrganisationAdvice, UnitTestUserNotification.Instance.LastMessage.Text);
					Factory.Save();
					Assert(!Env.Licence.ImportBroker.IsLoggedIn);
					menu.clientRegistrationRequest.PerformClick();
					Assert(Env.Licence.ImportBroker.IsLoggedIn);
					AssertEquals("Errors in Organisation", false, organisation.HasErrors);
					AssertEquals(typeof(ClientRegistrationRequestForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				}
			}
		}
	}
}
