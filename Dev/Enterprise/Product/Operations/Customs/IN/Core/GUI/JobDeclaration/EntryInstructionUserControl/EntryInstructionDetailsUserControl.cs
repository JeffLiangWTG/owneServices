using System;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public partial class EntryInstructionDetailsUserControl : BaseCustomsEntryUserControl
{
	public EntryInstructionDetailsUserControl()
	{
		InitializeComponent();

		Tuple<ZTabPage, ZDynamicControlCreationUserControl, Type>[] dynamicControlConfigs =
		[
			Tuple.Create(EntryInstructionDetailsTabPage, DetailsUserControl, typeof(LayoutEntryInstructionDetailBasicUserControl)),
			Tuple.Create(EntryInstructionOtherPartiesTabPage, OtherPartiesUserControl, typeof(EntryInstructionOtherPartiesUserControl)),
			Tuple.Create(SupportingDocumentTabPage, SupportingDocumentUserControl, typeof(LayoutSupportingDocumentsUserControl)),
			Tuple.Create(EntryInstructionContainerTabPage, ContainerUserControl, typeof(EntryInstructionContainersUserControl)),
			Tuple.Create(EntryInstructionSWControlsTabPage, SWControlsUserControl, typeof(SWControlsUserControl))
		];
		foreach (var config in dynamicControlConfigs)
		{
			config.Item1.RunWhenBindingOrFirstShown((_, _) => config.Item2.UserControlType = config.Item3);
		}
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		EntryInstructionTopPanelUserControl.UserControlType = typeof(EntryInstructionTopPanelUserControl);
	}

	protected override void ChangeControlsVisibility()
	{
		base.ChangeControlsVisibility();
		EntryInstructionContainerTabPage.TabVisible = JobDeclaration?.IsExport ?? false;
		EntryInstructionSWControlsTabPage.TabVisible = JobDeclaration?.IsExport ?? false;
	}
}
