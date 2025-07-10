using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USInBonds))]
	class USInBondsTest : DbCreateScriptTest
	{
		public void TestPortDescriptionUsingGlobalData()
		{
			var portSQL = @"INSERT INTO dbo.RefDbEntUS_USCForeignPort(UH_PK, UH_Code, UH_Name, UH_ValidForType) VALUES(NEWID(), '0000', 'TEST PORT 0000', '');";
			using (var command = Db.Connection.Command(portSQL))
			{
				command.ExecuteNonQuery();
			}
			TestDataCreator.CreateRefDbDataGroupingCodeTypeAndListItems("US", "PORT", ("9999", "TEST PORT 9999"));

			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var importerAddress = TestDataCreator.CreateAddress(importerPK, "Addr01", "TestAddress01");
			var inBondCarrierPK = TestDataCreator.CreateOrganisation("OrgCode2", "Company Name 2");
			var inBondCarrierAddress = TestDataCreator.CreateAddress(inBondCarrierPK, "Addr02", "TestAddress02");
			var supplierPK = TestDataCreator.CreateOrganisation("OrgCode3", "Company Name 3");
			var bhPK = CreateCusInbondHeader("INB00001", branchPK, importerAddress, supplierPK, "20", "2704", DateTime.Today, "INB", importLoadPortKCode: "0000");
			var bmPK = CreateCusInBondMoveHeader(bhPK, inBondCarrierAddress, "63", "AMS", "", foreignDestPortKCode: "9999");

			var reportSql = @"select JobNumber, LoadingPortName, ForeignDestinationName
								from USInBonds(@companyPK, @importerPK, NULL, '', @carrierOrg, '', '', '', @branchPK,
								'', '', '', '', '', '', '', '', '',
								'', '', '') ORDER BY JobNumber";
			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<string, string, string>>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				command.AddParameter("@carrierOrg", SqlDbType.UniqueIdentifier, inBondCarrierPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var jobNumber = reader["JobNumber"] as string;
						var loadingPortName = reader["LoadingPortName"] as string;
						var foreignDestinationName = reader["ForeignDestinationName"] as string;
						reportList.Add(new Tuple<string, string, string>(jobNumber, loadingPortName, foreignDestinationName));
					}

					AssertEquals(1, reportList.Count);
					var result1 = reportList.FirstOrDefault();
					AssertEquals("INB00001", result1.Item1);
					AssertEquals(null, result1.Item2);
					AssertEquals("TEST PORT 9999", result1.Item3);
				}
			}
		}

		public void TestUSInBondsOnReport()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var importerAddress = TestDataCreator.CreateAddress(importerPK, "Addr01", "TestAddress01");
			var inBondCarrierPK = TestDataCreator.CreateOrganisation("OrgCode2", "Company Name 2");
			var inBondCarrierAddress = TestDataCreator.CreateAddress(inBondCarrierPK, "Addr02", "TestAddress02");
			var supplierPK = TestDataCreator.CreateOrganisation("OrgCode3", "Company Name 3");
			var bhPK = CreateCusInbondHeader("INB00001", branchPK, importerAddress, supplierPK, "20", "2704", DateTime.Today, "INB");
			var bmPK = CreateCusInBondMoveHeader(bhPK, inBondCarrierAddress, "63", "AMS", "");
			var cusEntryNumPk = TestDataCreator.CreateCusEntryNum(bmPK, "CusInBondMoveHeader", "100001", "INB", "CUS", "US");
			var cusInBondBillPK = CreateCusInbondBill(100, "PCK", 5000m, "KG", 0, "", bhPK);
			var cusInBondMoveDatailPK = CreateCusInBondMoveDetail(bmPK, cusInBondBillPK, 108, 108.00m);

			var reportSql = @"select JobNumber, InBondNumber, InBondType, ImporterName, InBondCarrierName, ImportPortOfArrival, UnladingPort, ImportETA
								from USInBonds(@companyPK, @importerPK, NULL, '', @carrierOrg, '', '', '', @branchPK,
								'', '', '', '', '', '', '', '', '',
								'', '', '') ORDER BY JobNumber";
			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<Tuple<string, string, string, string, string, string, DateTime>>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				command.AddParameter("@carrierOrg", SqlDbType.UniqueIdentifier, inBondCarrierPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var jobNumber = (string)reader["JobNumber"];
						var inBondNumber = (string)reader["InBondNumber"];
						var inBondType = (string)reader["InBondType"];
						var importerName = (string)reader["ImporterName"];
						var carrierName = (string)reader["InBondCarrierName"];
						var arrivalPort = (string)reader["ImportPortOfArrival"];
						var importETA = (DateTime)reader["ImportETA"];
						reportList.Add(new Tuple<string, string, string, string, string, string, DateTime>(jobNumber, inBondNumber, inBondType, importerName, carrierName, arrivalPort, importETA));
					}

					AssertEquals(1, reportList.Count);
					var result1 = reportList.FirstOrDefault();
					AssertEquals("INB00001", result1.Item1);
					AssertEquals("100001", result1.Item2);
					AssertEquals("63", result1.Item3);
					AssertEquals("Company Name 1", result1.Item4);
					AssertEquals("Company Name 2", result1.Item5);
					AssertEquals("2704", result1.Item6);
					AssertEquals(DateTime.Today, result1.Item7);
				}
			}
		}

		public void TestQPAndWPMessageStatusFilter()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var bhPK = CreateCusInbondHeader("INB00001", branchPK, null, null, "20", "2704", DateTime.Today, "INB");
			var bmPK = CreateCusInBondMoveHeader(bhPK, null, "63", "AMS");

			var sql = $"select JobNumber from USInBonds('{companyPK}', NULL, NULL, '', NULL, '', '', '', '{branchPK}', '', '', '', '', '', '', '', '', '', '', '', '')";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Filter without QP/WP, Data without QP/WP", 1, result.Rows.Count);

			TestConnection.ExecuteNonQuery($"UPDATE dbo.CusInBondMoveHeader SET BM_CustomsStatus = 'ADA', BM_MessageStatus = 'AAV', BM_SystemLastEditTimeUtc = GETUTCDATE(), BM_SystemLastEditUser = 'E' WHERE BM_PK = '{bmPK}'");
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Filter without QP/WP, Data with QP/WP", 1, result.Rows.Count);

			sql = $"select JobNumber from USInBonds('{companyPK}', NULL, NULL, '', NULL, '', '', '', '{branchPK}', '', '', '', '', '', '', '', '', '', '', 'ADA', '')";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Filter with QP without WP, Data with QP/WP", 1, result.Rows.Count);

			sql = $"select JobNumber from USInBonds('{companyPK}', NULL, NULL, '', NULL, '', '', '', '{branchPK}', '', '', '', '', '', '', '', '', '', '', 'ADA', 'AAV')";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Filter with QP/WP, Data with QP/WP", 1, result.Rows.Count);

			sql = $"select JobNumber from USInBonds('{companyPK}', NULL, NULL, '', NULL, '', '', '', '{branchPK}', '', '', '', '', '', '', '', '', '', '', 'ADO', 'AAV')";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Filter with QP/WP but not match, Data with QP/WP", 0, result.Rows.Count);
		}

		public void TestUSInBondsOnReport_AuthMoveDate()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var importerAddress = TestDataCreator.CreateAddress(importerPK, "Addr01", "TestAddress01");
			var inBondCarrierPK = TestDataCreator.CreateOrganisation("OrgCode2", "Company Name 2");
			var inBondCarrierAddress = TestDataCreator.CreateAddress(inBondCarrierPK, "Addr02", "TestAddress02");
			var supplierPK = TestDataCreator.CreateOrganisation("OrgCode3", "Company Name 3");

			var bhPKForRail = CreateCusInbondHeader("INB00001", branchPK, importerAddress, supplierPK, "20", "2704", DateTime.Today, "INB");
			var bmPK = CreateCusInBondMoveHeader(bhPKForRail, inBondCarrierAddress, "63", "AMS", "");
			var cusEntryNumPK = TestDataCreator.CreateCusEntryNum(bmPK, "CusInBondMoveHeader", "100001", "INB", "CUS", "US");
			var cusInBondBillPK = CreateCusInbondBill(100, "PCK", 5000m, "KG", 0, "", bhPKForRail);
			var cusInBondMoveDatailPK = CreateCusInBondMoveDetail(bmPK, cusInBondBillPK, 108, 108.00m);

			TestDataCreator.CreateCusAddInfo("UDP", "Code=1J*DispositionDate=2022-07-15 13:45:00.000*Order=1", "B9", cusInBondMoveDatailPK);
			TestDataCreator.CreateCusAddInfo("UDP", "Code=95*DispositionDate=2022-07-15 13:46:00.000*Order=2", "B9", cusInBondMoveDatailPK);
			TestDataCreator.CreateCusAddInfo("UDP", "Code=1J*DispositionDate=2022-07-15 13:51:00.000*Order=3", "B9", cusInBondMoveDatailPK);

			var bhPKForAir = CreateCusInbondHeader("INB00002", branchPK, importerAddress, supplierPK, "40", "2704", DateTime.Today, "INB");
			var bmPK2 = CreateCusInBondMoveHeader(bhPKForAir, inBondCarrierAddress, "63", "AMS", "");
			var cusEntryNumPK2 = TestDataCreator.CreateCusEntryNum(bmPK2, "CusInBondMoveHeader", "100001", "INB", "CUS", "US");
			var cusInBondBillPK2 = CreateCusInbondBill(100, "PCK", 5000m, "KG", 0, "", bhPKForAir);
			var cusInBondMoveDatailPK2 = CreateCusInBondMoveDetail(bmPK2, cusInBondBillPK2, 108, 108.00m);

			TestDataCreator.CreateCusAddInfo("UDP", "Code=1D*DispositionDate=2022-07-15 13:45:00.000*Order=1", "B9", cusInBondMoveDatailPK2);
			TestDataCreator.CreateCusAddInfo("UDP", "Code=95*DispositionDate=2022-07-15 13:46:00.000*Order=2", "B9", cusInBondMoveDatailPK2);
			TestDataCreator.CreateCusAddInfo("UDP", "Code=1D*DispositionDate=2022-07-15 13:51:00.000*Order=3", "B9", cusInBondMoveDatailPK2);
			TestDataCreator.CreateCusAddInfo("UDP", "Code=1J*DispositionDate=2022-07-15 13:52:00.000*Order=4", "B9", cusInBondMoveDatailPK2);

			var reportSql = @"select JobNumber, AuthorisedToMoveDate
								from USInBonds(@companyPK, @importerPK, NULL, '', @carrierOrg, '', '', '', @branchPK,
								'', '', '', '', '', '', '', '', '',
								'', '', '') ORDER BY JobNumber";

			using (var command = Db.Connection.Command(reportSql))
			{
				var reportList = new List<(string, DateTime?)>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				command.AddParameter("@carrierOrg", SqlDbType.UniqueIdentifier, inBondCarrierPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var jobNumber = (string)reader["JobNumber"];
						DateTime? authorisedToMoveDate = null;
						if (reader["AuthorisedToMoveDate"] != DBNull.Value)
						{
							authorisedToMoveDate = (DateTime)reader["AuthorisedToMoveDate"];
						}
						reportList.Add((jobNumber, authorisedToMoveDate));
					}

					AssertEquals(2, reportList.Count);
					AssertEquals("INB00001", reportList[0].Item1);
					AssertEquals(new DateTime(2022, 07, 15, 13, 51, 00), reportList[0].Item2);
					AssertEquals("INB00002", reportList[1].Item1);
					AssertEquals(new DateTime(2022, 07, 15, 13, 51, 00), reportList[1].Item2);
				}
			}
		}

		Guid CreateCusInbondHeader(string jobNumber, Guid branchPK, Guid? importerAddressPK, Guid? supplierPK, string importTransportMode, string arrivalPort, DateTime etaTime, string applicationCode = "", string importLoadPortKCode = "")
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusInbondHeader
(BH_PK, BH_JobReference, BH_GB, BH_SystemCreateTimeUtc, BH_SystemCreateUser, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser, BH_OA_Importer, BH_OH_Supplier, BH_ImportTransportMode, BH_PortUnladingDCode, BH_ETA, BH_ApplicationCode, BH_ImportLoadPortKCode, BH_IsActive)
VALUES
(@BH_PK, @BH_JobReference, @BH_GB, GetUtcDate(), '~BP', GetUtcDate(), '~BP', @BH_OA_Importer, @BH_OH_Supplier, @BH_ImportTransportMode, @BH_PortUnladingDCode, @BH_ETA, @BH_ApplicationCode, @BH_ImportLoadPortKCode ,1)
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@BH_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@BH_JobReference", SqlDbType.VarChar, jobNumber);
				command.AddParameter("@BH_GB", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@BH_OA_Importer", SqlDbType.UniqueIdentifier, importerAddressPK.HasValue ? importerAddressPK : DBNull.Value);
				command.AddParameter("@BH_OH_Supplier", SqlDbType.UniqueIdentifier, supplierPK.HasValue ? supplierPK : DBNull.Value);
				command.AddParameter("@BH_ImportTransportMode", SqlDbType.VarChar, importTransportMode);
				command.AddParameter("@BH_PortUnladingDCode", SqlDbType.VarChar, arrivalPort);
				command.AddParameter("@BH_ETA", SqlDbType.SmallDateTime, etaTime);
				command.AddParameter("@BH_ApplicationCode", SqlDbType.VarChar, applicationCode);
				command.AddParameter("@BH_ImportLoadPortKCode", SqlDbType.VarChar, importLoadPortKCode);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		Guid CreateCusInbondBill(int manifestQty, string manifestUQ, decimal weight, string weightUQ, decimal volume, string volumeUQ, Guid bondHeaderPK)
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusInbondBill
(B0_PK, B0_ManifestQty, B0_ManifestUQ, B0_Weight, B0_WeightUQ, B0_Volume, B0_VolumeUQ, B0_SystemCreateTimeUtc, B0_SystemCreateUser, B0_SystemLastEditTimeUtc, B0_SystemLastEditUser, B0_BH, B0_IsValid)
VALUES
(@BH_PK, @B0_ManifestQty, @B0_ManifestUQ, @B0_Weight, @B0_WeightUQ, @B0_Volume, @B0_VolumeUQ, GetUtcDate(), '~BP', GetUtcDate(), '~BP', @B0_BH, 1)
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@BH_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@B0_ManifestQty", SqlDbType.Int, manifestQty);
				command.AddParameter("@B0_ManifestUQ", SqlDbType.VarChar, manifestUQ);
				command.AddParameter("@B0_Weight", SqlDbType.Decimal, weight);
				command.AddParameter("@B0_WeightUQ", SqlDbType.VarChar, weightUQ);
				command.AddParameter("@B0_Volume", SqlDbType.Decimal, volume);
				command.AddParameter("@B0_VolumeUQ", SqlDbType.VarChar, volumeUQ);
				command.AddParameter("@B0_BH", SqlDbType.UniqueIdentifier, bondHeaderPK);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		Guid CreateCusInBondMoveDetail(Guid bmPK, Guid b0PK, int inBoundQty, decimal monetaryValue)
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusInBondMoveDetail
(B9_PK, B9_BM, B9_B0, B9_IsValid, B9_InBoundQty, B9_MonetaryValue, B9_SystemCreateTimeUtc, B9_SystemCreateUser, B9_SystemLastEditTimeUtc, B9_SystemLastEditUser)
VALUES
(@B9_PK, @B9_BM, @B9_B0, 1, @B9_InBoundQty, @B9_MonetaryValue, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@B9_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@B9_BM", SqlDbType.UniqueIdentifier, bmPK);
				command.AddParameter("@B9_B0", SqlDbType.UniqueIdentifier, b0PK);
				command.AddParameter("@B9_InBoundQty", SqlDbType.Int, inBoundQty);
				command.AddParameter("@B9_MonetaryValue", SqlDbType.Decimal, monetaryValue);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		Guid CreateCusInBondMoveHeader(Guid bondHeaderPK, Guid? bondCarrier, string bondType, string subApplicationCode = "", string customsStatus = "", string foreignDestPortKCode = "", string messageStatus = "")
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusInbondMoveHeader
(BM_PK, BM_BH, BM_OA_InBondCarrier, BM_InBondEntryType, BM_SubApplicationCode, BM_CustomsStatus, BM_MessageStatus, BM_ForeignDestPortKCode, BM_SystemCreateTimeUtc, BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser)
VALUES
(@BM_PK, @BM_BH, @BM_OA_InBondCarrier, @BM_InBondEntryType, @BM_SubApplicationCode, @BM_CustomsStatus, @BM_MessageStatus, @BM_ForeignDestPortKCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@BM_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@BM_BH", SqlDbType.UniqueIdentifier, bondHeaderPK);
				command.AddParameter("@BM_OA_InBondCarrier", SqlDbType.UniqueIdentifier, bondCarrier.HasValue ? bondCarrier : DBNull.Value);
				command.AddParameter("@BM_InBondEntryType", SqlDbType.VarChar, bondType);
				command.AddParameter("@BM_SubApplicationCode", SqlDbType.VarChar, subApplicationCode);
				command.AddParameter("@BM_CustomsStatus", SqlDbType.VarChar, customsStatus);
				command.AddParameter("@BM_MessageStatus", SqlDbType.VarChar, messageStatus);
				command.AddParameter("@BM_ForeignDestPortKCode", SqlDbType.VarChar, foreignDestPortKCode);
				command.ExecuteNonQuery();
			}
			return pk;
		}
	}
}
