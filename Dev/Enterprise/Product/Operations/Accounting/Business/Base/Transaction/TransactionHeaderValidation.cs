using System;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public partial class TransactionHeaderValidation : AccTransactionHeaderValidation
	{
		public TransactionHeaderValidation(TransactionHeader parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		protected new TransactionHeader Parent;

		#region Helper Validators and Calculators

		public PeriodValidationProvider PeriodValidation
		{
			get { return periodValidation ?? (periodValidation = GetPeriodValidationProvider()); }
		}
		PeriodValidationProvider periodValidation;

		protected virtual PeriodValidationProvider GetPeriodValidationProvider()
		{
			return new PeriodValidationProvider(Parent.Factory);
		}

		AccountingPeriodCalculator fPeriodCalculator;
		public AccountingPeriodCalculator PeriodCalculator
		{
			get
			{
				if (fPeriodCalculator == null)
				{
					fPeriodCalculator = new AccountingPeriodCalculator(Parent.Factory);
				}
				return fPeriodCalculator;
			}
		}

		/// <summary>
		/// This function makes validation if the EnableGovernmentAllocatedNumberBehavior registry is enabled.
		/// </summary>
		protected void CheckAH_GovernmentAllocatedID_BasedOnRegistry(InvoicingBase invoicingBase)
		{
			var companyPK = Parent.AH_GC.IsValid ? Parent.AH_GC.ToGuid() : Guid.Empty;
			var validationEnabled = AccountingMasterFilesRegistry.Instance
				.EnableGovernmentAllocatedNumberBehavior
				.GetValueWithoutFallback(companyPK, branchPK: Guid.Empty, departmentPK: Guid.Empty);

			if (validationEnabled)
			{
				var eInvoicingEligibilityLiteTransaction = (IEInvoicingEligibilityLiteTransaction)invoicingBase;
				var validationInstanceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(eInvoicingEligibilityLiteTransaction.CountryCode)
					as IInstanceProvider<IGovernmentAllocatedIDValidationProvider>;

				var validation = validationInstanceProvider?.Get();
				var validationError = validation?.ValidateGovernmentAllocatedID(new GovernmentAllocatedIDValidationData(invoicingBase));

				if (validationError != null)
				{
					Parent.AH_GovernmentAllocatedIDInfo.AddError(validationError);
				}
			}
		}

		#endregion

		protected override void CheckAH_InvoiceAmount()
		{
			base.CheckAH_InvoiceAmount();

			if (!Parent.IsReverseTransaction)
			{
				var message = CriticalValidationHelpers.GetAmountGreaterThanMaximumAllowedAmountMessage(Parent.AH_GC, CriticalValidationHelpers.MaximumAmountLevel.Header, Parent.AH_InvoiceAmount);
				if (message != null)
				{
					Parent.AH_InvoiceAmountInfo.AddError(message);
				}
			}
		}

		protected override void CheckAH_GSTAmount()
		{
			base.CheckAH_GSTAmount();

			if (!Parent.IsReverseTransaction)
			{
				var message = CriticalValidationHelpers.GetAmountGreaterThanMaximumAllowedAmountMessage(Parent.AH_GC, CriticalValidationHelpers.MaximumAmountLevel.Header, Parent.AH_GSTAmount);
				if (message != null)
				{
					Parent.AH_GSTAmountInfo.AddError(message);
				}
			}
		}

		protected void CheckAH_DueDateMustAfterInvoiceDate()
		{
			if (Parent.AH_DueDate.IsValid && !Parent.AH_DueDate.IsEmpty
				&& Parent.AH_InvoiceDate.IsValid && !Parent.AH_InvoiceDate.IsEmpty)
			{
				if (Parent.AH_DueDate.Date < Parent.AH_InvoiceDate.Date)
				{
					Parent.AH_DueDateInfo.AddError(Res.GetString("110b1faa-49cc-4994-84b6-59d7757fb252", "Due date should be after or equal to Invoice Date"));
				}
			}
		}

		protected override void CheckAH_InvoiceDate()
		{
			base.CheckAH_InvoiceDate();
			var countryComplianceInfo = ObjectFactory.Get<ICountryComplianceFactoryIntegration>().GetICountryComplianceInfo(Parent.Company.GC_RN_NKCountryCode);
			var isSytemTimeValidationEligible = countryComplianceInfo?.GetIsTransactionSequencingRequired(Parent.AH_Ledger, Parent.AH_TransactionType) ?? false;

			if (isSytemTimeValidationEligible)
			{
				var query = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, Parent.AH_GC);
				query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, new[] { TransactionTypes.Invoice, TransactionTypes.CreditNote, TransactionTypes.Receipt });
				query.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				query.OrderBy = AccTransactionHeaderSchema.AH_SystemCreateTimeUtc.Name + " desc";
				var result = Parent.Factory.LoadTop1<AccTransactionHeader>(query);
				var previousTransactionSystemTime = result?.AH_SystemCreateTimeUtc ?? ZDateTime.MinSmallDateTimeValue;

				if (previousTransactionSystemTime > ZDateTime.UtcNow)
				{
					Parent.AH_InvoiceDateInfo.AddError(Res.GetString("987F26BC-43F9-4D02-9A7C-D4F2C2B4D55D", "System date must be later than previous document"));
				}
			}
		}

		#region CheckAH_PostDate

		protected override void CheckAH_PostDate()
		{
			base.CheckAH_PostDate();
			//Common validation
			MandatoryValidation.CheckEntered(Parent.AH_PostDateInfo);

			if (!(Parent.IsInDirectDebitBatchLineContext && !Parent.IsReversed))
			{
				PeriodValidation.CheckDateFallsIntoValidPeriod(Parent.AH_PostDateInfo);
				//Overriden in children
				CheckAH_PostDateNotInFuture();
				CheckAH_PostDateNotInPast();
			}
		}

		protected virtual void CheckAH_PostDateNotInFuture()
		{
			if (!Parent.AH_PostDateInfo.HasErrors())
			{
				if (Parent.AH_PostDate.Date > ZDateTime.Today && !Parent.AllowFuturePosting)
				{
					Parent.AH_PostDateInfo.AddError(FuturePostDateError);
				}
			}
		}

		protected virtual void CheckAH_PostDateNotInPast()
		{
			if (!Parent.AH_PostDateInfo.HasErrors())
			{
				if (Parent.AH_PostDate.Date < ZDateTime.Today)
				{
					if (!Parent.AllowBackPosting)
					{
						Parent.AH_PostDateInfo.AddError(PreviousPostDateError);
					}
					else
					{
						Parent.AH_PostDateInfo.AddWarning(PreviousPostDateWarning);
					}
				}
			}
		}

		#endregion

		#region CheckAH_DocumentReceivedDate

		protected override void CheckAH_DocumentReceivedDate()
		{
			base.CheckAH_DocumentReceivedDate();

			if (Parent.IsDocumentReceivedDateApplicable && AccountingMasterFilesRegistry.Instance.DocumentReceivedDateMustBeEntered.Value)
			{
				MandatoryValidation.CheckEntered(Parent.AH_DocumentReceivedDateInfo);
			}
		}

		#endregion

		#region CheckAH_ExchangeRate

		protected override void CheckAH_ExchangeRate()
		{
			base.CheckAH_ExchangeRate();
			MandatoryValidation.CheckNotNegative(Parent.AH_ExchangeRateInfo);
			MandatoryValidation.CheckEntered(Parent.AH_ExchangeRateInfo);
		}

		#endregion

		#region CheckAH_AG

		protected override void CheckAH_AG()
		{
			base.CheckAH_AG();

			if ((Parent.AH_Ledger == LedgerTypes.AccountsPayable || Parent.AH_Ledger == LedgerTypes.AccountsReceivable)
				&& (Parent.AH_TransactionType == TransactionTypes.Journal ||
				Parent.AH_TransactionType == TransactionTypes.ExchangeDifference ||
				Parent.AH_TransactionType == TransactionTypes.Overpayment ||
				Parent.AH_TransactionType == TransactionTypes.Discount))
			{
				CheckIsAH_AGEmpty();
				ListValidation.ErrorIfInvalidPK(Parent.AH_AGInfo);
			}

			if (Parent.GLHeader != null)
			{
				GLAccountHelper.CheckLocalAccountDescriptorHasMapping(Parent);
			}
		}

		protected virtual void CheckIsAH_AGEmpty() => MandatoryValidation.CheckEntered(Parent.AH_AGInfo);

		#endregion

		#region CheckAH_GB

		protected override void CheckAH_GB()
		{
			base.CheckAH_GB();
			ListValidation.ErrorIfInvalidPK(Parent.AH_GBInfo);
		}

		#endregion

		#region CheckAH_GE

		protected override void CheckAH_GE()
		{
			base.CheckAH_GE();
			ListValidation.ErrorIfInvalidPK(Parent.AH_GEInfo);
		}

		#endregion

		#region CheckAH_PlaceOfSupply

		protected override void CheckAH_PlaceOfSupply()
		{
			base.CheckAH_PlaceOfSupply();

			if (!Parent.AH_PlaceOfSupply.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.AH_PlaceOfSupplyInfo);
			}
			else if (!Parent.AH_PlaceOfSupplyType.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.AH_PlaceOfSupplyInfo);
			}
		}

		#endregion

		#region CheckAH_PlaceOfSupplyType

		protected override void CheckAH_PlaceOfSupplyType()
		{
			base.CheckAH_PlaceOfSupplyType();

			if (!Parent.AH_PlaceOfSupplyType.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.AH_PlaceOfSupplyTypeInfo);
			}
			else if (!Parent.AH_PlaceOfSupply.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.AH_PlaceOfSupplyTypeInfo);
			}
		}

		#endregion

		#region CheckAH_ReceiptType

		protected override void CheckAH_ReceiptType()
		{
			base.CheckAH_ReceiptType();
			CheckEPaymentReceiptType();
		}

		public static string GetCashAccountTypeErrorMessage(string columnInfoHumanReadableName) =>
			Res.GetString("459FC449-4A69-0E9C-4B56-CB586BCAAEB3", "For Cash Account, please select CSH - Cash {0}.", columnInfoHumanReadableName);

		protected virtual void CheckEPaymentReceiptType()
		{
			if (Parent.AH_ReceiptType == ReceiptTypes.EPayment)
			{
				Parent.AH_ReceiptTypeInfo.AddError(Res.GetString("37b112c5-0e47-42f0-8ffb-401fec083a2d", "To process E-Payments, please create a Payment or Payment Batch in the Payables Transactions module or a Payment Approval in the Payment Processing module."));
			}
		}

		#endregion

		#region CheckAH_AB

		protected override void CheckAH_AB()
		{
			base.CheckAH_AB();
			if (Parent.AH_Ledger != LedgerTypes.CashBook && Parent.BankAccount != null)
			{
				if (Parent.BankAccount.AB_AccountType == AccountTypeCodeDescriptionPairList.Codes.EPA && Parent.AH_ReceiptType != ReceiptTypes.EPayment)
				{
					Parent.AH_ABInfo.AddError(Res.GetString("26e38ea4-d284-4f33-885c-e473956ab25c", "This bank account is an E-Payment Account. Please set Payment Type to EPA - E-Payment."));
				}
				else if (Parent.BankAccount.AB_AccountType != AccountTypeCodeDescriptionPairList.Codes.EPA && Parent.AH_ReceiptType == ReceiptTypes.EPayment)
				{
					Parent.AH_ABInfo.AddError(Res.GetString("02794c08-f6b7-4bdf-8bf1-877b495c333e", "Bank Account is not an E-Payment Account."));
				}
			}
		}

		#endregion

		#region CheckAH_GB_TaxBranch

		protected override void CheckAH_GB_TaxBranch()
		{
			base.CheckAH_GB_TaxBranch();
			ListValidation.ErrorIfInvalidPK(Parent.AH_GB_TaxBranchInfo);
		}

		#endregion

		public void ValidateSourceReference()
		{
			ValidateCalculatedProperty(Parent.SourceReferenceInfo);
		}

		protected virtual void CheckSourceReference()
		{
			var value = Parent.SourceReference;

			if (Parent.IsSourceReferenceEnabled)
			{
				BusinessObject.CheckMaximumLength(Parent.SourceReferenceInfo, value);

				if (!Parent.SourceReferenceInfo.HasErrors()
					&& PortugalComplianceInfo.IsComplianceSubTypeCompatibleWithSourceReference(Parent.AH_ComplianceSubType)
					&& !new Regex($"^{PortugalComplianceInfo.GetSourceReferencePrefix(Parent.AH_ComplianceSubType)} .+$").IsMatch(value))
				{
					//Any changes to the Source Reference structure should be also applied to SAFT SalesInvoices.BuildReferencesXml() and DocARInvoice.OriginalReferenceComplianceNumber
					Parent.SourceReferenceInfo.AddError(Res.GetString("7c8825df-38fe-4115-9a0d-1e1880e0be85", "When compliance sub type is {0}, Source Reference must contain the prefix '{1}' plus a space and the Original Invoice Number.",
						Parent.AH_ComplianceSubType, PortugalComplianceInfo.GetSourceReferencePrefix(Parent.AH_ComplianceSubType)));
				}

				if (!Parent.SourceReferenceInfo.HasErrors() && !Parent.SourceReference.IsEmpty)
				{
					var duplicateSourceReference = Parent.GetTransactionNumWhereSourceReferenceIsAlreadyUsed();
					if (!string.IsNullOrEmpty(duplicateSourceReference))
					{
						Parent.SourceReferenceInfo.AddError(Res.GetString("56193c7a-f7e3-472d-aee7-0499abc95316",
							"The entered Source Reference value is already recorded against transaction {0}.", duplicateSourceReference));
					}
					else
					{
						var mutex = TransactionSourceReferenceMutexService.GetTransactionSourceReferenceMutexService(Parent.Factory).GetSourceReferenceMutex(Parent.Company.GC_Code, Parent.SourceReference);
						if (mutex.IsLocked && !mutex.HasLock)
						{
							Parent.SourceReferenceInfo.AddError(Res.GetString("f96b08af-bed9-4541-a138-e8b48b2277c9",
							"{0} is in the process of allocating the same source reference value. We cannot create duplicate source reference.", mutex.GetMutexLockByInfo()));
						}
						else if (!mutex.IsLocked)
						{
							mutex.Lock();
							Parent.Factory.AddDisposableServiceIfRequired();
							Parent.Factory.SubscribeForDispose(mutex);
						}
					}
				}
			}
		}

		#region Calculated Properties Validation

		#region PostPeriod

		public void ValidatePostPeriod()
		{
			ValidateCalculatedProperty(Parent.PostPeriodInfo);
		}

		protected virtual void CheckPostPeriod()
		{
		}

		#endregion

		#region AgePeriod

		public void ValidateAgePeriod()
		{
			ValidateCalculatedProperty(Parent.AgePeriodInfo);
		}

		protected virtual void CheckAgePeriod()
		{
		}

		#endregion

		#region AH_LocalExTaxAmount

		public void ValidateAH_LocalExTaxAmount()
		{
			ValidateCalculatedProperty(Parent.AH_LocalExTaxAmountInfo);
		}

		protected virtual void CheckAH_LocalExTaxAmount()
		{
		}

		#endregion

		#region AH_OSExTaxAmount

		public void ValidateAH_OSExTaxAmount()
		{
			ValidateCalculatedProperty(Parent.AH_OSExTaxAmountInfo);
		}

		protected virtual void CheckAH_OSExTaxAmount()
		{
		}

		#endregion

		#region BindableInvoiceAmount

		public void ValidateBindableInvoiceAmount()
		{
			ValidateCalculatedProperty(Parent.BindableInvoiceAmountInfo);
		}

		protected virtual void CheckBindableInvoiceAmount()
		{
		}

		#endregion

		#region AH_LocalTaxAmount
		protected virtual void CheckAH_LocalTaxAmount()
		{
		}
		public void ValidateAH_LocalTaxAmount()
		{
			ValidateCalculatedProperty(Parent.AH_LocalTaxAmountInfo);
		}

		#endregion

		#region AH_OSTaxAmount
		protected virtual void CheckAH_OSTaxAmount()
		{
		}
		public void ValidateAH_OSTaxAmount()
		{
			ValidateCalculatedProperty(Parent.AH_OSTaxAmountInfo);
		}

		#endregion

		#region AH_OSTotalAmount

		public void ValidateAH_OSTotalAmount()
		{
			ValidateCalculatedProperty(Parent.AH_OSTotalAmountInfo);
		}

		protected virtual void CheckAH_OSTotalAmount()
		{
		}

		#endregion

		#region AH_LocalTotalAmount

		public void ValidateAH_LocalTotalAmount()
		{
			ValidateCalculatedProperty(Parent.AH_LocalTotalAmountInfo);
		}

		protected virtual void CheckAH_LocalTotalAmount()
		{
		}

		#endregion

		public virtual void ValidateDisplayInvoiceAddressOverride()
		{
		}

		public virtual void ValidateDisplayInvoiceContactOverride()
		{
		}

		#region AH_AgreedPaymentMethodOverride

		protected override void CheckAH_AgreedPaymentMethodOverride()
		{
			ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("14b9b718-3d90-4835-86d6-4497f1e75b83", "The agreed payment method '{0}' is no longer valid, please review Organization > AR/AP > Agreed Payment Method value and corresponding system registry before posting.", Parent.AH_AgreedPaymentMethodOverride), Parent.AH_AgreedPaymentMethodOverrideInfo);
		}

		#endregion

		#region AH_OH

		protected override void CheckAH_OH()
		{
			base.CheckAH_OH();

			if (Parent.AH_OH.IsValid && !TransactionCreationRestrictionHelper.Instance.AllowToCreateTransaction(Parent, out ResourceString errorMessage))
			{
				Parent.AH_OHInfo.AddError(errorMessage);
			}
		}

		#endregion

		#endregion

		#region PreSaveValidation

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidatePostPeriod();
			ValidateAgePeriod();
			ValidateAH_LocalExTaxAmount();
			ValidateAH_OSExTaxAmount();
			ValidateAH_OSTaxAmount();
			ValidateAH_OSTotalAmount();

			ValidateMultipleReversing();

			ValidateUnmatchDate();
			ValidateDisplayInvoiceAddressOverride();
			ValidateDisplayInvoiceContactOverride();

			ValidateSourceReference();
		}

		#endregion

		protected override INotificationType NotificationTypeForBranchDepartmentCombination
		{
			get
			{
				return Parent.IsReverseTransaction ? CargoWise.EntityFramework.NotificationType.Warning : base.NotificationTypeForBranchDepartmentCombination;
			}
		}

		protected override bool ShouldValidateBranchDepartmentCombinationForParentInDatabase
		{
			get
			{
				var originalledger = (ZString)Parent.AH_LedgerInfo.OriginalValue;

				return originalledger == LedgerTypes.IncompleteTransactions
					|| originalledger == LedgerTypes.TransactionsPendingAllocation
					|| originalledger == LedgerTypes.UnapprovedPayableTransactions;
			}
		}

		protected void ValidateMultipleReversing()
		{
			if (Parent.MultipleReversingErrors != null)
			{
				foreach (string error in Parent.MultipleReversingErrors)
				{
					Parent.AddRowError(error);
				}
			}
		}

		public void ValidateUnmatchDate()
		{
			ValidateCalculatedProperty(Parent.UnmatchDateInfo);
		}

		protected void CheckUnmatchDate()
		{
			var parentAsUnmatchOnReversing = Parent as IUnmatchOnReversing;
			if (parentAsUnmatchOnReversing != null && !Parent.UnmatchDateInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.UnmatchDateInfo);

				TypeValidation.CheckValidSmallDateTime(Parent.UnmatchDateInfo);
				TypeValidation.CheckValidZDateTimeRange(Parent.UnmatchDateInfo);

				if (Parent.UnmatchDate.Date > ZDateTime.Today)
				{
					Parent.UnmatchDateInfo.AddError(Res.GetString("e74545aa-c6e0-46b6-aba9-66e0511d502b", "The date must be less or equal today's date."));
				}

				if (!Parent.UnmatchDateInfo.HasErrors())
				{
					if (Parent.UnmatchDate.Date < parentAsUnmatchOnReversing.UnmatchingData.MinUnmatchDate.Date)
					{
						Parent.UnmatchDateInfo.AddError(Res.GetString("7a0d8c57-faf9-4cc5-8ae5-bbd960257fac", "The date can't be less than match date {0}", parentAsUnmatchOnReversing.UnmatchingData.MinUnmatchDate.ToShortDateString()));
					}
				}

				PeriodValidation.CheckDateFallsIntoValidPeriod(Parent.UnmatchDateInfo);
			}
		}

		public void ValidateSupportingDocumentNumber()
		{
			ValidateCalculatedProperty(Parent.SupportingDocumentNumberInfo);
		}

		protected virtual void CheckSupportingDocumentNumber()
		{
		}

		#region Error Messages

		public ZString InvalidPeriodError
		{
			get
			{
				return Res.GetString("68353dfc-53a2-4137-a70c-8bcd1a955255", "This period is invalid. Please go to Period Management to setup periods");
			}
		}

		public ZString ClosedPeriodError
		{
			get
			{
				return Res.GetString("bd9ffafb-eeb8-450d-8d50-b7f59348fb7f", "This period is closed. You cannot post to closed periods");
			}
		}

		public ZString InsufficientRightsToPostToPreviousPeriodError
		{
			get
			{
				return Res.GetString("82dc6976-f2f9-46b3-b0ac-e3c1bcaf6331", "You do not have sufficient rights to post to previous periods");
			}
		}

		public ZString AgePeriodMustBeGreaterThanPostPeriodError
		{
			get
			{
				return Res.GetString("64efdeac-c49e-4b7d-805e-4ba4b2503f62", "Reverse/Ending Period must be greater than post period");
			}
		}

		public static ZString PreviousPostDateWarning
		{
			get
			{
				return Res.GetString("023abbbf-2a11-4de1-97c4-b1e75ef65ba4", "You are posting to a previous date. If this transaction is posted, there may be implications in the following subsystems \r\n - Financial Reports\r\n - Sub-Ledger Reports\r\n - Bank Reconciliation\r\n - Reversing");
			}
		}

		public static ZString FuturePostDateError
		{
			get
			{
				return Res.GetString("13ca73ac-3610-4fd1-8a0a-9fdc48eea2cc", "The post date cannot be in the future");
			}
		}

		public static ZString PreviousPostDateError
		{
			get
			{
				return Res.GetString("98c79877-ad95-4fb7-811d-5027fca51e72", "The post date cannot be in the past");
			}
		}

		protected ZBool ErrorExists(BusinessObject bizO, ZString rowError)
		{
			foreach (INotification error in bizO.RowErrors)
			{
				if (error.Message == rowError)
				{
					return true;
				}
			}
			return false;
		}

		protected ZBool MessageErrorExists(BusinessObject bizO, ZString rowMessageError)
		{
			foreach (INotification messageError in bizO.RowMessageErrors)
			{
				if (rowMessageError == messageError.Message)
				{
					return true;
				}
			}
			return false;
		}

		#endregion
	}
}
