using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public class TransactionLineEmptyValidation_InnerTest : AccTransactionLinesValidationTest
	{
		public void TestValidateMultipleReversing()
		{
			var creator = new TestObjectCreator(Factory);
			creator.CreateTestPeriods(ZDateTime.Today);
			var invoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV1", creator.AUD, 1m, 10m, 0m, 10m, 0m);
			var line = invoice.Lines[0];
			Factory.Save();

			var expectedErrorMessage = "This is an error for testing";

			AssertEquals("No errors", 0, line.MultipleReversingErrors.Count);
			AssertNoRowError(line, expectedErrorMessage);
			line.RunPreSaveValidation();
			AssertNoRowError(line, expectedErrorMessage);

			line.MultipleReversingErrors.Add(expectedErrorMessage);
			AssertEquals("Has error", 1, line.MultipleReversingErrors.Count);
			AssertNoRowError(line, expectedErrorMessage);
			line.RunPreSaveValidation();
			AssertHasRowError(line, expectedErrorMessage);
		}

		protected override bool IsTaxDateValidationAllowed => false;

		protected override AccTransactionLines GetNewLine(BusinessObjectFactory factory = null) => factory != null ? factory.New<APInvoiceLine>() : Factory.New<APInvoiceLine>();

		protected override AccTransactionLinesValidation GetNewValidation(AccTransactionLines line) => new TransactionLineEmptyValidation((TransactionLine)line);
	}
}
