using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsUnloadingMovementHeader))]
	sealed class NctsUnloadingMovementHeaderBaseOnlyTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBM_RL_NKDestinationPortReadOnly()
		{
			AssertEquals(false, unloadingMovement.BM_RL_NKDestinationPortInfo.ReadOnly);

			var goodItem = unloadingMovement.GoodsItems.AddNew();
			AssertEquals(false, unloadingMovement.BM_RL_NKDestinationPortInfo.ReadOnly);

			goodItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.China;
			AssertEquals(false, unloadingMovement.BM_RL_NKDestinationPortInfo.ReadOnly);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(Common.EU.NctsMoveHeaderType.Codes.Unloading, unloadingMovement.BM_SubApplicationCode);
		}

		public void TestValidation()
		{
			AssertType<NctsUnloadingMovementHeaderValidation>(unloadingMovement.Validation);
		}

		public void TestLookups()
		{
			AssertType<NctsUnloadingMovementHeaderLookups>(unloadingMovement.Lookups);
		}

		public void TestTotalNumberOfItems()
		{
			var missingGoodsItems = unloadingMovement.GoodsItems.AddNew();
			missingGoodsItems.IsMissing = true;
			unloadingMovement.GoodsItems.AddNew();
			unloadingMovement.GoodsItems.AddNew();
			AssertEquals("TotalNumberOfItems excluding missing goods items", 2, unloadingMovement.TotalNumberOfItems);
		}

		public void TestTotalNumberOfPackages()
		{
			var missingGoodsItems = AddGoodsItems();
			missingGoodsItems.IsMissing = true;
			AddGoodsItems();
			AddGoodsItems();
			AssertEquals("TotalNumberOfPackages excluding missing goods items", 10, unloadingMovement.TotalNumberOfPackages);

			NctsArrivalAndUnloadingCargoDesc AddGoodsItems()
			{
				var result = unloadingMovement.GoodsItems.AddNew();
				var package1 = result.Packages.AddNew();
				package1.B5_UnitCount = 2;
				var package2 = result.Packages.AddNew();
				package2.B5_UnitCount = 3;
				return result;
			}
		}

		public void TestTotalGrossMassInKilograms_IsNotAffectedByGoodsItemsValues()
		{
			nctsHeader.ArrivalMovementHeader.BM_GrossWeight = 1000m;
			nctsHeader.ResetUnloadedValues();
			var missingGoodsItems = AddGoodsItems();
			missingGoodsItems.IsMissing = true;
			AddGoodsItems();
			AddGoodsItems();
			AssertEquals("TotalGrossMassInKilograms is taken from BM_GrossWeight and set by incoming IE43 message", 1000m, unloadingMovement.TotalGrossMassInKilograms);

			NctsArrivalAndUnloadingCargoDesc AddGoodsItems()
			{
				var result = unloadingMovement.GoodsItems.AddNew();
				result.BY_GrossWeight = 12.1m;
				result.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
				return result;
			}
		}

		public void TestSumAndStoreLinesGrossMassIfNotConforming()
		{
			nctsHeader.ArrivalMovementHeader.BM_GrossWeight = 1000m;
			nctsHeader.UnloadingRemark.G9_Conform = YesNoList.Codes.Yes;
			nctsHeader.ResetUnloadedValues();
			var missingGoodsItems = AddGoodsItems();
			missingGoodsItems.IsMissing = true;
			AddGoodsItems();
			AddGoodsItems();
			unloadingMovement.SumAndStoreLinesGrossMassIfNotConforming();
			AssertEquals("TotalGrossMassInKilograms is taken from BM_GrossWeight because CONFORMS=YES", 1000m, unloadingMovement.TotalGrossMassInKilograms);
			nctsHeader.UnloadingRemark.G9_Conform = YesNoList.Codes.No;
			unloadingMovement.SumAndStoreLinesGrossMassIfNotConforming();
			AssertEquals("TotalGrossMassInKilograms is sum of non missing goods items", 24.2m, unloadingMovement.TotalGrossMassInKilograms);

			NctsArrivalAndUnloadingCargoDesc AddGoodsItems()
			{
				var result = unloadingMovement.GoodsItems.AddNew();
				result.BY_GrossWeight = 12.1m;
				result.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
				return result;
			}
		}

		public void TestResetUnloadedGoodsItem()
		{
			var arrivalMovement = nctsHeader.ArrivalMovementHeader;
			arrivalMovement.GoodsItems.AddNew();
			arrivalMovement.GoodsItems.AddNew();
			unloadingMovement.GoodsItems.AddNew();
			unloadingMovement.ResetUnloadedGoodsItem(arrivalMovement.GoodsItems);
			AssertEquals(2, unloadingMovement.GoodsItems.Count);
		}

		public void TestUnloadingTotalGrossMassInKilograms_ReadOnly()
		{
			nctsHeader.UnloadingRemark.G9_Conform = YesNoList.Codes.Yes;
			AssertEquals("pre-req", YesNoList.Codes.Yes, unloadingMovement.Header.UnloadingRemark.G9_Conform);
			AssertEquals("pre-req", true, unloadingMovement.TotalGrossMassInKilogramsInfo.ReadOnly);
			unloadingMovement.Header.UnloadingRemark.G9_Conform = YesNoList.Codes.No;
			AssertEquals("Should be editable if CONFORMS=NO", false, unloadingMovement.TotalGrossMassInKilogramsInfo.ReadOnly);
			unloadingMovement.Header.UnloadingRemark.G9_Conform = YesNoList.Codes.Yes;
			AssertEquals("Should be read-only if CONFORMS=YES", true, unloadingMovement.TotalGrossMassInKilogramsInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject() => unloadingMovement;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			return nctsHeader.UnloadingMovementHeader;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => unloadingMovement;

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			unloadingMovement = nctsHeader.UnloadingMovementHeader;
		}
		NctsHeader nctsHeader;
		NctsUnloadingMovementHeader unloadingMovement;
	}
}
