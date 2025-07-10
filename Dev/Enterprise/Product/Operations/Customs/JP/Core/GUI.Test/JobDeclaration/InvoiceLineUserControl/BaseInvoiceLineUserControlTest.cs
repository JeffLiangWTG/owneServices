using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(BaseInvoiceLineUserControl))]
	sealed class BaseInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestTariffPopupForm()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Japan, Universal.Constants.TariffTypes.Import);
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO;
			helper.LoadOrCreateNewTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, tariffType.PK, "1234567890", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "test description 1");
			Factory.Save();

			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			var header = jobDeclartion.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";

			using var testForm = new JobDeclarationForm(jobDeclartion);
			testForm.Show();
			var brokerageUserControl = testForm.CustomsBrokerageUserControl;
			brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.InvoiceLinesTabPage;
			var invoiceLinesUserControl = brokerageUserControl.InvoiceLinesUserControl as ImportInvoiceLineUserControl;
			var grid = invoiceLinesUserControl.CustomsInvoiceLinesBoundGrid;
			grid.CurrentRowIndex = 0;
			var info = grid.GetColumnStyle("JI_FormattedTariff") as TariffColumnStyleInfo;
			using (var style = new TariffColumnStyle(info))
			{
				style.SetParentGrid(grid);
				style.EditControl.Parent = grid;
				((TariffGridFindBox)style.EditControl).SelectFromPopupForm();
				var tariffGridSearchForm = ZFormModaliser.LastFormShownForTest as TariffFindBoxTreeViewForm;
				AssertNotNull(tariffGridSearchForm);
			}

			ZFormModaliser.LastFormShownForTest = null;
			var tariffFindBox = invoiceLinesUserControl.FindSingle<TariffFindBox>("TariffFindBox");
			tariffFindBox.SelectFromPopupForm();
			var tariffSearchForm = ZFormModaliser.LastFormShownForTest as TariffFindBoxTreeViewForm;
			AssertNotNull(tariffSearchForm);
		}

		public void TestTariffType()
		{
			var decl = Factory.NewWithValidTestData<JobDeclaration>();
			decl.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			var header = decl.Invoices.AddNew();
			header.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(decl))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var tariffFindBox = invoiceLineUserControl.FindSingle<TariffFindBox>("TariffFindBox");
				AssertEquals("TariffType: IMP", Universal.Constants.TariffTypes.Import, tariffFindBox.GetTariffType.Invoke());
			}

			decl.JE_MessageType = JPJobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(decl))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var tariffFindBox = invoiceLineUserControl.FindSingle<TariffFindBox>("TariffFindBox");
				AssertEquals("TariffType: EXP", Universal.Constants.TariffTypes.Export, tariffFindBox.GetTariffType.Invoke());
			}
		}

		public void TestTariffColumnName()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclartion.Invoices.AddNew();
			header.InvoiceLines.AddNew();

			using var form = new JobDeclarationForm(jobDeclartion);
			form.Show();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
			AssertEquals(JobComInvoiceLine.Schema.JI_FormattedTariff, invoiceLineUserControl.TariffColumnName);
		}

		public void TestCustomsInvoiceLinesBoundGridNewColumns()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclartion.Invoices.AddNew();
			header.InvoiceLines.AddNew();

			using var form = new JobDeclarationForm(jobDeclartion);
			form.Show();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;

			var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
			var grid = invoiceLineUserControl.FindSingle<ZGrid>("CustomsInvoiceLinesBoundGrid");
			var tariffDescriptionColumn = grid.GetColumnStyle(nameof(JobComInvoiceLine.TariffDescription)) as ZTextBoxColumnStyleInfo;
			var naccsCodeColumn = grid.GetColumnStyle(nameof(JobComInvoiceLine.JI_NACCSCode)) as ZDropEditColumnStyleInfo;

			AssertNotNull(tariffDescriptionColumn);
			AssertNotNull(naccsCodeColumn);
			Assert(!naccsCodeColumn.IsMandatory);

			var entryInstructionColumn = grid.GetColumnStyle(nameof(JobComInvoiceLine.JI_CEI));
			var entryInstructionDescriptionColumn = grid.GetColumnStyle(nameof(JobComInvoiceLine.EntryInstructionDescription));

			AssertNotNull(entryInstructionColumn);
			AssertNotNull(entryInstructionDescriptionColumn);
			AssertEquals("Entry Instruction", entryInstructionColumn.GroupName.Caption);
			AssertEquals("Entry Instruction", entryInstructionDescriptionColumn.GroupName.Caption);

			var tariffColumn = grid.GetColumnStyle("JI_FormattedTariff");
			var tariffIndex = grid.ColumnStyles.IndexOf(tariffColumn);
			var descriptionIndex = grid.ColumnStyles.IndexOf(tariffDescriptionColumn);
			var naccsIndex = grid.ColumnStyles.IndexOf(naccsCodeColumn);
			Assert(tariffIndex == descriptionIndex - 1);
			Assert(descriptionIndex == naccsIndex - 1);

			AssertEquals(70, entryInstructionColumn.Width);
			AssertEquals(106, entryInstructionDescriptionColumn.Width);
		}

		public void TestCustomsInvoiceLinesBoundGridColumnsWidth()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var header = jobDeclartion.Invoices.AddNew();
			header.InvoiceLines.AddNew();

			using var form = new JobDeclarationForm(jobDeclartion);
			form.Show();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;

			var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
			var grid = invoiceLineUserControl.FindSingle<ZGrid>("CustomsInvoiceLinesBoundGrid");

			CombineAssertions(() =>
			{
				var invoiceQtyStyleInfo = grid.GetColumnStyle("JI_InvoiceQuantity");
				Assert("JI_InvoiceQuantity", invoiceQtyStyleInfo.Width >= 120);
				var customsQtyStyleInfo = grid.GetColumnStyle("JI_CustomsQuantity");
				Assert("JI_CustomsQuantity", customsQtyStyleInfo.Width >= 120);
			});
		}
	}
}
