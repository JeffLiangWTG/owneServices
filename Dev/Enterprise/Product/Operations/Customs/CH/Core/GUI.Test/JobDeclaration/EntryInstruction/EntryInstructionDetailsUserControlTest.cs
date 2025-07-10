using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(EntryInstructionDetailsUserControl))]
sealed class EntryInstructionDetailsUserControlTest : TestCaseWithFactory
{
	public void TestEntryInstructionGrid_Import() => AssertEntryInstructionGrid(Common.Shared.SharedJobMessageTypeList.Codes.Import, string.Empty, importControls);

	public void TestEntryInstructionGrid_Export() => AssertEntryInstructionGrid(Common.Shared.SharedJobMessageTypeList.Codes.Export, string.Empty, exportControls);

	public void TestEntryInstructionGrid_EDAPassar() => AssertEntryInstructionGrid(CHJobMessageTypeList.Codes.ExportDeclarationActivation, ActivationTypeList.Codes.Passar, edaControls);

	void AssertEntryInstructionGrid(string messageType, string messageSubType, IEnumerable<(string columnName, CharacterCasing? columnCharacterCasing)> availableControls)
	{
		Declaration.JE_MessageType = messageType;
		Declaration.JE_MessageSubType = messageSubType;

		using (var form = new ZForm(Declaration))
		using (var parent = new CustomsBrokerageUserControl())
		using (var control = new EntryInstructionDetailsUserControl())
		{
			parent.Controls.Add(control);
			form.Controls.Add(parent);
			form.Show();
			parent.MainTabControl.SelectedTab = parent.EntryInstructionDetailsTabPage;

			var entryInstructionsGrid = control.EntryInstructionsGrid;
			var availableGridColumns = availableControls;

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder($"Correct Columns for {messageType}", availableGridColumns.Select(x => x.columnName).ToArray(), entryInstructionsGrid.Columns.GetVisibleColumnMappingNames());

				foreach (var column in availableGridColumns)
				{
					var columnInfo = entryInstructionsGrid.GetColumnStyle(column.columnName);
					AssertEquals($"{column.columnName} column character case", column.columnCharacterCasing, columnInfo.CharacterCasing);
					AssertEquals($"{column.columnName} column visibility", true, columnInfo.IsVisible);
				}
			});
		}
	}

	public void TestTabPagesVisibilityAndOrder_Import() => AssertTabPagesVisibilityAndOrder(CHJobMessageTypeList.Codes.Import);

	public void TestTabPagesVisibilityAndOrder_Export() => AssertTabPagesVisibilityAndOrder(CHJobMessageTypeList.Codes.Export);

	public void TestTabPagesVisibilityAndOrder_EDA() => AssertTabPagesVisibilityAndOrder(CHJobMessageTypeList.Codes.ExportDeclarationActivation);

	void AssertTabPagesVisibilityAndOrder(string messageType)
	{
		Declaration.JE_MessageType = messageType;

		using (var form = new ZForm(Declaration))
		using (var parent = new CustomsBrokerageUserControl())
		using (var control = new EntryInstructionDetailsUserControl())
		{
			parent.Controls.Add(control);
			form.Controls.Add(parent);
			form.Show();

			CombineAssertions(() =>
			{
				var expectedTabPagesInOrder = GetExpectedTabPagesInOrder(control, messageType);

				foreach (var expectedTabPage in expectedTabPagesInOrder)
				{
					Assert($"{expectedTabPage.Name} is visible", expectedTabPage.TabVisible);
				}
				AssertContainsExactElementsInExactOrder(expectedTabPagesInOrder, control.EntryInstructionTabControl.TabPages);
			});
		}
	}

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;

	readonly IEnumerable<(string, CharacterCasing?)> importControls =
	[
		(CusEntryInstruction.Schema.CEI_Style, CharacterCasing.Upper),
		(CusEntryInstruction.Schema.CEI_SubStyle, CharacterCasing.Upper),
		(CusEntryInstruction.Schema.CEI_Description, CharacterCasing.Normal),
		(CusEntryInstruction.Schema.CEI_DeclarationReason, CharacterCasing.Upper),
		(CusEntryInstruction.Schema.CEI_DateForDuty, CharacterCasing.Normal)
	];

	readonly IEnumerable<(string, CharacterCasing?)> exportControls =
	[
		(CusEntryInstruction.Schema.CEI_Procedure, CharacterCasing.Normal),
		(CusEntryInstruction.Schema.CEI_Style, CharacterCasing.Upper),
		(CusEntryInstruction.Schema.CEI_NextProcedure, CharacterCasing.Upper),
	];

	readonly IEnumerable<(string, CharacterCasing?)> edaControls =
	[
		(CusEntryInstruction.Schema.CEI_Procedure, CharacterCasing.Normal),
		(CusEntryInstruction.Schema.CEI_Style, CharacterCasing.Upper),
		(CusEntryInstruction.Schema.CEI_SubStyle, CharacterCasing.Upper),
		(CusEntryInstruction.Schema.CEI_NextProcedure, CharacterCasing.Upper),
	];

	IEnumerable<ZTabPage> GetExpectedTabPagesInOrder(EntryInstructionDetailsUserControl control, string messageType)
	{
		switch (messageType)
		{
			case CHJobMessageTypeList.Codes.Import:
				yield return control.EntryInstructionDetailsTabPage;
				break;
			case CHJobMessageTypeList.Codes.Export:
				yield return control.EntryInstructionDetailsTabPage;
				yield return control.PreviousDocumentsTabPage;
				yield return control.SupportingDocumentsTabPage;
				yield return control.TransportDocumentsTabPage;
				yield return control.CusSupplyChainActorsTabPage;
				yield return control.AdditionalInformationTabPage;
				break;
			case CHJobMessageTypeList.Codes.ExportDeclarationActivation:
				yield return control.EntryInstructionDetailsTabPage;
				yield return control.PreviousDocumentsTabPage;
				yield return control.SupportingDocumentsTabPage;
				yield return control.TransportDocumentsTabPage;
				yield return control.CusSupplyChainActorsTabPage;
				yield return control.AdditionalInformationTabPage;
				break;
		}
	}
}
