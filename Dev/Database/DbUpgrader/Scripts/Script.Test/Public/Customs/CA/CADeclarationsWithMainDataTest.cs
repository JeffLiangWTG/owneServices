using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.CA;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.CA
{
	[TestedType(typeof(CADeclarationsWithMainData))]
	class CADeclarationsWithMainDataTest : DbCreateScriptTest
	{
		public void TestJE_ClusterKey()
		{
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IM2", 1, dataModel: "CA");

			var clusterKey = 0;
			Db.Connection.ExecuteReader($@"SELECT JE_ClusterKey FROM CADeclarationsWithMainData('{companyPK}')", reader =>
			{
				clusterKey = reader.GetInt32(0);
			});

			AssertEquals(1, clusterKey);
		}

		public void TestSupplierDocumentaryAddress()
		{
			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IM2", 1, dataModel: "CA");
			var organisationPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var addressPK = TestDataCreator.CreateAddress(organisationPK, "SSSS", "Supplier Address", "Address 2", "Vancouver", "BC", "L8K 0A1", "Supplier Toronto");
			TestDataCreator.CreateDocAddress(addressPK, "", declarationPK, "JE", "SUD");

			var mainSupplierCode = string.Empty;
			var mainSupplierFullName = string.Empty;
			var mainSupplierPK = Guid.Empty;
			Db.Connection.ExecuteReader($@"SELECT MainSupplierCode, MainSupplierFullName, MainSupplierPK FROM CADeclarationsWithMainData('{companyPK}')", reader =>
			{
				mainSupplierCode = reader.GetString(0);
				mainSupplierFullName = reader.GetString(1);
				mainSupplierPK = reader.GetGuid(2);
			});

			CombineAssertions(() =>
			{
				AssertEquals("MainSupplierCode", "OrgCode1", mainSupplierCode);
				AssertEquals("MainSupplierFullName", "Supplier Toronto", mainSupplierFullName);
				AssertEquals("MainSupplierPK", organisationPK, mainSupplierPK);
			});
		}

		public void TestImporterOfRecordAddress()
		{
			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IM2", 1, dataModel: "CA");
			var organisationPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var addressPK = TestDataCreator.CreateAddress(organisationPK, "IIII", "Importer Address", "Address 2", "Vancouver", "BC", "L8K 0A1", "Importer Toronto");
			TestDataCreator.CreateDocAddress(addressPK, "", declarationPK, "JE", "IMR");

			var importerOfRecordCode = string.Empty;
			var importerOfRecordFullName = string.Empty;
			var importerOfRecordPK = Guid.Empty;
			Db.Connection.ExecuteReader($@"SELECT ImporterOfRecordCode, ImporterOfRecordFullName, ImporterOfRecordPK FROM CADeclarationsWithMainData('{companyPK}')", reader =>
			{
				importerOfRecordCode = reader.GetString(0);
				importerOfRecordFullName = reader.GetString(1);
				importerOfRecordPK = reader.GetGuid(2);
			});

			CombineAssertions(() =>
			{
				AssertEquals("ImporterOfRecordCode", "OrgCode1", importerOfRecordCode);
				AssertEquals("ImporterOfRecordFullName", "Importer Toronto", importerOfRecordFullName);
				AssertEquals("ImporterOfRecordPK", organisationPK, importerOfRecordPK);
			});
		}

		public void TestLVXFields()
		{
			var mainDeclaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "LVS", 1, dataModel: "CA");
			var subDeclaration1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0002", "LVX", 2, dataModel: "CA");
			var subDeclaration2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0003", "LVX", 3, dataModel: "CA");
			var entryPK = TestDataCreator.CreateCusEntryNum(mainDeclaration, "JobDeclaration", "TRN00001", "REL", "CUS", "CA");

			var subInvoiceHeader1 = TestDataCreator.CreateJobComInvoiceHeader(subDeclaration1, false, 2, valuationDateOverride: new DateTime(2020, 12, 09), dataModel: "CA");
			var subInvoiceHeader2 = TestDataCreator.CreateJobComInvoiceHeader(subDeclaration2, false, 3, addInfo: "OtherReference=TestOtherReference", dataModel: "CA");

			var createGenPivots = @"
INSERT INTO dbo.GenPivot (XX_PK, XX_Relation1ID, XX_Relation1TableCode, XX_Relation2ID, XX_Relation2TableCode, XX_RelationType, XX_Sequence)
VALUES (NEWID(), @JZ_PK1, 'JZ', @JE_PK1, 'JE', 'ZE', 0)
";
			using (var command2 = Db.Connection.Command(createGenPivots))
			{
				command2.AddParameter("@JZ_PK1", SqlDbType.UniqueIdentifier, subInvoiceHeader1);
				command2.AddParameter("@JE_PK1", SqlDbType.UniqueIdentifier, mainDeclaration);
				command2.ExecuteNonQuery();
			}

			var reportSql = @"SELECT JE_PK, LVXDirectShipmentDate, LVXOtherReferenceData, TransactionNumber FROM CADeclarationsWithMainData(@companyPK)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					var shipDates = new Dictionary<Guid, string>();
					var otherRefs = new Dictionary<Guid, string>();
					var trnNums = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						shipDates[reader.GetGuid(0)] = reader.GetValue(1)?.ToString();
						otherRefs[reader.GetGuid(0)] = reader.GetValue(2)?.ToString();
						trnNums[reader.GetGuid(0)] = reader.GetValue(3)?.ToString();
					}
					AssertEquals(3, shipDates.Count);
					AssertEquals("", shipDates[mainDeclaration]);
					AssertEquals("9/12/2020 12:00:00 AM", shipDates[subDeclaration1]);
					AssertEquals("", shipDates[subDeclaration2]);

					AssertEquals("", otherRefs[mainDeclaration]);
					AssertEquals("", otherRefs[subDeclaration1]);
					AssertEquals("TestOtherReference", otherRefs[subDeclaration2]);

					AssertEquals("TRN00001", trnNums[mainDeclaration]);
					AssertEquals("TRN00001", trnNums[subDeclaration1]);
					AssertEquals("", trnNums[subDeclaration2]);
				}
			}
		}

		public void TestDeclarationsWithNoB3SubmissionDate()
		{
			var mainDeclaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "LVS", 1, dataModel: "CA");
			var subDeclaration1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0002", "LVX", 2, dataModel: "CA");
			var subDeclaration2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0003", "LVX", 3, dataModel: "CA");

			var subInvoiceHeader1 = TestDataCreator.CreateJobComInvoiceHeader(subDeclaration1, false, 2, dataModel: "CA");
			var subInvoiceHeader2 = TestDataCreator.CreateJobComInvoiceHeader(subDeclaration2, false, 3, dataModel: "CA");

			var mainEntryHeader = Guid.NewGuid();
			var subEntryHeader1 = Guid.NewGuid();
			var subEntryHeader2 = Guid.NewGuid();
			var createEntryHeaders = @"
INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_EntryStatus, CH_IsValid, CH_JE, CH_MessageType, CH_Status, CH_EntrySubmittedDate, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
VALUES 
(@CH_PK1, 'CA', 'CLR', 1, @CH_JE1, 'B3C', 'CLO', '2020-06-19 21:05:00', 1, getutcdate(), '~BP', getutcdate(), '~BP'),
(@CH_PK2, 'CA', '', 1, @CH_JE2, 'B3C', '', null, 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(@CH_PK3, 'CA', '', 1, @CH_JE3, 'B3C', '', null, 3, getutcdate(), '~BP', getutcdate(), '~BP')
";
			var createGenPivots = @"
INSERT INTO dbo.GenPivot (XX_PK, XX_Relation1ID, XX_Relation1TableCode, XX_Relation2ID, XX_Relation2TableCode, XX_RelationType, XX_Sequence)
VALUES (NEWID(), @JZ_PK1, 'JZ', @JE_PK1, 'JE', 'ZE', 0)
";
			using (var command1 = Db.Connection.Command(createEntryHeaders))
			using (var command2 = Db.Connection.Command(createGenPivots))
			{
				command1.AddParameter("@CH_PK1", SqlDbType.UniqueIdentifier, mainEntryHeader);
				command1.AddParameter("@CH_PK2", SqlDbType.UniqueIdentifier, subEntryHeader1);
				command1.AddParameter("@CH_PK3", SqlDbType.UniqueIdentifier, subEntryHeader2);

				command1.AddParameter("@CH_JE1", SqlDbType.UniqueIdentifier, mainDeclaration);
				command1.AddParameter("@CH_JE2", SqlDbType.UniqueIdentifier, subDeclaration1);
				command1.AddParameter("@CH_JE3", SqlDbType.UniqueIdentifier, subDeclaration2);

				command2.AddParameter("@JZ_PK1", SqlDbType.UniqueIdentifier, subInvoiceHeader1);
				command2.AddParameter("@JE_PK1", SqlDbType.UniqueIdentifier, mainDeclaration);

				command1.ExecuteNonQuery();
				command2.ExecuteNonQuery();
			}

			var reportSql = @"SELECT JE_PK, CA_NoB3SubmissionDate, B3SubmissionDate FROM CADeclarationsWithMainData(@companyPK)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					var flags = new Dictionary<Guid, string>();
					var dates = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						flags[reader.GetGuid(0)] = reader.GetString(1);
						dates[reader.GetGuid(0)] = reader.GetValue(2)?.ToString();
					}
					AssertEquals(3, flags.Count);
					AssertEquals("N", flags[mainDeclaration]);
					AssertEquals("N", flags[subDeclaration1]);
					AssertEquals("Y", flags[subDeclaration2]);

					AssertEquals("19/06/2020 9:05:00 PM", dates[mainDeclaration]);
					AssertEquals("19/06/2020 9:05:00 PM", dates[subDeclaration1]);
					AssertEquals(string.Empty, dates[subDeclaration2]);
				}
			}
		}

		public void TestAccountingAge()
		{
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "CA", addInfo: "AccountingAge=10");

			var reportSql = @"SELECT AccountingAge FROM CADeclarationsWithMainData(@companyPK)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(10, reader.GetInt32(0));
				}
			}
		}

		public void TestCarrier()
		{
			CreateTestRefDataGroupingCA();
			TestDataCreator.CreateRefDbEntZZRefCarrier("A000", "A000 TEST CARRIER", "CA", "CARGOCARRIER");
			var impDecPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "CA", carrierCode: "A000");
			var im2DecPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0002", "IM2", 2, dataModel: "CA", carrierCode: "A000");
			var lvxPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0003", "LVX", 3, dataModel: "CA");

			var lvxHeaderPK = TestDataCreator.CreateJobComInvoiceHeader(lvxPK, false, 3, addInfo: "CarrierCode=A000", dataModel: "CA");

			var reportSql = @"SELECT CarrierCode, CarrierName FROM CADeclarationsWithMainData(@companyPK)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					var resultCount = 0;
					while (reader.Read())
					{
						AssertEquals("A000", reader.GetString(0));
						AssertEquals("A000 TEST CARRIER", reader.GetString(1));
						resultCount++;
					}
					AssertEquals(3, resultCount);
				}
			}
		}

		public void TestCarrierName()
		{
			CreateTestRefDataGroupingCA();
			TestDataCreator.CreateRefDbEntZZRefCarrier("A001", "NA001", "CA", "CARGOCARRIER");
			TestDataCreator.CreateRefDbEntZZRefCarrier("A002", "NA002", "CA", "CARGOCARRIER");

			var dec1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "CA");

			var dec2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0002", "IMP", 2, dataModel: "CA", carrierCode: "A001");

			var dec3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0003", "LVX", 3, dataModel: "CA");
			var invoice31 = TestDataCreator.CreateJobComInvoiceHeader(dec3, false, 3, addInfo: "CarrierCode=A002", dataModel: "CA");

			var dec4 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0004", "LVX", 4, dataModel: "CA", carrierCode: "A001");
			var invoice41 = TestDataCreator.CreateJobComInvoiceHeader(dec4, false, 4, addInfo: "CarrierCode=A002", dataModel: "CA");

			var dec5 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0005", "IMP", 5, dataModel: "CA");
			var invoice51 = TestDataCreator.CreateJobComInvoiceHeader(dec5, false, 5, addInfo: "CarrierCode=A002", dataModel: "CA");

			var reportSql = @"SELECT CarrierName FROM CADeclarationsWithMainData(@companyPK) ORDER BY JE_ClusterKey ASC";
			var carrierNames = new List<string>();
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						carrierNames.Add(reader.GetString(0));
					}
				}
			}
			AssertArrayEqualsByElements(new string[] { "", "NA001", "NA002", "NA001", "" }, carrierNames.ToArray());
		}

		void CreateTestRefDataGroupingCA()
		{
			if (!Db.Connection.Exists("FROM dbo.RefDatabase_RefDataGrouping WHERE ZZZ_DataGrouping = 'CA'"))
			{
				TestDataCreator.CreateRefDatabaseRefDataGrouping("CA", "Canada");
			}
		}

		public void TestCADeclarationsWithMainData_IM2AndB3X()
		{
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IM2", 1, dataModel: "CA");
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0002", "B3X", 2, dataModel: "CA");

			var reportSql = @"SELECT COUNT(*) FROM CADeclarationsWithMainData(@companyPK) WHERE JE_MessageType = @JE_MessageType";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@JE_MessageType", SqlDbType.VarChar, "IM2");

				using (var reader = command.ExecuteReader())
				{
					var count = 0;
					while (reader.Read())
					{
						count = (int)reader.GetValue(0);
					}

					AssertEquals(1, count);
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@JE_MessageType", SqlDbType.VarChar, "B3X");

				using (var reader = command.ExecuteReader())
				{
					var count = 0;
					while (reader.Read())
					{
						count = (int)reader.GetValue(0);
					}

					AssertEquals(1, count);
				}
			}
		}

		public void TestAuditData()
		{
			AssertNoExceptionThrown(() =>
			{
				var reportSql = @"SELECT JE_AuditDateUtc, JE_AuditReference, JE_GS_NKAuditUser FROM CADeclarationsWithMainData(@companyPK)";

				using (var command = Db.Connection.Command(reportSql))
				{
					command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
					command.ExecuteNonQuery();
				}
			});
		}

		public void TestSearchByCSAReleaseOnlyFlag()
		{
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, addInfo: "CSAEntry=N", dataModel: "CA");
			var pk = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0002", "IMP", 2, addInfo: "CSAEntry=Y", dataModel: "CA");
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0003", "IMP", 3, dataModel: "CA");

			var dt = new DataTable();
			using (var command = Db.Connection.Command($@"SELECT JE_PK FROM CADeclarationsWithMainData('{companyPK}') WHERE CSAReleaseOnlyFlag = 'Y'"))
			using (var reader = command.ExecuteReader())
			{
				dt.Load(reader);
			}

			CombineAssertions(() =>
			{
				AssertEquals("Number of CSA records", 1, dt.Rows.Count);
				AssertEquals("CSA records found", pk, dt.Rows[0].Field<Guid>(0));
			});
		}

		public void TestBondInfo()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "CA", "CAD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "CABLO");
			var declaraction = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0001", "IMP", 1, dataModel: "CA", addInfo: "BondType=9*BondNo=12122*SuretyCode=ABC");

			var reportSql = @"SELECT JE_BondType,JE_BondNo,JE_SuretyCode FROM CADeclarationsWithMainData(@companyPK) WHERE JE_PK = @JE_PK";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@JE_PK", SqlDbType.UniqueIdentifier, declaraction);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						AssertEquals("9", reader["JE_BondType"].ToString());
						AssertEquals("12122", reader["JE_BondNo"].ToString());
						AssertEquals("ABC", reader["JE_SuretyCode"].ToString());
					}
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			companyPK = TestDataCreator.CreateCompany("CAN", "CA", "CAD");
			branchPK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_RL_NKHomePort, GB_GC) VALUES (@branchPK, 'BLO', 'CABLO', @companyPK)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}
		}

		Guid companyPK;
		Guid branchPK;
	}
}
