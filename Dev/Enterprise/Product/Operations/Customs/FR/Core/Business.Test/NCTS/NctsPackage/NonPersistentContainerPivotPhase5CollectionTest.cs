using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(NonPersistentContainerPivotPhase5Collection))]
	class NonPersistentContainerPivotPhase5CollectionTest : NonPersistentBusinessObjectCollectionTestCase<NonPersistentContainerPivotPhase5Collection>
	{
		protected override NonPersistentContainerPivotPhase5Collection GetCollectionToTest() => new NonPersistentContainerPivotPhase5Collection(Package);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new NonPersistentContainerPivotPhase5(Package, HeaderContainers);

		NctsHeader NctsHeader
		{
			get
			{
				if (nctsHeader == null)
				{
					nctsHeader = Factory.New<NctsHeader>();
					nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
					nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
					headerContainers = NctsHeader.ArrivalHeaderContainers.AddNew();
				}
				return nctsHeader;
			}
		}
		NctsHeader nctsHeader;

		NctsArrivalCargoDesc GoodItem
		{
			get
			{
				if (goodItem == null)
				{
					var nctsBill = NctsHeader.Bills.AddNew();
					goodItem = nctsBill.ArrivalGoodsItems.AddNew();
				}
				return goodItem;
			}
		}
		NctsArrivalCargoDesc goodItem;

		NctsPackage Package => package ?? (package = (NctsPackage)GoodItem.Packages.AddNew());
		NctsPackage package;

		NctsArrivalHeaderContainer HeaderContainers => headerContainers ?? (headerContainers = NctsHeader.ArrivalHeaderContainers.AddNew());
		NctsArrivalHeaderContainer headerContainers;
	}
}
