using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using NUnit.Framework;
using static Enterprise.Build.Database.Script.TestFramework.TestDataCreator;

namespace Enterprise.Build.Database.Script.Public.Customs.Testing
{
	[TestedType(typeof(Report_EuEntriesChargesAndFees))]
	sealed class Report_EuEntriesChargesAndFeesTest : DbCreateScriptTest
	{
		public void TestCf_Source() => CombineAssertions(() =>
		{
			var today = new DateTime(2022, 10, 18);
			var companyPk = CreateCompany("DDD", "DE", "EUR");
			var branchPk = CreateBranch(companyPk, "BRN", "HAM", "DE");

			var jobDeclaration = CreateJobDeclaration(branchPk, companyPk, "B001", "IMP", 1);

			DateTime CreateHeaderWithFees(DateTime cHEntrySubmittedDate, params string[] feeSources)
			{
				var entryInstruction1 = CreateCusEntryInstruction(jobDeclaration, "EZA", "TBD", today, 1);

				var cusEntryHeader = CreateCusEntryHeader(true, "IMP", "", "", 1, jobDeclaration,
					cHEntrySubmittedDate, today, entryInstruction1, today, 1);

				foreach (var feeSource in feeSources)
				{
					var cusEntryLine = CreateCusEntryLine(cusEntryHeader, 1);
					CreateCusEntryLineFee(cusEntryLine, "DEC", 0, 1, feeSource);
				}
				return cHEntrySubmittedDate;
			}

			// adding headers with different submitted dates to correlate them with expected source
			var expectedFeeSources = new List<(DateTime CH_EntrySubmittedDate, string ExpectedSource)>
			{
				(CH_EntrySubmittedDate: CreateHeaderWithFees(today.AddDays(-1), "CW1","CW1"), ExpectedSource: "CW1"),
				(CH_EntrySubmittedDate: CreateHeaderWithFees(today.AddDays(-2), "CW1","CUS"), ExpectedSource: "CUS"),
				(CH_EntrySubmittedDate: CreateHeaderWithFees(today.AddDays(-3), "CUS","CUS"), ExpectedSource: "CUS"),
			};

			var reportSql = $@"
SELECT CF_Source, CH_EntrySubmittedDate FROM [dbo].[Report_EuEntriesChargesAndFees] (
   '{companyPk}' --<@CompanyPK, uniqueidentifier,>
  ,'DE' --<@Country, char(2),>
  ,'IMP' --<@DeclarationMessageType, char(3),>
  ,'2022-10-10' --<@EntrySubmittedDateFrom, datetime,>
  ,'2022-10-19' --<@EntrySubmittedDateTo, datetime,>
  ,NULL --<@EntryStyle, char(7),>
  ,NULL --<@EntrySubStyle, char(3),>
  ,NULL --<@PrimaryPaymentMethod, char(3),>
  ,NULL --<@PrimaryPaymentAccount, char(35),>
  ,NULL --<@SecondaryPaymentMethod, char(3),>
  ,NULL --<@SecondaryPaymentAccount, char(20),>
  ,NULL --<@ChargeType, char(5),>
  ,NULL --<@ChargePaymentMethod, char(3),>
)
";
			TestConnection.ExecuteReader(
				reportSql,
				reader =>
				{
					var actualCF_Source = (string)reader["CF_Source"];
					var actualCH_EntrySubmittedDate = (DateTime)reader["CH_EntrySubmittedDate"];
					var expectedCF_Source = expectedFeeSources.First(x => x.CH_EntrySubmittedDate == actualCH_EntrySubmittedDate).ExpectedSource;
					AssertEquals("CF_Source", expectedCF_Source, actualCF_Source);
				}
			);
		});
	}
}
