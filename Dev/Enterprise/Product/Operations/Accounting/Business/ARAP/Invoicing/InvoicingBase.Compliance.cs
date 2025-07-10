using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingIServices;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.RSACryptography;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.DocumentWrappers;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	partial class InvoicingBase : IEvaluateComplianceRule, ISupportQueueingForComplianceReports, IComplianceRuleParentTransaction, IComplianceNumberSequence, IDocAddresses
	{
		#region Document Signing

		public (int invoiceSignedCount, ZString errorMessage) SignInvoicesRecursivelyInBackwardDirection()
		{
			var invoiceSignedCount = 0;
			var errorMessage = ZString.Empty;

			if (!IsDigitallySigned)
			{
				var invoicesToSign = new List<InvoicingBase>();
				errorMessage = BuildReversedInvoiceSequenceForSigning(invoicesToSign);
				if (errorMessage.IsEmpty)
				{
					invoicesToSign.Reverse();
					errorMessage = SignInvoicesInSequence(invoicesToSign);
					invoiceSignedCount = errorMessage.IsEmpty ? invoicesToSign.Count : 0;
				}
			}

			return (invoiceSignedCount, errorMessage);
		}

		public ZBool IsDigitallySigned => !AH_DigitalSignature_COMPRESSED.IsEmpty;

		ZString BuildReversedInvoiceSequenceForSigning(List<InvoicingBase> signingSequence)
		{
			var result = ZString.Empty;

			if (!IsDigitallySigned)
			{
				signingSequence.Add(this);

				if (signingSequence.Count <= MaxNumberOfInvoicesToSign)
				{
					var previousInvoice = FindPreviousInvoiceInTheSequenceBasedOnTransactionReference();
					if (previousInvoice != null)
					{
						result = previousInvoice.BuildReversedInvoiceSequenceForSigning(signingSequence);
					}
					else if (IsDigitialSignatureFailedDueToPreviousInvoiceNotFound)
					{
						result = BuildSigningErrorMessage(previousInvoice);
					}
				}
				else
				{
					result = Res.GetString("65978d37-35ee-420c-8953-ed5753de57a1",
						"The number of invoices that you want to sign exceeds the maximum limit of {0}, please choose an invoice with a smaller compliance number and try again",
						MaxNumberOfInvoicesToSign);
				}
			}

			return result;
		}

		int MaxNumberOfInvoicesToSign
		{
			get
			{
				return
#if DEBUG
				Globals.IsTest ? 5 :
#endif
					50;
			}
		}

		static ZString SignInvoicesInSequence(List<InvoicingBase> invoiceSequence)
		{
			var result = ZString.Empty;

			if (invoiceSequence.Any())
			{
				var factory = invoiceSequence.First().Factory;
				using (var transactionManager = ((IDbConnected)factory).Connection.BeginTransactionWithManager())
				{
					try
					{
						InvoicingBase previousInvoice = null;
						foreach (var currentInvoice in invoiceSequence)
						{
							currentInvoice.SignInvoiceWithRSASignature(previousInvoice);
							if (currentInvoice.HasSigningError)
							{
								result = currentInvoice.BuildSigningErrorMessage(previousInvoice);
								break;
							}
							else
							{
								previousInvoice = currentInvoice;
							}
						}

						if (result.IsEmpty)
						{
							factory.Save();
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						result = ex.Message;
					}
					finally
					{
						if (result.IsEmpty)
						{
							transactionManager.CommitTransaction();
						}
						else
						{
							transactionManager.RollbackTransaction();
						}
					}
				}
			}

			return result;
		}

		bool HasSigningError => IsDigitialSignatureFailedDueToPreviousInvoiceNotFound
								|| IsDigitialSignatureFailedDueToEmptySignatureInPreviousInvoice
								|| IsDigitialSignatureFailedDueToNotBeingAbleToGetInvoiceCreatedLogTime;

		ZString BuildSigningErrorMessage(InvoicingBase previousInvoice)
		{
			var result = ZString.Empty;

			if (IsDigitialSignatureFailedDueToPreviousInvoiceNotFound)
			{
				result = Res.GetString("c70c060e-2337-4c42-be70-b02228d188a3", "Cannot sign invoice {0} due to failed to find the previous invoice based on transaction reference {1}", AH_TransactionNum, AH_TransactionReference);
			}
			else if (IsDigitialSignatureFailedDueToNotBeingAbleToGetInvoiceCreatedLogTime)
			{
				result = Res.GetString("ed8437cc-931c-49ca-9698-282d3d834339", "Cannot sign invoice {0} due to failed to get the invoice creation log time", AH_TransactionNum);
			}
			else if (IsDigitialSignatureFailedDueToEmptySignatureInPreviousInvoice)
			{
				result = Res.GetString("85246345-066e-4fb9-8f90-3e35109d8692", "Cannot sign invoice {0} due to the previous invoice {1} is not signed yet", AH_TransactionNum, previousInvoice.AH_TransactionNum);
			}

			return result;
		}

		void SignInvoiceWithRSASignature(InvoicingBase previousInvoice = null)
		{
			var invoiceCreatedLogTime = GetInvoiceCreationLogTime();
			if (invoiceCreatedLogTime.HasValue)
			{
				var stringBuilder = new ZStringBuilder();
				stringBuilder.Append(AH_InvoiceDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
				stringBuilder.Append(invoiceCreatedLogTime.Value.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));
				stringBuilder.Append(AH_TransactionReference);
				stringBuilder.Append(new ZDecimal(Math.Abs(AH_InvoiceAmount + AH_GSTAmount)).ToString(2));

				var previousInvoiceInSequence = previousInvoice ?? FindPreviousInvoiceInTheSequenceBasedOnTransactionReference();
				// if not the first one and failed to find the previous one.
				if (previousInvoiceInSequence == null && IsDigitialSignatureFailedDueToPreviousInvoiceNotFound)
				{
					return;
				}
				if (previousInvoiceInSequence != null)
				{
					if (previousInvoiceInSequence.AH_DigitalSignature_COMPRESSED.IsEmpty)
					{
						IsDigitialSignatureFailedDueToEmptySignatureInPreviousInvoice = true;
						return;
					}
					else
					{
						stringBuilder.Append(Convert.ToBase64String(previousInvoiceInSequence.AH_DigitalSignature_COMPRESSED));
					}
				}
				else
				{
					stringBuilder.Append("");
				}

				var dataToSign = stringBuilder.ToStringWithDelimiterBetweenAppends(";");
				AH_DigitalSignature_COMPRESSED = (new RSASecurityProvider()).SignatureSigner.SignHashAsBytes(dataToSign);
			}
		}

		ZDateTime? GetInvoiceCreationLogTime()
		{
			var logQuery = new ZQuery(StmALogSchema.SL_Parent, PK);
			logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.AddedARecordToTheSystemCode);
			var result = Factory.LoadTop1<StmALog>(logQuery)?.SL_EventTime;

			if (!result.HasValue)
			{
				IsDigitialSignatureFailedDueToNotBeingAbleToGetInvoiceCreatedLogTime = true;
			}

			return result;
		}

		bool IsDigitalSignatureApplicable => GlbCompany.CurrentCompany.Country.SupportDocumentSigning
			&& (this is ARInvoice || this is ARCreditNote || ((this is APInvoice || this is APCreditNote) && this.IsSelfBillingInvoice));

		InvoicingBase FindPreviousInvoiceInTheSequenceBasedOnTransactionReference()
		{
			// Find the previous invoice based on the compliance number
			// i.e. minus 1 from the current compliance number, and use ZQuery to find the previous invoice based on the number.
			InvoicingBase result = null;
			var sequence = ComplianceSequenceFromTransactionReference;
			if (sequence != null)
			{
				var currentNumberText = ComplianceSequenceRetriever.GetSequenceNumber(Company.Country.Code, AH_TransactionReference, sequence.XD_Prefix);
				var currentNumber = Convert.ToInt32(currentNumberText, CultureInfo.InvariantCulture);
				var previousSequenceNumber = currentNumber - 1;
				var prefix = AH_TransactionReference.Substring(0, AH_TransactionReference.Length - currentNumberText.Length);
				var previousSequenceNumberAsString = prefix + previousSequenceNumber.ToString(CultureInfo.InvariantCulture).PadLeft(sequence.XD_MaximumNumberDigits, '0');

				ZString firstNumberAsString = ((int)sequence.XD_StartNumber).ToString(CultureInfo.InvariantCulture).PadLeft(sequence.XD_MaximumNumberDigits, '0');
				// if the invoice is Not the 1st in the serie, then need to return the one in front of it, otherwise returns null
				if (AH_TransactionReference != prefix + firstNumberAsString)
				{
					var previousInvoice = Factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionReference, previousSequenceNumberAsString).AddToFilter(AccTransactionHeaderSchema.AH_GC, AH_GC));
					// if 0 or there are more than 1, it's an error which we should investigate!
					if (previousInvoice.Length != 1)
					{
						IsDigitialSignatureFailedDueToPreviousInvoiceNotFound = true;
					}
					else
					{
						result = previousInvoice[0];
					}
				}
			}
			return result;
		}

		IComplianceSequenceRetriever ComplianceSequenceRetriever => complianceSequenceRetriever ?? (complianceSequenceRetriever = new ComplianceSequenceRetriever());
		IComplianceSequenceRetriever complianceSequenceRetriever;

		#endregion

		#region UpdateComplianceSubTypeAndSequenceNumber

		bool IsComplianceSubTypeValid => !AH_ComplianceSubType.IsEmpty;

		bool ShouldAllocateComplianceNumber => !IsComplianceDocumentModuleEnabled
			&& IsComplianceSubTypeValid
			&& (ShouldAllocateComplianceNumberOnPosting() || IsReversalTransactionEligibleForComplianceNumber)
			&& (ObjectFactory.Get<IAccountingCountryComplianceGlobalFactory>().GetFeatureInterface<IComplianceNumberProvider>(Company.GC_RN_NKCountryCode)?.CanAllocateComplianceNumber(AllLinesWithCMTCharge) ?? true);

		#region Vietnam Restriction

		bool AllLinesWithCMTCharge => Lines.Cast<InvoicingLineBase>().All(x => ChargeType.Comment.Equals(x.ChargeCode?.AC_ChargeType ?? ZString.Empty));

		public bool IsValidTransactionToAllocateInVietnam => (AH_TransactionType == TransactionTypes.Invoice ||
															IsAmendingCreditNote &&
															AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.Value) &&
															!AH_ComplianceSubType.IsEmpty &&
															string.IsNullOrEmpty(AH_TransactionReference) &&
															!(AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value && IsCancelled) &&
															!AllLinesWithCMTCharge;

		public bool IsValidTransactionToPrintGovtTaxInvoiceInVietnam => AH_Ledger == LedgerTypes.AccountsReceivable &&
																		AH_TransactionType == TransactionTypes.Invoice &&
																		!string.IsNullOrEmpty(AH_ComplianceSubType) &&
																		!IsCancelled &&
																		!AllLinesWithCMTCharge;

		#endregion

		public bool ShouldApplyCompliancePolicyForPrintingAuthorizationNumber => ShouldAllocateComplianceNumber
				&& (ObjectFactory.Get<ICountryComplianceFactory>()?.GetITransactionAuthorizationNumber(Company.GC_RN_NKCountryCode)?.IsTransactionAuthorizationNumberEnabled(AH_GC, AH_InvoiceDate) ?? false)
				&& AH_Ledger == LedgerTypes.AccountsReceivable
				&& new List<string>() { TransactionTypes.Invoice, TransactionTypes.CreditNote, TransactionTypes.AdjustmentNote }.Contains(AH_TransactionType);

		bool IsReversalTransactionEligibleForComplianceNumber =>
			!IsInDatabase
			&& AH_IsCancelled
			&& ShouldAllocateComplianceNumberForReversalTransactions()
			&& OriginalTransaction != null
			&& !OriginalTransaction.AH_ComplianceSubType.IsEmpty
			&& !OriginalTransaction.AH_TransactionReference.IsEmpty;

		public void UpdateComplianceSubTypeAndSequenceNumberTogether()
		{
			if (Factory.IsEqualToCurrentSaveCount(FactoryCountUsedToGenerateTransactionReference))
			{
				return;
			}

			if (SetComplianceSubTypeIfIsNecessary() && ShouldAllocateComplianceNumber)
			{
				try
				{
					AllocateComplianceSequenceNumberForInvoice(false);
				}
				catch (ComplianceSequenceRelatedException ex)
				{
					if (ex is FailedToFindComplianceSequenceException ||
						ex is AllocationComplianceSequenceFullException ||
						ex is AllocationComplianceSequenceBusyException ||
						ex is UnableToAllocateNumberDueToPostDateEarlierThanLastDateUsedException ||
						ex is UnableToAllocateNumberDueToSparseComplianceBookException ||
						ex is ComplianceNumberExceedMaximumLengthException)
					{
						IsComplianceSequenceFailedToAssign = true;
						EventArgsForCompliance = new ComplianceSequenceRelatedExceptionEventArgs(ex);
					}
					else if (ShouldShowErrorAndStopSaving(ex))
					{
						throw;
					}
				}
			}
		}

		static bool ShouldShowErrorAndStopSaving(ComplianceSequenceRelatedException ex)
			=> ex is InvoiceDateLessThanPreviousException ||
				ex is PostDateLessThanPreviousException ||
				ex is InvoiceDateGreaterThanPostDateException ||
				ex is HasNonCMTChargeZeroAmountLineException;

		public ZString GetMatchingComplianceSubType()
		{
			var result = string.Empty;
			if (GlbCompany.CurrentCompany.Country.SupportComplianceSubType &&
				(AH_Ledger == LedgerTypes.AccountsPayable || AH_Ledger == LedgerTypes.AccountsReceivable) &&
				(AH_TransactionType == TransactionTypes.Invoice || AH_TransactionType == TransactionTypes.CreditNote) &&
				(!IsInDatabase || IsAllocatingInvoice || IsCompletingInvoice || IsApprovingInvoice))
			{
				result = ComplianceSubTypeRule.GetMatchingComplianceSubType(AH_GB.ToGuid());
			}
			return result;
		}

		public ZString AllocateAndLogComplianceSubTypeIfNumberingByPostDate()
		{
			var warningMessageForUnapproved = ZString.Empty;
			if (IsComplianceNumberAllocationMandatory && ShouldAllocateComplianceNumberOnPosting())
			{
				if (this.HasContext(APInvoiceChargesApprovalRequest.Context.Editing))
				{
					AH_ComplianceSubType = ZString.Empty;
					warningMessageForUnapproved = Res.GetString("037648D8-6E18-4F66-A5F7-6E6F0F6E46AF",
						$"Note: Unapproved Invoice is saved without Compliance Sub Type, because it must be set only when Compliance Number is allocated");
				}
				else
				{
					SetComplianceSubTypeIfIsNecessary();
				}
			}
			return warningMessageForUnapproved;
		}

		public bool SetComplianceSubTypeIfIsNecessary()
		{
			var defaultComplianceSubType = GetMatchingComplianceSubType();
			if (!AH_ComplianceSubType.Equals(defaultComplianceSubType))
			{
				#region SuppressResourceStringsCheckRegion
				if (AH_ComplianceSubType.IsEmpty)
				{
					AH_ComplianceSubType = defaultComplianceSubType;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(AutoEvents.EditedARecord, $"Compliance Sub Type was defaulted to {defaultComplianceSubType}.");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				else if (defaultComplianceSubType.IsEmpty)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(AutoEvents.EditedARecord, $"Compliance Sub Type was manually set to {AH_ComplianceSubType}. No default Compliance Sub Type was found.");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				else
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(AutoEvents.EditedARecord, $"Compliance Sub Type was manually set to {AH_ComplianceSubType}. The default was {defaultComplianceSubType}.");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				#endregion
			}

			return IsComplianceSubTypeValid;
		}

		public bool ShouldAllocateComplianceNumberOnPosting()
		{
			bool result = false;
			if (AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				result = ShouldAllocateComplianceNumberOnPostingForAR();
			}
			else if (AH_Ledger == LedgerTypes.AccountsPayable)
			{
				result = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables.Value == AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;
			}
			return result;
		}

		public bool ShouldAllocateComplianceNumberOnPostingForAR()
		{
			var allocationMethod = GetComplianceAllocationMethodAR();
			return allocationMethod == AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;
		}

		bool ShouldAllocateComplianceNumberForReversalTransactions()
		{
			bool result = false;
			if (AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				var allocationMethod = GetComplianceAllocationMethodAR();
				result = allocationMethod == AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;
			}
			return result;
		}

		[Flags]
		enum ComplianceFailureStatus
		{
			None = 0,
			ComplianceSequenceFailedToAssign = 1,
			DigitialSignatureFailedDueToPreviousInvoiceNotFound = 2,
			DigitialSignatureFailedDueToEmptySignatureInPreviousInvoice = 4,
			DigitialSignatureFailedDueToNotBeingAbleToGetInvoiceCreatedLogTime = 8
		}

		ComplianceFailureStatus complianceFailureStatus = ComplianceFailureStatus.None;

		public ComplianceSequenceRelatedExceptionEventArgs EventArgsForCompliance { get; set; }

		public static string GetMessageForComplianceSequenceErrors(EventArgs e)
		{
			string msg;

			if (e is ComplianceSequenceRelatedExceptionEventArgs compSeqEx)
			{
				msg = compSeqEx.complianceSequenceRelatedException.UserFriendlyMessage;
			}
			else if (e is NoComplianceInvoicesToPrintHBDExceptionEventArgs hBDEx)
			{
				msg = hBDEx.noComplianceInvoicesToPrintHBDException.UserFriendlyMessage;
			}
			else
			{
				msg = ComplianceSequenceNumberAllocationErrorMessages.NoComplianceInvoicesToPrintLBDMessage;
			}

			return msg;
		}

		bool IsComplianceSequenceFailedToAssign
		{
			get { return (complianceFailureStatus & ComplianceFailureStatus.ComplianceSequenceFailedToAssign) == ComplianceFailureStatus.ComplianceSequenceFailedToAssign; }
			set
			{
				if (value != IsComplianceSequenceFailedToAssign)
				{
					complianceFailureStatus ^= ComplianceFailureStatus.ComplianceSequenceFailedToAssign;
				}
			}
		}

		public bool IsDigitialSignatureFailedDueToPreviousInvoiceNotFound
		{
			get { return (complianceFailureStatus & ComplianceFailureStatus.DigitialSignatureFailedDueToPreviousInvoiceNotFound) == ComplianceFailureStatus.DigitialSignatureFailedDueToPreviousInvoiceNotFound; }
			private set
			{
				if (value != IsDigitialSignatureFailedDueToPreviousInvoiceNotFound)
				{
					complianceFailureStatus ^= ComplianceFailureStatus.DigitialSignatureFailedDueToPreviousInvoiceNotFound;
				}
			}
		}

		public bool IsDigitialSignatureFailedDueToEmptySignatureInPreviousInvoice
		{
			get { return (complianceFailureStatus & ComplianceFailureStatus.DigitialSignatureFailedDueToEmptySignatureInPreviousInvoice) == ComplianceFailureStatus.DigitialSignatureFailedDueToEmptySignatureInPreviousInvoice; }
			private set
			{
				if (value != IsDigitialSignatureFailedDueToEmptySignatureInPreviousInvoice)
				{
					complianceFailureStatus ^= ComplianceFailureStatus.DigitialSignatureFailedDueToEmptySignatureInPreviousInvoice;
				}
			}
		}

		public bool IsDigitialSignatureFailedDueToNotBeingAbleToGetInvoiceCreatedLogTime
		{
			get { return (complianceFailureStatus & ComplianceFailureStatus.DigitialSignatureFailedDueToNotBeingAbleToGetInvoiceCreatedLogTime) == ComplianceFailureStatus.DigitialSignatureFailedDueToNotBeingAbleToGetInvoiceCreatedLogTime; }
			private set
			{
				if (value != IsDigitialSignatureFailedDueToNotBeingAbleToGetInvoiceCreatedLogTime)
				{
					complianceFailureStatus ^= ComplianceFailureStatus.DigitialSignatureFailedDueToNotBeingAbleToGetInvoiceCreatedLogTime;
				}
			}
		}

		void AllocateComplianceSequenceNumberForInvoice(bool isPrintingOptionAllocateSequenceNumberOnly, bool skipSparseBookCheck = false)
		{
			if (Factory.IsEqualToCurrentSaveCount(FactoryCountUsedToGenerateTransactionReference))
			{
				return;
			}

			var complianceSequence = GetComplianceSequenceFromSubTypeWithCheck();
			// TODO: Need to check in here if the compliance sequence type is configured for preprinted paper - if so, don't allocate and throw exception
			if (!(complianceSequence.IsConfiguredAsPrePrintedSequence && isPrintingOptionAllocateSequenceNumberOnly))
			{
				if (!skipSparseBookCheck)
				{
					EnsureNoPastTransactionsWithEmptyComplNum(this);
				}

				CheckAllocationDateEarlierThanLastDateUsed();

				var nextNumberInfo = complianceSequence.GetNextNumberInfo(this, ComplianceNumberAllocationDateWithFallbackValue, ComplianceNumberAllocationDateOption, ComplianceNumberAllocationDate);

				if (!nextNumberInfo.NextNumberWithPrefix.IsEmpty)
				{
					CheckComplianceNumberLength(nextNumberInfo.NextNumberWithPrefix);

					AH_TransactionReference = nextNumberInfo.NextNumberWithPrefix;
					AH_XD_ComplianceBook = complianceSequence.PK;
					FactoryCountUsedToGenerateTransactionReference = Factory.SaveCount;
					if (IsDigitalSignatureApplicable)
					{
						SignInvoiceWithRSASignature();
					}
					if (AH_ComplianceDocumentDate.IsEmpty && GlbCompany.CurrentCompany.Country.Code == CountryCodes.VietNam)
					{
						AH_ComplianceDocumentDate = ZDate.Today;
					}
					if (Ledger == LedgerTypes.AccountsReceivable && GlbCompany.CurrentCompany.Country.Code == CountryCodes.Portugal && !complianceSequence.XD_PrintingAuthorizationNumber.IsEmpty)
					{
						AuthorizationNumberReference = complianceSequence.XD_PrintingAuthorizationNumber + "-" + nextNumberInfo.NextNumber;
					}

					CheckInvoiceDateLessThanPrevious();
					CheckPostDateLessThanPrevious();
					CheckInvoiceDateNotHigherThanPostDate();
					CheckHasNonCMTChargeZeroAmountLine();
				}
			}
			else
			{
				throw new CannotAllocateSequenceNumberToPrePrintedSequenceWhenNotPrintingException();
			}
		}

		public void AllocateComplianceSequenceNumberForInvoiceIfEmpty(bool isPrintingOptionAllocateSequenceNumberOnly, bool skipSparseBookCheck = false)
		{
			if (AH_TransactionReference.IsEmpty)
			{
				AllocateComplianceSequenceNumberForInvoice(isPrintingOptionAllocateSequenceNumberOnly, skipSparseBookCheck);
			}
		}

		#endregion

		#region Check Methods

		public ZString AssignComplianceSubTypeAndCheckComplianceErrors()
		{
			var result = ZString.Empty;
			if (ShouldAllocateComplianceNumberOnPosting() && ShouldCheckForCompliance)
			{
				try
				{
					SetComplianceSubTypeIfIsNecessary();

					CheckIfComplianceBookIsFullOrExpired();
					CheckAllocationDateEarlierThanLastDateUsed();
					EnsureNoPastTransactionsWithEmptyComplNum(this);
				}
				catch (ComplianceSequenceRelatedException ex)
				{
					result = ex.UserFriendlyMessage;
				}
			}
			return result;
		}

		public static void EnsureNoPastTransactionsWithEmptyComplNum(params InvoicingBase[] transactions)
		{
			foreach (var transactionsGroupedByLedger in transactions.GroupBy(t => t.AH_Ledger))
			{
				if (!transactionsGroupedByLedger.First().IsComplianceNumberAllocationMandatory)
				{
					continue;
				}

				var transactionsToModify = transactionsGroupedByLedger.Where(th => th.ComplianceNumberAllocationDate.IsValid).Select(x => new { transaction = x , allocationDate = x.ComplianceNumberAllocationDate }).ToArray();
				if (!transactionsToModify.Any())
				{
					continue;
				}

				var firstTransaction = transactionsToModify.First().transaction;
				var sequence = firstTransaction.GetComplianceSequenceFromSubTypeWithCheck();
				foreach (var trans in transactionsToModify.Select(x => x.transaction))
				{
					var transSequence = trans.GetComplianceSequenceFromSubTypeWithCheck();
					if (transSequence.PK != sequence.PK)
					{
						throw new MultipleComplianceSequenceFoundException();
					}
				}

				var startDate = sequence.XD_StartDate;
				var expiryDate = sequence.XD_ExpiryDate;

				var dateColumn = firstTransaction.ComplianceNumberAllocationDateColumn;
				var transHeaderQuery = new ZQuery(AccTransactionHeaderSchema.AH_GC, sequence.XD_GC_Company);
				transHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_ComplianceSubType, sequence.XD_SequenceClass);
				transHeaderQuery.AddToFilter(dateColumn, SQLComparisonOperator.LessThan, transactionsToModify.Max(x => x.allocationDate).Date);
				transHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionReference, SQLComparisonOperator.Equal, ZString.Empty);
				transHeaderQuery.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, transactionsToModify.Select(x => x.transaction.PK));

				if (startDate.IsValid)
				{
					transHeaderQuery.AddToFilter(dateColumn, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, startDate);
				}
				if (expiryDate.IsValid)
				{
					transHeaderQuery.AddToFilter(dateColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, expiryDate);
				}

				if (firstTransaction.Factory.ExistsInDatabase(AccTransactionHeader.Schema.TableName, transHeaderQuery))
				{
					throw new UnableToAllocateNumberDueToSparseComplianceBookException(sequence.XD_SequenceClass, transactionsToModify.Min(x => x.allocationDate).Date, firstTransaction.ComplianceNumberAllocationDateOption);
				}
			}
		}

		public void CheckComplianceNumberLength(ZString complianceNumber)
		{
			if (complianceNumber.Length > AccTransactionHeaderSchema.AH_TransactionReference.MaxLength)
			{
				throw new ComplianceNumberExceedMaximumLengthException(complianceNumber);
			}
		}

		public void CheckAllocationDateEarlierThanLastDateUsed()
		{
			var sequence = GetComplianceSequenceFromSubTypeWithCheck();
			if (IsComplianceNumberAllocationDateEarlierThanLastDateUsedInBook(sequence))
			{
				throw new UnableToAllocateNumberDueToPostDateEarlierThanLastDateUsedException(AH_ComplianceSubType, GetLastDateUsedInComplianceBook(sequence), ComplianceNumberAllocationDateOption);
			}
		}

		public void CheckIfComplianceBookIsFullOrExpired()
		{
			var sequence = GetComplianceSequenceFromSubTypeWithCheck();
			if (sequence.XD_NextNumber > sequence.XD_EndNumber)
			{
				throw new AllocationComplianceSequenceFullException();
			}
			var startDate = sequence.XD_StartDate;
			var expiryDate = sequence.XD_ExpiryDate;
			var allocationDate = ComplianceNumberAllocationDate;

			if (allocationDate.IsValid &&
				(expiryDate.IsValid && allocationDate.Date > expiryDate || startDate.IsValid && allocationDate.Date < startDate))
			{
				throw new AllocationComplianceSequenceFullException();
			}
		}

		void CheckInvoiceDateLessThanPrevious()
		{
			if (GlbCompany.CurrentCompany.Country.Code == CountryCodes.Portugal &&
				TransactionsUseThisComplianceBook != null &&
				AH_InvoiceDate.IsValid &&
				TransactionsUseThisComplianceBook.Any(x => x.PK != PK && x.AH_InvoiceDate.Date > AH_InvoiceDate.Date))
			{
				throw new InvoiceDateLessThanPreviousException();
			}
		}

		void CheckHasNonCMTChargeZeroAmountLine()
		{
			if (GlbCompany.CurrentCompany.Country.Code == CountryCodes.VietNam &&
				AH_Ledger == LedgerTypes.AccountsReceivable &&
				(AH_TransactionType == TransactionTypes.Invoice || AH_TransactionType == TransactionTypes.CreditNote) &&
				Lines.Cast<InvoicingLineBase>().Any(line => !ChargeType.Comment.Equals(line.ChargeCode?.AC_ChargeType ?? ZString.Empty) && line.AL_LineAmount == 0))
			{
				throw new HasNonCMTChargeZeroAmountLineException();
			}
		}

		void CheckPostDateLessThanPrevious()
		{
			if (GlbCompany.CurrentCompany.Country.Code == CountryCodes.Portugal &&
				AH_PostDate.IsValid &&
				TransactionsUseThisComplianceBook != null &&
				TransactionsUseThisComplianceBook.Any(x => x.PK != PK && x.AH_PostDate.Date > AH_PostDate.Date))
			{
				throw new PostDateLessThanPreviousException();
			}
		}

		void CheckInvoiceDateNotHigherThanPostDate()
		{
			if (GlbCompany.CurrentCompany.Country.Code == CountryCodes.Portugal &&
				AH_InvoiceDate.IsValid &&
				AH_PostDate.IsValid &&
				AH_InvoiceDate.Date > AH_PostDate.Date)
			{
				throw new InvoiceDateGreaterThanPostDateException();
			}
		}

		AccTransactionHeader[] TransactionsUseThisComplianceBook
		{
			get
			{
				if (!AH_XD_ComplianceBook.IsEmpty && fTransactionsUseThisComplianceBook == null)
				{
					fTransactionsUseThisComplianceBook = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_XD_ComplianceBook, AH_XD_ComplianceBook));
				}

				return fTransactionsUseThisComplianceBook;
			}
		}
		AccTransactionHeader[] fTransactionsUseThisComplianceBook;

		AccComplianceSequence invComplSeq;
		AccComplianceSequence GetComplianceSequenceFromSubTypeWithCheck()
		{
			if (invComplSeq == null)
			{
				invComplSeq = ComplianceSequenceFromSubType;
			}
			if (invComplSeq == null)
			{
				throw new FailedToFindComplianceSequenceException();
			}
			return invComplSeq;
		}

		#endregion

		#region Create Compliance Document

		public ZBool IsComplianceDocumentModuleEnabled => AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.GetValueWithoutFallback(Company.PK.ToGuid(), Guid.Empty, Guid.Empty);

		public ZBool ShouldCheckForCompliance => IsComplianceNumberAllocationMandatory && Company.Country.SupportComplianceSubType;

		public ZBool IsCreditNoteComplianceDocumentConfigurationEnabled => AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.GetValueWithoutFallback(Company.PK.ToGuid(), Guid.Empty, Guid.Empty);

		public ZBool IsAmendingARCreditNoteForComplianceDocument => IsCreditNoteComplianceDocumentConfigurationEnabled && IsAmendingWithARCreditNote;

		public ZBool IsPostingMiscARCreditNoteLinkedToARInvoiceForComplianceDocument => IsCreditNoteComplianceDocumentConfigurationEnabled && !IsInDatabase && AH_Ledger == LedgerTypes.AccountsReceivable && AH_TransactionType == TransactionTypes.CreditNote && OriginalReferenceTransaction != null;

		public ZBool IsPreventedInvoiceDateGreaterThanPostDate => AccountingMasterFilesRegistry.Instance.PreventInvoiceDateGreaterThanPostDate.GetValueWithoutFallback(Company.PK.ToGuid(), Guid.Empty, Guid.Empty);

		public bool HasSupportedTransactionTypes
		{
			get { return AH_TransactionType == TransactionTypes.Invoice || AH_TransactionType == TransactionTypes.CreditNote; }
		}

		bool ShouldCreateComplianceDocumentBasedOnFactoryCache()
		{
			var alreadyCreatedPKs = Factory.GetCachedValue("CreatedComplianceDocumentOfInvoicePKs", () => new HashSet<ZGuid>(), CacheStalenessPolicy.StaleOnFactorySave);
			return alreadyCreatedPKs.Add(PK); // If false, there is another BusinessObject around the same DataRow which already queued for Compliance Reports
		}

		bool ShouldAutoCreateComplianceDocument
		{
			get
			{
				var notApplicable = OrganisationCreateComplianceDocumentOnPostingTypes.NotApplicable;
				switch (AH_Ledger)
				{
					case LedgerTypes.AccountsPayable:
						var apCreateVATComplianceDocumentOnPosting = Header?.CompanyData?.OB_APCreateVATComplianceDocumentOnPosting ?? notApplicable;
						if (apCreateVATComplianceDocumentOnPosting == OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge)
						{
							return true;
						}
						else if (apCreateVATComplianceDocumentOnPosting == OrganisationCreateComplianceDocumentOnPostingTypes.PerComplianceDocumentNumber)
						{
							return Lines.Cast<InvoicingLineBase>().Any(x => x.CreateComplianceDocumentRecordOnPosting);
						}
						else
						{
							return false;
						}
					case LedgerTypes.AccountsReceivable:
						var arCreateVATComplianceDocumentOnPosting = Header?.CompanyData?.OB_ARCreateVATComplianceDocumentOnPosting ?? notApplicable;

						if (IsBadDebtWritingOff)
						{
							return false;
						}
						else if (TransactionType == TransactionTypes.Invoice)
						{
							return arCreateVATComplianceDocumentOnPosting != notApplicable;
						}
						else
						{
							var complianceDocumentHeader = GetOriginalTransactionGeneratedComplianceDocument();
							if (complianceDocumentHeader == null)
							{
								return arCreateVATComplianceDocumentOnPosting != notApplicable;
							}
							else
							{
								if (IsCreditNoteComplianceDocumentConfigurationEnabled)
								{
									return !complianceDocumentHeader.ADH_DocumentNumber.IsEmpty;
								}
								else
								{
									return arCreateVATComplianceDocumentOnPosting != notApplicable;
								}
							}
						}
					default:
						return false;
				}
			}
		}

		public bool HasGeneratedComplianceDocument(ZGuid pk)
		{
			var query = GetComplianceDocumentByTransactionQuery(pk);
			return Factory.Exists(typeof(AccComplianceDocumentHeader), query);
		}

		public AccComplianceDocumentHeader GetTransactionGeneratedComplianceDocument()
		{
			return GetComplianceDocumentByTransaction(PK);
		}

		public AccComplianceDocumentHeader[] GetAllTransactionGeneratedComplianceDocument()
		{
			return GetAllComplianceDocumentByTransaction(PK);
		}

		public AccComplianceDocumentHeader GetOriginalTransactionGeneratedComplianceDocument()
		{
			return GetComplianceDocumentByTransaction(OriginalTransactionReference);
		}

		AccComplianceDocumentHeader GetComplianceDocumentByTransaction(ZGuid pk)
		{
			var query = GetComplianceDocumentByTransactionQuery(pk);

			return Factory.LoadTop1<AccComplianceDocumentHeader>(query);
		}

		AccComplianceDocumentHeader[] GetAllComplianceDocumentByTransaction(ZGuid pk)
		{
			var query = GetComplianceDocumentByTransactionQuery(pk);

			return Factory.Load<AccComplianceDocumentHeader>(query);
		}

		ZDBOnlyQuery GetComplianceDocumentByTransactionQuery(ZGuid pk)
		{
			var query = new ZDBOnlyQuery(typeof(AccComplianceDocumentHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(AccComplianceDocumentLine), AccComplianceDocumentLineSchema.ADL_ADH);
			var subQuery1 = new ZDBOnlySubQuery(typeof(AccComplianceDocumentPivot), AccComplianceDocumentPivotSchema.ADP_ADL);
			var subQuery2 = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.PK);
			var subQuery3 = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
			subQuery3.AddToFilter(AccTransactionHeaderSchema.PK, pk);

			subQuery2.AddSubQuery(AccTransactionLinesSchema.AL_AH, subQuery3, JoinCondition.And);
			subQuery1.AddSubQuery(AccComplianceDocumentPivotSchema.ADP_AL, subQuery2, JoinCondition.And);
			subQuery.AddSubQuery(AccComplianceDocumentLineSchema.PK, subQuery1, JoinCondition.And);
			query.AddSubQuery(subQuery, JoinCondition.And);

			query.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_DocumentStatus, SQLComparisonOperator.NotEqual, Core.Constants.ComplianceDocumentStatus.Voided);

			return query;
		}

		public bool LinesContainEmptyTaxID
		{
			get
			{
				return Lines.Cast<InvoicingLineBase>().Any(x => !x.AL_AT.IsValid && !ChargeType.Comment.Equals(x.ChargeCode?.AC_ChargeType ?? ZString.Empty));
			}
		}

		public bool CanCreateComplianceDocument => IsBeingCreatedOrPayablesWithLedgerChanged
			&& HasSupportedTransactionTypes
			&& !AH_IsCancelled
			&& IsComplianceDocumentModuleEnabled
			&& ShouldAutoCreateComplianceDocument
			&& !LinesContainEmptyTaxID;

		public bool CanPromptToPrintComplianceDocument => !HasNegativeComplianceLinesWhenCreating
			&& Ledger == LedgerTypes.AccountsReceivable
			&& AccountingMasterFilesRegistry.Instance.PromptToPrintComplianceDocumentOnCreation.Value;

		public PromptAndPrintComplianceDocumentHandler OnPromptAndPrintComplianceDocumentHandler;
		public delegate void PromptAndPrintComplianceDocumentHandler(
			Func<string, bool> userChoiceIsPrint,
			Action<string, bool> showDialog,
			IEnumerable<AccComplianceDocumentHeader> complianceDocuments,
			string message,
			bool isSkipPrompt = false);

		public bool HasNegativeComplianceLinesWhenCreating { get; private set; }

		void CreateComplianceDocument()
		{
			if (CanCreateComplianceDocument && ShouldCreateComplianceDocumentBasedOnFactoryCache())
			{
				var complianceDocuments = new ComplianceDocumentCreator(new[] { this }, CreateOption).CreateComplianceDocumentRecords();

				if (complianceDocuments.Any())
				{
					HasNegativeComplianceLinesWhenCreating = false;
					HookComplianceDocumentRelatedPropertiesChanged();
				}
				else
				{
					HasNegativeComplianceLinesWhenCreating = true;
				}

				if (CanPromptToPrintComplianceDocument)
				{
					OnPromptAndPrintComplianceDocumentHandler += ComplianceDocumentHelper.PromptAndPrintComplianceDocument;
				}
			}
		}

		string CreateOption => HasPCDSettingForAP ? OrganisationCreateComplianceDocumentOnPostingTypes.PerComplianceDocumentNumber : OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge;

		public ZBool HasPCDSettingForAP => AH_Ledger == LedgerTypes.AccountsPayable
			&& (AH_TransactionType == TransactionTypes.Invoice || AH_TransactionType == TransactionTypes.CreditNote)
			&& IsEnabledComplianceDocumentModuleAndHasPCDSetting;

		public ZBool HasPCDSettingForIN => AH_Ledger == LedgerTypes.IncompleteTransactions
			&& (AH_TransactionType == TransactionTypes.IncompleteInvoice || AH_TransactionType == TransactionTypes.IncompleteCreditNote)
			&& IsEnabledComplianceDocumentModuleAndHasPCDSetting;

		public ZBool IsEnabledComplianceDocumentModuleAndHasPCDSetting => IsComplianceDocumentModuleEnabled && (Header?.CompanyData?.IsAPPCDSetting ?? false);

		ComplianceSubTypeRule ComplianceSubTypeRule
		{
			get
			{
				if (complianceSubTypeRule == null)
				{
					complianceSubTypeRule = new ComplianceSubTypeRule(Factory, this);
				}
				return complianceSubTypeRule;
			}
		}
		ComplianceSubTypeRule complianceSubTypeRule;

		#endregion

		#region IEvaluateComplianceRule

		ZString IEvaluateComplianceRule.Ledger => Ledger;

		ZString IEvaluateComplianceRule.TransactionType => TransactionType;

		ZString IEvaluateComplianceRule.ComplianceSubType => AH_ComplianceSubType;

		ZBool IEvaluateComplianceRule.IsDisbursementOrFinal => IsDisbursementOrFinal;

		ZBool IEvaluateComplianceRule.IsSelfBillingInvoice => IsSelfBillingInvoice;

		ZBool IEvaluateComplianceRule.IsAmendingTransaction => IsAmendingTransaction;

		ZBool IEvaluateComplianceRule.IsReversalTransaction => IsReversalTransaction;

		OrgHeader IEvaluateComplianceRule.Header => Header;

		GlbCompany IEvaluateComplianceRule.Company => Company;

		IEnumerable<AccTransactionLines> IEvaluateComplianceRule.Lines => Lines.ToArray<AccTransactionLines>();

		ZBool IEvaluateComplianceRule.EmptyLedgerMatchesAll => true;

		ZBool IEvaluateComplianceRule.EmptyTransactionTypeMatchesAll => true;

		IEnumerable<AccTaxTransaction> IEvaluateComplianceRule.TaxTransactions => ObjectFactory.Get<ITaxProcessor>().GetTaxTransactions(TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(this));

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		ZDecimal IEvaluateComplianceRule.LocalTotalAmount => AH_LocalTotalAmount;

		#endregion

		#region IComplianceRuleParentTransaction

		IEvaluateComplianceRule IComplianceRuleParentTransaction.ParentTransaction => (OriginalTransaction ?? GetOriginalTransactionForAmending()) as IEvaluateComplianceRule;

		#endregion

		#region IComplianceNumberSequence

		public ZBool IsCorrected => NumberFountainTransactionDataProvider.GetIsCorrected(this);

		public ZString ComplianceTransactionType => AH_TransactionType;

		public ZString ComplianceSubType => AH_ComplianceSubType;

		public ZDateTime ComplianceDocumentDate => AH_ComplianceDocumentDate;

		public ZDateTime PostDate => AH_PostDate;

		public ZDateTime InvoiceDate => AH_InvoiceDate;

		#endregion

		#region ICanBeQueuedForComplianceReport

		ComplianceSubTypeRule ISupportQueueingForComplianceReports.ComplianceMatchingRule => ComplianceSubTypeRule;

		bool ISupportQueueingForComplianceReports.CheckIsValidForQueueing(BusinessObjectFactory factory) =>
			ShouldQueueForComplianceReports() && (!IsInDatabase || IsAllocatingInvoice || IsCompletingInvoice || IsApprovingInvoice ||
			(IsComplianceSubTypeUpdatedOnPrinting && CanQueueIfUpdatingComplianceSubTypeOnPrinting(factory)));

		IComplianceReportQueuer ISupportQueueingForComplianceReports.Queuer => queuer ?? (queuer = new ComplianceReportTransactionQueuer<InvoicingBase>(this));
		ComplianceReportTransactionQueuer<InvoicingBase> queuer;

		#endregion

		#region Queue for Compliance Report

		internal IEnumerable<AccComplianceReport> GetFinalisedComplianceReports()
		{
			return GetGeneratedComplianceReports(Factory).Where(x => x.ACR_IsFinalised);
		}

		IEnumerable<AccComplianceReport> GetGeneratedComplianceReports(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));
			var result = Enumerable.Empty<AccComplianceReport>();

			var headerQuery = new ZQuery(AccComplianceReportTransactionPivotSchema.ACL_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);
			headerQuery.AddToFilter(AccComplianceReportTransactionPivotSchema.ACL_ParentID, PK);

			var lineQuery = new ZQuery(AccComplianceReportTransactionPivotSchema.ACL_ParentTableCode, AccTransactionLinesSchema.Constants.Prefix);
			lineQuery.AddToFilter(AccComplianceReportTransactionPivotSchema.ACL_ParentID, Lines.GetPKs());

			var pivotFilter = new ZQuery(headerQuery, JoinCondition.Or, lineQuery);
			var sql = string.Format("SELECT DISTINCT {0} FROM {1} {2}",
				AccComplianceReportTransactionPivotSchema.Constants.ACL_ACR_Report,
				AccComplianceReportTransactionPivotSchema.Constants.TableName,
				pivotFilter.GetAsWhereClause(combineFilterAndParams: false));
			var reportPKs = new DynamicBusinessObjectCollection(factory);
			reportPKs.Load(sql, pivotFilter.Params);

			var pks = reportPKs.Select(x => (ZGuid)x[AccComplianceReportTransactionPivotSchema.Constants.ACL_ACR_Report]);
			if (pks.Any())
			{
				result = factory.Load<AccComplianceReport>(new ZQuery(AccComplianceReportSchema.PK, pks));
			}
			return result;
		}

		bool ShouldQueueForComplianceReports()
		{
			var result = (AH_Ledger == LedgerTypes.AccountsReceivable || AH_Ledger == LedgerTypes.AccountsPayable)
				&& (AH_TransactionType == TransactionTypes.Invoice || AH_TransactionType == TransactionTypes.CreditNote || AH_TransactionType == TransactionTypes.AdjustmentNote);

			if (result)
			{
				var alreadyQueuedPKs = Factory.GetCachedValue("QueuedForComplianceReportPKs", () => new HashSet<ZGuid>(), CacheStalenessPolicy.StaleOnFactorySave);
				result = alreadyQueuedPKs.Add(PK); // If false, there is another BusinessObject around the same DataRow which already queued for Compliance Reports
			}
			return result;
		}

		bool CanQueueIfUpdatingComplianceSubTypeOnPrinting(BusinessObjectFactory factory)
		{
			var result = true;
			if (IsComplianceSubTypeUpdatedOnPrinting)
			{
				var generatedReports = GetGeneratedComplianceReports(factory);
				if (generatedReports.Any(x => x.ACR_IsFinalised))
				{
					result = false;
				}
				else
				{
					DeleteExistingQueueEntries(factory);
					generatedReports.ForEach(report => report.Invalidate());
				}
			}
			return result;
		}

		bool IsComplianceSubTypeUpdatedOnPrinting => IsInDatabase && AH_ComplianceSubTypeInfo.HasChanges;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void DeleteExistingQueueEntries(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));
			var reportTypes = ComplianceReportTransactionQueueingHelper.GetComplianceReportsOfCompanyCountry()
				.Where(x => x.ReportBaseTablePrefix == AccTransactionHeaderSchema.Constants.Prefix
						 || x.ReportBaseTablePrefix == AccTransactionLinesSchema.Constants.Prefix)
				.Select(x => x.ReportCode).Distinct();
			if (!reportTypes.Any())
			{
				return;
			}

			var subqueryForAL = $@"
	SELECT {AccTransactionLinesSchema.Constants.PK}
	FROM {AccTransactionLinesSchema.Constants.SqlSchemaName}.{AccTransactionLinesSchema.Constants.TableName}
	WHERE {AccTransactionLinesSchema.Constants.AL_AH} = @ParentPK
";

			// Performance: query adds additional columns to ensure clustered index seek.
			var reportCodesParameterInline = string.Join(",", reportTypes.Select(x => "'" + x + "'"));
			var sql = FormattableString.Invariant($@"
DECLARE @ALPostDateFrom date;
DECLARE @ALPostDateTo date;
SELECT @ALPostDateFrom = MIN({AccTransactionLinesSchema.Constants.AL_PostDate}),
       @ALPostDateTo   = MAX({AccTransactionLinesSchema.Constants.AL_PostDate})
FROM {AccTransactionLinesSchema.Constants.SqlSchemaName}.{AccTransactionLinesSchema.Constants.TableName}
WHERE {AccTransactionLinesSchema.Constants.AL_AH} = @ParentPK

DELETE {AccTransactionComplianceReportQueueSchema.Constants.SqlSchemaName}.{AccTransactionComplianceReportQueueSchema.Constants.TableName}
WHERE {AccTransactionComplianceReportQueueSchema.Constants.ACQ_GC_Company}    = @CompanyPK
AND {AccTransactionComplianceReportQueueSchema.Constants.ACQ_ReportType}      IN ({reportCodesParameterInline})
AND {AccTransactionComplianceReportQueueSchema.Constants.ACQ_ParentTableCode} = '{AccTransactionHeaderSchema.Constants.Prefix}'
AND {AccTransactionComplianceReportQueueSchema.Constants.ACQ_ParentID}        = @ParentPK
AND {AccTransactionComplianceReportQueueSchema.Constants.ACQ_Date}            IN (@CurrentAHPostDate, @OriginalAHPostDate)

DELETE {AccTransactionComplianceReportQueueSchema.Constants.SqlSchemaName}.{AccTransactionComplianceReportQueueSchema.Constants.TableName}
WHERE {AccTransactionComplianceReportQueueSchema.Constants.ACQ_GC_Company}    = @CompanyPK
AND {AccTransactionComplianceReportQueueSchema.Constants.ACQ_ReportType}      IN ({reportCodesParameterInline})
AND {AccTransactionComplianceReportQueueSchema.Constants.ACQ_ParentTableCode} = '{AccTransactionLinesSchema.Constants.Prefix}'
AND {AccTransactionComplianceReportQueueSchema.Constants.ACQ_ParentID}        IN ({subqueryForAL})
AND {AccTransactionComplianceReportQueueSchema.Constants.ACQ_Date}            BETWEEN @ALPostDateFrom AND @ALPostDateTo
");

			var command = ((IDbConnected)factory).Connection.Command(sql);  // No BizO generated for this table. There is no sense to generate business objects for queue entry and pivot as they are not bound to GUI
			command.AddParameterBasedOnDbColumn("@CompanyPK", AH_GC.ToGuid(), AccTransactionComplianceReportQueueSchema.ACQ_GC_Company);
			command.AddParameterBasedOnDbColumn("@ParentPK", PK.ToGuid(), AccTransactionComplianceReportQueueSchema.ACQ_ParentID);
			command.AddParameterBasedOnDbColumn("@CurrentAHPostDate", AH_PostDate.Date.ToDateTime(), AccTransactionComplianceReportQueueSchema.ACQ_Date);
			command.AddParameterBasedOnDbColumn("@OriginalAHPostDate", ((ZDateTime)AH_PostDateInfo.OriginalValue).Date.ToDateTime(), AccTransactionComplianceReportQueueSchema.ACQ_Date);
			command.ExecuteNonQuery();
		}

		#endregion

		#region CanReprint

		public (ZBool Result, ZString ReasonForNotBeingAbleToPrint) CheckCanPrintPostedInvoicingBase()
		{
			var reason = string.Empty;
			var canPrint = !IsInDatabase || AccountingCountrySpecificValidationHelper.CanReprint(this) || HasNoPrintedInvoices;
			if (!canPrint)
			{
				reason = AccountingConstants.ReprintingInvoiceMessage;
			}
			return (canPrint, reason);
		}

		#endregion

		#region Create JobDocAddress

		void CreateJobDocAddress()
		{
			if (!IsInDatabase && AccountingUtils.ShouldPortugalStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting(AH_Ledger, AH_TransactionType))
			{
				var docARInvoiceDataProvider = ObjectFactory.Get<IDocARInvoiceDataProvider>();
				CreateBranchOrCompanyProxyJobDocAddress(docARInvoiceDataProvider);
				CreateDebtorJobDocAddress(docARInvoiceDataProvider);
			}
		}

		void CreateBranchOrCompanyProxyJobDocAddress(IDocARInvoiceDataProvider docARInvoiceDataProvider)
		{
			var branchProxyDocAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.BranchOrCompanyProxyARAdress);
			var taxId = docARInvoiceDataProvider.GetTaxId(this, Factory);
			var printBranchAddress = false;
			if (Branch.PK.IsValid)
			{
				printBranchAddress = AccountingConfigurationRegistry.Instance.PrintBranchAddressInFooter.GetValueWithoutFallback(Guid.Empty, Branch.PK.ToGuid(), Guid.Empty);
			}
			OrgAddress addressForSendingARDocuments = null;
			if (printBranchAddress && Branch.OrgProxy != null)
			{
				addressForSendingARDocuments = Branch.OrgProxy.AddressForSendingARDocuments;
			}
			else if (Branch.Company != null && Branch.Company.OrgProxy != null)
			{
				addressForSendingARDocuments = Branch.Company.OrgProxy.AddressForSendingARDocuments;
			}

			SetJobDocAddress(branchProxyDocAddress, addressForSendingARDocuments, taxId);
		}

		void CreateDebtorJobDocAddress(IDocARInvoiceDataProvider docARInvoiceDataProvider)
		{
			var debtorDocAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.DebtorAddress);
			var recipientTaxIDNumber = docARInvoiceDataProvider.GetRecipientTaxIDNumber(this, Factory);
			var debtorAddress = Factory.Load<OrgAddress>(DisplayInvoiceAddressOverride);

			SetJobDocAddress(debtorDocAddress, debtorAddress, recipientTaxIDNumber, InvoiceContactOverride?.Name ?? ZString.Empty);
		}

		void SetJobDocAddress(JobDocAddress jobDocAddress, OrgAddress orgAddress, ZString govRegNum, string contact = "")
		{
			if (jobDocAddress != null && orgAddress != null)
			{
				jobDocAddress.E2_ValidationStatus = AddressValidationStatus.NotRequired;
				jobDocAddress.E2_OA_Address = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress;
				jobDocAddress.E2_AddressOverride = false;

				jobDocAddress.E2_GovRegNum = govRegNum;
				jobDocAddress.E2_Address1 = orgAddress.OA_Address1;
				jobDocAddress.E2_Address2 = orgAddress.OA_Address2;
				jobDocAddress.E2_City = orgAddress.OA_City;
				jobDocAddress.E2_State = orgAddress.OA_State;
				jobDocAddress.E2_Postcode = orgAddress.OA_PostCode;
				jobDocAddress.E2_CompanyName = orgAddress.CompanyName;
				jobDocAddress.E2_RN_NKCountryCode = orgAddress.OA_RN_NKCountryCode;
				jobDocAddress.E2_ParentID = PK;
				jobDocAddress.E2_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;

				if (!string.IsNullOrEmpty(contact))
				{
					jobDocAddress.E2_Contact = contact;
				}
			}
		}

		[ChildEditable(true)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					RegisterEditableChildObject(fDocAddresses);
				}

				return fDocAddresses;
			}
		}

		JobDocAddressDependentCollection fDocAddresses;

		#region IDocAddresses

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		Security.SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType) => null;

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType) => null;

		ZString IDocAddresses.HumanReadableName => ZString.Empty;

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => null;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				return new DocAddressType[]
				{
					DocAddressType.DebtorAddress,
					DocAddressType.BranchOrCompanyProxyARAdress,
				};
			}
		}

		#endregion

		#endregion

		void ValidateINVDocTypesIfAny()
		{
			if (!AccountingCountrySpecificValidationHelper.CanChangeINVDocTypeOnPostedTransaction(this))
			{
				var changedError = Res.GetString("a14f28d2-6c79-4a4e-8f0d-989db73b0063", "Updating the Document Type for system generated AR documents of document type INV is not allowed. Please close the form without saving to discard your changes.");
				var deletedError = Res.GetString("0a9580a6-c014-49a0-b479-e5957e9c724e", @"Deleting system generated AR documents of document type INV is not allowed. These documents must remain on file to allow re-printing if required.
To restore the document, tick on the ‘Show Deleted Documents’ option. Then right click on the deleted document and click the ‘Restore’ action.");

				var docManager = DocManagerInfo as InvoicingDocManagerInfo;
				docManager?.InvoiceDocStorageMain.ClearRowNotificationsContaining(changedError);
				docManager?.InvoiceDocStorageMain.ClearRowNotificationsContaining(deletedError);

				var invDocs = GetDeliveredINVDocIfAny(DocManagerInfo);
				if (invDocs.Any())
				{
					invDocs.ForEach(d =>
					{
						d.ClearRowNotificationsContaining(changedError);
						d.ClearRowNotificationsContaining(deletedError);
					});

					invDocs.Where(d => d.SC_DocTypeInfo.HasChanges)
						.ForEach(d => d.AddRowError(changedError));

					invDocs.Where(d => d.SC_IsDeleted)
						.ForEach(d => d.AddRowError(deletedError));
				}

				if (DeliveredInvoiceDocumentsInDatabase.Any())
				{
					var invDocPKs = invDocs.Select(id => id.PK).ToArray();
					var deletedEDoc = DeliveredInvoiceDocumentsInDatabase.FirstOrDefault(d => !invDocPKs.Contains(d.PK));
					if (deletedEDoc != null)
					{
						docManager?.InvoiceDocStorageMain.AddRowError(Res.GetString("e1b7a370-2b89-4d04-a20f-074a7062676e", @"One or more INV type document(s) have been changed in the eDocs tab.
Possible causes:
- Permanently deleting system generated AR documents of document type INV is not allowed. These documents must remain on file to allow re-printing if required.
- Another user has modified the INV type documents on another form while you were working on it.
Please close the form without saving to discard your changes."));
					}
				}
			}
		}

		bool HasNoPrintedInvoices => !AH_InvoicePrinted && !DeliveredInvoiceDocumentsInDatabase.Any();

		IEnumerable<StorageDocsBase> DeliveredInvoiceDocumentsInDatabase => !deliveredInvoiceDocumentsInDatabase.IsNullOrEmpty() ? deliveredInvoiceDocumentsInDatabase : (deliveredInvoiceDocumentsInDatabase = GetDeliveredINVDocIfAny(GetNewDocManagerInfo()));

		IEnumerable<StorageDocsBase> deliveredInvoiceDocumentsInDatabase;

		StorageDocsBase[] GetDeliveredINVDocIfAny(DocManagerInfo docManager) => docManager.AllEDocs.OfType<StorageDocsBase>()
			.Where(d => d.IsInDatabase && d.SC_IsSystemGenerated && (ZString)d.SC_DocTypeInfo.OriginalValue == Enterprise.Core.Constants.RefDocTypes.Invoice).ToArray();

#if DEBUG
		public ZDBOnlyQuery GetComplianceDocumentByTransactionQuery_ForTestOnly(ZGuid pk) => GetComplianceDocumentByTransactionQuery(pk);
#endif

		bool isComplianceDocumentRelatedPropertiesHooked;

		void UnhookComplianceDocumentRelatedProperties()
		{
			AH_OA_InvoiceAddressOverrideInfo.ValueChanged -= OnComplianceDocumentRelatedPropertiesChanged;
			AH_OHInfo.ValueChanged -= OnComplianceDocumentRelatedPropertiesChanged;
			AH_GCInfo.ValueChanged -= OnComplianceDocumentRelatedPropertiesChanged;
			AH_DescInfo.ValueChanged -= OnComplianceDocumentRelatedPropertiesChanged;
			AH_LedgerInfo.ValueChanged -= OnComplianceDocumentRelatedPropertiesChanged;
			AH_TransactionTypeInfo.ValueChanged -= OnComplianceDocumentRelatedPropertiesChanged;

			foreach (InvoicingLineBase line in Lines)
			{
				line.AL_DescInfo.ValueChanged -= OnComplianceDocumentRelatedPropertiesChanged;
				line.AL_ACInfo.ValueChanged -= OnComplianceDocumentRelatedPropertiesChanged;
				line.AL_AGInfo.ValueChanged -= OnComplianceDocumentRelatedPropertiesChanged;
			}

			isComplianceDocumentRelatedPropertiesHooked = false;
		}

		void HookComplianceDocumentRelatedPropertiesChanged()
		{
			AH_OA_InvoiceAddressOverrideInfo.ValueChanged += OnComplianceDocumentRelatedPropertiesChanged;
			AH_OHInfo.ValueChanged += OnComplianceDocumentRelatedPropertiesChanged;
			AH_GCInfo.ValueChanged += OnComplianceDocumentRelatedPropertiesChanged;
			AH_DescInfo.ValueChanged += OnComplianceDocumentRelatedPropertiesChanged;
			AH_LedgerInfo.ValueChanged += OnComplianceDocumentRelatedPropertiesChanged;
			AH_TransactionTypeInfo.ValueChanged += OnComplianceDocumentRelatedPropertiesChanged;

			foreach (InvoicingLineBase line in Lines)
			{
				line.AL_DescInfo.ValueChanged += OnComplianceDocumentRelatedPropertiesChanged;
				line.AL_ACInfo.ValueChanged += OnComplianceDocumentRelatedPropertiesChanged;
				line.AL_AGInfo.ValueChanged += OnComplianceDocumentRelatedPropertiesChanged;
			}

			isComplianceDocumentRelatedPropertiesHooked = true;
		}

		void OnComplianceDocumentRelatedPropertiesChanged(object sender, EventArgs e)
		{
			Factory.ClearCachedValue<HashSet<ZGuid>>("CreatedComplianceDocumentOfInvoicePKs");

			if (e is ValueChangedEventArgs valueChangedEventArgs)
			{
				var propertyInfo = valueChangedEventArgs.Info;
				ErrorReporter.ReportOnce($"ComplianceDocumentRelatedFieldChanged_{propertyInfo.Name}", $@"{propertyInfo.Name} changed from {valueChangedEventArgs.OldValue} to {valueChangedEventArgs.NewValue}.
Call stack:
{System.Environment.StackTrace}");
			}
			else
			{
				ErrorReporter.ReportOnce("ComplianceDocumentRelatedFieldChanged", "Property value changed. No details available.");
			}
		}
	}
}
