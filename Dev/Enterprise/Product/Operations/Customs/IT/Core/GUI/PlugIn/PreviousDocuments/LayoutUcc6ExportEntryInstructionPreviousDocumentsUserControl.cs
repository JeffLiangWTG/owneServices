using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

class LayoutUcc6ExportEntryInstructionPreviousDocumentsUserControl : EU.GUI.PlugIn.LayoutPreviousDocumentsUserControl, ISupportMultipleResourceStringDataSupporter, ISupportMultipleResourceStringData
{
	protected override IGridColumnLayoutProvider GetGridColumnLayoutProvider() => new Ucc6ExportEntryInstructionPreviousDocumentGridColumnLayout();

	ISupportMultipleResourceStringData ISupportMultipleResourceStringDataSupporter.SupportMultipleResourceStringData => this;

	IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => new[] { PreviousDocument.Ucc6ExportEntryInstructionResourceStringKey };
}
