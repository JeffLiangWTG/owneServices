using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.BE.GUI;

public partial class CustomsBrokerageUserControl : EU.GUI.CustomsBrokerageUserControl
{
	public CustomsBrokerageUserControl()
	{
		InitializeComponent();
	}

	protected override BaseCustomsEntryUserControl GetDeclarationUserControl()
	{
		return new JobDeclarationUserControl();
	}

	protected override BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
	{
		if (JobDeclaration.IsImport)
		{
			return new ImportSupplierHeaderUserControl();
		}
		else
		{
			return new ExportSupplierHeaderUserControl();
		}
	}

	protected override BaseCustomsEntryUserControl GetMessageUserControl() => new EntryMessageUserControl();

	protected override BaseCustomsEntryUserControl GetEntryInstructionUserControl() => new EntryInstructionDetailsUserControl((JobDeclaration)JobDeclaration);

	protected override void RemoveUserControlOfEachTabPage()
	{
		base.RemoveUserControlOfEachTabPage();
		RemoveControl(EntryInstructionDetailsTabPage);
	}

	protected override BaseCustomsCusContainersUserControl GetContainerUserControl()
	{
		return new ContainerUserControl();
	}

	#region Create New User Controls for each tab

	protected override BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
	{
		BaseInvoiceLineUserControl result;

		if (JobDeclaration.IsImport)
		{
			result = new ImportInvoiceLineUserControl();
		}
		else
		{
			result = new ExportInvoiceLineUserControl();
		}

		return result;
	}

	#endregion
}
