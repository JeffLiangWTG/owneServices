using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Cartage;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Cartage.Testing
{
	[TestedType(typeof(GetJobCartagesWithSecurityContext))]
	class GetJobCartagesWithSecurityContextTest : DbCreateScriptTest
	{
		public void TestReturnsJobCartageColumns()
		{
			var fromJobCartage = GetColumnNames("SELECT * FROM dbo.JobCartage");
			var fromTvf = GetColumnNames("SELECT * FROM GetJobCartagesWithSecurityContext('', '')");

			var missingColumns = fromJobCartage.Except(fromTvf);
			var errorMessage = $"The following columns are missing and should be added to the function: {string.Join(", ", missingColumns)}";

			Assert(errorMessage, !missingColumns.Any());
		}

		public void TestCartagesWithAllowedAddresses()
		{
			var companyPK = TestDataCreator.CreateCompany("CMP", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "BRN", "USORD");

			var org1PK = TestDataCreator.CreateOrganisation("ORG1", "Test Org1");
			var address1PK = TestDataCreator.CreateAddress(org1PK, "Address1", "Test Address1");
			var address2PK = TestDataCreator.CreateAddress(org1PK, "Address2", "Test Address2");

			var org2PK = TestDataCreator.CreateOrganisation("ORG2", "Test Org2");
			var address3PK = TestDataCreator.CreateAddress(org2PK, "Address3", "Test Address3");

			var cartageWithLCFPK = CreateCartageWithDocAddress(branchPK, address1PK, "LCF");
			var cartageWithLCTPK = CreateCartageWithDocAddress(branchPK, address2PK, "LCT");
			var cartageWithLCMPK = CreateCartageWithDocAddress(branchPK, address1PK, "LCM");
			var cartageWithLCYPK = CreateCartageWithDocAddress(branchPK, address2PK, "LCY");
			var cartageWithLCSPK = CreateCartageWithDocAddress(branchPK, address1PK, "LCS");
			var cartageWithLCEPK = CreateCartageWithDocAddress(branchPK, address2PK, "LCE");
			var cartageWithLCIPK = CreateCartageWithDocAddress(branchPK, address1PK, "LCI");

			var cartageOtherPK = TestDataCreator.CreateJobCartage(branchPK, "Consignment 1");
			TestDataCreator.CreateDocAddress(address3PK, "Cartage Address", cartageOtherPK, "JJ", "LCF");
			TestDataCreator.CreateDocAddress(address3PK, "Cartage Address", cartageOtherPK, "JJ", "LCT");
			TestDataCreator.CreateDocAddress(address3PK, "Cartage Address", cartageOtherPK, "JJ", "LCM");
			TestDataCreator.CreateDocAddress(address3PK, "Cartage Address", cartageOtherPK, "JJ", "LCY");
			TestDataCreator.CreateDocAddress(address3PK, "Cartage Address", cartageOtherPK, "JJ", "LCS");
			TestDataCreator.CreateDocAddress(address3PK, "Cartage Address", cartageOtherPK, "JJ", "LCE");
			TestDataCreator.CreateDocAddress(address3PK, "Cartage Address", cartageOtherPK, "JJ", "LCI");
			TestDataCreator.CreateDocAddress(address1PK, "Cartage Address", cartageOtherPK, "JJ", "OT1");
			TestDataCreator.CreateDocAddress(address2PK, "Cartage Address", cartageOtherPK, "JJ", "OT2");

			var relatedAddressesList = new[] { address1PK, address2PK };
			var cartages = GetJobCartagesWithSecurityContext(relatedAddressesList: relatedAddressesList);
			Assert(cartages.Contains(cartageWithLCFPK));
			Assert(cartages.Contains(cartageWithLCTPK));
			Assert(cartages.Contains(cartageWithLCMPK));
			Assert(cartages.Contains(cartageWithLCYPK));
			Assert(cartages.Contains(cartageWithLCSPK));
			Assert(cartages.Contains(cartageWithLCEPK));
			Assert(cartages.Contains(cartageWithLCIPK));
			AssertEquals(7, cartages.Count());
		}

		Guid CreateCartageWithDocAddress(Guid branchPK, Guid addressPK, string addressType)
		{
			var cartagePK = TestDataCreator.CreateJobCartage(branchPK, $"Consignment {addressType}{++cartageCounter}");
			TestDataCreator.CreateDocAddress(addressPK, $"Cartage {addressType} Address", cartagePK, "JJ", addressType);

			return cartagePK;
		}

		public void TestCartagesWithJobHeaders()
		{
			var companyPK = TestDataCreator.CreateCompany("CMP", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "BRN", "USORD");
			var departmentPK = TestDataCreator.CreateDepartment("DPT");

			var org1PK = TestDataCreator.CreateOrganisation("ORG1", "Test Org1");
			var address1PK = TestDataCreator.CreateAddress(org1PK, "Address1", "Test Address1");
			var address2PK = TestDataCreator.CreateAddress(org1PK, "Address2", "Test Address2");

			var org2PK = TestDataCreator.CreateOrganisation("ORG2", "Test Org2");
			var address3PK = TestDataCreator.CreateAddress(org2PK, "Address3", "Test Address3");

			var cartageAgentCollect1PK = CreateCartageWithJob("JH_OA_AgentCollectAddr", address1PK, branchPK, companyPK, departmentPK);
			var cartageAgentCollect2PK = CreateCartageWithJob("JH_OA_AgentCollectAddr", address2PK, branchPK, companyPK, departmentPK);
			CreateCartageWithJob("JH_OA_AgentCollectAddr", address3PK, branchPK, companyPK, departmentPK);

			var cartageLocalCharges1PK = CreateCartageWithJob("JH_OA_LocalChargesAddr", address1PK, branchPK, companyPK, departmentPK);
			var cartageLocalCharges2PK = CreateCartageWithJob("JH_OA_LocalChargesAddr", address2PK, branchPK, companyPK, departmentPK);
			CreateCartageWithJob("JH_OA_LocalChargesAddr", address3PK, branchPK, companyPK, departmentPK);

			var cartagePK = TestDataCreator.CreateJobCartage(branchPK, "Consignment");
			TestDataCreator.CreateJobHeader(branchPK, companyPK, cartagePK, departmentPK, "JJ", "Parent", "Reference", "WRK");

			var relatedAddressesList = new[] { address1PK, address2PK };
			var cartages = GetJobCartagesWithSecurityContext(relatedAddressesList: relatedAddressesList);
			Assert(cartages.Contains(cartageAgentCollect1PK));
			Assert(cartages.Contains(cartageAgentCollect2PK));
			Assert(cartages.Contains(cartageLocalCharges1PK));
			Assert(cartages.Contains(cartageLocalCharges2PK));
			AssertEquals(4, cartages.Count());
		}

		Guid CreateCartageWithJob(string addressFieldName, Guid addressPK, Guid branchPK, Guid companyPK, Guid departmentPK)
		{
			var cartagePK = TestDataCreator.CreateJobCartage(branchPK, $"Consignment{++cartageCounter}");
			var jobPK = TestDataCreator.CreateJobHeader(branchPK, companyPK, cartagePK, departmentPK, "JJ", $"Parent{cartageCounter}", $"Reference{cartageCounter}", "WRK");
			TestConnection.ExecuteNonQuery($"UPDATE dbo.JobHeader SET {addressFieldName}='{addressPK}', JH_SystemLastEditTimeUtc = GETUTCDATE(), JH_SystemLastEditUser = '~BP' WHERE JH_PK='{jobPK}'");

			return cartagePK;
		}

		public void TestCartagesWithOrgs()
		{
			var companyPK = TestDataCreator.CreateCompany("CMP", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "BRN", "USORD");

			var org1PK = TestDataCreator.CreateOrganisation("ORG1", "Test Org1");
			var org2PK = TestDataCreator.CreateOrganisation("ORG2", "Test Org2");
			var org3PK = TestDataCreator.CreateOrganisation("ORG3", "Test Org3");

			var cartage1PK = CreateCartageWithRelatedClient(org1PK, branchPK);
			var cartage2PK = CreateCartageWithRelatedClient(org2PK, branchPK);
			CreateCartageWithRelatedClient(org3PK, branchPK);

			var relatedOrgsList = new[] { org1PK, org2PK };
			var cartages = GetJobCartagesWithSecurityContext(relatedOrgsList: relatedOrgsList);
			Assert(cartages.Contains(cartage1PK));
			Assert(cartages.Contains(cartage2PK));
			AssertEquals(2, cartages.Count());
		}

		Guid CreateCartageWithRelatedClient(Guid clientOrgPK, Guid branchPK)
		{
			var cartagePK = TestDataCreator.CreateJobCartage(branchPK, $"Consginment{++cartageCounter}");
			TestConnection.ExecuteNonQuery($"UPDATE dbo.JobCartage SET JJ_OH_ClientID='{clientOrgPK}', JJ_SystemLastEditTimeUtc=GetUtcDate(), JJ_SystemLastEditUser='~BP' WHERE JJ_PK='{cartagePK}'");

			return cartagePK;
		}

		public void TestAllAllowedCartages()
		{
			var companyPK = TestDataCreator.CreateCompany("CMP", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "BRN", "USORD");
			var departmentPK = TestDataCreator.CreateDepartment("DPT");

			var orgPK = TestDataCreator.CreateOrganisation("ORG1", "Test Org1");
			var addressPK = TestDataCreator.CreateAddress(orgPK, "Address1", "Test Address1");

			var cartageAgentCollectPK = CreateCartageWithJob("JH_OA_AgentCollectAddr", addressPK, branchPK, companyPK, departmentPK);
			var cartageLocalChargesPK = CreateCartageWithJob("JH_OA_LocalChargesAddr", addressPK, branchPK, companyPK, departmentPK);

			var cartageWithLCFPK = CreateCartageWithDocAddress(branchPK, addressPK, "LCF");
			var cartageWithLCTPK = CreateCartageWithDocAddress(branchPK, addressPK, "LCT");
			var cartageWithLCMPK = CreateCartageWithDocAddress(branchPK, addressPK, "LCM");
			var cartageWithLCYPK = CreateCartageWithDocAddress(branchPK, addressPK, "LCY");
			var cartageWithLCSPK = CreateCartageWithDocAddress(branchPK, addressPK, "LCS");
			var cartageWithLCEPK = CreateCartageWithDocAddress(branchPK, addressPK, "LCE");
			var cartageWithLCIPK = CreateCartageWithDocAddress(branchPK, addressPK, "LCI");

			var cartageWithClientIDPK = CreateCartageWithRelatedClient(orgPK, branchPK);

			var relatedAddressesList = new[] { addressPK };
			var relatedOrgsList = new[] { orgPK };
			var cartages = GetJobCartagesWithSecurityContext(relatedAddressesList: relatedAddressesList, relatedOrgsList: relatedOrgsList);

			Assert(cartages.Contains(cartageAgentCollectPK));
			Assert(cartages.Contains(cartageLocalChargesPK));
			Assert(cartages.Contains(cartageWithLCFPK));
			Assert(cartages.Contains(cartageWithLCTPK));
			Assert(cartages.Contains(cartageWithLCMPK));
			Assert(cartages.Contains(cartageWithLCYPK));
			Assert(cartages.Contains(cartageWithLCSPK));
			Assert(cartages.Contains(cartageWithLCEPK));
			Assert(cartages.Contains(cartageWithLCIPK));
			Assert(cartages.Contains(cartageWithClientIDPK));
			AssertEquals(10, cartages.Count());
		}

		public IEnumerable<Guid> CreateAllowedCartages(Guid orgPK, Guid addressPK, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
				var cartageAgentCollectPK = CreateCartageWithJob("JH_OA_AgentCollectAddr", addressPK, branchPK, companyPK, departmentPK);
				var cartageLocalChargesPK = CreateCartageWithJob("JH_OA_LocalChargesAddr", addressPK, branchPK, companyPK, departmentPK);

				var cartageWithDocAddressPK = CreateCartageWithDocAddress(branchPK, addressPK, "LCF");

				var cartageWithClientIDPK = CreateCartageWithRelatedClient(orgPK, branchPK);

				return new[]
				{
					cartageAgentCollectPK,
					cartageLocalChargesPK,
					cartageWithDocAddressPK,
					cartageWithClientIDPK
				};
		}

		IEnumerable<Guid> GetJobCartagesWithSecurityContext(
			IEnumerable<Guid> relatedAddressesList = null,
			IEnumerable<Guid> relatedOrgsList = null)
		{
			var relatedAddresses = relatedAddressesList?.Select(pk => $"{pk}") ?? Enumerable.Empty<string>();
			var relatedOrgs = relatedOrgsList?.Select(pk => $"{pk}") ?? Enumerable.Empty<string>();

			using (var command = TestConnection.Command("SELECT * FROM GetJobCartagesWithSecurityContext(@addressListForCartages, @orgListForCartages)"))
			{
				command.AddParameter("@addressListForCartages", SqlDbType.NVarChar, string.Join(",", relatedAddresses));
				command.AddParameter("@orgListForCartages", SqlDbType.NVarChar, string.Join(",", relatedOrgs));

				var data = DataUtils.GetDataTableFromCommand(command);

				return data.AsEnumerable().Select(row => (Guid)row[JobCartageSchema.Constants.PK]);
			}
		}

		IEnumerable<string> GetColumnNames(string commandText)
		{
			using (var command = TestConnection.Command(commandText))
			using (var reader = command.ExecuteReader(CommandBehavior.SchemaOnly))
			{
				return reader
					.GetSchemaTable()
					.Select()
					.Select(x => (string)x["ColumnName"]);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			cartageCounter = 0;
		}

		int cartageCounter;
	}
}
