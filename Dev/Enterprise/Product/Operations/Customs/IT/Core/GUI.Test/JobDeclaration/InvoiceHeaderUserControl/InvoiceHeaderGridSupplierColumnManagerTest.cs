using System;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class InvoiceHeaderGridSupplierColumnManagerTest : TestCase
{
	public void TestGuardClause()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new InvoiceHeaderGridSupplierColumnManager(invoiceHeaderUserControl: null));
	}

	public void TestSupplierOrgPKIsNotVisible_Export()
	{
		using var control = new ExportSupplierHeaderUserControl();
		CheckSupplierOrgPKIsNotVisible(control);
	}

	public void TestSupplierColumns_Export()
	{
		using var control = new ExportSupplierHeaderUserControl();
		CheckSupplierColumnConfigurations(control);
	}

	public void TestSupplierOrgPKIsNotVisible_Import()
	{
		using var control = new ImportSupplierHeaderUserControl();
		CheckSupplierOrgPKIsNotVisible(control);
	}

	public void TestSupplierColumns_Import()
	{
		using var control = new ImportSupplierHeaderUserControl();
		CheckSupplierColumnConfigurations(control);
	}

	#region Implementation

	void CheckSupplierOrgPKIsNotVisible(EUCustomsSupplierHeaderUserControl control)
	{
		control.InitializeGridLayout();

		var invoicesGrid = control.JobComInvoiceHeadersBoundGrid;
		var columnsStyles = invoicesGrid.ColumnStyles.Cast<ZGridColumnInfo>();
		AssertNull("SupplierOrgPK ColumnStyle", columnsStyles.FirstOrDefault(x => x.ColumnName == "SupplierOrgPK"));
	}

	void CheckSupplierColumnConfigurations(EUCustomsSupplierHeaderUserControl control)
	{
		control.InitializeGridLayout();

		var invoicesGrid = control.JobComInvoiceHeadersBoundGrid;
		var columnsStyles = invoicesGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
		var supplierColumnStyle = columnsStyles.FirstOrDefault(x => x.ColumnName == "JZ_OH_Supplier");
		var supplierAddressColumnStyle = columnsStyles.FirstOrDefault(x => x.ColumnName == "JZ_OA_SupplierAddress");
		AssertNotNull("JZ_OH_Supplier ColumnStyle", supplierColumnStyle);
		AssertNotNull("JZ_OA_SupplierAddress ColumnStyle", supplierAddressColumnStyle);

		CombineAssertions(() =>
		{
			AssertSupplierColumn(supplierColumnStyle, expectedIndex: 2);
			AssertSupplierColumn(supplierAddressColumnStyle, expectedIndex: 3);
		});

		void AssertSupplierColumn(ZGridColumnInfo column, int expectedIndex)
		{
			AssertEquals($"{column.ColumnName} GroupName", "Supplier", column.GroupName.Caption);
			AssertEquals($"{column.ColumnName} IsMandatory", true, column.IsMandatory);
			AssertEquals($"{column.ColumnName} IsVisible", true, column.IsVisible);
			AssertEquals($"{column.ColumnName} Index", expectedIndex, invoicesGrid.ColumnStyles.IndexOf(column));
		}
	}

	#endregion
}
