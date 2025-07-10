using Enterprise.DocumentEngine.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class ReportCommandValidationTest : StmMenuItemBaseValidationTest
	{
		#region Test Cases

		public void TestCheckSU_FilterList()
		{
			var command = Factory.NewWithValidTestData<ReportCommand>();
			command.SU_FilterList = "1=0";
			command.SU_IsSystemDefined = true;

			command.RunPreSaveValidation();
			AssertHasErrors(command.SU_FilterListInfo);

			Factory.Save();

			command.RunPreSaveValidation();
			AssertNoErrors(command.SU_FilterListInfo);
		}

		public void TestCheckSU_Calc_IsWebSupportableWhenReportIsNull()
		{
			ReportCommand testCommand = Factory.New<ReportCommand>();
			testCommand.SU_MenuName = "Test Report";

			using (Report testReport = testCommand.GetReport())
			{
				AssertNull(testReport);
			}
			testCommand.SU_Calc_IsWebSupportable = false;
			testCommand.Validation.ValidateAll();
			AssertNoErrors(testCommand.SU_Calc_IsWebSupportableInfo);

			testCommand.SU_Calc_IsWebSupportable = true;
			testCommand.Validation.ValidateAll();
			AssertHasError(testCommand.SU_Calc_IsWebSupportableInfo, "Report cannot be published on Web because it cannot be linked to a client organization.");
		}

		public void TestCheckSU_Calc_IsWebSupportableWhenLinkedLookupFieldIsNull()
		{
			ReportCommand testCommand = Factory.New<ReportCommand>();
			testCommand.SU_MenuName = "Test Report";

			StmTemplate template = Factory.New<StmTemplate>();
			template.SO_Name = "Test Template";

			StmMenuTemplatePivot pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_DocumentTitle = "Test Document";
			pivot.SI_SU = testCommand.PK;
			pivot.SI_SO = template.PK;

			template.SO_Template = DocumentEngineTestHelper.CreateTemplateFromString(
@"{A}-[#Config]
{A}-[Name=TestIsDraft]
{A}-[#SectionBody]
{A}-[#EndOfReport]");
			using (Report testReport = testCommand.GetReport())
			{
				AssertNotNull(testReport);
				AssertNull(testReport.LinkedLookupField);
				Assert(!testReport.ForceWebPublish);
			}
			testCommand.SU_Calc_IsWebSupportable = false;
			testCommand.Validation.ValidateAll();
			AssertNoErrors(testCommand.SU_Calc_IsWebSupportableInfo);

			testCommand.SU_Calc_IsWebSupportable = true;
			testCommand.Validation.ValidateAll();
			AssertHasError(testCommand.SU_Calc_IsWebSupportableInfo, "Report cannot be published on Web because it cannot be linked to a client organization.");

			template.SO_Template = DocumentEngineTestHelper.CreateTemplateFromString(
@"{A}-[#Config]
{A}-[Name=TestIsDraft]
{A}-[ForceWebPublish]
{A}-[#SectionBody]
{A}-[#EndOfReport]");
			using (Report testReport = testCommand.GetReport())
			{
				AssertNotNull(testReport);
				AssertNull(testReport.LinkedLookupField);
				Assert(testReport.ForceWebPublish);
			}
			testCommand.SU_Calc_IsWebSupportable = false;
			testCommand.Validation.ValidateAll();
			AssertNoErrors(testCommand.SU_Calc_IsWebSupportableInfo);

			testCommand.SU_Calc_IsWebSupportable = true;
			testCommand.Validation.ValidateAll();
			AssertHasWarning(testCommand.SU_Calc_IsWebSupportableInfo, "Report is published on Web, but it is not linked to a client organization.");
		}

		#endregion
	}
}
