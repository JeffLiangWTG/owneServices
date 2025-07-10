using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	// Test for validation when under a PIVOT

	public class EUAddInfoTaxValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidationIsHookedUpOK()
		{
			taxLine.G4_MethodOfPayment = "?";
			AssertHasMessageErrors(taxLine.G4_MethodOfPaymentInfo);
		}

		public void TestDuplicateItemsInCollectionBasedOnType()
		{
			taxLine.G4_Type = "A00";
			taxLine.G4_MethodOfPayment = "F";
			AssertNoMessageErrorContaining(taxLine.G4_TypeInfo, "already exists");
			AssertNoMessageErrorContaining(taxLine.G4_MethodOfPaymentInfo, "already exists");
			var taxLineDuplicate = pivot.Taxes.AddNew().Data;
			taxLineDuplicate.G4_Type = "A00";
			taxLineDuplicate.G4_MethodOfPayment = "F";
			AssertHasMessageErrorContaining(taxLineDuplicate.G4_MethodOfPaymentInfo, "already exists");
			taxLineDuplicate.G4_Type = "B00";
			AssertNoMessageErrorContaining(taxLineDuplicate.G4_TypeInfo, "already exists");
			taxLineDuplicate.G4_MethodOfPayment = "Q";
			taxLineDuplicate.G4_Type = "A00";
			AssertNoMessageErrorContaining(taxLineDuplicate.G4_TypeInfo, "already exists");
		}

		public void TestCheckG4_Type_ListValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = Env.CurrentCompany.Country.Code;
			var eunId = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(currentCountry, parent: eunId);
			Factory.Save();

			var dut = helper.CreateNewOrGetExistingRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DUT", "Duty");
			helper.LoadOrCreateNewCusRateCode(Factory, "A00", dut.PK);

			Factory.Save();

			taxLine.G4_Type = "A00";
			AssertNoMessageErrorContaining(taxLine.G4_TypeInfo, "list");
			taxLine.G4_Type = "XXX";
			AssertHasMessageErrorContaining(taxLine.G4_TypeInfo, "list");
		}

		public void TestCheckG4_RateDuty_ListValidation()
		{
			taxLine.G4_Type = UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			taxLine.G4_RateDuty = TaxRateVATDutyListImport.Codes.VATTheGoodsAreLiableToVATAtTheStandardRate; // S for A00 is bad
			AssertHasMessageErrorContaining(taxLine.G4_RateDutyInfo, "list");
			taxLine.G4_RateDuty = TaxRateCustomsDutyListImport.Codes.DutyTheGoodsAreLiableToDutyAtTheFullRateThisIncludesGoodsBeingEnteredForATariffQuotaReliefToWhichNoneOfTheCodesBelowApply; // F for A00 is OK
			AssertNoMessageErrorContaining(taxLine.G4_RateDutyInfo, "list");

			taxLine.G4_Type = UniversalReferenceConstants.RefCusRateCodes.Vat;
			taxLine.G4_RateDuty = TaxRateVATDutyListImport.Codes.VATTheGoodsAreLiableToVATAtTheStandardRate; // S for B00 is OK
			AssertNoMessageErrorContaining(taxLine.G4_RateDutyInfo, "list");
			taxLine.G4_RateDuty = TaxRateCustomsDutyListImport.Codes.DutyTheGoodsAreLiableToDutyAtTheFullRateThisIncludesGoodsBeingEnteredForATariffQuotaReliefToWhichNoneOfTheCodesBelowApply; // F for B00 is bad
			AssertHasMessageErrorContaining(taxLine.G4_RateDutyInfo, "list");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var orgSupplierPart = Factory.New<OrgSupplierPart>();
			orgSupplierPart.OP_PartNum = "Stupid constraints";
			pivot = orgSupplierPart.PivotsForBinding.AddNew();
			pivot.CI_ChildType = "IMP";
			taxLine = pivot.Taxes.AddNew().Data;
		}
		CusClassPartPivot pivot;
		Tax_CusAddInfoOnlyForPIVOT taxLine;
	}
}
