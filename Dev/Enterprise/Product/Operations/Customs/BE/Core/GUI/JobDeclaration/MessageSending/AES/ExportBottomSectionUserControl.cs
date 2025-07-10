using Enterprise.Customs.BE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public partial class ExportBottomSectionUserControl : ZUserControl
{
	public ExportBottomSectionUserControl()
	{
		InitializeComponent();
		AfterFirstBinding += MessageSendingFormBottomSectionUserControl_AfterFirstBinding;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			components?.Dispose();
			if (dataSource != null)
			{
				foreach (ExportEntryMessageSendingAction action in dataSource.SendingObjectsCollection)
				{
					action.TypeOfEntryInfo.ValueChanged -= TypeOfEntryInfo_ValueChanged;
				}
			}
		}
		base.Dispose(disposing);
	}

	void MessageSendingFormBottomSectionUserControl_AfterFirstBinding(object sender, System.EventArgs e)
	{
		dataSource = (ExportDeclarationMessageSendingActionParent)DataSource;
		foreach (ExportEntryMessageSendingAction action in dataSource.SendingObjectsCollection)
		{
			action.TypeOfEntryInfo.ValueChanged += TypeOfEntryInfo_ValueChanged;
			TypeOfEntryChanged(action);
		}
	}
	ExportDeclarationMessageSendingActionParent dataSource;

	void TypeOfEntryInfo_ValueChanged(object sender, System.EventArgs e)
	{
		TypeOfEntryChanged((ExportEntryMessageSendingAction)sender);
	}

	internal void TypeOfEntryChanged(ExportEntryMessageSendingAction action)
	{
		SetControlsVisibility(action);
	}

	void SetControlsVisibility(ExportEntryMessageSendingAction action)
	{
		var isTypeOfEntryCan = false;
		if (action != null)
		{
			var actionEntryType = action.TypeOfEntry;
			isTypeOfEntryCan = actionEntryType == BEExportEntryTypeList.Codes.CancellationRequest;
		}
		JustificationGroupBox.Visible = isTypeOfEntryCan;
		AlternativeEvidenceGroupBox.Visible = !isTypeOfEntryCan;
	}
}
