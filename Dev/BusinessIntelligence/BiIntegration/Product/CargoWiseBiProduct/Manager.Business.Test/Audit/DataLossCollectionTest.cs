using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business
{
	[TestedType(typeof(DataLossCollection))]
	class DataLossCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DataLossCollection>
	{
		protected override DataLossCollection GetCollectionToTest()
		{
			return new DataLossCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DataLoss("", "");
		}
	}
}
