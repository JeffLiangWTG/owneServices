using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZA;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZA.Testing
{
	[TestedType(typeof(Report_ZAProductListing))]
	class Report_ZAProductListingTest : DbCreateScriptTest
	{
		public void TestReportFunction()
		{
			PrepareData();

			RunTest("Columns and Data", CheckColumns, Organisation1PK, "Part1", "Part1 Org1-1", "11111111");
			RunTest("Orgnisation Filter", OrgFilter, Organisation1PK, "", "", "");
			RunTest("Invalid Org Filter", EmptyResults, Guid.Empty, "", "", "");
			RunTest("Invalid ProductCode", EmptyResults, Organisation1PK, "DoesNotExist", "", "");
			RunTest("Invalid Description", EmptyResults, Organisation1PK, "", "DoesNotExist", "");
			RunTest("Invalid Tariff", EmptyResults, Organisation1PK, "", "", "DoesNotExist");
		}

		//[TestCase(...)] would have been nice
		void RunTest(string testName, Action<string, IDataReader> testCaseAssertions, Guid org, string productCode, string description, string tariffCode)
		{
			var sql = @"SELECT * FROM Report_ZAProductListing(@Organisation, @ProductCode, @Description, @TariffCode)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@Organisation", SqlDbType.UniqueIdentifier, org);
				command.AddParameter("@ProductCode", SqlDbType.VarChar, productCode);
				command.AddParameter("@Description", SqlDbType.VarChar, description);
				command.AddParameter("@TariffCode", SqlDbType.VarChar, tariffCode);

				using (var reader = command.ExecuteReader())
				{
					testCaseAssertions.Invoke(testName, reader);
				}
			}
		}

		#region Test Cases
		void CheckColumns(string testName, IDataReader reader)
		{
			AssertNotNull(testName + " expecting reader", reader);
			AssertEquals("Expecting 1 row", true, reader.Read());
			AssertEquals("Expecting 15 columns", 15, reader.FieldCount);

			CombineAssertions("Checking columns", () =>
			{
				AssertEquals("Col  0:", "ProductCode", reader.GetName(0));
				AssertEquals("Col  1:", "Description", reader.GetName(1));
				AssertEquals("Col  2:", "Unit", reader.GetName(2));
				AssertEquals("Col  3:", "Tariff", reader.GetName(3));
				AssertEquals("Col  4:", "DutyRate", reader.GetName(4));
				AssertEquals("Col  5:", "Origin", reader.GetName(5));
				AssertEquals("Col  6:", "CustomsOrganisation", reader.GetName(6));
				AssertEquals("Col  7:", "Type", reader.GetName(7));
				AssertEquals("Col  8:", "NewUsed", reader.GetName(8));
				AssertEquals("Col  9:", "EngineCapacity", reader.GetName(9));
				AssertEquals("Col 10:", "VehicleFormat", reader.GetName(10));
				AssertEquals("Col 11:", "VehicleType", reader.GetName(11));
				AssertEquals("Col 12:", "VehicleColour", reader.GetName(12));
				AssertEquals("Col 13:", "RelatedOrganisation", reader.GetName(13));
				AssertEquals("Col 14:", "RelationshipType", reader.GetName(14));
			});

			CombineAssertions("Checking Content", () =>
			{
				AssertEquals("ProductCode", "Part1", reader[0]);
				AssertEquals("Description", "Part1 Org1-1", reader[1]);
				AssertEquals("Unit", "UNT", reader[2]);
				AssertEquals("Tariff", "11111111", reader[3]);
				AssertEquals("DutyRate", "FREE", reader[4]);
				AssertEquals("Origin", "ZA", reader[5]);
				AssertEquals("CustomsOrganisation", "MDORG001", reader[6]);
				AssertEquals("Type", "BTH", reader[7]);
				AssertEquals("NewUsed", "N", reader[8]);
				AssertEquals("EngineCapacity", "1800", reader[9]);
				AssertEquals("VehicleFormat", "FBU", reader[10]);
				AssertEquals("VehicleType", "SUV", reader[11]);
				AssertEquals("VehicleColour", "BLUE", reader[12]);
				AssertEquals("RelatedOrganisation", "MDORG001", reader[13]);
				AssertEquals("RelationshipType", "BTH", reader[14]);
			});

			AssertEquals("Expecting 1 row", false, reader.Read());
		}
		void OrgFilter(string testName, IDataReader reader)
		{
			var details = new List<Tuple<string, string>>();

			while (reader.Read())
			{
				details.Add(new Tuple<string, string>(reader["CustomsOrganisation"].ToString(), reader["VehicleColour"].ToString()));
			}

			CombineAssertions(testName + ": Expecting 3 records for Org1", () =>
			{
				AssertEquals("Record Count: ", 3, details.Count);

				Assert("MDORG001-BLUE", details.Any(x => x.Item1 == "MDORG001" && x.Item2 == "BLUE"));
				Assert("MDORG001-RED", details.Any(x => x.Item1 == "MDORG001" && x.Item2 == "RED"));
				Assert("MDORG002-PINK", details.Any(x => x.Item1 == "MDORG002" && x.Item2 == "PINK"));
			});
		}
		void EmptyResults(string testName, IDataReader reader)
		{
			CombineAssertions(testName + ": Expecting Empty Reader", () =>
			{
				AssertNotNull("reader", reader);
				AssertEquals("Should be empty", false, reader.Read());
			});
		}
		#endregion
		#region Test Data
		void PrepareData()
		{
			var companyPK = TestDataCreator.CreateCompany("TC001", "ZA", "ZAR");

			var branchPK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_RL_NKHomePort, GB_GC) VALUES (@branchPK, 'TB1', 'ZAJNB', @companyPK)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}

			Organisation1PK = TestDataCreator.CreateOrganisation("MDORG001", "MD001");
			Organisation2PK = TestDataCreator.CreateOrganisation("MDORG002", "MD002");

			var tariffCodes = new List<string>() { "11111111", "22222222", "33333333", "44444444" };

			tariffCodes.ForEach(CreateTariffData);

			CreateTestCase("Part1", "Part1 Org1-1", Organisation1PK, tariffCodes[0], "BLUE", false, Organisation1PK, "BTH");
			CreateTestCase("Part2", "Part2 Org1-2", Organisation1PK, tariffCodes[1], "RED", false, Organisation2PK, "SUP");
			CreateTestCase("Part3", "Part3 Org2-2", Organisation2PK, tariffCodes[2], "GREEN", false, Organisation2PK, "SUP");
			CreateTestCase("Part4", "Part4 Org2-1", Organisation2PK, tariffCodes[3], "PINK", false, Organisation1PK, "SUP");
		}

		void CreateTariffData(string tariffCode)
		{
			var sql = @"
				DECLARE @TariffTypePK UNIQUEIDENTIFIER 
				DECLARE @PrefPK UNIQUEIDENTIFIER 
				DECLARE @TariffPK1 UNIQUEIDENTIFIER = NEWID()
				DECLARE @TariffPK2 UNIQUEIDENTIFIER = NEWID()

				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'ZA')
					INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
					VALUES (NEWID(), 'ZA', 'South Africa', NULL)

				SELECT @TariffTypePK = ZZI_PK from RefDatabase_RefCusTariffType where ZZI_TariffType = '1P1'
				IF @TariffTypePK IS NULL
				BEGIN
					SET @TariffTypePK = NEWID()
					INSERT RefDatabase_RefCusTariffType(ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping) 
					VALUES (@TariffTypePK, '1P1', 'Sched 1 Part 1', 'ZA')
				END

				SELECT @PrefPK = ZZS_PK FROM RefDatabase_RefCusPreference WHERE ZZS_ZZZ_NKDataGrouping = 'ZA' and ZZS_Preference = '100'
				IF @PrefPK IS NULL
				BEGIN
					SET @PrefPK = NEWID()
					INSERT INTO RefDatabase_RefCusPreference (ZZS_PK, ZZS_Preference, ZZS_Description, ZZS_ZZZ_NKDataGrouping)
					VALUES (@PrefPK, '100', 'Normal', 'ZA')
				END

				INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode) 
				VALUES (@TariffPK1, @TariffTypePK, @TariffCode, 'Expired Tariff' + @TariffCode, 'ZA', '2014-01-01 00:00:00.000', '2019-12-31 23:59:00.000', '')

				INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode) 
				VALUES (@TariffPK2, @TariffTypePK, @TariffCode, 'Valid Tariff' + @TariffCode, 'ZA', '2020-01-01 00:00:00.000', '2079-06-06 23:59:00.000', '')

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormulaDerivedFrom, ZZ2_RateFormula)
				VALUES (NEWID(), @TariffPK1, @PrefPK, 'ZA', 'OLD', 'TEST')

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormulaDerivedFrom, ZZ2_RateFormula)
				VALUES (NEWID(), @TariffPK2, @PrefPK, 'ZA', 'FREE', 'TEST')
			";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@TariffCode", SqlDbType.VarChar, tariffCode);
				command.ExecuteNonQuery();
			}
		}

		void CreateTestCase(string partNum, string partDescription, Guid orgPK, string tariffCode, string colour,
							bool useCusClass, Guid orgRelationPK, string orgRelationship)
		{
			var sql = @"
				--MD2 - For testing in SQL
				--DECLARE
				--@PartNum varchar(35) = 'PART001',
				--@PartDescription varchar(80) = 'Test Part1',
				--@OrgPK UNIQUEIDENTIFIER = '89C2809E-799A-44A0-9513-545DB44CD019',
				--@TariffCode varchar(15) = '12345000',
				--@Colour varchar(50) = 'BLUE',
				--@UseCusClass bit = 1,
				--@OrgRelation UNIQUEIDENTIFIER = '7E4DC7C2-B231-4D49-A5EC-6CCB480B7526',
				--@OrgRelationship varchar(3) = 'BTH'

				DECLARE
				@OP_PK UNIQUEIDENTIFIER = NEWID(),
				@StockUnit varchar(3) = 'UNT',
				@ChildType varchar(3) = 'BTH',
				@VehicleFormat varchar(3) = 'FBU',
				@VehicleType varchar(50) = 'SUV',
				@Engine varchar(50) = '1800',
				@NewUsed varchar(1) = 'N',
				@CI_Country varchar(2) = 'ZA'
				DECLARE
				@AddInfo varchar(1024) = replace(replace(replace(replace(replace(
										'Colour={0}*EngineCapacity={1}*NewUsed={2}*VehicleFormat={3}*VehicleType={4}',
											'{0}',@Colour),
											'{1}',@Engine),
											'{2}',@NewUsed),
											'{3}',@VehicleFormat),
											'{4}',@VehicleType),
				@CC_PK UNIQUEIDENTIFIER,
				@CI_TariffCode VARCHAR(15) = @TariffCode

				INSERT INTO dbo.OrgSupplierPart(OP_PK, OP_PartNum, OP_Desc, OP_StockKeepingUnit)
				VALUES (@OP_PK, @PartNum, @PartDescription, @StockUnit)

				if @UseCusClass = 1
				BEGIN
					SET @CC_PK = NEWID()
					SET @CI_TariffCode = ''

					INSERT INTO dbo.CusClassification(CC_PK, CC_TariffNum)
					VALUES (@CC_PK, @TariffCode)
				END

				INSERT INTO dbo.CusClassPartPivot (CI_PK, CI_OP, CI_OH, CI_TariffNum, CI_ChildType, CI_AddInfo, CI_CC, CI_RN_NKCountryOfOrigin, CI_RN_NKCountry, CI_SystemCreateTimeUtc, CI_SystemCreateUser, CI_SystemLastEditTimeUtc, CI_SystemLastEditUser)
				VALUES (NEWID(), @OP_PK, @OrgPK, @CI_TariffCode, @ChildType, @AddInfo, @CC_PK, @CI_Country, @CI_Country, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.OrgPartRelation(OU_PK, OU_OH, OU_OP, OU_Relationship, OU_SystemCreateTimeUtc, OU_SystemCreateUser, OU_SystemLastEditTimeUtc, OU_SystemLastEditUser)
				VALUES (NEWID(), @OrgRelation, @OP_PK, @OrgRelationship, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@PartNum", SqlDbType.VarChar, partNum);
				command.AddParameter("@PartDescription", SqlDbType.VarChar, partDescription);
				command.AddParameter("@OrgPK", SqlDbType.UniqueIdentifier, orgPK);
				command.AddParameter("@TariffCode", SqlDbType.VarChar, tariffCode);
				command.AddParameter("@Colour", SqlDbType.VarChar, colour);
				command.AddParameter("@UseCusClass", SqlDbType.Bit, useCusClass);
				command.AddParameter("@OrgRelation", SqlDbType.UniqueIdentifier, orgRelationPK);
				command.AddParameter("@OrgRelationship", SqlDbType.VarChar, orgRelationship);

				command.ExecuteNonQuery();
			}
		}
		#endregion

		Guid Organisation1PK;
		Guid Organisation2PK;
	}
}
