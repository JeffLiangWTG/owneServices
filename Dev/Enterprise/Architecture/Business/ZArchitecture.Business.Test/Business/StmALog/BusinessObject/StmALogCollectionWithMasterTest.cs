using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmALogCollectionWithMaster))]
	sealed class StmALogCollectionWithMasterTest : BusinessObjectCollectionTestCase
	{
		public void TestMaster()
		{
			var master = Factory.New<DummyEnterpriseBusinessObject>();
			var collection = new StmALogCollectionWithMaster(master);

			AssertSame(master, collection.Master);
			AssertSame(Factory, collection.Factory);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmALogCollectionWithMaster(Factory.New<DummyEnterpriseBusinessObject>());
		}
	}
}
