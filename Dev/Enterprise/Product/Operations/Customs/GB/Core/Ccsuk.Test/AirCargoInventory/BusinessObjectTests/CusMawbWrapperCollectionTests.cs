using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	[TestedType(typeof(CusMawbWrapperCollection))]
	public class CusMawbWrapperCollectionTests : NonPersistentBusinessObjectCollectionTestCase<CusMawbWrapperCollection>
	{
		protected override CusMawbWrapperCollection GetCollectionToTest()
		{
			return new CusMawbWrapperCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var cusMawb = Factory.New<CusMAWB>();
			return new CusMawbWrapper(cusMawb);
		}
	}
}
