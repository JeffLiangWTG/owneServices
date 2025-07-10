using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class EntryInstructionAuthorisationsComputedUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionAuthorisationsComputedUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var authorisationsGrid = control.FindSingleOrDefault<ZGrid>("AuthorisationsGrid");
					AssertEquals("Grid binding", "CustomsEntryInstructions.CusAuthorizationUsages", authorisationsGrid.GetBindingMember());

					var codeColumnStyle = authorisationsGrid.GetColumnStyle(AutoCusAuthorizationUsage.Schema.AGC_Code);
					var customsCodeColumnStyle = authorisationsGrid.GetColumnStyle(nameof(CusAuthorizationUsage.CustomsCode));
					var numberColumnStyle = authorisationsGrid.GetColumnStyle(nameof(CusAuthorizationUsage.EffectiveReferenceNumber));
					var orgColumnStyle = authorisationsGrid.GetColumnStyle(AutoCusAuthorizationUsage.Schema.AGC_OH_Owner);

					AssertEquals("Columns", 4, authorisationsGrid.ColumnStyles.Count);

					AssertEquals("AGC_Code ColumnInfo", "ZDropEditColumnStyle", codeColumnStyle.ColumnStyleType.Name);
					AssertEquals("CustomsCode ColumnInfo", "ZTextBoxColumnStyle", customsCodeColumnStyle.ColumnStyleType.Name);
					AssertEquals("Authorization Number ColumnInfo", "ZCodeFindBoxColumnStyle", numberColumnStyle.ColumnStyleType.Name);
					AssertEquals("AGC_OH_Owner ColumnInfo", "ZOrganisationFindBoxColumnStyle", orgColumnStyle.ColumnStyleType.Name);

					AssertEquals("AGC_Code Column Width", 40, codeColumnStyle.Width);
					AssertEquals("Customs Code Column Width", 100, customsCodeColumnStyle.Width);
					AssertEquals("Authorization Number Column Width", 130, numberColumnStyle.Width);
					AssertEquals("AGC_OH_Owner Column Width", 80, orgColumnStyle.Width);

					AssertEquals("AGC_Code Visible", true, codeColumnStyle.IsVisible);
					AssertEquals("CustomsCode Visible", true, customsCodeColumnStyle.IsVisible);
					AssertEquals("Authorization Number Visible", true, numberColumnStyle.IsVisible);
					AssertEquals("AGC_OH_Owner Visible", true, orgColumnStyle.IsVisible);

					AssertEquals("AGC_Code Mandatory", true, codeColumnStyle.IsMandatory);
					AssertEquals("CustomsCode Mandatory", false, customsCodeColumnStyle.IsMandatory);
					AssertEquals("Authorization Number Mandatory", true, numberColumnStyle.IsMandatory);
					AssertEquals("AGC_OH_Owner Mandatory", true, orgColumnStyle.IsMandatory);
				});
			}
		}
	}
}
