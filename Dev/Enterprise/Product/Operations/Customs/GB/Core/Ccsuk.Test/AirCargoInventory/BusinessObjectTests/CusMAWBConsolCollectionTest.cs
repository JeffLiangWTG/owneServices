using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	[TestedType(typeof(CusMAWBConsolCollection))]
	sealed class CusMAWBConsolCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var consol = Factory.New<ForwardingConsol>();
			return new CusMAWBConsolCollection(Factory, consol.PK);
		}

		public void TestConstructor()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			_ = CreateMawb(consol1);
			_ = CreateMawb(consol1);

			var consol2 = Factory.New<ForwardingConsol>();
			var expectedMawbs = new CusMAWB[] { CreateMawb(consol2), CreateMawb(consol2) };

			var mawbsCollection = new CusMAWBConsolCollection(Factory, consol2.PK);
			mawbsCollection.Load();

			AssertContainsExactElementsInAnyOrder(expectedMawbs, mawbsCollection);
		}

		public void Test_AddNew()
		{
			var consol = Factory.New<ForwardingConsol>();
			var expectedMawbs = new List<CusMAWB> { CreateMawb(consol) };

			var mawbsCollection = new CusMAWBConsolCollection(Factory, consol.PK);
			mawbsCollection.Load();

			AssertContainsExactElementsInAnyOrder("Pre-Condition", expectedMawbs, mawbsCollection);

			expectedMawbs.Add(mawbsCollection.AddNew());

			AssertContainsExactElementsInAnyOrder("Post-Condition", expectedMawbs, mawbsCollection);
		}

		CusMAWB CreateMawb(ForwardingConsol consol)
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_JK = consol.PK;
			mawb.CM_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			return mawb;
		}
	}
}
