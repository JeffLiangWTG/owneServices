using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MarketingManager;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MarketingManager.Testing
{
	[TestedType(typeof(ViewCampaignContact))]
	class ViewCampaignContactTest : DbCreateScriptTest
	{
		public void TestInquiryContactTimezone()
		{
			var inquiryContact = Guid.NewGuid();
			InsertOrgColdCallRegister(inquiryContact, "I90001000", "AU");

			var inquiryContact2 = Guid.NewGuid();
			InsertOrgColdCallRegister(inquiryContact2, "I90001001", "AUSYD");

			using (DbCommand command = TestConnection.Command("SELECT * FROM dbo.ViewCampaignContact WHERE VCC_PK = '" + inquiryContact.ToString() + "' OR VCC_PK = '" + inquiryContact2.ToString() + "' ORDER BY VCC_RelatedPortCode"))
			{
				DataTable result = new DataTable();
				result.Load(command.ExecuteReader());
				AssertEquals("Pre-condition: should return a result", 2, result.Rows.Count);
				AssertEquals("VCC_OffsetMinutesFromUtc for country", 0, Convert.ToInt32(result.Rows[0]["VCC_OffsetMinutesFromUtc"]));
				AssertEquals("VCC_OffsetMinutesFromUtc for port", 600, Convert.ToInt32(result.Rows[1]["VCC_OffsetMinutesFromUtc"]));
				AssertEquals("VCC_CivilianZone for country", string.Empty, result.Rows[0]["VCC_CivilianZone"].ToString());
				AssertEquals("VCC_CivilianZone for port", "EST", result.Rows[1]["VCC_CivilianZone"].ToString());
				AssertEquals("VCC_TimeZoneSetName for country", string.Empty, result.Rows[0]["VCC_TimeZoneSetName"].ToString());
				AssertEquals("VCC_TimeZoneSetName for port", "Australia/Sydney", result.Rows[1]["VCC_TimeZoneSetName"].ToString());
				AssertEquals("VCC_TableCode from SalesEnquiry", "O1", result.Rows[0]["VCC_TableCode"].ToString());
				AssertEquals("VCC_TableCode from SalesEnquiry", "O1", result.Rows[1]["VCC_TableCode"].ToString());
			}
		}

		public void TestViewCampaignContacts()
		{
			var orgContact = Guid.NewGuid();
			InsertOrgContacts(orgContact);

			var inquiryContact = Guid.NewGuid();
			InsertOrgColdCallRegister(inquiryContact, "I90001000", "AUSYD");

			InsertOrgAddress();

			using (DbCommand command = TestConnection.Command("SELECT * FROM dbo.ViewCampaignContact WHERE VCC_PK = '" + orgContact.ToString() + "' OR VCC_PK = '" + inquiryContact.ToString() + "' ORDER BY VCC_Email"))
			{
				DataTable result = new DataTable();
				result.Load(command.ExecuteReader());
				AssertEquals("Pre-condition: should return a result", 2, result.Rows.Count);
				AssertEquals("VCC_Email from dbo.OrgContact", "edward@wisegrid.com", result.Rows[0]["VCC_Email"].ToString());
				AssertEquals("VCC_Email from SalesEnquiry", "myInquiry@gmail.com", result.Rows[1]["VCC_Email"].ToString());
				AssertEquals("VCC_TableCode from dbo.OrgContact", "OC", result.Rows[0]["VCC_TableCode"].ToString());
				AssertEquals("VCC_TableCode from SalesEnquiry", "O1", result.Rows[1]["VCC_TableCode"].ToString());
				AssertEquals("VCC_OrgIsActive from dbo.OrgHeader", true, result.Rows[0]["VCC_OrgIsActive"]);
				AssertEquals("VCC_OrgIsActive from SalesEnquiry", false, result.Rows[1]["VCC_OrgIsActive"]);
			}
		}

		#region Implementation

		void InsertOrg(Guid pk, string code)
		{
			string query = string.Format("INSERT INTO {0} ({1}, {2}) VALUES ",
				OrgHeaderSchema.Constants.TableName,
				OrgHeaderSchema.PK.Name,
				OrgHeaderSchema.OH_Code.Name);

			using (DbCommand command = TestConnection.Command(query + "(@OH_PK, @OH_Code)"))
			{
				command.AddParameter("@OH_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@OH_Code", SqlDbType.VarChar, code);
				command.ExecuteNonQuery();
			}
		}

		void InsertOrgContacts(Guid pk)
		{
			orgPK = Guid.NewGuid();
			InsertOrg(orgPK, "PAS");

			string query = string.Format("INSERT INTO {0} ({1}, {2}, {3}) VALUES ",
				OrgContactSchema.Constants.TableName,
				OrgContactSchema.PK.Name,
				OrgContactSchema.OC_Email.Name,
				OrgContactSchema.OC_OH.Name);

			using (DbCommand command = TestConnection.Command(query + "(@OC_PK, @OC_Email, @OC_OH)"))
			{
				command.AddParameter("@OC_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@OC_Email", SqlDbType.VarChar, "edward@wisegrid.com");
				command.AddParameter("@OC_OH", SqlDbType.UniqueIdentifier, orgPK);
				command.ExecuteNonQuery();
			}
		}

		void InsertOrgAddress()
		{
			Guid oaPK = Guid.NewGuid();
			string addressQuery = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}) VALUES ",
				OrgAddressSchema.Constants.TableName,
				OrgAddressSchema.PK.Name,
				OrgAddressSchema.OA_Code.Name,
				OrgAddressSchema.OA_OH.Name,
				OrgAddressSchema.OA_Address1.Name);
			using (DbCommand command = TestConnection.Command(addressQuery + "(@OA_PK, @OA_Code, @OA_OH, @OA_Address1)"))
			{
				command.AddParameter("@OA_PK", SqlDbType.UniqueIdentifier, oaPK);
				command.AddParameter("@OA_Code", SqlDbType.VarChar, "SYD");
				command.AddParameter("@OA_OH", SqlDbType.UniqueIdentifier, orgPK);
				command.AddParameter("@OA_Address1", SqlDbType.VarChar, "O'Riordan Street");
				command.ExecuteNonQuery();
			}

			string capabilityQuery = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}) VALUES ",
				OrgAddressCapabilitySchema.Constants.TableName,
				OrgAddressCapabilitySchema.PK.Name,
				OrgAddressCapabilitySchema.PZ_AddressType.Name,
				OrgAddressCapabilitySchema.PZ_IsMainAddress.Name,
				OrgAddressCapabilitySchema.PZ_OA.Name);
			using (DbCommand command = TestConnection.Command(capabilityQuery + "(@PZ_PK, @PZ_AddressType, @PZ_IsMainAddress, @PZ_OA)"))
			{
				command.AddParameter("@PZ_PK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@PZ_AddressType", SqlDbType.VarChar, "OFC");
				command.AddParameter("@PZ_IsMainAddress", SqlDbType.Bit, 1);
				command.AddParameter("@PZ_OA", SqlDbType.UniqueIdentifier, oaPK);
				command.ExecuteNonQuery();
			}
		}

		void InsertOrgColdCallRegister(Guid pk, string leadUniqueReference, string portOrCountry)
		{
			string query = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}) VALUES ",
				OrgColdCallRegisterSchema.Constants.TableName,
				OrgColdCallRegisterSchema.PK.Name,
				OrgColdCallRegisterSchema.O1_LeadUniqueReference.Name,
				OrgColdCallRegisterSchema.O1_Email.Name,
				OrgColdCallRegisterSchema.O1_PortOrCountry.Name,
				OrgColdCallRegisterSchema.O1_SystemCreateTimeUtc.Name,
				OrgColdCallRegisterSchema.O1_SystemCreateUser.Name,
				OrgColdCallRegisterSchema.O1_SystemLastEditTimeUtc.Name,
				OrgColdCallRegisterSchema.O1_SystemLastEditUser.Name);

			using (DbCommand command = TestConnection.Command(query + "(@O1_PK, @O1_LeadUniqueReference, @O1_Email, @O1_PortOrCountry, GetUtcDate(), 'E', GetUtcDate(), 'E')"))
			{
				command.AddParameter("@O1_PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@O1_LeadUniqueReference", SqlDbType.VarChar, leadUniqueReference);
				command.AddParameter("@O1_Email", SqlDbType.VarChar, "myInquiry@gmail.com");
				command.AddParameter("@O1_PortOrCountry", SqlDbType.VarChar, portOrCountry);
				command.ExecuteNonQuery();
			}
		}

		Guid orgPK;
		#endregion
	}
}

