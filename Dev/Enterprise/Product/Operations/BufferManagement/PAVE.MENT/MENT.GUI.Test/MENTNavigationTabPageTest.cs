using Enterprise.PAVE.MENT.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.PAVE.MENT.GUI.Test
{
	class MENTNavigationTabPageTest : ZTabPageControlTest
	{
		ZTabPage UnrelatedTabPage
		{
			get { return unrelatedTabPage ?? (unrelatedTabPage = new ZTabPage()); }
		}
		ZTabPage unrelatedTabPage;

		public void TestNavigateToTabItem()
		{
			var navigationTabPage = new MENTNavigationTabPage();
			navigationTabPage.Initialize_ForTest(Dummy);
			TestTabControl.TabPages.Add(UnrelatedTabPage);
			TestTabControl.TabPages.Add(navigationTabPage);
			Form.Controls.Add(TestTabControl);
			TestTabControl.SelectedTab = UnrelatedTabPage;
			Form.Show();

			var extractionNew = Factory.NewWithValidTestData<MENTAgedScoreExtraction>();
			Dummy.Extractions.Add(extractionNew);
			Factory.Save();

			navigationTabPage.NavigateToItem(extractionNew);
			Assert(TestTabControl.SelectedTab == navigationTabPage);
		}

		new MENTAgedScoreQuery Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.NewWithValidTestData<MENTAgedScoreQuery>();
				}
				return dummy;
			}
		}
		MENTAgedScoreQuery dummy;

		ZChildForm Form
		{
			get
			{
				if (form == null)
				{
					form = new ZChildForm(Dummy);
				}
				return form;
			}
		}
		ZChildForm form;

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}
	}
}
