using CargoWise.Common;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IT.GUI;

sealed class InvoiceHeaderGridSupplierColumnManager
{
	public InvoiceHeaderGridSupplierColumnManager(EUCustomsSupplierHeaderUserControl invoiceHeaderUserControl)
	{
		Argument.NotNull(invoiceHeaderUserControl, nameof(invoiceHeaderUserControl));
		invoicesGrid = Argument.NotNull(invoiceHeaderUserControl.InvoiceHeadersBoundGrid, nameof(invoiceHeaderUserControl.InvoiceHeadersBoundGrid));
	}

	public void SetUpSupplierColumns()
	{
		RemoveSupplierForBindingOnlyColumn();

		var supplierColumn = invoicesGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_OH_Supplier);
		var supplierAddressColumn = invoicesGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_OA_SupplierAddress);
		if (supplierColumn != null && supplierAddressColumn != null)
		{
			RepositionSupplierAddressColumn(supplierColumn, supplierAddressColumn);
			CustomizeColumnsConfig(supplierColumn, supplierAddressColumn);
		}
	}

	void RemoveSupplierForBindingOnlyColumn()
	{
		var column = invoicesGrid.GetColumnStyle(JobComInvoiceHeader.Schema.SupplierOrgPK);
		if (column != null)
		{
			invoicesGrid.ColumnStyles.Remove(column);
		}
	}

	void RepositionSupplierAddressColumn(ZGridColumnInfo supplierColumn, ZGridColumnInfo supplierAddressColumn)
	{
		invoicesGrid.ColumnStyles.Remove(supplierAddressColumn);
		invoicesGrid.ColumnStyles.Insert(invoicesGrid.ColumnStyles.IndexOf(supplierColumn) + 1, supplierAddressColumn);
	}

	void CustomizeColumnsConfig(ZGridColumnInfo supplierColumn, ZGridColumnInfo supplierAddressColumn)
	{
		supplierColumn.GroupName = supplierAddressColumn.GroupName;
		supplierColumn.IsMandatory = supplierAddressColumn.IsMandatory = true;
		supplierColumn.IsVisible = supplierAddressColumn.IsVisible = true;
	}

	readonly ZGrid invoicesGrid;
}
