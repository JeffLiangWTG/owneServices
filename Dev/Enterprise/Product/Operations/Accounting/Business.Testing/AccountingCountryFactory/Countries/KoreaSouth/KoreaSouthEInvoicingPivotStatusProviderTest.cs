using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class KoreaSouthEInvoicingPivotStatusProviderTest : EInvoicingPivotStatusProviderTestBase<KoreaSouthEInvoicingPivotStatusProvider>
	{
		protected override string CountryCode => CountryCodes.KoreaSouth;

		public void TestGetInitialPivotStatus_ForARInvoice()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			{
				var transaction = Factory.NewWithValidTestData<APCreditNote>();

				AssertNull("Precondition: Is NOT an amendment invoice", transaction.OriginalTransaction);

				AssertInitialPivotStatusEquals(transaction, null, "Amendment transaction status should be null.");
			}
		}

		public void TestGetInitialPivotStatus_ForAmendmentARInvoice()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			{
				var invoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(invoice1, invoice1.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				Assert("PreCondition", invoice1.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

				var invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("AR002", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(invoice2, invoice2.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				Assert("PreCondition", invoice2.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));
				Factory.Save();

				invoice1.GetMostRecentEInvoicingTransactionPivot().AIP_Status = EInvoicingPivotState.Delivered;
				invoice2.GetMostRecentEInvoicingTransactionPivot().AIP_Status = EInvoicingPivotState.Succeed;
				Factory.Save();

				var amendmentInvoice1 = TestObjectCreator.AmendARTransaction(TransactionTypes.Invoice, invoice1).amendTransaction as InvoicingBase;
				var amendmentInvoice2 = TestObjectCreator.AmendARTransaction(TransactionTypes.Invoice, invoice2).amendTransaction as InvoicingBase;
				Factory.Save();

				AssertNotNull("Precondition: Is an amendment invoice", amendmentInvoice1.OriginalTransaction);
				AssertEquals("Precondition: Still in E-Invoicing", false, (amendmentInvoice1.OriginalTransaction as InvoicingBase).IsApprovedByGovt);
				AssertInitialPivotStatusEquals(amendmentInvoice1, EInvoicingPivotState.Pending, "Amendment transaction status should be Pending");

				AssertNotNull("Precondition: Is an amendment invoice", amendmentInvoice2.OriginalTransaction);
				AssertEquals("Precondition: Successfully E-Invoiced", true, (amendmentInvoice2.OriginalTransaction as InvoicingBase).IsApprovedByGovt);
				AssertInitialPivotStatusEquals(amendmentInvoice2, null, "Amendment transaction status should be null. The status will later be evaluated by standard logic");
			}
		}

		public void TestGetInitialPivotStatus_ForARCreditNote()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			{
				var arCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
				
				AssertNull("Precondition", arCreditNote.OriginalTransaction);
				AssertEquals("Precondition", true, arCreditNote.IsARCreditNote);
				AssertInitialPivotStatusEquals(arCreditNote, expectedStatus: null);
				AssertInitialPivotStatusEquals(transaction: null, expectedStatus: null);
			}
		}

		public void TestGetInitialPivotStatus_ForARCreditNote_GeneratedByReverse()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			{
				var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(arInvoice1, arInvoice1.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				Assert("PreCondition", arInvoice1.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

				var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("AR002", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(arInvoice2, arInvoice2.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				Assert("PreCondition", arInvoice2.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));
				Factory.Save();

				arInvoice1.GetMostRecentEInvoicingTransactionPivot().AIP_Status = EInvoicingPivotState.Delivered;
				arInvoice2.GetMostRecentEInvoicingTransactionPivot().AIP_Status = EInvoicingPivotState.Succeed;
				Factory.Save();

				new ReversingFactory().NewReversing(arInvoice1).Reverse();
				var reverseTransactionForARInvoice1 = arInvoice1.ReverseInvoice;
				new ReversingFactory().NewReversing(arInvoice2).Reverse();
				var reverseTransactionForARInvoice2 = arInvoice2.ReverseInvoice;
				Factory.Save();

				AssertNotNull("Precondition", reverseTransactionForARInvoice1.OriginalTransaction);
				AssertEquals("Precondition", true, reverseTransactionForARInvoice1.IsARCreditNote);
				AssertEquals("Precondition", false, (reverseTransactionForARInvoice1.OriginalTransaction as InvoicingBase).IsApprovedByGovt);
				AssertInitialPivotStatusEquals(reverseTransactionForARInvoice1, EInvoicingPivotState.Pending);

				AssertNotNull("Precondition", reverseTransactionForARInvoice2.OriginalTransaction);
				AssertEquals("Precondition", true, reverseTransactionForARInvoice2.IsARCreditNote);
				AssertEquals("Precondition", true, (reverseTransactionForARInvoice2.OriginalTransaction as InvoicingBase).IsApprovedByGovt);
				AssertInitialPivotStatusEquals(reverseTransactionForARInvoice2, expectedStatus: null);
			}
		}

		public void TestGetInitialPivotStatus_ForARCreditNote_GeneratedByAmend()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			{
				var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(arInvoice1, arInvoice1.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				Assert("PreCondition", arInvoice1.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

				var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("AR002", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(arInvoice2, arInvoice2.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				Assert("PreCondition", arInvoice2.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));
				Factory.Save();

				arInvoice1.GetMostRecentEInvoicingTransactionPivot().AIP_Status = EInvoicingPivotState.Delivered;
				arInvoice2.GetMostRecentEInvoicingTransactionPivot().AIP_Status = EInvoicingPivotState.Succeed;
				Factory.Save();

				var amendTransactionForARInvoice1 = TestObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, arInvoice1).amendTransaction as InvoicingBase;
				var amendTransactionForARInvoice2 = TestObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, arInvoice2).amendTransaction as InvoicingBase;
				Factory.Save();

				AssertNotNull("Precondition", amendTransactionForARInvoice1.OriginalTransaction);
				AssertEquals("Precondition", true, amendTransactionForARInvoice1.IsARCreditNote);
				AssertEquals("Precondition", false, (amendTransactionForARInvoice1.OriginalTransaction as InvoicingBase).IsApprovedByGovt);
				AssertInitialPivotStatusEquals(amendTransactionForARInvoice1, EInvoicingPivotState.Pending);

				AssertNotNull("Precondition", amendTransactionForARInvoice2.OriginalTransaction);
				AssertEquals("Precondition", true, amendTransactionForARInvoice2.IsARCreditNote);
				AssertEquals("Precondition", true, (amendTransactionForARInvoice2.OriginalTransaction as InvoicingBase).IsApprovedByGovt);
				AssertInitialPivotStatusEquals(amendTransactionForARInvoice2, expectedStatus: null);
			}
		}
	}
}
