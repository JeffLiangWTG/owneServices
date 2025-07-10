using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsArrivalAndUnloadingCargoDescValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckSupportingDocumentsMissingForUnloadedItemsWithDifferences()
		{
			var messageError = "You have not entered Unloaded Documents";

			CombineAssertions(() =>
			{
				unloadingGoodsItem.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when no supporting documents declared in arrival and uloading goods items", unloadingGoodsItem, messageError);

				arrivalGoodsItem.SupportingDocuments.AddNew();
				unloadingGoodsItem.Validation.ValidateAll();
				AssertHasRowMessageError("Message error when supporting documents declared only in arrival goods item and unloading goods item has differences", unloadingGoodsItem, messageError);

				unloadingGoodsItem.HasDifferences = false;
				unloadingGoodsItem.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when supporting documents declared only in arrival goods item but unloading goods item doesn't have differences", unloadingGoodsItem, messageError);

				unloadingGoodsItem.HasDifferences = true;
				unloadingGoodsItem.SupportingDocuments.AddNew();
				unloadingGoodsItem.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when supporting documents declared in arrival and unloading goods items", unloadingGoodsItem, messageError);

				var unloadingGoodsItem2 = unloadingMovement.GoodsItems.AddNew();
				unloadingGoodsItem2.BY_LineNo = 2;
				unloadingGoodsItem2.HasDifferences = true;
				unloadingGoodsItem.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when arrival and unloading goods items have different line No", unloadingGoodsItem2, messageError);
			});
		}

		public void TestCheckContainersMissingForUnloadedItemsWithDifferences()
		{
			var messageError = "You have not entered Unloaded Containers";

			CombineAssertions(() =>
			{
				unloadingGoodsItem.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when no containers declared in arrival and uloading goods items", unloadingGoodsItem, messageError);

				arrivalGoodsItem.Containers.AddNew();
				unloadingGoodsItem.Validation.ValidateAll();
				AssertHasRowMessageError("Message error when containers declared only in arrival goods item and unloading goods item has differences", unloadingGoodsItem, messageError);

				unloadingGoodsItem.HasDifferences = false;
				unloadingGoodsItem.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when containers declared only in arrival goods item but unloading goods item doesn't have differences", unloadingGoodsItem, messageError);

				unloadingGoodsItem.HasDifferences = true;
				unloadingGoodsItem.Containers.AddNew();
				unloadingGoodsItem.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when containers declared in arrival and unloading goods items", unloadingGoodsItem, messageError);

				var unloadingGoodsItem2 = unloadingMovement.GoodsItems.AddNew();
				unloadingGoodsItem2.BY_LineNo = 2;
				unloadingGoodsItem2.HasDifferences = true;
				unloadingGoodsItem.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when arrival and unloading goods items have different line No", unloadingGoodsItem2, messageError);
			});
		}

		public void TestCheckPackagesMissingForUnloadedItemsWithDifferences()
		{
			var messageError = "You have not entered Unloaded Packages";

			CombineAssertions(() =>
			{
				unloadingGoodsItem.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when no packages declared in arrival and uloading goods items", unloadingGoodsItem, messageError);

				arrivalGoodsItem.Packages.AddNew();
				unloadingGoodsItem.Validation.ValidateAll();
				AssertHasRowMessageError("Message error when packages declared only in arrival goods item and unloading goods item has differences", unloadingGoodsItem, messageError);

				unloadingGoodsItem.HasDifferences = false;
				unloadingGoodsItem.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when packages declared only in arrival goods item but unloading goods item doesn't have differences", unloadingGoodsItem, messageError);

				unloadingGoodsItem.HasDifferences = true;
				unloadingGoodsItem.Packages.AddNew();
				unloadingGoodsItem.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when packages declared in arrival and unloading goods items", unloadingGoodsItem, messageError);

				var unloadingGoodsItem2 = unloadingMovement.GoodsItems.AddNew();
				unloadingGoodsItem2.BY_LineNo = 2;
				unloadingGoodsItem2.HasDifferences = true;
				unloadingGoodsItem.Validation.ValidateAll();
				AssertNoRowMessageError("No message error when arrival and unloading goods items have different line No", unloadingGoodsItem2, messageError);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalMovement = header.ArrivalMovementHeader;
			arrivalGoodsItem = arrivalMovement.GoodsItems.AddNew();
			arrivalGoodsItem.BY_LineNo = 1;

			unloadingMovement = header.UnloadingMovementHeader;
			unloadingGoodsItem = unloadingMovement.GoodsItems.AddNew();
			unloadingGoodsItem.BY_LineNo = 1;
			unloadingGoodsItem.HasDifferences = true;
		}
		NctsHeader header;
		NctsArrivalAndUnloadingCargoDesc arrivalGoodsItem;
		NctsUnloadingMovementHeader unloadingMovement;
		NctsArrivalAndUnloadingCargoDesc unloadingGoodsItem;
	}
}
