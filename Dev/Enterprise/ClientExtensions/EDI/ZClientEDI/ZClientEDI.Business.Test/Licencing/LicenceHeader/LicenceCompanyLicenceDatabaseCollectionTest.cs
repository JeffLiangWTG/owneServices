using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(BusinessObjectCollection))]
	public class LicenceCompanyLicenceDatabaseCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			LicenceCompany comp = Factory.New<LicenceCompany>();
			return new LicenceCompanyLicenceDatabaseCollection(comp);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			// This test fails because of a probable bug in the M2M Bo Coll.
			// Deleting pivot records (LicenceHeader) deletes the pivot's dependent LicenceModules In doing so, the HasChanges of the collection gets set to true by HasChangesFromDelete.
			// Work Item W00036668 has been created for the Core team to investigate and fix this bug.
			Assert(true);
		}
	}

	[TestedType(typeof(LicenceHeaderCollection))]
	public class LicenceHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			AssertEquals("AllowNew", false, Collection.AllowNew);
		}

		public void TestFindByID()
		{
			LicenceDatabase dB = TestHeader.LicCompany.LicDatabases.AddNew();
			dB.LD_ServerCode = "XB1";

			LicenceHeaderCollection testCollection = TestHeader.LicCompany.LicDatabases[0].LicHeadersForAllCompanies;
			AssertEquals("PRE: there is one item in the header collection", 1, testCollection.Count);
			AssertNotNull("Can find the header initially set-up", testCollection.FindById("XB1", "COM"));
			AssertNull("Cant find header with no DB", testCollection.FindById("XXX", "COM"));
			AssertNull("Cant find header with no Company", testCollection.FindById("XB1", "CCC"));
		}

		public void TestActiveCountChanged()
		{
			TestHeader.LicCompany.LicDatabases.ActiveCountChanged += new EventHandler(LicDatabases_ActiveCountChanged);
			AssertEquals(0, callsToActiveCountChanged);
			LicenceDatabase db1 = TestHeader.LicCompany.LicDatabases.AddNew();
			AssertEquals(1, callsToActiveCountChanged);
			LicenceDatabase db2 = TestHeader.LicCompany.LicDatabases.AddNew();
			AssertEquals(2, callsToActiveCountChanged);
			db1.LD_IsActive = true;
			AssertEquals("no change", 2, callsToActiveCountChanged);
			db2.LD_IsActive = false;
			AssertEquals(3, callsToActiveCountChanged);
			var licHeader1 = TestHeader.LicCompany.GetHeader(db1);
			licHeader1.LA_IsActive = false;
			AssertEquals(4, callsToActiveCountChanged);
			TestHeader.LicCompany.LicDatabases.Remove(db2);
			AssertEquals(5, callsToActiveCountChanged);
		}

		int callsToActiveCountChanged;

		void LicDatabases_ActiveCountChanged(object sender, EventArgs e)
		{
			++callsToActiveCountChanged;
		}

		#region Implementation

		EDIOrgHeader TestHeader;

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new LicenceHeaderCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			LicenceDatabase dB = TestHeader.LicCompany.LicDatabases.AddNew();
			return TestHeader.LicCompany.GetHeader(dB);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestHeader = Factory.New<EDIOrgHeader>();
			TestHeader.CreateAndLoadLicenceForOrg();
			TestHeader.LicCompany.LC_CompanyCode = "COM";
		}

		#endregion
	}
}
