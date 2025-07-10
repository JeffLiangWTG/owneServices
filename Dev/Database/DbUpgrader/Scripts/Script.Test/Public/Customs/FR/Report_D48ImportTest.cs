using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.FR;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.FR.Testing
{
	[TestedType(typeof(Report_D48Import))]
	class Report_D48ImportTest : DbCreateScriptTest
	{
		Guid companyPK;
		Guid branchPK;
		Guid importerOrgPK;
		Guid supplierOrgPK;
		const string reportSql = @"select CSI_Quantity3,CSI_ParentID,Client,CE_EntryNum,D48EndDate FROM Report_D48Import(@companyPK, @BaeFromDate, @BaeToDate, @WriteOffOnly)";
		protected override void SetUp()
		{
			base.SetUp();
			companyPK = TestDataCreator.CreateCompany("TC1", "FR", "NTD");
			branchPK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_RL_NKHomePort,GB_RN_NKCountryCode, GB_GC) VALUES (@branchPK, 'TB1', 'TAJNB','FR', @companyPK)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}

			importerOrgPK = TestDataCreator.CreateOrganisation("TESTA", "TESTA", "CAYVR");
			supplierOrgPK = TestDataCreator.CreateOrganisation("TESTB", "TESTB", "CAYVR");

			TestDataCreator.CreateRefDbDataGroupingCodeTypeAndListAttributeItems("FR", new string[] { "DC44I", "DC44E" }, ("0001", "statut juridique", "IsD48", "Y"));
			entryStatus = "100";
		}

		public void TestCompanyPK()
		{
			var declarationPK = CreateTestJobDeclarationuUseJE_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 6, 13), 1, 1m);
			var invoiceLinePK = CreateTestJobDeclarationUseJI_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 7, 13), 2, 1m);
			var invoiceHeaderPK = CreateTestJobDeclarationUseJZ_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 8, 13), 3, 1m);

			AssertD48HasSingleDataOnly(companyPK, new DateTime(2020, 6, 12), new DateTime(2020, 6, 13), 10m, declarationPK, "TESTA", "5525145", new DateTime(2021, 4, 13), "BOTH");
			AssertD48DoNotHaveData(branchPK, new DateTime(2020, 6, 12), new DateTime(2020, 6, 13), "BOTH");
		}

		public void TestBaeDate()
		{
			var declarationPK = CreateTestJobDeclarationuUseJE_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 6, 13), 1, 1m);
			var invoiceLinePK = CreateTestJobDeclarationUseJI_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 7, 13), 2, 1m);
			var invoiceHeaderPK = CreateTestJobDeclarationUseJZ_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 8, 13), 3, 1m);

			AssertD48HasSingleDataOnly(companyPK, new DateTime(2020, 6, 12), new DateTime(2020, 6, 13), 10m, declarationPK, "TESTA", "5525145", new DateTime(2021, 4, 13), "BOTH");
			AssertD48HasSingleDataOnly(companyPK, new DateTime(2020, 7, 13), new DateTime(2020, 7, 14), 10m, invoiceLinePK, "TESTA", "5525145", new DateTime(2021, 5, 13), "BOTH");
			AssertD48HasSingleDataOnly(companyPK, new DateTime(2020, 8, 13), new DateTime(2020, 8, 13), 10m, invoiceHeaderPK, "TESTA", "5525145", new DateTime(2021, 6, 13), "BOTH");
			AssertD48DoNotHaveData(companyPK, new DateTime(2020, 6, 14), new DateTime(2020, 7, 12), "BOTH");
			AssertD48DoNotHaveData(companyPK, new DateTime(2020, 7, 14), new DateTime(2020, 8, 12), "BOTH");
			AssertD48DoNotHaveData(companyPK, new DateTime(1986, 6, 12), new DateTime(2020, 6, 12), "BOTH");
			AssertD48DoNotHaveData(companyPK, new DateTime(2020, 8, 14), new DateTime(2050, 8, 14), "BOTH");
		}

		public void TestMessageType()
		{
			var declarationPK = CreateTestJobDeclarationuUseJE_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 7, 13), 1, 1m);
			var invoiceLinePK = CreateTestJobDeclarationUseJI_PKJoinSupportingInfo("EXP", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 8, 13), 2, 1m);
			var invoiceHeaderPK = CreateTestJobDeclarationUseJZ_PKJoinSupportingInfo("MSC", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 9, 13), 3, 1m);

			AssertD48HasSingleDataOnly(companyPK, new DateTime(2020, 7, 12), new DateTime(2020, 7, 13), 10m, declarationPK, "TESTA", "5525145", new DateTime(2021, 5, 13), "BOTH");
			AssertD48HasSingleDataOnly(companyPK, new DateTime(2020, 8, 12), new DateTime(2020, 8, 13), 10m, invoiceLinePK, "TESTB", "5525145", new DateTime(2021, 6, 13), "BOTH");
			AssertD48DoNotHaveData(companyPK, new DateTime(2020, 9, 12), new DateTime(2020, 9, 13), "BOTH");
		}

		public void TestCsiTypeCondition()
		{
			var declarationPK = CreateTestJobDeclarationuUseJE_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 7, 13), 1, 1m);
			var invoiceLinePK = CreateTestJobDeclarationUseJI_PKJoinSupportingInfo("IMP", "PRD", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 7, 13), 2, 1m);
			var invoiceHeaderPK = CreateTestJobDeclarationUseJZ_PKJoinSupportingInfo("IMP", "PQD", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 7, 13), 3, 1m);

			AssertD48HasSingleDataOnly(companyPK, new DateTime(2020, 7, 12), new DateTime(2020, 7, 13), 10m, declarationPK, "TESTA", "5525145", new DateTime(2021, 5, 13), "BOTH");
		}

		public void TestD48IsExistedCondition()
		{
			var declarationPK = CreateTestJobDeclarationuUseJE_PKJoinSupportingInfo("IMP", "SUP", "2044", new DateTime(2020, 7, 13), 0m, "CES", "100", "5525145", 1, new DateTime(2020, 7, 13), 1, 1m);
			var invoiceLinePK = CreateTestJobDeclarationUseJI_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 8, 13), 2, 1m);
			var invoiceHeaderPK = CreateTestJobDeclarationUseJZ_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 5m, "CES", "100", "5525145", 1, new DateTime(2020, 9, 13), 3, 1m);
			var invoiceLinePK2 = CreateTestJobDeclarationUseJI_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 10, 13), 4, 0m);
			var invoiceHeaderPK2 = CreateTestJobDeclarationUseJZ_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 5m, "CES", "100", "5525145", 1, new DateTime(2020, 11, 13), 5, 0m);

			AssertD48DoNotHaveData(companyPK, new DateTime(2020, 7, 12), new DateTime(2020, 7, 13), "BOTH");
			AssertD48HasSingleDataOnly(companyPK, new DateTime(2020, 8, 12), new DateTime(2020, 8, 13), 10m, invoiceLinePK, "TESTA", "5525145", new DateTime(2021, 6, 13), "BOTH");
			AssertD48HasSingleDataOnly(companyPK, new DateTime(2020, 9, 12), new DateTime(2020, 9, 13), 5m, invoiceHeaderPK, "TESTA", "5525145", new DateTime(2021, 2, 13), "BOTH");
			AssertD48DoNotHaveData(companyPK, new DateTime(2020, 10, 12), new DateTime(2020, 10, 13), "BOTH");
			AssertD48DoNotHaveData(companyPK, new DateTime(2020, 11, 12), new DateTime(2020, 11, 13), "BOTH");
		}

		public void TestCsiValueCondition()
		{
			var declarationPK = CreateTestJobDeclarationuUseJE_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 7, 13), 1, 1m);
			var invoiceLinePK = CreateTestJobDeclarationUseJI_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 8, 13), 2, 2m);
			var invoiceHeaderPK = CreateTestJobDeclarationUseJZ_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 9, 13), 3, 3m);

			AssertD48HasSingleDataOnly(companyPK, new DateTime(2020, 7, 12), new DateTime(2020, 7, 13), 10m, declarationPK, "TESTA", "5525145", new DateTime(2021, 5, 13), "BOTH");
			AssertD48HasSingleDataOnly(companyPK, new DateTime(2020, 8, 12), new DateTime(2020, 8, 13), 10m, invoiceLinePK, "TESTA", "5525145", new DateTime(2021, 6, 13), "BOTH");
			AssertD48HasSingleDataOnly(companyPK, new DateTime(2020, 9, 12), new DateTime(2020, 9, 13), 10m, invoiceHeaderPK, "TESTA", "5525145", new DateTime(2021, 7, 13), "BOTH");
		}

		public void TestEntryIsSystemGeneratedCondition()
		{
			var declarationPK = CreateTestJobDeclarationuUseJE_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 0, new DateTime(2020, 7, 13), 1, 1m);
			var invoiceLinePK = CreateTestJobDeclarationUseJI_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525146", 1, new DateTime(2020, 8, 13), 2, 1m);
			var invoiceHeaderPK = CreateTestJobDeclarationUseJZ_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525147", 0, new DateTime(2020, 9, 13), 3, 1m);

			AssertD48HasSingleDataOnly(companyPK, new DateTime(2020, 7, 12), new DateTime(2020, 7, 13), 10m, declarationPK, "TESTA", null, new DateTime(2021, 5, 13), "BOTH");
			AssertD48HasSingleDataOnly(companyPK, new DateTime(2020, 8, 12), new DateTime(2020, 8, 13), 10m, invoiceLinePK, "TESTA", "5525146", new DateTime(2021, 6, 13), "BOTH");
			AssertD48HasSingleDataOnly(companyPK, new DateTime(2020, 9, 12), new DateTime(2020, 9, 13), 10m, invoiceHeaderPK, "TESTA", null, new DateTime(2021, 7, 13), "BOTH");
		}

		public void TestD48EndDate()
		{
			var declarationPK = CreateTestJobDeclarationuUseJE_PKJoinSupportingInfo("IMP", "SUP", "0001", null, 0m, "CES", "100", "5525145", 1, new DateTime(2020, 7, 13), 1, 1m);
			var invoiceLinePK = CreateTestJobDeclarationUseJI_PKJoinSupportingInfo("IMP", "SUP", "0001", null, 0m, "CES", "100", "5525145", 1, new DateTime(2020, 8, 13), 2, 2m);
			var invoiceHeaderPK = CreateTestJobDeclarationUseJZ_PKJoinSupportingInfo("IMP", "SUP", "0001", null, 0m, "CES", "100", "5525145", 1, new DateTime(2020, 9, 13), 3, 3m);

			AssertD48HasSingleDataOnly(companyPK, new DateTime(2020, 7, 12), new DateTime(2020, 7, 13), 0m, declarationPK, "TESTA", "5525145", null, "BOTH");
			AssertD48HasSingleDataOnly(companyPK, new DateTime(2020, 8, 12), new DateTime(2020, 8, 13), 0m, invoiceLinePK, "TESTA", "5525145", null, "BOTH");
			AssertD48HasSingleDataOnly(companyPK, new DateTime(2020, 9, 12), new DateTime(2020, 9, 13), 0m, invoiceHeaderPK, "TESTA", "5525145", null, "BOTH");
		}

		public void TestD48WriteOff()
		{
			var declarationPK = CreateTestJobDeclarationuUseJE_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 7, 13), 1, 1m);
			var invoiceLinePK = CreateTestJobDeclarationUseJI_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 0m, "CES", "100", "5525145", 1, new DateTime(2020, 7, 13), 2, 1m);
			var invoiceHeaderPK = CreateTestJobDeclarationUseJZ_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 9, 13), 3, 1m);

			AssertD48HasSingleDataOnly(companyPK, new DateTime(2020, 7, 12), new DateTime(2020, 7, 13), 10m, declarationPK, "TESTA", "5525145", new DateTime(2021, 5, 13), "NO");
			AssertD48HasSingleDataOnly(companyPK, new DateTime(2020, 7, 12), new DateTime(2020, 7, 13), 0m, invoiceLinePK, "TESTA", "5525145", null, "YES");

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@BaeFromDate", SqlDbType.SmallDateTime, new DateTime(2020, 6, 12));
				command.AddParameter("@BaeToDate", SqlDbType.SmallDateTime, new DateTime(2020, 8, 13));
				command.AddParameter("@WriteOffOnly", SqlDbType.VarChar, "BOTH");

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					Assert("There should be another records", reader.Read());
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestCH_EntryStatusConditionBAE()
		{
			AssertCH_EntryStatusCondition("100");
		}

		public void TestCH_EntryStatusConditionVAL()
		{
			AssertCH_EntryStatusCondition("060");
		}

		public void TestCH_EntryStatusConditionBAEComplete()
		{
			AssertCH_EntryStatusCondition("130");
		}

		void AssertCH_EntryStatusCondition(string entryStatus)
		{
			this.entryStatus = entryStatus;
			var declarationPK = CreateTestJobDeclarationuUseJE_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 7, 13), 1, 1m);
			var invoiceLinePK = CreateTestJobDeclarationUseJI_PKJoinSupportingInfo("IMP", "PRD", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 7, 13), 2, 1m);
			var invoiceHeaderPK = CreateTestJobDeclarationUseJZ_PKJoinSupportingInfo("IMP", "PQD", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 7, 13), 3, 1m);

			AssertD48HasSingleDataOnly(companyPK, new DateTime(2020, 7, 12), new DateTime(2020, 7, 13), 10m, declarationPK, "TESTA", "5525145", new DateTime(2021, 5, 13), "BOTH");
		}

		public void AssertD48HasSingleDataOnly(Guid companyPk, DateTime baeFromDate, DateTime baeToDate, decimal expectedQuantity, Guid expectedGuid, string expectedClient, string expectedEntryNumber, DateTime? expectedD48EndDate, string writeOffOnly)
		{
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPk);
				command.AddParameter("@BaeFromDate", SqlDbType.SmallDateTime, baeFromDate);
				command.AddParameter("@BaeToDate", SqlDbType.SmallDateTime, baeToDate);
				command.AddParameter("@WriteOffOnly", SqlDbType.VarChar, writeOffOnly);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(expectedQuantity, reader.GetDecimal(0));
					AssertEquals(expectedGuid, reader.GetGuid(1));

					if (expectedClient == null)
					{
						AssertExceptionThrown<SqlNullValueException>(() => reader.GetString(2));
					}
					else
					{
						AssertEquals(expectedClient, reader.GetString(2));
					}

					if (expectedEntryNumber == null)
					{
						AssertExceptionThrown<SqlNullValueException>(() => reader.GetString(3));
					}
					else
					{
						AssertEquals(expectedEntryNumber, reader.GetString(3));
					}

					if (expectedD48EndDate == null)
					{
						AssertExceptionThrown<SqlNullValueException>(() => reader.GetDateTime(4));
					}
					else
					{
						AssertEquals(expectedD48EndDate.Value, reader.GetDateTime(4));
					}

					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void AssertD48DoNotHaveData(Guid companyPk, DateTime baeFromDate, DateTime baeToDate, string writeOffOnly)
		{
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPk);
				command.AddParameter("@BaeFromDate", SqlDbType.SmallDateTime, baeFromDate);
				command.AddParameter("@BaeToDate", SqlDbType.SmallDateTime, baeToDate);
				command.AddParameter("@WriteOffOnly", SqlDbType.VarChar, writeOffOnly);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		Guid CreateTestJobDeclarationuUseJE_PKJoinSupportingInfo(string messageType, string csiType, string csiCode, DateTime? dateForDuty, decimal csiQuantity3, string slEvent, string slReference, string entryNum, int entryIsSystemGenerated, DateTime baeDate, int clusterKey, decimal csiValue)
		{
			var declarationPK = CreateTestJobDeclaration(messageType, slEvent, slReference, baeDate, dateForDuty, entryNum, entryIsSystemGenerated, "JE", clusterKey);
			CreateCusSupportingInfo(declarationPK, "JE", csiType, csiCode, csiQuantity3, csiValue);
			return declarationPK;
		}

		Guid CreateTestJobDeclarationUseJI_PKJoinSupportingInfo(string messageType, string csiType, string csiCode, DateTime? dateForDuty, decimal csiQuantity3, string slEvent, string slReference, string entryNum, int entryIsSystemGenerated, DateTime baeDate, int clusterKey, decimal csiValue)
		{
			var invoiceLinePK = CreateTestJobDeclaration(messageType, slEvent, slReference, baeDate, dateForDuty, entryNum, entryIsSystemGenerated, "JI", clusterKey);
			CreateCusSupportingInfo(invoiceLinePK, "JI", csiType, csiCode, csiQuantity3, csiValue);
			return invoiceLinePK;
		}

		Guid CreateTestJobDeclarationUseJZ_PKJoinSupportingInfo(string messageType, string csiType, string csiCode, DateTime? dateForDuty, decimal csiQuantity3, string slEvent, string slReference, string entryNum, int entryIsSystemGenerated, DateTime baeDate, int clusterKey, decimal csiValue)
		{
			var invoiceHeaderPK = CreateTestJobDeclaration(messageType, slEvent, slReference, baeDate, dateForDuty, entryNum, entryIsSystemGenerated, "JZ", clusterKey);
			CreateCusSupportingInfo(invoiceHeaderPK, "JZ", csiType, csiCode, csiQuantity3, csiValue);
			return invoiceHeaderPK;
		}

		Guid CreateTestJobDeclaration(string messageType, string slEvent, string slReference, DateTime baeDate, DateTime? dateForDuty, string entryNum, int entryIsSystemGenerated, string joinWay, int clusterKey)
		{
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, importerOrgPK, supplierOrgPK, messageType, clusterKey);
			var entryInstructionPK = CreateCusEntryInstruction(declarationPK, clusterKey, dateForDuty);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, entryInstructionPK, clusterKey, baeDate);
			var entryLinePK = CreateCusEntryLine(entryHeaderPK, clusterKey);
			var invoiceHeaderPK = CreateInvoiceHeader(declarationPK, clusterKey);
			var invoiceLinePK = CreateInvoiceLine(invoiceHeaderPK, entryInstructionPK, entryLinePK, clusterKey);
			CreateCusEntryNum(entryHeaderPK, messageType, entryNum, entryIsSystemGenerated);

			if (null == dateForDuty)
			{
				AssertCusEntryInstructionHasNullDateForDuty(entryInstructionPK);
			}

			if (joinWay == "JZ")
			{
				return invoiceHeaderPK;
			}
			else if (joinWay == "JI")
			{
				return invoiceLinePK;
			}
			else
			{
				return declarationPK;
			}
		}

		void AssertCusEntryInstructionHasNullDateForDuty(Guid entryInstructionPK)
		{
			var entryInstructionSql = string.Format(@" select CEI_PK, CEI_DateForDuty from dbo.CusEntryInstruction where CEI_PK = '{0}'", entryInstructionPK);
			using (var command = Db.Connection.Command(entryInstructionSql))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("record 1", reader.Read());
					AssertEquals(entryInstructionPK, reader.GetGuid(0));
					AssertEquals("CEI_DateForDuty is not NULL", DBNull.Value, reader.GetValue(1));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		Guid CreateJobDeclaration(Guid branchPK, Guid companyPK, Guid importerPK, Guid supplierPK, string messageType, int clusterKey, string declarationReference = "")
		{
			var declarationPK = Guid.NewGuid();
			var declarationRef = string.IsNullOrEmpty(declarationReference) ? Guid.NewGuid().ToString("n") : declarationReference;
			var sql = @"
INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_OH_Importer, JE_OH_Supplier, JE_MessageType, JE_GB, JE_GC, JE_TransportMode, JE_SystemCreateUser, JE_SystemLastEditUser, JE_ClusterKey, JE_SystemCreateTimeUtc, JE_SystemLastEditTimeUtc, JE_DeclarationReference)
VALUES (@declarationPK, 'FR', @importerPK, @supplierPK, @messageType, @branchPK, @companyPK, @transportMode, @createUser, @lastEditUser, @clusterKey, GetUtcDate(), GetUtcDate(), @declarationReference)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				command.AddParameter("@supplierPK", SqlDbType.UniqueIdentifier, supplierPK);
				command.AddParameter("@messageType", SqlDbType.VarChar, JobDeclarationSchema.JE_MessageType.MaxLength, messageType);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@transportMode", SqlDbType.VarChar, JobDeclarationSchema.JE_TransportMode.MaxLength, "SEA");
				command.AddParameter("@createUser", SqlDbType.VarChar, JobDeclarationSchema.JE_SystemCreateUser.MaxLength, "ABC");
				command.AddParameter("@lastEditUser", SqlDbType.VarChar, JobDeclarationSchema.JE_SystemLastEditUser.MaxLength, "ABC");
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@declarationReference", SqlDbType.VarChar, declarationRef);
				command.ExecuteNonQuery();
			}
			return declarationPK;
		}

		Guid CreateCusSupportingInfo(Guid parentID, string parentTableCode, string csiType, string csiCode, decimal csiQuantity3, decimal csiValue)
		{
			var supportingInfoPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusSupportingInfo (CSI_PK, CSI_DataModel, CSI_ParentID, CSI_ParentTableCode, CSI_Type, CSI_Code, CSI_Quantity3, CSI_Value)
    VALUES (@supportingInfoPK, 'FR', @parentID, @parentTableCode, @type, @csiCode, @quantity3, @value);
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@supportingInfoPK", SqlDbType.UniqueIdentifier, supportingInfoPK);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentID);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, CusSupportingInfoSchema.CSI_ParentTableCode.MaxLength, parentTableCode);
				command.AddParameter("@type", SqlDbType.VarChar, CusSupportingInfoSchema.CSI_Type.MaxLength, csiType);
				command.AddParameter("@csiCode", SqlDbType.VarChar, CusSupportingInfoSchema.CSI_Code.MaxLength, csiCode);
				command.AddParameter("@quantity3", SqlDbType.Decimal, CusSupportingInfoSchema.CSI_Quantity3.MaxLength, csiQuantity3);
				command.AddParameter("@value", SqlDbType.Decimal, CusSupportingInfoSchema.CSI_Value.MaxLength, csiValue);
				command.ExecuteNonQuery();
			}
			return supportingInfoPK;
		}

		Guid CreateCusEntryInstruction(Guid declarationPK, int clusterKey, DateTime? dateForDuty)
		{
			var entryInstructionPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_JE, CEI_DateForDuty, CEI_ClusterKey, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser)
VALUES (@entryInstructionPK, 'FR', @declarationPK, @dateForDuty, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@entryInstructionPK", SqlDbType.UniqueIdentifier, entryInstructionPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@dateForDuty", SqlDbType.SmallDateTime, dateForDuty ?? (object)DBNull.Value);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return entryInstructionPK;
		}

		Guid CreateCusEntryNum(Guid entryHeaderPK, string entryType, string entryNum, int entryIsSystemGenerated)
		{
			var cusEntryNumPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusEntryNum (CE_PK, CE_ParentID, CE_ParentTable, CE_EntryIsSystemGenerated, CE_EntryType, CE_EntryNum, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser)
VALUES (@cusEntryNumPK, @parentID, 'CusEntryHeader', @entryIsSystemGenerated, @entryType, @entryNum, getutcdate(), '~BP', getutcdate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@cusEntryNumPK", SqlDbType.UniqueIdentifier, cusEntryNumPK);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("@entryIsSystemGenerated", SqlDbType.Bit, CusEntryNumSchema.CE_EntryIsSystemGenerated.MaxLength, entryIsSystemGenerated);
				command.AddParameter("@entryType", SqlDbType.VarChar, CusEntryNumSchema.CE_EntryType.MaxLength, entryType);
				command.AddParameter("@entryNum", SqlDbType.VarChar, CusEntryNumSchema.CE_EntryNum.MaxLength, entryNum);
				command.ExecuteNonQuery();
			}
			return cusEntryNumPK;
		}

		Guid CreateCusEntryHeader(Guid declarationPK, Guid entryInstructionPK, int clusterKey, DateTime baeDate)
		{
			var entryHeaderPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE,CH_CEI_Instruction, CH_EntryStatus, CH_ClusterKey, CH_EntryReleaseDate, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
VALUES (@entryHeaderPK, 'FR', @declarationPK, @entryInstructionPK, @entryStatus, @clusterKey, @beaDate, getutcdate(), '~BP', getutcdate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@entryHeaderPK", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@entryInstructionPK", SqlDbType.UniqueIdentifier, entryInstructionPK);
				command.AddParameter("@entryStatus", SqlDbType.VarChar, CusEntryHeaderSchema.CH_EntryStatus.MaxLength, entryStatus);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@beaDate", SqlDbType.DateTime, baeDate);
				command.ExecuteNonQuery();
			}
			return entryHeaderPK;
		}
		string entryStatus;

		Guid CreateCusEntryLine(Guid entryHeaderPK, int clusterKey)
		{
			var entryLinePK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusEntryLine (CL_PK, CL_DataModel, CL_CH, CL_ClusterKey, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser)
VALUES (@entryLinePK, 'FR', @entryHeaderPK, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
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
VALUES (@invoiceLinePK, 'FR', @declarationPK, @clusterKey)
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
VALUES (@invoiceLinePK, 'FR', @invoiceHeaderPK, @entryLinePK, @entryInstructionPK, @clusterKey)
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

		public void TestCaseForCountriesUnderFrenchCustomsJurisdiction()
		{
			companyPK = TestDataCreator.CreateCompany("TC2", "MQ", "NTD");
			branchPK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_RL_NKHomePort,GB_RN_NKCountryCode, GB_GC) VALUES (@branchPK, 'TB2', 'TAJNB','MQ', @companyPK)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}

			var declarationPK = CreateTestJobDeclarationuUseJE_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 7, 13), 1, 1m);
			var invoiceLinePK = CreateTestJobDeclarationUseJI_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 0m, "CES", "100", "5525145", 1, new DateTime(2020, 7, 13), 2, 1m);
			CreateTestJobDeclarationUseJZ_PKJoinSupportingInfo("IMP", "SUP", "0001", new DateTime(2020, 7, 13), 10m, "CES", "100", "5525145", 1, new DateTime(2020, 9, 13), 3, 1m);

			AssertD48HasSingleDataOnly(companyPK, new DateTime(2020, 7, 12), new DateTime(2020, 7, 13), 10m, declarationPK, "TESTA", "5525145", new DateTime(2021, 5, 13), "NO");

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@BaeFromDate", SqlDbType.SmallDateTime, new DateTime(2020, 6, 12));
				command.AddParameter("@BaeToDate", SqlDbType.SmallDateTime, new DateTime(2020, 8, 13));
				command.AddParameter("@WriteOffOnly", SqlDbType.VarChar, "BOTH");

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					Assert("There should be another records", reader.Read());
					Assert("There should be no other records", !reader.Read());
				}
			}
		}
	}
}

