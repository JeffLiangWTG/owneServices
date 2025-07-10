using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(NonPersistentDepartureContainerPivotCollection))]
	public class NonPersistentContainerPivotCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NonPersistentDepartureContainerPivotCollection>
	{
		protected override NonPersistentDepartureContainerPivotCollection GetCollectionToTest() => new NonPersistentDepartureContainerPivotCollection(nctsHeader.MovementHeader.GoodsItems.AddNew());

		protected override CargoWise.EntityFramework.BusinessObject GetNewElementToAddToTheCollection() => new NonPersistentDepartureContainerPivot(nctsHeader.MovementHeader.GoodsItems.AddNew());

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		}
		NctsHeader nctsHeader;
	}
}
