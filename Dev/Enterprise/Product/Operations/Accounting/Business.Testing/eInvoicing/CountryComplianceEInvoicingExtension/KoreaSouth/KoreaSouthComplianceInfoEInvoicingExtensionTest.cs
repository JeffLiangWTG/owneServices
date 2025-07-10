using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing.Testing
{
	[TestedType(typeof(KoreaSouthComplianceInfoEInvoicingExtension))]
	public class KoreaSouthComplianceInfoEInvoicingExtensionTest : KoreaSouthComplianceInfoTest
	{
		#region IComplianceInfoEInvoicingGUIActionStatusRequest

		public void TestIComplianceInfoEInvoicingGUIActionStatusRequest_StatusRequestMenuName()
		{
			AssertEquals("Request e-Invoice Status", (Extension as IComplianceInfoEInvoicingGUIActionStatusRequest).StatusRequestMenuName);
		}

		public void TestIComplianceInfoEInvoicingGUIActionStatusRequest_StatusRequestActionInformation()
		{
			var expected = @"Your request is being processed.

Please note:
1. Only transactions that have an E-Reporting Status of 'FAL - Failed' are eligible for 'Request e-Invoice Status'.
2. This action will update the E-Reporting Status of transactions in the same batch as the currently selected transaction(s) to 'DLV - Delivered' and submit a request to NTS to check on the invoice submission status.";
			AssertEquals(expected, (Extension as IComplianceInfoEInvoicingGUIActionStatusRequest).StatusRequestActionInformation);
		}

		#endregion

		#region IComplianceInfoEInvoicingGUIActionProvider

		public void TestGetEligibleInvoices_WithNullParameter()
		{
			var complianceInfo = Extension as IComplianceInfoEInvoicingGUIActionProvider;
			AssertNoExceptionThrown(() => complianceInfo.GetEligibleInvoices(null));
		}

		public void TestGetEligibleInvoices()
		{
			var provider = Extension as IComplianceInfoEInvoicingGUIActionProvider;
			AssertNotNull("Precondition", provider);

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.Today.AddDays(-1)))
			{
				var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("Test001", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(arInvoice1, arInvoice1.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				Assert("PreCondition", arInvoice1.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

				var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("Test002", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(arInvoice2, arInvoice2.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				Assert("PreCondition", arInvoice2.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

				var arInvoice3 = TestObjectCreator.CreateARInvoice<ARInvoice>("Test003", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(arInvoice3, arInvoice3.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				Assert("PreCondition", arInvoice3.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));
				Factory.Save();

				var pivotSUB1 = arInvoice1.GetMostRecentEInvoicingTransactionPivot();
				var pivotSUB2 = arInvoice2.GetMostRecentEInvoicingTransactionPivot();
				var pivotSUB3 = arInvoice3.GetMostRecentEInvoicingTransactionPivot();

				var batchSUB1 = TestObjectCreator.CreateEInvoicingBatch(10001, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				var batchSUB2 = TestObjectCreator.CreateEInvoicingBatch(10002, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				pivotSUB1.AIP_AIB = batchSUB1.PK;
				pivotSUB2.AIP_AIB = batchSUB1.PK;
				pivotSUB3.AIP_AIB = batchSUB2.PK;
				Factory.Save();

				AssertContainsExactElementsInAnyOrder("Should be grouped by EInvoicingBatchNumber and then only the first Invoice from each group should be returned",
					new AccTransactionHeader[] { arInvoice1, arInvoice3 }, provider.GetEligibleInvoices(new AccTransactionHeader[] { arInvoice1, arInvoice2, arInvoice3 }));
			}
		}

		public void TestExistActiveDocumentRequestPivot()
		{
			var complianceInfo = Extension as IComplianceInfoEInvoicingGUIActionProvider;

			AssertExceptionThrown<NotImplementedException>("Should throw NotImplementedException because Korea eInvoicing does not support relative feature",
				() => complianceInfo.ExistActiveDocumentRequestPivot(ZDateTime.Empty));
		}

		#endregion

		public void TestGetCantAmendWithCRDErrorMessage()
		{
			var expectedError = "Amendments to Stand-alone AR Credit Notes are not supported in Korea, please reverse the transaction in Receivables Transactions.";

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.KRW, 1.0m);
			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.KRW, 1.0m);
			var arCreditNote = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), TestObjectCreator.KRW, 1.0m);
			var apCreditNote = TestObjectCreator.CreateInvoice(typeof(APCreditNote), TestObjectCreator.KRW, 1.0m);

			using (testObjectCreator.SetUpForTestingEInvoicing(Core.Constants.CountryCodes.KoreaSouth, false))
			{
				AssertNullOrEmpty("The return value for ARInvoice", Extension.GetCantAmendErrorMessage(arInvoice, TransactionTypes.Invoice));
				AssertNullOrEmpty("The return value for ARInvoice", Extension.GetCantAmendErrorMessage(arInvoice, TransactionTypes.CreditNote));
				AssertNullOrEmpty("The return value for ARCreditNote", Extension.GetCantAmendErrorMessage(arCreditNote, TransactionTypes.Invoice));
				AssertNullOrEmpty("The return value for ARCreditNote", Extension.GetCantAmendErrorMessage(arCreditNote, TransactionTypes.CreditNote));
			}

			using (testObjectCreator.SetUpForTestingEInvoicing(Core.Constants.CountryCodes.KoreaSouth, true))
			{
				AssertNullOrEmpty("The return value for APInvoice", Extension.GetCantAmendErrorMessage(apInvoice, TransactionTypes.Invoice));
				AssertNullOrEmpty("The return value for APInvoice", Extension.GetCantAmendErrorMessage(apInvoice, TransactionTypes.CreditNote));
				AssertNullOrEmpty("The return value for APCreditNote", Extension.GetCantAmendErrorMessage(apCreditNote, TransactionTypes.Invoice));
				AssertNullOrEmpty("The return value for APCreditNote", Extension.GetCantAmendErrorMessage(apCreditNote, TransactionTypes.CreditNote));

				arInvoice.OriginalTransactionReference = ZGuid.NewZGuid();
				arCreditNote.OriginalTransactionReference = ZGuid.NewZGuid();
				AssertNullOrEmpty("The return value for ARInvoice with non-empty OriginalTransactionReference", Extension.GetCantAmendErrorMessage(arInvoice, TransactionTypes.Invoice));
				AssertNullOrEmpty("The return value for ARInvoice with non-empty OriginalTransactionReference", Extension.GetCantAmendErrorMessage(arInvoice, TransactionTypes.CreditNote));
				AssertNullOrEmpty("The return value for ARCreditNote with non-empty OriginalTransactionReference", Extension.GetCantAmendErrorMessage(arCreditNote, TransactionTypes.Invoice));
				AssertNullOrEmpty("The return value for ARCreditNote with non-empty OriginalTransactionReference", Extension.GetCantAmendErrorMessage(arCreditNote, TransactionTypes.CreditNote));

				arInvoice.OriginalTransactionReference = ZGuid.Empty;
				arCreditNote.OriginalTransactionReference = ZGuid.Empty;
				AssertNullOrEmpty("The return value for ARInvoice with empty OriginalTransactionReference", Extension.GetCantAmendErrorMessage(arInvoice, TransactionTypes.Invoice));
				AssertNullOrEmpty("The return value for ARInvoice with empty OriginalTransactionReference", Extension.GetCantAmendErrorMessage(arInvoice, TransactionTypes.CreditNote));
				AssertEquals("The return value for ARCreditNote with empty OriginalTransactionReference", expectedError, Extension.GetCantAmendErrorMessage(arCreditNote, TransactionTypes.Invoice));
				AssertEquals("The return value for ARCreditNote with empty OriginalTransactionReference", expectedError, Extension.GetCantAmendErrorMessage(arCreditNote, TransactionTypes.CreditNote));
			}
		}

		public void TestGetCantReverseErrorMessage()
		{
			var expectedError = "Reversing of Amendment Transactions are not allowed in Korea when e-Reporting is enabled. Please Amend the Original Transaction in Job Billing module.";

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR_INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			var amendARWithInvoice = TestObjectCreator.AmendARTransaction(TransactionTypes.Invoice, arInvoice).amendTransaction as IReversing;
			var amendARWithCreditNote = TestObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, arInvoice).amendTransaction as IReversing;

			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "AP_INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			var amendAPWithCreditNote = TestObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, apInvoice).amendTransaction as IReversing;

			AssertNotNull("Precondition", amendARWithInvoice);
			AssertNotNull("Precondition", amendARWithCreditNote);
			AssertNotNull("Precondition", amendAPWithCreditNote);

			using (testObjectCreator.SetUpForTestingEInvoicing(Core.Constants.CountryCodes.KoreaSouth, true))
			{
				AssertNullOrEmpty(Extension.GetCantReverseErrorMessage(null));

				AssertNullOrEmpty(Extension.GetCantReverseErrorMessage(arInvoice));
				AssertEquals(expectedError, Extension.GetCantReverseErrorMessage(amendARWithInvoice));
				AssertEquals(expectedError, Extension.GetCantReverseErrorMessage(amendARWithCreditNote));

				AssertNullOrEmpty(Extension.GetCantReverseErrorMessage(apInvoice));
				AssertNullOrEmpty(Extension.GetCantReverseErrorMessage(amendAPWithCreditNote));
			}

			using (testObjectCreator.SetUpForTestingEInvoicing(Core.Constants.CountryCodes.KoreaSouth, false))
			{
				AssertNullOrEmpty(Extension.GetCantReverseErrorMessage(null));

				AssertNullOrEmpty(Extension.GetCantReverseErrorMessage(arInvoice));
				AssertNullOrEmpty(Extension.GetCantReverseErrorMessage(amendARWithInvoice));
				AssertNullOrEmpty(Extension.GetCantReverseErrorMessage(amendARWithCreditNote));

				AssertNullOrEmpty(Extension.GetCantReverseErrorMessage(apInvoice));
				AssertNullOrEmpty(Extension.GetCantReverseErrorMessage(amendAPWithCreditNote));
			}
		}

		public void TestGetValidationMessageForAfterPostAction()
		{
			var expectedMessage = "The transaction is not eligible for requests because the E-Reporting Status of the transaction is not 'FAL - Failed'.";

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.Today.AddDays(-1)))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("Test001", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));
				Factory.Save();

				var pivotSUB = arInvoice.GetMostRecentEInvoicingTransactionPivot();

				pivotSUB.AIP_Status = EInvoicingPivotState.Sent;
				AssertEquals(expectedMessage, Extension.GetValidationMessageForAfterPostAction(new[] { pivotSUB }, ""));

				pivotSUB.AIP_Status = EInvoicingPivotState.Failed;
				AssertNullOrEmpty(Extension.GetValidationMessageForAfterPostAction(new[] { pivotSUB }, ""));
			}
		}

		#region IEInvoicingTransactionUpdater

		public void TestIEInvoicingTransactionUpdater_UpdateTransactionForAction_StatusCheck()
		{
			var updater = Extension as IEInvoicingTransactionUpdater;
			AssertNotNull("Precondition", updater);

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.Today.AddDays(-1)))
			{
				var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("Test001", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(arInvoice1, arInvoice1.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				Assert("PreCondition", arInvoice1.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

				var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("Test002", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(arInvoice2, arInvoice2.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				Assert("PreCondition", arInvoice2.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));
				Factory.Save();

				var pivotSUB1 = arInvoice1.GetMostRecentEInvoicingTransactionPivot();
				pivotSUB1.AIP_Status = EInvoicingPivotState.Failed;
				pivotSUB1.AIP_ErrorDescription = "Error Description 1";

				var pivotSUB2 = arInvoice2.GetMostRecentEInvoicingTransactionPivot();
				pivotSUB2.AIP_Status = EInvoicingPivotState.Failed;
				pivotSUB2.AIP_ErrorDescription = "Error Description 2";

				var batchSUB = TestObjectCreator.CreateEInvoicingBatch(10001, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				pivotSUB1.AIP_AIB = batchSUB.PK;
				pivotSUB2.AIP_AIB = batchSUB.PK;

				batchSUB.AIB_QueryTimes = 3;
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				updater.UpdateTransaction(newFactory, arInvoice1, EInvoicingPivotActionType.StatusCheck);
				newFactory.Save();

				AssertEquals("pivotSUB1: AIP_Status should be updated to DLV", EInvoicingPivotState.Delivered, pivotSUB1.AIP_Status);
				AssertEquals("pivotSUB2: AIP_Status should be updated to DLV", EInvoicingPivotState.Delivered, pivotSUB2.AIP_Status);
				AssertNullOrEmpty("pivotSUB1: AIP_ErrorDescription should be updated to empty", pivotSUB1.AIP_ErrorDescription);
				AssertNullOrEmpty("pivotSUB2: AIP_ErrorDescription should be updated to empty", pivotSUB2.AIP_ErrorDescription);

				AssertEquals("batchSUB: AIB_QueryTimes should be updated to 0", 0, batchSUB.AIB_QueryTimes);
			}
		}

		#endregion

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		KoreaSouthComplianceInfoEInvoicingExtension Extension => extension ?? (extension = new KoreaSouthComplianceInfoEInvoicingExtension());
		KoreaSouthComplianceInfoEInvoicingExtension extension;
	}
}
