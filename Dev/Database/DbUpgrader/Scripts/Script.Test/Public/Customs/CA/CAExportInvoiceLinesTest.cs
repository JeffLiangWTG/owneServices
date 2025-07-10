using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.CA;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.CA
{
	[TestedType(typeof(CAExportInvoiceLines))]
	class CAExportInvoiceLinesTest : DbCreateScriptTest
	{
		public void TestFunction()
		{
			var companyPK01 = DbHelper.InsertCompany("TC1", "Test Company 01", "CAD", "CA", false, false);

			var branch11 = DbHelper.InsertBranch("TB1", companyPK01, "Test Branch 11");
			var branch12 = DbHelper.InsertBranch("TB2", companyPK01, "Test Branch 12");

			var org01 = DbHelper.InsertOrgHeader("ORG001", "Org 001");
			var org02 = DbHelper.InsertOrgHeader("ORG002", "Org 002");
			var org03 = DbHelper.InsertOrgHeader("ORG003", "Org 003");

			var address = DbHelper.InsertOrgAddress(org01, "Jian Ye", "NJ");

			var dec01 = Guid.NewGuid();
			var dec02 = Guid.NewGuid();
			var dec03 = Guid.NewGuid();

			var invoice01 = Guid.NewGuid();
			var invoice02 = Guid.NewGuid();
			var invoice03 = Guid.NewGuid();

			var entry01 = Guid.NewGuid();
			var entry02 = Guid.NewGuid();
			var entry03 = Guid.NewGuid();

			var orderHeader = TestDataCreator.CreateJobOrderHeader("ON1", address);
			var orderLinePk = TestDataCreator.CreateJobOrderLine(orderHeader);
			var classficationPk = TestDataCreator.CreateCusClassification("LC111", "CA", "IMP");

			var insertSQL = $@"INSERT INTO JobDeclaration(JE_PK, JE_DeclarationReference, JE_MessageType, JE_GC, JE_GB, JE_TransportMode, JE_RL_NKPortOfLoading, JE_OH_Supplier
, JE_OH_Importer, JE_ExportDate, JE_DateOfArrival, JE_VesselName, JE_VoyageFlightNo, JE_MasterBill, JE_HouseBill, JE_GoodsDescription, JE_TotalWeight, JE_TotalWeightUnit
, JE_DataModel, JE_EntrySubmittedDate, JE_RL_NKFinalDestination, JE_RL_NKPortOfArrival, JE_AddInfo, JE_DateAtFinalDestination, JE_DateAtOrigin, JE_OwnerRef, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser, JE_ClusterKey)
VALUES ('{dec01}', 'B0000001', 'EXP', '{companyPK01}', '{branch11}', 'AIR', 'CATOR', '{org01}', '{org02}', '2023-08-01', '2023-08-07', 'VES01', 'FL0001', 'MB0001', 'HB0001', 'DESC 01'
, 1000, 'KG', 'CA', '2023-08-06', 'AUSYD', 'CATOR', 'PortOfExit=0480*ReasonForExportCode=EA*TransportDocumentNumber=111', '2023-08-02', '2023-08-03', 'RRR001', '2023-08-01', '~BP', GetUtcDate(), '~BP', 1)
, ('{dec02}', 'B0000002', 'IMP', '{companyPK01}', '{branch12}', 'AIR', 'JPOSA', '{org02}', '{org03}', '2023-08-01', '2023-08-07', 'VES02', 'FL0002', 'MB0002', 'HB0002', 'DESC 02'
, 2000, 'KG', 'CA', '2023-08-06', 'AUSYD', 'CATOR', 'PortOfExit=0480*ReasonForExportCode=EA*TransportDocumentNumber=222', '2023-08-02', '2023-08-03', 'RRR002', '2023-08-01', '~BP', GetUtcDate(), '~BP', 2)
, ('{dec03}', 'B0000003', 'EXP', '{companyPK01}', '{branch12}', 'AIR', 'USFVC', '{org03}', '{org01}', '2023-08-01', '2023-08-07', 'VES03', 'FL0003', 'MB0003', 'HB0003', 'DESC 03'
, 3000, 'KG', 'CA', '2023-08-06', 'AUSYD', 'CATOR', 'PortOfExit=0480*ReasonForExportCode=EA*TransportDocumentNumber=333', '2023-08-02', '2023-08-03', 'RRR003', '2023-08-01', '~BP', GetUtcDate(), '~BP', 3);

INSERT INTO JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_JE, JZ_ClusterKey, JZ_InvoiceNumber, JZ_InvoiceDate, JZ_RX_NKInvoice_Currency, JZ_InvoiceCurrExRate, JZ_Weight, JZ_WeightUQ
, JZ_IncoTerm, JZ_PaymentAmount, JZ_PaymentDate, JZ_PaymentNo, JZ_RN_NKDefaultOrigin, JZ_RW_NKOriginState, JZ_Volume, JZ_VolumeUQ, JZ_IncoTermPlace, JZ_InvoiceDisplaySequence)
VALUES
('{invoice01}', 'CA', '{dec01}', 1, 'INV0001', '2023-08-06', 'CAD', 0.05, 123, 'KG', 'FOB', 101, '2023-08-01', '21', 'CA', 'QB', 201, 'M3', 'Toronto', '0'),
('{invoice02}', 'CA', '{dec01}', 1, 'INV0002', '2023-08-06', 'CAD', 0.05, 124, 'KG', 'FOB', 102, '2023-08-01', '22', 'CA', 'AL', 202, 'M3', 'Windsor', '1'),
('{invoice03}', 'CA', '{dec03}', 3, 'INV0003', '2023-08-06', 'CAD', 0.05, 124, 'KG', 'FOB', 102, '2023-08-01', '22', 'CA', 'AL', 202, 'M3', 'Windsor', '2');

INSERT INTO JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey, JI_JO, JI_CC, JI_LineNo,JI_RH_NKCommodity_Code,JI_ContainerMode,JI_CustomsQuantity,JI_CustomsUnitQty,JI_CountryOfOrigin,JI_Description,JI_Weight,JI_WeightUQ,JI_Tariff,JI_MatchingKey,JI_NetWeight,JI_NetWeightUQ,JI_OrderNumber,JI_InvoiceQuantity,JI_InvoiceUQ,JI_LinePrice,JI_PartNo,JI_StateOrRegionOfOrigin,JI_SerialNumber,JI_ClassUsageComment,JI_GS_NKClassUsageCommentReviewer,JI_Volume,JI_VolumeUQ,JI_AddInfo)
VALUES
(NEWID(), 'CA', '{invoice01}', '1', '{orderLinePk}', '{classficationPk}', '1', 'C001', '1', '1.11', 'KG', 'CA', 'Cup of tea', '1.22', 'G', '8111.00.00 29', 'Matching Key 001', '1.33', 'MG', 'ON1', '100', 'P', '100', 'PART:0/1', 'WND', '112233', 'Very tasty', 'DNZ', '1.55', 'T', 'CA_ConveyanceIdentificationNumber=CIN01*'),
(NEWID(), 'CA', '{invoice01}', '1', '{orderLinePk}', '{classficationPk}', '2', 'C002', '2', '2.11', 'KG', 'CA', 'Bottle of water', '2.22', 'G', '8222.00.00 29', 'Matching Key 002', '2.33', 'MG', 'ON2', '200', 'P', '100', 'PART:0/2', 'WND', '222233', 'Very thirsty', 'DNN', '2.55', 'T', ''),
(NEWID(), 'CA', '{invoice03}', '3', null, null, '1', '', '', '0', '', '', '', '0', '', '', '', '0', '', '', '0', '', '0', '', '', '', '', '', '0', '', '');

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

			var resultSQL = $"SELECT * FROM dbo.CAExportInvoiceLines('{companyPK01}', 'AU', null, null, null, null) ORDER BY JZ_InvoiceNumber, JI_LineNo";
			using (var command = Db.Connection.Command(resultSQL))
			{
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						AssertEquals(true, reader.Read());
						AssertEquals("B0000001", reader["JobNumber"].ToString());
						AssertEquals("6/08/2023 12:00:00 AM", reader["JZ_InvoiceDate"].ToString());
						AssertEquals("0", reader["JZ_InvoiceDisplaySequence"].ToString());
						AssertEquals("CAD", reader["JZ_RX_NKInvoice_Currency"].ToString());
						AssertEquals("INV0001", reader["JZ_InvoiceNumber"].ToString());
						AssertEquals("CATOR", reader["PortOfDischarge"].ToString());
						AssertEquals("0480", reader["CustomsPortOfExit"].ToString());
						AssertEquals(org01.ToString(), reader["ExporterPK"].ToString());
						AssertEquals(org02.ToString(), reader["ConsigneePK"].ToString());
						AssertEquals("CATOR", reader["LoadingPort"].ToString());
						AssertEquals("AIR", reader["TransportMode"].ToString());
						AssertEquals("AWO", reader["EXPStatus"].ToString());
						AssertEquals("Awaiting G7 Export Message Original", reader["EXPStatusDescription"].ToString());
						AssertEquals("1/08/2023 12:00:00 AM", reader["ExportDate"].ToString());
						AssertEquals("LC111", reader["LookupCode"].ToString());
						AssertEquals("1", reader["JI_LineNo"].ToString());
						AssertEquals("C001", reader["JI_RH_NKCommodity_Code"].ToString());
						AssertEquals("1", reader["JI_ContainerMode"].ToString());
						AssertEquals(1.11, double.Parse(reader["JI_CustomsQuantity"].ToString()));
						AssertEquals("KG", reader["JI_CustomsUnitQty"].ToString());
						AssertEquals("CA", reader["JI_CountryOfOrigin"].ToString());
						AssertEquals("Cup of tea", reader["JI_Description"].ToString());
						AssertEquals(1.22, double.Parse(reader["JI_Weight"].ToString()));
						AssertEquals("G", reader["JI_WeightUQ"].ToString());
						AssertEquals("8111.00.00 29", reader["JI_Tariff"].ToString());
						AssertEquals("Matching Key 001", reader["JI_MatchingKey"].ToString());
						AssertEquals(1.33, double.Parse(reader["JI_NetWeight"].ToString()));
						AssertEquals("MG", reader["JI_NetWeightUQ"].ToString());
						AssertEquals("ON1", reader["JI_OrderNumber"].ToString());
						AssertEquals(100d, double.Parse(reader["JI_InvoiceQuantity"].ToString()));
						AssertEquals("P", reader["JI_InvoiceUQ"].ToString());
						AssertEquals(100d, double.Parse(reader["JI_LinePrice"].ToString()));
						AssertEquals(1d, double.Parse(reader["UnitPrice"].ToString()));
						AssertEquals("PART:0/1", reader["JI_PartNo"].ToString());
						AssertEquals("WND", reader["JI_StateOrRegionOfOrigin"].ToString());
						AssertEquals("112233", reader["JI_SerialNumber"].ToString());
						AssertEquals("Very tasty", reader["JI_ClassUsageComment"].ToString());
						AssertEquals("DNZ", reader["JI_GS_NKClassUsageCommentReviewer"].ToString());
						AssertEquals(1.55, double.Parse(reader["JI_Volume"].ToString()));
						AssertEquals("T", reader["JI_VolumeUQ"].ToString());
						AssertEquals("1", reader["JO_LineNo"].ToString());
						AssertEquals("CIN01", reader["ConveyanceID"].ToString());
					});

					CombineAssertions(() =>
					{
						AssertEquals(true, reader.Read());
						AssertEquals("B0000001", reader["JobNumber"].ToString());
						AssertEquals("6/08/2023 12:00:00 AM", reader["JZ_InvoiceDate"].ToString());
						AssertEquals("0", reader["JZ_InvoiceDisplaySequence"].ToString());
						AssertEquals("CAD", reader["JZ_RX_NKInvoice_Currency"].ToString());
						AssertEquals("INV0001", reader["JZ_InvoiceNumber"].ToString());
						AssertEquals("CATOR", reader["PortOfDischarge"].ToString());
						AssertEquals("0480", reader["CustomsPortOfExit"].ToString());
						AssertEquals(org01.ToString(), reader["ExporterPK"].ToString());
						AssertEquals(org02.ToString(), reader["ConsigneePK"].ToString());
						AssertEquals("CATOR", reader["LoadingPort"].ToString());
						AssertEquals("AIR", reader["TransportMode"].ToString());
						AssertEquals("AWO", reader["EXPStatus"].ToString());
						AssertEquals("Awaiting G7 Export Message Original", reader["EXPStatusDescription"].ToString());
						AssertEquals("1/08/2023 12:00:00 AM", reader["ExportDate"].ToString());
						AssertEquals("LC111", reader["LookupCode"].ToString());
						AssertEquals("2", reader["JI_LineNo"].ToString());
						AssertEquals("C002", reader["JI_RH_NKCommodity_Code"].ToString());
						AssertEquals("2", reader["JI_ContainerMode"].ToString());
						AssertEquals(2.11, double.Parse(reader["JI_CustomsQuantity"].ToString()));
						AssertEquals("KG", reader["JI_CustomsUnitQty"].ToString());
						AssertEquals("CA", reader["JI_CountryOfOrigin"].ToString());
						AssertEquals("Bottle of water", reader["JI_Description"].ToString());
						AssertEquals(2.22, double.Parse(reader["JI_Weight"].ToString()));
						AssertEquals("G", reader["JI_WeightUQ"].ToString());
						AssertEquals("8222.00.00 29", reader["JI_Tariff"].ToString());
						AssertEquals("Matching Key 002", reader["JI_MatchingKey"].ToString());
						AssertEquals(2.33, double.Parse(reader["JI_NetWeight"].ToString()));
						AssertEquals("MG", reader["JI_NetWeightUQ"].ToString());
						AssertEquals("ON2", reader["JI_OrderNumber"].ToString());
						AssertEquals(200d, double.Parse(reader["JI_InvoiceQuantity"].ToString()));
						AssertEquals("P", reader["JI_InvoiceUQ"].ToString());
						AssertEquals(100d, double.Parse(reader["JI_LinePrice"].ToString()));
						AssertEquals(0.5, double.Parse(reader["UnitPrice"].ToString()));
						AssertEquals("PART:0/2", reader["JI_PartNo"].ToString());
						AssertEquals("WND", reader["JI_StateOrRegionOfOrigin"].ToString());
						AssertEquals("222233", reader["JI_SerialNumber"].ToString());
						AssertEquals("Very thirsty", reader["JI_ClassUsageComment"].ToString());
						AssertEquals("DNN", reader["JI_GS_NKClassUsageCommentReviewer"].ToString());
						AssertEquals(2.55, double.Parse(reader["JI_Volume"].ToString()));
						AssertEquals("T", reader["JI_VolumeUQ"].ToString());
						AssertEquals("1", reader["JO_LineNo"].ToString());
						AssertEquals("", reader["ConveyanceID"].ToString());
					});

					CombineAssertions(() =>
					{
						AssertEquals(true, reader.Read());
						AssertEquals("B0000003", reader["JobNumber"].ToString());
						AssertEquals("6/08/2023 12:00:00 AM", reader["JZ_InvoiceDate"].ToString());
						AssertEquals("2", reader["JZ_InvoiceDisplaySequence"].ToString());
						AssertEquals("CAD", reader["JZ_RX_NKInvoice_Currency"].ToString());
						AssertEquals("INV0003", reader["JZ_InvoiceNumber"].ToString());
						AssertEquals("CATOR", reader["PortOfDischarge"].ToString());
						AssertEquals("0480", reader["CustomsPortOfExit"].ToString());
						AssertEquals(org03.ToString(), reader["ExporterPK"].ToString());
						AssertEquals(org01.ToString(), reader["ConsigneePK"].ToString());
						AssertEquals("USFVC", reader["LoadingPort"].ToString());
						AssertEquals("AIR", reader["TransportMode"].ToString());
						AssertEquals("CEO", reader["EXPStatus"].ToString());
						AssertEquals("", reader["EXPStatusDescription"].ToString());
						AssertEquals("1/08/2023 12:00:00 AM", reader["ExportDate"].ToString());
						AssertEquals("", reader["LookupCode"].ToString());
						AssertEquals("1", reader["JI_LineNo"].ToString());
						AssertEquals("", reader["JI_RH_NKCommodity_Code"].ToString());
						AssertEquals("", reader["JI_ContainerMode"].ToString());
						AssertEquals(0d, double.Parse(reader["JI_CustomsQuantity"].ToString()));
						AssertEquals("", reader["JI_CustomsUnitQty"].ToString());
						AssertEquals("", reader["JI_CountryOfOrigin"].ToString());
						AssertEquals("", reader["JI_Description"].ToString());
						AssertEquals(0d, double.Parse(reader["JI_Weight"].ToString()));
						AssertEquals("", reader["JI_WeightUQ"].ToString());
						AssertEquals("", reader["JI_Tariff"].ToString());
						AssertEquals("", reader["JI_MatchingKey"].ToString());
						AssertEquals(0d, double.Parse(reader["JI_NetWeight"].ToString()));
						AssertEquals("", reader["JI_NetWeightUQ"].ToString());
						AssertEquals("", reader["JI_OrderNumber"].ToString());
						AssertEquals(0d, double.Parse(reader["JI_InvoiceQuantity"].ToString()));
						AssertEquals("", reader["JI_InvoiceUQ"].ToString());
						AssertEquals(0d, double.Parse(reader["JI_LinePrice"].ToString()));
						AssertEquals(0d, double.Parse(reader["UnitPrice"].ToString()));
						AssertEquals("", reader["JI_PartNo"].ToString());
						AssertEquals("", reader["JI_StateOrRegionOfOrigin"].ToString());
						AssertEquals("", reader["JI_SerialNumber"].ToString());
						AssertEquals("", reader["JI_ClassUsageComment"].ToString());
						AssertEquals("", reader["JI_GS_NKClassUsageCommentReviewer"].ToString());
						AssertEquals(0d, double.Parse(reader["JI_Volume"].ToString()));
						AssertEquals("", reader["JI_VolumeUQ"].ToString());
						AssertEquals("", reader["JO_LineNo"].ToString());
						AssertEquals("", reader["ConveyanceID"].ToString());
					});

					AssertEquals(false, reader.Read());
				}
			}
		}
	}
}
