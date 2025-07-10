using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Integration;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	class TurkeyTransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProviderTest : TestCaseWithFactory
	{
		const string CountryCode = CountryCodes.Turkey;

		ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider GetEInvoicingRequestProvider() => (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCode) as IInstanceProvider<ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider>)?.Get();

		#region IEnablePendingAllocationTransactionsEInvoicingApprovalAndRejectionFunctionality

		public void TestIEnablePendingAllocationTransactionsEInvoicingApprovalAndRejectionFunctionality()
		{
			var rejectionProvider = GetEInvoicingRequestProvider();
			var complianceSubTypes = (ObjectFactory.Get<ICountryComplianceFactory>().GetICountryComplianceInfoBase(CountryCode) as IComplianceSubTypeCodeProvider).GetComplianceSubTypes();

			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			Assert("Compliance sub type is eligible and transaction is not in database", !rejectionProvider.IsTransactionEligibleToCreateApprovalRequest(transaction));
			Assert("Compliance sub type is eligible and transaction is not in database", !rejectionProvider.IsTransactionEligibleToCreateRejectionRequest(transaction));

			((INeedRow)transaction).Row.AcceptChanges();
			Assert("Transaction is in database and it doesn't have eInvoicing pivot", !rejectionProvider.IsTransactionEligibleToCreateApprovalRequest(transaction));
			Assert("Transaction is in database and it doesn't have eInvoicing pivot", !rejectionProvider.IsTransactionEligibleToCreateRejectionRequest(transaction));

			transaction.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PIN;
			Assert("Compliance sub type is not eligible", !rejectionProvider.IsTransactionEligibleToCreateApprovalRequest(transaction));
			Assert("Compliance sub type is not eligible", !rejectionProvider.IsTransactionEligibleToCreateRejectionRequest(transaction));

			var mockEInvoicingHelper = new Mock<IEInvoicingHelper>();
			ObjectFactory.Substitute(mockEInvoicingHelper.Object);

			transaction.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.PIC;
			mockEInvoicingHelper.Setup(x => x.HasActiveEInvoicingTransactionPivot(transaction, EInvoicingPivotActionType.ConfirmTransactionReceived)).Returns(false);
			mockEInvoicingHelper.Setup(x => x.HasActiveEInvoicingTransactionPivot(transaction, EInvoicingPivotActionType.Approve)).Returns(false);
			mockEInvoicingHelper.Setup(x => x.HasActiveEInvoicingTransactionPivot(transaction, EInvoicingPivotActionType.Reject)).Returns(false);

			Assert("Transaction does not have eInvoicing pivot", !rejectionProvider.IsTransactionEligibleToCreateApprovalRequest(transaction));
			Assert("Transaction does not have eInvoicing pivot", !rejectionProvider.IsTransactionEligibleToCreateRejectionRequest(transaction));

			mockEInvoicingHelper.Setup(x => x.HasActiveEInvoicingTransactionPivot(transaction, EInvoicingPivotActionType.Approve)).Returns(true);

			Assert("Transaction does not have eInvoicing pivot", !rejectionProvider.IsTransactionEligibleToCreateApprovalRequest(transaction));
			Assert("Transaction does not have eInvoicing pivot", !rejectionProvider.IsTransactionEligibleToCreateRejectionRequest(transaction));

			mockEInvoicingHelper.Setup(x => x.HasActiveEInvoicingTransactionPivot(transaction, EInvoicingPivotActionType.Approve)).Returns(false);
			mockEInvoicingHelper.Setup(x => x.HasActiveEInvoicingTransactionPivot(transaction, EInvoicingPivotActionType.Reject)).Returns(true);

			Assert("Transaction does not have eInvoicing pivot", !rejectionProvider.IsTransactionEligibleToCreateApprovalRequest(transaction));
			Assert("Transaction does not have eInvoicing pivot", !rejectionProvider.IsTransactionEligibleToCreateRejectionRequest(transaction));

			mockEInvoicingHelper.Setup(x => x.HasActiveEInvoicingTransactionPivot(transaction, EInvoicingPivotActionType.ConfirmTransactionReceived)).Returns(true);
			mockEInvoicingHelper.Setup(x => x.HasActiveEInvoicingTransactionPivot(transaction, EInvoicingPivotActionType.Reject)).Returns(false);

			Assert("Transaction is eligible to create rejection pivot", rejectionProvider.IsTransactionEligibleToCreateApprovalRequest(transaction));
			Assert("Transaction is eligible to create rejection pivot", rejectionProvider.IsTransactionEligibleToCreateRejectionRequest(transaction));

			foreach (var complianceSubType in complianceSubTypes)
			{
				transaction.AH_ComplianceSubType = complianceSubType.Code;
				var isEligibleForERequest = complianceSubType.Code == TurkeyComplianceInfo.ComplianceSubTypeCodes.PIC;
				AssertEquals("Tested Compliance Sub Type: " + complianceSubType.Code, isEligibleForERequest, rejectionProvider.IsTransactionEligibleToCreateApprovalRequest(transaction));
				AssertEquals("Tested Compliance Sub Type: " + complianceSubType.Code, isEligibleForERequest, rejectionProvider.IsTransactionEligibleToCreateRejectionRequest(transaction));
			}
		}

		#endregion

		public void TestIsTransactionEditable()
		{
			var eInvoicingRequestProvider = GetEInvoicingRequestProvider();

			var statuses = new (string, bool)[] {
				(GenApprovalRequestApprovalStatus.Requested, true),
				(GenApprovalRequestApprovalStatus.Cancelled, true),
				(GenApprovalRequestApprovalStatus.Rejected, true),
				(GenApprovalRequestApprovalStatus.Posted, true),
				(GenApprovalRequestApprovalStatus.Error, true),
				(GenApprovalRequestApprovalStatus.ApprovalRequested, false),
				(GenApprovalRequestApprovalStatus.RejectionRequested, false),
				(GenApprovalRequestApprovalStatus.Approved, false)
			};

			var transaction = Factory.NewWithValidTestData<TransactionPendingAllocation>();
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(transaction);

			statuses.ForEach(status =>
			{
				request.XP_ApprovalStatus = status.Item1;
				AssertEquals($"Expected {status.Item2} for status {status.Item1}", status.Item2, eInvoicingRequestProvider.IsTransactionEditable(transaction));
			});
		}
	}
}
