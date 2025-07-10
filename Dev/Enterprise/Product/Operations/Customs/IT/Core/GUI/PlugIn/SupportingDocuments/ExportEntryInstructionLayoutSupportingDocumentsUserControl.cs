using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IT.GUI;

public partial class ExportEntryInstructionLayoutSupportingDocumentsUserControl : EU.GUI.PlugIn.InvoiceLayoutSupportingDocumentsUserControl
{
	public ExportEntryInstructionLayoutSupportingDocumentsUserControl()
	{
		InitializeComponent();
		InitializeGridLayout();
	}

	protected override EU.GUI.PlugIn.LayoutSupportingDocumentsFieldsControl GetSupportingDocumentsFieldsControl(EU.Business.Declaration.JobDeclaration declaration)
		=> new ExportEntryInstructionLayoutSupportingDocumentsFieldsControl(declaration);

	protected override IReadOnlyList<string> AvailableColumnNames => new[]
	{
		nameof(SupportingDocument.CSI_Code),
		nameof(SupportingDocument.CSI_ReferenceNumber),
		nameof(SupportingDocument.CSI_YearOfIssue),
		nameof(SupportingDocument.CSI_RN_NKCountryCode),
		nameof(SupportingDocument.CSI_ReferenceNumber2),
		nameof(SupportingDocument.CSI_DateOfExpiry),
		nameof(SupportingDocument.CSI_LineNo)
	};

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();

		SupportingDocumentsGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
		{
			ColumnName = nameof(SupportingDocument.CSI_YearOfIssue),
			Width = 100,
			CharacterCasing = CharacterCasing.Upper,
		});
	}
}
