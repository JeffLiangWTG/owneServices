using Enterprise.Customs.GUI;

namespace Enterprise.Customs.IN.GUI;

public partial class CustomsBrokerageUserControl : BaseCustomsBrokerageUserControl
{
	public CustomsBrokerageUserControl()
	{
		InitializeComponent();
	}

	protected override BaseCustomsEntryUserControl GetDeclarationUserControl() => new JobDeclarationUserControl();

	protected override Customs.GUI.BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
	{
		Customs.GUI.BaseCustomsSupplierHeaderUserControl result;

		if (JobDeclaration.IsImport)
		{
			result = new ImportSupplierHeaderUserControl();
		}
		else
		{
			result = new ExportSupplierHeaderUserControl();
		}

		return result;
	}

	protected override Customs.GUI.BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
	{
		Customs.GUI.BaseInvoiceLineUserControl result;

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

	public override bool EntryInstructionsTabVisibleForCountry => true;

	protected override BaseMiscOptionsUserControl GetMiscOptionsUserControl() => new MiscOptionsUserControl();

	protected override BaseCustomsCusContainersUserControl GetContainerUserControl() => new ContainerUserControl();

	protected override BaseCustomsEntryUserControl GetEntryInstructionUserControl() => new EntryInstructionDetailsUserControl();

	protected override BaseCustomsEntryUserControl GetMessageUserControl() => JobDeclaration.IsDeclarationIntegrated ? base.GetMessageUserControl() : new CustomsEntryAndDiscardedMessagesUserControl(JobDeclaration);
}
