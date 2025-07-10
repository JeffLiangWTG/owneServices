using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(ImportSupplierHeaderUserControl))]
	class ImportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ImportSupplierHeaderUserControl, JobDeclaration>
	{
		protected override IEnumerable<string> ExpectedControlList => DefaultControlList.Except(new[] { "GroupInvoiceDropEdit" });

		public void TestColumnLayoutContext()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				AssertEquals("Column layout context should have been set", nameof(Customs.GUI.DeclarationType.Import), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestGridId()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				AssertEquals("GridLayoutT+oYGAheR1633dT3oeP+lw==", control.InvoiceHeadersBoundGrid.GridId);
			}
		}

		public void TestGridColumns()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				control.InitializeGridLayout();
				var columns = control.JobComInvoiceHeadersBoundGrid.InnerGrid.ColumnStyles;
				CombineAssertions(() =>
				{
					AssertEquals("Should have JZ_InvoiceDisplaySequence column in JobComInvoiceHeadersBoundGrid", "JZ_InvoiceDisplaySequence", ((ZTextBoxColumnStyleInfo)control.JobComInvoiceHeadersBoundGrid.InnerGrid.ColumnStyles[0]).ColumnName);
					AssertEquals("Should have JZ_InvoiceType column in JobComInvoiceHeadersBoundGrid", "JZ_InvoiceType", ((ZTextBoxColumnStyleInfo)control.JobComInvoiceHeadersBoundGrid.InnerGrid.ColumnStyles[1]).ColumnName);
					AssertEquals("Should have JZ_InvoiceNumber column in JobComInvoiceHeadersBoundGrid", "JZ_InvoiceNumber", ((ZTextBoxColumnStyleInfo)control.JobComInvoiceHeadersBoundGrid.InnerGrid.ColumnStyles[2]).ColumnName);
					AssertEquals("Should have JZ_InvoiceDate column in JobComInvoiceHeadersBoundGrid", "JZ_InvoiceDate", ((ZTextBoxColumnStyleInfo)control.JobComInvoiceHeadersBoundGrid.InnerGrid.ColumnStyles[3]).ColumnName);
					AssertEquals("Should have JZ_InvoiceAmount column in JobComInvoiceHeadersBoundGrid", "JZ_InvoiceAmount", ((ZTextBoxColumnStyleInfo)control.JobComInvoiceHeadersBoundGrid.InnerGrid.ColumnStyles[4]).ColumnName);
					AssertEquals("Should have JZ_RX_NKInvoice_Currency column in JobComInvoiceHeadersBoundGrid", "JZ_RX_NKInvoice_Currency", ((ZTextBoxColumnStyleInfo)control.JobComInvoiceHeadersBoundGrid.InnerGrid.ColumnStyles[5]).ColumnName);
					AssertEquals("Should have JZ_IncoTerm column in JobComInvoiceHeadersBoundGrid", "JZ_IncoTerm", ((ZTextBoxColumnStyleInfo)control.JobComInvoiceHeadersBoundGrid.InnerGrid.ColumnStyles[6]).ColumnName);
					AssertEquals("Should have JZ_OH_Supplier column in JobComInvoiceHeadersBoundGrid", "JZ_OH_Supplier", ((ZTextBoxColumnStyleInfo)control.JobComInvoiceHeadersBoundGrid.InnerGrid.ColumnStyles[7]).ColumnName);
					AssertEquals("Should have JZ_Calc_BalanceString column in JobComInvoiceHeadersBoundGrid", "JZ_Calc_BalanceString", ((ZTextBoxColumnStyleInfo)control.JobComInvoiceHeadersBoundGrid.InnerGrid.ColumnStyles[8]).ColumnName);
				});
			}
		}
	}
}
