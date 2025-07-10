using System;

namespace Enterprise.Customs.IT.GUI;

public partial class ImportSupplierHeaderUserControl : EU.GUI.EUImportSupplierHeaderUserControl
{
	public ImportSupplierHeaderUserControl()
	{
		InitializeComponent();
	}

	protected override Type GetSupportingDocumentsUserControlType() => typeof(InvoiceLayoutSupportingDocumentsUserControl);

	protected override Type GetPreviousDocumentsUserControlType() => typeof(LayoutPreviousDocumentsUserControl);

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();
		new InvoiceHeaderGridSupplierColumnManager(this).SetUpSupplierColumns();
	}
}
