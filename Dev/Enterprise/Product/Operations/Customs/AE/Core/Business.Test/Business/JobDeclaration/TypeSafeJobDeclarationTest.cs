using Enterprise.Customs.Business;

namespace Enterprise.Customs.AE.Business.Testing;

public abstract class TypeSafeJobDeclarationTest : Customs.Business.Testing.BaseJobDeclarationTest<JobDeclaration>
{
	public void TestTypeDecider()
	{
		AssertEquals("Update Enterprise.Customs.Business.BaseJobDeclaration to include a decider for this class", typeof(JobDeclaration), Factory.New(typeof(BaseJobDeclaration)).GetType());
	}

	public void TestValidationObject()
	{
		var bizO = Factory.New<JobDeclaration>();
		JobDeclarationValidation firstValidation = bizO.Validation;
		JobDeclarationValidation secondValidation = bizO.Validation;
		Assert(secondValidation != firstValidation);
	}

	public void TestLookupObject()
	{
		var bizO = Factory.New<JobDeclaration>();
		JobDeclarationLookups firstLookup = bizO.Lookups;
		JobDeclarationLookups secondLookup = bizO.Lookups;
		AssertEquals(secondLookup, firstLookup);
	}

	public void TestFilteredInvoiceLines()
	{
		var bizO = Factory.New<JobDeclaration>();
		AssertType<InvoiceLineViewCollection<JobComInvoiceLine>>(bizO.FilteredInvoiceLines);
	}

	public void TestEntryInstructions()
	{
		var bizO = Factory.New<JobDeclaration>();
		AssertType<CusEntryInstructionCollection>(bizO.CustomsEntryInstructions);
	}

	public void TestEntryInstructionProvider()
	{
		var bizO = Factory.New<JobDeclaration>();
		AssertType<EntryInstructionProvider>(bizO.CustomsEntryInstructionProvider);
	}
}
