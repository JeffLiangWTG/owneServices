using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(NotificationForwardingPartyCollection))]
	class NotificationForwardingPartyCollectionTest : CargoWise.EntityFramework.Testing.ActiveBusinessObjectCollectionTestCase<NotificationForwardingPartyCollection>
	{
		public void TestSetRelationshipDefaults()
		{
			var orl = NotificationForwardingParties.AddNew();
			AssertEquals(Bill.PK, orl.CY_ParentID);
			AssertEquals(JPAFRBillsSchema.Constants.Prefix, orl.CY_ParentTableCode);
		}

		public void TestGetNonEmptyNFPInSortOrder()
		{
			var nfp1 = NotificationForwardingParties.AddNew();
			nfp1.CY_Data = "T5";
			var nfp2 = NotificationForwardingParties.AddNew();
			var nfp3 = NotificationForwardingParties.AddNew();
			nfp3.CY_Data = "T3";
			var nfp4 = NotificationForwardingParties.AddNew();
			nfp4.CY_Data = "T4";
			nfp3.CY_Order = 1;
			nfp2.CY_Order = 2;
			nfp1.CY_Order = 3;
			nfp4.CY_Order = 4;

			var nfp5 = new BusinessObjectFactory().New<NotificationForwardingParty>();
			nfp5.CY_Order = 3;
			nfp5.CY_Data = "T1";
			nfp5.CY_ParentID = Bill.PK;
			nfp5.CY_ParentTableCode = Bill.TablePrefix;
			nfp5.Factory.Save();
			nfp5 = Factory.Load<NotificationForwardingParty>(nfp5.PK);
			AssertEquals("Precondition", 5, NotificationForwardingParties.Count);
			AssertEquals((short)1, nfp3.CY_Order);
			AssertEquals((short)2, nfp2.CY_Order);
			AssertEquals((short)3, nfp1.CY_Order);
			AssertEquals((short)3, nfp5.CY_Order);
			AssertEquals((short)4, nfp4.CY_Order);

			var nfps = NotificationForwardingParties.GetNonEmptyInSortOrder();
			AssertEquals(4, nfps.Length);
			AssertEquals("nfp3", nfp3, nfps[0]);
			AssertEquals("nfp1", nfp1, nfps[2]);
			AssertEquals("nfp5", nfp5, nfps[1]);
			AssertEquals("nfp4", nfp4, nfps[3]);
		}

		public void TestAddNewIfNotExist()
		{
			AssertEquals(0, NotificationForwardingParties.Count);
			var law1 = NotificationForwardingParties.AddNewIfNotExist("JP");
			AssertEquals(1, NotificationForwardingParties.Count);
			var law2 = NotificationForwardingParties.AddNewIfNotExist("JP");
			AssertEquals(1, NotificationForwardingParties.Count);
			AssertEquals(law1, law2);
			var law3 = NotificationForwardingParties.AddNewIfNotExist("DJ");
			AssertEquals(2, NotificationForwardingParties.Count);
			AssertNotEquals(law1, law3);
		}

		public void TestAllowNew()
		{
			var testConsol = Factory.New<ForwardingConsol>();
			Header.JPH_ParentId = testConsol.PK;
			Header.JPH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			Header.JPH_OverrideFreightDefaults = false;
			var testCollection = new NotificationForwardingPartyCollectionForTest(Bill);
			Assert(!testCollection.AllowNewExposed);
			Header.JPH_OverrideFreightDefaults = true;
			Assert(testCollection.AllowNewExposed);
		}

		#region Implementation

		JPAFRHeader Header
		{
			get { return header ?? (header = Factory.New<JPAFRHeader>()); }
		}
		JPAFRHeader header;

		JPAFRBills Bill
		{
			get { return bill ?? (bill = Header.Bills.AddNew()); }
		}
		JPAFRBills bill;

		NotificationForwardingPartyCollection NotificationForwardingParties
		{
			get { return Bill.NotificationForwardingParties; }
		}

		protected override NotificationForwardingPartyCollection GetCollectionToTest()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			return new NotificationForwardingPartyCollection(bill);
		}

		#endregion

		#region Preperation

		class NotificationForwardingPartyCollectionForTest : NotificationForwardingPartyCollection
		{
			public NotificationForwardingPartyCollectionForTest(JPAFRBills parent)
				: base(parent)
			{
			}

			public bool AllowNewExposed
			{
				get { return base.AllowNew; }
			}
		}

		#endregion
	}
}
