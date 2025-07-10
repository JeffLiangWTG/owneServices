using System;
using Enterprise.DbUpgrader.Data.BaseData.Common;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.BaseData.Organisation
{
	sealed class OrganisationUpgradeTaskTest : TransactionedTestCase
	{
		public void TestRun()
		{
			// Prepare test data
			Guid newOrgPk = Guid.NewGuid();
			Guid newAddrPk = Guid.NewGuid();
			Guid newPzPk = Guid.NewGuid();
			Guid newPuPk = Guid.NewGuid();
			Guid existingOrgPk = new Guid("6034C9C6-A8D0-4D07-B85D-14F73C1A8FB9");

			string sqlText = String.Format(@"
				-- OrgHeader
				INSERT dbo.OrgHeader (OH_PK, OH_Code, OH_FullName) VALUES ('{0}', '~NewCode', '~NewName');
				UPDATE dbo.OrgHeader SET OH_FullName = 'Some~Name~Not~To~Be~Changed' WHERE OH_PK = '{4}';
				-- Remove PatternMatching Constraints
				DELETE dbo.PatternMatchingAddress WHERE PMA_OH = (SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'MISC');
				DELETE dbo.PatternMatchingName WHERE PMN_OH = (SELECT OH_PK FROM dbo.OrgHeader WHERE OH_Code = 'MISC');
				DELETE dbo.OrgHeader WHERE OH_Code = 'MISC';
				-- OrgAddress
				INSERT dbo.OrgAddress (OA_PK, OA_OH, OA_Code, OA_Address1) VALUES ('{1}', '{0}', '~NewCode', '~NewAddress');
				UPDATE dbo.OrgAddress SET OA_Code = '~NC' WHERE OA_Code = 'OFC: NO ADDRESS SPECIFIED';
				-- OrgAddressCapability
				INSERT dbo.OrgAddressCapability (PZ_PK, PZ_OA, PZ_AddressType) VALUES ('{2}', '{1}', '~TP');
				UPDATE dbo.OrgAddressCapability SET PZ_AddressType = '~CR' WHERE PZ_AddressType = 'OFC';
				-- OrgWebURL
				INSERT dbo.OrgWebURL (PU_PK, PU_URL, PU_Type, PU_OH) VALUES ('{3}', 'newurl', 'MAI', '{0}');
				DELETE dbo.OrgWebURL WHERE PU_OH = '{4}' AND PU_Type = 'MAI';",
				newOrgPk.ToString(),
				newAddrPk.ToString(),
				newPzPk.ToString(),
				newPuPk.ToString(),
				existingOrgPk.ToString());
			TestConnection.ExecuteNonQuery(sqlText);

			Guid miscOrgPk = new Guid("79DE2ECD-1BB0-40FE-A02F-F3C16F9C3686");
			Guid miscOrgAddrPk = new Guid("E14EA2DE-BE05-4CC1-ADA0-0915FE01EC46");

			// OrgHeader
			AssertEquals("[BEFORE] OrgHeader (~NewCode) in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, OrgHeaderSchema.PK, newOrgPk));
			AssertEquals("[BEFORE] OrgHeader (MISC) in database?", false, OrgHeaderInDatabase("MISC"));
			// OrgAddress
			AssertEquals("[BEFORE] OrgAddress (~NewCode) in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, OrgAddressSchema.PK, newAddrPk));
			AssertEquals("[BEFORE] OrgAddress (OFC: NO ADDRESS SPECIFIED) in database?", false, OrgAddressInDatabase("OFC: NO ADDRESS SPECIFIED"));
			AssertEquals("[BEFORE] OrgAddress (OFC: NO ADDRESS SPECIFIED) in database?", true, OrgAddressInDatabase("~NC"));
			// OrgAddressCapability
			AssertEquals("[BEFORE] OrgAddressCapability (~TP) in database?", true, OrgAddressCapabilityInDatabase(newAddrPk, "~TP"));
			AssertEquals("[BEFORE] OrgAddressCapability (OFC) in database?", false, OrgAddressCapabilityInDatabase("OFC"));
			// OrgWebURL
			AssertEquals("[BEFORE] OrgWebURL (Org:~NewCode, Type:MAI) in database?", true, OrgWebURLInDatabase(newOrgPk, "MAI"));
			AssertEquals("[BEFORE] OrgWebURL (Org:DEMORG, Type:MAI) in database?", false, OrgWebURLInDatabase(existingOrgPk, "MAI"));

			OrganisationUpgradeTask testTask = new OrganisationUpgradeTask();
			testTask.Run();

			// Assert results

			// OrgHeader
			AssertEquals("OrgHeader (~NewCode) in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, OrgHeaderSchema.PK, newOrgPk));
			AssertEquals("OrgHeader (MISC) in database?", true, OrgHeaderInDatabase(miscOrgPk, "MISC"));
			AssertEquals("Existing OrgHeader should NOT change", "Some~Name~Not~To~Be~Changed", OrgHeaderName(existingOrgPk));
			// OrgAddress
			AssertEquals("OrgAddress (~NewCode) in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, OrgAddressSchema.PK, newAddrPk));
			AssertEquals("OrgAddress (OFC: NO ADDRESS SPECIFIED) in database?", true, OrgAddressInDatabase(miscOrgPk, "OFC: NO ADDRESS SPECIFIED"));
			// OrgAddressCapability
			AssertEquals("OrgAddressCapability (~TP) in database?", true, OrgAddressCapabilityInDatabase(newAddrPk, "~TP"));
			AssertEquals("OrgAddressCapability (OFC) in database?", true, OrgAddressCapabilityInDatabase(miscOrgAddrPk, "OFC"));
			// OrgWebURL
			AssertEquals("OrgWebURL (Org:~NewCode, Type:MAI) in database?", true, OrgWebURLInDatabase(newOrgPk, "MAI"));
			AssertEquals("OrgWebURL (Org:DEMORG, Type:MAI) in database?", true, OrgWebURLInDatabase(existingOrgPk, "MAI"));
		}

		string OrgHeaderName(Guid orgPk)
		{
			string sqlText = String.Format("SELECT OH_FullName FROM dbo.OrgHeader WHERE OH_PK = '{0}'", orgPk.ToString());
			object objResult = TestConnection.ExecuteScalar(sqlText);
			return (objResult == null || objResult == DBNull.Value) ? null : objResult.ToString();
		}

		bool OrgHeaderInDatabase(string orgCode)
		{
			return BaseDataUpgradeTask.IsRecordInDatabase(String.Format("SELECT TOP 1 OH_Code FROM dbo.OrgHeader WHERE OH_Code = '{0}'", orgCode));
		}

		bool OrgHeaderInDatabase(Guid orgPk, string orgCode)
		{
			return BaseDataUpgradeTask.IsRecordInDatabase(String.Format("SELECT TOP 1 OH_Code FROM dbo.OrgHeader WHERE OH_PK = '{0}' AND OH_Code = '{1}'", orgPk.ToString(), orgCode));
		}

		bool OrgAddressInDatabase(string addrCode)
		{
			return BaseDataUpgradeTask.IsRecordInDatabase(String.Format("SELECT TOP 1 OA_Code FROM dbo.OrgAddress WHERE OA_Code = '{0}'", addrCode));
		}

		bool OrgAddressInDatabase(Guid orgPk, string addrCode)
		{
			return BaseDataUpgradeTask.IsRecordInDatabase(String.Format("SELECT TOP 1 OA_Code FROM dbo.OrgAddress WHERE OA_OH = '{0}' AND OA_Code = '{1}'", orgPk.ToString(), addrCode));
		}

		bool OrgAddressCapabilityInDatabase(Guid addrPk, string addrType)
		{
			return BaseDataUpgradeTask.IsRecordInDatabase(String.Format("SELECT TOP 1 PZ_AddressType FROM dbo.OrgAddressCapability WHERE PZ_OA = '{0}' AND PZ_AddressType = '{1}'", addrPk.ToString(), addrType));
		}

		bool OrgAddressCapabilityInDatabase(string addrType)
		{
			return BaseDataUpgradeTask.IsRecordInDatabase(String.Format("SELECT TOP 1 PZ_AddressType FROM dbo.OrgAddressCapability WHERE PZ_AddressType = '{0}'", addrType));
		}

		bool OrgWebURLInDatabase(Guid orgPk, string urlType)
		{
			return BaseDataUpgradeTask.IsRecordInDatabase(String.Format("SELECT TOP 1 PU_Type FROM dbo.OrgWebURL WHERE PU_OH = '{0}' AND PU_Type = '{1}'", orgPk.ToString(), urlType));
		}
	}
}
