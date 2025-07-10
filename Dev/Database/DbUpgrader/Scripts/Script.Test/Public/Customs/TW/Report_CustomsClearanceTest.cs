using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.TW;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.TW
{
	[TestedType(typeof(Report_TWCustomsClearance))]
	class Report_TWCustomsClearanceTest : DbCreateScriptTest
	{
		public void TestColumnsFromCusEntryInstruction()
		{
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, "IMP", "SEA", "ABC", "ABC", 1);
			var cusEntryInstructionPK = CreateCusEntryInstruction(declarationPK, new DateTime(2019, 12, 12), 1);
			CreateCusEntryHeader(declarationPK, "C1", 1);
			Db.Connection.Command($@"
UPDATE
	dbo.CusEntryInstruction
SET
	CEI_Style = 'F1',
	CEI_AddInfo = 'CustomsOffice=AB*ExamMode=8*GoodsLocation=AL030',
	CEI_SystemLastEditTimeUtc = GETUTCDATE(),
	CEI_SystemLastEditUser = '~BP'
WHERE CEI_PK = '{cusEntryInstructionPK}'").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK, DeclarationDate, DeclarationType, OfficeofReceipt, ReceiptLocation, ExamMode FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("DeclarationDate", new DateTime(2019, 12, 12), reader.GetDateTime(1));
					AssertEquals("DeclarationType", "F1", reader.GetString(2));
					AssertEquals("OfficeofReceipt", "AB", reader.GetString(3));
					AssertEquals("ReceiptLocation", "AL030", reader.GetString(4));
					AssertEquals("ExamMode", "8", reader.GetString(5));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestColumnsFromJobDeclaration()
		{
			var createUserPK = Guid.NewGuid();
			Db.Connection.Command($@"
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES('{createUserPK}', 'ABC', 'ABC', GETUTCDATE(), 'E', GETUTCDATE(), 'E')").ExecuteNonQuery();

			var declarationPK = CreateJobDeclaration(branchPK, companyPK, "IMP", "SEA", "ABC", "ABC", 1);
			CreateCusEntryInstruction(declarationPK, new DateTime(2019, 12, 12), 1);
			CreateCusEntryHeader(declarationPK, "C1", 1);

			Db.Connection.Command($@"
UPDATE
	dbo.JobDeclaration
SET
	JE_DeclarationReference = 'B00001001',
	JE_MasterBill = 'MB12345',
	JE_HouseBill = 'HB12345',
	JE_VoyageFlightNo = 'QF123',
	JE_VesselName = 'ADMIRALENGRACHT',
	JE_RL_NKOrigin = 'NZAKL',
	JE_RL_NKFinalDestination = 'AUSYD',
	JE_TotalNoOfPacks = 110,
	JE_TotalNoOfPacksPackType = 'PLT',
	JE_TotalWeight = 220,
	JE_TotalWeightUnit = 'KG',
	JE_GoodsDescription = 'normal goods',
	JE_PaymentMethod = 'BRK',
	JE_OwnerRef = 'OWNERS REFERENCE',
	JE_AddInfo = 'SLD=5556',
	JE_DateAtFinalDestination = DATEFROMPARTS(2021, 1, 1),
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = 'ABC'
WHERE
	JE_PK = '{declarationPK}'").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK, JobNumber, MasterBill, HouseBill, FlightVoyage, VesselName, PortOfOrigin, FinalDestination, TotalWeight, GoodsDescription, PaymentMethod, CreateUser, LastEditUser, OwnersReference, SONoManifest, ImportDate, TotalPackages, JobBranch, CreateUserPK FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("JobNumber", "B00001001", reader.GetString(1));
					AssertEquals("MasterBill", "MB12345", reader.GetString(2));
					AssertEquals("HouseBill", "HB12345", reader.GetString(3));
					AssertEquals("FlightVoyage", "QF123", reader.GetString(4));
					AssertEquals("VesselName", "ADMIRALENGRACHT", reader.GetString(5));
					AssertEquals("PortOfOrigin", "NZAKL", reader.GetString(6));
					AssertEquals("FinalDestination", "AUSYD", reader.GetString(7));
					AssertEquals("TotalWeight", "220.000 KG", reader.GetString(8));
					AssertEquals("GoodsDescription", "normal goods", reader.GetString(9));
					AssertEquals("PaymentMethod", "BRK", reader.GetString(10));
					AssertEquals("CreateUser", "ABC", reader.GetString(11));
					AssertEquals("LastEditUser", "ABC", reader.GetString(12));
					AssertEquals("OwnersReference", "OWNERS REFERENCE", reader.GetString(13));
					AssertEquals("SONoManifest", "5556", reader.GetString(14));
					AssertEquals("ImportDate", new DateTime(2021, 1, 1), reader.GetDateTime(15));
					AssertEquals("TotalPackages", "110 PLT", reader.GetString(16));
					AssertEquals("JobBranch", branchPK, reader.GetGuid(17));
					AssertEquals("CreateUserPK", createUserPK, reader.GetGuid(18));
					Assert("There should be no other records", !reader.Read());
				}
			}

			Db.Connection.Command($@"
UPDATE
	dbo.JobDeclaration
SET
	JE_MasterBill = '08331423814',
	JE_TransportMode = 'AIR',
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = 'ABC'
WHERE
	JE_PK = '{declarationPK}'").ExecuteNonQuery();
			reportSql = @"SELECT JE_PK, MasterBill FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("MasterBill", "083-31423814", reader.GetString(1));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestColumnsFromCusEntryHeader()
		{
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, "IMP", "SEA", "ABC", "ABC", 1);
			CreateCusEntryInstruction(declarationPK, new DateTime(2019, 12, 12), 1);
			var cusEntryHeaderPK = CreateCusEntryHeader(declarationPK, "C1", 1);
			Db.Connection.Command($@"
UPDATE
	dbo.CusEntryHeader
SET
	CH_EntryReleaseDate = DATEFROMPARTS(2021, 1, 1),
	CH_SystemLastEditTimeUtc = GETUTCDATE(),
	CH_SystemLastEditUser = '~BP'
WHERE CH_PK = '{cusEntryHeaderPK}'").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK, EntryReleaseDate FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("EntryReleaseDate", new DateTime(2021, 1, 1), reader.GetDateTime(1));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestDutyDueDate()
		{
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, "IMP", "SEA", "ABC", "ABC", 1);
			CreateCusEntryInstruction(declarationPK, new DateTime(2019, 12, 12), 1);
			var cusEntryHeaderPK = CreateCusEntryHeader(declarationPK, "C1", 1);
			TestDataCreator.CreateCusEntryPayInfo("79", 99.2m, "", new DateTime(2021, 1, 1), "", "G", "", cusEntryHeaderPK, DateTime.Today, 2);

			var reportSql = @"SELECT JE_PK, DutyDueDate FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("DutyDueDate", new DateTime(2021, 1, 1), reader.GetDateTime(1));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestColumnsFromCusEntryNum()
		{
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, "IMP", "SEA", "ABC", "ABC", 1);
			CreateCusEntryInstruction(declarationPK, new DateTime(2019, 12, 12), 1);
			CreateCusEntryHeader(declarationPK, "C1", 1);
			var cusEntryNumPK = TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", "ABAM10123BBBB2", "IMP", "CUS", "TW");

			Db.Connection.Command($@"
UPDATE
	dbo.CusEntryNum
SET
	CE_EntryStatus = 'C1',
	CE_SystemLastEditTimeUtc = GETUTCDATE(),
	CE_SystemLastEditUser = '~BP'
WHERE CE_PK = '{cusEntryNumPK}'").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK, EntryNumber, ClearanceStatus FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("EntryNumber", "AB/AM/10/123/BBBB2", reader.GetString(1));
					AssertEquals("ClearanceStatus", "C1", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}

			Db.Connection.Command($@"
UPDATE
	dbo.CusEntryNum
SET
	CE_EntryNum = '',
	CE_SystemLastEditTimeUtc = GETUTCDATE(),
	CE_SystemLastEditUser = '~BP'
WHERE CE_PK = '{cusEntryNumPK}'").ExecuteNonQuery();

			reportSql = @"SELECT JE_PK, EntryNumber, ClearanceStatus FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("EntryNumber", "", reader.GetString(1));
					AssertEquals("ClearanceStatus", "C1", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestColumnsFromCusEntryNum_CusEntryHeader()
		{
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, "EXP", "AIR", "ABC", "ABC", 1);
			CreateCusEntryInstruction(declarationPK, new DateTime(2019, 12, 12), 1);
			var cusEntryHeaderPK = CreateCusEntryHeader(declarationPK, "C1", 1);
			var cusEntryNumPK = TestDataCreator.CreateCusEntryNum(cusEntryHeaderPK, "CusEntryHeader", "ABAM10123BBBB2", "EXP", "CUS", "TW");

			Db.Connection.Command($@"
UPDATE
	dbo.CusEntryNum
SET
	CE_EntryStatus = 'C1',
	CE_SystemLastEditTimeUtc = GETUTCDATE(),
	CE_SystemLastEditUser = '~BP'
WHERE CE_PK = '{cusEntryNumPK}'").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK, EntryNumber, ClearanceStatus FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("EntryNumber", "AB/AM/10/123/BBBB2", reader.GetString(1));
					AssertEquals("ClearanceStatus", "C1", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}

			Db.Connection.Command($@"
UPDATE
	dbo.CusEntryNum
SET
	CE_EntryNum = '',
	CE_SystemLastEditTimeUtc = GETUTCDATE(),
	CE_SystemLastEditUser = '~BP'
WHERE CE_PK = '{cusEntryNumPK}'").ExecuteNonQuery();

			reportSql = @"SELECT JE_PK, EntryNumber, ClearanceStatus FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("EntryNumber", "", reader.GetString(1));
					AssertEquals("ClearanceStatus", "C1", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestSupplierName()
		{
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, "IMP", "SEA", "ABC", "ABC", 1);
			CreateCusEntryInstruction(declarationPK, new DateTime(2019, 12, 12), 1);
			CreateCusEntryHeader(declarationPK, "C1", 1);
			TestDataCreator.CreateDocAddress(Guid.Empty, "Org Override SUD", declarationPK, "JE", "SUD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true);

			var reportSql = @"SELECT JE_PK, SupplierName FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("SupplierName", "Org Override SUD", reader.GetString(1));
					Assert("There should be no other records", !reader.Read());
				}
			}

			TestDataCreator.CreateDocAddress(Guid.Empty, "Org Override STA", declarationPK, "JE", "STA", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true, "TW");
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("SupplierName", "Org Override STA", reader.GetString(1));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestImporterName()
		{
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, "EXP", "SEA", "ABC", "ABC", 1);
			CreateCusEntryInstruction(declarationPK, new DateTime(2019, 12, 12), 1);
			CreateCusEntryHeader(declarationPK, "C1", 1);
			TestDataCreator.CreateDocAddress(Guid.Empty, "Org Override IMD", declarationPK, "JE", "IMD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true);

			var reportSql = @"SELECT JE_PK, ImporterName FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("ImporterName", "Org Override IMD", reader.GetString(1));
					Assert("There should be no other records", !reader.Read());
				}
			}

			TestDataCreator.CreateDocAddress(Guid.Empty, "Org Override ITA", declarationPK, "JE", "ITA", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true, "TW");
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("ImporterName", "Org Override ITA", reader.GetString(1));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestColumnsFromJobService()
		{
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, "IMP", "SEA", "ABC", "ABC", 1);
			CreateCusEntryInstruction(declarationPK, new DateTime(2019, 12, 12), 1);
			CreateCusEntryHeader(declarationPK, "C1", 1);
			TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", "ABAM10123BBBB2", "IMP", "CUS", "TW");

			var jobDocsAndCartagePK = TestDataCreator.CreateJobDocsAndCartage(declarationPK, "JE");
			Db.Connection.Command($@"
INSERT INTO dbo.JobService (ES_PK, ES_ServiceCode, ES_ParentID, ES_ParentTableCode, ES_BookedDateTimeOffset, ES_SubLocation, ES_ServiceNote)
VALUES(NEWID(), 'ICI','{jobDocsAndCartagePK}', 'JP', DATEFROMPARTS(2021, 1, 1), 'LOC1', 'ABAM10123BBBB2')
").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK, ExaminationZone, ExaminationDate FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("ExaminationZone", "LOC1", reader.GetString(1));
					AssertEquals("ExaminationDate", new DateTime(2021, 1, 1), reader.GetDateTime(2));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestTransportCompanyEXP()
		{
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, "EXP", "SEA", "ABC", "ABC", 1);
			CreateCusEntryInstruction(declarationPK, new DateTime(2019, 12, 12), 1);
			CreateCusEntryHeader(declarationPK, "C1", 1);
			var jobDocsAndCartagePK = TestDataCreator.CreateJobDocsAndCartage(declarationPK, "JE");

			var organisation1PK = TestDataCreator.CreateOrganisation("ORG_1", "Org One");
			var organisation1MainAddressPK = TestDataCreator.CreateAddress(organisation1PK, "Head Office", "Somewhere");
			Db.Connection.Command($@"
UPDATE dbo.JobDocsAndCartage
SET
	JP_OA_PickupCartageCoAddr = '{organisation1MainAddressPK}',
	JP_SystemLastEditTimeUtc = GETUTCDATE(),
	JP_SystemLastEditUser = '~BP'
WHERE
	JP_PK = '{jobDocsAndCartagePK}'").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK, TransportCompany FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("TransportCompany", "Org One", reader.GetString(1));
					Assert("There should be no other records", !reader.Read());
				}
			}

			Db.Connection.Command($@"UPDATE dbo.OrgAddress SET OA_CompanyNameOverride = 'Org One override' where OA_PK = '{organisation1MainAddressPK}'").ExecuteNonQuery();
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("TransportCompany", "Org One override", reader.GetString(1));
					Assert("There should be no other records", !reader.Read());
				}
			}

			Db.Connection.Command($@"INSERT INTO dbo.OrgTranslatedAddress(OTA_PK, OTA_OA, OTA_Language, OTA_CompanyName, OTA_Address1) VALUES (NEWID(), '{organisation1MainAddressPK}', 'ZH-TW', 'Org One translate', 'address1 translate')").ExecuteNonQuery();
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("TransportCompany", "Org One translate", reader.GetString(1));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestTransportCompanyIMP()
		{
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, "IMP", "SEA", "ABC", "ABC", 1);
			CreateCusEntryInstruction(declarationPK, new DateTime(2019, 12, 12), 1);
			CreateCusEntryHeader(declarationPK, "C1", 1);
			var jobDocsAndCartagePK = TestDataCreator.CreateJobDocsAndCartage(declarationPK, "JE");

			var organisation1PK = TestDataCreator.CreateOrganisation("ORG_1", "Org One");
			var organisation1MainAddressPK = TestDataCreator.CreateAddress(organisation1PK, "Head Office", "Somewhere");
			Db.Connection.Command($@"
UPDATE dbo.JobDocsAndCartage
SET
	JP_OA_DeliveryCartageCoAddr = '{organisation1MainAddressPK}',
	JP_SystemLastEditTimeUtc = GETUTCDATE(),
	JP_SystemLastEditUser = '~BP'
where
	JP_PK = '{jobDocsAndCartagePK}'").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK, TransportCompany FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("TransportCompany", "Org One", reader.GetString(1));
					Assert("There should be no other records", !reader.Read());
				}
			}

			Db.Connection.Command($@"UPDATE dbo.OrgAddress SET OA_CompanyNameOverride = 'Org One override' where OA_PK = '{organisation1MainAddressPK}'").ExecuteNonQuery();
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("TransportCompany", "Org One override", reader.GetString(1));
					Assert("There should be no other records", !reader.Read());
				}
			}

			Db.Connection.Command($@"INSERT INTO dbo.OrgTranslatedAddress(OTA_PK, OTA_OA, OTA_Language, OTA_CompanyName, OTA_Address1) VALUES (NEWID(), '{organisation1MainAddressPK}', 'ZH-TW', 'Org One translate', 'address1 translate')").ExecuteNonQuery();
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("TransportCompany", "Org One translate", reader.GetString(1));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestDivision()
		{
			AssertDivision("000", "000", "000", "000", "CB", "");
			AssertDivision("01000000000", "01000000001", "39000000000", "39000000000", "CB", "分估一課一股");
			AssertDivision("39000000000", "39000000002", "01000000000", "01000000001", "CB", "分估一課二股");
			AssertDivision("72000000000", "72000000003", "39000000000", "39000000000", "CB", "分估一課三股");
			AssertDivision("85010000000", "85010000004", "39000000000", "39000000000", "CB", "分估二課一股");
			AssertDivision("85390000000", "85390000005", "39000000000", "39000000000", "CB", "分估二課二股");
			AssertDivision("90000000000", "90000000006", "39000000000", "39000000000", "CB", "分估二課三股");
			AssertDivision("90000000000", "98990000006", "39000000000", "39000000000", "CB", "分估二課三股");

			AssertDivision("000", "000", "000", "000", "AB", "");
			AssertDivision("01000000000", "01000000001", "39000000000", "39000000001", "AB", "分估一課一股");
			AssertDivision("22000000000", "22000000001", "39000000000", "39000000001", "AB", "分估一課二股");
			AssertDivision("34000000000", "34000000003", "01000000000", "01000000000", "AB", "分估一課三股");
			AssertDivision("44000000000", "63999999999", "39000000000", "39000000001", "AB", "分估二課一股");
			AssertDivision("64000000000", "83999999999", "39000000000", "39000000001", "AB", "分估二課二股");
			AssertDivision("86000000000", "87999999999", "39000000000", "39000000001", "AB", "分估二課三股");
			AssertDivision("84000000000", "84999999999", "39000000000", "39000000001", "AB", "分估三課一股");
			AssertDivision("90000000000", "90999999999", "39000000000", "39000000001", "AB", "分估三課一股");
			AssertDivision("85000000000", "85999999999", "39000000000", "39000000001", "AB", "分估三課二股");
			AssertDivision("88000000000", "89999999999", "39000000000", "39000000001", "AB", "分估三課三股");
			AssertDivision("91000000000", "97999999999", "39000000000", "39000000001", "AB", "分估三課三股");

			AssertDivision("000", "000", "000", "000", "DA", "");
			AssertDivision("01000000000", "26999999999", "39000000000", "39000000001", "DA", "分估一課一股");
			AssertDivision("68000000000", "71999999999", "39000000000", "39000000001", "DA", "分估一課一股");
			AssertDivision("98000000000", "98999999999", "39000000000", "39000000001", "DA", "分估一課一股");
			AssertDivision("27000000000", "67999999999", "01000000000", "01000000001", "DA", "分估一課二股");
			AssertDivision("72000000000", "97999999999", "39000000000", "39000000001", "DA", "分估一課三股");

			AssertDivisionStartWithB("01000000000","72000000000", "BA", "分估一課一股");
			AssertDivisionStartWithB("71999999999", "72000000000", "BA", "分估一課一股");
			AssertDivisionStartWithB("72000000000", "39000000000", "BA", "分估一課二股");
			AssertDivisionStartWithB("97999999999", "39000000001", "BA", "分估一課二股");
			AssertDivisionStartWithB("98999999999", "72000000000", "BA", "");

			AssertDivisionStartWithB("01000000000", "72000000000", "BJ", "分估一課一股");
			AssertDivisionStartWithB("71999999999", "72000000001", "BJ", "分估一課一股");
			AssertDivisionStartWithB("72000000000", "39000000000", "BJ", "分估一課二股");
			AssertDivisionStartWithB("97999999999", "39000000001", "BJ", "分估一課二股");
			AssertDivisionStartWithB("98999999999", "72000000000", "BJ", "");

			AssertDivisionStartWithB("01000000000", "72000000000", "BC", "分估一課一股");
			AssertDivisionStartWithB("06999999999", "72000000000", "BC", "分估一課一股");
			AssertDivisionStartWithB("13000000000", "72000000000", "BC", "分估一課一股");
			AssertDivisionStartWithB("16999999999", "72000000000", "BC", "分估一課一股");
			AssertDivisionStartWithB("07000000000", "72000000000", "BC", "分估一課二股");
			AssertDivisionStartWithB("12999999999", "72000000000", "BC", "分估一課二股");
			AssertDivisionStartWithB("17000000000", "72000000000", "BC", "分估一課二股");
			AssertDivisionStartWithB("24999999999", "72000000000", "BC", "分估一課二股");
			AssertDivisionStartWithB("25000000000", "72000000000", "BC", "分估一課三股");
			AssertDivisionStartWithB("40999999999", "72000000000", "BC", "分估一課三股");
			AssertDivisionStartWithB("41000000000", "72000000000", "BC", "分估一課四股");
			AssertDivisionStartWithB("70999999999", "72000000000", "BC", "分估一課四股");
			AssertDivisionStartWithB("71000000000", "01000000000", "BC", "分估二課一股");
			AssertDivisionStartWithB("83999999999", "01000000000", "BC", "分估二課一股");
			AssertDivisionStartWithB("90000000000", "72000000000", "BC", "分估二課一股");
			AssertDivisionStartWithB("97999999999", "72000000000", "BC", "分估二課一股");
			AssertDivisionStartWithB("84000000000", "72000000000", "BC", "分估二課二股");
			AssertDivisionStartWithB("89999999999", "72000000000", "BC", "分估二課二股");
			AssertDivisionStartWithB("99999999999", "72000000000", "BC", "");

			AssertDivisionStartWithB("01000000000", "72000000000", "BD", "分估一課一股");
			AssertDivisionStartWithB("24999999999", "72000000000", "BD", "分估一課一股");
			AssertDivisionStartWithB("25000000000", "72000000000", "BD", "分估一課二股");
			AssertDivisionStartWithB("40999999999", "72000000000", "BD", "分估一課二股");
			AssertDivisionStartWithB("41000000000", "72000000000", "BD", "分估一課三股");
			AssertDivisionStartWithB("63999999999", "72000000000", "BD", "分估一課三股");
			AssertDivisionStartWithB("64000000000", "72000000000", "BD", "分估一課四股");
			AssertDivisionStartWithB("71999999999", "01000000000", "BD", "分估一課四股");
			AssertDivisionStartWithB("72000000000", "01000000000", "BD", "分估二課一股");
			AssertDivisionStartWithB("90000000000", "72000000000", "BD", "分估二課一股");
			AssertDivisionStartWithB("97999999999", "72000000000", "BD", "分估二課一股");
			AssertDivisionStartWithB("83999999999", "72000000000", "BD", "分估二課一股");
			AssertDivisionStartWithB("84000000000", "72000000000", "BD", "分估二課二股");
			AssertDivisionStartWithB("89999999999", "72000000000", "BD", "分估二課二股");
			AssertDivisionStartWithB("99999999999", "72000000000", "BD", "");

			AssertDivisionStartWithB("01000000000", "72000000000", "BE", "分估一課一股");
			AssertDivisionStartWithB("22999999999", "72000000000", "BE", "分估一課一股");
			AssertDivisionStartWithB("23000000000", "72000000000", "BE", "分估一課二股");
			AssertDivisionStartWithB("46999999999", "72000000000", "BE", "分估一課二股");
			AssertDivisionStartWithB("47000000000", "01000000000", "BE", "分估一課三股");
			AssertDivisionStartWithB("73999999999", "01000000000", "BE", "分估一課三股");
			AssertDivisionStartWithB("74000000000", "72000000000", "BE", "分估一課四股");
			AssertDivisionStartWithB("97999999999", "72000000000", "BE", "分估一課四股");
			AssertDivisionStartWithB("98999999999", "72000000000", "BE", "");

			AssertDivisionStartWithB("01000000000", "84000000000", "BF", "分估一股");
			AssertDivisionStartWithB("83999999999", "84000000000", "BF", "分估一股");
			AssertDivisionStartWithB("84000000000", "72000000000", "BF", "分估二股");
			AssertDivisionStartWithB("98999999999", "72000000000", "BF", "分估二股");
			AssertDivisionStartWithB("99999999999", "72000000000", "BF", "");
		}

		void AssertDivision(string adValoremTariffForExpect1, string adValoremTariffForExpect2, string adValoremTariffForNotExpect1, string adValoremTariffForNotExpect2, string entryNumber, string expectDivision)
		{
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, "IMP", "SEA", "ABC", "ABC", 1);
			var cusEntryInstructionPK = CreateCusEntryInstruction(declarationPK, new DateTime(2019, 12, 12), 1);
			var cusEntryNumPK = TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", entryNumber, "IMP", "CUS", "TW");
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, "C1", 1);
			var entryLinePK11 = CreateCusEntryLine(entryHeaderPK, 1);
			var entryLinePK12 = CreateCusEntryLine(entryHeaderPK, 1);
			var entryLinePK21 = CreateCusEntryLine(entryHeaderPK, 1);
			var entryLinePK22 = CreateCusEntryLine(entryHeaderPK, 1);

			Db.Connection.Command($@"
UPDATE dbo.CusEntryLine 
SET 
    CL_AdValoremTariff = '{adValoremTariffForExpect1}', 
    CL_CustomsValue = 400, 
    CL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CL_SystemLastEditUser = '~BP' 
WHERE 
    CL_PK = '{entryLinePK11}';

UPDATE dbo.CusEntryLine 
SET 
    CL_AdValoremTariff = '{adValoremTariffForExpect2}', 
    CL_CustomsValue = 300, 
    CL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CL_SystemLastEditUser = '~BP' 
WHERE 
    CL_PK = '{entryLinePK12}';

UPDATE dbo.CusEntryLine 
SET 
    CL_AdValoremTariff = '{adValoremTariffForNotExpect1}', 
    CL_CustomsValue = 100, 
    CL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CL_SystemLastEditUser = '~BP' 
WHERE 
    CL_PK = '{entryLinePK21}';

UPDATE dbo.CusEntryLine 
SET 
    CL_AdValoremTariff = '{adValoremTariffForNotExpect2}', 
    CL_CustomsValue = 200, 
    CL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CL_SystemLastEditUser = '~BP' 
WHERE 
    CL_PK = '{entryLinePK22}';
").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK, Division FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("Division", expectDivision, reader.GetString(1));
					Assert("There should be no other records", !reader.Read());
				}
			}

			Db.Connection.Command($@"DELETE FROM dbo.CusEntryLine
DELETE FROM dbo.CusEntryHeader
DELETE FROM dbo.CusEntryInstruction
DELETE FROM dbo.JobDeclaration
").ExecuteNonQuery();
		}

		void AssertDivisionStartWithB(string adValoremTariffForExpect, string adValoremTariffForNotExpect, string entryNumber, string expectDivision)
		{
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, "IMP", "SEA", "ABC", "ABC", 1);
			var cusEntryInstructionPK = CreateCusEntryInstruction(declarationPK, new DateTime(2019, 12, 12), 1);
			var cusEntryNumPK = TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", entryNumber, "IMP", "CUS", "TW");
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, "C1", 1);
			var entryLinePK11 = CreateCusEntryLine(entryHeaderPK, 1);
			var entryLinePK21 = CreateCusEntryLine(entryHeaderPK, 1);

			Db.Connection.Command($@"
UPDATE dbo.CusEntryLine 
SET 
    CL_AdValoremTariff = '{adValoremTariffForExpect}', 
    CL_LineNumber = 1, 
    CL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CL_SystemLastEditUser = '~BP' 
WHERE 
    CL_PK = '{entryLinePK11}';

UPDATE dbo.CusEntryLine 
SET 
    CL_AdValoremTariff = '{adValoremTariffForNotExpect}', 
    CL_LineNumber = 2, 
    CL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CL_SystemLastEditUser = '~BP' 
WHERE 
    CL_PK = '{entryLinePK21}';
").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK, Division FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("Division", expectDivision, reader.GetString(1));
					Assert("There should be no other records", !reader.Read());
				}
			}

			Db.Connection.Command($@"DELETE FROM dbo.CusEntryLine
DELETE FROM dbo.CusEntryHeader
DELETE FROM dbo.CusEntryInstruction
DELETE FROM dbo.JobDeclaration
").ExecuteNonQuery();
		}

		public void TestBusinessTaxBase()
		{
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, "IMP", "SEA", "ABC", "ABC", 1);
			var cusEntryInstructionPK = CreateCusEntryInstruction(declarationPK, new DateTime(2019, 12, 12), 1);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, "C1", 1);
			var invoiceHeaderPK = CreateInvoiceHeader(declarationPK, 1);

			var entryLinePK1 = CreateCusEntryLine(entryHeaderPK, 1);
			var invoiceLinePK1 = CreateInvoiceLine(invoiceHeaderPK, cusEntryInstructionPK, entryLinePK1, 1);
			var entryLinePK2 = CreateCusEntryLine(entryHeaderPK, 1);
			var invoiceLinePK2 = CreateInvoiceLine(invoiceHeaderPK, cusEntryInstructionPK, entryLinePK2, 1);
			var entryLinePK3 = CreateCusEntryLine(entryHeaderPK, 1);
			CreateInvoiceLine(invoiceHeaderPK, cusEntryInstructionPK, entryLinePK3, 1);

			Db.Connection.Command($@"
UPDATE dbo.JobComInvoiceLine 
SET 
    JI_AddInfo = 'VatPymntMthd=CAS',
    JI_SystemLastEditTimeUtc = GetUtcDate(),
    JI_SystemLastEditUser = '~BP'
WHERE 
    JI_PK = '{invoiceLinePK1}';

UPDATE dbo.JobComInvoiceLine 
SET 
    JI_AddInfo = 'VatPymntMthd=CAS', 
    JI_SystemLastEditTimeUtc = GETUTCDATE(), 
    JI_SystemLastEditUser = '~BP' 
WHERE 
    JI_PK = '{invoiceLinePK2}';

UPDATE dbo.CusEntryLine 
SET 
    CL_ValueForVAT = 100.3, 
    CL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CL_SystemLastEditUser = '~BP' 
WHERE 
    CL_PK = '{entryLinePK1}';

UPDATE dbo.CusEntryLine 
SET 
    CL_ValueForVAT = 200.4, 
    CL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CL_SystemLastEditUser = '~BP' 
WHERE 
    CL_PK = '{entryLinePK2}';

UPDATE dbo.CusEntryLine 
SET 
    CL_ValueForVAT = 30, 
    CL_SystemLastEditTimeUtc = GETUTCDATE(), 
    CL_SystemLastEditUser = '~BP' 
WHERE 
    CL_PK = '{entryLinePK3}';
").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK, BusinessTaxBase FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("BusinessTaxBase", new decimal(300), reader.GetDecimal(1));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestTotalTaxAmountCash()
		{
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, "IMP", "SEA", "ABC", "ABC", 1);
			CreateCusEntryInstruction(declarationPK, new DateTime(2019, 12, 12), 1);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, "C1", 1);
			var entryLinePK1 = CreateCusEntryLine(entryHeaderPK, 1);
			var feePK11 = TestDataCreator.CreateCusEntryLineFee(entryLinePK1, "A99", 100f, 1);
			var feePK12 = TestDataCreator.CreateCusEntryLineFee(entryLinePK1, "A99", 200f, 1);
			var entryLinePK2 = CreateCusEntryLine(entryHeaderPK, 1);
			var feePK21 = TestDataCreator.CreateCusEntryLineFee(entryLinePK2, "A99", 30f, 1);
			TestDataCreator.CreateCusEntryLineFee(entryLinePK2, "A99", 40f, 1);

			Db.Connection.Command($@"
INSERT INTO dbo.CusEntryHeaderCharges 
    (C1_PK, C1_CH, C1_ChargeAmount, C1_MethodOfPayment, C1_ClusterKey, C1_ChargeType, C1_SystemCreateTimeUtc, C1_SystemCreateUser, C1_SystemLastEditTimeUtc, C1_SystemLastEditUser) 
VALUES
    (NEWID(), '{entryHeaderPK}', 50.5, 'CAS', 1, 'aa', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
    (NEWID(), '{entryHeaderPK}', 50.5, 'CAS', 1, 'bb', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
    (NEWID(), '{entryHeaderPK}', 100.5, 'DEF', 1, 'aa', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
    (NEWID(), '{entryHeaderPK}', 100.5, 'DEF', 1, 'bb', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');

UPDATE dbo.CusEntryLineFee 
SET 
    CF_MethodOfPayment = 'CAS', 
    CF_SystemLastEditTimeUtc = GETUTCDATE(), 
    CF_SystemLastEditUser = '~BP' 
WHERE 
    CF_PK IN ('{feePK11}', '{feePK12}', '{feePK21}');
").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK, TotalTaxAmountCash FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("TotalTaxAmountCash", new decimal(430), reader.GetDecimal(1));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestTotalTaxAmountNonCash()
		{
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, "IMP", "SEA", "ABC", "ABC", 1);
			CreateCusEntryInstruction(declarationPK, new DateTime(2019, 12, 12), 1);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, "C1", 1);
			var entryLinePK1 = CreateCusEntryLine(entryHeaderPK, 1);
			var feePK11 = TestDataCreator.CreateCusEntryLineFee(entryLinePK1, "A99", 100f, 1);
			var feePK12 = TestDataCreator.CreateCusEntryLineFee(entryLinePK1, "A99", 200f, 1);
			var entryLinePK2 = CreateCusEntryLine(entryHeaderPK, 1);
			var feePK21 = TestDataCreator.CreateCusEntryLineFee(entryLinePK2, "A99", 30f, 1);
			TestDataCreator.CreateCusEntryLineFee(entryLinePK2, "A99", 40f, 1);

			Db.Connection.Command($@"
INSERT INTO dbo.CusEntryHeaderCharges 
    (C1_PK, C1_CH, C1_ChargeAmount, C1_MethodOfPayment, C1_ClusterKey, C1_ChargeType, C1_SystemCreateTimeUtc, C1_SystemCreateUser, C1_SystemLastEditTimeUtc, C1_SystemLastEditUser) 
VALUES
    (NEWID(), '{entryHeaderPK}', 50.5, 'CAS', 1, 'aa', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
    (NEWID(), '{entryHeaderPK}', 50.5, 'CAS', 1, 'bb', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
    (NEWID(), '{entryHeaderPK}', 100.5, 'DEF', 1, 'aa', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),
    (NEWID(), '{entryHeaderPK}', 100.5, 'DEF', 1, 'bb', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');

UPDATE dbo.CusEntryLineFee 
SET 
    CF_MethodOfPayment = 'DEF', 
    CF_SystemLastEditTimeUtc = GETUTCDATE(), 
    CF_SystemLastEditUser = '~BP' 
WHERE 
    CF_PK IN ('{feePK11}', '{feePK12}', '{feePK21}');
").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK, TotalTaxAmountNonCash FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("TotalTaxAmountNonCash", new decimal(530), reader.GetDecimal(1));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestFOBCIFCurrency()
		{
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, "IMP", "SEA", "ABC", "ABC", 1);
			var cusEntryInstructionPK = CreateCusEntryInstruction(declarationPK, new DateTime(2019, 12, 12), 1);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, "C1", 1);
			var invoiceHeaderPK1 = CreateInvoiceHeader(declarationPK, 1);
			var invoiceHeaderPK2 = CreateInvoiceHeader(declarationPK, 1);

			var entryLinePK1 = CreateCusEntryLine(entryHeaderPK, 1);
			CreateInvoiceLine(invoiceHeaderPK1, cusEntryInstructionPK, entryLinePK1, 1);
			var entryLinePK2 = CreateCusEntryLine(entryHeaderPK, 1);
			CreateInvoiceLine(invoiceHeaderPK2, cusEntryInstructionPK, entryLinePK2, 1);

			Db.Connection.Command($@"
UPDATE dbo.JobComInvoiceHeader
SET
	JZ_InvoiceNumber = '456',
	JZ_RX_NKInvoice_Currency = 'USD',
	JZ_SystemLastEditTimeUtc = GETUTCDATE(),
	JZ_SystemLastEditUser = '~BP'
WHERE
	JZ_PK = '{invoiceHeaderPK1}'
UPDATE dbo.JobComInvoiceHeader
SET
	JZ_InvoiceNumber = '123',
	JZ_RX_NKInvoice_Currency = 'AUD',
	JZ_SystemLastEditTimeUtc = GETUTCDATE(),
	JZ_SystemLastEditUser = '~BP'
WHERE
	JZ_PK = '{invoiceHeaderPK2}'
").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK, FOBCIFCurrency FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("FOBCIFCurrency", "AUD", reader.GetString(1));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestFOBCIFAmountIMP()
		{
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, "IMP", "SEA", "ABC", "ABC", 1);
			var cusEntryInstructionPK = CreateCusEntryInstruction(declarationPK, new DateTime(2019, 12, 12), 1);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, "C1", 1);
			var invoiceHeaderPK1 = CreateInvoiceHeader(declarationPK, 1);
			var invoiceHeaderPK2 = CreateInvoiceHeader(declarationPK, 1);

			var entryLinePK1 = CreateCusEntryLine(entryHeaderPK, 1);
			var invoiceLinePK1 = CreateInvoiceLine(invoiceHeaderPK1, cusEntryInstructionPK, entryLinePK1, 1);
			var entryLinePK2 = CreateCusEntryLine(entryHeaderPK, 1);
			var invoiceLinePK2 = CreateInvoiceLine(invoiceHeaderPK2, cusEntryInstructionPK, entryLinePK2, 1);

			Db.Connection.Command($@"
UPDATE dbo.JobComInvoiceHeader
SET
	JZ_InvoiceNumber = '456',
	JZ_RX_NKInvoice_Currency = 'USD',
	JZ_InvoiceCurrExRate = 1,
	JZ_SystemLastEditTimeUtc = GETUTCDATE(),
	JZ_SystemLastEditUser = '~BP'
WHERE
	JZ_PK = '{invoiceHeaderPK1}'
UPDATE dbo.JobComInvoiceHeader
SET
	JZ_InvoiceNumber = '123',
	JZ_RX_NKInvoice_Currency = 'AUD',
	JZ_InvoiceCurrExRate = 4,
	JZ_SystemLastEditTimeUtc = GETUTCDATE(),
	JZ_SystemLastEditUser = '~BP'
WHERE
	JZ_PK = '{invoiceHeaderPK2}'
UPDATE dbo.JobComInvoiceLine
SET
	JI_LinePrice = 100,
	JI_SystemLastEditTimeUtc = GETUTCDATE(),
	JI_SystemLastEditUser = '~BP'
WHERE
	JI_PK = '{invoiceLinePK1}'
UPDATE dbo.JobComInvoiceLine
SET
	JI_LinePrice = 200,
	JI_SystemLastEditTimeUtc = GETUTCDATE(),
	JI_SystemLastEditUser = '~BP'
WHERE
	JI_PK = '{invoiceLinePK2}'
").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK, FOBCIFAmount FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("FOBCIFAmount = 100/4 + 200/1", new decimal(225), reader.GetDecimal(1));
					Assert("There should be no other records", !reader.Read());
				}
			}

			Db.Connection.Command($@"INSERT INTO dbo.JobComInvHeaderCharge(J7_PK, J7_ParentTableCode, J7_ParentID, J7_IsValid, J7_ChargeType, J7_Amount, J7_ExchangeRate, J7_IsStatisticalValueApplicable, J7_IsIncludedInITOT) VALUES (NEWID(), 'JZ', '{invoiceHeaderPK2}', 1, '', 100, 10, 1, 0)
INSERT INTO dbo.JobComInvHeaderCharge(J7_PK, J7_ParentTableCode, J7_ParentID, J7_IsValid, J7_ChargeType, J7_Amount, J7_ExchangeRate, J7_IsStatisticalValueApplicable, J7_IsIncludedInITOT) VALUES (NEWID(), 'JZ', '{invoiceHeaderPK2}', 1, '', 110, 10, 0, 1)
INSERT INTO dbo.JobComInvHeaderCharge(J7_PK, J7_ParentTableCode, J7_ParentID, J7_IsValid, J7_ChargeType, J7_Amount, J7_ExchangeRate, J7_IsStatisticalValueApplicable, J7_IsIncludedInITOT) VALUES (NEWID(), 'JZ', '{invoiceHeaderPK2}', 1, 'OFT', 120, 10, 0, 1)
INSERT INTO dbo.JobComInvHeaderCharge(J7_PK, J7_ParentTableCode, J7_ParentID, J7_IsValid, J7_ChargeType, J7_Amount, J7_ExchangeRate, J7_IsStatisticalValueApplicable, J7_IsIncludedInITOT) VALUES (NEWID(), 'JZ', '{invoiceHeaderPK2}', 1, 'ONS', 130, 10, 0, 1)
INSERT INTO dbo.JobComInvHeaderCharge(J7_PK, J7_ParentTableCode, J7_ParentID, J7_IsValid, J7_ChargeType, J7_Amount, J7_ExchangeRate, J7_IsStatisticalValueApplicable, J7_IsIncludedInITOT, J7_IsDutiable) VALUES (NEWID(), 'JZ', '{invoiceHeaderPK2}', 1, '', 140, 10, 0, 0, 1)
INSERT INTO dbo.JobComInvHeaderCharge(J7_PK, J7_ParentTableCode, J7_ParentID, J7_IsValid, J7_ChargeType, J7_Amount, J7_ExchangeRate, J7_IsStatisticalValueApplicable, J7_IsIncludedInITOT, J7_IsDutiable) VALUES (NEWID(), 'JZ', '{invoiceHeaderPK2}', 1, '', 150, 10, 1, 1, 0)
").ExecuteNonQuery();
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("FOBCIFAmount + 100*10/4 + (-1)*110*10/4 + 140*10/4 + (-1) * 150*10/4", new decimal(175), reader.GetDecimal(1));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestFOBCIFAmountEXP()
		{
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, "EXP", "SEA", "ABC", "ABC", 1);
			var cusEntryInstructionPK = CreateCusEntryInstruction(declarationPK, new DateTime(2019, 12, 12), 1);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, "C1", 1);
			var invoiceHeaderPK1 = CreateInvoiceHeader(declarationPK, 1);
			var invoiceHeaderPK2 = CreateInvoiceHeader(declarationPK, 1);

			var entryLinePK1 = CreateCusEntryLine(entryHeaderPK, 1);
			var invoiceLinePK1 = CreateInvoiceLine(invoiceHeaderPK1, cusEntryInstructionPK, entryLinePK1, 1);
			var entryLinePK2 = CreateCusEntryLine(entryHeaderPK, 1);
			var invoiceLinePK2 = CreateInvoiceLine(invoiceHeaderPK2, cusEntryInstructionPK, entryLinePK2, 1);

			Db.Connection.Command($@"
UPDATE dbo.JobComInvoiceHeader
SET
	JZ_InvoiceNumber = '456',
	JZ_RX_NKInvoice_Currency = 'USD',
	JZ_InvoiceCurrExRate = 1,
	JZ_SystemLastEditTimeUtc = GETUTCDATE(),
	JZ_SystemLastEditUser = '~BP'
WHERE
	JZ_PK = '{invoiceHeaderPK1}'
UPDATE dbo.JobComInvoiceHeader
SET
	JZ_InvoiceNumber = '123',
	JZ_RX_NKInvoice_Currency = 'AUD',
	JZ_InvoiceCurrExRate = 4,
	JZ_SystemLastEditTimeUtc = GETUTCDATE(),
	JZ_SystemLastEditUser = '~BP'
WHERE
	JZ_PK = '{invoiceHeaderPK2}'
UPDATE dbo.JobComInvoiceLine
SET
	JI_LinePrice = 100,
	JI_SystemLastEditTimeUtc = GETUTCDATE(),
	JI_SystemLastEditUser = '~BP'
WHERE
	JI_PK = '{invoiceLinePK1}'
UPDATE dbo.JobComInvoiceLine
SET
	JI_LinePrice = 200,
	JI_SystemLastEditTimeUtc = GETUTCDATE(),
	JI_SystemLastEditUser = '~BP'
WHERE
	JI_PK = '{invoiceLinePK2}'
").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK, FOBCIFAmount FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("FOBCIFAmount = 100/4 + 200/1", new decimal(225), reader.GetDecimal(1));
					Assert("There should be no other records", !reader.Read());
				}
			}

			Db.Connection.Command($@"INSERT INTO dbo.JobComInvHeaderCharge(J7_PK, J7_ParentTableCode, J7_ParentID, J7_IsValid, J7_ChargeType, J7_Amount, J7_ExchangeRate, J7_IsGSTApplicable, J7_IsIncludedInITOT, J7_IsDutiable) VALUES (NEWID(), 'JZ', '{invoiceHeaderPK2}', 1, '', 100, 10, 1, 0, 1)
INSERT INTO dbo.JobComInvHeaderCharge(J7_PK, J7_ParentTableCode, J7_ParentID, J7_IsValid, J7_ChargeType, J7_Amount, J7_ExchangeRate, J7_IsGSTApplicable, J7_IsIncludedInITOT, J7_IsDutiable) VALUES (NEWID(), 'JZ', '{invoiceHeaderPK2}', 1, '', 110, 10, 0, 1, 0)
INSERT INTO dbo.JobComInvHeaderCharge(J7_PK, J7_ParentTableCode, J7_ParentID, J7_IsValid, J7_ChargeType, J7_Amount, J7_ExchangeRate, J7_IsGSTApplicable, J7_IsIncludedInITOT, J7_IsDutiable) VALUES (NEWID(), 'JZ', '{invoiceHeaderPK2}', 1, 'OFT', 120, 10, 0, 1, 0)
INSERT INTO dbo.JobComInvHeaderCharge(J7_PK, J7_ParentTableCode, J7_ParentID, J7_IsValid, J7_ChargeType, J7_Amount, J7_ExchangeRate, J7_IsGSTApplicable, J7_IsIncludedInITOT, J7_IsDutiable) VALUES (NEWID(), 'JZ', '{invoiceHeaderPK2}', 1, 'ONS', 130, 10, 0, 1, 0)
INSERT INTO dbo.JobComInvHeaderCharge(J7_PK, J7_ParentTableCode, J7_ParentID, J7_IsValid, J7_ChargeType, J7_Amount, J7_ExchangeRate, J7_IsGSTApplicable, J7_IsIncludedInITOT, J7_IsDutiable) VALUES (NEWID(), 'JZ', '{invoiceHeaderPK2}', 1, '', 140, 10, 1, 0, 0)
INSERT INTO dbo.JobComInvHeaderCharge(J7_PK, J7_ParentTableCode, J7_ParentID, J7_IsValid, J7_ChargeType, J7_Amount, J7_ExchangeRate, J7_IsGSTApplicable, J7_IsIncludedInITOT, J7_IsDutiable) VALUES (NEWID(), 'JZ', '{invoiceHeaderPK2}', 1, '', 150, 10, 0, 1, 1)
").ExecuteNonQuery();
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("FOBCIFAmount 225 + 100*10/4 +(-1) * 110 * 10 / 4 + (-2) * 120 * 10 / 4 + (-2) * 130 * 10 / 4", new decimal(-1050), reader.GetDecimal(1));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestFOBCIFAmountEXP_EXW()
		{
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, "EXP", "SEA", "ABC", "ABC", 1);
			var cusEntryInstructionPK = CreateCusEntryInstruction(declarationPK, new DateTime(2019, 12, 12), 1);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, "C1", 1);
			var invoiceHeaderPK1 = CreateInvoiceHeader(declarationPK, 1);
			var invoiceHeaderPK2 = CreateInvoiceHeader(declarationPK, 1);

			var entryLinePK1 = CreateCusEntryLine(entryHeaderPK, 1);
			var invoiceLinePK1 = CreateInvoiceLine(invoiceHeaderPK1, cusEntryInstructionPK, entryLinePK1, 1);
			var entryLinePK2 = CreateCusEntryLine(entryHeaderPK, 1);
			var invoiceLinePK2 = CreateInvoiceLine(invoiceHeaderPK2, cusEntryInstructionPK, entryLinePK2, 1);

			Db.Connection.Command($@"
UPDATE dbo.JobComInvoiceHeader
SET
	JZ_InvoiceNumber = '456',
	JZ_RX_NKInvoice_Currency = 'USD',
	JZ_InvoiceCurrExRate = 1,
	JZ_SystemLastEditTimeUtc = GETUTCDATE(),
	JZ_SystemLastEditUser = '~BP'
WHERE
	JZ_PK = '{invoiceHeaderPK1}'
UPDATE dbo.JobComInvoiceHeader
SET
	JZ_InvoiceNumber = '123',
	JZ_RX_NKInvoice_Currency = 'AUD',
	JZ_InvoiceCurrExRate = 4 ,
	JZ_IncoTerm = 'EXW',
	JZ_SystemLastEditTimeUtc = GETUTCDATE(),
	JZ_SystemLastEditUser = '~BP'
WHERE
	JZ_PK = '{invoiceHeaderPK2}'
UPDATE dbo.JobComInvoiceLine
SET
	JI_LinePrice = 100,
	JI_SystemLastEditTimeUtc = GETUTCDATE(),
	JI_SystemLastEditUser = '~BP'
WHERE
	JI_PK = '{invoiceLinePK1}'
UPDATE dbo.JobComInvoiceLine
SET
	JI_LinePrice = 200,
	JI_SystemLastEditTimeUtc = GETUTCDATE(),
	JI_SystemLastEditUser = '~BP'
WHERE
	JI_PK = '{invoiceLinePK2}'
").ExecuteNonQuery();

			var reportSql = @"SELECT JE_PK, FOBCIFAmount FROM Report_TWCustomsClearance()";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("FOBCIFAmount = 100/4 + 200/1", new decimal(225), reader.GetDecimal(1));
					Assert("There should be no other records", !reader.Read());
				}
			}

			Db.Connection.Command($@"INSERT INTO dbo.JobComInvHeaderCharge(J7_PK, J7_ParentTableCode, J7_ParentID, J7_IsValid, J7_ChargeType, J7_Amount, J7_ExchangeRate, J7_IsGSTApplicable, J7_IsIncludedInITOT, J7_IsDutiable) VALUES (NEWID(), 'JZ', '{invoiceHeaderPK2}', 1, '', 100, 10, 1, 0, 1)
INSERT INTO dbo.JobComInvHeaderCharge(J7_PK, J7_ParentTableCode, J7_ParentID, J7_IsValid, J7_ChargeType, J7_Amount, J7_ExchangeRate, J7_IsGSTApplicable, J7_IsIncludedInITOT, J7_IsDutiable) VALUES (NEWID(), 'JZ', '{invoiceHeaderPK2}', 1, '', 110, 10, 0, 1, 0)
INSERT INTO dbo.JobComInvHeaderCharge(J7_PK, J7_ParentTableCode, J7_ParentID, J7_IsValid, J7_ChargeType, J7_Amount, J7_ExchangeRate, J7_IsGSTApplicable, J7_IsIncludedInITOT, J7_IsDutiable) VALUES (NEWID(), 'JZ', '{invoiceHeaderPK2}', 1, 'OFT', 120, 10, 0, 1, 0)
").ExecuteNonQuery();
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(declarationPK, reader.GetGuid(0));
					AssertEquals("FOBCIFAmount + 2*100*10/4 + (-2) * 110*10/4 + (-1) * 120*10/4", new decimal(-125), reader.GetDecimal(1));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		Guid companyPK;
		Guid branchPK;

		protected override void SetUp()
		{
			base.SetUp();
			companyPK = TestDataCreator.CreateCompany("TC1", "TW", "NTD");
			branchPK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_RL_NKHomePort, GB_GC) VALUES (@branchPK, 'TB1', 'TAJNB', @companyPK)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}
		}

		Guid CreateJobDeclaration(Guid branchPK, Guid companyPK, string messageType, string transportMode, string createUser, string lastEditUser, int clusterKey)
		{
			var declarationPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_TransportMode, JE_SystemCreateUser, JE_SystemLastEditUser, JE_ClusterKey)
VALUES (@declarationPK, 'TW', @messageType, @branchPK, @companyPK, @transportMode, @createUser, @lastEditUser, @clusterKey)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@messageType", SqlDbType.VarChar, JobDeclarationSchema.JE_MessageType.MaxLength, messageType);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@transportMode", SqlDbType.VarChar, JobDeclarationSchema.JE_TransportMode.MaxLength, transportMode);
				command.AddParameter("@createUser", SqlDbType.VarChar, JobDeclarationSchema.JE_SystemCreateUser.MaxLength, createUser);
				command.AddParameter("@lastEditUser", SqlDbType.VarChar, JobDeclarationSchema.JE_SystemLastEditUser.MaxLength, lastEditUser);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return declarationPK;
		}

		Guid CreateCusEntryInstruction(Guid declarationPK, DateTime dateForDuty, int clusterKey)
		{
			var entryInstructionPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_JE, CEI_DateForDuty, CEI_ClusterKey, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser)
VALUES (@entryInstructionPK, 'TW', @declarationPK, @dateForDuty, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@entryInstructionPK", SqlDbType.UniqueIdentifier, entryInstructionPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@dateForDuty", SqlDbType.SmallDateTime, CusEntryInstructionSchema.CEI_DateForDuty.MaxLength, dateForDuty);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return entryInstructionPK;
		}

		Guid CreateCusEntryHeader(Guid declarationPK, string entryStatus, int clusterKey)
		{
			var entryHeaderPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_EntryStatus, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
VALUES (@entryHeaderPK, 'TW', @declarationPK, @entryStatus, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@entryHeaderPK", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@entryStatus", SqlDbType.VarChar, CusEntryHeaderSchema.CH_EntryStatus.MaxLength, entryStatus);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return entryHeaderPK;
		}

		Guid CreateCusEntryLine(Guid entryHeaderPK, int clusterKey)
		{
			var entryLinePK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusEntryLine (CL_PK, CL_DataModel, CL_CH, CL_ClusterKey, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser)
VALUES (@entryLinePK, 'TW', @entryHeaderPK, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@entryLinePK", SqlDbType.UniqueIdentifier, entryLinePK);
				command.AddParameter("@entryHeaderPK", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return entryLinePK;
		}

		Guid CreateInvoiceHeader(Guid declarationPK, int clusterKey)
		{
			var invoiceLinePK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobComInvoiceHeader (JZ_PK, JZ_DataModel, JZ_JE, JZ_ClusterKey)
VALUES (@invoiceLinePK, 'TW', @declarationPK, @clusterKey)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return invoiceLinePK;
		}

		Guid CreateInvoiceLine(Guid invoiceHeaderPK, Guid entryInstructionPK, Guid entryLinePK, int clusterKey)
		{
			var invoiceLinePK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobComInvoiceLine (JI_PK, JI_DataModel, JI_JZ, JI_CL, JI_CEI, JI_ClusterKey)
VALUES (@invoiceLinePK, 'TW', @invoiceHeaderPK, @entryLinePK, @entryInstructionPK, @clusterKey)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				command.AddParameter("@entryInstructionPK", SqlDbType.UniqueIdentifier, entryInstructionPK);
				command.AddParameter("@entryLinePK", SqlDbType.UniqueIdentifier, entryLinePK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return invoiceLinePK;
		}
	}
}

