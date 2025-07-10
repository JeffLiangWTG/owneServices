using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	class EntryInstructionGridUserControlTest : TestCaseWithFactory
	{
		public void TestAvailableColumns()
		{
			using (var control = new EntryInstructionGridUserControl())
			{
				var grid = control.FindSingle<ZGrid>("EntryInstructionsGrid");
				CombineAssertions(() =>
				{
					AssertEquals("Count of columns", 8, grid.ColumnStyles.Count);
					AssertEquals("EntryInstructionsGrid.CaptionVisible", false, grid.CaptionVisible);
				});

				AssertColumnStyle(CusEntryInstruction.Schema.CEI_SubStyle, true, 68, CharacterCasing.Normal);
				AssertColumnStyle(CusEntryInstruction.Schema.WarehouseIDFor27, false, 133, CharacterCasing.Normal);
				AssertColumnStyle(CusEntryInstruction.Schema.CEI_Style, true, 105, CharacterCasing.Upper);
				AssertColumnStyle(CusEntryInstruction.Schema.CEI_Description, true, 250, CharacterCasing.Upper);
				AssertColumnStyle(CusEntryInstruction.Schema.CEI_DateForDuty, true, 106, CharacterCasing.Normal);
				AssertColumnStyle(CusEntryInstruction.Schema.CEI_Procedure, true, 60, CharacterCasing.Normal);
				AssertColumnStyle(CusEntryInstruction.Schema.ZG_TransNature, true, 110, CharacterCasing.Normal);
				AssertColumnStyle(CusEntryInstruction.Schema.CEI_TotalInnerPackages, true, 90, CharacterCasing.Normal);

				void AssertColumnStyle(string columnName, bool expectedVisible, int expectedWidth, CharacterCasing expectedCharacterCasing)
				{
					var columnStyle = control.FindSingle<ZGrid>("EntryInstructionsGrid").GetColumnStyle(columnName);
					CombineAssertions(() =>
					{
						AssertEquals($"{columnName}.Visible", expectedVisible, columnStyle.IsVisible);
						AssertEquals($"{columnName}.Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(expectedWidth), columnStyle.Width);
						AssertEquals($"{columnName}.CharacterCasing", expectedCharacterCasing, columnStyle.CharacterCasing);
					});
				}
			}
		}

		public void TestShowRequestedProcedure()
		{
			using (var userControl = new EntryInstructionGridUserControlForTesting())
			{
				AssertEquals(true, userControl.ShowRequestedProcedureExposed);
			}
		}
	}

	sealed class EntryInstructionGridUserControlForTesting : EntryInstructionGridUserControl
	{
		internal bool ShowRequestedProcedureExposed => ShowRequestedProcedure;
	}
}
