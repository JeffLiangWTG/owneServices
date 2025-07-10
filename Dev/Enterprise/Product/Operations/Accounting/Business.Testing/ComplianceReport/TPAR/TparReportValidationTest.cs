namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Accounting.Business.ComplianceReport.TPAR;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Environment;

	internal class TparReportValidationTest : BusinessObjectValidationTestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestValidateSenderContactPhone()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			var tparReport = Factory.NewWithValidTestData<TparReport>();
			tparReport.ATR_ACR_ComplianceReport = complianceReport.PK;

			AssertEquals(string.Empty, EnvProxy.Instance.CurrentUser.WorkPhone);
			AssertEquals(string.Empty, complianceReport.Company.OrgProxy.MainAddress.OA_Phone);
			AssertEquals(string.Empty, tparReport.SenderContactPhone);

			var expectedMessage = "Missing Sender Contact Phone. Add a work phone number in your contact numbers or organization proxy phone on the details tab.";
			tparReport.RunPreSaveValidation();
			AssertHasRowError(tparReport, expectedMessage);

			complianceReport.Company.OrgProxy.MainAddress.OA_Phone = "+61280012666";
			AssertEquals("+61280012666", tparReport.SenderContactPhone);
			tparReport.RunPreSaveValidation();
			AssertNoRowErrors(tparReport);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_WorkPhone = "+61280012200";
			Factory.Save();
			Env.SetUserContext(new UserContext(staff1.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

			AssertEquals("+61280012200", tparReport.SenderContactPhone);
			tparReport.RunPreSaveValidation();
			AssertNoRowErrors(tparReport);
		}
	}
}