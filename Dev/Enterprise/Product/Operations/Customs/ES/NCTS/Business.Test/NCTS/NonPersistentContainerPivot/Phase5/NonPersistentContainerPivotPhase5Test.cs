using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(NonPersistentContainerPivotPhase5))]
	class NonPersistentContainerPivotPhase5Test : NonPersistentBusinessObjectTestCase
	{
		public void TestContainerSelectedReadOnly()
		{
			(var package, var containerPivot) = CreateNewBusinessObject();
			CombineAssertions(() =>
			{
				package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
				AssertEquals("Selected property read only for arrival when package unloaded state is DIF", true, containerPivot.ContainerSelectedInfo.ReadOnly);
				package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("[Base Behaviour] Selected property not read only for arrival when unloaded state is NEW and Container and GoodItem unloaded state not NEW or DIF", false, containerPivot.ContainerSelectedInfo.ReadOnly);
			});
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject() => CreateNewBusinessObject().ContainerPivot;

		(NctsPackage package, NonPersistentContainerPivotPhase5 ContainerPivot) CreateNewBusinessObject()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var headerContainer = nctsHeader.ArrivalHeaderContainers.AddNew();
			headerContainer.BC_ContainerNum = "CONTAINER1";
			headerContainer.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			var nctsBill = nctsHeader.Bills.AddNew();
			var goodsItem = nctsBill.ArrivalGoodsItems.AddNew();
			goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			var package = goodsItem.Packages.AddNew();
			var containerPivot = ((NonPersistentContainerPivotPhase5)package.ContainersPivotsForBindingOnly[0]);
			containerPivot.ContainerSelected = true;
			return (package, containerPivot);
		}
	}
}
