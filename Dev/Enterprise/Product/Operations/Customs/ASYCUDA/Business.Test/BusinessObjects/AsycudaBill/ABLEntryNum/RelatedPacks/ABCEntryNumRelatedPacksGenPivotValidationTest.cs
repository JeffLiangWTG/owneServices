using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class ABCEntryNumRelatedPacksGenPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckXX_Relation2ID()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var entryNum = bill.CustomsEntryNumbers.AddNew();

			var pack1 = entryNum.Bill.Packs.AddNew();
			var pack2 = entryNum.Bill.Packs.AddNew();

			var pivot1 = entryNum.PackPivots.AddPivotFor(pack1);
			var pivot2 = entryNum.PackPivots.AddPivotFor(pack2);

			AssertNoMessageError(pivot1.XX_Relation2IDInfo, pivot1.Validation.DuplicatePack);
			AssertNoMessageError(pivot2.XX_Relation2IDInfo, pivot2.Validation.DuplicatePack);

			var pivot3 = entryNum.PackPivots.AddNew();
			pivot3.Relation2Object = pack2;
			AssertHasError(pivot3.XX_Relation2IDInfo, pivot3.Validation.DuplicatePack);

			pack2.APA_VINNumber = "VIN1";
			pivot2.Validation.ValidateAll();
			pivot3.Validation.ValidateAll();
			AssertHasMessageError(pivot2.XX_Relation2IDInfo, pivot2.Validation.Only1AssociatedLRNAllowedFor1Pack);
			AssertHasMessageError(pivot3.XX_Relation2IDInfo, pivot3.Validation.Only1AssociatedLRNAllowedFor1Pack);
			pack2.APA_VINNumber = "";
			pivot2.Validation.ValidateAll();
			pivot3.Validation.ValidateAll();
			AssertNoMessageError(pivot2.XX_Relation2IDInfo, pivot2.Validation.Only1AssociatedLRNAllowedFor1Pack);
			AssertNoMessageError(pivot3.XX_Relation2IDInfo, pivot3.Validation.Only1AssociatedLRNAllowedFor1Pack);
		}
	}
}
