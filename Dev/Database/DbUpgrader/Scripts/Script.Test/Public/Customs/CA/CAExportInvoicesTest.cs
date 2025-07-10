using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.CA;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.CA
{
	[TestedType(typeof(CAExportInvoices))]
	class CAExportInvoicesTest : DbCreateScriptTest
	{
		public void TestFunction()
		{
			var companyPK01 = DbHelper.InsertCompany("TC1", "Test Company 01", "CAD", "CA", false, false);

			var branch11 = DbHelper.InsertBranch("TB1", companyPK01, "Test Branch 11");
			var branch12 = DbHelper.InsertBranch("TB2", companyPK01, "Test Branch 12");

			var org01 = DbHelper.InsertOrgHeader("ORG001", "Org 001");
			var org02 = DbHelper.InsertOrgHeader("ORG002", "Org 002");
			var org03 = DbHelper.InsertOrgHeader("ORG003", "Org 003");

			var dec01 = Guid.NewGuid();
			var dec02 = Guid.NewGuid();
			var dec03 = Guid.NewGuid();

			var invoice01 = Guid.NewGuid();
			var invoice02 = Guid.NewGuid();
			var invoice03 = Guid.NewGuid();

			var entry01 = Guid.NewGuid();
			var entry02 = Guid.NewGuid();
			var entry03 = Guid.NewGuid();

			var insertSQL = $@"INSERT INTO JobDeclaration(JE_PK, JE_DeclarationReference, JE_MessageType, JE_GC, JE_GB, JE_TransportMode, JE_RL_NKPortOfLoading, JE_OH_Supplier
, JE_OH_Importer, JE_ExportDate, JE_DateOfArrival, JE_VesselName, JE_VoyageFlightNo, JE_MasterBill, JE_HouseBill, JE_GoodsDescription, JE_TotalWeight, JE_TotalWeightUnit
, JE_DataModel, JE_EntrySubmittedDate, JE_RL_NKFinalDestination, JE_RL_NKPortOfArrival, JE_AddInfo, JE_DateAtFinalDestination, JE_DateAtOrigin, JE_OwnerRef, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser, JE_ClusterKey)
VALUES ('{dec01}', 'B0000001', 'EXP', '{companyPK01}', '{branch11}', 'AIR', 'CATOR', '{org01}', '{org02}', '2023-08-01', '2023-08-07', 'VES01', 'FL0001', 'MB0001', 'HB0001', 'DESC 01'
, 1000, 'KG', 'CA', '2023-08-06', 'AUSYD', 'CATOR', 'PortOfExit=0480*ReasonForExportCode=EA*TransportDocumentNumber=111', '2023-08-02', '2023-08-03', 'RRR001', '2023-08-01', '~BP', GetUtcDate(), '~BP', 1)
, ('{dec02}', 'B0000002', 'IMP', '{companyPK01}', '{branch12}', 'AIR', 'JPOSA', '{org02}', '{org03}', '2023-08-01', '2023-08-07', 'VES02', 'FL0002', 'MB0002', 'HB0002', 'DESC 02'
, 2000, 'KG', 'CA', '2023-08-06', 'AUSYD', 'CATOR', 'PortOfExit=0480*ReasonForExportCode=EA*TransportDocumentNumber=222', '2023-08-02', '2023-08-03', 'RRR002', '2023-08-01', '~BP', GetUtcDate(), '~BP', 2)
, ('{dec03}', 'B0000003', 'EXP', '{companyPK01}', '{branch12}', 'AIR', 'USFVC', '{org03}', '{org01}', '2023-08-01', '2023-08-07', 'VES03', 'FL0003', 'MB0003', 'HB0003', 'DESC 03'
, 3000, 'KG', 'CA', '2023-08-06', 'CATOR', 'CATOR', 'PortOfExit=0480*ReasonForExportCode=EA*TransportDocumentNumber=333', '2023-08-02', '2023-08-03', 'RRR003', '2023-08-01', '~BP', GetUtcDate(), '~BP', 3);

INSERT INTO JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_JE, JZ_ClusterKey, JZ_InvoiceNumber, JZ_InvoiceDate, JZ_RX_NKInvoice_Currency, JZ_InvoiceCurrExRate, JZ_Weight, JZ_WeightUQ
, JZ_IncoTerm, JZ_PaymentAmount, JZ_PaymentDate, JZ_PaymentNo, JZ_RN_NKDefaultOrigin, JZ_RW_NKOriginState, JZ_Volume, JZ_VolumeUQ, JZ_IncoTermPlace)
VALUES ('{invoice01}', 'CA', '{dec01}', 1, 'INV0001', '2023-08-06', 'CAD', 0.05, 123, 'KG', 'FOB', 101, '2023-08-01', '21', 'CA', 'QB', 201, 'M3', 'Toronto')
, ('{invoice02}', 'CA', '{dec01}', 1, 'INV0002', '2023-08-06', 'CAD', 0.05, 124, 'KG', 'FOB', 102, '2023-08-01', '22', 'CA', 'AL', 202, 'M3', 'Windsor')
, ('{invoice03}', 'CA', '{dec03}', 2, 'INV0003', '2023-08-06', 'CAD', 0.05, 124, 'KG', 'FOB', 102, '2023-08-01', '22', 'CA', 'AL', 202, 'M3', 'Windsor');

INSERT INTO JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey)
VALUES (NEWID(), 'CA', '{invoice01}', 1)
, (NEWID(), 'CA', '{invoice01}', 1);

INSERT INTO CusEntryHeader(CH_PK, CH_JE, CH_MessageType, CH_Status, CH_DataModel, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
VALUES ('{entry01}', '{dec01}', 'G7X', 'AWO', 'CA', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
, ('{entry02}', '{dec02}', 'B3C', 'AWO', 'CA', 2, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
, ('{entry03}', '{dec03}', 'G7X', 'CEO', 'CA', 3, GetUtcDate(), '~BP', GetUtcDate(), '~BP');";

			using (var command = Db.Connection.Command(insertSQL))
			{
				command.ExecuteNonQuery();
			}
			
			TestDataCreator.CreateCusEntryNum(entry01, "CusEntryHeader", "0000001", "EXP", "CUS", "CA");
			TestDataCreator.CreateCusEntryNum(entry02, "CusEntryHeader", "0000002", "IMP", "CUS", "CA");
			TestDataCreator.CreateCusEntryNum(entry03, "CusEntryHeader", "0000003", "EXP", "CUS", "CA");
			TestDataCreator.CreateCusEntryNum(dec01, "JobDeclaration", "0000004", "CCN", "OTH", "CA");
			TestDataCreator.CreateCusEntryNum(dec02, "JobDeclaration", "0000005", "CCN", "OTH", "CA");
			TestDataCreator.CreateCusEntryNum(dec03, "JobDeclaration", "0000006", "CCN", "OTH", "CA");

			var rc11 = TestDataCreator.CreateRefContainer("REF001");
			var rc12 = TestDataCreator.CreateRefContainer("REF002");
			var rc21 = TestDataCreator.CreateRefContainer("REF003");
			var rc22 = TestDataCreator.CreateRefContainer("REF004");
			var rc31 = TestDataCreator.CreateRefContainer("REF005");
			var rc32 = TestDataCreator.CreateRefContainer("REF006");

			var jc11 = TestDataCreator.CreateJobContainer("CON0011", rc11);
			var jc12 = TestDataCreator.CreateJobContainer("CON0012", rc12);
			var jc21 = TestDataCreator.CreateJobContainer("CON0021", rc21);
			var jc22 = TestDataCreator.CreateJobContainer("CON0022", rc22);
			var jc31 = TestDataCreator.CreateJobContainer("CON0031", rc31);
			var jc32 = TestDataCreator.CreateJobContainer("CON0032", rc32);

			TestDataCreator.CreateCusContainer("", dec01, 1, "CA", jc11);
			TestDataCreator.CreateCusContainer("", dec01, 1, "CA", jc12);
			TestDataCreator.CreateCusContainer("", dec02, 2, "CA", jc21);
			TestDataCreator.CreateCusContainer("", dec02, 2, "CA", jc22);
			TestDataCreator.CreateCusContainer("", dec03, 3, "CA", jc31);
			TestDataCreator.CreateCusContainer("", dec03, 3, "CA", jc32);

			TestDataCreator.CreateCusAddInfo("CAC", "CCNInfoNumber=2066-222222", "JE", dec01);
			TestDataCreator.CreateCusAddInfo("CAC", "CCNInfoNumber=2026-032018", "JE", dec01);
			TestDataCreator.CreateCusAddInfo("CAC", "CCNInfoNumber=014-1235678", "JE", dec02);
			TestDataCreator.CreateCusAddInfo("CAC", "CCNInfoNumber=014-2345689", "JE", dec02);
			TestDataCreator.CreateCusAddInfo("CAC", "CCNInfoNumber=2026-072718", "JE", dec03);
			TestDataCreator.CreateCusAddInfo("CAC", "CCNInfoNumber=2026-012718", "JE", dec03);

			var resultSQL = $"SELECT * FROM dbo.CAExportInvoices('{companyPK01}', 'AU', null, null, null, null) ORDER BY InvoiceNumber";
			using (var command = Db.Connection.Command(resultSQL))
			{
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						AssertEquals(true, reader.Read());
						AssertEquals("B0000001", reader["JobNumber"].ToString());
						AssertEquals("0000001", reader["TransactionNumber"].ToString());
						AssertEquals("111", reader["TransportDoc"].ToString());
						AssertEquals("CATOR", reader["PortOfDischarge"].ToString());
						AssertEquals("0480", reader["CustomsPortOfExit"].ToString());
						AssertEquals("CATOR", reader["LoadingPort"].ToString());
						AssertEquals(org01.ToString(), reader["ExporterPK"].ToString());
						AssertEquals(org02.ToString(), reader["ConsigneePK"].ToString());
						AssertEquals("AIR", reader["TransportMode"].ToString());
						AssertEquals("EA", reader["ReasonForExport"].ToString());
						AssertEquals("VES01", reader["Vessel"].ToString());
						AssertEquals("FL0001", reader["VoyageFlightNumber"].ToString());
						AssertEquals("INV0001", reader["InvoiceNumber"].ToString());
						AssertEquals("6/08/2023 12:00:00 AM", reader["InvoiceDate"].ToString());
						AssertEquals("CAD", reader["InvoiceCurrency"].ToString());
						AssertEquals(0.05m, (decimal)(reader["ExchangeRate"]));
						AssertEquals("DESC 01", reader["GoodsDescription"].ToString());
						AssertEquals(123m, (decimal)(reader["GrossWeight"]));
						AssertEquals("KG", reader["GrossWeightUQ"].ToString());
						AssertEquals("2", reader["NumberOfInvoiceLines"].ToString());
						AssertEquals(branch11.ToString(), reader["BranchPK"].ToString());
						AssertEquals("TB1", reader["DeclarationBranch"].ToString());
						AssertEquals("1/08/2023 12:00:00 AM", reader["CreatedOn"].ToString());
						AssertEquals("7/08/2023 12:00:00 AM", reader["DateOfArrival"].ToString());
						AssertEquals("2/08/2023 12:00:00 AM", reader["ETA"].ToString());
						AssertEquals("3/08/2023 12:00:00 AM", reader["ETD"].ToString());
						AssertEquals("AWO", reader["EXPStatus"].ToString());
						AssertEquals("Awaiting G7 Export Message Original", reader["EXPStatusDescription"].ToString());
						AssertEquals("1/08/2023 12:00:00 AM", reader["ExportDate"].ToString());
						AssertEquals("FOB", reader["Incoterm"].ToString());
						AssertEquals(101m, (decimal)(reader["PaymentAmount"]));
						AssertEquals("1/08/2023 12:00:00 AM", reader["PaymentDate"].ToString());
						AssertEquals("21", reader["PaymentNumber"].ToString());
						AssertEquals("CA", reader["CountryOfOrigin"].ToString());
						AssertEquals("QB", reader["ProvinceOfOrigin"].ToString());
						AssertEquals(201m, (decimal)(reader["Volume"]));
						AssertEquals("M3", reader["VolumeUQ"].ToString());
						AssertEquals("Toronto", reader["AgreedPlace"].ToString());
					});

					CombineAssertions(() =>
					{
						AssertEquals(true, reader.Read());
						AssertEquals("B0000001", reader["JobNumber"].ToString());
						AssertEquals("0000001", reader["TransactionNumber"].ToString());
						AssertEquals("111", reader["TransportDoc"].ToString());
						AssertEquals("CATOR", reader["PortOfDischarge"].ToString());
						AssertEquals("0480", reader["CustomsPortOfExit"].ToString());
						AssertEquals("CATOR", reader["LoadingPort"].ToString());
						AssertEquals(org01.ToString(), reader["ExporterPK"].ToString());
						AssertEquals(org02.ToString(), reader["ConsigneePK"].ToString());
						AssertEquals("AIR", reader["TransportMode"].ToString());
						AssertEquals("EA", reader["ReasonForExport"].ToString());
						AssertEquals("VES01", reader["Vessel"].ToString());
						AssertEquals("FL0001", reader["VoyageFlightNumber"].ToString());
						AssertEquals("INV0002", reader["InvoiceNumber"].ToString());
						AssertEquals("6/08/2023 12:00:00 AM", reader["InvoiceDate"].ToString());
						AssertEquals("CAD", reader["InvoiceCurrency"].ToString());
						AssertEquals(0.05m, (decimal)(reader["ExchangeRate"]));
						AssertEquals("DESC 01", reader["GoodsDescription"].ToString());
						AssertEquals(124m, (decimal)(reader["GrossWeight"]));
						AssertEquals("KG", reader["GrossWeightUQ"].ToString());
						AssertEquals("0", reader["NumberOfInvoiceLines"].ToString());
						AssertEquals(branch11.ToString(), reader["BranchPK"].ToString());
						AssertEquals("TB1", reader["DeclarationBranch"].ToString());
						AssertEquals("1/08/2023 12:00:00 AM", reader["CreatedOn"].ToString());
						AssertEquals("7/08/2023 12:00:00 AM", reader["DateOfArrival"].ToString());
						AssertEquals("2/08/2023 12:00:00 AM", reader["ETA"].ToString());
						AssertEquals("3/08/2023 12:00:00 AM", reader["ETD"].ToString());
						AssertEquals("AWO", reader["EXPStatus"].ToString());
						AssertEquals("Awaiting G7 Export Message Original", reader["EXPStatusDescription"].ToString());
						AssertEquals("1/08/2023 12:00:00 AM", reader["ExportDate"].ToString());
						AssertEquals("FOB", reader["Incoterm"].ToString());
						AssertEquals(102m, (decimal)(reader["PaymentAmount"]));
						AssertEquals("1/08/2023 12:00:00 AM", reader["PaymentDate"].ToString());
						AssertEquals("22", reader["PaymentNumber"].ToString());
						AssertEquals("CA", reader["CountryOfOrigin"].ToString());
						AssertEquals("AL", reader["ProvinceOfOrigin"].ToString());
						AssertEquals(202m, (decimal)(reader["Volume"]));
						AssertEquals("M3", reader["VolumeUQ"].ToString());
						AssertEquals("Windsor", reader["AgreedPlace"].ToString());
					});

					AssertEquals(false, reader.Read());
				}
			}
		}
	}
}
