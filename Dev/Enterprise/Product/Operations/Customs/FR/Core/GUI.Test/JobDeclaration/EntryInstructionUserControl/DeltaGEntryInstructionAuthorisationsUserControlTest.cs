using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.Declaration.Testing
{
	sealed class DeltaGEntryInstructionAuthorisationsUserControlTest : TestCaseWithFactory
	{
		public void TestControls_AuthorisationsGrid()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new DeltaGEntryInstructionAuthorisationsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var authorisationsGrid = control.FindSingleOrDefault<ZGrid>("AuthorisationsGrid");
					var codeColumnStyle = authorisationsGrid.GetColumnStyle(AutoCusAuthorizationUsage.Schema.AGC_Code);
					var customsCodeColumnStyle = authorisationsGrid.GetColumnStyle(nameof(Business.CusAuthorizationUsage.CustomsCode));
					var numberColumnStyle = authorisationsGrid.GetColumnStyle(AutoCusAuthorizationUsage.Schema.AGC_Number);
					var orgColumnStyle = authorisationsGrid.GetColumnStyle(AutoCusAuthorizationUsage.Schema.AGC_OH_Owner);
					var authColumnStyle = authorisationsGrid.GetColumnStyle(AutoCusAuthorizationUsage.Schema.AGC_CPH_Authorization);
					var shortCodeColumnStyle = authorisationsGrid.GetColumnStyle(FR.Business.CusAuthorizationUsage.Schema.AGC_AuthorizationShortCode);

					AssertEquals("Columns", 7, authorisationsGrid.ColumnStyles.Count);

					AssertEquals("AGC_Code ColumnInfo", "ZDropEditColumnStyle", codeColumnStyle.ColumnStyleType.Name);
					AssertEquals("CustomsCode ColumnInfo", "ZTextBoxColumnStyle", customsCodeColumnStyle.ColumnStyleType.Name);
					AssertEquals("AGC_Number ColumnInfo", "ZMultiControlColumnStyle", numberColumnStyle.ColumnStyleType.Name);
					AssertEquals("AGC_OH_Owner ColumnInfo", "ZOrganisationFindBoxColumnStyle", orgColumnStyle.ColumnStyleType.Name);
					AssertEquals("AGC_CPH_Authorization ColumnInfo", "ZGuidFindBoxColumnStyle", authColumnStyle.ColumnStyleType.Name);
					AssertEquals("AGC_AuthorizationShortCode ColumnInfo", "ZTextBoxColumnStyle", shortCodeColumnStyle.ColumnStyleType.Name);

					AssertEquals("AGC_Code Column Width", 40, codeColumnStyle.Width);
					AssertEquals("CustomsCode Column Width", 100, customsCodeColumnStyle.Width);
					AssertEquals("AGC_Number Column Width", 130, numberColumnStyle.Width);
					AssertEquals("AGC_OH_Owner Column Width", 80, orgColumnStyle.Width);
					AssertEquals("AGC_CPH_Authorization Column Width", 130, authColumnStyle.Width);
					AssertEquals("AGC_AuthorizationShortCode Column Width", 80, shortCodeColumnStyle.Width);
				});
			}
		}
	}
}
