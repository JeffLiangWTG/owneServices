using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US.RefDb;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US.RefDb
{
	[TestedType(typeof(vw_RefDbEntUs_USCCarrierAndFIRMS))]
	class vw_RefDbEntUs_USCCarrierAndFIRMSTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			string testName = TestConnection.ExecuteScalar("SELECT TOP 1 US_Name FROM " + ScriptToTest.Name).ToString();
			AssertEquals("US_Name empty?", false, string.IsNullOrEmpty(testName));
		}

		public void TestTypeFromUSCCarrierAndFIRMS()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");

			var carrierRecordPK = Guid.NewGuid();
			var firmRecordPK = Guid.NewGuid();

			var declarationSql = @"INSERT INTO dbo.USCarrierCombined (UI_PK, UI_Code, UI_Name, UI_ModeOfTransportation, UI_Address, UI_AirwayBillPrefix, UI_IsSystem)
VALUES (@carrierRecordPK, 'LTAN', 'LASER TRANSPORT', '30', '3380 WHEELTON DR WINDSOR                      ON 857       CA', '', 1)";

			using (var command = Db.Connection.Command(declarationSql))
			{
				command.AddParameter("@carrierRecordPK", SqlDbType.UniqueIdentifier, carrierRecordPK);
				command.ExecuteNonQuery();
			}

			TestDataCreator.CreateCusEntryNum(carrierRecordPK, "JobDeclaration", "EntryNum1", "ENS", "CUS", "US");

			var invoiceLineSql = @"
INSERT INTO RefDbEntUS_USCFIRMS(US_PK, US_Code, US_Name, US_City, US_State, US_ZipCode, US_Country, US_FacilityType, Us_DistrictPortCode, US_IsActive, US_LastUpdate, US_Address)
VALUES (@firmRecordPK, 'Q9', 'NAME FIELD', 'SYD', 'LA', '7035407', 'US', '03', '2001', 0, '2015-06-17 00:00:00.000', '7908 PO BOX')
";
			using (var command = Db.Connection.Command(invoiceLineSql))
			{
				command.AddParameter("@firmRecordPK", SqlDbType.UniqueIdentifier, firmRecordPK);
				command.ExecuteNonQuery();
			}

			var reportSql = @"select US_PK, US_Type from dbo.USCCarrierAndFIRMS";
			using (var command = Db.Connection.Command(reportSql))
			{
				using (var reader = command.ExecuteReader())
				{
					var invoices = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						invoices[reader.GetGuid(0)] = reader.GetString(1);
					}

					AssertEquals("Should be Carrier", "CARRIER", invoices[carrierRecordPK]);
					AssertEquals("Should be Firms", "FIRMS", invoices[firmRecordPK]);
				}
			}
		}
	}
}
