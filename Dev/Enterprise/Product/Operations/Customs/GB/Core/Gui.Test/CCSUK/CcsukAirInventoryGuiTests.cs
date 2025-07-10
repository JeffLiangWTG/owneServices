using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Ccsuk.Testing
{
	[TestedType(typeof(C1ReleaseForm))]
	class C1ReleaseFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var basic = Factory.New<CusMAWB>();
			basic.NumberOfPiecesExpected = 10;
			basic.NumberOfPiecesReceived = 10;
			basic.SetCustomsActionCode(CustomsStatusCodes.Codes.ReleasedForInterAirportRemoval, ZDateTime.BrettsBirthday);
			return new C1ReleaseForm(basic);
		}
	}

	[TestedType(typeof(ErtsReleaseForm))]
	class ErtsReleaseFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var basic = Factory.New<CusMAWB>();
			basic.NumberOfPiecesExpected = 10;
			basic.NumberOfPiecesReceived = 10;
			basic.SetCustomsActionCode(CustomsStatusCodes.Codes.ReleasedForInterAirportRemoval, ZDateTime.BrettsBirthday);
			return new ErtsReleaseForm(basic);
		}

		public void TestClickPartialReleaseLinkShowsHelpfulMessage()
		{
			using (var form = (ZForm)GetFormToBashCore())
			{
				var link = (IButtonControl)form.Controls.Find("partialReleaseHelpLink", true)[0];
				link.PerformClick();
				AssertContains("If you want to do a partial release", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}

	[TestedType(typeof(SplitConsignmentForm))]
	class SplitConsignmentFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var mawb = Factory.New<CusMAWB>();
			var house = mawb.ChildBills.AddNew();
			var splitHouse = house.Splits.AddNew();
			Factory.Save();
			var f = new SplitConsignmentForm(splitHouse);
			return f;
		}

		public void TestCaptionBasic()
		{
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "001-12345678";
			var split = basic.Splits.AddNew();
			split.SplitReference = "69";
			split.NumberOfPiecesExpected = 100;
			using (var f = new SplitConsignmentForm(split))
			{
				AssertContains("Split Basic 001-12345678/69 (100 pieces)", f.FormCaption);
			}
		}

		public void TestCaptionHouse()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "001-12345678";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "87654321";
			var split = hawb.Splits.AddNew();
			split.SplitReference = "69";
			split.NumberOfPiecesExpected = 1;
			using (var f = new SplitConsignmentForm(split))
			{
				AssertContains("Split House 001-12345678-87654321/69 (1 piece)", f.FormCaption);
			}
		}
	}

	[TestedType(typeof(CcsukAirInventoryUFOForm))]
	class CcsukAirInventoryUFOFormBasher : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.InitialiseUFO();
			Factory.Save(); //BOGUS
			var result = new CcsukAirInventoryUFOForm(cusMawb);
			result.ControllerID = ControllerIDs.Customs.GB.CcsukAirInventoryUFO;
			return result;
		}
	}

	[TestedType(typeof(CcsukAirInventoryForm))]
	class CcsukAirInventoryGuiMawbBasher : ZFormBasherTest
	{
		public void TestCcsukMenuIsInRightPosition()
		{
			using (var form = GetFormToBashCore())
			{
				RunCcsukMenuIsInRightPositionTest(form);
			}
		}

		internal static void RunCcsukMenuIsInRightPositionTest(Form form)
		{
			form.Show();
			var menu = form.Menu.MenuItems;
			var actions = menu.FindByText("Actio&ns");
			var ccsuk = menu.FindByText(CcsukMenu.MenuCaption);
			var document = menu.FindByText("&Documents");
			Assert(menu.IndexOf(actions) < menu.IndexOf(ccsuk));
			Assert(menu.IndexOf(ccsuk) < menu.IndexOf(document));
		}

		public void TestTabVisibility()
		{
			var basic = Factory.New<CusMAWB>();

			var housesTabPageName = "HousesTabPage";
			var splitsTabPageName = "SplitsTabPage";
			var deliveryTabPageName = "DeliveryTabPage";
			basic.Profile = "CUKAIR98LHRBAC";
			using (var mawbForm = new CcsukAirInventoryForm(basic))
			{
				mawbForm.Show();
				AssertEquals("Delivery tab invisible, even for shed", 0, mawbForm.Controls.Find(deliveryTabPageName, true).Length);
			}
			Factory.Save();
			using (var mawbForm = new CcsukAirInventoryForm(basic))
			{
				mawbForm.Show();
				var deliveryTab = (ZTabPage)mawbForm.Controls.Find(deliveryTabPageName, true)[0];
				Assert("Basic is in DB and is a shed record, tab should be visible", deliveryTab.TabVisible);
				basic.Profile = "CUKFFW98000ABC";
				Assert("Tab hidden for agent even without close and reopen", !deliveryTab.TabVisible);
				basic.Profile = "CUKAIR98LHRBAC";
				Assert("Tab show for shed even without close and reopen", deliveryTab.TabVisible);
			}

			using (var mawbForm = new CcsukAirInventoryForm(basic))
			{
				mawbForm.Show();
				var housesTab = (ZTabPage)mawbForm.Controls.Find(housesTabPageName, true)[0];
				Assert(housesTab.TabVisible);
				AssertEquals(0, mawbForm.Controls.Find(splitsTabPageName, true).Length);
			}
			var house = basic.ChildBills.AddNew();
			Factory.Save();
			using (var mawbForm = new CcsukAirInventoryForm(basic))
			{
				mawbForm.Show();
				var housesTab = (ZTabPage)mawbForm.Controls.Find(housesTabPageName, true)[0];
				Assert(housesTab.TabVisible);
				AssertEquals(0, mawbForm.Controls.Find(splitsTabPageName, true).Length);
			}
			basic.ChildBills.RemoveAndDelete(house);
			var split = basic.Splits.AddNew();
			Factory.Save();
			using (var mawbForm = new CcsukAirInventoryForm(basic))
			{
				mawbForm.Show();
				var splitsTab = (ZTabPage)mawbForm.Controls.Find(splitsTabPageName, true)[0];
				Assert(splitsTab.TabVisible);
				AssertEquals(0, mawbForm.Controls.Find(housesTabPageName, true).Length);
				basic.Splits.RemoveAndDeleteAll();
				Assert("Splits tab hidden even without close and reopen", !splitsTab.TabVisible);
				AssertEquals("Houses tab shown even without close and reopen", 1, mawbForm.Controls.Find(housesTabPageName, true).Length);
				split = basic.Splits.AddNew();
				Assert("Splits tab shown even without close and reopen", splitsTab.TabVisible);
				AssertEquals("Houses tab hidden even without close and reopen", 0, mawbForm.Controls.Find(housesTabPageName, true).Length);
			}
			basic.Splits.RemoveAndDelete(split);
			basic.SetCustomsActionCode(CustomsStatusCodes.Codes.ClearedByCustoms, ZDateTime.BrettsBirthday);
			Factory.Save();
			using (var mawbForm = new CcsukAirInventoryForm(basic))
			{
				mawbForm.Show();
				AssertEquals(0, mawbForm.Controls.Find(housesTabPageName, true).Length);
				AssertEquals(0, mawbForm.Controls.Find(splitsTabPageName, true).Length);
			}
		}

		public void TestRemovalsTabsVisibility()
		{
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKAIR98LHRABC";
			Factory.Save();
			using (var mawbForm = new CcsukAirInventoryForm(basic))
			{
				RunRemovalsTabsVisibilityTest(basic, mawbForm, delegate (string p)
				{ basic.Profile = p; });
			}
		}

		internal static void RunRemovalsTabsVisibilityTest(ICcsukCusAwb awb, ZForm form, Action<string> setProfile)
		{
			form.Show();
			AssertEquals(1, form.Controls.Find("IsrsTabsPage", true).Length);
			AssertEquals(0, form.Controls.Find("TsrTabPage", true).Length);
			AssertEquals(0, form.Controls.Find("IarTabPage", true).Length);
			AssertEquals(0, form.Controls.Find("FallbackTabPage", true).Length);
			setProfile("CUKFFW98000ABC");
			AssertEquals(0, form.Controls.Find("IsrsTabsPage", true).Length);
			AssertEquals(1, form.Controls.Find("TsrTabPage", true).Length);
			AssertEquals(1, form.Controls.Find("IarTabPage", true).Length);
			AssertEquals(1, form.Controls.Find("FallbackTabPage", true).Length);
		}

		protected override Form GetFormToBashCore()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.CM_MAWB = "001-12345678";
			Factory.Save(); // to make the following test shut up: "On the Top Level Object Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB.HasChanges was set to true when the form: CcsukAirInventoryForm with Caption: New CCSUK Air Inventory loaded."	
			CcsukAirInventoryForm result = new CcsukAirInventoryForm(cusMawb);
			result.ControllerID = ControllerIDs.Customs.GB.CcsukAirInventory;
			return result;
		}

		public void TestCaption()
		{
			using (var form = (ZForm)GetFormToBashCore())
			{
				AssertContains("001-12345678", form.FormCaption);
			}
		}

		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.UnitedKingdom; }
		}

		public void TestEdocsAndDocuments()
		{
			using (var form = (ZTemplateForm)GetFormToBashCore())
			{
				var foundEDocsPlugIn = false;
				foreach (var plugIn in form.PlugIns.Instances)
				{
					if (plugIn.GetType().FullName == "Enterprise.DocumentScanning.PlugIn.eDocsPlugIn")
					{
						foundEDocsPlugIn = true;
						break;
					}
				}
				AssertEquals("Should support edocs", true, foundEDocsPlugIn);
			}
		}

		public void TestBusinessEntityPassedThroughToBaseToHelpPowerEdocsTab()
		{
			using (var form = (ZTemplateForm)GetFormToBashCore())
			{
				AssertNotNull(form.BusinessEntity);
				AssertType(typeof(CusMAWB), form.BusinessEntity);
			}
		}

		public void TestEdifactUNOATextBoxValidation()
		{
			using (var testForm = (ZForm)GetFormToBashCore())
			{
				var control = testForm.Controls.Find("DescriptionOfGoodsTextBox", true)[0];
				var descriptionOfGoods = (EdifactUNOATextBox)control;

				testForm.Show();

				StmNoteUserInfoInserter.RegisterHotkeys(descriptionOfGoods);
				AssertEquals("Prereq - Initial Text", "", descriptionOfGoods.Text);
				KeySender.PostKeyDown(descriptionOfGoods, Keys.OemOpenBrackets);
				Application.DoEvents();
				AssertEquals("No text after invalid open square brace character keypress", "", descriptionOfGoods.Text);
				KeySender.PostKeyDown(descriptionOfGoods, Keys.Oemcomma);
				Application.DoEvents();
				AssertEquals("Text after comma character keypress", ",", descriptionOfGoods.Text);
			}
		}

		public void TestPressingTheButtonActuallyGeneratesAMessage_FRI()
		{
			ShedTest.CreateShed(Factory, "GB", "MANSLS", "SERVISAIR UK LTD. at Manchester", acpCode: "M", portName: "Manchester");
			Factory.Save();

			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("ZPE");
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			mawb.AirportOfDestination = "MAN";
			mawb.AirportOfArrival = "MAN";
			mawb.CargoTerminalOperatorAirport = "MAN";
			mawb.CargoTerminalOperator = "SLS";
			mawb.AgentBadge = "ZPE";
			mawb.NumberOfPiecesReceived = 0;
			mawb.CM_ArrivalDate = ZDateTime.Empty;
			ShutFirstHawbValidationUp(mawb);
			using (var form = new CcsukAirInventoryFormWithExposedMenuForTest(mawb))
			{
				MenuItem fri = GetMenuItemUnderCcsukMenu(form, "send FRI", false);
				AssertNotNull("Looking for menu CCSUK Messaging>send FRI", fri);
				AssertEquals("Pre req", 0, mawb.Messages.Count);
				fri.PerformClick();
				AssertEquals("Message created when click the menu item. The properties of the message are not relevant to this test, see  CcsukInventoryBusinessObjectMessageSenderTests. If this test fails, check for validation errors, especially if you have edited the ports/sheds/agents standing data.", 1, mawb.Messages.Count);
			}
		}

		public void TestPressingTheFriButtonActuallyGeneratesAMessage_FSR()
		{
			ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow", acpCode: "H", portName: "Heathrow");
			Factory.Save();

			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("ZPE");
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			mawb.CargoTerminalOperator = "BAC";
			mawb.CM_ArrivalDate = ZDateTime.Empty;
			mawb.NumberOfPiecesReceived = 0;
			mawb.AgentBadge = "ZPE";
			ShutFirstHawbValidationUp(mawb);
			using (var form = new CcsukAirInventoryFormWithExposedMenuForTest(mawb))
			{
				MenuItem fsr = GetMenuItemUnderCcsukMenu(form, "send FSR", false);
				MenuItem fsrShed = FindInSubMenu("To shed", fsr, false);
				MenuItem fsrCommDb = FindInSubMenu("To community database", fsr, false);
				AssertEquals("Pre req", 0, mawb.Messages.Count);
				fsrShed.PerformClick();
				AssertEquals("Message created when click the menu item. The properties of the message are not relevant to this test, see  CcsukInventoryBusinessObjectMessageSenderTests. If this test fails, check for validation errors, especially if you have edited the ports/sheds/agents standing data.", 1, mawb.Messages.Count);
				fsrCommDb.PerformClick();
				AssertEquals("Message created when click the menu item. The properties of the message are not relevant to this test, see  CcsukInventoryBusinessObjectMessageSenderTests", 2, mawb.Messages.Count);
			}
		}

		public void TestHousesGridContextMenu()
		{
			Env.Security.AirCcsukHouse.IsAllowed = true;
			var mawb = CUSCAR9122GeneratorTests.CreateMawbForTest(true, Factory);
			var hawb1 = mawb.ChildBills[0];
			hawb1.CargoTerminalOperator = "ONE";
			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CargoTerminalOperator = "TWO";
			Factory.Save();
			using (var mawbForm = new CcsukAirInventoryForm(mawb))
			{
				mawbForm.Show();
				var housesTab = (ZTabPage)mawbForm.Controls.Find("HousesTabPage", true)[0];
				var grid = (ZGrid)(housesTab.Controls.Find("ChildrenGrid", true)[0]);
				var ccsukMenu = (CcsukMenu)(grid.ContextMenu.MenuItems[0]);
				ccsukMenu.RefreshMenu();
				AssertContains("CCSUK", ccsukMenu.Text);
				AssertContains("Please select exactly", ccsukMenu.MenuItems[0].Text);
				grid.Select(0);
				ccsukMenu.RefreshMenu();
				AssertContains("To shed ONE", ccsukMenu.MenuItems[1].MenuItems[1].Text);
				grid.Select(1);
				ccsukMenu.RefreshMenu();
				AssertContains("Please select exactly", ccsukMenu.MenuItems[0].Text);
				grid.UnSelect(0);
				ccsukMenu.RefreshMenu();
				AssertContains("To shed TWO", ccsukMenu.MenuItems[1].MenuItems[1].Text);
			}
			Env.Security.AirCcsukHouse.IsAllowed = false;
			using (var mawbForm = new CcsukAirInventoryForm(mawb))
			{
				mawbForm.Show();
				var housesTab = (ZTabPage)mawbForm.Controls.Find("HousesTabPage", true)[0];
				var grid = (ZGrid)(housesTab.Controls.Find("ChildrenGrid", true)[0]);
				var firstMenu = (grid.ContextMenu.MenuItems[0]);
				AssertNotContains("CCSUK", firstMenu.Text);
			}
		}

		public void TestSplitsGridContextMenu()
		{
			var basic = CUSCAR9122GeneratorTests.CreateMawbForTest(false, Factory);
			var splits = basic.Splits.AddNew();
			Factory.Save();
			using (var mawbForm = new CcsukAirInventoryForm(basic))
			{
				mawbForm.Show();
				var splitsControl = (SplitConsignmentGridUserControlBasic)mawbForm.Controls.Find("splitConsignmentUserControl1", true)[0];
				var grid = (SplitConsignmentModuleButtonGrid)(splitsControl.Controls.Find("zGrid1", true)[0]);
				grid.SelectFirstRowIfOnlyRowInGrid();
				var ccsukMenu = (CcsukMenu)(grid.InnerGrid.ContextMenu.MenuItems[0]);
				ccsukMenu.RefreshMenu();
				AssertContains("CCSUK", ccsukMenu.Text);
				AssertNotContains("save", ccsukMenu.MenuItems[0].Text);
				basic.NumberOfPiecesExpected = 7; // changes
				ccsukMenu.RefreshMenu();
				AssertContains("save", ccsukMenu.MenuItems[0].Text);
			}
		}

		static MenuItem GetMenuItemUnderCcsukMenu(CcsukAirInventoryFormWithExposedMenuForTest form, string captionToSeek, bool exact)
		{
			// There must be a better way....
			MenuItem ccsuk = null;
			foreach (MenuItem menu in form.MainMenuForTest.MenuItems)
			{
				if (menu.Text == "CCSUK Messaging")
				{
					ccsuk = menu;
					((CcsukMenu)ccsuk).RefreshMenu();
					break;
				}
			}
			return FindInSubMenu(captionToSeek, (CcsukMenu)ccsuk, exact);
		}

		static MenuItem FindInSubMenu(string captionToSeek, MenuItem subMenu, bool exact)
		{
			MenuItem result = null;
			foreach (MenuItem menu in subMenu.MenuItems)
			{
				if ((exact && menu.Text == captionToSeek) || (menu.Text.ToLower().Contains(captionToSeek.ToLower())))
				{
					result = menu;
					break;
				}
			}
			return result;
		}

		void ShutFirstHawbValidationUp(CusMAWB mawb)
		{
			var hawb = mawb.ChildBills[0];
			hawb.AgentBadge = "ZPE";
			hawb.CS_GoodsDescription = "x";
			hawb.CS_PiecesLanded = 87;
			hawb.CS_PiecesManifested = 87;
			hawb.CS_Weight = 89m;
			hawb.CargoTerminalOperator = mawb.CargoTerminalOperator;
			hawb.CargoTerminalOperatorAirport = mawb.CargoTerminalOperatorAirport;
		}

		class CcsukAirInventoryFormWithExposedMenuForTest : CcsukAirInventoryForm
		{
			public CcsukAirInventoryFormWithExposedMenuForTest(CusMAWB cusMAWB)
				: base(cusMAWB)
			{ }

			public MainMenu MainMenuForTest
			{
				get { return base.MainMenu; }
			}
		}
	}

	[TestedType(typeof(CcsukAirInventoryFormHouse))]
	class CcsukAirInventoryFormHouseBasher : ZFormBasherTest
	{
		public void TestReceitpsGridContextMenu()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.Profile = "CUKAIR98LHRBAC";
			var ot = hawb.OutTurns.AddNew();
			ot.C5_PackagesOutturned = 10;
			ot.ReadOnly = false;
			Factory.Save();
			using (var hawbForm = new CcsukAirInventoryFormHouse(hawb))
			{
				hawbForm.Show();
				var deliveryTab = (ZTabPage)hawbForm.Controls.Find("DeliveryTabPage", true)[0];
				var grid = (ZGrid)(deliveryTab.Controls.Find("zGrid1", true)[0]);
				deliveryTab.Show();
				var divideMenu = (ZMenuItem)(grid.ContextMenu.MenuItems[0]);
				AssertContains("Divide", divideMenu.Text);
				grid.Select(0);
				UnitTestUserNotification.Instance.AddUserResponse("1");
				divideMenu.PerformClick();
				AssertContains("Divide this receipt", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(2, hawb.OutTurns.Count);
				AssertEquals(9, ot.C5_PackagesOutturned);
			}
		}

		public void TestDeliveryAndSplitsTabVisibility()
		{
			var mawb = Factory.New<CusMAWB>();
			var house = mawb.ChildBills.AddNew();
			var splitsTabPageName = "SplitsTabPage";
			var deliveryTabPageName = "DeliveryTabPage";
			house.Profile = "CUKAIR98LHRBAC";
			using (var form = new CcsukAirInventoryFormHouse(house))
			{
				form.Show();
				AssertEquals("Delivery tab invisible, even for shed", 0, form.Controls.Find(deliveryTabPageName, true).Length);
			}
			Factory.Save();
			using (var form = new CcsukAirInventoryFormHouse(house))
			{
				form.Show();
				var deliveryTab = (ZTabPage)form.Controls.Find(deliveryTabPageName, true)[0];
				Assert("House is in DB and is a shed record, tab should be visible", deliveryTab.TabVisible);
				house.Profile = "CUKFFW98000AAA";
				Assert("Receipts tab is hidden for agent even without close and reopen", !deliveryTab.TabVisible);
				house.Profile = "CUKAIR98LHRBAC";
				Assert("Receipts tab is shown for shed even without close and reopen", deliveryTab.TabVisible);
			}

			var split = house.Splits.AddNew();
			Factory.Save();
			using (var form = new CcsukAirInventoryFormHouse(house))
			{
				form.Show();
				var splitsTab = (ZTabPage)form.Controls.Find(splitsTabPageName, true)[0];
				Assert(splitsTab.TabVisible);
				house.Splits.RemoveAndDeleteAll();
				Assert("Splits tab becomes invisible even without close-and-reopen, hurrah", !splitsTab.TabVisible);
				house.Splits.AddNew();
				Assert("Splits tab becomes visible even without close-and-reopen, hurrah", splitsTab.TabVisible);
			}
		}

		public void TestRemovalsTabsVisibility()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.Profile = "CUKAIR98LHRABC";
			Factory.Save();
			using (var hawbForm = new CcsukAirInventoryFormHouse(hawb))
			{
				CcsukAirInventoryGuiMawbBasher.RunRemovalsTabsVisibilityTest(hawb, hawbForm, delegate (string p)
				{ hawb.Profile = p; });
			}
		}

		public void TestCcsukMenuIsInRightPosition()
		{
			using (var form = GetFormToBashCore())
			{
				CcsukAirInventoryGuiMawbBasher.RunCcsukMenuIsInRightPositionTest(form);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return GetHawbAndForm(true);
		}

		public void TestCaption()
		{
			using (var form = GetHawbAndForm(false))
			{
				AssertContains("001-12345678-87654321", form.FormCaption);
			}
		}

		CcsukAirInventoryFormHouse GetHawbAndForm(bool linkToEntryToo)
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.Profile = "XXX";
			cusMawb.CM_MAWB = "001-12345678";
			var cusHawb = cusMawb.ChildBills.AddNew();
			cusHawb.CS_HAWB = "87654321";

			if (linkToEntryToo)
			{
				var declaration = Factory.New<JobDeclaration>();
				cusHawb.CS_JE_CustomsFormalEntry = declaration.PK;
				declaration.JE_EntryStatus = "DAN";
				declaration.JE_UCR = "1GB000000000000-B00000001";
			}
			Factory.Save();
			var result = new CcsukAirInventoryFormHouse(cusHawb);
			result.ControllerID = ControllerIDs.Customs.GB.CcsukAirInventoryHouse;
			return result;
		}

		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.UnitedKingdom; }
		}

		public void TestEntryLabel()
		{
			using (var form = GetHawbAndForm(true))
			{
				form.Show();
				var control = form.Controls.Find("LinkLabelEntry", true)[0];
				AssertEquals("Entry link label is visible", true, control.Visible);
				AssertEquals("Entry link label caption", "1GB000000000000-B00000001", control.Text);
			}
		}

		public void TestMessagesGridColumns()
		{
			using (var form = (ZTemplateForm)GetFormToBashCore())
			{
				var control = form.Controls.Find("ccsukMessagesUserControl1", true)[0];
				var grid = (control as CcsukMessagesUserControl).Controls.Find("zGrid1", true)[0] as ZGrid;
				foreach (ZGridColumnInfo column in grid.ColumnStyles)
				{
					if (column.ColumnName == GbEDIMessage.Schema.EM_SystemCreateUser)
					{
						Assert("EM _ SystemCreateUser was found", true);
						return;
					}
				}
				Assert("EM _ SystemCreateUser was not found", false);
			}
		}
	}

	[TestedType(typeof(NonPersistentSplitCreatorForm))]
	class SplitCreatorFormBasherTest : ZFormBasherTest
	{
		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.UnitedKingdom; }
		}

		protected override Form GetFormToBashCore()
		{
			var mawb = Factory.New<CusMAWB>();
			return new NonPersistentSplitCreatorForm(mawb);
		}

		public void TestButtonsEnabled()
		{
			ShedTest.CreateShed(Factory, "GB", "LHRBAC", "BRITISH AIRWAYS at Heathrow", acpCode: "H");
			Factory.Save();

			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKFFW98000LXA";
			using (var form = new NonPersistentSplitCreatorForm(basic))
			{
				form.Show();
				AssertEquals(true, form.Controls.Find("FRDButton", true)[0].Enabled);
				AssertEquals(true, form.Controls.Find("GenralButton", true)[0].Enabled);
				AssertEquals(false, form.Controls.Find("FcsButton", true)[0].Enabled);
			}

			basic.Profile = "CUKAIR98LHRBAC";
			using (var form = new NonPersistentSplitCreatorForm(basic))
			{
				form.Show();
				AssertEquals(false, form.Controls.Find("FRDButton", true)[0].Enabled);
				AssertEquals(false, form.Controls.Find("GenralButton", true)[0].Enabled);
				AssertEquals(true, form.Controls.Find("FcsButton", true)[0].Enabled);
			}

			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(true);
			basic.Profile = "CUKAIR98LHRLXA";
			using (var form = new NonPersistentSplitCreatorForm(basic))
			{
				form.Show();
				AssertEquals(false, form.Controls.Find("FRDButton", true)[0].Enabled);
				AssertEquals(false, form.Controls.Find("GenralButton", true)[0].Enabled);
				AssertEquals(true, form.Controls.Find("FcsButton", true)[0].Enabled);
			}
		}

		public void TestFlightDetailsForNprSplitAllocation()
		{
			var mawb = Factory.New<CusMAWB>();
			using (var form = new NonPersistentSplitCreatorForm(mawb))
			{
				form.Show();
				AssertEquals("FlightDetailsPanel not enabled when there are no splits and consignment NPR=0", false, form.Controls.Find("FlightDetailsPanel", true)[0].Enabled);
			}

			var split1 = mawb.Splits.AddNew();
			split1.NumberOfPiecesExpected = 4;
			var split2 = mawb.Splits.AddNew();
			split2.NumberOfPiecesExpected = 6;
			using (var form = new NonPersistentSplitCreatorForm(mawb))
			{
				form.Show();
				AssertEquals("FlightDetailsPanel not enabled when there are splits and consignment NPR=0", false, form.Controls.Find("FlightDetailsPanel", true)[0].Enabled);
				form.Close();
			}

			mawb.NumberOfPiecesReceived = 7;
			mawb.CM_ArrivalDate = ZDateTime.Today;
			using (var form = new NonPersistentSplitCreatorForm(mawb))
			{
				form.Show();
				AssertEquals("FlightDetailsPanel enabled when there are splits and NPR>0 and flight arrived", true, form.Controls.Find("FlightDetailsPanel", true)[0].Enabled);
				AssertEquals("SendFlightInfoTooCheckBox enabled when there are splits and NPR>0  and flight arrived", true, form.Controls.Find("SendFlightInfoTooCheckBox", true)[0].Enabled);
				AssertEquals("FlightNumberTextBox should be readonly when there are splits and NPR>0 and checkbox not checked", true, form.Controls.Find("FlightNumberTextBox", true)[0].GetReadOnly());
				AssertEquals("FlightArrivalDateDateEdit should be readonly when there are splits and NPR>0 and checkbox not checked", true, form.Controls.Find("FlightArrivalDateDateEdit", true)[0].GetReadOnly());
				form.Close();
			}

			using (var form = new NonPersistentSplitCreatorForm(mawb))
			{
				form.Show();
				var control = form.Controls.Find("SendFlightInfoTooCheckBox", true)[0];
				var sendFlightInfoToo = (ZCheckBox)control;
				sendFlightInfoToo.Checked = true;
				AssertEquals("FlightNumberTextBox should be enabled when there are splits and NPR>0 and checkbox checked", false, form.Controls.Find("FlightNumberTextBox", true)[0].GetReadOnly());
				AssertEquals("FlightArrivalDateDateEdit should be enabled when there are splits and NPR>0 and checkbox checked", false, form.Controls.Find("FlightArrivalDateDateEdit", true)[0].GetReadOnly());
				form.Close();
			}
		}

		public void TestPressButtonsDoesNothingIfInvalid()
		{
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "11122222222";
			basic.CargoTerminalOperator = "BAC";
			basic.CargoTerminalOperatorAirport = "LHR";
			basic.NumberOfPiecesExpected = 10;
			basic.Profile = "CUKFFW98000LXA";
			var split1 = basic.Splits.AddNew();
			split1.SplitReference = "01";
			split1.NumberOfPiecesExpected = 4;
			var split2 = basic.Splits.AddNew();
			split2.SplitReference = "02";
			split2.NumberOfPiecesExpected = 6;
			// Try to send with validation errors
			using (var form = new NonPersistentSplitCreatorForm(basic))
			{
				form.FormClosed += new FormClosedEventHandler(form_FormClosed);
				form.Show();
				var frdButton = (ZButton)form.Controls.Find("FRDButton", true)[0];
				var genralButton = (ZButton)form.Controls.Find("GenralButton", true)[0];
				var fcsButton = (ZButton)form.Controls.Find("FcsButton", true)[0];
				frdButton.PerformClick();
				AssertEquals("No message sent because the action did nothing", 0, basic.Messages.Count);
				AssertEquals("Form has not been closed & disposed because the action did nothing", false, formWasClosed);
				genralButton.PerformClick();
				AssertEquals("No message sent because the action did nothing", 0, basic.Messages.Count);
				AssertEquals("Form has not been closed & disposed because the action did nothing", false, formWasClosed);
				fcsButton.PerformClick();
				AssertEquals("No message sent because the action did nothing", 0, basic.Messages.Count);
				AssertEquals("Form has not been closed & disposed because the action did nothing", false, formWasClosed);
			}
			// Fix the validation errors and try again
			split1.Weight = 1;
			split2.Weight = 2;
			using (var form = new NonPersistentSplitCreatorForm(basic))
			{
				form.FormClosed += new FormClosedEventHandler(form_FormClosed);
				form.Show();
				var frdButton = (ZButton)form.Controls.Find("FRDButton", true)[0];
				frdButton.PerformClick();
				AssertEquals("One message sent", 1, basic.Messages.Count);
				AssertEquals("Form has been closed & disposed", true, formWasClosed);
			}
		}

		//public void TestIdleWorkerUpdatesCountOfPieces()
		//{ 
		//	var basic = Factory.New<CusMAWB>();
		//	basic.NumberOfPiecesExpected = 15;
		//	var split1 = basic.Splits.AddNew();
		//	split1.SplitReference = "01";
		//	split1.NumberOfPiecesExpected = 4;
		//	var split2 = basic.Splits.AddNew();
		//	split2.SplitReference = "02";
		//	split2.NumberOfPiecesExpected = 6;
		//	using (var form = new NonPersistentSplitCreatorForm(basic))
		//	{
		//		form.FormClosed += new FormClosedEventHandler(form_FormClosed);
		//		form.Show();
		//		AssertEquals(0, form.controller.SplitsAndFlightData.TotalPieces);
		//		System.Threading.Thread.Sleep(300);
		//		Application.DoEvents();  // shudder
		//		AssertEquals(10, form.controller.SplitsAndFlightData.TotalPieces);
		//	}
		//}

		bool formWasClosed;
		void form_FormClosed(object sender, FormClosedEventArgs e)
		{
			formWasClosed = true;
		}
	}

	[TestedType(typeof(RenominationForm))]
	class RenominationFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var cusMawb = Factory.New<CusMAWB>();
			cusMawb.AgentBadge = "DAN";
			cusMawb.Profile = "CUKFFW98000DAN";
			return new RenominationForm(cusMawb);
		}
	}

	class SplitConsignmentGridUserControlTest : TestCase
	{
		public void TestDoesntHaveCcsukMenuTooSoon()
		{
			using (var control = new SplitConsignmentGridUserControl())
			{
				var numberOfMenus = control.splitConsignmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.Count;
				using (var form = new ZForm())
				{
					form.Controls.Add(control);
					AssertEquals(numberOfMenus, control.splitConsignmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.Count);
					form.Show();
					AssertEquals("Now the CCSUK menu is present", numberOfMenus + 1, control.splitConsignmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.Count);
					AssertNotNull(control.splitConsignmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText(CcsukMenu.MenuCaption));
				}
			}
		}
	}

	[TestedType(typeof(SplitConsignmentModuleButtonGrid))]
	class SplitConsignmentModuleButtonGridModuleButtonGridTest : ZModuleButtonGridTestBase
	{
	}

	[TestedType(typeof(CheckInAllChildPiecesForm))]
	class CheckInAllChildPiecesFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var cusMawb = Factory.New<CusMAWB>();
			return new CheckInAllChildPiecesForm(cusMawb);
		}
	}
}
