using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class EntryInstructionLayoutPreviousDocumentsUserControl : EU.GUI.PlugIn.LayoutPreviousDocumentsUserControl, ISupportMultipleResourceStringDataSupporter, ISupportMultipleResourceStringData
{
	public EntryInstructionLayoutPreviousDocumentsUserControl()
	{
		InitializeComponent();
	}

	protected override IGridColumnLayoutProvider GetGridColumnLayoutProvider() => new EntryInstructionPreviousDocumentGridColumnLayout();

	ISupportMultipleResourceStringData ISupportMultipleResourceStringDataSupporter.SupportMultipleResourceStringData => this;

	IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => new[] { PreviousDocument.EntryInstructionResourceStringKey };
}
