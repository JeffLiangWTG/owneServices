using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(LicenceConnectionDependentCollection))]
	class LicenceConnectionDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			LicenceDatabase parent = Factory.New<LicenceDatabase>();
			return new LicenceConnectionDependentCollection(parent, Factory);
		}

		public void TestConnectionOrderAutoIncrements()
		{
			LicenceConnectionDependentCollection collection = (LicenceConnectionDependentCollection)base.Collection;
			LicenceConnection conn1 = collection.AddNew();
			LicenceConnection conn2 = collection.AddNew();
			AssertEquals("ConnectionOrder Should Autoincrement", (ZByte)1, conn1.LK_ConnectionOrder);
			AssertEquals("ConnectionOrder Should Autoincrement", (ZByte)2, conn2.LK_ConnectionOrder);
		}

		public void TestReadOnlySecurity()
		{
			bool oldValue = EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails.IsAllowed;

			LicenceConnectionDependentCollection collection = (LicenceConnectionDependentCollection)base.Collection;
			try
			{
				EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails.IsAllowed = true;
				Assert("Collection Should AllowNew", collection.AllowNew);

				EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails.IsAllowed = false;
				Assert("Collection Should NOT AllowNew", !collection.AllowNew);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails.IsAllowed = oldValue;
			}
		}

		public void TestConnectionDetailsOrderSortedByConnectionOrder()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_Code = "ABCXYZ";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.GenerateNewLicenceCode();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();

			LicenceConnection line1 = database.Connections.AddNew();
			line1.LK_ConnectionOrder = 4;
			LicenceConnection line2 = database.Connections.AddNew();
			line2.LK_ConnectionOrder = 2;
			LicenceConnection line3 = database.Connections.AddNew();
			line3.LK_ConnectionOrder = 1;
			LicenceConnection line4 = database.Connections.AddNew();
			line4.LK_ConnectionOrder = 3;
			Factory.Save();

			EDIOrgHeader testHeader2 = new BusinessObjectFactory().Load<EDIOrgHeader>(testHeader.PK);
			AssertEquals("There should be 4 connections", 4, testHeader2.LicCompany.LicDatabases[0].Connections.Count);
			AssertEquals("Item 0 should be Line 3", line3.PK, testHeader2.LicCompany.LicDatabases[0].Connections[0].PK);
			AssertEquals("Item 1 should be Line 2", line2.PK, testHeader2.LicCompany.LicDatabases[0].Connections[1].PK);
			AssertEquals("Item 2 should be Line 4", line4.PK, testHeader2.LicCompany.LicDatabases[0].Connections[2].PK);
			AssertEquals("Item 3 should be Line 1", line1.PK, testHeader2.LicCompany.LicDatabases[0].Connections[3].PK);
		}

		EDIOrgHeader HeaderForTest
		{
			get
			{
				if (fHeaderForTest == null)
				{
					fHeaderForTest = Factory.NewWithValidTestData<EDIOrgHeader>();
					fHeaderForTest.OH_RL_NKClosestPort = "AUBNE";
					fHeaderForTest.OH_FullName = "My Organisation";
					fHeaderForTest.OH_Code = "TGBLOG";

					OrgAddress newAddress = fHeaderForTest.Addresses.AddNew();
					newAddress.OA_Address1 = "666 Test Address";
				}

				return fHeaderForTest;
			}
		}
		EDIOrgHeader fHeaderForTest;
	}
}
