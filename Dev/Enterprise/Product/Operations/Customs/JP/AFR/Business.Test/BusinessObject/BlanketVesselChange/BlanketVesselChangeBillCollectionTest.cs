using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(BlanketVesselChangeBillCollection))]
	class BlanketVesselChangeBillCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BlanketVesselChangeBillCollection>
	{
		protected override BlanketVesselChangeBillCollection GetCollectionToTest() => new BlanketVesselChangeBillCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			return new BlanketVesselChangeBill(bill, new BlanketVesselChange(header));
		}
	}
}
