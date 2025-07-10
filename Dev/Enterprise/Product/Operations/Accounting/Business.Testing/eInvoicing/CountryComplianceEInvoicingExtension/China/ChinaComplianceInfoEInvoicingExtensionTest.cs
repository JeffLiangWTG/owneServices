using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using ComplianceDocumentStatus = Enterprise.MasterFiles.Business.CountryCompliance.ChinaComplianceInfo.ComplianceDocumentStatusTypes;

namespace Enterprise.Accounting.Business.EInvoicing.Testing
{
	[TestedType(typeof(ChinaComplianceInfoEInvoicingExtension))]
	public class ChinaComplianceInfoEInvoicingExtensionTest : ChinaComplianceInfoTest
	{
		public void TestGetCantAmendErrorMessage()
		{
			AssertNullOrEmpty("It's no need for China.", TestExtension.GetCantAmendErrorMessage(null, null));
		}

		public void TestGetCantReverseErrorMessage()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.CNY, 1.0m);

			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(invoice.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("When EnableEInvoicingFunctionality is false, CantReverseErrorMessage will be empty", ZString.Empty, TestExtension.GetCantReverseErrorMessage(invoice));

			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(invoice.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetValue(Guid.Empty, invoice.Branch.PK.ToGuid(), Guid.Empty, string.Empty);
			AssertEquals("When ChinaEInvoicingCredentials is empty, CantReverseErrorMessage will be empty", ZString.Empty, TestExtension.GetCantReverseErrorMessage(invoice));

			AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.SetValue(Guid.Empty, invoice.Branch.PK.ToGuid(), Guid.Empty, "test123");
			AssertNull(invoice.GetMostRecentEInvoicingTransactionPivot());
			AssertEquals("When invoice don't have pivot, CantReverseErrorMessage will be empty", ZString.Empty, TestExtension.GetCantReverseErrorMessage(invoice));

			var pivot = invoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Queued);
			AssertEquals("When invoice have QUE pivot, CantReverseErrorMessage will be 'You cannot reverse the AR invoice because it is in the process of E-Reporting.'", "You cannot reverse the AR invoice because it is in the process of E-Reporting.", TestExtension.GetCantReverseErrorMessage(invoice));

			pivot.AIP_Status = EInvoicingPivotState.Sent;
			AssertEquals("When invoice have SNT pivot, CantReverseErrorMessage will be 'You cannot reverse the AR invoice because it is in the process of E-Reporting.'", "You cannot reverse the AR invoice because it is in the process of E-Reporting.", TestExtension.GetCantReverseErrorMessage(invoice));

			pivot.AIP_Status = EInvoicingPivotState.Batched;
			AssertEquals("When invoice have BCH pivot, CantReverseErrorMessage will be 'You cannot reverse the AR invoice because it is in the process of E-Reporting.'", "You cannot reverse the AR invoice because it is in the process of E-Reporting.", TestExtension.GetCantReverseErrorMessage(invoice));

			pivot.AIP_Status = EInvoicingPivotState.Failed;
			AssertEquals("When invoice have FAL pivot, CantReverseErrorMessage will be empty", ZString.Empty, TestExtension.GetCantReverseErrorMessage(invoice));

			pivot.AIP_Status = EInvoicingPivotState.Succeed;
			AssertEquals("When invoice has SUC pivot, no compliance document status", GetExpectedMessageForComplianceDocumentStatus(""), TestExtension.GetCantReverseErrorMessage(invoice));

			var headerReference = Factory.New<AccTransactionHeaderReference>();
			headerReference.AH1_AH = invoice.PK;
			headerReference.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.CDS;
			headerReference.AH1_Reference = ComplianceDocumentStatus.CDI.Code;

			AssertEquals("When invoice has SUC pivot, CDI compliance document status", GetExpectedMessageForComplianceDocumentStatus(ComplianceDocumentStatus.CDI.Code), TestExtension.GetCantReverseErrorMessage(invoice));

			headerReference.AH1_Reference = ComplianceDocumentStatus.CDD.Code;
			AssertEquals("When invoice has SUC pivot, CDV compliance document status", ZString.Empty, TestExtension.GetCantReverseErrorMessage(invoice));

			invoice.AH_InvoiceAmount = 100m;
			invoice.AH_GSTAmount = 10m;
			headerReference.AH1_Amount = 1000m;
			headerReference.AH1_Reference = ComplianceDocumentStatus.CDV.Code;
			AssertEquals("When invoice has SUC pivot, CDV compliance document status", ZString.Empty, TestExtension.GetCantReverseErrorMessage(invoice));

			headerReference.AH1_Reference = ComplianceDocumentStatus.CDR.Code;
			AssertEquals("When invoice has SUC pivot, CDV compliance document status", ZString.Empty, TestExtension.GetCantReverseErrorMessage(invoice));

			headerReference.AH1_Amount = 110m;
			headerReference.AH1_Reference = ComplianceDocumentStatus.CDV.Code;
			AssertEquals("When invoice has SUC pivot, CDV compliance document status", ZString.Empty, TestExtension.GetCantReverseErrorMessage(invoice));

			headerReference.AH1_Reference = ComplianceDocumentStatus.CDR.Code;
			AssertEquals("When invoice has SUC pivot, CDV compliance document status", ZString.Empty, TestExtension.GetCantReverseErrorMessage(invoice));

			headerReference.AH1_Amount = 100m;
			var cannotReverseDueToAmountMessage = @"The invoice has been split to multiple VAT invoices.
Reversal is allowed only if all related VAT invoices have been credited or voided.
If all related VAT invoices have been credited or voided, please get latest e-Invoice status via 'Request e-Invoice Status' action menu before attempting to reverse this invoice.";
			headerReference.AH1_Reference = ComplianceDocumentStatus.CDV.Code;
			AssertEquals("The voided or credited amount less than invoice amount.", cannotReverseDueToAmountMessage, TestExtension.GetCantReverseErrorMessage(invoice));

			headerReference.AH1_Reference = ComplianceDocumentStatus.CDR.Code;
			AssertEquals("The voided or credited amount less than invoice amount.", cannotReverseDueToAmountMessage, TestExtension.GetCantReverseErrorMessage(invoice));

			string GetExpectedMessageForComplianceDocumentStatus(string complianceDocumentStatus)
			{
				return string.Format(@"Reversal is allowed only if the compliance document status is CDD, CDV or CDR.
The current compliance document status is {0}.
Please get latest e-Invoice status via 'Request e-Invoice Status' action menu before attempting to reverse this invoice.", complianceDocumentStatus);
			}
		}

		public void TestGetValidationMessageForAfterPostAction()
		{
			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot.AIP_ActionType = Constants.EInvoicingPivotActionType.Adjustment;
			var pivots = new List<AccEInvoicingTransactionPivot>();
			pivots.Add(pivot);
			var message = TestExtension.GetValidationMessageForAfterPostAction(pivots, ZString.Empty);

			AssertEquals(TestExtension.NotEligibleForRequestsMessage, message);

			var submitSucceedPivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			submitSucceedPivot.AIP_ActionType = Constants.EInvoicingPivotActionType.Submit;
			submitSucceedPivot.AIP_Status = Constants.EInvoicingPivotState.Succeed;
			pivots.Add(submitSucceedPivot);

			message = TestExtension.GetValidationMessageForAfterPostAction(pivots, ZString.Empty);

			AssertNullOrEmpty(message);

			var submitDeliveredPivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			submitDeliveredPivot.AIP_ActionType = Constants.EInvoicingPivotActionType.Submit;
			submitDeliveredPivot.AIP_Status = Constants.EInvoicingPivotState.Delivered;
			pivots.Add(submitDeliveredPivot);
			pivots.Remove(submitSucceedPivot);

			message = TestExtension.GetValidationMessageForAfterPostAction(pivots, ZString.Empty);

			AssertNullOrEmpty(message);
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator = new TestObjectCreator(Factory);
			TestExtension = new ChinaComplianceInfoEInvoicingExtension();
		}

		TestObjectCreator TestObjectCreator;
		ChinaComplianceInfoEInvoicingExtension TestExtension;
	}
}
