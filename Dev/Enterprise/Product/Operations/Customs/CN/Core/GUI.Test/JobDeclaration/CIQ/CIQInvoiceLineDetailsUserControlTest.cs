using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.CN.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(CIQInvoiceLineDetailsUserControl))]
	class CIQInvoiceLineDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestControlsExistence()
		{
			using var control = new CIQInvoiceLineDetailsUserControl();
			TestUtility.AssertControlExistance(control, "DangerousGoodsGuidFindBox", "DangerousGoodsDGSubs");
			TestUtility.AssertControlExistance(control, "BatchNumbersGrid", "ProductionBatch");
			TestUtility.AssertControlExistance(control, "JI_CIQQualityGuaranteePeriodCalcEdit", "JI_CIQQualityGuaranteePeriod");
			TestUtility.AssertControlExistance(control, "JI_CIQExpiryDateEdit", "JI_CIQExpiryDate");
			TestUtility.AssertControlExistance(control, "JI_CIQEndUseDropEdit", "JI_CIQEndUse");
			TestUtility.AssertControlExistance(control, "JI_CIQCodeFindBox", "JI_CIQTariff");
			TestUtility.AssertControlExistance(control, "JI_OrigContainerFlagDropEdit", "JI_OrigContainerFlag");
			TestUtility.AssertControlExistance(control, "JI_BrandNameTextBox", "JI_BrandName");
			TestUtility.AssertControlExistance(control, "JI_ModelTextBox", "JI_Model");
			TestUtility.AssertControlExistance(control, "JI_NDescriptionTextBox", "JI_NDescription");
			TestUtility.AssertControlExistance(control, "JI_OA_ManufacturerAddressControl", "JI_OA_ManufacturerAddress");
			TestUtility.AssertControlExistance(control, "JI_PackageTypeOfUNDGDropEdit", "JI_PackageTypeOfUNDG");
			TestUtility.AssertControlExistance(control, "JI_NonDangerousChemicalFlagCheckBox", "JI_NonDangerousChemicalFlag");
			TestUtility.AssertControlExistance(control, "CIQIngredientTextBox", "CIQIngredient");
			TestUtility.AssertControlExistance(control, "SyncCIQDetailsButton");
			TestUtility.AssertControlExistance(control, "BatchNumbersTextBox", "BatchNumbersAsString");
			TestUtility.AssertControlExistance(control, "ManufactureDatesTextBox", "ManufactureDatesAsString");
		}

		public void TestSyncCIQDetailsButton_Propeties()
		{
			using var control = new CIQInvoiceLineDetailsUserControl();
			var syncCIQDetailsButton = control.FindSingle<ZButton>("SyncCIQDetailsButton");
			AssertEquals("ToolTipCaption", "Synchronize CIQ Ingredient, Specification, Brand, Model, Manufacture Dates with Specification & Model (Additional Information)", syncCIQDetailsButton.ToolTipCaption);
		}

		public void TestSyncCIQDetailsButton_Click()
		{
			var item = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () =>
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateCustomsTariff("6202131000", "720c28d9e797c0d8408d34da1b0ab981", "baeb884cae1c682fd9c48a9bbc20b849", "99998", "ef374a392b7934885636cee2f907884a");
				helper.CreateAdditionalElement("720c28d9e797c0d8408d34da1b0ab981", "面料成分含量");
				helper.CreateAdditionalElement("baeb884cae1c682fd9c48a9bbc20b849", "品牌(厂商)");
				helper.CreateAdditionalElement("99998", "规格型号");
				helper.CreateAdditionalElement("ef374a392b7934885636cee2f907884a", "生产日期");
				Factory.Save();
			});
			var invoiceLine = item.InvoiceLine;
			invoiceLine.JI_Tariff = "6202131000";
			invoiceLine.XC_GoodsSpecModel = "50|B|规格：333、型号：444|20220201;20220202";

			using var form = new JobDeclarationForm(item.JobDeclaration);
			form.Show();
			var invoiceLineUserControl = form.FindInvoiceLineUserControl();
			invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Select(0);
			invoiceLineUserControl.LineDetailTabControl.SelectedIndex = 2;
			var ciqInvoiceLineDetailsUserControl = invoiceLineUserControl.LineDetailTabControl.FindSingle<ZUserControl>("CIQInvoiceLineDetailsUserControl");
			var currentInvoiceLine = (JobComInvoiceLine)ciqInvoiceLineDetailsUserControl.CurrentDataItem;
			var syncCIQDetailsButton = ciqInvoiceLineDetailsUserControl.FindSingle<ZButton>("SyncCIQDetailsButton");
			syncCIQDetailsButton.PerformClick();

			CombineAssertions(() =>
			{
				AssertEquals("Ingredient", "50", invoiceLine.CIQIngredient);
				AssertEquals("Specification", "333", invoiceLine.JI_NDescription);
				AssertEquals("Brand", "B", invoiceLine.JI_BrandName);
				AssertEquals("Model", "444", invoiceLine.JI_Model);
				AssertEquals("Manufacture Date", "20220201;20220202", invoiceLine.ManufactureDatesAsString);
			});
		}

		public void TestJI_NonDangerousChemicalFlagCheckBox_Visibility()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.UNDGs.AddNew();
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var invoiceLineUserControl = form.FindInvoiceLineUserControl();
			invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Select(0);
			invoiceLineUserControl.LineDetailTabControl.SelectedIndex = 2;
			var ji_NonDangerousChemicalFlagCheckBox = invoiceLineUserControl.LineDetailTabControl.FindSingle<ZCheckBox>("JI_NonDangerousChemicalFlagCheckBox");
			var ciqInvoiceLineDetailsUserControl = invoiceLineUserControl.LineDetailTabControl.FindSingle<ZUserControl>("CIQInvoiceLineDetailsUserControl");
			var currentInvoiceLine = (JobComInvoiceLine)ciqInvoiceLineDetailsUserControl.CurrentDataItem;
			currentInvoiceLine.DangerousGoodsDGSubs = ZGuid.Empty;
			AssertEquals(false, ji_NonDangerousChemicalFlagCheckBox.Visible);
			currentInvoiceLine.DangerousGoodsDGSubs = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			AssertEquals("Non Dangerous should be visible if UNDG is entered", true, ji_NonDangerousChemicalFlagCheckBox.Visible);
			currentInvoiceLine.DangerousGoodsDGSubs = ZGuid.Empty;
			AssertEquals("Non Dangerous should be invisible for UNDG is cleared", false, ji_NonDangerousChemicalFlagCheckBox.Visible);
			var testCollection = currentInvoiceLine.CargoAttributes as ICodeDescriptionOptionStorage;
			testCollection.AddNew(CargoAttributeList.Codes._31);
			AssertEquals("JI_NonDangerousChemicalFlagCheckBox should be shown for CargoAttribute 31 selected.", true, ji_NonDangerousChemicalFlagCheckBox.Visible);
			testCollection.RemoveAndDelete(testCollection.FindByCode(CargoAttributeList.Codes._31));
			testCollection.AddNew(CargoAttributeList.Codes._33);
			AssertEquals("JI_NonDangerousChemicalFlagCheckBox should be shown for CargoAttribute 33 selected.", true, ji_NonDangerousChemicalFlagCheckBox.Visible);
		}

		public void TestCargoAttributesButton_Click()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew() as JobComInvoiceLine;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var invoiceLineUserControl = form.FindInvoiceLineUserControl();
			invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Select(0);
			invoiceLineUserControl.LineDetailTabControl.SelectedIndex = 2;
			var control = invoiceLineUserControl.FindSingle<CodeDescriptionSelectionUserControl>("CargoAttributesTextBox");
			AssertEquals("CargoAttributesAsString", control.TextBox.GetBindingMember());
			control.EditButton.PerformClick();
			AssertType<CodeDescriptionOptionForm>(ZFormModaliser.LastFormShownDialogForTest);
			AssertSame(invoiceLine.CargoAttributes, (ZFormModaliser.LastIBusinessShownOnDialogForTest as CodeDescriptionOptionCollectionParent).OptionCollection.Storage);
		}

		public void TestTriggerRequiresCIQOnJI_CIQTariffChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			Assert(!instruction.CEI_CIQRequires);
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
			brokerageControl.LoadInvoiceLinesTabPage();
			var invoiceLineUserControl = (InvoiceLineUserControl)brokerageControl.InvoiceLinesUserControl;
			invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Select(0);
			invoiceLineUserControl.LineDetailTabControl.SelectedIndex = 2;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			invoiceLine.JI_CIQTariff = "1101000001999";
			Assert(instruction.CEI_CIQRequires);
			instruction.CEI_CIQRequires = false;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			invoiceLine.JI_CIQTariff = "1101000001999";
			Assert(!instruction.CEI_CIQRequires);
		}
	}
}
