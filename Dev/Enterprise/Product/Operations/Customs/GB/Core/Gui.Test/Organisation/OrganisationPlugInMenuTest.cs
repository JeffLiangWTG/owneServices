using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.GUI.Organisation;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Testing
{
	[TestedType(typeof(OrganisationPlugInMenu))]
	sealed class OrganisationPlugInMenuTest : TestCaseWithFactory
	{
		public void TestShowHmrcQueryForm()
		{
			var organisation = Factory.New<OrgHeader>();
			using var plugIn = new OrganisationPlugIn(organisation);
			using var menu = new OrganisationPlugInMenu(organisation, plugIn);
			var showHmrcQueryFormMenuItem = menu.MenuItems[0];
			showHmrcQueryFormMenuItem.PerformClick();

			AssertEquals("Customs Messaging", menu.Text);
			AssertEquals("Verify GB EORI, UK VAT number or XI NOP Waiver", showHmrcQueryFormMenuItem.Text);
			AssertType<HmrcQueryForm>(ZFormModaliser.LastFormShownDialogForTest);
		}
	}
}
