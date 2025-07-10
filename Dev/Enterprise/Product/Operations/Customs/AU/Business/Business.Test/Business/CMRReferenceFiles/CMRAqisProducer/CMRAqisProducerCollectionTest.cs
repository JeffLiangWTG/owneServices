using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRAqisProducerCollection))]
	sealed class CMRAqisProducerCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestOrderAfterLoad()
		{
			TestCaseHelper.ClearTable(CMRAqisProducer.Schema.TableName);

			CMRAqisProducer producer1 = CMRAqisProducer.New(Factory);
			producer1.QR_AQISProducerCode = "Code";
			producer1.QR_AQISProducerName = "Name";

			CMRAqisProducer producer2 = CMRAqisProducer.New(Factory);
			producer2.QR_AQISProducerCode = "A Code";
			producer2.QR_AQISProducerName = "A Name";

			CMRAqisProducerCollection collection = new CMRAqisProducerCollection(Factory);
			collection.Load();
			AssertEquals("First Code in list", "A Name", collection[0].QR_AQISProducerName);
			AssertEquals("Second Code in list", "Name", collection[1].QR_AQISProducerName);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CMRAqisProducerCollection(Factory);
	}
}
