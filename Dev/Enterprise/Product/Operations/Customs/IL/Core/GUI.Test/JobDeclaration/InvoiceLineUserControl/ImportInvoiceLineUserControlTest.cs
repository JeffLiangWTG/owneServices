using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IL.GUI.Testing
{
	sealed class ImportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestColumnLayoutContextForInvoiceLinesGrid()
		{
			using (var control = new ImportInvoiceLineUserControl())
			{
				AssertEquals("Column layout context should be import", nameof(Customs.GUI.DeclarationType.Import), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestInvoiceLineGrid_DefaultColumns()
		{
			var columns = invLinesForm.CustomsInvoiceLinesBoundGrid.Columns;
			CombineAssertions(() =>
			{
				var i = 0;
				foreach (var columnName in ExpectedInvoiceLineGrid_DefaultColumns)
				{
					AssertDefaultColumn(columnName, columns, i++);
				}
			});
		}

		public void TestSetupGridColumns()
		{
			TestColumnStyle(JobComInvoiceLine.Schema.JI_CountryOfOrigin, 45, true, "Origin", isCaption: true);
			TestColumnStyle(JobComInvoiceLine.Schema.JI_PrimaryPreference, 80, true, "Preference");

			TestColumnStyle(JobComInvoiceLine.Schema.JI_PreferenceDocNumber, 80, false, "Preference Doc.#");
			TestColumnStyle(JobComInvoiceLine.Schema.JI_CustomsSecondQuantity, 120, false, "Statistical Qty");
			TestColumnStyle(JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty, 30, false, "Statistical UQ");
			TestColumnStyle(JobComInvoiceLine.Schema.JI_CustomsThirdQuantity, 100, false, "Additional Qty");
			TestColumnStyle(JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty, 30, false, "Additional UQ");
			TestColumnStyle(JobComInvoiceLine.Schema.JI_BondedWhsQuantity, 100, false, "Countable Qty");
			TestColumnStyle(JobComInvoiceLine.Schema.JI_BondedWhsUnitQty, 30, false, "Countable UQ");
			TestColumnStyle(JobComInvoiceLine.Schema.JI_ZZF_NKTaxType, 100, false, "VAT Code");
		}

		public void TestUniversalTariffType()
		{
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				AssertEquals("Tariff Type should be IMP", "IMP", control.UniversalTariffTypeExposed);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			decForm = new JobDeclarationForm(declaration);
			decForm.Show();
			decForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = decForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			invLinesForm = (ImportInvoiceLineUserControl)decForm.CustomsBrokerageUserControl.InvoiceLinesUserControl;
		}

		protected override void TearDown()
		{
			decForm?.Dispose();
			base.TearDown();
		}

		void AssertDefaultColumn(string expectedName, ZGridColumns columns, int index)
		{
			var column = columns[index];
			AssertEquals(index.ToString(), expectedName, column.ColumnStyle.MappingName);
			AssertEquals(expectedName, expected: true, column.IsVisible);
		}

		IEnumerable<string> ExpectedInvoiceLineGrid_DefaultColumns
		{
			get
			{
				yield return JobComInvoiceLine.Schema.JI_LineNo;
				yield return JobComInvoiceLine.Schema.JI_Calc_Invoice;
				yield return JobComInvoiceLine.Schema.JI_PartNo;
				yield return JobComInvoiceLine.Schema.JI_Tariff;
				yield return JobComInvoiceLine.Schema.JI_InvoiceQuantity;
				yield return JobComInvoiceLine.Schema.JI_InvoiceUQ;
				yield return JobComInvoiceLine.Schema.JI_CustomsQuantity;
				yield return JobComInvoiceLine.Schema.JI_CustomsUnitQty;
				yield return JobComInvoiceLine.Schema.JI_LinePrice;
				yield return JobComInvoiceLine.Schema.JI_CountryOfOrigin;
				yield return JobComInvoiceLine.Schema.JI_PrimaryPreference;
			}
		}

		void TestColumnStyle(string columnName, int expectedWidth, bool expectedVisible, string expectedCaption, bool isCaption = false)
		{
			var column = invLinesForm.CustomsInvoiceLinesBoundGrid.GetColumnStyle(columnName);
			CombineAssertions(columnName, () =>
			{
				AssertNotNull(column);
				AssertEquals(expected: expectedWidth, column.Width);
				AssertEquals(expected: expectedVisible, column.IsVisible);
				AssertEquals(expected: expectedCaption, isCaption ? column.Caption : column.CaptionResourceString.Caption);
			});
		}

		JobDeclarationForm decForm;
		ImportInvoiceLineUserControl invLinesForm;
	}

	class ImportInvoiceLineUserControlForTest : ImportInvoiceLineUserControl
	{
		public ZString UniversalTariffTypeExposed => UniversalTariffType;
	}
}
