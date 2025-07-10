using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class TreatmentActiveIngredientValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateProduceType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			const string messageError = "Treatment Active Ingredients may only be present when Produce Type is Horticulture or Grains and Seeds";
			var header = Factory.New<JobComInvoiceHeader>();
			header.JZ_JE = declaration.PK;
			var quarantineHeader = header.QuarantineExDocHeader;
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			var quarantineLine = line.QuarantineExDocLine;
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			var process = quarantineLine.Processes.AddNew();
			var ingredient = process.Ingredients.AddNew();

			CombineAssertions(() =>
			{
				declaration.RunPreSaveValidation();
				AssertHasRowMessageError("Produce type is dairy", ingredient, messageError);
				quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.GrainsAndPlants;
				declaration.RunPreSaveValidation();
				AssertNoRowMessageError("Produce type is grain", ingredient, messageError);
			});
		}
	}
}
