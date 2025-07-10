using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.GUI.PlugIn;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.BE.GUI;

partial class EntryInstructionDetailsUserControl : EU.GUI.EntryInstructionDetailsUserControl
{
	public EntryInstructionDetailsUserControl(JobDeclaration declaration)
	{
		JobDeclaration = declaration;
		InitializeComponent();
		FixTabPageOrder();
	}

	protected override Type GetGridUserControl() => typeof(EntryInstructionGridUserControl);

	protected override Type GetDetailsUserControlType() => typeof(LayoutEntryInstructionDetailBasicUserControl);

	protected override Type GetSupportingDocumentsUserControlType() => typeof(EntryInstructionSupportingDocumentsUserControl);

	protected override Type GetPreviousDocumentsUserControlType() => typeof(EntryInstructionPreviousDocumentsUserControl);

	protected override ResourceStringData GetAdditionalInfosTabCaption() => EntryInstructionDetailsUserControlHelper.AdditionalInfosTabCaption;

	protected override Type GetGuaranteesUserControlType() => typeof(EntryInstructionGuaranteesUserControl);

	protected override void SetTabPagesVisibilityCore()
	{
		base.SetTabPagesVisibilityCore();
		FixTabPageOrder();
	}

	void FixTabPageOrder()
	{
		EntryInstructionTabControl.TabPages.Remove(SupportingDocumentsTabPage);
		EntryInstructionTabControl.TabPages.Remove(AdditionalInfoTabPage);
		EntryInstructionTabControl.TabPages.Remove(PreviousDocumentsTabPage);
		EntryInstructionTabControl.TabPages.Remove(GuaranteesTabPage);
		EntryInstructionTabControl.TabPages.Remove(SupplyChainActorReferencesTabPage);
		EntryInstructionTabControl.TabPages.Remove(AuthorisationsTabPage);
		EntryInstructionTabControl.TabPages.Add(SupportingDocumentsTabPage);
		EntryInstructionTabControl.TabPages.Add(AdditionalInfoTabPage);
		EntryInstructionTabControl.TabPages.Add(PreviousDocumentsTabPage);
		if (JobDeclaration.IsImport)
		{
			EntryInstructionTabControl.TabPages.Add(GuaranteesTabPage);
		}
		EntryInstructionTabControl.TabPages.Add(SupplyChainActorReferencesTabPage);
		EntryInstructionTabControl.TabPages.Add(AuthorisationsTabPage);
	}
}
