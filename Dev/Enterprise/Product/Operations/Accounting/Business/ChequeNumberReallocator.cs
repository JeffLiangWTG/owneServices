using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.Validation;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public interface ICanUpdateChequeNumber : IBusiness
	{
		void UpdateChequeNumber(string newChequeNumber, bool withReprint);
		AccChequeBook ChequeBook { get; }
	}

	public class ChequeNumberReallocator : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ChequeNumberReallocator(BusinessObjectFactory factory, AccChequeBook chequeBook, IEnumerable<TransactionHeader> transactions, bool withReprint)
			: base(factory)
		{
			this.chequeBook = chequeBook;
			this.transactions = transactions.OrderBy(x => x.AH_ChequeOrReference);
			this.withReprint = withReprint;
			ChequeNumber = chequeBook.AK_Calc_CurrentNoString;
		}

		#region Messages

		public static ZString DeleteExistingPrintJobs { get { return Res.GetString("def0d12b-2182-461f-b2c4-d1923d083f73", @"There are print jobs for one or more of the selected payments that may not have been processed by the printer yet. 
Do you wish to delete them?"); } }
		public static ZString DeleteExistingPrintJobsFormCaption { get { return Res.GetString("c73850a4-b31d-48ac-9c20-76e61822afab", "Delete Print Jobs"); } }
		public static ZString NoTransactionIsSelectedToPrint { get { return Res.GetString("5cd4565e-3ac3-4ebe-b730-6ed419efb441", "Please select transaction(s) to print"); } }
		public static ZString NoTransactionIsSelectedToReAllocate { get { return Res.GetString("A5F4B3EB-7908-44e1-B828-2F693543DD48", "Please select transaction(s) to re-allocate"); } }
		public static ZString TransactionCannotBeReprinted { get { return Res.GetString("8F96EA26-2E93-4E94-BB7A-F8C6BDB4C793", @"The transaction/s you have selected cannot be reprinted. Please select a different print option or change the transaction/s selected for reprinting.
Reprinting is a special print action used to reprint checks recorded against an Auto Print Check Book.  
The reprint option can only be used to reprint a set of Check transactions belonging to one Check Book. Note: Check Payments that have been cleared in the cash book bank reconciliation cannot be reprinted."); } }
		public static ZString ChequeNumberCannotBeReAllocated { get { return Res.GetString("F1B88C04-B9E5-4171-B5EA-B69B57318D7F", @"The check number/s on transaction/s you have selected cannot be re-allocated. Please change the transaction/s selected for check number re-allocation.
Re-allocation is a special action used to allocate new check numbers recorded against an Auto Print Check Book.  
The re-allocation option can only be used on set of Check transactions belonging to one Check Book. Note: Check Numbers recorded on payments that have been cleared in the cash book bank reconciliation cannot be re-allocated."); } }
		public static ZString ChequeNumberCannotBeReAllocatedBecauseOfConcurrencyError
		{
			get
			{
				return Res.GetString("EBD8A43E-5793-4A2A-A880-83991B118301", "Another user has just updated the cheque book. Please re-open the current form and try it again.");
			}
		}

		#endregion

		#region Properties

		readonly AccChequeBook chequeBook;
		readonly IEnumerable<TransactionHeader> transactions;
		readonly bool withReprint;

		public ZString Caption
		{
			get
			{
				return Res.GetString("e9a47cf3-7f9f-40bb-ad71-26497ff66627", @"{0} checks will be {1}.
Below, please enter / confirm the number of the first check {2}", transactions.Count(), withReprint ? Res.GetString("d6344a53-52c0-4b98-88b5-ab035817c063", "reprinted") : Res.GetString("fa350fe4-ea5b-44dc-aa0e-81b8ea82548d", "renumbered"),
								withReprint ? Res.GetString("6E74AB79-8026-4419-BDAB-8FA5B68C3B52", "in the printer") : Res.GetString("FB0916CC-9C5C-4711-8D2B-697B7A52FCA2", "number to be allocated"));
			}
		}

		#endregion

		#region Bindable Properties

		#region ChequeNumber

		ZString fChequeNumber;
		[MaxLength(AccTransactionHeader.Schema.AH_ChequeOrReferenceMaxLength)]
		public ZString ChequeNumber
		{
			get { return fChequeNumber; }
			set
			{
				value = AccValidationHelper.PadChequeDigitsWithLeadingZeros(chequeBook.BankAccount, value);

				if (value != fChequeNumber)
				{
					CheckMaximumLength(ChequeNumberInfo, value);
					SetNonPersistentPropertyValue(ChequeNumberInfo, ref fChequeNumber, value);
					if (!IsValidationSuspended)
					{
						ValidateChequeNumber();
					}
				}
			}
		}

		public ZPropertyInfo ChequeNumberInfo
		{
			get { return GetZPropertyInfo(nameof(ChequeNumber)); }
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateChequeNumber();
		}

		public void ValidateChequeNumber()
		{
			ChequeNumberInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(ChequeNumberInfo);

			if (!ChequeNumberInfo.HasErrors() && chequeBook.BankAccount != null)
			{
				AccValidationHelper.ValidateChequeDigits(ChequeNumberInfo, chequeBook.BankAccount.AB_ChequeNumDigits);
			}

			if (!ChequeNumberInfo.HasErrors())
			{
				ZString chequeNumberErrorMessage = ChequeOrReferenceValidationHelper.CheckIsNumbersLettersAllowed(true, ChequeNumber);

				if (chequeNumberErrorMessage.IsEmpty)
				{
					ZDecimal value;
					if (ZDecimal.TryParse(ChequeNumber, out value))
					{
						if (value <= chequeBook.AK_LastNo && value + transactions.Count() > chequeBook.AK_LastNo + 1)
						{
							ChequeNumberInfo.AddError(Res.GetString("91e85d21-c211-4f14-ad0d-72ae42bae11c", "There are not enough numbers remaining in this check book from this starting point. The last check number for this check book is {0}. Please reduce the number of transactions being printed, or modify the check book setup.", chequeBook.AK_Calc_LastNoString));
						}
					}

					if (!ChequeNumberInfo.HasErrors())
					{
						List<ZString> selectedChequeNums = new List<ZString>();
						HashSet<ZString> generatedChequeNumbers = new HashSet<ZString>();

						var counter = ZDecimal.Parse(ChequeNumber);
						foreach (TransactionHeader transaction in transactions)
						{
							selectedChequeNums.Add(transaction.AH_ChequeOrReference);
							generatedChequeNumbers.Add(AccValidationHelper.PadChequeDigitsWithLeadingZeros(chequeBook.BankAccount, (counter++).ToString()));
						}

						var newChequeNumbers = generatedChequeNumbers.Except(selectedChequeNums);
						if (chequeNumberErrorMessage.IsEmpty)
						{
							chequeNumberErrorMessage = ChequeOrReferenceValidationHelper.CheckIsChequeNumberNotInBook(chequeBook, newChequeNumbers.ToArray());
						}
						if (chequeNumberErrorMessage.IsEmpty && chequeBook.BankAccount != null)
						{
							var usedChequeNumber = chequeBook.BankAccount.GetTheFirstUsedChequeNumber(newChequeNumbers.ToArray());
							if (!usedChequeNumber.IsEmpty)
							{
								chequeNumberErrorMessage = ChequeOrReferenceValidationHelper.GetInUseErrorMessage(usedChequeNumber, chequeBook.BankAccount, Factory);
							}
						}
						if (!chequeNumberErrorMessage.IsEmpty)
						{
							ChequeNumberInfo.AddError(chequeNumberErrorMessage);
						}
					}
				}
				else
				{
					ChequeNumberInfo.AddError(chequeNumberErrorMessage);
				}
			}
		}

		#endregion

		public static bool CheckIsValidForReallocationOfCheckNumbers(IEnumerable<TransactionHeader> collection)
		{
			bool result = true;
			ZGuid chequeBook = ZGuid.Empty;

			foreach (AccTransactionHeader header in collection)
			{
				ICanUpdateChequeNumber payment = header as ICanUpdateChequeNumber;

				if (payment == null || payment.ChequeBook == null || header.AH_IsCancelled || header.AH_ReceiptType != ReceiptTypes.Cheque || !payment.ChequeBook.AK_AutoPrintCheque || !header.AH_DateClearedInCashbook.IsEmpty)
				{
					result = false;
					break;
				}

				if (!chequeBook.IsValid)
				{
					chequeBook = payment.ChequeBook.PK;
				}
				else
				{
					if (chequeBook != payment.ChequeBook.PK)
					{
						result = false;
						break;
					}
				}
			}

			return result;
		}

		public StmPrintJob[] GetExistingPrintJobs()
		{
			if (withReprint)
			{
				List<ZGuid> guids = new List<ZGuid>();
				foreach (BusinessObject businessObject in transactions)
				{
					guids.Add(businessObject.PK);
				}

				ZQuery existingPrintJobsQuery = new ZQuery();
				existingPrintJobsQuery.AddToFilter(StmPrintJobSchema.SP_ParentTableName, AccPaymentApprovalSchema.Constants.TableName);
				existingPrintJobsQuery.AddToFilter(StmPrintJobSchema.SP_ParentGuid, guids);
				return Factory.Load<StmPrintJob>(existingPrintJobsQuery);
			}
			else
			{
				return System.Array.Empty<StmPrintJob>();
			}
		}

		public void DeletePrintJobs(StmPrintJob[] printJobs)
		{
			foreach (StmPrintJob printJob in printJobs)
			{
				printJob.Delete();
			}
		}

		public delegate void ReprintCheque(TransactionHeader transaction);
		public delegate void ReprintCheques(IEnumerable<TransactionHeader> transactions);

		public ZString Process(ReprintCheque reprintChequeDelegate)
		{
			return Process(reprintChequeDelegate, null);
		}

		public ZString Process(ReprintCheques reprintChequesDelegate)
		{
			return Process(null, reprintChequesDelegate);
		}

		ZString Process(ReprintCheque reprintChequeDelegate, ReprintCheques reprintChequesDelegate)
		{
			foreach (TransactionHeader header in transactions)
			{
				ICanUpdateChequeNumber payment = (ICanUpdateChequeNumber)header;
				UpdateJobChargesAndJobConsolCostsChequeNum(header.AH_ChequeOrReference, ChequeNumber, header.BankAccount);
				payment.UpdateChequeNumber(ChequeNumber, withReprint);
				ChequeNumber = (ZDecimal.Parse(ChequeNumber) + 1).ToString(CultureInfo.InvariantCulture);
				if (ZDecimal.Parse(ChequeNumber) > payment.ChequeBook.AK_CurrentNo)
				{
					payment.ChequeBook.AK_CurrentNo = ZDecimal.Parse(ChequeNumber);
				}
				try
				{
					payment.Factory.Save();
				}
				catch (ZSaveConcurrencyException)
				{
					return ChequeNumberCannotBeReAllocatedBecauseOfConcurrencyError;
				}

				if (withReprint && reprintChequeDelegate != null)
				{
					reprintChequeDelegate(header);
				}
			}

			if (withReprint && reprintChequesDelegate != null)
			{
				reprintChequesDelegate(transactions);
			}
			return ZString.Empty;
		}

		void UpdateJobChargesAndJobConsolCostsChequeNum(string oldChequeNumber, string newChequeNumber, AccBankAccount bankAccount)
		{
			JobCharge[] jobCharges = bankAccount.GetJobChargesUsingChequeNumber(oldChequeNumber, ZGuid.Empty);
			foreach (JobCharge charge in jobCharges)
			{
				charge.JR_ChequeNo = newChequeNumber;
				if (charge.ParentConsolCost != null)
				{
					((JobConsolCost)charge.ParentConsolCost).E6_ChequeOrReference = newChequeNumber;
				}
			}
		}

		AccValidationHelper fAccValidationHelper;
		AccValidationHelper AccValidationHelper
		{
			get { return fAccValidationHelper ?? (fAccValidationHelper = new AccValidationHelper()); }
		}
	}
}