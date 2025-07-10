using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class EntryInstructionDetailsUserControl : EU.GUI.EntryInstructionDetailsUserControl
{
	public EntryInstructionDetailsUserControl()
	{
		InitializeComponent();
	}

	protected override void ChangeControlsVisibility()
	{
		base.ChangeControlsVisibility();

		SupportingDocumentsTabPage.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("39713E53-8AB9-4473-B098-4F60DA0718AE", "[44] Supporting Documents");
		PreviousDocumentsTabPage.CaptionResourceString = GetPreviousDocumentsTabPageCaption();
	}

	protected override Type GetGridUserControl() => typeof(EntryInstructionGridUserControl);

	protected override Type GetDetailsUserControlType() => typeof(LayoutEntryInstructionDetailBasicUserControl);

	protected override Type GetGuaranteesUserControlType() => typeof(EntryInstructionGuaranteesUserControl);

	protected override Type GetSupportingDocumentsUserControlType()
	{
		var declaration = CurrentDataItem;
		if (declaration is null || !(declaration.IsExport || declaration.IsImport))
		{
			return null;
		}

		return declaration.IsExport
			? typeof(ExportEntryInstructionLayoutSupportingDocumentsUserControl)
			: typeof(ImportEntryInstructionLayoutSupportingDocumentsUserControl);
	}

	protected override Type GetPreviousDocumentsUserControlType()
		=> IsUCC6AndIsExport ? typeof(LayoutUcc6ExportEntryInstructionPreviousDocumentsUserControl) : typeof(EntryInstructionLayoutPreviousDocumentsUserControl);

	protected override ResourceStringData GetAdditionalInfosTabCaption() => Res.GetData("e30b8bc7-46c0-4590-a1dc-afde2b9e8acd", "Additional Documents");

	ResourceStringData GetPreviousDocumentsTabPageCaption()
	{
		if (IsUCC6AndIsExport)
		{
			return Res.GetData("08165089-7FDA-4DE9-ACD9-50A7C5939D24", "Previous Documents");
		}
		return Res.GetData("D5988E14-0034-4C48-A999-B85CA75E08F3", "[40] Previous Documents");
	}
}
