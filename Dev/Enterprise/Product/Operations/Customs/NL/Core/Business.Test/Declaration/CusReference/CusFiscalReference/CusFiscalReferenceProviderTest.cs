using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(CusFiscalReferenceProvider))]
sealed class CusFiscalReferenceProviderTest : TestCaseWithFactory
{
	public void TestGetByDataGroupingCode()
	{
		var provider = EU.Business.Declaration.CusFiscalReferenceProvider.GetByDataGroupingCode(Core.Constants.CountryCodes.Netherlands);
		CombineAssertions(() =>
		{
			AssertType<CusFiscalReferenceProvider>("Type", provider);
			AssertEquals("DataGroupingCode", Core.Constants.CountryCodes.Netherlands, provider.DataGroupingCode);
		});
	}

	public void TestGetNewValidation_WhenParentIsCustomsEntryInstruction()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var fiscalReference = entryInstruction.FiscalReferences.AddNew();
		var provider = fiscalReference.Provider;
		declaration.JE_MessageType = "IMP";
		AssertType<EntryInstructionCusFiscalReferenceValidation>("Parent is entry instruction", provider.GetNewValidation(fiscalReference));
	}

	public void TestGetNewValidation_WhenParentIsInvoiceLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		var fiscalReference = invoiceLine.FiscalReferences.AddNew();
		var provider = fiscalReference.Provider;
		declaration.JE_MessageType = "IMP";
		AssertType<CusFiscalReferenceValidation>("Parent is invoice line", provider.GetNewValidation(fiscalReference));
	}

	public void TestGetNewLookups()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var cusFiscalReference = entryInstruction.FiscalReferences.AddNew();

		CombineAssertions(() =>
		{
			AssertType<CusFiscalReferenceLookups>(cusFiscalReference.Lookups);

			declaration.JE_MessageType = "IMP";
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusFiscalReference = entryInstruction.FiscalReferences.AddNew();
			AssertType<ImportCusFiscalReferenceLookups>(cusFiscalReference.Lookups);
		});
	}
}
