using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Integration.BondedWarehouse;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CMRJobComInvoiceLineTest : TestCaseWithFactory
	{
		public void TestBondedWarehouseTransactionLine()
		{
			var creator = new Customs.Business.Testing.MergedDeclarationCreator<JobDeclaration>(Factory);
			IWhsBondedWarehouseTransactionLine line = ((IBondedWarehouseTransactionLineProvider)creator.InvoiceLine1).TransactionLine;
			AssertEquals("Correct type", typeof(BondedWarehouseTransactionLine), line.GetType());
		}

		public void TestValidationObject()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("IsDrawback", true, declaration.IsDrawback);
			AssertEquals("Validation object", typeof(DrawbackJobComInvoiceLineValidation), invoiceLine.Validation.GetType());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			AssertEquals("IsSACWithoutLines", true, declaration.IsSACWithoutLines);
			AssertEquals("Validation object", typeof(SACWithoutLinesJobComInvoiceLineValidation), invoiceLine.Validation.GetType());

			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			AssertEquals("IsSACWithoutLines", true, declaration.IsSACWithLines);
			AssertEquals("Validation object", typeof(SACJobComInvoiceLineValidation), invoiceLine.Validation.GetType());

			declaration.JE_MessageSubType = "FRM";
			AssertEquals("IsSACWithoutLines", false, declaration.IsSACWithoutLines);
			AssertEquals("Validation object", typeof(IMDJobComInvoiceLineValidation), invoiceLine.Validation.GetType());
		}

		#region Set-up
		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}

		#endregion
	}
}
