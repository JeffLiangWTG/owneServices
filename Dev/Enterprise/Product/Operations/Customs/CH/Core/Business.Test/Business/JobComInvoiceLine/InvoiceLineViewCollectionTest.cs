using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(InvoiceLineViewCollection))]
class InvoiceLineCollectionBOTest : Customs.Business.Testing.InvoiceLineCollectionBOTest<InvoiceLineViewCollection>
{
	protected override InvoiceLineViewCollection GetCollectionToTest() => new InvoiceLineViewCollection(JobDeclaration);

	new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

	public void TestInvoiceLineLinkedToDefaultEntryInstruction_Import() => AssertInvoiceLineLinkedToDefaultEntryInstruction(JobMessageTypeList.Codes.Import);
	public void TestInvoiceLineLinkedToDefaultEntryInstruction_Export() => AssertInvoiceLineLinkedToDefaultEntryInstruction(JobMessageTypeList.Codes.Export);

	public void AssertInvoiceLineLinkedToDefaultEntryInstruction(ZString messageType)
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = messageType;

		CombineAssertions(() =>
		{
			var invoiceLine1 = (JobComInvoiceLine)((IBindingList)declaration.FilteredInvoiceLines).AddNew();
			AssertEquals(true, declaration.FilteredInvoiceLines.IsNonCommittedCollectionElement(invoiceLine1));
			AssertEquals(0, declaration.CustomsEntryInstructions.Count);

			var invoiceLine2 = Factory.NewWithValidTestData<JobComInvoiceLine>();
			invoiceLine2.MakeNonPersistent();
			declaration.FilteredInvoiceLines.Add(invoiceLine2);
			AssertEquals(0, declaration.CustomsEntryInstructions.Count);

			var invoiceLine3 = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals(1, declaration.CustomsEntryInstructions.Count);
			var entryInstruction = declaration.CustomsEntryInstructions[0];
			AssertEquals(entryInstruction.PK, invoiceLine3.JI_CEI);

			var invoiceLine4 = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals(1, declaration.CustomsEntryInstructions.Count);
			AssertEquals(entryInstruction.PK, invoiceLine4.JI_CEI);
		});
	}

	public void TestInvoiceLineLinkedToEntryInstruction_Import() => AssertInvoiceLineLinkedToEntryInstruction(JobMessageTypeList.Codes.Import);
	public void TestInvoiceLineLinkedToEntryInstruction_Export() => AssertInvoiceLineLinkedToEntryInstruction(JobMessageTypeList.Codes.Export);

	public void AssertInvoiceLineLinkedToEntryInstruction(ZString messageType)
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = declaration.FilteredInvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals(1, declaration.CustomsEntryInstructions.Count);
			AssertEquals(entryInstruction.PK, invoiceLine.JI_CEI);
		});
	}

	public override void TestAddNew()
	{
		JobDeclaration.Invoices.AddNew();
		base.TestAddNew();
	}
}
