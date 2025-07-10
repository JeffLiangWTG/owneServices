using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class InvoiceLineImportLayoutPreviousDocumentsUserControl : EU.GUI.PlugIn.LayoutPreviousDocumentsUserControl, ISupportMultipleResourceStringDataSupporter, ISupportMultipleResourceStringData
{
	public InvoiceLineImportLayoutPreviousDocumentsUserControl()
	{
		InitializeComponent();
	}

	protected override IGridColumnLayoutProvider GetGridColumnLayoutProvider() => new InvoiceLineImportPreviousDocumentGridColumnLayout();

	ISupportMultipleResourceStringData ISupportMultipleResourceStringDataSupporter.SupportMultipleResourceStringData => this;

	IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => new[] { PreviousDocument.InvoiceLineImportResourceStringKey };
}
