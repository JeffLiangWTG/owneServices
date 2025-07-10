using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.AutoAllocationAndPrinting
{
	public partial class InvoiceBatchComplianceSequenceNumberAllocator : NonPersistentBusinessObject, IObsoleteValidation
	{
		public static string QuestionReprintPrintedInvoice { get { return Res.GetString("ecc59772-ffb8-40b1-a0c4-630ec826888f", "Invoice has been printed already. Do you want to reprint it?"); } }
		public static string QuestionAssignNewSequenceNumber { get { return Res.GetString("e941fe01-6f3e-4086-b129-6f9fdd929027", "Do you want to assign a new sequence number instead of reusing the same number again?"); } }
		public static string QuestionUseCurrentBranchesBooks { get { return Res.GetString("8534a806-1abf-4034-ad42-bec874001786", @"You have chosen transactions posted by another Branch. Government Compliance Numbers and documents will be assigned from Compliance Books configured for your current login Branch.
Do you want to proceed?"); } }
		public static string QuestionUseCurrentBrancheDepartmentsBooks { get { return Res.GetString("1edd2760-b6d0-473a-a28f-f6439a1d03d2", @"You have chosen transactions posted by another Branch and Department. Government Compliance Numbers and documents will be assigned from Compliance Books configured for your current login Branch and Department.
Do you want to proceed?"); } }

		public static string WarningSignatureFaildDueToEmptySignatureInPreviousInvoice { get { return Res.GetString("bbd014fe-8918-48f0-8ff6-e35d4ecec96b", @"Current invoice was not signed with a digital signature due to previous invoice in the sequence having a blank signature."); } }

		public static string WarningSignatureFaildDueToPreviousInvoiceNotFound { get { return Res.GetString("f766d758-0878-4da2-928e-0ed70e334835", @"Current invoice was not signed with a digital signature as it failed to find the previous invoice in the sequence."); } }

		public static string WarningSignatureFaildDueToNotBeingAbleToGetInvoiceCreatedLogTime { get { return Res.GetString("ac883fcf-4a66-4629-a63d-6227f7422817", @"Current invoice was not signed with a digital signature as it failed to get the invoice creation log time."); } }

		public delegate bool AskUserQuestion();
		public AskUserQuestion AskReprintInvoiceOption;
		public AskUserQuestion AskReassignNumberOption;
		public AskUserQuestion AskUseCurrentBrancheBooksOption;
		public AskUserQuestion AskUseCurrentBrancheDepartmentBooksOption;

		public InvoiceBatchComplianceSequenceNumberAllocator(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public InvoiceBatchComplianceSequenceNumberAllocator(ZString invoicePrintingOptionCode, TransactionHeader[] transactions, BusinessObjectFactory bizObjFactory)
			: base(bizObjFactory)
		{
			this.InvoicePrintingOptionCode = invoicePrintingOptionCode;
			this.Transactions = transactions;
			Factory.Saving += new BusinessObjectFactory.SavingEventHandler(Factory_Saving);
		}

		public static ITransactionParticipant[] GetFactoriesWithAllocationCodeToBeCalledOnSaving(InvoiceBatchComplianceSequenceNumberAllocator allocator, BusinessObjectFactory factory)
		{
			return GetFactoriesWithAllocationCodeToBeCalledOnSaving_Core(allocator, new ITransactionParticipant[] { factory });
		}

		static ITransactionParticipant[] GetFactoriesWithAllocationCodeToBeCalledOnSaving_Core(InvoiceBatchComplianceSequenceNumberAllocator allocator, ITransactionParticipant[] factories)
		{
			Array.Resize(ref factories, factories.Length + 1);
			factories[factories.Length - 1] = new SaveInTransactionWithRollBackAction(allocator.Factory, allocator.PrintComplianceInvoiceDocument, allocator.CheckAllocationOrPrintingFailed);
			return factories;
		}

		readonly ZString InvoicePrintingOptionCode;
		readonly TransactionHeader[] Transactions;
		readonly List<TransactionHeader> TransactionsGoingToBePrinted = new List<TransactionHeader>();
		bool allocationProcessFailed;
		bool printingProcessFailed;

		public ZString LastGovernmentPrintTaskErrorMessage
		{
			get;
			private set;
		}

		public ZBool PartialAllocationOccured
		{
			get;
			private set;
		}

		public GovtComplianceInvoicePrintManager PrintManager
		{
			get;
			private set;
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			canContinueWithAllocation = CanContinueWithAllocation;
		}

		bool canContinueWithAllocation;

		void Factory_Saving(BusinessObjectFactory factory)
		{
			AutoAllocateComplianceSequenceNumbers();
		}

		void AutoAllocateComplianceSequenceNumbers()
		{
			try
			{
				if (canContinueWithAllocation)
				{
					bool isAllocateSequenceNumberOnly = InvoicePrintingOptionCode == GovtTaxInvoicePrintTask.AllocateSequenceNumberOnly;
					if (Transactions.Length > 0)
					{
						var sortedTrans = Transactions.OrderBy(x => x.ComplianceNumberAllocationDate.IsValid ? x.ComplianceNumberAllocationDate : x.AH_PostDate).Cast<InvoicingBase>().ToArray();
						InvoicingBase.EnsureNoPastTransactionsWithEmptyComplNum(sortedTrans);

						foreach (InvoicingBase transaction in sortedTrans)
						{
							try
							{
								if (IsEligibleToAllocationComplianceSequenceNumber(transaction, isAllocateSequenceNumberOnly))
								{
									transaction.AllocateComplianceSequenceNumberForInvoiceIfEmpty(isAllocateSequenceNumberOnly, true);
								}
								TransactionsGoingToBePrinted.Add(transaction);
							}
							catch (ComplianceSequenceRelatedException ex)
							{
								if (ex is CannotAllocateSequenceNumberToPrePrintedSequenceWhenNotPrintingException ||
									ex is UnableToAllocateNumberDueToPostDateEarlierThanLastDateUsedException ||
									ex is UnableToAllocateNumberDueToSparseComplianceBookException ||
									ex is InvoiceDateLessThanPreviousException ||
									ex is PostDateLessThanPreviousException ||
									ex is InvoiceDateGreaterThanPostDateException ||
									ex is ComplianceNumberExceedMaximumLengthException ||
									ex is HasNonCMTChargeZeroAmountLineException)
								{
									throw;
								}
							}
						}

						if (TransactionsGoingToBePrinted.Count == 0)
						{
							allocationProcessFailed = true;

							var shouldUseLoginBranchForComplianceSequence = Transactions.FirstOrDefault().ShouldUseLoginBranchForComplianceSequence;

							if (shouldUseLoginBranchForComplianceSequence)
							{
								throw new NoComplianceInvoicesToPrintLBDException();
							}
							else
							{
								throw new NoComplianceInvoicesToPrintHBDException();
							}
						}
						else if (TransactionsGoingToBePrinted.Count < Transactions.Length)
						{
							if (!AccountingMasterFilesRegistry.Instance.ComplianceAllowPartialSequenceNumberAllocation.Value)
							{
								allocationProcessFailed = true;
								throw new AllocationComplianceSequenceFullException();
							}
							else
							{
								PartialAllocationOccured = true;
							}
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				allocationProcessFailed = true;
				throw;
			}
		}

		bool CanContinueWithAllocation
		{
			get
			{
				bool result = true;
				if (AskReprintInvoiceOption == null || AskReassignNumberOption == null || AskUseCurrentBrancheBooksOption == null || AskUseCurrentBrancheDepartmentBooksOption == null)
				{
					throw new UserQuestionsAreNotConfiguredProperlyException();
				}

				if (Transactions.Length == 1)
				{
					var transaction = Transactions[0];
					if (transaction.AH_InvoicePrinted)
					{
						if (AskReprintInvoiceOption())
						{
							result = ResetSequenceNumberIfRequiredByUser();
						}
						else
						{
							allocationProcessFailed = true;
							result = false;
						}
					}
					else
					{
						if (transaction.AH_TransactionReference.IsEmpty)
						{
							if (ShouldAskUseCurrentBranchBooksOption(transaction) && !AskUseCurrentBrancheBooksOption())
							{
								allocationProcessFailed = true;
								result = false;
							}
							else if (ShouldAskUseCurrentBranchDepartmentBooksOption(transaction) && !AskUseCurrentBrancheDepartmentBooksOption())
							{
								allocationProcessFailed = true;
								result = false;
							}
						}
						else
						{
							result = ResetSequenceNumberIfRequiredByUser();
						}
					}
				}
				else // multiple invoices selected
				{
					foreach (InvoicingBase transaction in Transactions)
					{
						if (transaction.AH_ComplianceSubType.IsEmpty)
						{
							throw new UnableToAllocateNumberDueToSubtypeMissingException();
						}

						if (transaction.AH_InvoicePrinted)
						{
							throw new UnableToPrintDueToAtLeastOneInvoicePrintedAlreadyException();
						}
					}

					foreach (InvoicingBase transaction in Transactions)
					{
						if (transaction.AH_TransactionReference.IsEmpty)
						{
							if (ShouldAskUseCurrentBranchBooksOption(transaction))
							{
								if (AskUseCurrentBrancheBooksOption())
								{
									break;
								}
								else
								{
									allocationProcessFailed = true;
									result = false;
								}
							}
							else if (ShouldAskUseCurrentBranchDepartmentBooksOption(transaction))
							{
								if (AskUseCurrentBrancheDepartmentBooksOption())
								{
									break;
								}
								else
								{
									allocationProcessFailed = true;
									result = false;
								}
							}
						}
					}
				}
				return result;
			}
		}

		bool ResetSequenceNumberIfRequiredByUser()
		{
			bool result = true;
			var transaction = Transactions[0];
			var isComplianceNumberResetAllowed = ObjectFactory.Get<IElectronicInvoicingAccountingObjectFactory>().GetElectronicInvoicingUpdateActionPermissions().CheckIsComplianceNumberResetAllowed(transaction);
			if (transaction.Company.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Portugal)  // Comparison of current country: will be refactored to use ObjectFactory in WI00551825
			{
				isComplianceNumberResetAllowed = false;
			}
			if (isComplianceNumberResetAllowed && AskReassignNumberOption())
			{
				if (ShouldAskUseCurrentBranchBooksOption(transaction))
				{
					if (!AskUseCurrentBrancheBooksOption())
					{
						allocationProcessFailed = true;
						result = false;
					}
				}
				else if (ShouldAskUseCurrentBranchDepartmentBooksOption(transaction))
				{
					if (!AskUseCurrentBrancheDepartmentBooksOption())
					{
						allocationProcessFailed = true;
						result = false;
					}
				}

				var originalSequence = transaction.ComplianceSequence;
				if (originalSequence != null)
				{
					if (transaction.AH_TransactionReference.Length > originalSequence.XD_Prefix.Length)
					{
						var originalNumber = transaction.AH_TransactionReference.Substring(originalSequence.XD_Prefix.Length);
						originalSequence.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.VoidingComplianceSequenceNo, string.Format("sequence number {0} is voided due to reprinting", originalNumber));
					}

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					transaction.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.EditedARecord, string.Format("Reset compliance sequence number during reprint. Original value is [{0}] ", transaction.AH_TransactionReference));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					transaction.AH_TransactionReference = ZString.Empty;
					transaction.AH_XD_ComplianceBook = ZGuid.Empty;
					transaction.AH_InvoicePrinted = false;
				}
				else
				{
					throw new FailedToFindComplianceSequenceFromSequenceNumberException();
				}
			}
			return result;
		}

		bool IsEligibleToAllocationComplianceSequenceNumber(TransactionHeader transaction, bool isAllocateSequenceNumberOnly) =>
			(isAllocateSequenceNumberOnly || IsTransactionEligibleToAllocationOnPrinting(transaction)) && !AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.GetFallBackValueAtAllLevels(transaction.AH_GC.ToGuid(), Guid.Empty, Guid.Empty);

		bool IsTransactionEligibleToAllocationOnPrinting(TransactionHeader transaction) => transaction.GetComplianceAllocationMethodAR() != AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;

		bool ShouldAskUseCurrentBranchBooksOption(TransactionHeader transaction)
		{
			var complianceSequence = transaction.ComplianceSequence;
			return transaction.ShouldUseLoginBranchForComplianceSequence && GlbBranch.CurrentBranch.PK != transaction.AH_GB && (complianceSequence == null || complianceSequence.AllocationStrategy.IsBranchApplicable); // if it uses COM/CTR level book, then no need to ask the question.
		}

		bool ShouldAskUseCurrentBranchDepartmentBooksOption(TransactionHeader transaction)
		{
			var complianceSequence = transaction.ComplianceSequence;
			return transaction.ShouldUseLoginBranchForComplianceSequence && GlbDepartment.CurrentDepartment.PK != transaction.AH_GE && (complianceSequence == null || complianceSequence.AllocationStrategy.IsDepartmentApplicable); // if it uses COM/BRN/CTR level book, then no need to ask the question.
		}

		ChangedTableNames PrintComplianceInvoiceDocument()
		{
			if (!allocationProcessFailed)
			{
				try
				{
					if (TransactionsGoingToBePrinted.Count > 0)
					{
						PrintManager = new GovtComplianceInvoicePrintManager();
						PrintManager.PrintGovtComplianceInvoices(TransactionsGoingToBePrinted.ToArray(), InvoicePrintingOptionCode);
						if (!PrintManager.LastGovernmentPrintTaskErrorMessage.IsEmpty)
						{
							LastGovernmentPrintTaskErrorMessage = PrintManager.LastGovernmentPrintTaskErrorMessage;
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					printingProcessFailed = true;
					throw;
				}
			}
			return ChangedTableNames.Empty;
		}

		void CheckAllocationOrPrintingFailed()
		{
			if (allocationProcessFailed || printingProcessFailed)
			{
				foreach (InvoicingBase transaction in TransactionsGoingToBePrinted)
				{
					transaction.AH_InvoicePrinted = ZBool.False;
				}
			}
		}
	}
}
