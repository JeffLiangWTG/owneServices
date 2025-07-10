using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.GUI;

public partial class CustomsBrokerageUserControl : EU.GUI.CustomsBrokerageUserControl
{
	public CustomsBrokerageUserControl()
	{
		InitializeComponent();
	}

	JobDeclaration ITJobDeclaration => (JobDeclaration)JobDeclaration;

	protected override Customs.GUI.BaseCustomsEntryUserControl GetDeclarationUserControl() => new JobDeclarationUserControl();

	protected override Customs.GUI.BaseCustomsEntryUserControl GetEntryInstructionUserControl() => new EntryInstructionDetailsUserControl();

	protected override Customs.GUI.BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
	{
		if (ITJobDeclaration.IsImport)
		{
			return new ImportInvoiceLineUserControl();
		}

		return new ExportInvoiceLineUserControl();
	}

	protected override Customs.GUI.BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
	{
		if (ITJobDeclaration.IsImport)
		{
			return new ImportSupplierHeaderUserControl();
		}

		return new ExportSupplierHeaderUserControl();
	}

	protected override Customs.GUI.BaseCustomsEntryUserControl GetMessageUserControl() => new MessageUserControl();

	protected override void OnJobDeclarationSet()
	{
		base.OnJobDeclarationSet();
		if (ITJobDeclaration != null)
		{
			ITJobDeclaration.MessageVersionInfo.ValueChanged += MessageVersionInfo_ValueChanged;
		}
	}

	void MessageVersionInfo_ValueChanged(object sender, System.EventArgs e)
	{
		RemoveUserControlOfEachTabPage();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && ITJobDeclaration != null)
		{
			ITJobDeclaration.MessageVersionInfo.ValueChanged -= MessageVersionInfo_ValueChanged;
		}
		base.Dispose(disposing);
	}
}
