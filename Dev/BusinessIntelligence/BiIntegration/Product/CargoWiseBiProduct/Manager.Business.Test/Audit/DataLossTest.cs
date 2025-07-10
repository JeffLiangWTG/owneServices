using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business
{
	[TestedType(typeof(DataLoss))]
	class DataLossTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DataLoss("Schema", "Table");
		}

		public void TestDataLoss()
		{
			var dataLoss = new DataLoss("Schema", "Table");
			AssertNotNull(dataLoss);
		}
	}
}
