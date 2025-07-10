using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USExportDeclarationsWithMainData))]
	class USExportDeclarationsWithMainDataTest : DbCreateScriptTest
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
			var decPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, Guid.NewGuid().ToString("n"), "EXP", 1, dataModel: "US", addInfo: "SchDArrival=0000");
			var decPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, Guid.NewGuid().ToString("n"), "EXP", 2, dataModel: "US", addInfo: "SchDArrival=9999");

			var result = new Dictionary<Guid, string>();
			const string sql = @"select JE_PK, DischargePortDescription from dbo.USExportDeclarationsWithMainData(@companyPK, '', '')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add((Guid)reader["JE_PK"], reader.IsDBNull(1) ? "" : reader.GetString(1));
					}
				}
			}
			AssertEquals("", result[decPK1]);
			AssertEquals("TEST PORT 9999", result[decPK2]);
		}

		[ExpectNoExceptions]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1122:DoNotUseDateTimeParse", Justification = "Testing")]
		public void TestUSExportDeclarationsWithMainData()
		{
			var supplierOrgPK = TestDataCreator.CreateOrganisation("Supplier", "Test Supplier", "USPHL");
			var supplierAddressPK = TestDataCreator.CreateAddress(supplierOrgPK, "Supplier Address", "Address 1");
			TestDataCreator.CreateOrgAddressCapability(supplierAddressPK, "OFC", true);
			TestDataCreator.CreateOrgCusCode(supplierOrgPK, supplierAddressPK, "DUN", "758516744", "US");

			var companyPK = TestDataCreator.CreateCompany("DUS", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "PHL", "USPHL");

			var additionalInfoHelper = new AdditionalInfoHelper(new[]
			{
				new AdditionalInfoConfig("InbondType", "70"),
				new AdditionalInfoConfig("SchDLoading", "3005"),
				new AdditionalInfoConfig("SchDArrival", "20105"),
				new AdditionalInfoConfig("SchDExport", "5301"),
				new AdditionalInfoConfig("RN_NKCountryOfDestination", "MX"),
				new AdditionalInfoConfig("DateOfExport", new DateTime(2021, 2, 26, 6, 4, 5)),
				new AdditionalInfoConfig("TransportReference", "NJ34G")
			});

			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0000001", "EXP", 1, null, supplierPK: supplierOrgPK,
				addInfo: additionalInfoHelper.AdditionalInfoText, dataModel: "US");

			var script = $@"SELECT * FROM USExportDeclarationsWithMainData('{companyPK}','','')";
			using (var command = TestConnection.Command(script))
			using (var reader = command.ExecuteReader())
			{
				reader.Read();
				CombineAssertions(() =>
				{
					AssertEquals("B0000001", reader["JE_DeclarationReference"].ToString());
					AssertEquals("DUN: 758516744", reader["MainSupplierIDNumber"].ToString());
					AssertEquals("70", reader["US_InbondType"].ToString());
					AssertEquals("3005", reader["US_SchDLoading"].ToString());
					AssertEquals("20105", reader["US_SchDArrival"].ToString());
					AssertEquals("5301", reader["US_SchDExport"].ToString());
					AssertEquals("MX", reader["US_RN_NKCountryOfDestination"].ToString());
					AssertEquals(new DateTime(2021, 02, 26, 6, 4, 5), DateTime.Parse(reader["US_DateOfExport"].ToString()));  // Hardcoded column name in function result
					AssertEquals("NJ34G", reader["US_TransportReference"].ToString());
				});
			}
		}

		public void TestOrgCusCode()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var supplierOrgPK = TestDataCreator.CreateOrganisation("ORG2", "Organization2");
			TestDataCreator.CreateOrgCusCode(supplierOrgPK, "EIN", "EIN_CODE 2", "US");
			TestDataCreator.CreateOrgCusCode(supplierOrgPK, "DUN", "DUN_CODE 2", "US");
			var forwarderOrgPK = TestDataCreator.CreateOrganisation("ORG3", "Organization3");
			TestDataCreator.CreateOrgCusCode(forwarderOrgPK, "EIN", "EIN_CODE 3", "US");
			TestDataCreator.CreateOrgCusCode(forwarderOrgPK, "DUN", "DUN_CODE 3", "US");
			var mainDeclaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B000010", "EXP", 1, supplierPK: supplierOrgPK, forwarderPK: forwarderOrgPK, dataModel: "US");

			var reportSql = $"SELECT MainSupplierIDNumber, ForwarderIDNumber FROM USExportDeclarationsWithMainData('{companyPK}', null, null)";

			TestConnection.ExecuteReader(
				reportSql,
				(reader) =>
				{
					AssertEquals("EIN: EIN_CODE 2", (string)reader["MainSupplierIDNumber"]);
					AssertEquals("EIN: EIN_CODE 3", (string)reader["ForwarderIDNumber"]);
				}
			);
		}

		public void TestSystemCreateTimeUtc()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var utcNow = DateTime.UtcNow;

			var declarationPk1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test001", "EXP", "SEA", "Calypso", "0308", DateTime.Now, 1, createTime: utcNow, dataModel: "US");
			var declarationPk2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test002", "EXP", "SEA", "Calypso", "0308", DateTime.Now, 2, createTime: utcNow.AddMinutes(2), dataModel: "US");
			var declarationPk3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "Test003", "EXP", "SEA", "Calypso", "0308", DateTime.Now, 3, createTime: utcNow.AddMinutes(-2), dataModel: "US");

			var reportSql = @"SELECT JE_PK FROM USExportDeclarationsWithMainData(@companyPK, @dateFrom, @dateTo)";
			using (var command = Db.Connection.Command(reportSql))
			{
				var retList = new List<Guid>();
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@dateFrom", SqlDbType.SmallDateTime, utcNow);
				command.AddParameter("@dateTo", SqlDbType.SmallDateTime, utcNow.AddMinutes(1));

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						retList.Add((Guid)reader["JE_PK"]);
					}
				}
				AssertEquals(1, retList.Count);
				AssertEquals(declarationPk1, retList[0]);
			}
		}
	}
}
