
using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_CreditLimitTemporaryIncreaseTest : ScriptTest
	{
		public void TestCreditLimitTemporaryIncreaseScript()
		{
			var creditController = Factory.NewWithValidTestData<GlbStaff>();
			creditController.GS_Code = "RJW";
			creditController.GS_FullName = "Rosa J. Wynter";
			var controllingBranch = TestObjectCreator.CreateBranch("CTL", "Controlling", GlbCompany.CurrentCompany);
			var orgWithoutTempCreditLimit = TestObjectCreator.CreateOrgHeader("NOTEMP", true, true);
			var orgWithTempCreditLimit = TestObjectCreator.CreateOrgHeader("TEMP", true, true);
			var orgWithSettlementGroup = TestObjectCreator.CreateOrgHeader("HASSETTLE", true, true);
			var settlementGroup = TestObjectCreator.CreateOrgHeader("SETTLE", true, true);
			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(orgWithoutTempCreditLimit.CompanyData, 900M, 0M);
			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(orgWithTempCreditLimit.CompanyData, 1000M, 100M);
			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(settlementGroup.CompanyData, 300M, 19M);
			orgWithTempCreditLimit.OH_FullName = "An organisation with a temporary credit limit";
			orgWithTempCreditLimit.CompanyData.OB_ARCreditRating = OrgConstants.ARCreditRating.Code.LowRisk;
			settlementGroup.OH_FullName = "A settlement group";
			settlementGroup.CompanyData.OB_ARCreditRating = OrgConstants.ARCreditRating.Code.VeryHighRisk;
			settlementGroup.CompanyData.OB_GB_ControllingBranch = controllingBranch.PK;
			var staffAssignmentToCreditController = settlementGroup.StaffAssignments.AddNew();
			staffAssignmentToCreditController.O8_Department = "All";
			staffAssignmentToCreditController.O8_GC = GlbCompany.CurrentCompany.PK;
			staffAssignmentToCreditController.O8_GS_NKPersonResponsible = "RJW";
			staffAssignmentToCreditController.O8_Role = StaffAssignmentRoles.Codes.CreditController;
			Factory.Save();
			settlementGroup.CompanyData.OB_ARTemporaryCreditLimitIncrease = 20M;
			Factory.Save();

			var result = RunScript();

			AssertEquals("Only shows rows with temporary credit limit", 2, result.Rows.Count);
			var rowForOrgWithTempCreditLimit = (from row in result.AsEnumerable() where ((string)row["OrgCode"]) == orgWithTempCreditLimit.OH_Code select row).Single();
			var rowForSettlementGroup = (from row in result.AsEnumerable() where ((string)row["OrgCode"]) == settlementGroup.OH_Code select row).Single();

			var headers = new[] { "OrganisationPK", "OrgCode", "OrgName", "CreditLimit", "TemporaryCreditLimitIncrease", "CreditRating",
				"TemporaryCreditLimitIncreasePercent", "AdjustedCreditLimit", "ExpiryDateUTC", "AdjustedByPK", "AdjustedBy",
				"CreditControllerPK", "CreditController", "SettlementGroupPK", "SettlementGroupCode", "SettlementGroupName",
				"ControllingBranchCode", "ControllingBranchName", "NumberOfAdjustments", "UseSettlementGroupCreditLimit" };

			AssertDataRow(rowForOrgWithTempCreditLimit, headers,
				new object[] {
					orgWithTempCreditLimit.PK,
					"ZTEMP",
					"An organisation with a temporary credit limit",
					1000M,
					100M,
					OrgConstants.ARCreditRating.Code.LowRisk,
					10M,
					1100M,
					orgWithTempCreditLimit.CompanyData.OB_ARTemporaryCreditLimitIncreaseExpiry,
					Env.CurrentUser.PK,
					"E  ",
					DBNull.Value,
					DBNull.Value,
					DBNull.Value,
					DBNull.Value,
					DBNull.Value,
					DBNull.Value,
					DBNull.Value,
					1,
					false
				});

			var adjustedDate = (DateTime)rowForOrgWithTempCreditLimit["AdjustedDateUTC"];
			Assert("Adjusted date [adjustedDate] should be about now-ish", DateTime.UtcNow.AddMinutes(-5) < adjustedDate && adjustedDate < DateTime.UtcNow);

			AssertDataRow(rowForSettlementGroup, headers,
				new object[] {
					settlementGroup.PK,
					"ZSETTLE",
					"A settlement group",
					300M,
					20M,
					OrgConstants.ARCreditRating.Code.VeryHighRisk,
					6.6666666666666667M,
					320M,
					settlementGroup.CompanyData.OB_ARTemporaryCreditLimitIncreaseExpiry,
					Env.CurrentUser.PK,
					"E  ",
					creditController.PK,
					"RJW",
					DBNull.Value,
					DBNull.Value,
					DBNull.Value,
					"CTL",
					"Controlling",
					2,
					false
				});

			var adjustedDateForSettlementGroup = (DateTime)rowForOrgWithTempCreditLimit["AdjustedDateUTC"];
			Assert("Adjusted date [adjustedDateForSettlementGroup] should be about now-ish", DateTime.UtcNow.AddMinutes(-5) < adjustedDateForSettlementGroup && adjustedDateForSettlementGroup < DateTime.UtcNow);
		}

		DataTable RunScript()
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
select * from Report_CreditLimitTemporaryIncrease(
'{0}'	--@Company
)",
			GlbCompany.CurrentCompany.PK
			));
		}
	}
}

