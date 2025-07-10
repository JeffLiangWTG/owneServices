using System;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.GUI;

public partial class ImportEntryLineAdditionalDataUserControl : EU.GUI.EntryLineAdditionalDataUserControl
{
	public ImportEntryLineAdditionalDataUserControl()
	{
		InitializeComponent();
		SetUpEntryLineSupportingDocumentsGridColumns();
	}

	protected override Type GetDutyAndTaxDetailsUserControlType() => typeof(ImportEntryLineTaxAndFeeUserControl);

	void SetUpEntryLineSupportingDocumentsGridColumns()
	{
		EntryLineSupportingDocumentsGrid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
		{
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = ReadOnlySupportingDocument.Schema.CSI_Procedure,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90)
			},
			new ZArchitecture.ZCheckBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
				ColumnName = ReadOnlySupportingDocument.Schema.IsDocumentHeader,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60)
			}
		});
	}

	protected JobDeclaration Declaration => (JobDeclaration)CurrentDataItem;

	void ChangeGridColumnsVisibility()
	{
		EntryLinePreviousDocumentsGrid.SetAvailability(Declaration?.IsUCC6 ?? false, ReadOnlyPreviousDocument.Schema.CSI_PackQty);
		EntryLinePreviousDocumentsGrid.SetAvailability(Declaration?.IsUCC6 ?? false, ReadOnlyPreviousDocument.Schema.CSI_PackType);
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		ChangeGridColumnsVisibility();
	}
}
