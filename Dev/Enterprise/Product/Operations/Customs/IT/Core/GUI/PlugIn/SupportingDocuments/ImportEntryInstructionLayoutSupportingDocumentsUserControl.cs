using System;
using System.Collections.Generic;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.GUI;

public partial class ImportEntryInstructionLayoutSupportingDocumentsUserControl : EU.GUI.PlugIn.InvoiceLayoutSupportingDocumentsUserControl
{
	public ImportEntryInstructionLayoutSupportingDocumentsUserControl()
	{
		InitializeComponent();
		InitializeGridLayout();
	}

	protected override EU.GUI.PlugIn.LayoutSupportingDocumentsFieldsControl GetSupportingDocumentsFieldsControl(EU.Business.Declaration.JobDeclaration declaration)
		=> new ImportEntryInstructionLayoutSupportingDocumentsFieldsControl(declaration);

	protected override IReadOnlyList<string> AvailableColumnNames => GetAvailableColumnNames();

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();
		new SupportingDocumentsGridInitializer(SupportingDocumentsGrid).Initialize();
	}

	string[] GetAvailableColumnNames()
	{
		if (CurrentDataItem is JobDeclaration declaration)
		{
			return new SupportingDocumentsGridColumnStylesHelper()
				.GetColumnStyles(declaration);
		}

		return Array.Empty<string>();
	}
}
