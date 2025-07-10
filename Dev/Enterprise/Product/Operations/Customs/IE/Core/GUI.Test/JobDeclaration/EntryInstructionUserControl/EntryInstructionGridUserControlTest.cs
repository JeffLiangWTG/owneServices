using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI.Testing
{
	class EntryInstructionGridUserControlTest : TestCaseWithFactory
	{
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
					declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
					AssertEquals("Import", "Import", grid.ColumnLayoutContext);
					declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
					AssertEquals("Export", "Export", grid.ColumnLayoutContext);
					declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
					AssertEquals("ReExport", "Export", grid.ColumnLayoutContext);
					declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
					AssertEquals("ExitSummary", "Export", grid.ColumnLayoutContext);
				});
			}
		}
	}
}
