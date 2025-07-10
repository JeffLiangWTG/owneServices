using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.TW;
using Enterprise.Build.Database.Script.Public.Test;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.TW
{
	[TestedType(typeof(Report_VolumeRanking))]
	class Report_VolumeRankingTest : DbCreateScriptTest
	{
		Guid companyPK;
		Guid branchPK;

		Guid organisation1PK;
		Guid organisation1MainAddressPK;
		Guid organisation2PK;
		Guid organisation2MainAddressPK;
		Guid organisation3PK;
		Guid organisation3MainAddressPK;
		Guid organisation4PK;
		Guid organisation4MainAddressPK;

		Guid nonTWOrganisationPK;
		Guid nonTWorganisationMainAddressPK;

		Guid broker1PK;
		Guid broker2PK;

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

			organisation1PK = TestDataCreator.CreateOrganisation("ORG_1", "Org One");
			organisation1MainAddressPK = TestDataCreator.CreateAddress(organisation1PK, "Head Office", "Somewhere", "TW");
			TestDataCreator.CreateOrgAddressCapability(organisation1MainAddressPK, "OFC", true);

			organisation2PK = TestDataCreator.CreateOrganisation("ORG_2", "Org Two");
			organisation2MainAddressPK = TestDataCreator.CreateAddress(organisation2PK, "Head Office", "Over there", "TW");
			TestDataCreator.CreateOrgAddressCapability(organisation2MainAddressPK, "OFC", true);

			organisation3PK = TestDataCreator.CreateOrganisation("ORG_3", "Org Three");
			organisation3MainAddressPK = TestDataCreator.CreateAddress(organisation3PK, "Head Office", "Out there", "TW");
			TestDataCreator.CreateOrgAddressCapability(organisation3MainAddressPK, "OFC", true);

			organisation4PK = TestDataCreator.CreateOrganisation("ORG_4", "Org Four");
			organisation4MainAddressPK = TestDataCreator.CreateAddress(organisation4PK, "Head Office", "addr 1", "addr2", "city", "state", "postcode", "Override Org Four", "TW");
			TestDataCreator.CreateOrgAddressCapability(organisation4MainAddressPK, "OFC", true);

			nonTWOrganisationPK = TestDataCreator.CreateOrganisation("ORG_NonTW", "Org NonTW");
			nonTWorganisationMainAddressPK = TestDataCreator.CreateAddress(nonTWOrganisationPK, "Head Office", "Not TW", "AU");
			TestDataCreator.CreateOrgAddressCapability(nonTWorganisationMainAddressPK, "OFC", true);

			broker1PK = GlbGeneratorForTests.NewStaff("BK1", null);
			broker2PK = GlbGeneratorForTests.NewStaff("BK2", null);
		}

		public void TestSupplierCode()
		{
			var reportSql = @"select JobCount, SupplierCode, SupplierName from Report_VolumeRanking(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @GroupingType, @SortingType, @BrokerStaffs, @Top)";
			var declaration1 = CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, Guid.Empty, 1);
			TestDataCreator.CreateDocAddress(Guid.Empty, string.Empty, declaration1, "JE", "SUD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true);
			TestDataCreator.CreateDocAddress(Guid.Empty, "Org One", declaration1, "JE", "STA", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true, "TW");

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 11));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 12));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Supplier");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(1, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));
				}
			}

			var declaration2 = CreateTestJobDeclaration(new DateTime(2019, 12, 13), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, Guid.Empty, 2);
			TestDataCreator.CreateDocAddress(Guid.Empty, "Org One", declaration2, "JE", "SUD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true, "TW");

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 13));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 14));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Supplier");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(1, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));
				}
			}

			var declaration3 = CreateTestJobDeclaration(new DateTime(2019, 12, 15), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, Guid.Empty, 3);
			TestDataCreator.CreateDocAddress(Guid.Empty, string.Empty, declaration3, "JE", "SUD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true);
			TestDataCreator.CreateDocAddress(Guid.Empty, "Override Org Four", declaration3, "JE", "STA", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true, "TW");

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 15));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 16));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Supplier");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(1, reader.GetInt32(0));
					AssertEquals("ORG_4", reader.GetString(1));
					AssertEquals("Override Org Four", reader.GetString(2));
				}
			}

			var declaration4 = CreateTestJobDeclaration(new DateTime(2019, 12, 17), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, Guid.Empty, 4);
			TestDataCreator.CreateDocAddress(Guid.Empty, "Override Org Four", declaration4, "JE", "SUD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true, "TW");

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 17));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 18));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Supplier");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(1, reader.GetInt32(0));
					AssertEquals("ORG_4", reader.GetString(1));
					AssertEquals("Override Org Four", reader.GetString(2));
				}
			}

			var declaration5 = CreateTestJobDeclaration(new DateTime(2019, 12, 19), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, Guid.Empty, 5);
			TestDataCreator.CreateDocAddress(organisation1MainAddressPK, string.Empty, declaration5, "JE", "SUD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, false);

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 19));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 20));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Supplier");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(1, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));
				}
			}
		}

		public void TestImporterCode()
		{
			var reportSql = @"select JobCount, ImporterCode, ImporterName from Report_VolumeRanking(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @GroupingType, @SortingType, @BrokerStaffs, @Top)";
			var declaration1 = CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, Guid.Empty, 1);
			TestDataCreator.CreateDocAddress(Guid.Empty, string.Empty, declaration1, "JE", "IMD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true);
			TestDataCreator.CreateDocAddress(Guid.Empty, "Org One", declaration1, "JE", "ITA", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true, "TW");

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 11));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 12));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(1, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));
				}
			}

			var declaration2 = CreateTestJobDeclaration(new DateTime(2019, 12, 13), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, Guid.Empty, 2);
			TestDataCreator.CreateDocAddress(Guid.Empty, "Org One", declaration2, "JE", "IMD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true, "TW");

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 13));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 14));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(1, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));
				}
			}

			var declaration3 = CreateTestJobDeclaration(new DateTime(2019, 12, 15), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, Guid.Empty, 3);
			TestDataCreator.CreateDocAddress(Guid.Empty, string.Empty, declaration3, "JE", "IMD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true);
			TestDataCreator.CreateDocAddress(Guid.Empty, "Override Org Four", declaration3, "JE", "ITA", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true, "TW");

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 15));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 16));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(1, reader.GetInt32(0));
					AssertEquals("ORG_4", reader.GetString(1));
					AssertEquals("Override Org Four", reader.GetString(2));
				}
			}

			var declaration4 = CreateTestJobDeclaration(new DateTime(2019, 12, 17), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, Guid.Empty, 4);
			TestDataCreator.CreateDocAddress(Guid.Empty, "Override Org Four", declaration4, "JE", "IMD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true, "TW");

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 17));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 18));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(1, reader.GetInt32(0));
					AssertEquals("ORG_4", reader.GetString(1));
					AssertEquals("Override Org Four", reader.GetString(2));
				}
			}

			var declaration5 = CreateTestJobDeclaration(new DateTime(2019, 12, 19), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, Guid.Empty, 5);
			TestDataCreator.CreateDocAddress(organisation1MainAddressPK, string.Empty, declaration5, "JE", "IMD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, false);

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 19));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 20));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(1, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));
				}
			}
		}

		public void TestDeclarationDate()
		{
			CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, organisation1MainAddressPK, 1);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "IMP", "SEA", "ABC", "ABC", 3, "C2", Guid.Empty, organisation1MainAddressPK, 2);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "SEA", "ABC", "ABC", 5, "C3X", Guid.Empty, organisation1MainAddressPK, 3);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "AIR", "ABC", "ABC", 7, "C3M", Guid.Empty, organisation1MainAddressPK, 4);
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 9, "C1", Guid.Empty, organisation1MainAddressPK, 5);
			CreateTestJobDeclaration(new DateTime(2019, 12, 14), "EXP", "AIR", "ABC", "ABC", 9, "C1", Guid.Empty, organisation1MainAddressPK, 6);
			CreateTestJobDeclaration(new DateTime(2019, 12, 15), "EXP", "AIR", "ABC", "ABC", 9, "C1", Guid.Empty, organisation1MainAddressPK, 7);

			CreateCusInBondHeader(new DateTime(2019, 12, 11), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation1MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 14), "ABC", "ABC", "04", organisation1MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 15), "ABC", "ABC", "04", organisation1MainAddressPK);

			var reportSql = @"select JobCount, ImporterCode, ImporterName from Report_VolumeRanking(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @GroupingType, @SortingType, @BrokerStaffs, @Top)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 12));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 13));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(4, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(12, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2018, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2018, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 12));
				command.AddParameter("@EntryToDate", SqlDbType.VarChar, "");
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(10, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.VarChar, "");
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 13));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(6, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestShipmentType()
		{
			CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, organisation1MainAddressPK, 1);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "IMP", "AIR", "ABC", "ABC", 3, "C2", Guid.Empty, organisation1MainAddressPK, 2);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "SEA", "ABC", "ABC", 5, "C3X", Guid.Empty, organisation1MainAddressPK, 3);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "AIR", "ABC", "ABC", 7, "C3M", Guid.Empty, organisation1MainAddressPK, 4);
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 9, "C1", Guid.Empty, organisation1MainAddressPK, 5);

			CreateCusInBondHeader(new DateTime(2019, 12, 11), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation1MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation1MainAddressPK);

			var reportSql = @"select JobCount, ImporterCode, ImporterName from Report_VolumeRanking(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @GroupingType, @SortingType, @BrokerStaffs, @Top)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "IMP");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(2, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "EXP");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(3, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "TRN");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(4, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(9, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestTransportMode()
		{
			CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, organisation1MainAddressPK, 1);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "IMP", "AIR", "ABC", "ABC", 3, "C2", Guid.Empty, organisation1MainAddressPK, 2);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "SEA", "ABC", "ABC", 5, "C3X", Guid.Empty, organisation1MainAddressPK, 3);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "AIR", "ABC", "ABC", 7, "C3M", Guid.Empty, organisation1MainAddressPK, 4);
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 9, "C1", Guid.Empty, organisation1MainAddressPK, 5);

			CreateCusInBondHeader(new DateTime(2019, 12, 11), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation1MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation1MainAddressPK);

			var reportSql = @"select JobCount, ImporterCode, ImporterName from Report_VolumeRanking(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @GroupingType, @SortingType, @BrokerStaffs, @Top)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "SEA");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(4, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "AIR");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(5, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(9, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestTop()
		{
			CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, organisation1MainAddressPK, 1);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "IMP", "AIR", "ABC", "ABC", 3, "C2", Guid.Empty, organisation1MainAddressPK, 2);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "SEA", "ABC", "ABC", 5, "C3X", Guid.Empty, organisation2MainAddressPK, 3);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "AIR", "ABC", "ABC", 7, "C3M", Guid.Empty, organisation2MainAddressPK, 4);
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 9, "C1", Guid.Empty, organisation3MainAddressPK, 5);

			CreateCusInBondHeader(new DateTime(2019, 12, 11), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation2MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation3MainAddressPK);

			var reportSql = @"select JobCount, ImporterCode, ImporterName from Report_VolumeRanking(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @GroupingType, @SortingType, @BrokerStaffs, @Top)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 1);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(4, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 2);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(4, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));

					Assert("There should be a record", reader.Read());
					AssertEquals(3, reader.GetInt32(0));
					AssertEquals("ORG_2", reader.GetString(1));
					AssertEquals("Org Two", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 3);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(4, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));

					Assert("There should be a record", reader.Read());
					AssertEquals(3, reader.GetInt32(0));
					AssertEquals("ORG_2", reader.GetString(1));
					AssertEquals("Org Two", reader.GetString(2));

					Assert("There should be a record", reader.Read());
					AssertEquals(2, reader.GetInt32(0));
					AssertEquals("ORG_3", reader.GetString(1));
					AssertEquals("Org Three", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestBrokerStaff()
		{
			CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, organisation1MainAddressPK, 1, "BK1");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "IMP", "AIR", "ABC", "ABC", 3, "C2", Guid.Empty, organisation1MainAddressPK, 2, "BK2");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "SEA", "ABC", "ABC", 5, "C3X", Guid.Empty, organisation1MainAddressPK, 3, "BK1");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "AIR", "ABC", "ABC", 7, "C3M", Guid.Empty, organisation1MainAddressPK, 4, "BK2");
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 9, "C1", Guid.Empty, organisation1MainAddressPK, 5, "BK1");

			var inBondHeaderPK = CreateCusInBondHeader(new DateTime(2019, 12, 11), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateCusInBondMoveHeader(inBondHeaderPK, "BK1");
			inBondHeaderPK = CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateCusInBondMoveHeader(inBondHeaderPK, "BK2");
			inBondHeaderPK = CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation1MainAddressPK);
			CreateCusInBondMoveHeader(inBondHeaderPK, "BK1");
			inBondHeaderPK = CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation1MainAddressPK);
			CreateCusInBondMoveHeader(inBondHeaderPK, "BK2");

			var reportSql = @"select JobCount from Report_VolumeRanking(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @GroupingType, @SortingType, @BrokerStaffs, @Top)";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "'" + broker1PK.ToString() + "'");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(5, reader.GetInt32(0));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "'" + broker1PK.ToString() + "','" + broker2PK.ToString() + "'");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(9, reader.GetInt32(0));
					Assert("There should be no other records", !reader.Read());
				}
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(9, reader.GetInt32(0));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestSortByNumberOfEntries()
		{
			var reportSql = @"select JobCount, ImporterCode, ImporterName from Report_VolumeRanking(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @GroupingType, @SortingType, @BrokerStaffs, @Top)";
			CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, organisation1MainAddressPK, 1);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "IMP", "AIR", "ABC", "ABC", 3, "C2", Guid.Empty, organisation1MainAddressPK, 2);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "SEA", "ABC", "ABC", 5, "C3X", Guid.Empty, organisation2MainAddressPK, 3);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "AIR", "ABC", "ABC", 7, "C3M", Guid.Empty, organisation2MainAddressPK, 4);
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 9, "C1", Guid.Empty, organisation3MainAddressPK, 5);

			CreateCusInBondHeader(new DateTime(2019, 12, 11), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation2MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation3MainAddressPK);

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(4, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));

					Assert("There should be a record", reader.Read());
					AssertEquals(3, reader.GetInt32(0));
					AssertEquals("ORG_2", reader.GetString(1));
					AssertEquals("Org Two", reader.GetString(2));

					Assert("There should be a record", reader.Read());
					AssertEquals(2, reader.GetInt32(0));
					AssertEquals("ORG_3", reader.GetString(1));
					AssertEquals("Org Three", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestSortByGrossWeight()
		{
			var reportSql = @"select JobCount, TotalGrossWeight from Report_VolumeRanking(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @GroupingType, @SortingType, @BrokerStaffs, @Top)";
			CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, organisation1MainAddressPK, 1, grossWeight: 5, grossWeightUQ: "KG");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "IMP", "AIR", "ABC", "ABC", 3, "C2", Guid.Empty, organisation1MainAddressPK, 2, grossWeight: 500, grossWeightUQ: "G");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "SEA", "ABC", "ABC", 5, "C3X", Guid.Empty, organisation2MainAddressPK, 3, grossWeight: 0.1m, grossWeightUQ: "T");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "AIR", "ABC", "ABC", 7, "C3M", Guid.Empty, organisation2MainAddressPK, 4, grossWeight: 4, grossWeightUQ: "KG");
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 9, "C1", Guid.Empty, organisation3MainAddressPK, 5, grossWeight: 3, grossWeightUQ: "KG");

			var inBondHeaderPK = CreateCusInBondHeader(new DateTime(2019, 12, 11), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateImportCusInBondBill(inBondHeaderPK, 6000m, "G");
			inBondHeaderPK = CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateImportCusInBondBill(inBondHeaderPK, 7m, "KG");
			inBondHeaderPK = CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation2MainAddressPK);
			CreateImportCusInBondBill(inBondHeaderPK, 0.8m, "T");
			inBondHeaderPK = CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation3MainAddressPK);
			CreateImportCusInBondBill(inBondHeaderPK, 9m, "KG");

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Gross Weight");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(3, reader.GetInt32(0));
					AssertEquals(904m, reader.GetDecimal(1));

					Assert("There should be a record", reader.Read());
					AssertEquals(4, reader.GetInt32(0));
					AssertEquals(18.5m, reader.GetDecimal(1));

					Assert("There should be a record", reader.Read());
					AssertEquals(2, reader.GetInt32(0));
					AssertEquals(12m, reader.GetDecimal(1));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestSortByNetWeight()
		{
			var reportSql = @"select JobCount, TotalNetWeight from Report_VolumeRanking(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @GroupingType, @SortingType, @BrokerStaffs, @Top)";
			CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, organisation1MainAddressPK, 1, netWeight: 5, netWeightUQ: "KG");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "IMP", "AIR", "ABC", "ABC", 3, "C2", Guid.Empty, organisation1MainAddressPK, 2, netWeight: 500, netWeightUQ: "G");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "SEA", "ABC", "ABC", 5, "C3X", Guid.Empty, organisation2MainAddressPK, 3, netWeight: 0.1m, netWeightUQ: "T");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "AIR", "ABC", "ABC", 7, "C3M", Guid.Empty, organisation2MainAddressPK, 4, netWeight: 4, netWeightUQ: "KG");
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 9, "C1", Guid.Empty, organisation3MainAddressPK, 5, netWeight: 3, netWeightUQ: "KG");

			CreateCusInBondHeader(new DateTime(2019, 12, 11), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation2MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation3MainAddressPK);

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Net Weight");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(3, reader.GetInt32(0));
					AssertEquals(104m, reader.GetDecimal(1));

					Assert("There should be a record", reader.Read());
					AssertEquals(4, reader.GetInt32(0));
					AssertEquals(5.5m, reader.GetDecimal(1));

					Assert("There should be a record", reader.Read());
					AssertEquals(2, reader.GetInt32(0));
					AssertEquals(3m, reader.GetDecimal(1));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestGroupBySupplier()
		{
			var reportSql = @"select JobCount, SupplierCode, SupplierName from Report_VolumeRanking(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @GroupingType, @SortingType, @BrokerStaffs, @Top)";
			CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", organisation1MainAddressPK, Guid.Empty, 1);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "IMP", "AIR", "ABC", "ABC", 3, "C2", organisation1MainAddressPK, Guid.Empty, 2);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "SEA", "ABC", "ABC", 5, "C3X", organisation1MainAddressPK, Guid.Empty, 3);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "AIR", "ABC", "ABC", 7, "C3M", organisation2MainAddressPK, Guid.Empty, 4);
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 9, "C1", organisation2MainAddressPK, Guid.Empty, 5);
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 1, "C1", organisation3MainAddressPK, Guid.Empty, 6);
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 1, "C1", nonTWorganisationMainAddressPK, Guid.Empty, 7);

			var declaration = CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 1, "C1", Guid.Empty, Guid.Empty, 8);
			TestDataCreator.CreateDocAddress(Guid.Empty, string.Empty, declaration, "JE", "SUD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true);
			TestDataCreator.CreateDocAddress(Guid.Empty, "Org One", declaration, "JE", "STA", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true, "TW");

			declaration = CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 1, "C1", Guid.Empty, Guid.Empty, 9);
			TestDataCreator.CreateDocAddress(Guid.Empty, "Org Two", declaration, "JE", "SUD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true, "TW");

			CreateCusInBondHeader(new DateTime(2019, 12, 11), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation2MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation3MainAddressPK);

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Supplier");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(4, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));

					Assert("There should be a record", reader.Read());
					AssertEquals(3, reader.GetInt32(0));
					AssertEquals("ORG_2", reader.GetString(1));
					AssertEquals("Org Two", reader.GetString(2));

					Assert("There should be a record", reader.Read());
					AssertEquals(1, reader.GetInt32(0));
					AssertEquals("ORG_3", reader.GetString(1));
					AssertEquals("Org Three", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestGroupByImporter()
		{
			var reportSql = @"select JobCount, ImporterCode, ImporterName from Report_VolumeRanking(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @GroupingType, @SortingType, @BrokerStaffs, @Top)";
			CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, organisation1MainAddressPK, 1);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "IMP", "AIR", "ABC", "ABC", 3, "C2", Guid.Empty, organisation1MainAddressPK, 2);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "SEA", "ABC", "ABC", 5, "C3X", Guid.Empty, organisation1MainAddressPK, 3);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "AIR", "ABC", "ABC", 7, "C3M", Guid.Empty, organisation2MainAddressPK, 4);
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 9, "C1", Guid.Empty, organisation2MainAddressPK, 5);
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 1, "C1", Guid.Empty, organisation3MainAddressPK, 6);
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 1, "C1", Guid.Empty, nonTWorganisationMainAddressPK, 7);

			var declaration = CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 1, "C1", Guid.Empty, Guid.Empty, 8);
			TestDataCreator.CreateDocAddress(Guid.Empty, string.Empty, declaration, "JE", "IMD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true);
			TestDataCreator.CreateDocAddress(Guid.Empty, "Org One", declaration, "JE", "ITA", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true, "TW");

			declaration = CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 1, "C1", Guid.Empty, Guid.Empty, 9);
			TestDataCreator.CreateDocAddress(Guid.Empty, "Org Two", declaration, "JE", "IMD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true, "TW");

			CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation2MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation3MainAddressPK);

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Importer");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(5, reader.GetInt32(0));
					AssertEquals("ORG_1", reader.GetString(1));
					AssertEquals("Org One", reader.GetString(2));

					Assert("There should be a record", reader.Read());
					AssertEquals(4, reader.GetInt32(0));
					AssertEquals("ORG_2", reader.GetString(1));
					AssertEquals("Org Two", reader.GetString(2));

					Assert("There should be a record", reader.Read());
					AssertEquals(2, reader.GetInt32(0));
					AssertEquals("ORG_3", reader.GetString(1));
					AssertEquals("Org Three", reader.GetString(2));
					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestGroupByDeclarationType()
		{
			var reportSql = @"select JobCount, DeclarationTypeCode from Report_VolumeRanking(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @GroupingType, @SortingType, @BrokerStaffs, @Top)";
			CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, organisation1MainAddressPK, 1, declarationType: "G1");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "IMP", "AIR", "ABC", "ABC", 3, "C2", Guid.Empty, organisation1MainAddressPK, 2, declarationType: "G1");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "SEA", "ABC", "ABC", 5, "C3X", Guid.Empty, organisation1MainAddressPK, 3, declarationType: "G5");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "AIR", "ABC", "ABC", 7, "C3M", Guid.Empty, organisation2MainAddressPK, 4, declarationType: "G5");
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 9, "C1", Guid.Empty, organisation2MainAddressPK, 5, declarationType: "G5");
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 1, "C1", Guid.Empty, organisation3MainAddressPK, 6, declarationType: "G3");

			var inBondHeaderPK = CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateCusInBondMoveHeader(inBondHeaderPK, declarationType: "T1");
			inBondHeaderPK = CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation2MainAddressPK);
			CreateCusInBondMoveHeader(inBondHeaderPK, declarationType: "T1");
			inBondHeaderPK = CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation3MainAddressPK);
			CreateCusInBondMoveHeader(inBondHeaderPK, declarationType: "T2");

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Declaration Type");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(3, reader.GetInt32(0));
					AssertEquals("G5", reader.GetString(1));

					Assert("There should be a record", reader.Read());
					AssertEquals(2, reader.GetInt32(0));
					AssertEquals("G1", reader.GetString(1));

					Assert("There should be a record", reader.Read());
					AssertEquals(2, reader.GetInt32(0));
					AssertEquals("T1", reader.GetString(1));

					Assert("There should be a record", reader.Read());
					AssertEquals(1, reader.GetInt32(0));
					AssertEquals("G3", reader.GetString(1));

					Assert("There should be a record", reader.Read());
					AssertEquals(1, reader.GetInt32(0));
					AssertEquals("T2", reader.GetString(1));

					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestGroupByExportLocation()
		{
			var reportSql = @"select JobCount, ExportLocationCode from Report_VolumeRanking(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @GroupingType, @SortingType, @BrokerStaffs, @Top)";
			CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, organisation1MainAddressPK, 1, exportLocation: "KELW149S");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "IMP", "AIR", "ABC", "ABC", 3, "C2", Guid.Empty, organisation1MainAddressPK, 2, exportLocation: "KELW149S");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "SEA", "ABC", "ABC", 5, "C3X", Guid.Empty, organisation1MainAddressPK, 3, exportLocation: "AG580");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "AIR", "ABC", "ABC", 7, "C3M", Guid.Empty, organisation2MainAddressPK, 4, exportLocation: "AG580");
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 9, "C1", Guid.Empty, organisation2MainAddressPK, 5, exportLocation: "AG580");
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 1, "C1", Guid.Empty, organisation3MainAddressPK, 6, exportLocation: "AG580");

			var inBondHeaderPK = CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateCusInBondMoveHeader(inBondHeaderPK, exportLocation: "ANP0030Y");
			inBondHeaderPK = CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation2MainAddressPK);
			CreateCusInBondMoveHeader(inBondHeaderPK, exportLocation: "ANP0030Y");
			inBondHeaderPK = CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation3MainAddressPK);
			CreateCusInBondMoveHeader(inBondHeaderPK, exportLocation: "AD710");

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Export Location");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(4, reader.GetInt32(0));
					AssertEquals("AG580", reader.GetString(1));

					Assert("There should be a record", reader.Read());
					AssertEquals(2, reader.GetInt32(0));
					AssertEquals("ANP0030Y", reader.GetString(1));

					Assert("There should be a record", reader.Read());
					AssertEquals(2, reader.GetInt32(0));
					AssertEquals("KELW149S", reader.GetString(1));

					Assert("There should be a record", reader.Read());
					AssertEquals(1, reader.GetInt32(0));
					AssertEquals("AD710", reader.GetString(1));

					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestGroupByOfficeOfReceipt()
		{
			var reportSql = @"select JobCount, OfficeOfReceiptCode from Report_VolumeRanking(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @GroupingType, @SortingType, @BrokerStaffs, @Top)";
			CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, organisation1MainAddressPK, 1, officeOfReceipt: "KELW149S");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "IMP", "AIR", "ABC", "ABC", 3, "C2", Guid.Empty, organisation1MainAddressPK, 2, officeOfReceipt: "KELW149S");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "SEA", "ABC", "ABC", 5, "C3X", Guid.Empty, organisation1MainAddressPK, 3, officeOfReceipt: "AG580");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "AIR", "ABC", "ABC", 7, "C3M", Guid.Empty, organisation2MainAddressPK, 4, officeOfReceipt: "AG580");
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 9, "C1", Guid.Empty, organisation2MainAddressPK, 5, officeOfReceipt: "AG580");
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 1, "C1", Guid.Empty, organisation3MainAddressPK, 6, officeOfReceipt: "AG580");

			CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "01", organisation1MainAddressPK, officeOfReceipt: "ANP0030Y");
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation2MainAddressPK, officeOfReceipt: "ANP0030Y");
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation3MainAddressPK, officeOfReceipt: "AD710");

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Office of Receipt");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(4, reader.GetInt32(0));
					AssertEquals("AG580", reader.GetString(1));

					Assert("There should be a record", reader.Read());
					AssertEquals(2, reader.GetInt32(0));
					AssertEquals("ANP0030Y", reader.GetString(1));

					Assert("There should be a record", reader.Read());
					AssertEquals(2, reader.GetInt32(0));
					AssertEquals("KELW149S", reader.GetString(1));

					Assert("There should be a record", reader.Read());
					AssertEquals(1, reader.GetInt32(0));
					AssertEquals("AD710", reader.GetString(1));

					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestGroupByPortOfOrigin()
		{
			var reportSql = @"select JobCount, PortOfOriginCode from Report_VolumeRanking(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @GroupingType, @SortingType, @BrokerStaffs, @Top)";
			CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, organisation1MainAddressPK, 1, portOfOrigin: "JPARI");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "IMP", "AIR", "ABC", "ABC", 3, "C2", Guid.Empty, organisation1MainAddressPK, 2, portOfOrigin: "JPARI");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "SEA", "ABC", "ABC", 5, "C3X", Guid.Empty, organisation1MainAddressPK, 3, portOfOrigin: "TWTPE");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "AIR", "ABC", "ABC", 7, "C3M", Guid.Empty, organisation2MainAddressPK, 4, portOfOrigin: "TWTPE");
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 9, "C1", Guid.Empty, organisation2MainAddressPK, 5, portOfOrigin: "TWTPE");
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 1, "C1", Guid.Empty, organisation3MainAddressPK, 6, portOfOrigin: "TWTPE");

			CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "01", organisation1MainAddressPK, "FRZ99");
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation2MainAddressPK, "FRZ99");
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation3MainAddressPK, "TWKEL");

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Port of Origin");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(4, reader.GetInt32(0));
					AssertEquals("TWTPE", reader.GetString(1));

					Assert("There should be a record", reader.Read());
					AssertEquals(2, reader.GetInt32(0));
					AssertEquals("FRZ99", reader.GetString(1));

					Assert("There should be a record", reader.Read());
					AssertEquals(2, reader.GetInt32(0));
					AssertEquals("JPARI", reader.GetString(1));

					Assert("There should be a record", reader.Read());
					AssertEquals(1, reader.GetInt32(0));
					AssertEquals("TWKEL", reader.GetString(1));

					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestGroupByFinalDestination()
		{
			var reportSql = @"select JobCount, FinalDestinationCode from Report_VolumeRanking(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @GroupingType, @SortingType, @BrokerStaffs, @Top)";
			CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, organisation1MainAddressPK, 1, finalDestination: "JPARI");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "IMP", "AIR", "ABC", "ABC", 3, "C2", Guid.Empty, organisation1MainAddressPK, 2, finalDestination: "JPARI");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "SEA", "ABC", "ABC", 5, "C3X", Guid.Empty, organisation1MainAddressPK, 3, finalDestination: "TWTPE");
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "AIR", "ABC", "ABC", 7, "C3M", Guid.Empty, organisation2MainAddressPK, 4, finalDestination: "TWTPE");
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 9, "C1", Guid.Empty, organisation2MainAddressPK, 5, finalDestination: "TWTPE");
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "ABC", 1, "C1", Guid.Empty, organisation3MainAddressPK, 6, finalDestination: "TWTPE");

			var inBondHeaderPK = CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateCusInBondMoveHeader(inBondHeaderPK, finalDestination: "FRZ99");
			inBondHeaderPK = CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation2MainAddressPK);
			CreateCusInBondMoveHeader(inBondHeaderPK, finalDestination: "FRZ99");
			inBondHeaderPK = CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "ABC", "04", organisation3MainAddressPK);
			CreateCusInBondMoveHeader(inBondHeaderPK, finalDestination: "TWKEL");

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Final Destination");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(4, reader.GetInt32(0));
					AssertEquals("TWTPE", reader.GetString(1));

					Assert("There should be a record", reader.Read());
					AssertEquals(2, reader.GetInt32(0));
					AssertEquals("FRZ99", reader.GetString(1));

					Assert("There should be a record", reader.Read());
					AssertEquals(2, reader.GetInt32(0));
					AssertEquals("JPARI", reader.GetString(1));

					Assert("There should be a record", reader.Read());
					AssertEquals(1, reader.GetInt32(0));
					AssertEquals("TWKEL", reader.GetString(1));

					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestGroupByCreateUser()
		{
			var reportSql = @"select JobCount, CreateUserCode from Report_VolumeRanking(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @GroupingType, @SortingType, @BrokerStaffs, @Top)";
			CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, organisation1MainAddressPK, 1);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "IMP", "AIR", "ABC", "ABC", 3, "C2", Guid.Empty, organisation1MainAddressPK, 2);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "SEA", "ABC", "ABC", 5, "C3X", Guid.Empty, organisation1MainAddressPK, 3);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "AIR", "DEF", "ABC", 7, "C3M", Guid.Empty, organisation2MainAddressPK, 4);
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "DEF", "ABC", 9, "C1", Guid.Empty, organisation2MainAddressPK, 5);
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "DEF", "ABC", 1, "C1", Guid.Empty, organisation3MainAddressPK, 6);

			CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "DEF", "ABC", "04", organisation2MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "DEF", "ABC", "04", organisation3MainAddressPK);

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Create User");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(5, reader.GetInt32(0));
					AssertEquals("DEF", reader.GetString(1));

					Assert("There should be a record", reader.Read());
					AssertEquals(4, reader.GetInt32(0));
					AssertEquals("ABC", reader.GetString(1));

					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		public void TestGroupByLastEditUser()
		{
			var reportSql = @"select JobCount, LastEditUserCode from Report_VolumeRanking(@companyPK, @ShipmentType, @TransportMode, @EntryFromDate, @EntryToDate, @GroupingType, @SortingType, @BrokerStaffs, @Top)";
			CreateTestJobDeclaration(new DateTime(2019, 12, 11), "IMP", "SEA", "ABC", "ABC", 1, "C1", Guid.Empty, organisation1MainAddressPK, 1);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "IMP", "AIR", "ABC", "ABC", 3, "C2", Guid.Empty, organisation1MainAddressPK, 2);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "SEA", "ABC", "ABC", 5, "C3X", Guid.Empty, organisation1MainAddressPK, 3);
			CreateTestJobDeclaration(new DateTime(2019, 12, 12), "EXP", "AIR", "ABC", "DEF", 7, "C3M", Guid.Empty, organisation2MainAddressPK, 4);
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "DEF", 9, "C1", Guid.Empty, organisation2MainAddressPK, 5);
			CreateTestJobDeclaration(new DateTime(2019, 12, 13), "EXP", "AIR", "ABC", "DEF", 1, "C1", Guid.Empty, organisation3MainAddressPK, 6);

			CreateCusInBondHeader(new DateTime(2019, 12, 12), "ABC", "ABC", "01", organisation1MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "DEF", "04", organisation2MainAddressPK);
			CreateCusInBondHeader(new DateTime(2019, 12, 13), "ABC", "DEF", "04", organisation3MainAddressPK);

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@ShipmentType", SqlDbType.VarChar, "ALL");
				command.AddParameter("@TransportMode", SqlDbType.VarChar, "ALL");
				command.AddParameter("@EntryFromDate", SqlDbType.SmallDateTime, new DateTime(2019, 1, 1));
				command.AddParameter("@EntryToDate", SqlDbType.SmallDateTime, new DateTime(2019, 12, 31));
				command.AddParameter("@GroupingType", SqlDbType.VarChar, "Last Edit User");
				command.AddParameter("@SortingType", SqlDbType.VarChar, "Volume of Entries");
				command.AddParameter("@BrokerStaffs", SqlDbType.VarChar, "");
				command.AddParameter("@Top", SqlDbType.Int, 999);

				using (var reader = command.ExecuteReader())
				{
					Assert("There should be a record", reader.Read());
					AssertEquals(5, reader.GetInt32(0));
					AssertEquals("DEF", reader.GetString(1));

					Assert("There should be a record", reader.Read());
					AssertEquals(4, reader.GetInt32(0));
					AssertEquals("ABC", reader.GetString(1));

					Assert("There should be no other records", !reader.Read());
				}
			}
		}

		Guid CreateCusInBondHeader(DateTime createDate, string createUser, string lastEditUser, string transportType, Guid importerAddress, string portOfOrigin = "", string officeOfReceipt = "")
		{
			var cusInBondHeaderPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusInBondHeader (BH_PK, BH_ETA, BH_GB, BH_SystemCreateUser, BH_SystemLastEditUser, BH_ImportTransportMode, BH_ApplicationCode, BH_OA_Importer, BH_RL_NKImportLoadPort, BH_SystemCreateTimeUtc, BH_SystemLastEditTimeUtc)
VALUES (@cusInBondHeaderPK, @createDate, @branchPK, @createUser, @lastEditUser, @transportType, @applicationCode, @importerAddress, @portOfOrigin, GetUtcDate(), GetUtcDate())
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@cusInBondHeaderPK", SqlDbType.UniqueIdentifier, cusInBondHeaderPK);
				command.AddParameter("@createDate", SqlDbType.SmallDateTime, createDate);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@createUser", SqlDbType.VarChar, CusInBondHeaderSchema.BH_SystemCreateUser.MaxLength, createUser);
				command.AddParameter("@lastEditUser", SqlDbType.VarChar, CusInBondHeaderSchema.BH_SystemLastEditUser.MaxLength, lastEditUser);
				command.AddParameter("@transportType", SqlDbType.VarChar, CusInBondHeaderSchema.BH_ImportTransportMode.MaxLength, transportType);
				command.AddParameter("@applicationCode", SqlDbType.VarChar, CusInBondHeaderSchema.BH_ApplicationCode.MaxLength, "TW");
				command.AddParameter("@importerAddress", SqlDbType.UniqueIdentifier, importerAddress);
				command.AddParameter("@portOfOrigin", SqlDbType.VarChar, portOfOrigin);
				command.ExecuteNonQuery();
			}

			if (!string.IsNullOrEmpty(officeOfReceipt))
			{
				CreateGenAddOnColumn(cusInBondHeaderPK, "BH", "STR", "ReceiptOffice", officeOfReceipt);
			}

			return cusInBondHeaderPK;
		}

		void CreateGenAddOnColumn(Guid parentID, string parentTableCode, string dataType, string name, string data)
		{
			var genAddOnColumnPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.GenAddOnColumn (XA_PK, XA_ParentID, XA_ParentTableCode, XA_Type, XA_Name, XA_Data)
VALUES(@genAddOnColumnPK, @parentID, @parentTableCode, @dataType, @name, @data)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@genAddOnColumnPK", SqlDbType.UniqueIdentifier, genAddOnColumnPK);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentID);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, parentTableCode);
				command.AddParameter("@dataType", SqlDbType.VarChar, dataType);
				command.AddParameter("@name", SqlDbType.VarChar, name);
				command.AddParameter("@data", SqlDbType.VarChar, data);
				command.ExecuteNonQuery();
			}
		}

		void CreateCusInBondMoveHeader(Guid inBondHeaderPK, string brokerCode = "", string declarationType = "", string exportLocation = "", string finalDestination = "")
		{
			var cusInBondMoveHeaderPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusInBondMoveHeader (BM_PK, BM_BH, BM_GS_NKCusAgent, BM_InBondEntryType, BM_PlaceOfLoading, BM_RL_NKForeignDestPort, BM_SystemCreateTimeUtc, BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser)
VALUES (@cusInBondMoveHeaderPK, @inBondHeaderPK, @brokerCode, @declarationType, @exportLocation, @finalDestination, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@cusInBondMoveHeaderPK", SqlDbType.UniqueIdentifier, cusInBondMoveHeaderPK);
				command.AddParameter("@inBondHeaderPK", SqlDbType.UniqueIdentifier, inBondHeaderPK);
				command.AddParameter("@brokerCode", SqlDbType.VarChar, brokerCode);
				command.AddParameter("@declarationType", SqlDbType.VarChar, declarationType);
				command.AddParameter("@exportLocation", SqlDbType.VarChar, exportLocation);
				command.AddParameter("@finalDestination", SqlDbType.VarChar, finalDestination);
				command.ExecuteNonQuery();
			}
		}

		void CreateImportCusInBondBill(Guid inBondHeaderPK, decimal grossWeight, string grossWeightUQ)
		{
			var cusInBondBillPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusInBondBill (B0_PK, B0_BH, B0_ShipmentType, B0_Weight, B0_WeightUQ, B0_SystemCreateTimeUtc, B0_SystemCreateUser, B0_SystemLastEditTimeUtc, B0_SystemLastEditUser)
VALUES (@cusInBondBillPK, @inBondHeaderPK, 'IMP', @grossWeight, @grossWeightUQ, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@cusInBondBillPK", SqlDbType.UniqueIdentifier, cusInBondBillPK);
				command.AddParameter("@inBondHeaderPK", SqlDbType.UniqueIdentifier, inBondHeaderPK);
				command.AddParameter("@grossWeight", SqlDbType.Decimal, grossWeight);
				command.AddParameter("@grossWeightUQ", SqlDbType.VarChar, grossWeightUQ);
				command.ExecuteNonQuery();
			}
		}

		Guid CreateTestJobDeclaration(DateTime createDate, string messageType, string transportMode, string createUser, string lastEditUser, int invoiceLineCount, string entryStatus, Guid supplierAddress, Guid importerAddress, int clusterKey, string brokerStaffCode = "", decimal grossWeight = 0, string grossWeightUQ = "", decimal netWeight = 0, string netWeightUQ = "", string declarationType = "", string exportLocation = "", string portOfOrigin = "", string finalDestination = "", string officeOfReceipt = "")
		{
			var declarationPK = CreateJobDeclaration(branchPK, companyPK, messageType, transportMode, createUser, lastEditUser, clusterKey, brokerStaffCode, grossWeight, grossWeightUQ, portOfOrigin, finalDestination);
			var entryInstructionPK = CreateCusEntryInstruction(declarationPK, createDate, clusterKey, declarationType, exportLocation, officeOfReceipt);
			var invoiceHeaderPK = CreateInvoiceHeader(declarationPK, clusterKey, netWeight, netWeightUQ);
			var entryHeaderPK = CreateCusEntryHeader(declarationPK, entryStatus, clusterKey);
			var entryLinePK = CreateCusEntryLine(entryHeaderPK, clusterKey);

			for (int i = 0; i < invoiceLineCount; i++)
			{
				CreateInvoiceLine(invoiceHeaderPK, entryInstructionPK, entryLinePK, clusterKey);
			}

			if (supplierAddress != Guid.Empty)
			{
				TestDataCreator.CreateDocAddress(supplierAddress, string.Empty, declarationPK, "JE", "SUD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, false);
			}

			if (importerAddress != Guid.Empty)
			{
				TestDataCreator.CreateDocAddress(importerAddress, string.Empty, declarationPK, "JE", "IMD", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, false);
			}

			return declarationPK;
		}

		Guid CreateJobDeclaration(Guid branchPK, Guid companyPK, string messageType, string transportMode, string createUser, string lastEditUser, int clusterKey, string brokerStaff, decimal grossWeight, string grossWeightUQ, string portOfOrigin, string finalDestination, string declarationReference = "")
		{
			var declarationPK = Guid.NewGuid();
			var declarationRef = string.IsNullOrEmpty(declarationReference) ? Guid.NewGuid().ToString("n") : declarationReference;
			var sql = @"
INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_TransportMode, JE_SystemCreateUser, JE_SystemLastEditUser, JE_ClusterKey, JE_GS_NKCusAgent, JE_TotalWeight, JE_TotalWeightUnit, JE_RL_NKOrigin, JE_RL_NKFinalDestination, JE_DeclarationReference)
VALUES (@declarationPK, 'TW', @messageType, @branchPK, @companyPK, @transportMode, @createUser, @lastEditUser, @clusterKey, @brokerStaff, @grossWeight, @grossWeightUQ, @portOfOrigin, @finalDestination, @declarationReference)
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
				command.AddParameter("@brokerStaff", SqlDbType.VarChar, brokerStaff);
				command.AddParameter("@grossWeight", SqlDbType.Decimal, grossWeight);
				command.AddParameter("@grossWeightUQ", SqlDbType.VarChar, grossWeightUQ);
				command.AddParameter("@portOfOrigin", SqlDbType.VarChar, portOfOrigin);
				command.AddParameter("@finalDestination", SqlDbType.VarChar, finalDestination);
				command.AddParameter("@declarationReference", SqlDbType.VarChar, declarationRef);
				command.ExecuteNonQuery();
			}
			return declarationPK;
		}

		Guid CreateCusEntryInstruction(Guid declarationPK, DateTime dateForDuty, int clusterKey, string declarationType, string exportLocation, string officeOfReceipt)
		{
			var entryInstructionPK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_JE, CEI_DateForDuty, CEI_ClusterKey, CEI_Style, CEI_AddInfo, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser)
VALUES (@entryInstructionPK, 'TW', @declarationPK, @dateForDuty, @clusterKey, @declarationType, @addInfo, getutcdate(), '~BP', getutcdate(), '~BP')
";

			var sl = new List<string>();
			if (!string.IsNullOrEmpty(exportLocation))
			{
				sl.Add($"GoodsLocation={exportLocation}");
			}

			if (!string.IsNullOrEmpty(officeOfReceipt))
			{
				sl.Add($"CustomsOffice={officeOfReceipt}");
			}

			var addInfoStr = "";
			foreach (var str in sl)
			{
				if (!string.IsNullOrEmpty(addInfoStr))
				{
					addInfoStr += "*";
				}
				addInfoStr += str;
			}

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@entryInstructionPK", SqlDbType.UniqueIdentifier, entryInstructionPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@dateForDuty", SqlDbType.SmallDateTime, CusEntryInstructionSchema.CEI_DateForDuty.MaxLength, dateForDuty);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@declarationType", SqlDbType.VarChar, declarationType);
				command.AddParameter("@addInfo", SqlDbType.VarChar, addInfoStr);
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

		Guid CreateInvoiceHeader(Guid declarationPK, int clusterKey, decimal netWeight, string netWeightUQ)
		{
			var invoiceLinePK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobComInvoiceHeader (JZ_PK, JZ_DataModel, JZ_JE, JZ_ClusterKey, JZ_NetWeight, JZ_NetWeightUQ)
VALUES (@invoiceLinePK, 'TW', @declarationPK, @clusterKey, @netWeight, @netWeightUQ)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@netWeight", SqlDbType.Decimal, netWeight);
				command.AddParameter("@netWeightUQ", SqlDbType.VarChar, netWeightUQ);
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
