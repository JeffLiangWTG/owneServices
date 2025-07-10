using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.JP.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(ImportSupplierHeaderUserControl))]
	sealed class ImportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ImportSupplierHeaderUserControl, JobDeclaration>
	{
		protected override IEnumerable<string> ExpectedControlList => DefaultControlList;

		public void TestColumnLayoutContext()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				AssertEquals("Column layout context should have been set", nameof(Customs.GUI.DeclarationType.Import), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestAddtionalDetails()
		{
			using var control = new ImportSupplierHeaderUserControl();
			CombineAssertions(() =>
			{
				AssertNotNull("ValuationTypeDropEdit", control.FindSingleOrDefault<ZDropEdit>("ValuationTypeDropEdit"));
				AssertNotNull("ComprehensiveValuationGroupBox", control.FindSingleOrDefault<ZGroupBox>("ComprehensiveValuationGroupBox"));
				AssertNotNull("ComprehensiveInsuranceNumberTextBox", control.FindSingleOrDefault<ZTextBox>("ComprehensiveInsuranceNumberTextBox"));
				AssertNotNull("InsuranceTypeDropEdit", control.FindSingleOrDefault<ZDropEdit>("InsuranceTypeDropEdit"));
				AssertNotNull("FreightTypeDropEdit", control.FindSingleOrDefault<ZDropEdit>("FreightTypeDropEdit"));
				AssertNotNull("AdvanceRulingonValuation1TextBox", control.FindSingle<ZTextBox>("AdvanceRulingonValuation1TextBox"));
				AssertNotNull("AdvanceRulingonValuation2TextBox", control.FindSingle<ZTextBox>("AdvanceRulingonValuation2TextBox"));
			});
		}

		public void TestJobComInvoiceHeadersBoundGrid()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				var grid = control.JobComInvoiceHeadersBoundGrid;
				var columns = grid.ColumnStyles.ToList<ZGridColumnInfo>();

				var valuationDateColumn = columns.Find(x => x.ColumnName == "JZ_ValuationDateOverride");
				AssertNotNull(valuationDateColumn);

				var valuationCodeColumn = columns.Find(x => x.ColumnName == "JZ_ValuationCode");
				AssertNotNull(valuationCodeColumn);
			}
		}

		public void TestComprehensiveValuationsGrid()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				var grid = control.FindSingle<ZGrid>("ComprehensiveValuationsGrid");
				var columns = grid.ColumnStyles.ToList<ZGridColumnInfo>();
				var valuationDateColumn = columns.Find(x => x.ColumnName == "CFR_Reference");
				AssertNotNull(valuationDateColumn);
				AssertEquals("MaximumRows of ComprehensiveValuationsGrid must be equal to ComprehensiveValuationCollection.MaxRowCount", ComprehensiveValuationCollection.MaxRowCount, grid.MaximumRows);
			}
		}
	}
}
