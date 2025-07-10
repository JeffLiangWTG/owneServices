using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(RNSRequestBOCollection))]
	sealed class RNSRequestBOCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RNSRequestBOCollection>
	{
		public void TestRNSRequestBOCollection()
		{
			var consol = Factory.New<ForwardingConsol>();
			var collection = new RNSRequestBOCollection(consol);
			AssertEquals("Empty collection", 0, collection.Count);
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			collection.Load();
			AssertEquals(2, collection.Count);
		}

		public void TestGetSelectedRequestBOs()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			var collection = new RNSRequestBOCollection(consol);
			AssertEquals(2, collection.Count);
			AssertEquals(0, collection.GetSelectedRequestBOs().Count());
			collection[0].Selected = true;
			AssertEquals(1, collection.GetSelectedRequestBOs().Count());
		}

		public void TestSelectAllAndDeselectAll()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			var collection = new RNSRequestBOCollection(consol);
			AssertEquals(2, collection.Count);
			AssertEquals(0, collection.GetSelectedRequestBOs().Count());
			collection.SelectAll();
			AssertEquals(2, collection.GetSelectedRequestBOs().Count());
			collection.DeselectAll();
			AssertEquals(0, collection.GetSelectedRequestBOs().Count());
		}

		protected override RNSRequestBOCollection GetCollectionToTest()
		{
			return new RNSRequestBOCollection(Factory.New<ForwardingConsol>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RNSRequestBO(RNSMessageTypes.Codes.ArrivalCertification, Factory);
		}
	}
}
