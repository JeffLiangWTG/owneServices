using System.Windows.Forms;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using CusEntryHeader = Enterprise.Customs.AsycudaCustoms.Business.CusEntryHeader;
using LineMerger = Enterprise.Customs.AsycudaCustoms.Business.LineMerger;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	class EDIMenuTest : TestCaseWithFactory
	{
		public void TestGenerateAsycudaXMLMenuItem()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda, "ASYCO");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda, Core.Constants.CountryCodes.Namibia, "Namibia", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();

			var declaration = factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_DeclarationReference = "B0001000";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			var header = declaration.Invoices.AddNew();
			header.InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			using (var menu = new EDIMenuTestHelper())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();

				var generateAsycudaXMLMenuItem = menu.MenuItems.FindByText("Generate Asycuda XML");
				Assert(!generateAsycudaXMLMenuItem.Visible);
			}

			SetupAsycudaXMLCodeListAttribute();

			using (var form = new ZForm(declaration))
			using (var menu = new EDIMenuTestHelper())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();

				var generateAsycudaXMLMenuItem = menu.MenuItems.FindByText("Generate Asycuda XML");
				Assert(generateAsycudaXMLMenuItem.Visible);

				declaration.JE_CustomsOffice = "OF1";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				generateAsycudaXMLMenuItem.PerformClick();
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(menu.Declaration.HasChanges);

				UnitTestUserNotification.Instance.ClearMessages();
				declaration.Factory.Save();
				generateAsycudaXMLMenuItem.PerformClick();
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(GenerateAsycudaXMLForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				var merger = new LineMerger(declaration);
				merger.DoMerge();
				factory.Save();

				AssertEquals("Merged Entries Count", 1, declaration.CustomsEntryHeaders.Count);
				AssertEquals("B0001000/1", declaration.CustomsEntryHeaders[0].CH_BGMReference);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				generateAsycudaXMLMenuItem.PerformClick();
				AssertEquals(1, declaration.CustomsEntryHeaders[0].Messages.Count);
				AssertEquals("1 Asycuda Declaration message(s) queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				declaration.CustomsEntryHeaders[0].CH_BGMReference = "";
				factory.Save();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				generateAsycudaXMLMenuItem.PerformClick();
				AssertEquals("All entries should have references. Please go to menu 'Brokerage - Allocate Entry Reference Number' to allocate references.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[UseSnapshotProtection]
		public void TestAllocateBGMReferenceMenuItem()
		{
			BGMReferenceCounterProvider.Instance.ResetValue();

			var factory = new BusinessObjectFactory();
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B0001000";

			using (var form = new ZForm(declaration))
			using (var menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;

				var allocateBGMReferenceMenuItem = menu.MenuItems.FindByText("Allocate Entry Reference Number");
				Assert(!allocateBGMReferenceMenuItem.Visible);

				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				declaration.Factory.Save();

				AssertEquals("B0001000/1", declaration.ActiveEntryHeaders[0].CH_BGMReference);

				entryHeader.CH_BGMReference = "";
				menu.RefreshMenu();
				Assert(allocateBGMReferenceMenuItem.Visible);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				allocateBGMReferenceMenuItem.PerformClick();
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, declaration.HasChanges);
				AssertEquals("", entryHeader.CH_BGMReference);
				AssertEquals(true, entryHeader.HasChanges);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				allocateBGMReferenceMenuItem.PerformClick();
				AssertEquals(false, declaration.HasChanges);
				AssertEquals("B0001000/2", entryHeader.CH_BGMReference);
				AssertEquals(false, entryHeader.HasChanges);

				var factory2 = new BusinessObjectFactory();
				AssertEquals("B0001000/2", factory2.Load<CusEntryHeader>(entryHeader.PK).CH_BGMReference);
			}
		}

		public void TestDisplayGenerateEntriesMenuOption()
		{
			var ediMenu = new EDIMenuTestHelper();
			var displayGenerateEntriesMenuOption = ediMenu.GetDisplayGenerateEntriesMenuOption();
			Assert("DisplayGenerateEntriesMenuOption flag is true", displayGenerateEntriesMenuOption);
		}

		public void TestGenerateEntriesMenuItemVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			using (var form = new ZForm(declaration))
			using (var menu = new EDIMenu())
			{
				form.Menu.MenuItems.Add(menu);
				menu.Declaration = declaration;
				menu.RefreshMenu();

				Assert("GenerateEntriesMenuItem is Visible", menu.GenerateEntriesMenuItem.Visible);
			}
		}

		public void TestSupportShortcutMenus()
		{
			var quantity = 100m;
			var helper = new Customs.Business.Testing.WhsDataTestHelper(Factory);
			helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = helper.Importer.PK;
			declaration.JE_DeclarationReference = "B0000123";
			declaration.JE_CustomsOffice = "BFN";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var invoice = declaration.Invoices.AddNew();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "01";
			entryInstruction1.CEI_OA_Warehouse2 = helper.WhsWarehouse.WW_OA_WarehouseAddress;
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = "02";
			entryInstruction2.CEI_OA_Warehouse2 = helper.WhsWarehouse.WW_OA_WarehouseAddress;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			invoiceLine1.JI_PartNo = helper.Part.OP_PartNum;
			invoiceLine1.JI_Procedure = helper.InwardCusProcedure.ZZ6_ProcedureCode + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
			invoiceLine1.JI_InvoiceQuantity = quantity;
			invoiceLine1.JI_InvoiceUQ = "NO";
			invoiceLine1.JI_BondedWhsQuantity = quantity;
			invoiceLine1.JI_BondedWhsUnitQty = "NO";
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.JI_CustomsQuantity = quantity * 10m;
			invoiceLine1.JI_LinePrice = quantity * 100m;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;
			invoiceLine2.JI_PartNo = helper.Part.OP_PartNum;
			invoiceLine2.JI_Procedure = helper.InwardCusProcedure.ZZ6_ProcedureCode + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
			invoiceLine2.JI_InvoiceQuantity = quantity;
			invoiceLine2.JI_InvoiceUQ = "NO";
			invoiceLine2.JI_BondedWhsQuantity = quantity;
			invoiceLine2.JI_BondedWhsUnitQty = "NO";
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_CustomsQuantity = quantity * 10m;
			invoiceLine2.JI_LinePrice = quantity * 100m;
			declaration.DoMerge();

			AssertEquals(2, declaration.ActiveEntryHeaders.Count);
			declaration.ActiveEntryHeaders[0].EntryNumber = "ENT3243";
			declaration.ActiveEntryHeaders[1].EntryNumber = "ENT3245";

			using (var menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				menu.RefreshMenu();

				CombineAssertions("EDI Menu Support ShortCut Menus", () =>
				{
					var bondedWarehouseMenuItem = menu.MenuItems.FindByText("Inventory Management");
					bondedWarehouseMenuItem.ShowPopupMenu();
					AssertEquals(13, bondedWarehouseMenuItem.MenuItems.Count);
					AssertEquals("Entry1 shortCut menu appears at the top", bondedWarehouseMenuItem.MenuItems[0].Text, "&Update Bonded Warehouse - 01 - ENT3243");
					AssertEquals("Entry2 shortCut menu appears behind Entry1 shortCut", bondedWarehouseMenuItem.MenuItems[1].Text, "&Update Bonded Warehouse - 02 - ENT3245");
					var entry1MenuItem = bondedWarehouseMenuItem.MenuItems[11];
					AssertEquals("Entry1 menu appears at the bottom", entry1MenuItem.Text, "01 - ENT3243");
					AssertEquals("Entry1 sub menu", entry1MenuItem.MenuItems[0].Text, "&Update Inventory");
					var entry2MenuItem = bondedWarehouseMenuItem.MenuItems[12];
					AssertEquals("Entr2 menu", entry2MenuItem.Text, "02 - ENT3245");
					AssertEquals("Entry2 sub menu", entry2MenuItem.MenuItems[0].Text, "&Update Inventory");
				});
			}
		}

		public void TestSetCustomsEntryStatusMenuItem()
		{
			var factory = new BusinessObjectFactory();
			var declaration = factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var menu = new EDIMenu())
			{
				menu.Declaration = declaration;
				form.Menu.MenuItems.Add(menu);
				var setEntryStatusMenuItem = menu.MenuItems.FindByText("Set Entry Status");
				AssertNotNull(setEntryStatusMenuItem);
				AssertEquals(true, setEntryStatusMenuItem.Visible);
				setEntryStatusMenuItem.PerformClick();
				using (var setEntryStatusFrom = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertNotNull("SetEntryStatusFrom should have been displayed", setEntryStatusFrom);
					AssertEquals("SetEntryStatusFrom should have been displayed", typeof(SetEntryStatusForm), setEntryStatusFrom.GetType());
					setEntryStatusFrom.Close();
					setEntryStatusFrom.Dispose();
				}
			}
		}

		static void SetupAsycudaXMLCodeListAttribute()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			var codeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda, Core.Constants.CountryCodes.Botswana, RefCountry.LoadFromCountryCode(factory, Core.Constants.CountryCodes.Botswana).RN_Desc, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(codeList.PK, Customs.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Name.AsycudaXML, Customs.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes);
			factory.Save();
		}
	}

	class EDIMenuTestHelper : EDIMenu
	{
		public bool GetDisplayGenerateEntriesMenuOption() => DisplayGenerateEntriesMenuOption;

		protected override JobDeclarationUniversalMessagingHelper GetJobDeclarationUniversalMessagingHelper(IJobDeclarationMessageSendingObjectParent wrapper)
		{
			((JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject>)wrapper).SendingObjectsCollection[0].ShouldSend = true;
			return new AsycudaJobDeclarationUniversalMessagingHelper(wrapper);
		}
	}
}
