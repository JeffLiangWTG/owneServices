using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterData.ComplianceRisk;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.MasterData.ComplianceRisk
{
	[TestedType(typeof(Report_Shipment_ComplianceRiskSummaryReport))]
	class Report_Shipment_ComplianceRiskSummaryReportTest : DbCreateScriptTest
	{
		#region Departed Report

		public void Test_Departed_ColumnNamesAndRowCount()
		{
			TestConnection.ExecuteNonQuery(GetSQL);
			const string sqlTemplate = @"EXEC Report_Shipment_ComplianceRiskSummaryReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = '2023-02-01', @ETDTo = '2023-05-30', @ETAFrom = '2023-01-01', @ETATo = '2023-04-30', @Today = '2023-05-15', @ReportType = 'Departed', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'ALL', @GroupBy = '{0}'";
			var reportSql1 = string.Format(CultureInfo.InvariantCulture, sqlTemplate, "OCR");
			var result1 = DataUtils.GetDataTableFromQuery(TestConnection, reportSql1);
			AssertColumnsAndRow(result1, 4, "[[\"Held\",1,\"25%\",1,\"25%\",2,\"50%\",0,\"0%\",1,\"25%\",1,\"25%\",2,\"33%\",8,\"26%\"],[\"Override Clear\",1,\"25%\",0,\"0%\",0,\"0%\",1,\"25%\",1,\"25%\",0,\"0%\",1,\"16%\",4,\"13%\"],[\"Clear\",0,\"0%\",1,\"25%\",0,\"0%\",1,\"25%\",0,\"0%\",1,\"25%\",0,\"0%\",3,\"10%\"],[\"No Status\",2,\"50%\",2,\"50%\",2,\"50%\",2,\"50%\",2,\"50%\",2,\"50%\",3,\"50%\",15,\"50%\"]]");

			var reportSql2 = string.Format(CultureInfo.InvariantCulture, sqlTemplate, "COU");
			var result2 = DataUtils.GetDataTableFromQuery(TestConnection, reportSql2);
			AssertColumnsAndRow(result2, 2, "[[\"Australia\",1,\"100%\",0,\"0%\",2,\"100%\",0,\"0%\",1,\"100%\",1,\"100%\",0,\"0%\",5,\"62%\"],[\"China\",0,\"0%\",1,\"100%\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",2,\"100%\",3,\"37%\"]]");

			var reportSql3 = string.Format(CultureInfo.InvariantCulture, sqlTemplate, "CAS");
			var result3 = DataUtils.GetDataTableFromQuery(TestConnection, reportSql3);
			AssertColumnsAndRow(result3, 4, "[[\"Not started\",0,\"0%\",2,\"50%\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",2,\"6%\"],[\"Commenced\",2,\"50%\",0,\"0%\",1,\"25%\",2,\"50%\",2,\"50%\",2,\"50%\",3,\"50%\",12,\"40%\"],[\"Declined\",0,\"0%\",0,\"0%\",1,\"25%\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",1,\"3%\"],[\"Unknown\",2,\"50%\",2,\"50%\",2,\"50%\",2,\"50%\",2,\"50%\",2,\"50%\",3,\"50%\",15,\"50%\"]]");

			void AssertColumnsAndRow(DataTable result, int rows, string expectedData)
			{
				var data = JsonConvert.SerializeObject(result.AsEnumerable().Select(r => r.ItemArray));

				CombineAssertions(() =>
				{
					Assert("Has a column named RowHeading", result1.Columns.Contains("RowHeading"));
					Assert("Has a column named AIR", result1.Columns.Contains("AIR"));
					Assert("Has a column named AIR_PER", result1.Columns.Contains("AIR_PER"));
					Assert("Has a column named SEA", result1.Columns.Contains("SEA"));
					Assert("Has a column named SEA_PER", result1.Columns.Contains("SEA_PER"));
					Assert("Has a column named ROA", result1.Columns.Contains("ROA"));
					Assert("Has a column named ROA_PER", result1.Columns.Contains("ROA_PER"));
					Assert("Has a column named RAI", result1.Columns.Contains("RAI"));
					Assert("Has a column named RAI_PER", result1.Columns.Contains("RAI_PER"));
					Assert("Has a column named COU", result1.Columns.Contains("COU"));
					Assert("Has a column named COU_PER", result1.Columns.Contains("COU_PER"));
					Assert("Has a column named FSA", result1.Columns.Contains("FSA"));
					Assert("Has a column named FSA_PER", result1.Columns.Contains("FSA_PER"));
					Assert("Has a column named FAS", result1.Columns.Contains("FAS"));
					Assert("Has a column named FAS_PER", result1.Columns.Contains("FAS_PER"));
					Assert("Has a column named TOTAL", result1.Columns.Contains("TOTAL"));
					Assert("Has a column named TOTAL_PER", result1.Columns.Contains("TOTAL_PER"));

					AssertEquals(rows, result.Rows.Count);
					AssertEquals(17, result.Columns.Count);
					AssertEquals(expectedData, data);
				});
			}
		}

		public void Test_Departed_WithETDAndETA()
		{
			TestConnection.ExecuteNonQuery(GetSQL);
			var reportSql = @"EXEC Report_Shipment_ComplianceRiskSummaryReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = '2023-02-01', @ETDTo = '2023-03-30', @ETAFrom = '2023-04-01', @ETATo = '2023-04-30', @Today = '2023-03-15', @ReportType = 'Departed', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'ALL', @GroupBy = 'OCR'";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			AssertEquals(3, result.Rows.Count);
			var data = JsonConvert.SerializeObject(result.AsEnumerable().Select(r => r.ItemArray));
			AssertEquals("[[\"Held\",1,\"50%\",1,\"50%\",2,\"100%\",0,\"0%\",1,\"50%\",1,\"50%\",1,\"50%\",7,\"50%\"],[\"Override Clear\",1,\"50%\",0,\"0%\",0,\"0%\",1,\"50%\",1,\"50%\",0,\"0%\",1,\"50%\",4,\"28%\"],[\"Clear\",0,\"0%\",1,\"50%\",0,\"0%\",1,\"50%\",0,\"0%\",1,\"50%\",0,\"0%\",3,\"21%\"]]", data);
		}

		public void Test_Departed_WithETD()
		{
			TestConnection.ExecuteNonQuery(GetSQL);
			var reportSql = @"EXEC Report_Shipment_ComplianceRiskSummaryReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = '2023-02-01', @ETDTo = '2023-03-30', @ETAFrom = null, @ETATo = null, @Today = '2023-03-15', @ReportType = 'Departed', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'ALL', @GroupBy = 'OCR'";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			AssertEquals(3, result.Rows.Count);
			var data = JsonConvert.SerializeObject(result.AsEnumerable().Select(r => r.ItemArray));
			AssertEquals("[[\"Held\",1,\"50%\",1,\"50%\",2,\"100%\",0,\"0%\",1,\"50%\",1,\"50%\",1,\"50%\",7,\"50%\"],[\"Override Clear\",1,\"50%\",0,\"0%\",0,\"0%\",1,\"50%\",1,\"50%\",0,\"0%\",1,\"50%\",4,\"28%\"],[\"Clear\",0,\"0%\",1,\"50%\",0,\"0%\",1,\"50%\",0,\"0%\",1,\"50%\",0,\"0%\",3,\"21%\"]]", data);
		}

		public void Test_Departed_WithETA()
		{
			TestConnection.ExecuteNonQuery(GetSQL);
			var reportSql = @"EXEC Report_Shipment_ComplianceRiskSummaryReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = null, @ETDTo = null, @ETAFrom = '2023-04-08', @ETATo = '2023-04-30', @Today = '2023-03-15', @ReportType = 'Departed', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'ALL', @GroupBy = 'OCR'";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			AssertEquals(3, result.Rows.Count);
			var data = JsonConvert.SerializeObject(result.AsEnumerable().Select(r => r.ItemArray));
			AssertEquals("[[\"Held\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",1,\"50%\",1,\"50%\",1,\"50%\",3,\"42%\"],[\"Override Clear\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",1,\"50%\",0,\"0%\",1,\"50%\",2,\"28%\"],[\"Clear\",0,\"0%\",0,\"0%\",0,\"0%\",1,\"100%\",0,\"0%\",1,\"50%\",0,\"0%\",2,\"28%\"]]", data);
		}

		#endregion Departed Report

		#region Planned Report

		public void Test_Planned_ColumnNamesAndRowCount()
		{
			TestConnection.ExecuteNonQuery(GetSQL);
			const string sqlTemplate = @"EXEC Report_Shipment_ComplianceRiskSummaryReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = '2023-03-20', @ETDTo = '2023-03-30', @ETAFrom = '2023-04-19', @ETATo = '2023-04-30', @Today = '2023-03-15', @ReportType = 'Planned', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'ALL', @GroupBy = '{0}'";
			var reportSql1 = string.Format(CultureInfo.InvariantCulture, sqlTemplate, "OCR");
			var result1 = DataUtils.GetDataTableFromQuery(TestConnection, reportSql1);
			AssertColumnsAndRow(result1, 2, "[[\"Held\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",1,\"33%\",1,\"9%\"],[\"No Status\",0,\"0%\",0,\"0%\",2,\"100%\",2,\"100%\",2,\"100%\",2,\"100%\",2,\"66%\",10,\"90%\"]]");

			var reportSql2 = string.Format(CultureInfo.InvariantCulture, sqlTemplate, "COU");
			var result2 = DataUtils.GetDataTableFromQuery(TestConnection, reportSql2);
			AssertColumnsAndRow(result2, 1, "[[\"China\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",1,\"100%\",1,\"100%\"]]");

			var reportSql3 = string.Format(CultureInfo.InvariantCulture, sqlTemplate, "CAS");
			var result3 = DataUtils.GetDataTableFromQuery(TestConnection, reportSql3);
			AssertColumnsAndRow(result3, 2, "[[\"Commenced\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",1,\"33%\",1,\"9%\"],[\"Unknown\",0,\"0%\",0,\"0%\",2,\"100%\",2,\"100%\",2,\"100%\",2,\"100%\",2,\"66%\",10,\"90%\"]]");

			void AssertColumnsAndRow(DataTable result, int rows, string expectedData)
			{
				var data = JsonConvert.SerializeObject(result.AsEnumerable().Select(r => r.ItemArray));

				CombineAssertions(() =>
				{
					Assert("Has a column named RowHeading", result1.Columns.Contains("RowHeading"));
					Assert("Has a column named AIR", result1.Columns.Contains("AIR"));
					Assert("Has a column named AIR_PER", result1.Columns.Contains("AIR_PER"));
					Assert("Has a column named SEA", result1.Columns.Contains("SEA"));
					Assert("Has a column named SEA_PER", result1.Columns.Contains("SEA_PER"));
					Assert("Has a column named ROA", result1.Columns.Contains("ROA"));
					Assert("Has a column named ROA_PER", result1.Columns.Contains("ROA_PER"));
					Assert("Has a column named RAI", result1.Columns.Contains("RAI"));
					Assert("Has a column named RAI_PER", result1.Columns.Contains("RAI_PER"));
					Assert("Has a column named COU", result1.Columns.Contains("COU"));
					Assert("Has a column named COU_PER", result1.Columns.Contains("COU_PER"));
					Assert("Has a column named FSA", result1.Columns.Contains("FSA"));
					Assert("Has a column named FSA_PER", result1.Columns.Contains("FSA_PER"));
					Assert("Has a column named FAS", result1.Columns.Contains("FAS"));
					Assert("Has a column named FAS_PER", result1.Columns.Contains("FAS_PER"));
					Assert("Has a column named TOTAL", result1.Columns.Contains("TOTAL"));
					Assert("Has a column named TOTAL_PER", result1.Columns.Contains("TOTAL_PER"));

					AssertEquals(rows, result.Rows.Count);
					AssertEquals(17, result.Columns.Count);
					AssertEquals(expectedData, data);
				});
			}
		}

		public void Test_Planned_WithETDAndETA()
		{
			TestConnection.ExecuteNonQuery(GetSQL);
			var reportSql = @"EXEC Report_Shipment_ComplianceRiskSummaryReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = '2023-02-01', @ETDTo = '2023-03-30', @ETAFrom = '2023-04-19', @ETATo = '2023-04-30', @Today = '2023-03-15', @ReportType = 'Planned', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'ALL', @GroupBy = 'OCR'";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);
			var data = JsonConvert.SerializeObject(result.AsEnumerable().Select(r => r.ItemArray));

			AssertEquals("[[\"Held\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",1,\"33%\",1,\"8%\"],[\"No Status\",0,\"0%\",1,\"100%\",2,\"100%\",2,\"100%\",2,\"100%\",2,\"100%\",2,\"66%\",11,\"91%\"]]", data);
		}

		public void Test_Planned_WithETD()
		{
			TestConnection.ExecuteNonQuery(GetSQL);
			var reportSql = @"EXEC Report_Shipment_ComplianceRiskSummaryReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = '2023-03-01', @ETDTo = '2023-03-30', @ETAFrom = null, @ETATo = null, @Today = '2023-03-15', @ReportType = 'Planned', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'ALL', @GroupBy = 'OCR'";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);
			var data = JsonConvert.SerializeObject(result.AsEnumerable().Select(r => r.ItemArray));

			AssertEquals("[[\"Held\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",1,\"25%\",1,\"6%\"],[\"No Status\",2,\"100%\",2,\"100%\",2,\"100%\",2,\"100%\",2,\"100%\",2,\"100%\",3,\"75%\",15,\"93%\"]]", data);
		}

		public void Test_Planned_WithETA()
		{
			TestConnection.ExecuteNonQuery(GetSQL);
			var reportSql = @"EXEC Report_Shipment_ComplianceRiskSummaryReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = null, @ETDTo = null, @ETAFrom = '2023-04-15', @ETATo = '2023-04-30', @Today = '2023-03-15', @ReportType = 'Planned', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'ALL', @GroupBy = 'OCR'";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);
			var data = JsonConvert.SerializeObject(result.AsEnumerable().Select(r => r.ItemArray));

			AssertEquals("[[\"Held\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",1,\"25%\",1,\"6%\"],[\"No Status\",2,\"100%\",2,\"100%\",2,\"100%\",2,\"100%\",2,\"100%\",2,\"100%\",3,\"75%\",15,\"93%\"]]", data);
		}

		#endregion

		public void Test_Summary_WithSingleShipmentLinkedWithMultipleJobHeadersUsingCurrentCompany()
		{
			TestConnection.ExecuteNonQuery(GetSQL);
			var reportSql = @"EXEC Report_Shipment_ComplianceRiskSummaryReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = '2023-02-01', @ETDTo = '2023-03-02', @ETAFrom = null, @ETATo = null, @Today = '2023-03-13', @ReportType = 'Departed', @Consignor = '', @Consignee = '',@ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'ALL', @GroupBy = 'OCR'";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);
			var data = JsonConvert.SerializeObject(result.AsEnumerable().Select(r => r.ItemArray));
			AssertEquals("[[\"Held\",1,\"50%\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",1,\"50%\"],[\"Override Clear\",1,\"50%\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",0,\"0%\",1,\"50%\"]]", data);
		}

		public void Test_Summary_ShowExportJob()
		{
			TestConnection.ExecuteNonQuery(GetSQL);
			var reportSql = @"EXEC Report_Shipment_ComplianceRiskSummaryReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = '2023-03-01', @ETDTo = '2023-03-30', @ETAFrom = null, @ETATo = null, @Today = '2023-03-30', @ReportType = 'Departed', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'EXP', @GroupBy = 'OCR'";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);
			var data = JsonConvert.SerializeObject(result.AsEnumerable().Select(r => r.ItemArray));
			AssertEquals("[[\"Held\",1,\"25%\",0,\"0%\",2,\"66%\",0,\"0%\",1,\"33%\",1,\"33%\",0,\"0%\",5,\"25%\"],[\"Override Clear\",1,\"25%\",0,\"0%\",0,\"0%\",0,\"0%\",1,\"33%\",0,\"0%\",1,\"33%\",3,\"15%\"],[\"Clear\",0,\"0%\",1,\"100%\",0,\"0%\",1,\"33%\",0,\"0%\",1,\"33%\",0,\"0%\",3,\"15%\"],[\"No Status\",2,\"50%\",0,\"0%\",1,\"33%\",2,\"66%\",1,\"33%\",1,\"33%\",2,\"66%\",9,\"45%\"]]", data);
		}

		string GetSQL
		{
			get
			{
				var orgHeader1PK = Guid.NewGuid();
				var orgHeader2PK = Guid.NewGuid();
				var orgHeaderLocalPK = Guid.NewGuid();
				var orgAddress1PK = Guid.NewGuid();
				var orgAddress2PK = Guid.NewGuid();
				var orgAddressLocalPK = Guid.NewGuid();
				var shipmentPKs = new Guid[30];

				for (var i = 0; i < 30; i++)
				{
					shipmentPKs[i] = Guid.NewGuid();
				}

				return $@"
DECLARE @CompanyPk AS UNIQUEIDENTIFIER = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D'
DECLARE @CompanyPk1 AS UNIQUEIDENTIFIER = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68C'
DECLARE @BranchPk AS UNIQUEIDENTIFIER = '3C282841-E41C-4CBD-9A98-15ED3C775433'
DECLARE @BranchPk1 AS UNIQUEIDENTIFIER = '3C282841-E41C-4CBD-9A98-15ED3C775432'
DECLARE @DepartmentPk AS UNIQUEIDENTIFIER = 'A433C594-8C39-46D2-B601-30877A9F69FB'
DECLARE @DepartmentPk1 AS UNIQUEIDENTIFIER = 'A433C594-8C39-46D2-B601-30877A9F69FA'

INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name) VALUES (@CompanyPk, 'DAN', 'AU company')
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC) VALUES (@BranchPk, @CompanyPk)
INSERT INTO dbo.GlbDepartment (GE_PK) VALUES (@DepartmentPk)

INSERT INTO dbo.GlbCompany (GC_PK, GC_CODE, GC_Name) VALUES (@CompanyPk1, 'COM', 'AU company')
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_CODE) VALUES (@BranchPk1, @CompanyPk1, 'ABC')
INSERT INTO dbo.GlbDepartment (GE_PK, GE_CODE) VALUES (@DepartmentPk1, 'DEF')

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_IsConsignor, OH_OverrideAdditionalAddressInformation) VALUES ('{orgHeader1PK}', 'QANAIR_WW', 'QANTAS AIRWAYS', 1, 1)
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_IsConsignee, OH_OverrideAdditionalAddressInformation) VALUES ('{orgHeader2PK}', 'INDIA_WW', 'AIR INDIA', 1, 1)
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_IsConsignee, OH_OverrideAdditionalAddressInformation) VALUES ('{orgHeaderLocalPK}', 'LOCAL CLIENT', 'LOCAL CLIENT', 1, 1)

INSERT INTO dbo.OrgAddress (OA_PK, OA_Address1, OA_State, OA_PostCode, OA_OH, OA_RL_NKRelatedPortCode, OA_RN_NKCountryCode, OA_City, OA_AdditionalAddressInformation) VALUES ('{orgAddress1PK}', 'CNR QANTAS DRIVE & LINK ROAD', 'NSW', '2000', '{orgHeader1PK}', 'AUSYD', 'AU', 'SYDNEY', 'GEORGE STREET BUILDING')
INSERT INTO dbo.OrgAddress (OA_PK, OA_Address1, OA_State, OA_PostCode, OA_OH, OA_RL_NKRelatedPortCode, OA_RN_NKCountryCode, OA_City, OA_AdditionalAddressInformation) VALUES ('{orgAddress2PK}', 'CNR QANTAS DRIVE & LINK ROAD', 'NSW', '2000', '{orgHeader2PK}', 'INBLR', 'IN', 'Bangalore', 'Brigade')
INSERT INTO dbo.OrgAddress (OA_PK, OA_Address1, OA_State, OA_PostCode, OA_OH, OA_RL_NKRelatedPortCode, OA_RN_NKCountryCode, OA_City, OA_AdditionalAddressInformation) VALUES ('{orgAddressLocalPK}', 'CNR QANTAS DRIVE & LINK ROAD', 'NSW', '2000', '{orgHeaderLocalPK}', 'INBLR', 'IN', 'Bangalore', 'Brigade')

INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '00000001','{shipmentPKs[0]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', '{orgAddressLocalPK}')
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '00000001','{shipmentPKs[0]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk1, 'WRK', '{orgAddressLocalPK}')

INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr)
VALUES
(NEWID(), '00000002','{shipmentPKs[1]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '00000003','{shipmentPKs[2]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '00000004','{shipmentPKs[3]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '00000005','{shipmentPKs[4]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '00000006','{shipmentPKs[5]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '00000007','{shipmentPKs[6]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '00000008','{shipmentPKs[7]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '00000009','{shipmentPKs[8]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '000000010','{shipmentPKs[9]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '000000011','{shipmentPKs[10]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '000000012','{shipmentPKs[11]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '000000013','{shipmentPKs[12]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '000000014','{shipmentPKs[13]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '000000015','{shipmentPKs[14]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '000000016','{shipmentPKs[15]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '000000017','{shipmentPKs[16]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '000000018','{shipmentPKs[17]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '000000019','{shipmentPKs[18]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '000000020','{shipmentPKs[19]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '000000021','{shipmentPKs[20]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '000000022','{shipmentPKs[21]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '000000023','{shipmentPKs[22]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '000000024','{shipmentPKs[23]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '000000025','{shipmentPKs[24]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '000000026','{shipmentPKs[25]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '000000027','{shipmentPKs[26]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '000000028','{shipmentPKs[27]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '000000029','{shipmentPKs[28]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL),
(NEWID(), '000000030','{shipmentPKs[29]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)

INSERT INTO dbo.JobShipment
(JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination)
VALUES
('{shipmentPKs[0]}', 'S00000001', '2023-03-01', '2023-04-01', 'AIR', 'AU', 'IN'),
('{shipmentPKs[1]}', 'S00000002', '2023-03-02', '2023-04-02', 'AIR', 'AU', 'IN'),
('{shipmentPKs[2]}', 'S00000003', '2023-03-03', '2023-04-03', 'SEA', 'AU', 'IN'),
('{shipmentPKs[3]}', 'S00000004', '2023-03-04', '2023-04-04', 'SEA', 'CN', 'IN'),
('{shipmentPKs[4]}', 'S00000005', '2023-03-05', '2023-04-05', 'ROA', 'AU', 'IN'),
('{shipmentPKs[5]}', 'S00000006', '2023-03-06', '2023-04-06', 'ROA', 'AU', 'IN'),
('{shipmentPKs[6]}', 'S00000007', '2023-03-07', '2023-04-07', 'RAI', 'CN', 'IN'),
('{shipmentPKs[7]}', 'S00000008', '2023-03-08', '2023-04-08', 'RAI', 'AU', 'IN'),
('{shipmentPKs[8]}', 'S00000009', '2023-03-09', '2023-04-09', 'COU', 'AU', 'IN'),
('{shipmentPKs[9]}', 'S00000010', '2023-03-10', '2023-04-10', 'COU', 'AU', 'IN'),
('{shipmentPKs[10]}', 'S00000011', '2023-03-11', '2023-04-11', 'FSA', 'AU', 'IN'),
('{shipmentPKs[11]}', 'S00000012', '2023-03-12', '2023-04-12', 'FSA', 'AU', 'IN'),
('{shipmentPKs[12]}', 'S00000013', '2023-03-13', '2023-04-13', 'FAS', 'AU', 'IN'),
('{shipmentPKs[13]}', 'S00000014', '2023-03-14', '2023-04-14', 'FAS', 'CN', 'IN'),
('{shipmentPKs[14]}', 'S00000015', '2023-03-15', '2023-04-15', 'FAS', 'AU', 'IN'),
('{shipmentPKs[15]}', 'S00000016', '2023-03-16', '2023-04-16', 'AIR', 'AU', 'IN'),
('{shipmentPKs[16]}', 'S00000017', '2023-03-17', '2023-04-17', 'AIR', 'AU', 'IN'),
('{shipmentPKs[17]}', 'S00000018', '2023-03-18', '2023-04-18', 'SEA', 'IN', 'IN'),
('{shipmentPKs[18]}', 'S00000019', '2023-03-19', '2023-04-19', 'SEA', 'IN', 'IN'),
('{shipmentPKs[19]}', 'S00000020', '2023-03-20', '2023-04-20', 'ROA', 'IN', 'IN'),
('{shipmentPKs[20]}', 'S00000021', '2023-03-21', '2023-04-21', 'ROA', 'AU', 'IN'),
('{shipmentPKs[21]}', 'S00000022', '2023-03-22', '2023-04-22', 'RAI', 'AU', 'IN'),
('{shipmentPKs[22]}', 'S00000023', '2023-03-23', '2023-04-23', 'RAI', 'AU', 'IN'),
('{shipmentPKs[23]}', 'S00000024', '2023-03-24', '2023-04-24', 'COU', 'IN', 'IN'),
('{shipmentPKs[24]}', 'S00000025', '2023-03-25', '2023-04-25', 'COU', 'AU', 'IN'),
('{shipmentPKs[25]}', 'S00000026', '2023-03-26', '2023-04-26', 'FSA', 'AU', 'IN'),
('{shipmentPKs[26]}', 'S00000027', '2023-03-27', '2023-04-27', 'FSA', 'IN', 'IN'),
('{shipmentPKs[27]}', 'S00000028', '2023-03-28', '2023-04-28', 'FAS', 'AU', 'IN'),
('{shipmentPKs[28]}', 'S00000029', '2023-03-29', '2023-04-29', 'FAS', 'CN', 'IN'),
('{shipmentPKs[29]}', 'S00000030', '2023-03-30', '2023-04-30', 'FAS', 'AU', 'IN')

INSERT INTO [dbo].[JobConsolTransport]
([JW_PK], [JW_TransportMode], [JW_Vessel], [JW_VoyageFlight], [JW_ETD], [JW_ETA], [JW_RL_NKLoadPort], [JW_RL_NKDiscPort], [JW_ParentGUID])
VALUES
(newid(), 'AIR', 'Vessel', 'Flight', '2023-02-01', '2023-04-01','AUBNE', 'CNTAI', '{shipmentPKs[0]}'),
(newid(), 'AIR', 'Vessel', 'Flight', '2023-03-02', '2023-04-02','KWSMY', 'USEZX', '{shipmentPKs[1]}'),
(newid(), 'AIR', 'Vessel', 'Flight', '2023-03-03', '2023-04-03','USEZX', 'AUBNE', '{shipmentPKs[1]}'),
(newid(), 'SEA', 'Vessel', 'Flight', '2023-03-04', '2023-04-04','AUBNE', 'AUBNE', '{shipmentPKs[3]}'),
(newid(), 'ROA', 'Vessel', 'Flight', '2023-03-05', '2023-04-05','AUBNE', 'USCHI', '{shipmentPKs[4]}'),
(newid(), 'ROA', 'Vessel', 'Flight', '2023-03-06', '2023-04-06','CNTAI', 'USCHI', '{shipmentPKs[5]}'),
(newid(), 'RAI', 'Vessel', 'Flight', '2023-03-07', '2023-04-07','AUBNE', 'KWSMY', '{shipmentPKs[6]}'),
(newid(), 'RAI', 'Vessel', 'Flight', '2023-03-08', '2023-04-08','AUBNE', 'USEZX', '{shipmentPKs[7]}'),
(newid(), 'COU', 'Vessel', 'Flight', '2023-03-09', '2023-04-09','AUBNE', 'CNTAI', '{shipmentPKs[8]}'),
(newid(), 'COU', 'Vessel', 'Flight', '2023-03-10', '2023-04-10','AUBNE', 'CNTAI', '{shipmentPKs[9]}'),
(newid(), 'FSA', 'Vessel', 'Flight', '2023-03-11', '2023-04-11','AUBNE', 'AUBNE', '{shipmentPKs[10]}'),
(newid(), 'FSA', 'Vessel', 'Flight', '2023-03-12', '2023-04-12','AUBNE', 'USEZX', '{shipmentPKs[11]}'),
(newid(), 'FAS', 'Vessel', 'Flight', '2023-03-13', '2023-04-13','AUBNE', 'USCHI', '{shipmentPKs[12]}'),
(newid(), 'FAS', 'Vessel', 'Flight', '2023-03-14', '2023-04-14','AUBNE', 'AUBNE', '{shipmentPKs[13]}'),
(newid(), 'AIR', 'Vessel', 'Flight', '2023-03-15', '2023-04-15','AUBNE', 'AUBNE', '{shipmentPKs[14]}'),
(newid(), 'AIR', 'Vessel', 'Flight', '2023-03-16', '2023-04-16','AUBNE', 'AUBNE', '{shipmentPKs[15]}'),
(newid(), 'SEA', 'Vessel', 'Flight', '2023-03-17', '2023-04-17','AUBNE', 'KWSMY', '{shipmentPKs[16]}'),
(newid(), 'SEA', 'Vessel', 'Flight', '2023-03-18', '2023-04-18','AUBNE', 'CNTAI', '{shipmentPKs[17]}'),
(newid(), 'ROA', 'Vessel', 'Flight', '2023-03-19', '2023-04-19','AUBNE', 'AUBNE', '{shipmentPKs[18]}'),
(newid(), 'ROA', 'Vessel', 'Flight', '2023-03-20', '2023-04-20','AUBNE', 'KWSMY', '{shipmentPKs[19]}'),
(newid(), 'RAI', 'Vessel', 'Flight', '2023-03-21', '2023-04-21','AUBNE', 'USCHI', '{shipmentPKs[20]}'),
(newid(), 'RAI', 'Vessel', 'Flight', '2023-03-22', '2023-04-22','AUBNE', 'AUBNE', '{shipmentPKs[21]}'),
(newid(), 'COU', 'Vessel', 'Flight', '2023-03-23', '2023-04-23','AUBNE', 'KWSMY', '{shipmentPKs[22]}'),
(newid(), 'COU', 'Vessel', 'Flight', '2023-03-24', '2023-04-24','AUBNE', 'AUBNE', '{shipmentPKs[23]}'),
(newid(), 'FSA', 'Vessel', 'Flight', '2023-03-25', '2023-04-25','AUBNE', 'KWSMY', '{shipmentPKs[24]}'),
(newid(), 'FSA', 'Vessel', 'Flight', '2023-03-26', '2023-04-26','AUBNE', 'USEZX', '{shipmentPKs[25]}'),
(newid(), 'FAS', 'Vessel', 'Flight', '2023-03-27', '2023-04-27','AUBNE', 'AUBNE', '{shipmentPKs[26]}'),
(newid(), 'FAS', 'Vessel', 'Flight', '2023-03-28', '2023-04-28','AUBNE', 'AUBNE', '{shipmentPKs[27]}'),
(newid(), 'FAS', 'Vessel', 'Flight', '2023-03-29', '2023-04-29','AUBNE', 'AUBNE', '{shipmentPKs[28]}'),
(newid(), 'FAS', 'Vessel', 'Flight', '2023-03-30', '2023-04-30','AUBNE', 'AUBNE', '{shipmentPKs[29]}')

INSERT INTO [dbo].[ComplianceRiskStatus] ([COR_PK],[COR_ParentTableCode],[COR_ParentID],[COR_OverallRisk],[COR_LocationRisk],[COR_PartyRisk],[COR_SystemCreateTimeUtc],[COR_SystemCreateUser],[COR_SystemLastEditTimeUtc],[COR_SystemLastEditUser])
VALUES(newid(),'JS', '{shipmentPKs[0]}','CLR','CLR','CLR',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[1]}','OVR','PSK','PSK',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[2]}','CLR','CLR','CLR',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[3]}','HLD','PSK','PSK',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[4]}','HLD','CLR','CLR',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[5]}','HLD','PSK','PSK',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[6]}','OVR','PSK','PSK',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[7]}','CLR','CLR','CLR',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[8]}','HLD','PSK','PSK',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[9]}','OVR','CLR','PSK',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[10]}','HLD','PSK','PSK',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[11]}','CLR','CLR','CLR',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[12]}','OVR','PSK','PSK',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[13]}','HLD','PSK','PSK',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[28]}','HLD','PSK','PSK',GETDATE(),'E',GETDATE(),'E')

INSERT INTO [dbo].[StmComplianceEvent] ([SCE_PK],[SCE_ParentID],[SCE_ParentTableCode],[SCE_EventType],[SCE_EventSubType],[SCE_EventReference],[SCE_NewValue],[SCE_OldValue],[SCE_Snapshot],[SCE_EventTimeOffset],[SCE_SystemCreateTimeUtc],[SCE_SystemCreateUser],[SCE_SystemLastEditTimeUtc],[SCE_SystemLastEditUser])
VALUES
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'OVL', 'Job Compliance status', 'HLD', 'OVR', '', '2023-02-25 18:00:00 +11:00', '2023-02-25 07:00:00', 'E', '2023-02-25 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'LOC', 'Location Compliance Risk status', 'CLR', 'PSK', '', '2023-02-25 18:00:00 +11:00', '2023-02-25 07:00:00', 'E', '2023-02-25 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'PTY', 'Party Compliance Risk status', 'PSK', 'CLR', '', '2023-02-25 18:00:00 +11:00', '2023-02-25 07:00:00', 'E', '2023-02-25 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'CMD', 'Commodities Compliance Risk status', 'HSK', 'INC', '', '2023-02-25 18:00:00 +11:00', '2023-02-25 07:00:00', 'E', '2023-02-25 07:00:00', 'E'),

	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'OVL', 'Job Compliance status', 'CLR', 'HLD', '', '2023-03-02 18:00:00 +11:00', '2023-03-02 07:00:00', 'E', '2023-03-02 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'LOC', 'Location Compliance Risk status', 'PSK', 'CLR', '', '2023-03-02 18:00:00 +11:00', '2023-03-02 07:00:00', 'E', '2023-03-02 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'PTY', 'Party Compliance Risk status', 'CLR', 'PSK', '', '2023-03-02 18:00:00 +11:00', '2023-03-02 07:00:00', 'E', '2023-03-02 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'CMD', 'Commodities Compliance Risk status', 'CLR', 'HSK', '', '2023-03-02 18:00:00 +11:00', '2023-03-02 07:00:00', 'E', '2023-03-02 07:00:00', 'E'),

	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'OVL', 'Job Compliance status', 'HLD', 'OVR', '', '2023-02-28 18:00:00 +11:00', '2023-02-28 07:00:00', 'E', '2023-02-28 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'LOC', 'Location Compliance Risk status', 'CLR', 'PSK', '', '2023-02-28 18:00:00 +11:00', '2023-02-28 07:00:00', 'E', '2023-02-28 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'PTY', 'Party Compliance Risk status', 'PSK', 'CLR', '', '2023-02-28 18:00:00 +11:00', '2023-02-28 07:00:00', 'E', '2023-02-28 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'CMD', 'Commodities Compliance Risk status', 'HSK', 'CLR', '', '2023-02-28 18:00:00 +11:00', '2023-02-28 07:00:00', 'E', '2023-02-28 07:00:00', 'E'),

	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'OVL', 'Job Compliance status', 'CLR', 'HLD', '', '2023-03-20 18:00:00 +11:00', '2023-03-20 07:00:00', 'E', '2023-03-20 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'LOC', 'Location Compliance Risk status', 'PSK', 'CLR', '', '2023-03-20 18:00:00 +11:00', '2023-03-20 07:00:00', 'E', '2023-03-20 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'PTY', 'Party Compliance Risk status', 'CLR', 'PSK', '', '2023-03-20 18:00:00 +11:00', '2023-03-20 07:00:00', 'E', '2023-03-20 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'CMD', 'Commodities Compliance Risk status', 'CLR', 'HSK', '', '2023-03-20 18:00:00 +11:00', '2023-03-20 07:00:00', 'E', '2023-03-20 07:00:00', 'E'),

	(NEWID(), '{shipmentPKs[0]}', 'JS', 'CRI', 'CAI', 'Compliance Assessment Initialized', '', '', '', '2023-03-20 18:00:00 +11:00', '2023-03-20 07:00:00', 'E', '2023-03-20 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[1]}', 'JS', 'CRI', 'CAI', 'Compliance Assessment Initialized', '', '', '', '2023-03-20 18:00:00 +11:00', '2023-03-20 07:00:00', 'E', '2023-03-20 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[4]}', 'JS', 'CRI', 'CAD', 'Compliance Assessment Declined', '', '', '', '2023-03-20 18:00:00 +11:00', '2023-03-20 07:00:00', 'E', '2023-03-20 07:00:00', 'E'),

	(NEWID(), '{shipmentPKs[5]}', 'JS', 'CRI', 'CAD', 'Compliance Assessment Declined', '', '', '', '2023-03-03 18:00:00 +11:00', '2023-03-03 07:00:00', 'E', '2023-03-03 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[5]}', 'JS', 'CRI', 'CAI', 'Compliance Assessment Initialized', '', '', '', '2023-03-04 18:00:00 +11:00', '2023-03-04 07:00:00', 'E', '2023-03-04 07:00:00', 'E'),

	(NEWID(), '{shipmentPKs[6]}', 'JS', 'CRI', 'CAI', 'Compliance Assessment Initialized', '', '', '', '2023-03-20 18:00:00 +11:00', '2023-03-20 07:00:00', 'E', '2023-03-20 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[7]}', 'JS', 'CRI', 'CAI', 'Compliance Assessment Initialized', '', '', '', '2023-03-20 18:00:00 +11:00', '2023-03-20 07:00:00', 'E', '2023-03-20 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[8]}', 'JS', 'CRI', 'CAI', 'Compliance Assessment Initialized', '', '', '', '2023-03-20 18:00:00 +11:00', '2023-03-20 07:00:00', 'E', '2023-03-20 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[9]}', 'JS', 'CRI', 'CAI', 'Compliance Assessment Initialized', '', '', '', '2023-03-20 18:00:00 +11:00', '2023-03-20 07:00:00', 'E', '2023-03-20 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[10]}', 'JS', 'CRI', 'CAI', 'Compliance Assessment Initialized', '', '', '', '2023-03-20 18:00:00 +11:00', '2023-03-20 07:00:00', 'E', '2023-03-20 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[11]}', 'JS', 'CRI', 'CAI', 'Compliance Assessment Initialized', '', '', '', '2023-03-20 18:00:00 +11:00', '2023-03-20 07:00:00', 'E', '2023-03-20 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[12]}', 'JS', 'CRI', 'CAI', 'Compliance Assessment Initialized', '', '', '', '2023-03-20 18:00:00 +11:00', '2023-03-20 07:00:00', 'E', '2023-03-20 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[13]}', 'JS', 'CRI', 'CAI', 'Compliance Assessment Initialized', '', '', '', '2023-03-20 18:00:00 +11:00', '2023-03-20 07:00:00', 'E', '2023-03-20 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[28]}', 'JS', 'CRI', 'CAI', 'Compliance Assessment Initialized', '', '', '', '2023-03-20 18:00:00 +11:00', '2023-03-20 07:00:00', 'E', '2023-03-20 07:00:00', 'E')

INSERT INTO dbo.JobDocAddress
(E2_PK, E2_OA_Address, E2_AddressOverride, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_AdditionalAddressInformation, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser)
VALUES(NEWID(), '{orgAddress1PK}', 0, '{shipmentPKs[0]}', 'JS', 'CRD', 'Inner Sanctum', '2020-12-17 15:31', 'E'),
	(NEWID(), '{orgAddress2PK}', 0, '{shipmentPKs[0]}', 'JS', 'CED', 'Inner Sanctum', '2020-12-17 15:31', 'E')
";
			}
		}
	}
}
