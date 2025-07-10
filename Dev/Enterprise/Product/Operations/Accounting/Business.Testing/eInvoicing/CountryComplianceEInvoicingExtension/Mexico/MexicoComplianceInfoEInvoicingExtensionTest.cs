using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing.Testing
{
	[TestedType(typeof(MexicoComplianceInfoEInvoicingExtension))]
	public class MexicoComplianceInfoEInvoicingExtensionTest : MexicoComplianceInfoTest
	{
		public void TestGetCantAmendWithCRDErrorMessage()
		{
			AssertEquals("Not necessary for Mexico, should be empty.", ZString.Empty, Extension.GetCantAmendErrorMessage(null, null));
		}

		public void TestGetCantReverseErrorMessage()
		{
			AssertEquals("Not necessary for Mexico, should be empty.", ZString.Empty, Extension.GetCantReverseErrorMessage(null));
		}

		public void TestGetValidationMessageForAfterPostAction()
		{
			var pivot = Factory.New<AccEInvoicingTransactionPivot>();
			pivot.AIP_ActionType = EInvoicingPivotActionType.Adjustment;
			var pivots = new List<AccEInvoicingTransactionPivot>() { pivot };

			var message = Extension.GetValidationMessageForAfterPostAction(pivots, ZString.Empty);
			AssertEquals(Expected_NotEligibleForRequestsMessage, message);

			var submitSucceedPivot = Factory.New<AccEInvoicingTransactionPivot>();
			submitSucceedPivot.AIP_ActionType = EInvoicingPivotActionType.Submit;
			submitSucceedPivot.AIP_Status = EInvoicingPivotState.Succeed;
			pivots.Add(submitSucceedPivot);

			message = Extension.GetValidationMessageForAfterPostAction(pivots, ZString.Empty);
			AssertEquals(ZString.Empty, message);

			var submitDeliveredPivot = Factory.New<AccEInvoicingTransactionPivot>();
			submitDeliveredPivot.AIP_ActionType = EInvoicingPivotActionType.Submit;
			submitDeliveredPivot.AIP_Status = EInvoicingPivotState.Delivered;
			pivots.Add(submitDeliveredPivot);
			pivots.Remove(submitSucceedPivot);

			message = Extension.GetValidationMessageForAfterPostAction(pivots, ZString.Empty);
			AssertEquals(Expected_NotEligibleForRequestsMessage, message);
		}

		#region IComplianceInfoEInvoicingGUIActionDocumentRequest

		public void TestComplianceInfoEInvoicingGUIActionDocumentRequest()
		{
			var complianceInfo = Extension as IComplianceInfoEInvoicingGUIActionDocumentRequest;
			AssertEquals("Request e-Invoice PDF Copy",complianceInfo.DocumentRequestMenuName);
			AssertEquals(@"Your request for a PDF copy of the tax invoice is being processed. 
Please Note:
- Request for a copy of the tax invoice will be made only for electronic invoices that have E-Reporting Status equal to SUC", complianceInfo.DocumentRequestActionInformation);
		}

		#endregion

		#region IComplianceInfoEInvoicingGUIActionProviderBase

		public void TestGetEligibleInvoices()
		{
			AssertGetEligibleInvoices(TransactionTypes.Invoice, MexicoComplianceInfo.ComplianceSubTypeCodes.TXI, false, true);
			AssertGetEligibleInvoices(TransactionTypes.Invoice, MexicoComplianceInfo.ComplianceSubTypeCodes.TDR, false, true);
			AssertGetEligibleInvoices(TransactionTypes.Invoice, MexicoComplianceInfo.ComplianceSubTypeCodes.TDR, true, true);
			AssertGetEligibleInvoices(TransactionTypes.CreditNote, MexicoComplianceInfo.ComplianceSubTypeCodes.TCR, false, true);
			AssertGetEligibleInvoices(TransactionTypes.CreditNote, MexicoComplianceInfo.ComplianceSubTypeCodes.TCR, true, true);
			AssertGetEligibleInvoices(TransactionTypes.Invoice, MexicoComplianceInfo.ComplianceSubTypeCodes.OTR, false, false);
			AssertGetEligibleInvoices(TransactionTypes.Invoice, MexicoComplianceInfo.ComplianceSubTypeCodes.XCL, false, false);
			AssertGetEligibleInvoices(TransactionTypes.Payment, MexicoComplianceInfo.ComplianceSubTypeCodes.XCL, false, false);

			void AssertGetEligibleInvoices(string transactionType, string complianceSubtype, bool isCancelled, bool isElegible)
			{
				var complianceInfo = Extension as IComplianceInfoEInvoicingGUIActionProvider;

				var transactions = new List<AccTransactionHeader>();
				var invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
				invoice.AH_TransactionType = transactionType;
				invoice.AH_ComplianceSubType = complianceSubtype;
				invoice.AH_IsCancelled = isCancelled;
				transactions.Add(invoice);

				var eligibleInvoices = complianceInfo.GetEligibleInvoices(transactions);

				AssertEquals(isElegible, eligibleInvoices.Contains(invoice));
			}
		}

		public void TestExistActiveDocumentRequestPivot()
		{
			var complianceInfo = Extension as IComplianceInfoEInvoicingGUIActionProvider;
			var lastSentDate = ZDateTime.UtcNow;
			AssertEquals(false, complianceInfo.ExistActiveDocumentRequestPivot(lastSentDate));

			lastSentDate = ZDateTime.UtcNow.AddHours(-2);
			AssertEquals(false, complianceInfo.ExistActiveDocumentRequestPivot(lastSentDate));
		}

		#endregion

		ZString Expected_NotEligibleForRequestsMessage => "The transaction is not eligible for requests as the E-Reporting Status is not equal to SUC.";

		IEInvoicingTransactionValidation Extension => fExtension ?? (fExtension = new MexicoComplianceInfoEInvoicingExtension());
		IEInvoicingTransactionValidation fExtension;
	}
}
