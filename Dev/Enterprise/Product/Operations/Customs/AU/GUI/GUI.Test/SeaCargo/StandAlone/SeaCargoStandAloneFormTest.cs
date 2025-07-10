using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	[TestedType(typeof(SeaCargoStandAloneForm))]
	sealed class SeaCargoStandAloneFormTest : ZFormBasherTest
	{
		public void TestSeaCargoMenusShown()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			using (SeaCargoStandAloneForm testForm = new SeaCargoStandAloneForm(oceanBill))
			{
				Assert("Menu should be displayed", ((IFileMenuItemsProvider)testForm).MainMenu.MenuItems.Contains(testForm.seaCargoMenu));
				AssertEquals("Menu should be in position 3", 3, testForm.seaCargoMenu.Index);
			}
		}

		public void TestPassAHouseBillPropertyIn()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			CusSCAHouse houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_CB = oceanBill.PK;
			using (SeaCargoStandAloneForm testForm = new SeaCargoStandAloneForm(houseBill.OceanBill))
			{
				Assert("Menu should be displayed", ((IFileMenuItemsProvider)testForm).MainMenu.MenuItems.Contains(testForm.seaCargoMenu));
			}
		}

		public void TestCusUnderbondTabShown()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			using (SeaCargoStandAloneForm testForm = new SeaCargoStandAloneForm(oceanBill))
			{
				testForm.Show();
				AssertEquals("Underbond tab should have been displayed", 4, testForm.OceanBillControl.OceanBillSpecificsTabControl.TabCount);
			}
		}

		public void TestMessageControlBindPrepend()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			using (SeaCargoStandAloneForm testForm = new SeaCargoStandAloneForm(oceanBill))
			{
				testForm.Show();
				AssertEquals("bind prepend should be FilteredHouseBills.", true, testForm.OceanBillControl.MessageUserControl.MessagesGrid.BindTo.StartsWith("FilteredHouseBills."));
			}
		}

		public void TestChangeVisibility()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			_ = oceanBill.HouseBills.AddNew();
			using (var testForm = new SeaCargoStandAloneForm(oceanBill))
			{
				testForm.Show();
				AssertVisibilityForNotIsOceanBillUnpack(testForm);
				oceanBill.CB_MultiOBLUnpack = true;
				AssertVisibilityForIsOceanBillUnpack(testForm);
				oceanBill.CB_MultiOBLUnpack = false;
				AssertVisibilityForNotIsOceanBillUnpack(testForm);
			}

			oceanBill.CB_MultiOBLUnpack = true;
			using (var testForm = new SeaCargoStandAloneForm(oceanBill))
			{
				testForm.Show();
				AssertVisibilityForIsOceanBillUnpack(testForm);
			}
		}

		public void TestChangeVisibilityCalledOnBind()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			using (TestHelperSeaCargoStandAloneForm form = new TestHelperSeaCargoStandAloneForm(oceanBill))
			{
				form.Show();
				Assert("ChangeVisibilityCalled", form.ChangeVisibilityCallCount > 0);
			}
		}

		public void TestChangeVisibilityCalledOnCB_MultiOBLUnpackChange()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			using (TestHelperSeaCargoStandAloneForm form = new TestHelperSeaCargoStandAloneForm(oceanBill))
			{
				form.Show();
				int preCount = form.ChangeVisibilityCallCount;
				oceanBill.CB_MultiOBLUnpack = !oceanBill.CB_MultiOBLUnpack;
				AssertEquals("NewChangeVisibilityCallCount", 1, form.ChangeVisibilityCallCount - preCount);
			}
		}

		public void TestOceanBill()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			using (TestHelperSeaCargoStandAloneForm form = new TestHelperSeaCargoStandAloneForm(oceanBill))
			{
				AssertEquals(oceanBill, form.OceanBill);
			}
		}

		public void TestGetManager()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			using (TestHelperSeaCargoStandAloneForm form = new TestHelperSeaCargoStandAloneForm(oceanBill))
			{
				AssertEquals(typeof(CusSCAOceanBillMessageManager), form.Manager.GetType());
			}
		}

		public void TestGetMessageingMenu()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			using (TestHelperSeaCargoStandAloneForm form = new TestHelperSeaCargoStandAloneForm(oceanBill))
			using (MenuItem menu = form.GetMessagingMenuInternal())
			{
				AssertEquals(typeof(SeaCargoStandAloneMenuWithScan), menu.GetType());
			}
		}

		public void TestDropEditVisiblilityStandAlone()
		{
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			using (SeaCargoStandAloneForm form = new SeaCargoStandAloneForm(oceanBill))
			{
				form.Show();
				AssertEquals("CB_ApplicationCodeBoundDropEdit Should not be visible", false, form.OceanBillControl.FindSingle<Control>("CB_ApplicationCodeBoundDropEdit").Visible);
				AssertEquals("MessagingModeLabel Should not be visible", false, form.OceanBillControl.FindSingle<Control>("MessagingModeLabel").Visible);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			return new SeaCargoStandAloneForm(oceanBill)
			{ ControllerID = ControllerIDs.Customs.AU.SeaCargo };
		}

		void AssertVisibilityForIsOceanBillUnpack(SeaCargoStandAloneForm form)
		{
			AssertEquals("HouseBillsTabPage.Text", "Ocean Bills", form.OceanBillControl.HouseBillsTabPage.Text);
			AssertEquals("OceanBillLabel.Visible", false, form.FindSingle<ZLabel>("OceanBillLabel").Visible);
			AssertEquals("CusSCAOceanBillParentBillLabel.Visible", false, form.FindSingle<ZLabel>("CusSCAOceanBillParentBillLabel").Visible);
			AssertEquals("CB_OceanBillBoundTextBox.Visible", false, form.FindSingle<ZTextBox>("CB_OceanBillBoundTextBox").Visible);
			AssertEquals("CusSCAOceanBillParentBillTextBox.Visible", false, form.FindSingle<ZTextBox>("CusSCAOceanBillParentBillTextBox").Visible);
			form.OceanBillControl.OceanBillSpecificsTabControl.SelectedTab = form.OceanBillControl.HouseBillsTabPage;
			AssertEquals("GroupBoxHouseBill.Text", "Ocean Bill", form.OceanBillControl.FindSingle<Control>("GroupBoxHouseBill").Text);
			AssertEquals("HouseBillLabel.Text", "Ocean Bill:", form.OceanBillControl.FindSingle<Control>("HouseBillLabel").Text);
			AssertEquals("CA_HouseBill Column Text", "Ocean Bill", form.OceanBillControl.HouseBillsGrid.Columns[Customs.Business.AutoCusSCAHouse.Schema.CA_HouseBill].ColumnStyle.HeaderText);
			form.OceanBillControl.HouseBillTabControl.SelectedTab = form.OceanBillControl.PackingTabPage;
			AssertEquals("CV_AssociatedHouse Column Text", "Associated Ocean Bill", form.OceanBillControl.PackingForContainersControl.PackingHouseGrid.Columns[CusSCAPivot.Schema.CV_AssociatedHouse].ColumnStyle.HeaderText);
		}

		void AssertVisibilityForNotIsOceanBillUnpack(SeaCargoStandAloneForm form)
		{
			AssertEquals("HouseBillsTabPage.Text", "House Bills", form.OceanBillControl.HouseBillsTabPage.Text);
			AssertEquals("OceanBillLabel.Visible", true, form.FindSingle<ZLabel>("OceanBillLabel").Visible);
			AssertEquals("CusSCAOceanBillParentBillLabel.Visible", true, form.FindSingle<ZLabel>("CusSCAOceanBillParentBillLabel").Visible);
			AssertEquals("CB_OceanBillBoundTextBox.Visible", true, form.FindSingle<ZTextBox>("CB_OceanBillBoundTextBox").Visible);
			AssertEquals("CusSCAOceanBillParentBillTextBox.Visible", true, form.FindSingle<ZTextBox>("CusSCAOceanBillParentBillTextBox").Visible);
			form.OceanBillControl.OceanBillSpecificsTabControl.SelectedTab = form.OceanBillControl.HouseBillsTabPage;
			AssertEquals("GroupBoxHouseBill.Text", "House Bill", form.OceanBillControl.FindSingle<Control>("GroupBoxHouseBill").Text);
			AssertEquals("HouseBillLabel.Text", "House Bill:", form.OceanBillControl.FindSingle<Control>("HouseBillLabel").Text);
			AssertEquals("CA_HouseBill Column Text", "House Bill", form.OceanBillControl.HouseBillsGrid.Columns[Customs.Business.AutoCusSCAHouse.Schema.CA_HouseBill].ColumnStyle.HeaderText);
			form.OceanBillControl.HouseBillTabControl.SelectedTab = form.OceanBillControl.PackingTabPage;
			AssertEquals("CV_AssociatedHouse Column Text", "Associated House Bill", form.OceanBillControl.PackingForContainersControl.PackingHouseGrid.Columns[CusSCAPivot.Schema.CV_AssociatedHouse].ColumnStyle.HeaderText);
		}

		sealed class TestHelperSeaCargoStandAloneForm : SeaCargoStandAloneForm
		{
			public TestHelperSeaCargoStandAloneForm(CusSCAOceanBill oceanBill) : base(oceanBill)
			{
				OceanBillControl.VisibilityChanged += new EventHandler(ChangeVisibility);
			}

			internal MenuItem GetMessagingMenuInternal() => GetMessagingMenu();

			internal int ChangeVisibilityCallCount;

			void ChangeVisibility(object sender, EventArgs e)
			{
				ChangeVisibilityCallCount++;
			}
		}
	}
}
