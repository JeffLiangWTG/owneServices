using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI.SADH;
using Enterprise.Customs.EU.GUI.SingleLineEntry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.EU.GUI.Testing
{
	public class EDIMenuTest : TestCaseWithFactory
	{
		public void TestMenuItems()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var testMenu = new EDIMenu())
			{
				CombineAssertions(() =>
				{
					testMenu.Declaration = declaration;
					AssertEquals("&Brokerage", testMenu.Text);

					var menuItemsText = new ZStringBuilder();
					testMenu.MenuItems.Cast<MenuItem>().ForEach(x => menuItemsText.Append(x.Text));
					AssertEquals(@"Submit
Commercial &Invoices
Auto Apportion &Weight
Allocate Remaining Weight
Inventory Management
TS Register Management
&Copy Previous Invoice Line
Create Product Files
Refresh Product Data
Data
Generate Entries (&Merge)
Perform Apportionment
Expand ALL Lines by their Bills Of Materials
Collapse Bills Of Materials for ALL Lines (Remove Expanded Lines)
Send to Global Manifest
Create Packing List
SAD/H Data Entry Form
Single Line Entry
Create supplementary entry from simplified entry
-
Audit Customs Declaration", menuItemsText.ToStringWithNewLineBetweenAppends());

					AssertEquals("Submit", testMenu.MenuItems[0].Text);
					var commercialInvoicesItem = testMenu.MenuItems[1];
					AssertEquals("Commercial &Invoices", commercialInvoicesItem.Text);
					AssertEquals("&Attach Commercial Invoices", commercialInvoicesItem.MenuItems[0].Text);
					AssertEquals("&Copy Commercial Invoices", commercialInvoicesItem.MenuItems[1].Text);

					AssertEquals("Auto Apportion &Weight", testMenu.MenuItems[2].Text);

					var allocateRemainingWeightMenuItem = testMenu.MenuItems[3];
					AssertEquals("Allocate Remaining Weight", allocateRemainingWeightMenuItem.Text);
					AssertEquals("Allocate Remaining Weight by Price", allocateRemainingWeightMenuItem.MenuItems[0].Text);
					AssertEquals("Allocate Remaining Weight by Quantity", allocateRemainingWeightMenuItem.MenuItems[1].Text);

					AssertEquals("Inventory Management", testMenu.MenuItems[4].Text);
					var tsRegisterManagementMenuItem = testMenu.MenuItems[5];
					AssertEquals("TS Register Management", tsRegisterManagementMenuItem.Text);
					AssertEquals("TS Register Management sub menu item", "Select Inventory", tsRegisterManagementMenuItem.MenuItems[0].Text);
					AssertEquals("&Copy Previous Invoice Line", testMenu.MenuItems[6].Text);
					AssertEquals("Create Product Files", testMenu.MenuItems[7].Text);
					AssertEquals("Refresh Product Data", testMenu.MenuItems[8].Text);
					var dataMenuItem = testMenu.MenuItems[9];
					AssertEquals("Data", dataMenuItem.Text);
					AssertEquals("Data sub menu items", "Import Invoices", dataMenuItem.MenuItems[0].Text);
					AssertEquals("Data sub menu items", "Import Order Lines", dataMenuItem.MenuItems[1].Text);
					AssertEquals("Data sub menu items", "Export Declaration to XML", dataMenuItem.MenuItems[2].Text);
					AssertEquals("Data sub menu items", "Auto Populate Authorizations", dataMenuItem.MenuItems[3].Text);
					AssertEquals("Generate Entries (&Merge)", testMenu.MenuItems[10].Text);
					AssertEquals("Perform Apportionment", testMenu.MenuItems[11].Text);
					AssertEquals("Expand ALL Lines by their Bills Of Materials", testMenu.MenuItems[12].Text);
					AssertEquals("Collapse Bills Of Materials for ALL Lines (Remove Expanded Lines)", testMenu.MenuItems[13].Text);
					AssertEquals("Send to Global Manifest", testMenu.MenuItems[14].Text);
					AssertEquals("Create Packing List", testMenu.MenuItems[15].Text);
					AssertEquals("SAD/H Data Entry Form", testMenu.MenuItems[16].Text);
					AssertEquals("Single Line Entry", testMenu.MenuItems[17].Text);
				});
			}
		}

		public void TestTSRegisterManagementMenuItemVisibility_TemporaryStorageRegisterNotEnabled()
		{
			var declaration = Factory.New<JobDeclaration>();
			var registry = ObjectFactory.Get<IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (var menu = new EDIMenu())
			{
				menu.Declaration = declaration;

				menu.RefreshMenu();
				var tsRegisterManagementMenuItem = menu.MenuItems.FindByText("TS Register Management");
				AssertEquals("tsRegisterManagementMenuItem.Visible is false when Temporary Storage Register is not enabled", false, tsRegisterManagementMenuItem.Visible);

				var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction1.GoodsLocation.Address.AuthorisationNumber = "location1";

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.GoodsLocation.Address.AuthorisationNumber = "location2";

				var premises = Factory.New<ICusTempStorageRegPremises>();
				premises.SRP_Type = "AAA";
				premises.SRP_CustomsLocation = "location1";
				premises.SRP_Code = "X";
				premises.SRP_Description = "DESC";

				menu.RefreshMenu();
				tsRegisterManagementMenuItem = menu.MenuItems.FindByText("TS Register Management");
				AssertEquals("tsRegisterManagementMenuItem.Visible is false when Temporary Storage Register is not enabled, even if entryInstruction's authorization No is in premises", false, tsRegisterManagementMenuItem.Visible);
			}
		}

		public void TestTSRegisterManagementMenuItemVisibility_TemporaryStorageRegisterEnabled()
		{
			var declaration = Factory.New<JobDeclaration>();
			var registry = ObjectFactory.Get<IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var menu = new EDIMenu())
			{
				menu.Declaration = declaration;

				menu.RefreshMenu();
				var tsRegisterManagementMenuItem = menu.MenuItems.FindByText("TS Register Management");
				AssertEquals("tsRegisterManagementMenuItem.Visible is false when Temporary Storage Register is enabled but there are no entry instructions", false, tsRegisterManagementMenuItem.Visible);

				var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction1.GoodsLocation.Address.AuthorisationNumber = "location1";

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.GoodsLocation.Address.AuthorisationNumber = "location2";

				var premises = Factory.New<ICusTempStorageRegPremises>();
				premises.SRP_Type = "AAA";
				premises.SRP_CustomsLocation = "location1";
				premises.SRP_Code = "X";
				premises.SRP_Description = "DESC";

				menu.RefreshMenu();
				tsRegisterManagementMenuItem = menu.MenuItems.FindByText("TS Register Management");
				AssertEquals("tsRegisterManagementMenuItem.Visible is true when Temporary Storage Register is enabled and at least one entryInstruction's authorization No is in premises", true, tsRegisterManagementMenuItem.Visible);

				entryInstruction1.GoodsLocation.Address.AuthorisationNumber = "location";
				declaration.JE_LocationOfGoods = "location1";
				menu.RefreshMenu();
				tsRegisterManagementMenuItem = menu.MenuItems.FindByText("TS Register Management");
				AssertEquals("tsRegisterManagementMenuItem.Visible is false when Temporary Storage Register is enabled but no entryInstruction's authorization No is in premises", false, tsRegisterManagementMenuItem.Visible);
			}
		}

		[RequiresSTA]
		public void TestImportNCTSLinesMenuItem()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euctp = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EUCommonTransitProcedure, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(euctp, GlbCompany.CurrentCompany.Country.Code);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			ediMenu.Declaration = declaration;
			var importNCTSLinesMenuItem = ediMenu.MenuItems.FindByText("Import NCTS lines");
			AssertNull(importNCTSLinesMenuItem);

			using (var form = new ZForm())
			{
				form.Menu.MenuItems.Add(ediMenu);
				form.Show();

				var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction1.CEI_Style = "A";
				entryInstruction1.CEI_Description = "Description 1";
				entryInstruction1.CEI_DisplaySequence = 2;

				var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
				entryHeader1.EntryNumber = "entry1";

				ediMenu.RefreshMenu();
				importNCTSLinesMenuItem = ediMenu.MenuItems.FindByText("Import NCTS lines");
				AssertNotNull(importNCTSLinesMenuItem);
				AssertEquals(0, importNCTSLinesMenuItem.MenuItems.Count);

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_Style = "B";
				entryInstruction2.CEI_Description = "Description 2";
				entryInstruction2.CEI_DisplaySequence = 1;

				var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
				entryHeader2.EntryNumber = "entry2";

				ediMenu.RefreshMenu();
				importNCTSLinesMenuItem = ediMenu.MenuItems.FindByText("Import NCTS lines");
				AssertNotNull(importNCTSLinesMenuItem);
				AssertEquals(2, importNCTSLinesMenuItem.MenuItems.Count);
				AssertEquals("1_B_Description 2_entry2", importNCTSLinesMenuItem.MenuItems[0].Text);
				AssertEquals("2_A_Description 1_entry1", importNCTSLinesMenuItem.MenuItems[1].Text);
				AssertEquals(entryInstruction2.PK, importNCTSLinesMenuItem.MenuItems[0].Tag);
				AssertEquals(entryInstruction1.PK, importNCTSLinesMenuItem.MenuItems[1].Tag);

				importNCTSLinesMenuItem.MenuItems[1].PerformClick();
				var lastFormShown = ZFormModaliser.LastFormShownForTest;
				AssertType<EmbeddedModulePopup>("LastFormShown type", lastFormShown);
				AssertEquals("NCTS Transit Movements", lastFormShown.Text);

				declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
				var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader3.CH_CEI_Instruction = entryInstruction3.PK;

				ediMenu.RefreshMenu();
				AssertEquals(0, importNCTSLinesMenuItem.MenuItems.Count);

				importNCTSLinesMenuItem.PerformClick();
				lastFormShown = ZFormModaliser.LastFormShownForTest;
				AssertType<EmbeddedModulePopup>("LastFormShown type", lastFormShown);
				AssertEquals("NCTS Transit Movements", lastFormShown.Text);
			}
		}

		public void TestGenerateEntriesMenuItem_Visible()
		{
			CombineAssertions(() =>
			{
				var generateEntriesMenuItem = ediMenu.GenerateEntriesMenuItem;
				AssertNotNull("Not null", generateEntriesMenuItem);
				AssertEquals("Included menu option", true, ediMenu.MenuItems.Contains(generateEntriesMenuItem));
			});
		}

		public void TestSADHDataEntryMenu_Caption()
		{
			AssertNotNull(ediMenu.MenuItems.FindByText(SADHMenuItemCaption));
		}

		public void TestSADHDataEntryMenu_Click()
		{
			ediMenu.Declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var sadhMenuItem = ediMenu.MenuItems.FindByText(SADHMenuItemCaption);
			sadhMenuItem.PerformClick();
			AssertType<SADHEntryForm>(ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestSingleLineEntryMenu_Caption()
		{
			AssertNotNull(ediMenu.MenuItems.FindByText(SingleLineEntryMenuItemCaption));
		}

		public void TestSingleLineEntryMenu_Click()
		{
			ediMenu.Declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var singleLineMenuItem = ediMenu.MenuItems.FindByText(SingleLineEntryMenuItemCaption);
			singleLineMenuItem.PerformClick();
			AssertType<SingleLineEntryForm>(ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestAutoPopulateAuthorizations_Caption()
		{
			var dataMenuItem = ediMenu.MenuItems.FindByText(DataMenuItemCaption);
			CombineAssertions(() =>
			{
				AssertNotNull(dataMenuItem);
				AssertNotNull(dataMenuItem.MenuItems.FindByText(AutoPopulateAuthorizationsMenuItemCaption));
			});
		}

		public void TestAutoPopulateAuthorizations_Click()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CusAuthorizationUsages.AddNew().AGC_IsSystemGenerated = true;
			ediMenu.Declaration = declaration;
			var dataMenuItem = ediMenu.MenuItems.FindByText(DataMenuItemCaption);
			var autoPopulateAuthorizationsMenuItem = dataMenuItem.MenuItems.FindByText(AutoPopulateAuthorizationsMenuItemCaption);
			autoPopulateAuthorizationsMenuItem.PerformClick();
			AssertEquals(0, entryInstruction.CusAuthorizationUsages.Count);
		}

		public void TestSupplementaryEntryMenuVisibility()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			ediMenu.Declaration = declaration;
			ediMenu.RefreshMenu();
			var menuItem = ediMenu.MenuItems.FindByText("Create supplementary entry from simplified entry");
			AssertEquals(false, menuItem.Visible);
		}

		public void TestSupplementaryEntryMenuVisibility_TestClass()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclarationForSupplementaryDeclarationTesting>();
			ediMenu.Declaration = declaration;
			ediMenu.RefreshMenu();
			var menuItem = ediMenu.MenuItems.FindByText("Create supplementary entry from simplified entry");
			AssertNotNull(menuItem);
			AssertEquals(true, menuItem.Visible);
		}

		#region InventoriesSelectionFromTSRegisterManagementMenuItem
		public void TestInventoriesSelectionFromTSRegisterManagementMenuItem_ClickType()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var testMenu = new EDIMenu())
			using (var form = new ZForm())
			{
				form.Menu.MenuItems.Add(testMenu);
				form.Show();
				testMenu.Declaration = declaration;

				var tsRegisterManagementMenuItem = testMenu.MenuItems.FindByText(EDIMenuCaptions.TSRegisterManagement);
				var inventoriesSelectionFromTSRegisterManagementMenuItem = tsRegisterManagementMenuItem.MenuItems.FindByText(EDIMenuCaptions.InventoriesSelectionFromTSRegisterManagement);

				Factory.Save();
				inventoriesSelectionFromTSRegisterManagementMenuItem.PerformClick();
				AssertType("The form that should open is TemporaryStorageRegisterLinesSelectionForm type", typeof(TemporaryStorageRegisterLinesSelectionForm), ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestInventoriesSelectionFromTSRegisterManagementMenuItem_ClickOKAndSucces()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();
			AssertInventoriesSelectionFromTSRegisterManagementMenuItem_ClickOK(declaration, "Import success; all has been saved", 1);
		}

		public void TestInventoriesSelectionFromTSRegisterManagementMenuItem_ClickOKAndFailing() => AssertInventoriesSelectionFromTSRegisterManagementMenuItem_ClickOK(Factory.New<JobDeclaration>(), "Import not success; nothing has been saved", 0);

		public void AssertInventoriesSelectionFromTSRegisterManagementMenuItem_ClickOK(JobDeclaration declaration, ZString expectedMessage, int invoiceNumberExpectedNumber)
		{
			using (var form = new JobDeclarationFormForTesting(declaration))
			{
				form.ControllerID = ControllerIDs.Customs.JobDeclaration;
				var testMenu = (EDIMenuForTesting)form.Menu.MenuItems.FindByText("&Brokerage");
				testMenu.Declaration = declaration;
				form.Menu.MenuItems.Add(testMenu);
				form.Show();

				var tsRegisterManagementMenuItem = testMenu.MenuItems.FindByText(EDIMenuCaptions.TSRegisterManagement);
				var inventoriesSelectionFromTSRegisterManagementMenuItem = tsRegisterManagementMenuItem.MenuItems.FindByText(EDIMenuCaptions.InventoriesSelectionFromTSRegisterManagement);

				Factory.Save();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					if (dialog is TemporaryStorageRegisterLinesSelectionForm sendingForm)
					{
						var sendingObjectParent = (CusTempStorageRegLinesSelectionHeader)sendingForm.DataSource;
						sendingObjectParent.SelectedLines.Add(CreateCusTempStorageSelectableRegLine());
					}
				});

				inventoriesSelectionFromTSRegisterManagementMenuItem.PerformClick();
				var factory = new BusinessObjectFactory();
				var newFactoryDeclaration = factory.Load<JobDeclaration>(declaration.PK);

				CombineAssertions(() =>
				{
					AssertEquals("Invoice line created if processed", invoiceNumberExpectedNumber, newFactoryDeclaration.InvoiceLines.Count);
					AssertEquals("Should show the correct message", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				});
				testMenu.ReloadedForm?.Dispose();
			}
		}

		class EDIMenuForTesting : EDIMenu
		{
			public ZForm ReloadedForm { get; private set; }

			protected override ZForm ReloadBaseForm()
			{
				ReloadedForm = base.ReloadBaseForm();
				return ReloadedForm;
			}
		}

		class JobDeclarationFormForTesting : JobDeclarationForm
		{
			public JobDeclarationFormForTesting(JobDeclaration declaration) : base(declaration)
			{
			}

			protected override Customs.GUI.IEDIMenu GetNewTopLevelMenuCore() => new EDIMenuForTesting();
		}

		CusTempStorageSelectableRegLine CreateCusTempStorageSelectableRegLine()
		{
			var mockPremises = new Mock<ICusTempStorageRegPremises>();
			mockPremises.Setup(p => p.SRP_CustomsLocation).Returns("12");

			var mockHeader = new Mock<ICusTempStorageRegHeader>();
			mockHeader.Setup(h => h.Premises).Returns(mockPremises.Object);
			mockHeader.Setup(h => h.SRH_Reference).Returns("TSDRef");
			mockHeader.Setup(h => h.SRH_ArrivalDate).Returns(ZDate.Today);
			var regLine = Factory.New<CusTempStorageRegLine>();
			var regItem = Factory.New<CusTempStorageRegLineItem>();
			regItem.SRI_Tariff = "12345";
			regItem.SRI_GoodsDescription = "TestGoods";
			regItem.SRI_CusC4Number = "1234";
			var collectionPivot = new CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot>(regLine);
			var pivot = collectionPivot.AddChild(regItem);

			var itemList = new RegLineItemQuantityCollection
				{
					RegLineItemQuantity.LoadNew(pivot)
				};

			var mockRegLine = new Mock<ICusTempStorageRegLine>();
			mockRegLine.Setup(r => r.RegHeader).Returns(mockHeader.Object);
			mockRegLine.Setup(r => r.RegLineItemQuantities).Returns(itemList);
			mockRegLine.Setup(r => r.GrossWeightRemainingCalculated).Returns(1234);
			mockRegLine.Setup(r => r.PackagesRemainingCalculated).Returns(1234);
			var selectedRegLine = new CusTempStorageSelectableRegLine(mockRegLine.Object);
			selectedRegLine.GrossWeightToDraw = 1234;
			selectedRegLine.PackagesToDraw = 1234;
			return selectedRegLine;
		}
		#endregion

		public void TestNewRelatedDeclarationMenuItem()
		{
			AssertMenuShowsNotification(EDIMenuCaptions.NewRelatedDeclaration, "No suitable simplified entry could be found");
		}

		public void TestNewEntryInstructionMenuItem()
		{
			AssertMenuShowsNotification(EDIMenuCaptions.NewEntryInstruction, "No suitable simplified entry could be found");
		}

		public void TestReUseEntryInstructionMenuItem()
		{
			AssertMenuShowsNotification(EDIMenuCaptions.ReUseEntryInstruction, "No suitable simplified entry could be found");
		}

		void AssertMenuShowsNotification(string menuSearchText, string expectedNotificationText)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclarationForSupplementaryDeclarationTesting>();
			ediMenu.Declaration = declaration;

			var menuItem = ediMenu.MenuItems.FindByText(menuSearchText, findSubitems: true);
			AssertNotNull("Pre-requisite: find menu item", menuItem);

			menuItem.PerformClick();

			var lastNotificationText = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertContains(expectedNotificationText, lastNotificationText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ediMenu = new EDIMenu();
		}

		protected override void TearDown()
		{
			base.TearDown();
			ediMenu.Dispose();
		}
		EDIMenu ediMenu;

		public const string SADHMenuItemCaption = "SAD/H Data Entry Form";
		public const string SingleLineEntryMenuItemCaption = "Single Line Entry";
		const string DataMenuItemCaption = "Data";
		const string AutoPopulateAuthorizationsMenuItemCaption = "Auto Populate Authorizations";
	}
}
