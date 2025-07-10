using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(EntryInstructionDetailsUserControl))]
sealed class EntryInstructionDetailsUserControlTest : TestCaseWithFactory
{
	public void TestDynamicControlType()
	{
		Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
		Declaration.JE_TransportMode = Constants.TransportModes.Sea;

		using var form = new ZForm();
		using var control = new EntryInstructionDetailsUserControl();
		control.JobDeclaration = Declaration;
		form.Controls.Add(control);
		form.Show();

		AssertEquals(typeof(EntryInstructionTopPanelUserControl), control.FindSingle<ZDynamicControlCreationUserControl>("EntryInstructionTopPanelUserControl").UserControlType);

		Tuple<string, string, Type>[] dynamicControlConfigs =
		[
			Tuple.Create("EntryInstructionDetailsTabPage", "DetailsUserControl", typeof(LayoutEntryInstructionDetailBasicUserControl)),
			Tuple.Create("EntryInstructionOtherPartiesTabPage", "OtherPartiesUserControl", typeof(EntryInstructionOtherPartiesUserControl)),
			Tuple.Create("SupportingDocumentTabPage", "SupportingDocumentUserControl", typeof(LayoutSupportingDocumentsUserControl)),
			Tuple.Create("EntryInstructionContainerTabPage", "ContainerUserControl", typeof(EntryInstructionContainersUserControl)),
			Tuple.Create("EntryInstructionSWControlsTabPage", "SWControlsUserControl", typeof(SWControlsUserControl))
		];
		foreach (var config in dynamicControlConfigs)
		{
			control.EntryInstructionTabControl.SelectTab(config.Item1);
			AssertEquals(config.Item3, control.FindSingle<ZDynamicControlCreationUserControl>(config.Item2).UserControlType);
		}
	}

	public void TestChangeControlsVisibility()
	{
		CombineAssertions(() =>
		{
			using (var control = new EntryInstructionDetailsUserControl())
			{
				Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				control.JobDeclaration = Declaration;
				AssertEquals("Display when export", true, control.EntryInstructionContainerTabPage?.TabVisible ?? false);
				AssertEquals("Display when export", true, control.EntryInstructionSWControlsTabPage?.TabVisible ?? false);

				Declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				AssertEquals("Hidden when import", false, control.EntryInstructionContainerTabPage?.TabVisible ?? false);
				AssertEquals("Hidden when import", false, control.EntryInstructionSWControlsTabPage?.TabVisible ?? false);
			}
		});
	}

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;
}
