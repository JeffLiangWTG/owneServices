using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing.KoreaSouth.Testing
{
	public class DocumentSupporterDataCheckerForKRElectronicInvoiceTest : TestCaseWithFactory
	{
		public void TestReportError_AwaitingApprovalFromGovt()
		{
			var errorMessage = "The Electronic Invoice Document cannot be printed as it has not been approved by the National Tax Service.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.KoreaSouth))
			{
				Pivot.AIP_Status = EInvoicingPivotState.Sent;
				var documentSupporterDataState = DocumentSupporterAR.GetDataStateBeforeRun(MenuItem);
				AssertEquals("IsAwaitingApprovalFromGovt", ARInvoice.IsAwaitingApprovalFromGovt, true);
				AssertEquals("Error message is shown when the status isn't SUC", errorMessage, documentSupporterDataState.ErrorMessage);

				Pivot.AIP_Status = EInvoicingPivotState.Succeed;
				documentSupporterDataState = DocumentSupporterAR.GetDataStateBeforeRun(MenuItem);
				AssertEquals("IsAwaitingApprovalFromGovt", ARInvoice.IsAwaitingApprovalFromGovt, false);
				AssertEquals("Document can be printed (No error message dialog)", true, documentSupporterDataState.IsValid);
			}
		}

		public void TestReportErrorWhenOverLength()
		{
			var errorMessage = "The Electronic Invoice Document cannot be printed as the total invoice amount exceeded the maximum invoice amount supported by this document layout.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.KoreaSouth))
			{
				ARInvoice.AH_InvoiceAmount = 1000000000000M;
				var documentSupporterDataState = DocumentSupporterAR.GetDataStateBeforeRun(MenuItem);
				AssertEquals("The total amount overlength", true, ARInvoice.AH_InvoiceAmount.Truncate().ToString().Length > 12);
				AssertEquals("Error message is shown when the display length exceeds maximum", errorMessage, documentSupporterDataState.ErrorMessage);

				ARInvoice.AH_InvoiceAmount = -100000000000M;
				documentSupporterDataState = DocumentSupporterAR.GetDataStateBeforeRun(MenuItem);
				AssertEquals("The total amount overlength", true, ARInvoice.AH_InvoiceAmount.Truncate().ToString().Length > 12);
				AssertEquals("Error message is shown when the display length exceeds maximum", errorMessage, documentSupporterDataState.ErrorMessage);

				ARInvoice.AH_InvoiceAmount = 100000000000.00M;
				documentSupporterDataState = DocumentSupporterAR.GetDataStateBeforeRun(MenuItem);
				AssertEquals("Document can be printed (No error message dialog)", true, documentSupporterDataState.IsValid);

				ARInvoice.AH_InvoiceAmount = -10000000000.00M;
				documentSupporterDataState = DocumentSupporterAR.GetDataStateBeforeRun(MenuItem);
				AssertEquals("Document can be printed (No error message dialog)", true, documentSupporterDataState.IsValid);
			}
		}

		public void TestReportErrorWhenErrorTransactionTypes()
		{
			var errorMessage = "The following transaction(s) cannot be printed.\r\nTransaction " + ARInvoice.AH_TransactionNum + " cannot be printed because it is not an Electronic Invoice Document.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.KoreaSouth))
			{
				var documentSupporterDataState = DocumentSupporterAR.GetDataStateBeforeRun(MenuItem);
				AssertEquals("Precondition ", true, ARInvoice.IsEligibleToCreateEInvoicingTransactionPivot);
				AssertEquals("Document can be printed (No error message dialog)", true, documentSupporterDataState.IsValid);

				ARInvoice.AH_TransactionType = TransactionTypes.Discount;
				documentSupporterDataState = DocumentSupporterAR.GetDataStateBeforeRun(MenuItem);
				AssertEquals("Precondition ", false, ARInvoice.IsEligibleToCreateEInvoicingTransactionPivot);
				AssertEquals("Error message is shown when the TransactionTypes error", errorMessage, documentSupporterDataState.ErrorMessage);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			ARInvoice = Factory.NewWithValidTestData<ARInvoice>(); 
			TestObjectCreator.CreateInvoiceLine(ARInvoice, ARInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
			Assert("PreCondition", ARInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));
			DocumentSupporterAR = InvoicingBaseDocumentSupporter.New(ARInvoice);

			ARInvoice.AH_TransactionType = TransactionTypes.Invoice;
			ARInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			Batch = TestObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			Pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(Batch, ARInvoice, EInvoicingPivotState.Succeed);
			MenuItem = Factory.New<IStmMenuItem>();
			MenuItem.SU_MenuName = "KR Electronic Invoice";
			Factory.Save();
		}

		ARInvoice ARInvoice;
		AccEInvoicingBatch Batch;
		IStmMenuItem MenuItem;
		AccEInvoicingTransactionPivot Pivot;
		InvoicingBaseDocumentSupporter DocumentSupporterAR;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
