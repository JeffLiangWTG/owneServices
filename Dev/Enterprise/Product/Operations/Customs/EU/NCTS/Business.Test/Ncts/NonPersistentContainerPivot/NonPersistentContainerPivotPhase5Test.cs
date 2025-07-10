using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NonPersistentContainerPivotPhase5))]
	sealed class NonPersistentContainerPivotPhase5Test : NonPersistentBusinessObjectTestCase
	{
		public void TestRefContainer()
		{
			var refContainer = Factory.New<RefContainer>();
			var containerPivot = (NonPersistentContainerPivotPhase5)GetNewBusinessObject();
			containerPivot.Container.BC_RC = refContainer.PK;
			AssertEquals(refContainer.PK, containerPivot.RefContainer.PK);
		}

		public void TestContainerType()
		{
			var containerPivot = (NonPersistentContainerPivotPhase5)GetNewBusinessObject();
			containerPivot.Container.BC_RC = ZGuid.BrettsGuid;
			AssertEquals(ZGuid.BrettsGuid, containerPivot.ContainerType);

			Assert(containerPivot.ContainerTypeInfo.ReadOnly);
		}

		public void TestContainerMode()
		{
			var containerPivot = (NonPersistentContainerPivotPhase5)GetNewBusinessObject();
			containerPivot.Container.BC_Mode = "LCL";
			AssertEquals("LCL", containerPivot.ContainerMode);

			Assert(containerPivot.ContainerModeInfo.ReadOnly);
		}

		public void TestContainerSelected()
		{
			CombineAssertions(() =>
			{
				var containerPivot = (NonPersistentContainerPivotPhase5)GetNewBusinessObject();
				Factory.Save();
				var pivot = containerPivot.ContainerSelectedPivot;
				AssertEquals("Container is selected", true, containerPivot.ContainerSelected);

				containerPivot.ContainerSelected = false;
				Factory.Save();
				AssertEquals("Pivot is deleted", true, pivot.IsDeleted);
				AssertEquals("Container isn't selected", false, containerPivot.ContainerSelected);

				containerPivot.ContainerSelected = true;
				AssertNotEquals("Pivot is regenerated", pivot.PK, containerPivot.ContainerSelectedPivot.PK);
			});
		}

		public void TestContainerHasChanges()
		{
			(var package, var containerPivot) = CreateNewBusinessObject();
			Factory.Save();
			containerPivot.ContainerSelected = false;

			CombineAssertions(() =>
			{
				AssertEquals("When ContainerSelected changes, package should have changes", true, package.HasChanges);
				Factory.Save();
				AssertEquals("After Save, package should not have changes", false, package.HasChanges);
			});
		}

		public void TestContainerSelectedPivot()
		{
			(var package, var containerPivot) = CreateNewBusinessObject();
			CombineAssertions(() =>
			{
				var pivot = containerPivot.ContainerSelectedPivot;
				AssertEquals("XX_RelationType", Core.Constants.GenPivotTypes.CusNctsContainer, pivot.XX_RelationType);
				AssertEquals("XX_Relation1ID", package.PK, pivot.XX_Relation1ID);
				AssertEquals("XX_Relation2ID", containerPivot.Container.PK, pivot.XX_Relation2ID);
				AssertEquals("XX_Relation1TableCode", CusInvPackSchema.Constants.Prefix, pivot.XX_Relation1TableCode);
				AssertEquals("XX_Relation2TableCode", CusInBondContainerSchema.Constants.Prefix, pivot.XX_Relation2TableCode);
			});
		}

		public void TestContainerSelectedReadOnly_Arrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var headerContainer = nctsHeader.ArrivalHeaderContainers.AddNew();
			headerContainer.BC_ContainerNum = "CONTAINER1";
			var package = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew().Packages.AddNew();

			var containerPivot = package.ContainersPivotsForBindingOnly[0];
			containerPivot.ContainerSelected = true;
			containerPivot.Container.BC_UnloadedState = ZString.Empty;

			CombineAssertions(() =>
			{
				package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("Selected property not read only for arrival when unloaded state is DEC", false, containerPivot.ContainerSelectedInfo.ReadOnly);
				package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("Selected property read only for arrival when unloaded state is NEW", false, containerPivot.ContainerSelectedInfo.ReadOnly);

				package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;

				var goodsItem = package.Parent;
				goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				AssertEquals("Selected property not Read Only for arrival when Parent Goods Item unloaded state is DIF", false, containerPivot.ContainerSelectedInfo.ReadOnly);

				goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("Selected property not Read Only for arrival when Parent Goods Item unloaded state is NEW", false, containerPivot.ContainerSelectedInfo.ReadOnly);

				goodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("Selected property Read Only for arrival when Parent Goods Item unloaded state is DEC", true, containerPivot.ContainerSelectedInfo.ReadOnly);

				var container = containerPivot.Container;
				container.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("Selected property Read Only for arrival when Container unloaded state is DEC", true, containerPivot.ContainerSelectedInfo.ReadOnly);

				container.BC_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("Selected property not Read Only for arrival when Container unloaded state is NEW", false, containerPivot.ContainerSelectedInfo.ReadOnly);

				container.BC_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				AssertEquals("Selected property not Read Only for arrival when Container unloaded state is DIF", false, containerPivot.ContainerSelectedInfo.ReadOnly);
			});
		}

		public void TestContainerSelectedReadOnly_Departure()
		{
			var (_, containerPivot) = CreateNewBusinessObject();
			AssertEquals("Selected property not read only for departure", false,
				containerPivot.ContainerSelectedInfo.ReadOnly);
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject() => CreateNewBusinessObject().ContainerPivot;

		(NctsPackage Package, NonPersistentContainerPivotPhase5 ContainerPivot) CreateNewBusinessObject()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var headerContainer = nctsHeader.DepartureHeaderContainers.AddNew();
			headerContainer.BC_ContainerNum = "CONTAINER1";
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var package = goodsItem.Packages.AddNew();
			var containerPivot = package.ContainersPivotsForBindingOnly[0];
			containerPivot.ContainerSelected = true;
			return (package, containerPivot);
		}
	}
}
