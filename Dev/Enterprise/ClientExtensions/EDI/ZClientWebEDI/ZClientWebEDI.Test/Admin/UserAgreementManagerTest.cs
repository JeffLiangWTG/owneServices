using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(UserAgreementManager))]
	public class UserAgreementManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestEDocs()
		{
			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			var doc1 = agreement.DocManagerInfo.AddFileOrDocument([1, 1, 1, 1], "TAX INVOICE - AUS00335939 - JUSINTHKG (30-Jun-23).pdf", "INV");
			var doc2 = agreement.DocManagerInfo.AddFileOrDocument([1, 1, 1, 1], "TAX INVOICE - AUS00335939 - JUSINTHKG (30-Jun-23).pdf", "INV");
			doc1.IsPublished = true;
			doc2.IsPublished = false;
			agreement.DocManagerInfo.Save();
			var manager = new UserAgreementManager();

			AssertEquals("EDocs should be null if agreement isn't attached", 0, manager.EDocs.Count);
			manager.Agreement = agreement;
			AssertEquals("Should have published edocs from the agreement", 1, manager.EDocs.Count);
			AssertEquals("Should have published edocs from the agreement", doc1.UniqueKey, manager.EDocs.FirstOrDefault().UniqueKey);
		}

		public void TestDatabases()
		{
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var database1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			var database3 = Factory.NewWithValidTestData<LicenceDatabase>();
			enterprise.Databases.Add(database1);
			enterprise.Databases.Add(database2);
			enterprise.Databases.Add(database3);
			Factory.Save();
			var manager = new UserAgreementManager();

			AssertEquals("Should have no databases", 0, manager.Databases.Count());

			manager.Enterprise = enterprise;

			AssertEquals("Should have databases from the enterprise", 3, manager.Databases.Count());
			AssertEquals("Should have databases from the enterprise", true, manager.Databases.Any(x => x.PK == database1.PK));
			AssertEquals("Should have databases from the enterprise", true, manager.Databases.Any(x => x.PK == database2.PK));
			AssertEquals("Should have databases from the enterprise", true, manager.Databases.Any(x => x.PK == database3.PK));

			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			Factory.Save();

			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.Parent = database1;
			assignment1.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment1.EAE_VariantCode = "VA1";
			assignment1.EAE_OH_ClientAgreementOrg = org1.PK;
			var assignment2 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment2.Parent = database2;
			assignment2.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment2.EAE_VariantCode = "VA1";
			assignment2.EAE_OH_ClientAgreementOrg = org1.PK;

			manager.Enterprise = null;
			manager.ClientAgreementOrg = org1;
			manager.Assignments = new EdiUserAgreementAssignment[] { assignment1, assignment2 };

			AssertEquals("Should have databases from the assignments", 2, manager.Databases.Count());
			AssertEquals("Should have databases from the assignments", true, manager.Databases.Any(x => x.PK == database1.PK));
			AssertEquals("Should have databases from the assignments", true, manager.Databases.Any(x => x.PK == database2.PK));
			AssertEquals("Should have databases from the assignments", false, manager.Databases.Any(x => x.PK == database3.PK));
		}

		public void TestIsDatabaseLevelAgreement()
		{
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var database1 = enterprise.Databases.AddNew();
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();

			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.Parent = database1;
			assignment1.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment1.EAE_VariantCode = "VA1";
			assignment1.EAE_OH_ClientAgreementOrg = org1.PK;
			Factory.Save();

			var manager = new UserAgreementManager();
			manager.Enterprise = enterprise;
			AssertEquals("Should not be a database level agreement if there are no assignments", false, manager.IsDatabaseLevelAgreement);

			manager.Enterprise = null;
			manager.ClientAgreementOrg = org1;
			manager.Assignments = Array.Empty<EdiUserAgreementAssignment>();
			AssertEquals("Should not be a database level agreement if there are no assignments", false, manager.IsDatabaseLevelAgreement);

			manager.Assignments = new [] { assignment1 };
			AssertEquals("Should be a database level agreement if there are assignments", true, manager.IsDatabaseLevelAgreement);
		}
	}
}
