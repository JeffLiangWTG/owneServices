using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class JobComInvoiceLineTaxValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidationIsHookedUpOK()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			var taxLine = invoiceLine.Taxes.AddNew().Data;
			taxLine.G4_MethodOfPayment = "?";
			AssertHasMessageErrors(taxLine.G4_MethodOfPaymentInfo);
		}

		public void TestDuplicateItemsInCollectionBasedOnType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var taxLine = invoiceLine.Taxes.AddNew().Data;
			taxLine.G4_Type = "A00";
			taxLine.G4_MethodOfPayment = "F";
			AssertNoMessageErrorContaining(taxLine.G4_TypeInfo, "already exists");
			AssertNoMessageErrorContaining(taxLine.G4_MethodOfPaymentInfo, "already exists");
			var taxLineDuplicate = invoiceLine.Taxes.AddNew().Data;
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

			var validG4_Type = ValidG4_Type();
			var dut = helper.CreateNewOrGetExistingRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DUT", "Duty");
			helper.LoadOrCreateNewCusRateCode(Factory, validG4_Type, dut.PK); // A20

			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			JobComInvoiceHeader inv = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = inv.InvoiceLines.AddNew();
			var taxLine = invoiceLine.Taxes.AddNew().Data;
			taxLine.G4_Type = validG4_Type;
			AssertNoMessageErrorContaining(taxLine.G4_TypeInfo, "list");
			taxLine.G4_Type = "XXX";
			AssertHasMessageErrorContaining(taxLine.G4_TypeInfo, "list");
		}

		protected virtual string ValidG4_Type() => UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge;
	}
}
