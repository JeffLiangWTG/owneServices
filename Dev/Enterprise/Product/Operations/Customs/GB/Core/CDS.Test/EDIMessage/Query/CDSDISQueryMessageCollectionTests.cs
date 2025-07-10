using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSDISQueryMessageCollection))]
	class CDSDISQueryMessageCollectionTests : ActiveBusinessObjectCollectionTestCase<CDSDISQueryMessageCollection>
	{
		public void TestNewItem()
		{
			var collection = new CDSDISQueryMessageCollection(Factory);
			var msg = collection.AddNew();

			AssertNotNull(msg);
			AssertType(typeof(CDSDISQueryMessage), msg);
			AssertEquals(ApplicationCodeList.Codes.GbCDSDISQuery, msg.EM_ApplicationCode);
		}
	}
}
