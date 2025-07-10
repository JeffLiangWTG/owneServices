using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow
{
	[TestedType(typeof(GetJobDeclarationsWithSecurityContext))]
	class GetJobDeclarationsWithSecurityContextTest : DbCreateScriptTest
	{
		public void TestReturnsJobDeclarationColumns()
		{
			var fromJobDeclaration = GetColumNames("SELECT * FROM dbo.JobDeclaration");
			var fromTvf = GetColumNames("SELECT * FROM GetJobDeclarationsWithSecurityContext(NEWID(), '')");

			var missingColumns = fromJobDeclaration.Except(fromTvf);
			var errorMessage = $"The following columns are missing and should be added to the function: {string.Join(", ", missingColumns)}";
			Assert(errorMessage, !missingColumns.Any());
		}

		public void TestImportMessageTypes()
		{
			var companyPK = TestDataCreator.CreateCompany("NEO", "AU", "AUD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "NEO", "AUSYD");

			var orgPK = TestDataCreator.CreateOrganisation("NEO", "Org 1");
			var otherOrgPK = TestDataCreator.CreateOrganisation("OTH", "Org 2");

			var expectedPKs = new []
			{
				TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001001", "IMP", 1, importerPK: orgPK),
				TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001002", "IMX", 2, importerPK: orgPK),
				TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001003", "WEA", 3, importerPK: orgPK),
				TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001004", "EXW", 4, importerPK: orgPK),
				TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001005", "LVS", 5, importerPK: orgPK),
				TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001006", "LVX", 6, importerPK: orgPK),
				TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001007", "IM2", 7, importerPK: orgPK),
				TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001008", "IPT", 8, importerPK: orgPK),
				TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001009", "INP", 9, importerPK: orgPK),
				TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001010", "TNP", 10, importerPK: orgPK),
				TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001011", "FTZ", 11, importerPK: orgPK),
				TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001012", "MSC", 12, importerPK: orgPK),
				TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001013", "IMP", 13, importerPK: orgPK),
			};

			_ = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001014", "IMP", 14, importerPK: otherOrgPK);
			_ = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001015", "EXP", 15, importerPK: orgPK);
			_ = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001016", "IMP", 16);

			var result = GetJobDeclarationsWithSecurityContext(orgPK);

			AssertContainsExactElementsInAnyOrder(expectedPKs, result);
		}

		public void TestExportMessageTypes()
		{
			var companyPK = TestDataCreator.CreateCompany("NEO", "AU", "AUD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "NEO", "AUSYD");

			var orgPK = TestDataCreator.CreateOrganisation("NEO", "Org 1");
			var otherOrgPK = TestDataCreator.CreateOrganisation("OTH", "Org 2");

			var expectedPKs = new[]
			{
				TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001001", "EXP", 1, supplierPK: orgPK),
				TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001002", "EXX", 2, supplierPK: orgPK),
				TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001003", "AQS", 3, supplierPK: orgPK),
				TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001004", "TNP", 4, supplierPK: orgPK),
				TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001005", "COO", 5, supplierPK: orgPK),
				TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001006", "EXP", 6, supplierPK: orgPK),
			};

			_ = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001007", "EXP", 7, supplierPK: otherOrgPK);
			_ = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001008", "IMP", 8, supplierPK: orgPK);
			_ = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001009", "EXP", 9);

			var result = GetJobDeclarationsWithSecurityContext(orgPK);

			AssertContainsExactElementsInAnyOrder(expectedPKs, result);
		}

		public void TestUSImporterOfRecord()
		{
			var companyPK = TestDataCreator.CreateCompany("NEO", "AU", "AUD");
			var usBranchPK = TestDataCreator.CreateBranch(companyPK, "NEO", "USORD", "US");
			var othBranchPK = TestDataCreator.CreateBranch(companyPK, "NST", "AUSYD", "AU");
			var orgPK = TestDataCreator.CreateOrganisation("NEO", "Org 1");
			var otherOrgPK = TestDataCreator.CreateOrganisation("OTH", "Org 2");
			var addressPK = TestDataCreator.CreateAddress(orgPK, "MAI", "123 Main Street");
			var otherAddressPK = TestDataCreator.CreateAddress(otherOrgPK, "MAI", "123 Main Street");

			var expectedPKs = new[]
			{
				TestDataCreator.CreateJobDeclaration(usBranchPK, companyPK, "B00001001", "IMP", 1, declarantAddressPK: addressPK),
				TestDataCreator.CreateJobDeclaration(usBranchPK, companyPK, "B00001002", "IMX", 2, declarantAddressPK: addressPK),
				TestDataCreator.CreateJobDeclaration(usBranchPK, companyPK, "B00001003", "WEA", 3, declarantAddressPK: addressPK),
				TestDataCreator.CreateJobDeclaration(usBranchPK, companyPK, "B00001004", "EXW", 4, declarantAddressPK: addressPK),
				TestDataCreator.CreateJobDeclaration(usBranchPK, companyPK, "B00001005", "LVS", 5, declarantAddressPK: addressPK),
				TestDataCreator.CreateJobDeclaration(usBranchPK, companyPK, "B00001006", "LVX", 6, declarantAddressPK: addressPK),
				TestDataCreator.CreateJobDeclaration(usBranchPK, companyPK, "B00001007", "IM2", 7, declarantAddressPK: addressPK),
				TestDataCreator.CreateJobDeclaration(usBranchPK, companyPK, "B00001008", "IPT", 8, declarantAddressPK: addressPK),
				TestDataCreator.CreateJobDeclaration(usBranchPK, companyPK, "B00001009", "INP", 9, declarantAddressPK: addressPK),
				TestDataCreator.CreateJobDeclaration(usBranchPK, companyPK, "B00001010", "TNP", 10, declarantAddressPK: addressPK),
				TestDataCreator.CreateJobDeclaration(usBranchPK, companyPK, "B00001011", "FTZ", 11, declarantAddressPK: addressPK),
				TestDataCreator.CreateJobDeclaration(usBranchPK, companyPK, "B00001012", "MSC", 12, declarantAddressPK: addressPK),
				TestDataCreator.CreateJobDeclaration(usBranchPK, companyPK, "B00001013", "IMP", 13, declarantAddressPK: addressPK),
			};

			_ = TestDataCreator.CreateJobDeclaration(usBranchPK, companyPK, "B00001014", "IMP", 14, declarantAddressPK: otherAddressPK);
			_ = TestDataCreator.CreateJobDeclaration(othBranchPK, companyPK, "B000010141", "IMP", 16, declarantAddressPK: addressPK);
			_ = TestDataCreator.CreateJobDeclaration(usBranchPK, companyPK, "B00001015", "EXP", 17, declarantAddressPK: addressPK);
			_ = TestDataCreator.CreateJobDeclaration(usBranchPK, companyPK, "B00001016", "IMP", 18);

			var result = GetJobDeclarationsWithSecurityContext(orgPK, new[] { addressPK });

			AssertContainsExactElementsInAnyOrder(expectedPKs, result);
		}

		public void TestLocalCharges()
		{
			var companyPK = TestDataCreator.CreateCompany("NEO", "AU", "AUD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "NEO", "USORD");
			var deptPK = TestDataCreator.CreateDepartment("NEO");
			var orgPK = TestDataCreator.CreateOrganisation("NEO", "Org 1");
			var otherOrgPK = TestDataCreator.CreateOrganisation("OTH", "Org 2");
			var addressPK = TestDataCreator.CreateAddress(orgPK, "MAI", "123 Main Street");
			var otherAddressPK = TestDataCreator.CreateAddress(otherOrgPK, "MAI", "123 Main Street");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001001", "", 1);
			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001002", "", 2);
			var declaration3PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001003", "", 3);

			_ = TestDataCreator.CreateJobHeader(branchPK, companyPK, declaration1PK, deptPK, "JE", "B00001001", "00001001", "INV", localChargesAddressPK: addressPK);
			_ = TestDataCreator.CreateJobHeader(branchPK, companyPK, declaration2PK, deptPK, "JE", "B00001002", "00001002", "INV", localChargesAddressPK: otherAddressPK);
			_ = TestDataCreator.CreateJobHeader(branchPK, companyPK, declaration3PK, deptPK, "JE", "B00001003", "00001003", "INV", localChargesAddressPK: addressPK);

			var expectedPKs = new[] { declaration1PK, declaration3PK };

			var result = GetJobDeclarationsWithSecurityContext(orgPK, new[] { addressPK });

			AssertContainsExactElementsInAnyOrder(expectedPKs, result);
		}

		public void TestShipmentLocalCharges()
		{
			var companyPK = TestDataCreator.CreateCompany("NEO", "AU", "AUD");
			var otherCompanyPK = TestDataCreator.CreateCompany("OTH", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "NEO", "USORD");
			var deptPK = TestDataCreator.CreateDepartment("NEO");
			var orgPK = TestDataCreator.CreateOrganisation("NEO", "Org 1");
			var otherOrgPK = TestDataCreator.CreateOrganisation("OTH", "Org 2");
			var addressPK = TestDataCreator.CreateAddress(orgPK, "MAI", "123 Main Street");
			var otherAddressPK = TestDataCreator.CreateAddress(otherOrgPK, "MAI", "123 Main Street");

			var shipment1PK = TestDataCreator.CreateJobShipment("S00001001");
			var shipment2PK = TestDataCreator.CreateJobShipment("S00001002");
			var shipment3PK = TestDataCreator.CreateJobShipment("S00001003");
			var shipment4PK = TestDataCreator.CreateJobShipment("S00001004");

			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001001", "", 1, shipmentPK: shipment1PK);
			_ = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001002", "", 2, shipmentPK: shipment2PK);
			var declaration3PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001003", "", 3, shipmentPK: shipment3PK);
			_ = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001004", "", 4, shipmentPK: shipment4PK);

			_ = TestDataCreator.CreateJobHeader(branchPK, companyPK, shipment1PK, deptPK, "JS", "S00001001", "00001001", "INV", localChargesAddressPK: addressPK);
			_ = TestDataCreator.CreateJobHeader(branchPK, companyPK, shipment2PK, deptPK, "JS", "S00001002", "00001002", "INV", localChargesAddressPK: otherAddressPK);
			_ = TestDataCreator.CreateJobHeader(branchPK, companyPK, shipment3PK, deptPK, "JS", "S00001003", "00001003", "INV", localChargesAddressPK: addressPK);
			_ = TestDataCreator.CreateJobHeader(branchPK, otherCompanyPK, shipment4PK, deptPK, "JS", "S00001004", "00001004", "INV", localChargesAddressPK: addressPK);

			var expectedPKs = new[] { declaration1PK, declaration3PK };

			var result = GetJobDeclarationsWithSecurityContext(orgPK, new[] { addressPK });

			AssertContainsExactElementsInAnyOrder(expectedPKs, result);
		}

		IEnumerable<Guid> GetJobDeclarationsWithSecurityContext(
			Guid orgPK,
			IEnumerable<Guid> relatedAddressesList = null,
			IEnumerable<Guid> relatedOrgsList = null)
		{
			var relatedAddresses = relatedAddressesList?.Select(pk => $"{pk}") ?? Enumerable.Empty<string>();
			var relatedOrgs = relatedOrgsList?.Select(pk => $"{pk}") ?? Enumerable.Empty<string>();

			using (var command = TestConnection.Command("SELECT * FROM GetJobDeclarationsWithSecurityContext(@contactOrganisationForDeclarations, @addressesListForDeclarations)"))
			{
				command.AddParameter("@contactOrganisationForDeclarations", SqlDbType.UniqueIdentifier, orgPK);
				command.AddParameter("@addressesListForDeclarations", SqlDbType.NVarChar, string.Join(",", relatedAddresses));

				var data = DataUtils.GetDataTableFromCommand(command);

				return data.AsEnumerable().Select(row => (Guid)row[JobDeclarationSchema.Constants.PK]);
			}
		}

		IEnumerable<string> GetColumNames(string commandText)
		{
			using (var command = TestConnection.Command(commandText))
			using (var reader = command.ExecuteReader(CommandBehavior.SchemaOnly))
			{
				return reader.GetSchemaTable()
					.Select()
					.Select(x => (string)x["ColumnName"]);
			}
		}
	}
}
