using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Common.BE;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI.Testing;

sealed class EntryInstructionGridUserControlTest : TestCaseWithFactory
{
	public void TestManualDeclarationColumn()
	{
		using (var control = new EntryInstructionGridUserControl())
		{
			var grid = control.FindSingle<ZGrid>("EntryInstructionsGrid");
			var manualDeclarationColumnInfo = grid.GetColumnStyle(CusEntryInstruction.Schema.ZG_ManualDeclaration);
			AssertNotNull(manualDeclarationColumnInfo);
		}
	}

	public void TestEntryInstructionsGridCurrentColumnLayout()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = ZString.Empty;
		declaration.CustomsEntryInstructions.AddNew();
		using (var form = new ZForm(declaration))
		using (var control = new EntryInstructionGridUserControl())
		{
			control.JobDeclaration = declaration;
			form.Controls.Add(control);
			form.Show();
			var grid = control.FindSingle<ZGrid>("EntryInstructionsGrid");
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = BEJobMessageTypeList.Codes.Import;
				AssertEquals("Import", "Import", grid.ColumnLayoutContext);
				declaration.JE_MessageType = BEJobMessageTypeList.Codes.Export;
				AssertEquals("Export", "Export", grid.ColumnLayoutContext);
				declaration.JE_MessageType = BEJobMessageTypeList.Codes.ReExport;
				AssertEquals("ReExport", "Export", grid.ColumnLayoutContext);
				declaration.JE_MessageType = BEJobMessageTypeList.Codes.ExitSummary;
				AssertEquals("ExitSummary", "Export", grid.ColumnLayoutContext);
			});
		}
	}

	public void TestEntryInstructionsGridCurrent_CEI_SubStyle_Visibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryInstructions.AddNew();
		using (var form = new ZForm(declaration))
		using (var control = new EntryInstructionGridUserControl())
		{
			control.JobDeclaration = declaration;
			form.Controls.Add(control);
			form.Show();
			var grid = control.FindSingle<ZGrid>("EntryInstructionsGrid");
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = BEJobMessageTypeList.Codes.Import;
				var style = grid.GetColumnStyle(CusEntryInstruction.Schema.CEI_SubStyle);
				AssertEquals("CEI_SubStyle should be available for Import", false, style.IsUnavailable);
				declaration.JE_MessageType = BEJobMessageTypeList.Codes.Export;
				style = grid.GetColumnStyle(CusEntryInstruction.Schema.CEI_SubStyle);
				AssertEquals("CEI_SubStyle should be available for Export", false, style.IsUnavailable);
				declaration.JE_MessageType = BEJobMessageTypeList.Codes.ReExport;
				style = grid.GetColumnStyle(CusEntryInstruction.Schema.CEI_SubStyle);
				AssertEquals("CEI_SubStyle should not be available for ReExport", true, style.IsUnavailable);
				declaration.JE_MessageType = BEJobMessageTypeList.Codes.ExitSummary;
				style = grid.GetColumnStyle(CusEntryInstruction.Schema.CEI_SubStyle);
				AssertEquals("CEI_SubStyle should not be available for ExitSummary", true, style.IsUnavailable);
			});
		}
	}
}
