using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(OrgLedgerFilterCollection))]
	public class OrgLedgerFilterCollectionTestCase : NonPersistentBusinessObjectCollectionTestCase<OrgLedgerFilterCollection>
	{
		#region Implementation

		protected OrgHeader TestOrg1;
		protected OrgHeader TestOrg2;
		protected OrgHeader TestOrg3;
		protected OrgLedgerFilterCollection TestOrgAccInfos;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrgLedgerFilter(Factory, GetCollectionToTest());
		}

		protected override OrgLedgerFilterCollection GetCollectionToTest()
		{
			return new OrgLedgerFilterCollection(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg1.OH_IsDebtor = true;

			TestOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg2.OH_IsDebtor = true;
			TestOrg2.OH_IsCreditor = true;

			TestOrg3 = Factory.NewWithValidTestData<OrgHeader>();

			TestOrgAccInfos = GetCollectionToTest();
		}

		#endregion

		#region TestGuidOccursTwice

		public void TestGuidOccursTwice()
		{
			OrgLedgerFilter accInfo1 = TestOrgAccInfos.AddNew();
			accInfo1.Organization = TestOrg1.PK;

			OrgLedgerFilter accInfo2 = TestOrgAccInfos.AddNew();
			accInfo2.Organization = TestOrg2.PK;

			Assert("Guid should not occur twice", !TestOrgAccInfos.GuidOccursTwice(TestOrg1.PK));
			Assert("Guid should not occur twice", !TestOrgAccInfos.GuidOccursTwice(TestOrg2.PK));

			OrgLedgerFilter accInfo3 = TestOrgAccInfos.AddNew();
			accInfo3.Organization = TestOrg1.PK;

			Assert("Guid should occur twice", TestOrgAccInfos.GuidOccursTwice(TestOrg1.PK));
			Assert("Guid should not occur twice", !TestOrgAccInfos.GuidOccursTwice(TestOrg2.PK));
		}

		#endregion

		#region TestAreOrgLedgerFilterCollectionsEqual

		public void TestAreOrgLedgerFilterCollectionsEqual()
		{
			OrgLedgerFilter accInfo1 = TestOrgAccInfos.AddNew();
			accInfo1.Organization = TestOrg1.PK;

			OrgLedgerFilter accInfo2 = TestOrgAccInfos.AddNew();
			accInfo2.Organization = TestOrg2.PK;

			OrgLedgerFilterCollection orgLedgers2 = new OrgLedgerFilterCollection(Factory);
			OrgLedgerFilter orgLedgers2_1 = orgLedgers2.AddNew();
			orgLedgers2_1.Organization = TestOrg1.PK;

			Assert("OrgLedgerCollections should not be equal", !TestOrgAccInfos.ArePKCollectionsEqual(orgLedgers2));

			OrgLedgerFilter orgLedgers2_2 = orgLedgers2.AddNew();
			orgLedgers2_2.Organization = TestOrg2.PK;

			Assert("OrgLedgerCollections should be equal", TestOrgAccInfos.ArePKCollectionsEqual(orgLedgers2));

			orgLedgers2_2.ARLedger = false;
			Assert("OrgLedgerCollections should not be equal", !TestOrgAccInfos.ArePKCollectionsEqual(orgLedgers2));
		}

		#endregion

		#region TestLoadOrgs

		public virtual void TestLoadOrgs()
		{
			Factory.Save();

			// not the filter used in practice but just to check 
			// setting of ledger fields
			ZQuery filter = new ZQuery(OrgHeaderSchema.PK, TestOrg1.PK);
			TestOrgAccInfos.LoadOrgs(filter, TestOrg2);

			AssertEquals("Should be 2 elements in the list", 2, TestOrgAccInfos.Count);
			AssertEquals("First element should be TestOrg2", TestOrg2.PK, TestOrgAccInfos[0].Organization);
			Assert("First org should be readonly", TestOrgAccInfos[0].OrganizationInfo.ReadOnly);
			Assert("APLedger should be true", TestOrgAccInfos[0].APLedger);
			Assert("ARLedger should be true", TestOrgAccInfos[0].ARLedger);

			AssertEquals("Second element should be TestOrg1", TestOrg1.PK, TestOrgAccInfos[1].Organization);
			Assert("ARLedger should be true", TestOrgAccInfos[1].ARLedger);
			Assert("APLedger should be false", !TestOrgAccInfos[1].APLedger);

			TestOrgAccInfos.LoadOrgs(filter, null);
			AssertEquals("The list should be empty", 0, TestOrgAccInfos.Count);

			TestOrgAccInfos.LoadOrgs(null, TestOrg2);
			AssertEquals("The list should be empty", 0, TestOrgAccInfos.Count);

			TestOrgAccInfos.LoadOrgs(null, null);
			AssertEquals("The list should be empty", 0, TestOrgAccInfos.Count);
		}

		#endregion

		#region TestSetARLedgerOfAllElements

		public void TestSetARLedgerOfAllElements()
		{
			TestOrgAccInfos.AddNew();
			TestOrgAccInfos.AddNew();
			TestOrgAccInfos.SetARLedgerOfAllElements(true);
			Assert("First element should have AR = true", TestOrgAccInfos[0].ARLedger);
			Assert("Second element should have AR = true", TestOrgAccInfos[1].ARLedger);

			TestOrgAccInfos.SetARLedgerOfAllElements(false);
			Assert("First element should have AR = false", !TestOrgAccInfos[0].ARLedger);
			Assert("Second element should have AR = false", !TestOrgAccInfos[1].ARLedger);
		}

		#endregion

		#region TestClone

		public void TestClone()
		{
			OrgLedgerFilter orgLedger1 = TestOrgAccInfos.AddNew();
			orgLedger1.Organization = TestOrg1.PK;
			orgLedger1.ARLedger = false;

			OrgLedgerFilter orgLedger2 = TestOrgAccInfos.AddNew();
			orgLedger2.Organization = TestOrg2.PK;

			OrgLedgerFilterCollection collectionClone = TestOrgAccInfos.Clone();

			orgLedger1.ARLedger = true;

			AssertEquals("Clone should have 2 elements", 2, collectionClone.Count);
			Assert("Clone should contain TestOrg1", collectionClone.ContainsOrgPK(TestOrg1.PK));
			Assert("Clone should contain TestOrg2", collectionClone.ContainsOrgPK(TestOrg2.PK));
			Assert("ARLedger for TestOrg1 should be false", !collectionClone.GetLedgerFilterForOrg_ForTestOnly(TestOrg1.PK).ARLedger);
		}

		#endregion

		#region TestCloneInvalidOrg

		public void TestCloneInvalidOrg()
		{
			OrgLedgerFilter orgLedger = TestOrgAccInfos.AddNew();
			orgLedger.Organization = ZGuid.Empty;

			OrgLedgerFilter orgLedger2 = TestOrgAccInfos.AddNew();
			orgLedger.Organization = ZGuid.Invalid;

			OrgLedgerFilter orgLedger3 = TestOrgAccInfos.AddNew();
			orgLedger3.Organization = TestOrg1.PK;

			OrgLedgerFilterCollection clonedCollection = TestOrgAccInfos.Clone();

			AssertEquals("There should be 1 element in the Clone", 1, clonedCollection.Count);

			OrgLedgerFilter clonedOrgLedger = clonedCollection[0];
			AssertEquals("Organisation of ClonedOrgLedger should be TestOrg1", TestOrg1.PK, clonedOrgLedger.Organization);
			Assert("APLedger of ClonedOrgLedger should be false", !clonedOrgLedger.APLedger);
			Assert("ARLedger of ClonedOrgLedger should be true", clonedOrgLedger.ARLedger);
		}

		#endregion

		#region TestGetLedgerFilterForOrg

		public void TestGetLedgerFilterForOrg()
		{
			OrgLedgerFilter orgLedger1 = TestOrgAccInfos.AddNew();
			orgLedger1.Organization = TestOrg1.PK;

			OrgLedgerFilter orgLedger2 = TestOrgAccInfos.AddNew();
			orgLedger2.Organization = TestOrg2.PK;

			AssertNull("Cannot get OrgLedger for non-existent Organisation", TestOrgAccInfos.GetLedgerFilterForOrg_ForTestOnly(ZGuid.NewZGuid()));

			OrgLedgerFilter orgLedgerFound = TestOrgAccInfos.GetLedgerFilterForOrg_ForTestOnly(TestOrg1.PK);
			AssertNotNull("OrgLEdger should exist", orgLedgerFound);
			AssertEquals("OrgLEdger should relate to TestOrg1", TestOrg1.PK, orgLedgerFound.Organization);
		}

		#endregion
	}
}
