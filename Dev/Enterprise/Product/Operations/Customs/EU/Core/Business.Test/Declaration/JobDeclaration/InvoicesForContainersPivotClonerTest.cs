using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Test
{
	public class TestInvoicesForContainersPivotCloner : TestCaseWithFactory
	{
		public void TestInvoicesForContainersPivotClonerTest()
		{
			JobDeclaration oldDec = Factory.New<JobDeclaration>();
			CusContainer oldContOne = oldDec.CusContainers.AddNew();
			oldContOne.CO_ContainerNumber = "MSCU1234560";
			JobComInvoiceHeader oldInvOne = oldDec.Invoices.AddNew();
			oldInvOne.JZ_InvoiceNumber = "ABC123";
			JobComInvoiceLine oldInvLineOne = oldDec.InvoiceLines.AddNew();
			// container pivot auto created here because 1 container on job
			oldInvLineOne.JI_JZ = oldInvOne.PK;
			oldInvLineOne.ContainersPivot.AddPivotFor(oldContOne);
			CusContainer oldContTwo = oldDec.CusContainers.AddNew();
			oldContTwo.CO_ContainerNumber = "DANU7654321";
			JobComInvoiceHeader oldInvTwo = oldDec.Invoices.AddNew();
			oldInvTwo.JZ_InvoiceNumber = "DEC456";
			JobComInvoiceLine oldInvLineTwo = oldDec.InvoiceLines.AddNew();
			oldInvLineTwo.JI_JZ = oldInvTwo.PK;

			// Set up a second invoice with the same containers and invoices. We are NOT testing the declaration cloning here, we are testing the pivot cloner. 
			JobDeclaration copiedDec = Factory.New<JobDeclaration>();
			CusContainer newContOne = copiedDec.CusContainers.AddNew();
			newContOne.CO_ContainerNumber = "MSCU1234560";
			CusContainer newContTwo = copiedDec.CusContainers.AddNew();
			newContTwo.CO_ContainerNumber = "DANU7654321";
			JobComInvoiceHeader newInvOne = copiedDec.Invoices.AddNew();
			newInvOne.JZ_InvoiceNumber = "ABC123";
			JobComInvoiceLine newInvLineOne = copiedDec.InvoiceLines.AddNew();
			newInvLineOne.JI_JZ = newInvOne.PK;
			JobComInvoiceHeader newInvTwo = copiedDec.Invoices.AddNew();
			newInvTwo.JZ_InvoiceNumber = "DEC456";
			JobComInvoiceLine newInvLineTwo = copiedDec.InvoiceLines.AddNew();
			newInvLineTwo.JI_JZ = newInvTwo.PK;

			AssertEquals("First dec's first invoice should have a pivot", oldDec.InvoiceLines[0].ContainersPivot.Count, 1);
			AssertEquals("First dec's second invoice should not yet have a pivot", oldDec.InvoiceLines[1].ContainersPivot.Count, 0);
			AssertEquals("Copied dec's first invoice should not yet have any pivots", copiedDec.InvoiceLines[0].ContainersPivot.Count, 0);
			AssertEquals("Copied dec's second invoice should not yet have any pivots", copiedDec.InvoiceLines[1].ContainersPivot.Count, 0);

			InvoicesForContainersPivotCloner cloner = new InvoicesForContainersPivotCloner(oldDec, copiedDec);
			cloner.CloneContainerInvoicePivots();

			AssertEquals("Copied dec's first invoice should now have a pivot", copiedDec.InvoiceLines[0].ContainersPivot.Count, 1);
			AssertEquals("Copied dec's second invoice should still not have a pivot", copiedDec.InvoiceLines[1].ContainersPivot.Count, 0);
			AssertEquals("Copied dec's first invoice's pivot should point to the right inv", copiedDec.InvoiceLines[0].ContainersPivot[0].C2_JI, copiedDec.InvoiceLines[0].PK);
			AssertEquals("Copied dec's first invoice's pivot should point to the right cont by ID", copiedDec.InvoiceLines[0].ContainersPivot[0].C2_CO, copiedDec.CusContainers[0].PK);
			AssertEquals("Copied dec's first invoice's pivot should point to the right cont", copiedDec.InvoiceLines[0].ContainersPivot[0].Container.CO_ContainerNumber, "MSCU1234560");
		}
	}
}
