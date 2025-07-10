using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.ES.GUI;

internal partial class EntryInstructionDetailsUserControl : EU.GUI.EntryInstructionDetailsUserControl
{
	public EntryInstructionDetailsUserControl()
	{
		InitializeComponent();
	}

	protected override Type GetGridUserControl() => typeof(EntryInstructionGridUserControl);

	protected override Type GetDetailsUserControlType() => typeof(LayoutEntryInstructionDetailBasicUserControl);

	protected override ResourceStringData GetAdditionalInfosTabCaption() => Res.GetData("D3C90919-96C8-4B58-A659-809F596B101B", "Additional Documents");

	protected override Type GetAdditionalInfosUserControlType() => JobDeclaration.IsImport ? typeof(AdditionalInfosUserControlWithGrid) : base.GetAdditionalInfosUserControlType();

	protected override Type GetSupportingDocumentsUserControlType() => CurrentDataItem.IsExport ? typeof(ExportSupportingDocumentsUserControl) : typeof(ImportSupportingDocumentsUserControl);

	protected override Type GetPreviousDocumentsUserControlType() => typeof(LayoutPreviousDocumentsUserControl);
}
