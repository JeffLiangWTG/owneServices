using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(JPAFRContainerCollection))]
	class JPAFRContainerCollectionTest : ActiveBusinessObjectCollectionTestCase<JPAFRContainerCollection>
	{
		public void TestAddNewAndIndexer_ContainerNumber()
		{
			var collection = Bill.Containers;
			var container1 = collection.AddNew("ISS1B1");
			var container2 = collection.AddNew("ISS2B1");
			var container3 = collection.AddNew("ISS1B2");

			AssertNull(collection[""]);
			AssertNull(collection["B2"]);
			AssertEquals(container3, collection["ISS1B2"]);
			AssertEquals(container1, collection["ISS1B1"]);
			AssertEquals(container2, collection["ISS2B1"]);
		}

		public void TestAllowNewWhenHeaderIsDeleted()
		{
			var collection = new JPAFRContainerCollectionForTest(Bill);
			Bill.Delete();

			AssertNoExceptionThrown(() =>
			{
				var allowNew = collection.GetAllowNew();
				Assert(!allowNew);
			});
		}

		protected override JPAFRContainerCollection GetCollectionToTest()
		{
			return new JPAFRContainerCollection(Bill);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<JPAFRContainer>();
		}

		ForwardingConsol Consol
		{
			get { return consol ?? (consol = Factory.New<ForwardingConsol>()); }
		}
		ForwardingConsol consol;

		JPAFRHeader Header
		{
			get
			{
				if (header == null)
				{
					header = Factory.New<JPAFRHeader>();
					header.JPH_ParentId = Consol.PK;
					header.JPH_ParentTableCode = Consol.TablePrefix;
				}
				return header;
			}
		}
		JPAFRHeader header;

		JPAFRBills Bill
		{
			get { return bill ?? (bill = Header.Bills.AddNew()); }
		}
		JPAFRBills bill;

		public class JPAFRContainerCollectionForTest : JPAFRContainerCollection
		{
			public JPAFRContainerCollectionForTest(JPAFRBills bill)
				: base(bill)
			{
			}

			public bool GetAllowNew()
			{
				return AllowNew;
			}
		}
	}
}
