using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	[TestedType(typeof(CusMawbWrapper))]
	public class CusMawbWrapperTests : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var mawb = Factory.New<CusMAWB>();
			return new CusMawbWrapper(mawb);
		}
	}
}
