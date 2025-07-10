using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class TransactionPendingAllocationValidationTest : TransactionHeaderValidationTest
	{
		[TestDate(2020, 3, 2, 00, 00, 0)]
		public void TestCheckAH_GovernmentAllocatedID()
		{
			AssertCheckAH_GovernmentAllocatedID_RunsValidation();
		}

		public void TestCheckAH_OH()
		{
			var invoice = (TransactionPendingAllocation)Factory.New(HeaderType);
			var validation = new TransactionPendingAllocationValidation(invoice);
			invoice.AH_OH = TestObjectCreator.Debtor.PK;
			validation.ValidateAH_OH();
			AssertHasError(invoice.AH_OHInfo, "Enter a valid Account.");
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			validation.ValidateAH_OH();
			AssertNoErrors(invoice.AH_OHInfo);
		}

		[TestDate(2017, 12, 4)]
		public void TestDuplicateTransactionNumberForCreditNote_Standard()
		{
			AssertDuplicateTransactionNumberForCreditNote(
				AllowDuplicateInvoiceNumberRule.STD,
				invoiceDate1: ZDateTime.Today,
				invoiceDate2: ZDateTime.Now.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths));
		}

		[TestDate(2017, 12, 4)]
		public void TestDuplicateTransactionNumberForCreditNote_Calendar()
		{
			AssertDuplicateTransactionNumberForCreditNote(
				AllowDuplicateInvoiceNumberRule.CAL,
				invoiceDate1: ZDateTime.Today,
				invoiceDate2: new ZDateTime(ZDateTime.Today.Year + 1, 5, 6));
		}

		void AssertDuplicateTransactionNumberForCreditNote(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate1, ZDateTime invoiceDate2)
		{
			string expInvoiceNumberDuplicateMessage = CreateDuplicateNumberErrorMessage(allowDuplicateInvoiceNumberRule);
			string expInvoiceNumberDuplicateMessageNoPerm = CreateDuplicateNumberErrorMessageNoPermission(allowDuplicateInvoiceNumberRule);
			string expInvoiceNumberDuplicateMessageWithPerm = CreateDuplicateNumberMessageWithPermission(allowDuplicateInvoiceNumberRule);

			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

				TransactionPendingAllocation transaction = Factory.New<TransactionPendingAllocation>();
				transaction.AH_TransactionNum = "ABC";
				transaction.AH_OH = org.PK;
				transaction.AH_OSExTaxAmount = -200;
				transaction.AH_InvoiceDate = invoiceDate1;
				Factory.Save();

				TransactionPendingAllocation transaction2 = Factory.New<TransactionPendingAllocation>();
				transaction2.AH_TransactionNum = "ABC";
				transaction2.AH_OH = org.PK;
				transaction2.AH_OSExTaxAmount = -200;
				AssertHasError(transaction2.AH_TransactionNumInfo, expInvoiceNumberDuplicateMessage);
				AssertNull(transaction2.GetPreviousSameNumberTransactionDetails());

				transaction2.AH_InvoiceDate = invoiceDate2;
				transaction2.Validation.ValidateAH_TransactionNum();
				AssertHasWarning(transaction2.AH_TransactionNumInfo, expInvoiceNumberDuplicateMessageWithPerm);
				AssertNotNull(transaction2.GetPreviousSameNumberTransactionDetails());

				Env.Security.NewPayablesDuplicateInvoiceNumber.IsAllowed = false;
				transaction2.Validation.ValidateAH_TransactionNum();
				AssertHasError(transaction2.AH_TransactionNumInfo, expInvoiceNumberDuplicateMessageNoPerm);
			}

			ZString CreateDuplicateNumberErrorMessage(string ruleCode)
			{
				var messageDetails = ruleCode == AllowDuplicateInvoiceNumberRule.STD ? "less than 12 months apart" : "in the same calendar year";
				return ZString.Format("The transaction number is already in use by Transaction Pending Allocation. Last posted transaction’s invoice date is 04-Dec-17 which is {0}. This transaction number cannot be used. Please enter another one.", messageDetails);
			}

			ZString CreateDuplicateNumberErrorMessageNoPermission(string ruleCode)
			{
				var messageDetails = ruleCode == AllowDuplicateInvoiceNumberRule.STD ? "at least 12 months apart" : "in another calendar year";
				return ZString.Format("The transaction number is already in use by Transaction Pending Allocation. Last posted transaction’s invoice date is 04-Dec-17 which is {0}. You are not granted security right to Payables > Payables Transactions > New Transactions > Allow Duplicate AP Invoice Number. This transaction number cannot be used. Please enter another one.", messageDetails);
			}

			ZString CreateDuplicateNumberMessageWithPermission(string ruleCode)
			{
				var messageDetails = ruleCode == AllowDuplicateInvoiceNumberRule.STD ? "at least 12 months apart" : "in another calendar year";
				return ZString.Format("The transaction number is already in use by Transaction Pending Allocation. Last posted transaction’s invoice date is 04-Dec-17 which is {0}. You are granted security right to Payables > Payables Transactions > New Transactions > Allow Duplicate AP Invoice Number. This transaction number can be used.", messageDetails);
			}
		}

		[TestDate(2023, 5, 6)]
		public void TestDuplicateTransactionNumberWithApprovalRequest_Standard()
		{
			AssertDuplicateTransactionNumberWithApprovalRequest(AllowDuplicateInvoiceNumberRule.STD,
			"The transaction number is already in use by Transaction Pending Allocation. Last posted transaction’s invoice date is 06-May-23 which is less than 12 months apart. This transaction number cannot be used. Please enter another one.");
		}

		[TestDate(2023, 5, 6)]
		public void TestDuplicateTransactionNumberWithApprovalRequest_Calendar()
		{
			AssertDuplicateTransactionNumberWithApprovalRequest(AllowDuplicateInvoiceNumberRule.CAL,
				"The transaction number is already in use by Transaction Pending Allocation. Last posted transaction’s invoice date is 06-May-23 which is in the same calendar year. This transaction number cannot be used. Please enter another one.");
		}

		void AssertDuplicateTransactionNumberWithApprovalRequest(string allowDuplicateInvoiceNumberRule, string expErrorMessage)
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var transaction = TestObjectCreator.CreateTransactionPendingAllocation("ABC", org, 100);
				Factory.Save();

				var request = new BusinessObjectFactory().Load<TransactionPendingAllocationApprovalRequest>(transaction.AllocationApprovalRequest.PK);
				var transaction2 = TestObjectCreator.CreateTransactionPendingAllocation("ABC", org, 150);

				var allStatusFields = typeof(GenApprovalRequestApprovalStatus).GetFields(BindingFlags.Public | BindingFlags.Static);
				var statusFieldsThatAllowDuplicateTransactionNumber = new string[] { GenApprovalRequestApprovalStatus.Cancelled, GenApprovalRequestApprovalStatus.Rejected };

				foreach (var statusField in allStatusFields)
				{
					var status = (string)statusField.GetValue(null);
					request.XP_ApprovalStatus = status;
					if (status == GenApprovalRequestApprovalStatus.Cancelled)
					{
						request.SetContext(BusinessContext.CancelApprovalRequestByUser);
					}

					request.Factory.Save();

					transaction2.Validation.ValidateAH_TransactionNum();

					if (statusFieldsThatAllowDuplicateTransactionNumber.Contains(status))
					{
						AssertNoErrors(string.Format(CultureInfo.InvariantCulture, "Transaction should has no error when related approval request has status {0}", status), transaction2.AH_TransactionNumInfo);
					}
					else
					{
						AssertHasError(string.Format(CultureInfo.InvariantCulture, "Transaction should has error when related approval request has status {0}", status), transaction2.AH_TransactionNumInfo,
							expErrorMessage);
					}
				}
			}
		}

		[TestDate(2017, 12, 4)]
		public void TestValidateTransactionNumber_Standard()
		{
			AssertValidateTransactionNumber(AllowDuplicateInvoiceNumberRule.STD,
				invoiceDate: ZDateTime.Today,
				invoiceDate2: ZDateTime.Today.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths));
		}

		[TestDate(2017, 12, 4)]
		public void TestValidateTransactionNumber_Calendar()
		{
			AssertValidateTransactionNumber(AllowDuplicateInvoiceNumberRule.CAL,
				invoiceDate: ZDateTime.Today,
				invoiceDate2: new ZDateTime(ZDateTime.Today.Year + 1, 5, 6));
		}

		void AssertValidateTransactionNumber(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, ZDateTime invoiceDate2)
		{
			string expInvoiceNumberDuplicateMessage = CreateDuplicateNumberErrorMessage(allowDuplicateInvoiceNumberRule);
			string expInvoiceNumberDuplicateMessageNoPerm = CreateDuplicateNumberErrorMessageNoPermission(allowDuplicateInvoiceNumberRule);
			string expInvoiceNumberDuplicateMessageWithPerm = CreateDuplicateNumberMessageWithPermission(allowDuplicateInvoiceNumberRule);
			string expInvoiceNumberDuplicateMessageUnapprovedNoPerm = CreateDuplicateNumberErrorMessageUnapprovedNoPermission(allowDuplicateInvoiceNumberRule);

			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

				APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
				invoice.AH_OH = org.PK;
				invoice.AH_GB = GlbBranch.CurrentBranch.PK;
				invoice.AH_TransactionNum = "ABC";
				invoice.AH_InvoiceDate = invoiceDate;

				UAInvoice uaInvoice = Factory.New<UAInvoice>();
				uaInvoice.AH_OH = org.PK;
				uaInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
				uaInvoice.AH_TransactionNum = "XYZ";
				uaInvoice.AH_InvoiceDate = invoiceDate;

				Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
				Charge charge = job.Charges.AddNew();
				charge.JR_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
				charge.JR_GB = GlbBranch.CurrentBranch.PK;
				charge.JR_OH_CostAccount = org.PK;
				charge.JR_LocalCostAmt = 100m;
				charge.JR_APInvoiceNum = "LMNOP";
				charge.JR_APInvoiceDate = invoiceDate;

				Factory.Save();

				TransactionPendingAllocation transaction = Factory.New<TransactionPendingAllocation>();
				transaction.AH_OH = org.PK;
				transaction.AH_TransactionNum = "ABC";
				transaction.Validation.ValidateAH_TransactionNum();
				AssertHasError(transaction.AH_TransactionNumInfo, expInvoiceNumberDuplicateMessage);

				transaction.AH_InvoiceDate = invoiceDate2;
				transaction.Validation.ValidateAH_TransactionNum();
				AssertHasWarning(transaction.AH_TransactionNumInfo, expInvoiceNumberDuplicateMessageWithPerm);

				Env.Security.NewPayablesDuplicateInvoiceNumber.IsAllowed = false;
				transaction.Validation.ValidateAH_TransactionNum();
				AssertHasError(transaction.AH_TransactionNumInfo, expInvoiceNumberDuplicateMessageNoPerm);

				transaction.AH_TransactionNum = "A";
				transaction.Validation.ValidateAH_TransactionNum();
				AssertNoErrors(transaction.AH_TransactionNumInfo);

				var expectedUsedByUnapproved = expInvoiceNumberDuplicateMessageUnapprovedNoPerm;
				transaction.AH_TransactionNum = "XYZ";
				transaction.Validation.ValidateAH_TransactionNum();
				AssertHasError(transaction.AH_TransactionNumInfo, expectedUsedByUnapproved);

				transaction.AH_InvoiceDate = invoiceDate2;
				transaction.Validation.ValidateAH_TransactionNum();
				AssertHasError(transaction.AH_TransactionNumInfo, expectedUsedByUnapproved);

				transaction.AH_TransactionNum = "A";
				transaction.Validation.ValidateAH_TransactionNum();
				AssertNoErrors(transaction.AH_TransactionNumInfo);

				var expectedUsedByJobInvocing = "The transaction number is already in use on Job Invoicing of the following Job(s):";
				transaction.AH_TransactionNum = "LMNOP";
				transaction.Validation.ValidateAH_TransactionNum();
				AssertHasErrorContaining(transaction.AH_TransactionNumInfo, expectedUsedByJobInvocing);

				transaction.AH_InvoiceDate = invoiceDate2;
				transaction.Validation.ValidateAH_TransactionNum();
				AssertHasErrorContaining(transaction.AH_TransactionNumInfo, expectedUsedByJobInvocing);

				transaction.AH_TransactionNum = "A";
				transaction.Validation.ValidateAH_TransactionNum();
				AssertNoErrors(transaction.AH_TransactionNumInfo);
			}

			ZString CreateDuplicateNumberErrorMessage(string ruleCode)
			{
				var messageDetails = ruleCode == AllowDuplicateInvoiceNumberRule.STD ? "less than 12 months apart" : "in the same calendar year";
				return ZString.Format("The transaction number is already in use. Last posted transaction’s invoice date is 04-Dec-17 which is {0}. This transaction number cannot be used. Please enter another one.", messageDetails);
			}

			ZString CreateDuplicateNumberErrorMessageNoPermission(string ruleCode)
			{
				var messageDetails = ruleCode == AllowDuplicateInvoiceNumberRule.STD ? "at least 12 months apart" : "in another calendar year";
				return ZString.Format("The transaction number is already in use. Last posted transaction’s invoice date is 04-Dec-17 which is {0}. You are not granted security right to Payables > Payables Transactions > New Transactions > Allow Duplicate AP Invoice Number. This transaction number cannot be used. Please enter another one.", messageDetails);
			}

			ZString CreateDuplicateNumberMessageWithPermission(string ruleCode)
			{
				var messageDetails = ruleCode == AllowDuplicateInvoiceNumberRule.STD ? "at least 12 months apart" : "in another calendar year";
				return ZString.Format("The transaction number is already in use. Last posted transaction’s invoice date is 04-Dec-17 which is {0}. You are granted security right to Payables > Payables Transactions > New Transactions > Allow Duplicate AP Invoice Number. This transaction number can be used.", messageDetails);
			}

			ZString CreateDuplicateNumberErrorMessageUnapprovedNoPermission(string ruleCode)
			{
				var messageDetails = ruleCode == AllowDuplicateInvoiceNumberRule.STD ? "at least 12 months apart" : "in another calendar year";
				return ZString.Format("The transaction number is already in use by Unapproved Invoice. Last posted transaction’s invoice date is 04-Dec-17 which is {0}. You are not granted security right to Payables > Payables Transactions > New Transactions > Allow Duplicate AP Invoice Number. This transaction number cannot be used. Please enter another one.", messageDetails);
			}
		}

		public void TestValidateAmounts()
		{
			TransactionPendingAllocation transaction = Factory.New<TransactionPendingAllocation>();
			transaction.AH_OSExTaxAmount = 100m;
			transaction.AH_OSTaxAmount = -10m;
			AssertHasErrors(transaction.AH_OSTaxAmountInfo);
		}

		public void TestPreventCreationOfCreditNotePendingAllocation()
		{
			AssertPreventCreationOfCreditNotePendingAllocation(false);
			AssertPreventCreationOfCreditNotePendingAllocation(true);
		}

		void AssertPreventCreationOfCreditNotePendingAllocation(bool preventCreationOfCreditNote)
		{
			using (AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, preventCreationOfCreditNote))
			{
				TransactionPendingAllocation transaction = Factory.New<TransactionPendingAllocation>();
				transaction.AH_InvoiceDate = ZDateTime.Now;
				transaction.AH_OH = transaction.Factory.NewWithValidTestData<OrgHeader>().PK;
				transaction.AH_OSExTaxAmount = -100m;
				if (preventCreationOfCreditNote)
				{
					string preventCreationOfCreditNoteMessage = "Amount cannot be negative as Posting of Credit Notes is prevented. This is controlled by the registry setting Accounting -> Payable Defaults -> Default Settings -> Prevent Creation of Credit Notes.";
					AssertHasError(transaction.AH_OSExTaxAmountInfo, preventCreationOfCreditNoteMessage);
				}
			}
		}

		public void TestCheckAH_InvoiceDate()
		{
			using (AccountingMasterFilesRegistry.Instance.PreventInvoiceDateGreaterThanPostDate.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				TransactionPendingAllocation transaction = Factory.New<TransactionPendingAllocation>();
				transaction.AH_OH = transaction.Factory.NewWithValidTestData<OrgHeader>().PK;
				transaction.AH_OSExTaxAmount = 100m;
				transaction.AH_PostDate = ZDateTime.Now;
				transaction.AH_InvoiceDate = ZDateTime.Now.AddDays(1);
				AssertHasError(transaction.AH_InvoiceDateInfo, @"Unable to post AP Invoice.
Invoice Date must be earlier or same as the Post Date. This is controlled by the registry Accounting > Payable Defaults > Default Settings > Prevent Posting Invoice Date Greater Than Post Date.");
			}
		}

		[ExpectNoExceptions()]
		public void TestCheckAH_TransactionNum_SetPreviousSameNumberTransactionDetails()
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AllowDuplicateInvoiceNumberRule.STD))
			{
				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

				var transaction = TestObjectCreator.CreateTransactionPendingAllocation("ABC", org, -200);
				transaction.AH_InvoiceDate = DateTime.Today.AddMonths(-13);
				transaction.Validation.ValidateAH_TransactionNum();
				transaction.AH_TransactionType = "IPA";
				AssertNoErrors(transaction.AH_TransactionNumInfo);
				Factory.Save();

				var transaction2 = TestObjectCreator.CreateTransactionPendingAllocation("ABC", org, 500);
				transaction2.AH_InvoiceDate = DateTime.Today;
				transaction2.AH_TransactionType = "IPA";
				transaction2.Validation.ValidateAH_TransactionNum();

				AssertNoErrors(transaction2.AH_TransactionNumInfo);
				AssertNotNull(transaction2.GetPreviousSameNumberTransactionDetails());
			}
		}

		protected override Type HeaderType
		{
			get { return typeof(TransactionPendingAllocation); }
		}
	}
}
