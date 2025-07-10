using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.HK.Business.Testing
{
	[TestedType(typeof(TraxonMessageCollection))]
	class TraxonMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestLoadOld()
		{
			ForwardingConsol testConsol = Factory.New<ForwardingConsol>();

			TraxonConsolStatus testStatus = new TraxonConsolStatus(testConsol);
			TraxonMessageCollection collection = new TraxonMessageCollection(testStatus);
			collection.Load();
			AssertEquals("Load before save Collection.Count", 0, collection.Count);

			EDIMessage message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.Traxon;
			message.EM_LinkTable = ForwardingConsol.Schema.TableName;
			message.EM_LinkUniqueID = testConsol.PK;

			collection.Load();
			AssertEquals("Load after save Collection.Count", 1, collection.Count);
			collection.Load();
			AssertEquals("Second Load after save Collection.Count", 1, collection.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			TraxonConsolStatus status = new TraxonConsolStatus(consol);
			return new TraxonMessageCollection(status);
		}
	}
}
