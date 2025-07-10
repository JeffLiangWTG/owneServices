using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NonPersistentDepartureContainerPivot))]
	class NonPersistentContainerPivotTest : NonPersistentBusinessObjectTestCase
	{
		public void TestRefContainer()
		{
			var refContainer = Factory.New<RefContainer>();
			var containerPivot = (NonPersistentDepartureContainerPivot)GetNewBusinessObject();
			containerPivot.Container.BC_RC = refContainer.PK;
			AssertEquals(refContainer.PK, containerPivot.RefContainer.PK);
		}

		public void TestContainerType()
		{
			var containerPivot = (NonPersistentDepartureContainerPivot)GetNewBusinessObject();
			containerPivot.Container.BC_RC = ZGuid.BrettsGuid;
			AssertEquals(ZGuid.BrettsGuid, containerPivot.ContainerType);

			containerPivot.ContainerType = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, containerPivot.Container.BC_RC);

			Assert(containerPivot.ContainerTypeInfo.ReadOnly);
		}

		public void TestContainerMode()
		{
			var containerPivot = (NonPersistentDepartureContainerPivot)GetNewBusinessObject();
			containerPivot.Container.BC_Mode = "LCL";
			AssertEquals("LCL", containerPivot.ContainerMode);

			containerPivot.ContainerMode = "FCL";
			AssertEquals("FCL", containerPivot.Container.BC_Mode);

			Assert(containerPivot.ContainerModeInfo.ReadOnly);
		}

		public void TestContainerSelected()
		{
			CombineAssertions(() =>
			{
				var containerPivot = (NonPersistentDepartureContainerPivot)GetNewBusinessObject();
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

		public void TestContainerSelectedPivot()
		{
			(var goodsItem, var containerPivot) = CreateNewBusinessObject();
			CombineAssertions(() =>
			{
				var pivot = containerPivot.ContainerSelectedPivot;
				AssertEquals("XX_RelationType", Core.Constants.GenPivotTypes.CusNctsContainer, pivot.XX_RelationType);
				AssertEquals("XX_Relation1ID", goodsItem.PK, pivot.XX_Relation1ID);
				AssertEquals("XX_Relation2ID", containerPivot.Container.PK, pivot.XX_Relation2ID);
				AssertEquals("XX_Relation1TableCode", CusInBondCargoDescSchema.Constants.Prefix, pivot.XX_Relation1TableCode);
				AssertEquals("XX_Relation2TableCode", CusInBondContainerSchema.Constants.Prefix, pivot.XX_Relation2TableCode);
			});
		}

		public void TestContainerSelectedReadOnly_Departure()
		{
			(var goodsItem, var containerPivot) = CreateNewBusinessObject();
			AssertEquals("Selected property not read only for departure", false, containerPivot.ContainerSelectedInfo.ReadOnly);
		}

		public void TestContainerSelectedReadOnly_Arrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var headerContainer = nctsHeader.ArrivalHeaderContainers.AddNew();
			headerContainer.BC_ContainerNum = "CONTAINER1";
			var goodsItem = nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
			var containerPivot = goodsItem.ContainersPivots.AddNew();
			containerPivot.ContainerSelected = true;

			AssertEquals("Selected property read only for arrival", true, containerPivot.ContainerSelectedInfo.ReadOnly);
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject() => CreateNewBusinessObject().ContainerPivot;

		(NctsDepartureCargoDesc GoodsItem, NonPersistentDepartureContainerPivot ContainerPivot) CreateNewBusinessObject()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var headerContainer = nctsHeader.DepartureHeaderContainers.AddNew();
			headerContainer.BC_ContainerNum = "CONTAINER1";
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			var containerPivot = goodsItem.ContainersPivots[0];
			containerPivot.ContainerSelected = true;
			return (goodsItem, containerPivot);
		}
	}
}
