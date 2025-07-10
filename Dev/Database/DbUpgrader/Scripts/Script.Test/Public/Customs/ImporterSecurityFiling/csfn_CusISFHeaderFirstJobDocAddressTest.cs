using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ImporterSecurityFiling;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ImporterSecurityFiling
{
	[TestedType(typeof(csfn_CusISFHeaderFirstJobDocAddress))]
	class csfn_CusISFHeaderFirstJobDocAddressTest : DbCreateScriptTest
	{
		public void TestFilterOrgCusCodeWithAddress() 
		{
			var companyPK = Guid.NewGuid();
			var branchPK = Guid.NewGuid();
			var isfPK = Guid.NewGuid();
			var orgPK = Guid.NewGuid();
			var address01 = Guid.NewGuid();
			var address02 = Guid.NewGuid();
			var address03 = Guid.NewGuid();
			var address04 = Guid.NewGuid();
			var address05 = Guid.NewGuid();
			var address06 = Guid.NewGuid();
			var address07 = Guid.NewGuid();

			var inertSQL = $@"
			INSERT INTO dbo.GlbCompany(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES('{companyPK}', 'US', 'USD', 'USC', 'US company')
			INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode) VALUES('{branchPK}', '{companyPK}', 'USB', 'US')
			
			INSERT INTO dbo.CusISFHeader(BF_PK, BF_ActionReasonCode, BF_EntryType, BF_GB, BF_ShipmentType, BF_TransportMode, BF_SystemCreateTimeUTC, BF_JobReference)
			VALUES('{isfPK}', 'CT', '1', '{branchPK}', '01', '11', '2019-05-10', 'ISF000001')
			
			INSERT INTO dbo.OrgHeader(OH_PK, OH_Code) VALUES('{orgPK}', 'TST01')
			INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Code, OA_Address1)
			VALUES('{address01}', '{orgPK}', 'ADD1', 'Address 1')
				, ('{address02}', '{orgPK}', 'ADD2', 'Address 2')
				, ('{address03}', '{orgPK}', 'ADD3', 'Address 3')
				, ('{address04}', '{orgPK}', 'ADD4', 'Address 4')
				, ('{address05}', '{orgPK}', 'ADD5', 'Address 5')
				, ('{address06}', '{orgPK}', 'ADD6', 'Address 6')
				, ('{address07}', '{orgPK}', 'ADD7', 'Address 7')

			INSERT INTO dbo.JobDocAddress(E2_PK, E2_OA_Address, E2_ParentID, E2_ParentTableCode, E2_AddressSequence, E2_AddressType)
			VALUES(NEWID(), '{address01}', '{isfPK}', 'BF', 0, 'STP')
				, (NEWID(), '{address02}', '{isfPK}', 'BF', 0, 'SEP')
				, (NEWID(), '{address03}', '{isfPK}', 'BF', 0, 'BYP')
				, (NEWID(), '{address04}', '{isfPK}', 'BF', 0, 'CSL')
				, (NEWID(), '{address05}', '{isfPK}', 'BF', 0, 'CON')
				, (NEWID(), '{address06}', '{isfPK}', 'BF', 0, 'BKD')
				, (NEWID(), '{address07}', '{isfPK}', 'BF', 0, '')

			INSERT INTO dbo.OrgCusCode(OK_PK, OK_OH, OK_OA_PremisesAddress, OK_CodeType, OK_CustomsRegNo, OK_RN_NKCodeCountry)
			VALUES(NEWID(), '{orgPK}', '{address02}', 'DN4', 'DN401', 'US')
				, (NEWID(), '{orgPK}', '{address03}', 'DUN', 'DUN01', 'US')
				, (NEWID(), '{orgPK}', '{address04}', 'EIN', 'EIN01', 'US')
				, (NEWID(), '{orgPK}', '{address05}', 'SSN', 'SSN01', 'US')
				, (NEWID(), '{orgPK}', '{address06}', 'CBN', 'CBN01', 'US')";

			using (var command = TestConnection.Command(inertSQL))
			{
				command.ExecuteScalar();
			}

			var searchSQL = "SELECT * FROM csfn_CusISFHeaderFirstJobDocAddress('STP')";
			using (var command = TestConnection.Command(searchSQL))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						AssertEquals("", reader["E2_GovRegNum"]);
					}
				}
			}

			searchSQL = "SELECT * FROM csfn_CusISFHeaderFirstJobDocAddress('BKD')";
			using (var command = TestConnection.Command(searchSQL))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						AssertEquals("", reader["E2_GovRegNum"]);
					}
				}
			}

			inertSQL = $@"INSERT INTO dbo.OrgCusCode(OK_PK, OK_OH, OK_OA_PremisesAddress, OK_CodeType, OK_CustomsRegNo, OK_RN_NKCodeCountry)
			VALUES(NEWID(), '{orgPK}', NULL, 'DUN', 'DUN01', 'US')";

			using (var command = TestConnection.Command(inertSQL))
			{
				command.ExecuteScalar();
			}

			using (var command = TestConnection.Command(searchSQL))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						AssertEquals("DUN01", reader["E2_GovRegNum"]);
					}
				}
			}
		}
	}
}
