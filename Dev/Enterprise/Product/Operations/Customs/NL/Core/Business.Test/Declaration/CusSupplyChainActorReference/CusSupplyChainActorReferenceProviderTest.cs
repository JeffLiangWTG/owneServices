using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class CusSupplyChainActorReferenceProviderTest : TestCaseWithFactory
{
	public void TestGetByDataGroupingCode()
	{
		var provider = EU.Business.Declaration.CusSupplyChainActorReferenceProvider.GetByDataGroupingCode(Core.Constants.CountryCodes.Netherlands);
		CombineAssertions(() =>
		{
			AssertType<CusSupplyChainActorReferenceProvider>("Type", provider);
			AssertEquals("DataGroupingCode", Core.Constants.CountryCodes.Netherlands, provider.DataGroupingCode);
		});
	}

	public void TestValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var cusSupplyChainActorReference = entryInstruction.CusSupplyChainActorReferences.AddNew();
		AssertType<CusSupplyChainActorReferenceValidation>(cusSupplyChainActorReference.Validation);

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var cusSupplyChainActorReference_InvoiceLine = invoiceLine.CusSupplyChainActorReferences.AddNew();
		AssertType<CusSupplyChainActorReferenceValidation>(cusSupplyChainActorReference_InvoiceLine.Validation);
	}
}
