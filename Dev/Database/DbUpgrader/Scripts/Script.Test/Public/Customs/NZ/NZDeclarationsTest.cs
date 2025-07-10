using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.NZ;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.NZ
{
	[TestedType(typeof(NZDeclarations))]
	class NZDeclarationsTest : DbCreateScriptTest
	{
		struct resultData
		{
			public DateTime DateOfArrival;
			public DateTime EntrySubmittedDate;
			public string EntryNumber;
			public string MasterBills;
			public string HouseBills;
			public string ImporterName;
			public string SupplierName;
			public string OriginCountryCode;
			public string ProcessingPort;
			public Guid CusAgentPK;
		}

		public void TestFindDeclarationsByEntryFirstSubmittedDate()
		{
			PopulateReportData("B0001001", "29292051", "OBL1", "HBL1", 1, dateOfArrival: new DateTime(2022, 03, 16), entrySubmittedDate: new DateTime(2022, 03, 16));
			PopulateReportData("B0001002", "29292052", "OBL2", "HBL2", 2, dateOfArrival: new DateTime(2022, 03, 16), entrySubmittedDate: new DateTime(2022, 03, 15));
			PopulateReportData("B0001003", "29292053", "OBL3", "HBL3", 3, dateOfArrival: new DateTime(2022, 03, 15), entrySubmittedDate: new DateTime(2022, 03, 16));
			PopulateReportData("B0001004", "29292054", "OBL4", "HBL4", 4, dateOfArrival: new DateTime(2022, 03, 18), entrySubmittedDate: new DateTime(2022, 03, 19));

			var reportSql = $@"
				SELECT JE_DeclarationReference, JE_DateOfArrival, JE_EntrySubmittedDate, EntryNumber, MasterBills, HouseBills, ImporterName, SupplierName, OriginCountryCode, JE_RL_NKProcessingPort, GS_PK
				FROM NZDeclarations(@CurrentCompany, @JobCreatedDateFrom, @JobCreatedDateTo, @ArrivalDischargeDateFrom, @ArrivalDischargeDateTo, @Importer, @Supplier,
					@EntryFirstSubmittedDateFrom, @EntryFirstSubmittedDateTo, @PortDischarge, @PortLoading, @EntryStyle, @EntryStatus)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@CurrentCompany", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@JobCreatedDateFrom", SqlDbType.DateTime, DBNull.Value);
				command.AddParameter("@JobCreatedDateTo", SqlDbType.DateTime, DBNull.Value);
				command.AddParameter("@ArrivalDischargeDateFrom", SqlDbType.DateTime, DBNull.Value);
				command.AddParameter("@ArrivalDischargeDateTo", SqlDbType.DateTime, DBNull.Value);
				command.AddParameter("@Importer", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@Supplier", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@EntryFirstSubmittedDateFrom", SqlDbType.DateTime, new DateTime(2022, 3, 16));
				command.AddParameter("@EntryFirstSubmittedDateTo", SqlDbType.DateTime, new DateTime(2022, 3, 18));
				command.AddParameter("@PortDischarge", SqlDbType.VarChar, "");
				command.AddParameter("@PortLoading", SqlDbType.VarChar, "");
				command.AddParameter("@EntryStyle", SqlDbType.VarChar, "");
				command.AddParameter("@EntryStatus", SqlDbType.VarChar, "");

				var data = new Dictionary<string, resultData>();
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						data.Add((string)reader["JE_DeclarationReference"],
							new resultData()
							{
								DateOfArrival = (DateTime)reader["JE_DateOfArrival"],
								EntrySubmittedDate = (DateTime)reader["JE_EntrySubmittedDate"],
								EntryNumber = (string)reader["EntryNumber"],
								MasterBills = (string)reader["MasterBills"],
								HouseBills = (string)reader["HouseBills"],
								ImporterName = (string)reader["ImporterName"],
								SupplierName = (string)reader["SupplierName"],
								OriginCountryCode = (string)reader["OriginCountryCode"],
								ProcessingPort = (string)reader["JE_RL_NKProcessingPort"],
								CusAgentPK = (Guid)reader["GS_PK"],
							});
					}
				}

				CombineAssertions(() =>
				{
					var b0001001 = data["B0001001"];
					AssertEquals("DateOfArrival", "2022-03-16 00:00:00Z", b0001001.DateOfArrival.ToString("u"));
					AssertEquals("EntrySubmittedDate", "2022-03-16 00:00:00Z", b0001001.EntrySubmittedDate.ToString("u"));
					AssertEquals("EntryNumber", "29292051", b0001001.EntryNumber);
					AssertEquals("MasterBills", "OBL1", b0001001.MasterBills);
					AssertEquals("HouseBills", "HBL1", b0001001.HouseBills);
					AssertEquals("ImporterName", "Test Importer Org", b0001001.ImporterName);
					AssertEquals("SupplierName", "Test Supplier Org", b0001001.SupplierName);
					AssertEquals("OriginCountryCode", "AU", b0001001.OriginCountryCode);
					AssertEquals("ProcessingPort", "NZAKL", b0001001.ProcessingPort);
					AssertEquals("CusAgentPK", brokerPK, b0001001.CusAgentPK);

					var b0001003 = data["B0001003"];
					AssertEquals("DateOfArrival", "2022-03-15 00:00:00Z", b0001003.DateOfArrival.ToString("u"));
					AssertEquals("EntrySubmittedDate", "2022-03-16 00:00:00Z", b0001003.EntrySubmittedDate.ToString("u"));
					AssertEquals("EntryNumber", "29292053", b0001003.EntryNumber);
					AssertEquals("MasterBills", "OBL3", b0001003.MasterBills);
					AssertEquals("HouseBills", "HBL3", b0001003.HouseBills);
					AssertEquals("ImporterName", "Test Importer Org", b0001003.ImporterName);
					AssertEquals("SupplierName", "Test Supplier Org", b0001003.SupplierName);
					AssertEquals("OriginCountryCode", "AU", b0001003.OriginCountryCode);
					AssertEquals("ProcessingPort", "NZAKL", b0001003.ProcessingPort);
					AssertEquals("CusAgentPK", brokerPK, b0001003.CusAgentPK);

					AssertEquals("B0001002 and B0001004 do not appear because they are out of range", 2, data.Count);
				});
			}
		}

		void PopulateReportData(string declarationReference, string entryNumber, string masterBillNumber, string houseBillNumber, int clusterKey, DateTime dateOfArrival, DateTime entrySubmittedDate)
		{
			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, declarationReference, "IMP", clusterKey,
				addInfo: "RL_NKProcessingPort=NZAKL", brokerCode: "SB1", importerPK: importerPK, supplierPK: supplierPK);
			TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK, clusterKey, billType: "MB", billNum: masterBillNumber);
			TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK, clusterKey, billType: "HB", billNum: houseBillNumber);

			var entryHeaderPK = TestDataCreator.CreateCusEntryHeader(jePk: declarationPK, bgmReference: "B0001000", messageType: "IMP",
				entrySubmittedDate: null, entryReleaseDate: null,
				linenum: 1, totalPaid: 470.33f, addInfo: "IsActive=Y", status: "", entryStatus: "", isValid: true, clusterKey: clusterKey,
				instruction: Guid.Empty, warehouseTransactionStatus: "", warehouseReleaseDate: null, bondAcquittedDate: null, bondValidToDate: null
				);
			TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", entryNumber, "FRM", "CUS", "NZ");

			var updateSql = $@"
				UPDATE dbo.JobDeclaration
				SET
					JE_RL_NKOrigin = 'AUSYD',
					JE_RL_NKPortOfArrival = 'NZAKL',
					JE_RL_NKPortOfLoading = 'AUSYD',
					JE_MessageSubType = 'NOR',
					JE_EntryStatus = 'ERR',
					JE_IsCancelled = 0,
					JE_DateOfArrival = @dateOfArrival,
					JE_EntrySubmittedDate = @entrySubmittedDate,
					JE_SystemLastEditTimeUtc = GETUTCDATE(),
					JE_SystemLastEditUser = '~BP'
				WHERE JE_PK = @declarationPK";

			using (DbCommand command = Db.Connection.Command(updateSql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@dateOfArrival", SqlDbType.DateTime, dateOfArrival);
				command.AddParameter("@entrySubmittedDate", SqlDbType.DateTime, entrySubmittedDate);
				command.ExecuteNonQuery();
			}
		}

		Guid companyPK, branchPK, brokerPK, importerPK, supplierPK;

		protected override void SetUp()
		{
			base.SetUp();

			companyPK = TestDataCreator.CreateCompany("TC1", "NZ", "NZD");
			branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "NZBER", "NZAKL");
			brokerPK = TestDataCreator.CreateGlbStaff("SB1", "Broker 1");
			importerPK = TestDataCreator.CreateOrganisation("TOI", "Test Importer Org");
			supplierPK = TestDataCreator.CreateOrganisation("TOS", "Test Supplier Org");
		}
	}
}

