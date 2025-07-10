using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using CargoWise.Schema;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(Report_PaymentsToIRS1099FormEligibleOrganisationsSummaryWithTwoOrgsPerRow))]
	class Report_PaymentsToIRS1099FormEligibleOrganisationsSummaryWithTwoOrgsPerRowTest : DbCreateScriptTest
	{
		const string ReportQuery = "SELECT * FROM Report_PaymentsToIRS1099FormEligibleOrganisationsSummaryWithTwoOrgsPerRow('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 2005, 'ALL', '', '')";

		string GetReportQueryWithOrgFilter(string onlyIncludeOrgs, string excludeOrgs)
		{
			return $"SELECT * FROM Report_PaymentsToIRS1099FormEligibleOrganisationsSummaryWithTwoOrgsPerRow('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 2005, 'ALL', '{onlyIncludeOrgs}', '{excludeOrgs}')";
		}
		string GetStringWithSameLengthAsColumn(SchemaStringColumn column)
		{
			return new string('A', column.MaxLength);
		}

		public void TestHardCodedLengths()
		{
			Guid orgPK = Guid.Parse("AB050B5F-2F74-4ABE-84ED-405B46F08007");
			InsertAccTransactionHeader("00005015", new DateTime(2005, 5, 27), new DateTime(2005, 5, 27), -50, -51, new DateTime(2005, 5, 27), orgPK);
			AssertNoExceptionThrown("Precondition: hardcoded fields, likely to change length, are OK for now", () => RunReportForTesting());

			UpdateOrg(orgPK, GetStringWithSameLengthAsColumn(OrgHeaderSchema.OH_Code), GetStringWithSameLengthAsColumn(OrgHeaderSchema.OH_FullName));
			AssertNoExceptionThrown("If there's an exception: Org code/name's max length is larger than the length in the report", () => RunReportForTesting());

			UpdateOrgAddress(orgPK, GetStringWithSameLengthAsColumn(OrgAddressSchema.OA_Address1), GetStringWithSameLengthAsColumn(OrgAddressSchema.OA_City));
			AssertNoExceptionThrown("If there's an exception: Org address/city's max length is larger than the length in the report", () => RunReportForTesting());

			UpdateOrgCusCode(orgPK, GetStringWithSameLengthAsColumn(OrgCusCodeSchema.OK_CustomsRegNo));
			AssertNoExceptionThrown("If there's an exception: CustomsRegNo's max length is larger than the length in the report", () => RunReportForTesting());
		}

		void RunReportForTesting()
		{
			using (DbCommand command = TestConnection.Command(ReportQuery))
			{
				command.ExecuteNonQuery();
			}
		}

		public void TestOnlyIncludeOrgFilter()
		{
			SetUpData();
			var kinyinOrgPK = Guid.Parse("1E9E3616-B55D-4CAC-9EEF-37FAB9B87BC5");
			var orgCode = TestConnection.ExecuteScalar<string>($"SELECT TOP 1 OH_CODE FROM dbo.OrgHeader where OH_PK = '{kinyinOrgPK}'");
			var filterReportQuery = GetReportQueryWithOrgFilter(orgCode , "");

			DataTable report = DataUtils.GetDataTableFromQuery(TestConnection, filterReportQuery);

			CombineAssertions(() =>
			{
				AssertEquals("Result should have rows", 1, report.Rows.Count);
				AssertEquals("Result should have rows", "KINYIN", ((string)report.Rows[0]["Org1_OH_Code"]).Trim());
				AssertEquals("Result should have rows", "AB-C123456", ((string)report.Rows[0]["Org1_OK_CustomsRegNoEIN"]).Trim());
				AssertEquals("Result should have rows", "SSNABC123456", ((string)report.Rows[0]["Org1_OK_CustomsRegNoSSN"]).Trim());
				AssertEquals("Result should have rows", "LG-N123456", ((string)report.Rows[0]["Org1_OK_LoginCompanyEIN"]).Trim());

				AssertEquals("Result should have rows", DBNull.Value, report.Rows[0]["Org2_OH_Code"]);
				AssertEquals("Result should have rows", DBNull.Value, report.Rows[0]["Org2_OK_CustomsRegNoEIN"]);
				AssertEquals("Result should have rows", DBNull.Value, report.Rows[0]["Org2_OK_CustomsRegNoSSN"]);
				AssertEquals("Result should have rows", DBNull.Value, report.Rows[0]["Org2_OK_LoginCompanyEIN"]);
			});
		}

		public void TestExcludeOrgFilter()
		{
			SetUpData();

			var kinyinOrgPK = Guid.Parse("1E9E3616-B55D-4CAC-9EEF-37FAB9B87BC5");
			var orgCode = TestConnection.ExecuteScalar<string>($"SELECT TOP 1 OH_CODE FROM dbo.OrgHeader where OH_PK = '{kinyinOrgPK}'");

			var filterReportQuery = GetReportQueryWithOrgFilter("", orgCode);

			DataTable report = DataUtils.GetDataTableFromQuery(TestConnection, filterReportQuery);

			CombineAssertions(() =>
			{
				AssertEquals("Result should have rows", 1, report.Rows.Count);
				AssertEquals("Result should have rows", "JANSEW", ((string)report.Rows[0]["Org1_OH_Code"]).Trim());
				AssertEquals("Result should have rows", "12-3456789", ((string)report.Rows[0]["Org1_OK_CustomsRegNoEIN"]).Trim());
				AssertEquals("Result should have rows", "SSN123456789", ((string)report.Rows[0]["Org1_OK_CustomsRegNoSSN"]).Trim());
				AssertEquals("Result should have rows", "LG-N123456", ((string)report.Rows[0]["Org1_OK_LoginCompanyEIN"]).Trim());

				AssertEquals("Result should have rows", "PALVAL", ((string)report.Rows[0]["Org2_OH_Code"]).Trim());
				AssertEquals("Result should have rows", "AB-CD12345", ((string)report.Rows[0]["Org2_OK_CustomsRegNoEIN"]).Trim());
				AssertEquals("Result should have rows", "SSNABCD12345", ((string)report.Rows[0]["Org2_OK_CustomsRegNoSSN"]).Trim());
				AssertEquals("Result should have rows", "LG-N123456", ((string)report.Rows[0]["Org2_OK_LoginCompanyEIN"]).Trim());
			});
		}

		void SetUpData()
		{
			InsertIntoOrgCusCode("EIN", "LG-N12345678", TestDbHelper.DefaultCompanyOrgProxyPK);

			var kinyinOrgPK = Guid.Parse("1E9E3616-B55D-4CAC-9EEF-37FAB9B87BC5");
			InsertAccTransactionHeader("00005013", new DateTime(2005, 5, 25), new DateTime(2005, 5, 25), -30, -31, new DateTime(2005, 5, 25), kinyinOrgPK);
			InsertIntoOrgCusCode("EIN", "AB-C12345678", kinyinOrgPK);
			InsertIntoOrgCusCode("SSN", "SSNABC123456", kinyinOrgPK);

			var jansewOrgPK = Guid.Parse("8CC4C7DD-AAE3-4514-8022-093DEC022333");
			InsertAccTransactionHeader("00005014", new DateTime(2005, 5, 26), new DateTime(2005, 5, 26), -40, -41, new DateTime(2005, 5, 26), jansewOrgPK);
			InsertIntoOrgCusCode("EIN", "12-345678901", jansewOrgPK);
			InsertIntoOrgCusCode("SSN", "SSN123456789", jansewOrgPK);

			var palvalOrgPK = Guid.Parse("AB050B5F-2F74-4ABE-84ED-405B46F08007");
			InsertAccTransactionHeader("00005015", new DateTime(2005, 5, 27), new DateTime(2005, 5, 27), -50, -51, new DateTime(2005, 5, 27), palvalOrgPK);
			InsertIntoOrgCusCode("EIN", "AB-CD1234567", palvalOrgPK);
			InsertIntoOrgCusCode("SSN", "SSNABCD12345", palvalOrgPK);
		}

		public void TestQueryOutputTwoOrgPerDataRow()
		{
			SetUpData();

			DataTable report = DataUtils.GetDataTableFromQuery(TestConnection, ReportQuery);

			CombineAssertions(() =>
			{
				AssertEquals("Result should have rows", 2, report.Rows.Count);
				AssertEquals("Result should have rows", "JANSEW", ((string)report.Rows[0]["Org1_OH_Code"]).Trim());
				AssertEquals("Result should have rows", "12-3456789", ((string)report.Rows[0]["Org1_OK_CustomsRegNoEIN"]).Trim());
				AssertEquals("Result should have rows", "SSN123456789", ((string)report.Rows[0]["Org1_OK_CustomsRegNoSSN"]).Trim());
				AssertEquals("Result should have rows", "LG-N123456", ((string)report.Rows[0]["Org1_OK_LoginCompanyEIN"]).Trim());

				AssertEquals("Result should have rows", "KINYIN", ((string)report.Rows[0]["Org2_OH_Code"]).Trim());
				AssertEquals("Result should have rows", "AB-C123456", ((string)report.Rows[0]["Org2_OK_CustomsRegNoEIN"]).Trim());
				AssertEquals("Result should have rows", "SSNABC123456", ((string)report.Rows[0]["Org2_OK_CustomsRegNoSSN"]).Trim());
				AssertEquals("Result should have rows", "LG-N123456", ((string)report.Rows[0]["Org2_OK_LoginCompanyEIN"]).Trim());

				AssertEquals("Result should have rows", "PALVAL", ((string)report.Rows[1]["Org1_OH_Code"]).Trim());
				AssertEquals("Result should have rows", "AB-CD12345", ((string)report.Rows[1]["Org1_OK_CustomsRegNoEIN"]).Trim());
				AssertEquals("Result should have rows", "SSNABCD12345", ((string)report.Rows[1]["Org1_OK_CustomsRegNoSSN"]).Trim());
				AssertEquals("Result should have rows", "LG-N123456", ((string)report.Rows[1]["Org1_OK_LoginCompanyEIN"]).Trim());

				AssertEquals("Result should have rows", DBNull.Value, report.Rows[1]["Org2_OH_Code"]);
				AssertEquals("Result should have rows", DBNull.Value, report.Rows[1]["Org2_OK_CustomsRegNoEIN"]);
				AssertEquals("Result should have rows", DBNull.Value, report.Rows[1]["Org2_OK_CustomsRegNoSSN"]);
				AssertEquals("Result should have rows", DBNull.Value, report.Rows[1]["Org2_OK_LoginCompanyEIN"]);
			});
		}

		void InsertAccTransactionHeader(string transactionNum, DateTime invoiceDate, DateTime dueDate, decimal invoiceAmount, decimal osTotal, DateTime postDate, Guid orgPK)
		{
			string query = @"
INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_TransactionCount,AH_TransactionReference,AH_Desc,AH_InvoiceDate,AH_TransactionCategory,AH_DueDate,AH_InvoiceAmount,AH_GSTAmount,AH_WithholdingTax,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_AgePeriod,AH_PostPeriod,AH_PostDate,AH_ChequeOrReference,AH_ReceiptType,AH_CashBasisGSTIndicator,AH_CashBasisGSTRealisedToGL,AH_ChequeDrawer,AH_DrawerBank,AH_DrawerBranch,AH_InvoiceApproved,AH_ConsolidatedInvoiceRef,AH_FullyPaidDate,AH_InvoicePrinted,AH_IsCancelled,AH_DateClearedInCashbook,AH_NotAllocated,AH_OutstandingAmount,AH_PostedToEFT,AH_PostToGL,AH_ReceiptBatchNo,AH_TransactionCreatedByMatching,AH_InvoiceTerm,AH_InvoiceTermDays,AH_POST1,AH_POST2,AH_POST3,AH_POST4,AH_AB,AH_OH,AH_JH,AH_GB,AH_GE,AH_AG,AH_TransactionBelongsToGroup,AH_AH_InvoiceStatement,AH_PostedInternal,AH_GC,AH_SystemCreateTimeUtc,AH_SystemCreateUser,AH_SystemLastEditTimeUtc,AH_SystemLastEditUser)VALUES(newID(),'AP','PAY',@AH_TransactionNum,1,'','DIRECT PAYMENT',@AH_InvoiceDate,'',@AH_DueDate,@AH_InvoiceAmount,-1.0000,0.0000,@AH_OSTotal,'AUD',1.000000000,0,0,@AH_PostDate,'CASH','CSH',0,0,'CASH','','',0,'',NULL,0,0,NULL,0,0.0000,0,1,'',0,'',0,0,0,0,0,NULL,@AH_OH,NULL,'FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227',NULL,NULL,NULL,0,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP');";

			using (DbCommand command = TestConnection.Command(query))
			{
				command.AddParameterBasedOnDbColumn("@AH_TransactionNum", transactionNum, AccTransactionHeaderSchema.AH_TransactionNum);
				command.AddParameterBasedOnDbColumn("@AH_InvoiceDate", invoiceDate, AccTransactionHeaderSchema.AH_InvoiceDate);
				command.AddParameterBasedOnDbColumn("@AH_DueDate", dueDate, AccTransactionHeaderSchema.AH_DueDate);
				command.AddParameterBasedOnDbColumn("@AH_InvoiceAmount", invoiceAmount, AccTransactionHeaderSchema.AH_InvoiceAmount);
				command.AddParameterBasedOnDbColumn("@AH_OSTotal", osTotal, AccTransactionHeaderSchema.AH_OSTotal);
				command.AddParameterBasedOnDbColumn("@AH_PostDate", postDate, AccTransactionHeaderSchema.AH_PostDate);
				command.AddParameterBasedOnDbColumn("@AH_OH", orgPK, AccTransactionHeaderSchema.AH_OH);
				command.ExecuteNonQuery();
			}
		}

		void InsertIntoOrgCusCode(string codeType, string customsCode, Guid orgPK, Guid? addressPK = null)
		{
			string query = $@"INSERT INTO dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH, OK_OA_PremisesAddress) VALUES (newid(), @OK_CustomsRegNo, @OK_CodeType, 'AU', @OK_OH, {(addressPK.HasValue ? $@"'{addressPK}'" : "NULL")})";

			using (var command = TestConnection.Command(query))
			{
				command.AddParameterBasedOnDbColumn("@OK_CustomsRegNo", customsCode, OrgCusCodeSchema.OK_CustomsRegNo);
				command.AddParameterBasedOnDbColumn("@OK_CodeType", codeType, OrgCusCodeSchema.OK_CodeType);
				command.AddParameterBasedOnDbColumn("@OK_OH", orgPK, OrgCusCodeSchema.OK_OH);
				command.ExecuteNonQuery();
			}
		}

		void UpdateOrg(Guid orgPk, string code, string fullname)
		{
			string query = string.Format("UPDATE {0} SET {1} = @OH_Code, {2} = @OH_FullName WHERE {3} = @OH_PK",
				OrgHeaderSchema.Constants.TableName,
				OrgHeaderSchema.OH_Code.Name,
				OrgHeaderSchema.OH_FullName.Name,
				OrgHeaderSchema.PK.Name);
			using (DbCommand command = TestConnection.Command(query))
			{
				command.AddParameterBasedOnDbColumn("@OH_PK", orgPk, OrgHeaderSchema.PK);
				command.AddParameterBasedOnDbColumn("@OH_Code", code, OrgHeaderSchema.OH_Code);
				command.AddParameterBasedOnDbColumn("@OH_FullName", fullname, OrgHeaderSchema.OH_FullName);
				command.ExecuteNonQuery();
			}
		}

		void UpdateOrgAddress(Guid orgPK, string address1, string city)
		{
			string query = string.Format("UPDATE {0} SET {1} = @OA_Address1, {2} = @OA_City WHERE {3} = @OA_OH",
				OrgAddressSchema.Constants.TableName,
				OrgAddressSchema.OA_Address1.Name,
				OrgAddressSchema.OA_City.Name,
				OrgAddressSchema.OA_OH.Name);
			using (DbCommand command = TestConnection.Command(query))
			{
				command.AddParameterBasedOnDbColumn("@OA_Address1", address1, OrgAddressSchema.OA_Address1);
				command.AddParameterBasedOnDbColumn("@OA_City", city, OrgAddressSchema.OA_City);
				command.AddParameterBasedOnDbColumn("@OA_OH", orgPK, OrgAddressSchema.OA_OH);
				command.ExecuteNonQuery();
			}
		}

		void UpdateOrgCusCode(Guid orgPK, string customsRegNo)
		{
			string query = string.Format("UPDATE {0} SET {1} = @OK_CustomsRegNo WHERE {2} = @OK_OH",
				OrgCusCodeSchema.Constants.TableName,
				OrgCusCodeSchema.OK_CustomsRegNo.Name,
				OrgCusCodeSchema.OK_OH.Name);
			using (DbCommand command = TestConnection.Command(query))
			{
				command.AddParameterBasedOnDbColumn("@OK_CustomsRegNo", customsRegNo, OrgCusCodeSchema.OK_CustomsRegNo);
				command.AddParameterBasedOnDbColumn("@OK_OH", orgPK, OrgCusCodeSchema.PK);
				command.ExecuteNonQuery();
			}
		}
	}
}

