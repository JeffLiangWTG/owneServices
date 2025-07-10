using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module.Test
{
	public class LicenceKeyBuilderTabPageTest : TestCaseWithFactory
	{
		public void TestTabPageContainsControl()
		{
			EDIOrgHeader newOrg = Factory.NewWithValidTestData<EDIOrgHeader>();
			newOrg.OH_Code = "IAMTSTORG";
			newOrg.OH_FullName = "I'm a test organisation";

			using (TestForm form = new TestForm(newOrg))
			{
				form.Show();
				form.Page.AddLicenceControl();
				AssertNotNull("Control Created", form.Page.LicenceControl);
				form.SelectEmptyTabPage();
				AssertEquals("Should not be shown", false, form.Page.LicenceControl.Visible);
				Assert("Control docked to entire tabpage", form.Page.LicenceControl.Dock == DockStyle.Fill);

				Factory.Save();
				Assert(newOrg.IsInDatabase);
				form.SelectLicenceTabPage();
				form.Page.HideCoveringLabel();
				AssertEquals("Should be shown", true, form.Page.LicenceControl.Visible);
				form.SelectEmptyTabPage();
				AssertEquals("Should not be shown", false, form.Page.LicenceControl.Visible);
			}
		}

		class TestForm : ZForm
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1104:DoNotUseSystemWindowsTabControl", Justification = "Testing")]
			public TestForm(EDIOrgHeader org)
			{
				EmptyPage = new TabPage();
				Page = new LicenceKeyBuilderTabPage(org);
				OrgTabControl = new TabControl();
				Controls.Add(OrgTabControl);
				OrgTabControl.TabPages.Add(EmptyPage);
				OrgTabControl.TabPages.Add(Page);
			}

			public void SelectEmptyTabPage()
			{
				OrgTabControl.SelectedTab = EmptyPage;
			}

			public void SelectLicenceTabPage()
			{
				OrgTabControl.SelectedTab = Page;
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				EmptyPage.Dispose();
				Page.Dispose();
			}

			public LicenceKeyBuilderTabPage Page;
			readonly TabPage EmptyPage;
			readonly TabControl OrgTabControl;
		}
	}
}
