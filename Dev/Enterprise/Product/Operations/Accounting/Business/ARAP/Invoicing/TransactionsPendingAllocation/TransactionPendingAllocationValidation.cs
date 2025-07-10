using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Validation;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	class TransactionPendingAllocationValidation : TransactionHeaderValidation
	{
		protected new TransactionPendingAllocation Parent;

		public TransactionPendingAllocationValidation(TransactionPendingAllocation parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected override void CheckAH_OH()
		{
			base.CheckAH_OH();
			MandatoryValidation.CheckEntered(Parent.AH_OHInfo);
			ListValidation.ErrorIfInvalidPK(Parent.AH_OHInfo);
			ValidateAH_TransactionNum();
			CheckExporterExemption();
		}

		void CheckExporterExemption()
		{
			var checkDate = Parent.AH_PostDate.IsEmpty ?
				(Parent.AH_InvoiceDate.IsEmpty ? ZDateTime.Now : Parent.AH_InvoiceDate)
				: Parent.AH_PostDate;

			var (warnings, _) = ExporterExemptionValidationHelper.CheckExporterExemption(Parent.Header, Parent.AH_Ledger, checkDate);
			if (!warnings.IsEmpty)
			{
				Parent.AH_OHInfo.AddWarning(warnings);
			}
		}

		protected override void CheckAH_InvoiceDate()
		{
			base.CheckAH_InvoiceDate();
			if (AccountingMasterFilesRegistry.Instance.PreventInvoiceDateGreaterThanPostDate.Value &&
				Parent.AH_InvoiceDate.Date > Parent.AH_PostDate.Date)
			{
				Parent.AH_InvoiceDateInfo.AddError(ResString.GetMultilingualString("3D734C9C-B3C0-40EC-BE6E-6EB51462F59D", @"Unable to post AP Invoice.
Invoice Date must be earlier or same as the Post Date. This is controlled by the registry Accounting > Payable Defaults > Default Settings > Prevent Posting Invoice Date Greater Than Post Date."));
			}
			ValidateAH_TransactionNum();
		}

		protected override void CheckAH_DueDate()
		{
			base.CheckAH_DueDate();
			MandatoryValidation.CheckEntered(Parent.AH_DueDateInfo);
		}

		protected override void CheckAH_Desc()
		{
			base.CheckAH_Desc();
			MandatoryValidation.CheckEntered(Parent.AH_DescInfo);
		}

		protected override void CheckAH_OSExTaxAmount()
		{
			base.CheckAH_OSExTaxAmount();
			MandatoryValidation.CheckEntered(Parent.AH_OSExTaxAmountInfo);
			ValidateAH_TransactionNum();
			if (Parent.AH_OSExTaxAmount < 0 && AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(Parent.AH_Ledger, Parent.AH_GC))
			{
				Parent.AH_OSExTaxAmountInfo.AddError(Res.GetString("DE79676F-F0CD-4AE6-B412-12CF97170198", "Amount cannot be negative as {0}", AccountingMasterFilesUtils.APCreditNoteDisallowedMessage));
			}
		}

		protected override void CheckAH_OSTaxAmount()
		{
			base.CheckAH_OSTaxAmount();
			if (Parent.AH_OSExTaxAmount < 0 && Parent.AH_OSTaxAmount > 0 || Parent.AH_OSExTaxAmount > 0 && Parent.AH_OSTaxAmount < 0)
			{
				Parent.AH_OSTaxAmountInfo.AddError(Res.GetString("19516d5c-bbd0-4a13-b902-0a4f09d71fd4", "Amount Excluding Tax and Tax Amount must both be positive or negative."));
			}
		}

		protected override void CheckAH_TransactionType()
		{
			base.CheckAH_TransactionType();
			ValidateAH_TransactionNum();
		}

		protected override void CheckAH_TransactionNum()
		{
			base.CheckAH_TransactionNum();
			MandatoryValidation.CheckEntered(Parent.AH_TransactionNumInfo);

			var jobNumbers = ZString.Empty;

			if (!Parent.AH_OH.IsEmpty && !Parent.AH_TransactionNum.IsEmpty)
			{
				var numberCheckingDetails = AccountingUtils.APTransactionNumberExists(Parent.AH_TransactionType, Parent.AH_TransactionNum, Parent.AH_OH, Parent.AH_InvoiceDate);
				if (numberCheckingDetails.HasNotification)
				{
					AccountingUtils.AddTransactionNumInfoNotification(numberCheckingDetails, numberCheckingDetails.NotificationMessage, Parent);
				}
				else if ((numberCheckingDetails = AccountingUtils.UATransactionNumberExists(Parent.AH_TransactionType, Parent.AH_TransactionNum, Parent.AH_OH, ZGuid.Empty, Parent.AH_InvoiceDate)).HasNotification)
				{
					AccountingUtils.AddTransactionNumInfoNotification(numberCheckingDetails, numberCheckingDetails.NotificationMessage, Parent);
				}
				else if ((numberCheckingDetails = AccountingUtils.PATransactionNumberExists(Parent.AH_TransactionType, Parent.AH_TransactionNum, Parent.AH_OH, Parent.PK, Parent.AH_InvoiceDate)).HasNotification)
				{
					AccountingUtils.AddTransactionNumInfoNotification(numberCheckingDetails, numberCheckingDetails.NotificationMessage, Parent);
				}
				else if (AccountingUtils.IsTransactionNumUsedInJobInvoicing(Parent.AH_TransactionType, Parent.AH_TransactionNum, Parent.AH_OH, Parent.PK, out jobNumbers))
				{
					Parent.AH_TransactionNumInfo.AddError(Res.GetString("1362db6a-0249-4024-bbdb-dd3de8162a0b", "The transaction number is already in use on Job Invoicing of the following Job(s): {0}. Please select another one.", jobNumbers));
				}
				else if (Parent.DoDuplicatesNumbersExistInParentCollection())
				{
					Parent.AH_TransactionNumInfo.AddError(Res.GetString("3cdbf6d2-4368-4d35-9cbf-74952aeeb1a3", "This transaction number is already used on another transaction in this batch. Transaction numbers must be unique by creditor and transaction type."));
				}
			}
		}

		protected override void CheckAH_GovernmentAllocatedID()
		{
			base.CheckAH_GovernmentAllocatedID();

			base.CheckAH_GovernmentAllocatedID_BasedOnRegistry(Parent);
		}

		protected override void CheckAH_OA_InvoiceAddressOverride()
		{
			base.CheckAH_OA_InvoiceAddressOverride();

			ValidateAH_GovernmentAllocatedID();
		}
	}
}
