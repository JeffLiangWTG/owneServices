using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZA;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZA.Testing
{
	[TestedType(typeof(Report_ZACustomsEntryLines))]
	class Report_ZACustomsEntryLinesTest : DbCreateScriptTest
	{
		public void TestFunctionalityDbFunction()
		{
			var clusterKey = 1;
			var eta = new DateTime(2023, 09, 23);
			var assessmentDate = new DateTime(2023, 09, 24);
			var entryReleaseDate = new DateTime(2023, 09, 25);
			var entrySubmittedDate = new DateTime(2023, 09, 26);
			var createTime = new DateTime(2023, 09, 28, 10, 05, 01);

			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001000", "IMP", "SEA", "Calypso", "0308", eta, clusterKey);
			var instructionPK = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A01", string.Empty, assessmentDate, clusterKey);
			var entryPK = TestDataCreator.CreateCusEntryHeader(isValid: true, "IMP", "ACK", "1", 1, declarationPK, entrySubmittedDate, entryReleaseDate, instructionPK, createTime, clusterKey);
			var entryLinePK = TestDataCreator.CreateCusEntryLine(entryPK, clusterKey, 1, 308, "8.8.8.1");
			var entryNumPK = TestDataCreator.CreateCusEntryNum(entryPK, "CusEntryHeader", "MRN-1", "MRN", "ZA");
			var invoicePK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, isGroupInvoice: false, clusterKey, supplierPK, string.Empty, importerPK);
			var invoiceLinePK = TestDataCreator.CreateJobComInvoiceLine(invoicePK, clusterKey, 11, 22, 33, "U1", "V1", "W1", "Order-1", "PartNo-1", "PT", 1, entryLinePK);

			using (var command = CargoWise.Data.Db.Connection.Command($@"SELECT * FROM Report_ZACustomsEntryLines ('{companyPK}', 'IMP', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)"))
			{
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						AssertEquals("Result not empty", expected: true, reader.Read());
						AssertEquals("JobNumber", "B00001000", reader["JobNumber"]);
						AssertEquals("EntryNumber", "MRN-1", reader["EntryNumber"]);
						AssertEquals("EntryReleaseDate", entryReleaseDate, reader["EntryReleaseDate"]);
						AssertEquals("ShipmentType", "IMP", reader["ShipmentType"]);
						AssertEquals("MessageStatus", "ACK", reader["MessageStatus"]);
						AssertEquals("EntryStatus", "1", reader["EntryStatus"]);
						AssertEquals("EntrySubmittedDate", entrySubmittedDate, reader["EntrySubmittedDate"]);
						AssertEquals("CustomsValue", 308m, reader["CustomsValue"]);
						AssertEquals("EntryLineNo", (short)1, reader["EntryLineNo"]);
						AssertEquals("TariffCode", "8.8.8.1", reader["TariffCode"]);
						AssertEquals("CPC", "A01", reader["CPC"]);
						AssertEquals("AssessmentDate", assessmentDate, reader["AssessmentDate"]);
						AssertEquals("VesselName", "Calypso", reader["VesselName"]);
						AssertEquals("VoyFlight", "0308", reader["VoyFlight"]);
						AssertEquals("ETA", eta, reader["ETA"]);
						AssertEquals("JobNumber", "B00001000", reader["JobNumber"]);
						AssertEquals("TransportMode", "SEA", reader["TransportMode"]);
						AssertEquals("ClientRef", "OwnerReference-1", reader["ClientRef"]);
						AssertEquals("TransportDocNo", "MasterBill-1", reader["TransportDocNo"]);
						AssertEquals("HouseBill", "HouseBill-1", reader["HouseBill"]);
						AssertEquals("CustomsOffice", "OF1", reader["CustomsOffice"]);
						AssertEquals("StatsQty", 11m, reader["StatsQty"]);
						AssertEquals("AdditionalQty1", 22m, reader["AdditionalQty1"]);
						AssertEquals("AdditionalQty2", 33m, reader["AdditionalQty2"]);
						AssertEquals("StatsQtyUnit", "U1", reader["StatsQtyUnit"]);
						AssertEquals("OrderNumber", "Order-1", reader["OrderNumber"]);
						AssertEquals("ProductCode", "PartNo-1", reader["ProductCode"]);
						AssertEquals("GoodsOrigin", "PT", reader["GoodsOrigin"]);
						AssertEquals("AdditionalQty1Unit", "V1", reader["AdditionalQty1Unit"]);
						AssertEquals("AdditionalQty2Unit", "W1", reader["AdditionalQty2Unit"]);
						AssertEquals("EntryNumber", "MRN-1", reader["EntryNumber"]);
						AssertEquals("NoOfContainers", 0, reader["NoOfContainers"]);
						AssertEquals("ImporterCode", "JEIMPORTER", reader["ImporterCode"]);
						AssertEquals("SupplierCode", "JESUPPLIER", reader["SupplierCode"]);
					});
				}
			}

			using (var command = CargoWise.Data.Db.Connection.Command($@"SELECT * FROM Report_ZACustomsEntryLines ('{companyPK}', 'IMP', NULL, NULL, '1900-01-01 00:00:00', '1900-01-01 00:00:00', '1900-01-01 00:00:00', '1900-01-01 00:00:00', '1900-01-01 00:00:00', '1900-01-01 00:00:00', NULL, NULL, NULL, '1900-01-01 00:00:00', '1900-01-01 00:00:00')"))
			{
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						AssertEquals("Result not empty", expected: true, reader.Read());
						AssertEquals("JobNumber", "B00001000", reader["JobNumber"]);
						AssertEquals("EntryNumber", "MRN-1", reader["EntryNumber"]);
						AssertEquals("EntryReleaseDate", entryReleaseDate, reader["EntryReleaseDate"]);
						AssertEquals("ShipmentType", "IMP", reader["ShipmentType"]);
						AssertEquals("MessageStatus", "ACK", reader["MessageStatus"]);
						AssertEquals("EntryStatus", "1", reader["EntryStatus"]);
						AssertEquals("EntrySubmittedDate", entrySubmittedDate, reader["EntrySubmittedDate"]);
						AssertEquals("CustomsValue", 308m, reader["CustomsValue"]);
						AssertEquals("EntryLineNo", (short)1, reader["EntryLineNo"]);
						AssertEquals("TariffCode", "8.8.8.1", reader["TariffCode"]);
						AssertEquals("CPC", "A01", reader["CPC"]);
						AssertEquals("AssessmentDate", assessmentDate, reader["AssessmentDate"]);
						AssertEquals("VesselName", "Calypso", reader["VesselName"]);
						AssertEquals("VoyFlight", "0308", reader["VoyFlight"]);
						AssertEquals("ETA", eta, reader["ETA"]);
						AssertEquals("JobNumber", "B00001000", reader["JobNumber"]);
						AssertEquals("TransportMode", "SEA", reader["TransportMode"]);
						AssertEquals("ClientRef", "OwnerReference-1", reader["ClientRef"]);
						AssertEquals("TransportDocNo", "MasterBill-1", reader["TransportDocNo"]);
						AssertEquals("HouseBill", "HouseBill-1", reader["HouseBill"]);
						AssertEquals("CustomsOffice", "OF1", reader["CustomsOffice"]);
						AssertEquals("StatsQty", 11m, reader["StatsQty"]);
						AssertEquals("AdditionalQty1", 22m, reader["AdditionalQty1"]);
						AssertEquals("AdditionalQty2", 33m, reader["AdditionalQty2"]);
						AssertEquals("StatsQtyUnit", "U1", reader["StatsQtyUnit"]);
						AssertEquals("OrderNumber", "Order-1", reader["OrderNumber"]);
						AssertEquals("ProductCode", "PartNo-1", reader["ProductCode"]);
						AssertEquals("GoodsOrigin", "PT", reader["GoodsOrigin"]);
						AssertEquals("AdditionalQty1Unit", "V1", reader["AdditionalQty1Unit"]);
						AssertEquals("AdditionalQty2Unit", "W1", reader["AdditionalQty2Unit"]);
						AssertEquals("EntryNumber", "MRN-1", reader["EntryNumber"]);
						AssertEquals("NoOfContainers", 0, reader["NoOfContainers"]);
						AssertEquals("ImporterCode", "JEIMPORTER", reader["ImporterCode"]);
						AssertEquals("SupplierCode", "JESUPPLIER", reader["SupplierCode"]);
					});
				}
			}
		}

		public void TestReport_ZACusEntryLinesFallsBackToJE_SystemCreateTimeUtc()
		{
			var clusterKey = 1;
			var eta = new DateTime(2023, 09, 23);
			var assessmentDate = new DateTime(2023, 09, 24);
			var entryReleaseDate = new DateTime(2023, 09, 25);
			var entrySubmittedDate = new DateTime(2023, 09, 26);
			var createTime = new DateTime(2023, 09, 28, 10, 05, 01);

			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001000", "IMP", "SEA", "Calypso", "0308", eta, clusterKey, createTime: createTime);
			var instructionPK = TestDataCreator.CreateCusEntryInstruction(declarationPK, "A01", string.Empty, assessmentDate, clusterKey);
			var entryPK = TestDataCreator.CreateCusEntryHeader(isValid: true, "IMP", "ACK", "1", 1, declarationPK, entrySubmittedDate, entryReleaseDate, instructionPK, createTime, clusterKey);
			var entryLinePK = CreateCusEntryLineWithCreateTime(entryPK, clusterKey, 1, 308, "8.8.8.1", createTime);
			var entryNumPK = TestDataCreator.CreateCusEntryNum(entryPK, "CusEntryHeader", "MRN-1", "MRN", "ZA");
			var invoicePK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, isGroupInvoice: false, clusterKey, supplierPK, string.Empty, importerPK);
			var invoiceLinePK = TestDataCreator.CreateJobComInvoiceLine(invoicePK, clusterKey, 11, 22, 33, "U1", "V1", "W1", "Order-1", "PartNo-1", "PT", 1, entryLinePK);

			var entryLine2PK = CreateCusEntryLineWithNullCreateTime(entryPK, clusterKey, 2, 308, "8.8.8.1");
			var invoice2LinePK = TestDataCreator.CreateJobComInvoiceLine(invoicePK, clusterKey, 11, 22, 33, "U2", "V2", "W2", "Order-2", "PartNo-2", "PT", 1, entryLine2PK);

			var results = new List<string>();
			var sql = $@"SELECT * FROM Report_ZACustomsEntryLines ('{companyPK}', 'IMP', NULL, NULL, '2023-09-20', '2023-09-30', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)";

			CargoWise.Data.Db.Connection.ExecuteReader(sql, reader =>
			{
				results.Add($"{reader["JobNumber"]}, {reader["EntryNumber"]}, {reader["EntryLineNo"]}");
			});
			AssertContainsExactElementsInAnyOrder("If CL_SystemCreateTimeUtc is null, we should look at JE_SystemCreateTimeUtc", new[] { "B00001000, MRN-1, 1", "B00001000, MRN-1, 2" }, results);
		}

		protected override void SetUp()
		{
			base.SetUp();
			companyPK = TestDataCreator.CreateCompany("DZ1", "ZA", "ZAR");
			branchPK = TestDataCreator.CreateBranch(companyPK, "GB1", "");
			importerPK = TestDataCreator.CreateOrganisation("JEIMPORTER", "JE IMPORTER NAME");
			supplierPK = TestDataCreator.CreateOrganisation("JESUPPLIER", "JE SUPPLIER NAME");
		}

		Guid CreateCusEntryLineWithNullCreateTime(Guid entryHeaderPK, int clusterKey, int lineNumber, decimal customsValue, string adValoremTariff)
		{
			TestConnection.ExecuteNonQuery("ALTER TABLE CusEntryLine DISABLE TRIGGER TG_CusEntryLine_AuditDetailsAreNotMissing_Insert");
			var result = Guid.NewGuid();

			var sql = @"
INSERT INTO CusEntryLine (
	[CL_PK],
	[CL_DataModel],
	[CL_CH],
	[CL_ClusterKey],
	[CL_LineNumber],
	[CL_CustomsValue],
	[CL_AdValoremTariff],
	[CL_SystemCreateTimeUtc],
	[CL_SystemCreateUser],
	[CL_SystemLastEditTimeUtc],
	[CL_SystemLastEditUser])
VALUES (
	@clPk,
	'!!',
	@clCh,
	@clusterKey,
	@lineNumber,
	@customsValue,
	@adValoremTariff,
	null,
	'~BP',
	GetUtcDate(),
	'~BP')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@clPk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@clCh", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@lineNumber", SqlDbType.Int, lineNumber);
				command.AddParameter("@customsValue", SqlDbType.Decimal, customsValue);
				command.AddParameter("@adValoremTariff", SqlDbType.VarChar, CusEntryLineSchema.CL_AdValoremTariff.MaxLength, adValoremTariff);
				_ = command.ExecuteNonQuery();
			}

			TestConnection.ExecuteNonQuery("ALTER TABLE CusEntryLine ENABLE TRIGGER TG_CusEntryLine_AuditDetailsAreNotMissing_Insert");

			return result;
		}

		Guid CreateCusEntryLineWithCreateTime(Guid entryHeaderPK, int clusterKey, int lineNumber, decimal customsValue, string adValoremTariff, DateTime createTime)
		{
			TestConnection.ExecuteNonQuery("ALTER TABLE CusEntryLine DISABLE TRIGGER TG_CusEntryLine_AuditDetailsAreNotMissing_Insert");
			var result = Guid.NewGuid();

			var sql = @"
INSERT INTO CusEntryLine (
	[CL_PK],
	[CL_DataModel],
	[CL_CH],
	[CL_ClusterKey],
	[CL_LineNumber],
	[CL_CustomsValue],
	[CL_AdValoremTariff],
	[CL_SystemCreateTimeUtc],
	[CL_SystemCreateUser],
	[CL_SystemLastEditTimeUtc],
	[CL_SystemLastEditUser])
VALUES (
	@clPk,
	'!!',
	@clCh,
	@clusterKey,
	@lineNumber,
	@customsValue,
	@adValoremTariff,
	@createTime,
	'~BP',
	GetUtcDate(),
	'~BP')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@clPk", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@clCh", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@lineNumber", SqlDbType.Int, lineNumber);
				command.AddParameter("@customsValue", SqlDbType.Decimal, customsValue);
				command.AddParameter("@adValoremTariff", SqlDbType.VarChar, CusEntryLineSchema.CL_AdValoremTariff.MaxLength, adValoremTariff);
				command.AddParameter("@createTime", SqlDbType.DateTime, createTime);
				_ = command.ExecuteNonQuery();
			}

			TestConnection.ExecuteNonQuery("ALTER TABLE CusEntryLine ENABLE TRIGGER TG_CusEntryLine_AuditDetailsAreNotMissing_Insert");

			return result;
		}

		Guid companyPK;
		Guid branchPK;
		Guid importerPK;
		Guid supplierPK;
	}
}
