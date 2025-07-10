using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(LicenceDatabaseCollection))]
	public class LicenceDatabaseCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			return enterprise.Databases;
		}

		[ExpectNoExceptions]
		public void TestSetDefaultsForNewChildNullValues()
		{
			LicenceEnterprise licEnterprise = Factory.New<LicenceEnterprise>();
			LicenceDatabaseCollection coll = new LicenceDatabaseCollection(licEnterprise, Factory);
			coll.AddNew();

			licEnterprise = Factory.New<LicenceEnterprise>();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = ZString.Empty;
			licEnterprise.LE_OH = org.PK;
			coll = new LicenceDatabaseCollection(licEnterprise, Factory);
			var db1 = coll.AddNew();
			AssertEquals(false, licEnterprise.LE_IsInternal);
			AssertEquals("Y", db1.LD_Billable);

			licEnterprise.LE_IsInternal = true;
			var db2 = coll.AddNew();
			AssertEquals("N", db2.LD_Billable);
		}

		public void TestSetDefaultsForNewChild_WebAccessOrg()
		{
			var enterprise = Factory.New<LicenceEnterprise>();
			var databaseCollection = new LicenceDatabaseCollection(enterprise, Factory);

			var db1 = databaseCollection.AddNew();
			AssertEquals(ZGuid.Empty, db1.LD_OH_WebAccessOrg);

			var org = Factory.New<OrgHeader>();
			enterprise.LE_OH = org.PK;

			var db2 = databaseCollection.AddNew();
			AssertEquals(org.PK, db2.LD_OH_WebAccessOrg);
		}
	}

	[TestedType(typeof(LicenceDatabaseNonDependentCollection))]
	public class LicenceDatabaseNonDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new LicenceDatabaseNonDependentCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			EDIOrgHeader testHeader = Factory.New<EDIOrgHeader>();
			testHeader.CreateAndLoadLicenceForOrg();
			return testHeader.LicCompany.LicDatabases.AddNew();
		}

		public void TestSortByCurrentVersion()
		{
			var build1 = Factory.New<ReleaseBuild>();
			build1.HL_MajorVersion = 1;
			build1.HL_MinorVersion = 2;
			build1.HL_Release = 3;
			build1.HL_Patch = 111;
			var build2 = Factory.New<ReleaseBuild>();
			build2.HL_MajorVersion = 1;
			build2.HL_MinorVersion = 2;
			build2.HL_Release = 3;
			build2.HL_Patch = 2;

			var licDb1 = Factory.New<LicenceDatabase>();
			licDb1.LD_HL_CurrentRunningVersion = build1.PK;
			var licDb2 = Factory.New<LicenceDatabase>();
			licDb2.LD_HL_CurrentRunningVersion = build2.PK;

			Collection.Add(licDb1);
			Collection.Add(licDb2);

			Collection.Sort(LicenceDatabase.Schema.LD_HL_CurrentRunningVersion, ListSortDirection.Ascending);

			Assert((Collection.First() as LicenceDatabase)?.CurrentVersion.HL_Patch == 2);
		}

		public void TestSortBySentVersion()
		{
			var build1 = Factory.New<ReleaseBuild>();
			build1.HL_MajorVersion = 1;
			build1.HL_MinorVersion = 2;
			build1.HL_Release = 3;
			build1.HL_Patch = 111;
			var build2 = Factory.New<ReleaseBuild>();
			build2.HL_MajorVersion = 1;
			build2.HL_MinorVersion = 2;
			build2.HL_Release = 3;
			build2.HL_Patch = 2;

			var licDb1 = Factory.New<LicenceDatabase>();
			licDb1.LD_HL_CurrentSentVersion = build1.PK;
			var licDb2 = Factory.New<LicenceDatabase>();
			licDb2.LD_HL_CurrentSentVersion = build2.PK;

			Collection.Add(licDb1);
			Collection.Add(licDb2);

			Collection.Sort(LicenceDatabase.Schema.LD_HL_CurrentSentVersion, ListSortDirection.Ascending);

			Assert((Collection.First() as LicenceDatabase)?.SentVersion.HL_Patch == 2);
		}
	}

	[TestedType(typeof(LicenceDatabaseNonDependentActiveCollection))]
	public class LicenceDatabaseNonDependentActiveCollectionTest : ActiveBusinessObjectCollectionTestCase<LicenceDatabaseNonDependentActiveCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var org = Factory.New<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			var db = Factory.New<LicenceDatabase>();
			org.LicCompany.LicDatabases.Add(db);
			return db;
		}
	}

	[TestedType(typeof(LicenceDatabaseGlobalCollection))]
	public class LicenceDatabaseGlobalCollectionTest : ActiveBusinessObjectCollectionTestCase<LicenceDatabaseGlobalCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var org = Factory.New<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			var db = Factory.New<LicenceDatabaseGlobal>();
			org.LicCompany.LicDatabases.Add(db);
			return db;
		}
	}
}
