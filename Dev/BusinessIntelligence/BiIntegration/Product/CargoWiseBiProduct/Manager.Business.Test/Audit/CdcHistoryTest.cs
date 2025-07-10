using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business
{
	[TestedType(typeof(CdcHistory))]
	class CdcHistoryTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CdcHistory("Schema", "Table");
		}

		public void TestCdcHistory()
		{
			var cdcHistory = new CdcHistory("Schema", "Table");
			AssertNotNull(cdcHistory);
		}
	}
}
