using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business
{
	[TestedType(typeof(CdcHistoryCollection))]
	class CdcHistoryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CdcHistoryCollection>
	{
		protected override CdcHistoryCollection GetCollectionToTest()
		{
			return new CdcHistoryCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CdcHistory("", "");
		}
	}
}
