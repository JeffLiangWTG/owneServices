using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.Org;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org
{
	[TestedType(typeof(Report_ListofEXVDocuments))]
	class Report_ListofEXVDocumentsTest : DbCreateScriptTest
	{
		//[ExpectNoExceptions]
		public void TestSampleCall()
		{
			var companyPK = "149ADD70-6FB5-4BAC-9A03-3E890C7B5253";
			var orgPK = "4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392";
			var taxId = Guid.NewGuid();

			var insertSql = @$"
			DECLARE @OrgPK1 UNIQUEIDENTIFIER = '{orgPK}';
			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPK1, 'TESTORG1');
			DECLARE @CompanyPK UNIQUEIDENTIFIER = '{companyPK}';
			INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyPK, 'XIT', 'IT company', 'IT', 'EUR');
			DECLARE @BranchPK UNIQUEIDENTIFIER = '27A55065-AC88-4EC3-8BED-E575E79172CB';
			DECLARE @DepartmentPK UNIQUEIDENTIFIER = '86BB1C22-0865-4685-996E-D56CBD136491';
			DECLARE @TransactionPK1 UNIQUEIDENTIFIER = 'D0D65D63-9C27-4CCB-9120-321703A017D3';
			DECLARE @JobReq1 UNIQUEIDENTIFIER = '7803F853-B05A-4091-B603-985567C2D373';
			INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DocNumber, EQ_DateReceived, EQ_ValidToDate, EQ_DocumentNotes, EQ_DocType, EQ_DocCategory, EQ_DocUsage, EQ_RN_NKRelatedCountry, EQ_ParentTableCode, EQ_ParentID) VALUES (@JobReq1, '1234A', '2024-01-01', '2024-08-01', 'Special Notes', 'EXV', 'CSR', 'CRT', 'IT', 'OH', '{orgPK}')
			INSERT INTO dbo.JobRequiredDocAttrib(D0_PK, D0_EQ, D0_AttribName, D0_AttribValue) VALUES (newid(), @JobReq1, 'COMPANY CODE', '{companyPK}')
			INSERT INTO dbo.AccTaxRate(AT_PK, AT_Code, AT_Description, AT_Type,AT_ExtraTaxRateType,AT_RN_NKCountry,AT_IsActive,AT_PostingGroupId,AT_ReferenceExtraRateType,AT_ReferenceRateType,AT_RateSource,AT_TaxSystemCode,AT_AutoVersion,AT_SystemCreateUser,AT_SystemLastEditUser) VALUES ('{taxId}', 'DICH.INT', 'Zero Rated Exporter Exemption', 'RAT','','IT',1,0,'','ZERO','TID','',1,'E','E')
			";

			TestConnection.ExecuteNonQuery(insertSql);

			var postDate = new DateTime(2024, 01, 15);
			var oldYearDate = new DateTime(2023, 12, 15);
			var postDateSTR = postDate.ToString("yyyyMMdd");
			var oldYearSTR = oldYearDate.ToString("yyyyMMdd");
			var tempPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(@$"INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_PostDate, AH_GB, AH_GC, AH_GE, AH_TransactionCount, AH_OH) 
				values('{tempPK}', 'AP', 'INV', '00001001', 'Test_Desc', '{postDateSTR}', '{postDateSTR}', '{postDateSTR}', 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', '{companyPK}', '4E5A97E8-85F4-41EC-95C6-5D14A676157C', 1, '{orgPK}')");

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccTransactionLines(AL_PK, AL_LineType, AL_LineAmount, AL_AH, AL_AG, AL_GC, AL_GB, AL_GE, AL_AT)
									VALUES(NEWID(), 'CST', -10.00, '{tempPK}', 'ac129d82-b88d-45ee-bce5-25592f734023', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', '2B67864D-42E9-4A43-A9C8-09D2083C4227', '{taxId}')");

			tempPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(@$"INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_PostDate, AH_GB, AH_GC, AH_GE, AH_TransactionCount, AH_OH) 
				values('{tempPK}', 'AP', 'INV', '00001002', 'Test_Desc', '{oldYearSTR}', '{oldYearSTR}', '{postDateSTR}', 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', '{companyPK}', '4E5A97E8-85F4-41EC-95C6-5D14A676157C', 1, '{orgPK}')");

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.AccTransactionLines(AL_PK, AL_LineType, AL_LineAmount, AL_AH, AL_AG, AL_GC, AL_GB, AL_GE, AL_AT)
									VALUES(NEWID(), 'CST', -10.00, '{tempPK}', 'ac129d82-b88d-45ee-bce5-25592f734023', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', '2B67864D-42E9-4A43-A9C8-09D2083C4227', '{taxId}')");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM Report_ListofEXVDocuments ('{companyPK}', '20240101', '20241231')");

			AssertEquals("Waited lines", 1, result.Rows.Count);

			AssertEquals(20M, result.Rows[0]["TotalDuringValidPeriod"]);
			AssertEquals(10M, result.Rows[0]["TotalDuringValidPeriodByInvoiceDate"]);
		}
	}
}
