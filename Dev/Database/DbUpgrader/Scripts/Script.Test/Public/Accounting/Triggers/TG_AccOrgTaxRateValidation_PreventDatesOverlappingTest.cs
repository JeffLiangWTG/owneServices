using System;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Triggers;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Testing
{
	[TestedType(typeof(TG_AccOrgTaxRateValidation_PreventDatesOverlapping))]
	class TG_AccOrgTaxRateValidation_PreventDatesOverlappingTest : DBCreateTriggerScriptTest
	{
		public void TestOTR_OTCOnInsert()
		{
			var orgTaxRate1 = DbHelper.InsertOrgTaxRate(orgTaxConfigPK);

			var otherTaxConfigurationPK = DbHelper.InsertTaxConfiguration("XX1", TestDbHelper.DefaultCompanyPK);
			var orgTaxConfigPK2 = DbHelper.InsertOrgTaxConfiguration(otherTaxConfigurationPK, orgCompanyDataPK);

			AssertInsert(false, orgTaxConfigPK2);
			AssertInsert(true, orgTaxConfigPK2);
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestOTR_OTCOnUpdate()
		{
			var otherTaxConfigurationPK = DbHelper.InsertTaxConfiguration("XX1", TestDbHelper.DefaultCompanyPK);
			var orgTaxConfigPK2 = DbHelper.InsertOrgTaxConfiguration(otherTaxConfigurationPK, orgCompanyDataPK);

			var orgTaxRate1 = DbHelper.InsertOrgTaxRate(orgTaxConfigPK);
			var orgTaxRate2 = DbHelper.InsertOrgTaxRate(orgTaxConfigPK2);

			var sql = @"UPDATE dbo.AccOrgTaxRate SET OTR_OTC = @OTR_OTC, OTR_SystemLastEditTimeUtc = GETUTCDATE(), OTR_SystemLastEditUser = 'TST' WHERE OTR_PK = @OTR_PK";

			AssertExceptionThrown<SqlException>("Should be: " + ExpectedErrorMessage, ExpectedErrorMessage + "\r\nThe transaction ended in the trigger. The batch has been aborted.", () => DbHelper.RunSQL(new { OTR_OTC = orgTaxConfigPK, OTR_PK = orgTaxRate2 }, sql));
			AssertExceptionThrown<SqlException>("Should be: " + ExpectedErrorMessage, ExpectedErrorMessage + "\r\nThe transaction ended in the trigger. The batch has been aborted.", () => DbHelper.RunSQL(new { OTR_OTC = orgTaxConfigPK2, OTR_PK = orgTaxRate1 }, sql));
		}

		public void TestOTR_EndDateOnInsert()
		{
			var orgTaxRate1 = DbHelper.InsertOrgTaxRate(orgTaxConfigPK, startDate: BaseStartDate, endDate: BaseEndDate);
			AssertInsert(true, orgTaxConfigPK, endDate: DateTime.Today.AddDays(5));
		}

		public void TestOTR_EndDateOnUpdate()
		{
			var orgTaxRate1 = DbHelper.InsertOrgTaxRate(orgTaxConfigPK, startDate: DateTime.Today, endDate: DateTime.Today.AddDays(2));
			var orgTaxRate2 = DbHelper.InsertOrgTaxRate(orgTaxConfigPK, startDate: DateTime.Today.AddDays(-1), endDate: DateTime.Today.AddDays(-1));

			var sql = @"UPDATE dbo.AccOrgTaxRate SET OTR_EndDate = @OTR_EndDate, OTR_SystemLastEditTimeUtc = GETUTCDATE(), OTR_SystemLastEditUser = 'TST' WHERE OTR_PK = @OTR_PK";
			AssertExceptionThrown<SqlException>("Should be: " + ExpectedErrorMessage, ExpectedErrorMessage + "\r\nThe transaction ended in the trigger. The batch has been aborted.", () => DbHelper.RunSQL(new { OTR_EndDate = DateTime.Today.AddDays(5), OTR_PK = orgTaxRate2 }, sql));
		}

		public void TestOTR_StartOnInsert()
		{
			var orgTaxRate1 = DbHelper.InsertOrgTaxRate(orgTaxConfigPK, startDate: BaseStartDate, endDate: BaseEndDate);
			AssertInsert(true, orgTaxConfigPK, startDate: DateTime.Today.AddDays(-5));
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestOTR_StartOnUpdate()
		{
			var orgTaxRate1 = DbHelper.InsertOrgTaxRate(orgTaxConfigPK, startDate: DateTime.Today.AddDays(-5), endDate: DateTime.Today.AddDays(-4));
			var orgTaxRate2 = DbHelper.InsertOrgTaxRate(orgTaxConfigPK, startDate: BaseStartDate, endDate: BaseEndDate);

			var sql = @"UPDATE dbo.AccOrgTaxRate SET OTR_StartDate = @OTR_StartDate, OTR_SystemLastEditTimeUtc = GETUTCDATE(), OTR_SystemLastEditUser = 'TST' WHERE OTR_PK = @OTR_PK";
			AssertExceptionThrown<SqlException>("Should be: " + ExpectedErrorMessage, ExpectedErrorMessage + "\r\nThe transaction ended in the trigger. The batch has been aborted.", () => DbHelper.RunSQL(new { OTR_StartDate = DateTime.Today.AddDays(-5), OTR_PK = orgTaxRate2 }, sql));
		}

		public void TestOTR_SourceOnInsert()
		{
			var allowedSources = new[] { "QUA", "MON", "MOV" };
			foreach (var sourceValue in allowedSources)
			{
				AssertInsert(false, orgTaxConfigPK, startDate: BaseStartDate, endDate: BaseEndDate, source: sourceValue);
			}
		}

		public void TestOTR_SourceOnUpdate()
		{
			var allowedSources = new[] { "QUA", "MON", "MOV" };
			var orgTaxRate = DbHelper.InsertOrgTaxRate(orgTaxConfigPK, startDate: BaseStartDate, endDate: BaseEndDate);

			var sql = @"UPDATE dbo.AccOrgTaxRate SET OTR_Source = @OTR_Source, OTR_SystemLastEditTimeUtc = GETUTCDATE(), OTR_SystemLastEditUser = 'TST' WHERE OTR_PK = @OTR_PK";

			foreach (var sourceValue in allowedSources)
			{
				AssertNoExceptionThrown(() => DbHelper.RunSQL(new { OTR_Source = sourceValue, OTR_PK = orgTaxRate }, sql));
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestDoNotAllowTaxRatesToBeAddedIfDatesOverlap()
		{
			var testCases = new[]
			{
				new { thrownException = true, startDate = DateTime.Today.AddDays(-1), endDate = DateTime.Today },
				new { thrownException = true, startDate = DateTime.Today.AddDays(-1), endDate = DateTime.Today.AddDays(1) },
				new { thrownException = true, startDate = DateTime.Today.AddDays(-1), endDate = DateTime.Today.AddDays(2) },
				new { thrownException = true, startDate = DateTime.Today.AddDays(-1), endDate = DateTime.Today.AddDays(3) },

				new { thrownException = true, startDate = DateTime.Today, endDate = DateTime.Today },
				new { thrownException = true, startDate = DateTime.Today, endDate = DateTime.Today.AddDays(1) },
				new { thrownException = true, startDate = DateTime.Today, endDate = DateTime.Today.AddDays(2) },
				new { thrownException = true, startDate = DateTime.Today, endDate = DateTime.Today.AddDays(3) },

				new { thrownException = true, startDate = DateTime.Today.AddDays(1), endDate = DateTime.Today.AddDays(+1) },
				new { thrownException = true, startDate = DateTime.Today.AddDays(1), endDate = DateTime.Today.AddDays(+2) },
				new { thrownException = true, startDate = DateTime.Today.AddDays(1), endDate = DateTime.Today.AddDays(+3) },

				new { thrownException = true, startDate = DateTime.Today.AddDays(2), endDate = DateTime.Today.AddDays(2) },
				new { thrownException = true, startDate = DateTime.Today.AddDays(2), endDate = DateTime.Today.AddDays(3) },

				new { thrownException = false, startDate = DateTime.Today.AddDays(3), endDate = DateTime.Today.AddDays(4) },
			};

			var orgTaxRate1 = DbHelper.InsertOrgTaxRate(orgTaxConfigPK, startDate: DateTime.Today, endDate: DateTime.Today.AddDays(2));
			var orgTaxRate2 = DbHelper.InsertOrgTaxRate(orgTaxConfigPK, startDate: DateTime.Today.AddDays(-1), endDate: DateTime.Today.AddDays(-1));

			Assert("Precondition: TG_AccOrgTaxRateValidation_PreventDatesOverlapping trigger does exist", DoesTG_AccOrgTaxRateValidation_PreventDatesOverlapping_Exists());
			foreach (var test in testCases)
			{
				AssertInsert(test.thrownException, orgTaxConfigPK, startDate: test.startDate, endDate: test.endDate);
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestDoNotAllowTaxRatesToBeModifiedIfDatesOverlap()
		{
			var testCases = new[]
			{
				new { thrownException = true, startDate = DateTime.Today.AddDays(-1), endDate = DateTime.Today },
				new { thrownException = true, startDate = DateTime.Today.AddDays(-1), endDate = DateTime.Today.AddDays(1) },
				new { thrownException = true, startDate = DateTime.Today.AddDays(-1), endDate = DateTime.Today.AddDays(2) },
				new { thrownException = true, startDate = DateTime.Today.AddDays(-1), endDate = DateTime.Today.AddDays(3) },

				new { thrownException = true, startDate = DateTime.Today, endDate = DateTime.Today },
				new { thrownException = true, startDate = DateTime.Today, endDate = DateTime.Today.AddDays(1) },
				new { thrownException = true, startDate = DateTime.Today, endDate = DateTime.Today.AddDays(2) },
				new { thrownException = true, startDate = DateTime.Today, endDate = DateTime.Today.AddDays(3) },

				new { thrownException = true, startDate = DateTime.Today.AddDays(1), endDate = DateTime.Today.AddDays(+1) },
				new { thrownException = true, startDate = DateTime.Today.AddDays(1), endDate = DateTime.Today.AddDays(+2) },
				new { thrownException = true, startDate = DateTime.Today.AddDays(1), endDate = DateTime.Today.AddDays(+3) },

				new { thrownException = true, startDate = DateTime.Today.AddDays(2), endDate = DateTime.Today.AddDays(2) },
				new { thrownException = true, startDate = DateTime.Today.AddDays(2), endDate = DateTime.Today.AddDays(3) },

				new { thrownException = false, startDate = DateTime.Today.AddDays(3), endDate = DateTime.Today.AddDays(4) },
			};

			var orgTaxRate1 = DbHelper.InsertOrgTaxRate(orgTaxConfigPK, startDate: DateTime.Today, endDate: DateTime.Today.AddDays(2));
			var orgTaxRate2 = DbHelper.InsertOrgTaxRate(orgTaxConfigPK, startDate: DateTime.Today.AddDays(-1), endDate: DateTime.Today.AddDays(-1));

			Assert("Precondition: TG_AccOrgTaxRateValidation_PreventDatesOverlapping trigger does exist", DoesTG_AccOrgTaxRateValidation_PreventDatesOverlapping_Exists());
			foreach (var test in testCases)
			{
				AssertUpdate(test.thrownException, orgTaxConfigPK, orgTaxRate2, startDate: test.startDate, endDate: test.endDate);
			}
		}

		void AssertInsert(bool shouldThrownException, Guid orgTaxConfigPK, DateTime? startDate = null, DateTime? endDate = null, string source = "MOV")
		{
			var expectedMessage = "Date Ranges overlap for same Tax Code & Source.";
			AssertDbAction(expectedMessage, shouldThrownException, () => DbHelper.InsertOrgTaxRate(orgTaxConfigPK, startDate ?? BaseStartDate, endDate ?? BaseEndDate, source));
		}

		void AssertUpdate(bool shouldThrownException, Guid orgTaxConfigPK, Guid orgTaxRatePK, DateTime? startDate = null, DateTime? endDate = null, string source = "MOV")
		{
			var expectedMessage = "Date Ranges overlap for same Tax Code & Source.";
			var sql = @"UPDATE dbo.AccOrgTaxRate SET OTR_StartDate = @OTR_StartDate, OTR_EndDate = @OTR_EndDate, OTR_Source = @OTR_Source, OTR_OTC = @OTR_OTC, OTR_SystemLastEditTimeUtc = GETUTCDATE(), OTR_SystemLastEditUser = 'TST' WHERE OTR_PK = @OTR_PK";
			AssertDbAction(expectedMessage, shouldThrownException, () => DbHelper.RunSQL(new { OTR_StartDate = startDate ?? BaseStartDate, OTR_EndDate = endDate ?? BaseEndDate, OTR_Source = source, OTR_OTC = orgTaxConfigPK, OTR_PK = orgTaxRatePK }, sql));
		}

		static void AssertDbAction(string message, bool shouldThrownException, AnonymousMethod dbAction)
		{
			if (shouldThrownException)
			{
				AssertExceptionThrown<SqlException>("Should be: " + message, message + "\r\nThe transaction ended in the trigger. The batch has been aborted.", dbAction);
			}
			else
			{
				AssertNoExceptionThrown(message, dbAction);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var organizationPK = DbHelper.InsertOrgHeader("ARCO1", "MY ORGANISATION");
			orgCompanyDataPK = DbHelper.InsertOrgCompanyData(organizationPK, companyPK);
			var taxConfigurationPK = DbHelper.InsertTaxConfiguration("AR1", companyPK);
			orgTaxConfigPK = DbHelper.InsertOrgTaxConfiguration(taxConfigurationPK, orgCompanyDataPK);
		}

		Guid orgTaxConfigPK;
		Guid orgCompanyDataPK;

		readonly DateTime BaseStartDate = DateTime.Today.AddDays(-1);
		readonly DateTime BaseEndDate = DateTime.Today.AddDays(1);
		const string ExpectedErrorMessage = "Date Ranges overlap for same Tax Code & Source.";

		bool DoesTG_AccOrgTaxRateValidation_PreventDatesOverlapping_Exists() => TestConnection.Exists("FROM sys.triggers WHERE name = 'TG_AccOrgTaxRateValidation_PreventDatesOverlapping'");
	}
}
