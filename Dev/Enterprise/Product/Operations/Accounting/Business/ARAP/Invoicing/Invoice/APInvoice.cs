using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ComponentModel;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Accounting.ProcessLogging;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[PropertyDescriptorCollection(typeof(MatchingPropertyDescriptorCollection))]
	[CodeProperty(AccTransactionHeaderSchema.Constants.AH_ConsolidatedInvoiceRef)]
	[RestrictedFilteredItem()]
	public partial class APInvoice : Invoice, IChequeNumberAutoAllocation, IDocManagerSupport, IAmending, IEDocsParsingSupport, ISupportAccProcessLogging
	{
		public APInvoice(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (!AccountingConfigurationRegistry.Instance.CollectConstructorCallStackDetailsToReportInCriticalValidationErrors.Value)
			{
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoOnceWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.APInvoiceConstructor, () =>
				{
					return System.Environment.StackTrace;
				});
			}
		}

		public void SetInvoiceAsPaid(ZDateTime postingTime)
		{
			AH_InvoiceApproved = ZBool.True;
			AH_FullyPaidDate = postingTime;
			AH_OutstandingAmount = 0M;
		}

		[SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoAccTransactionHeader.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public APInvoice LoadTop1NotReversed(ZString apInvoiceNumberStartingWith, ZGuid creditorPK, ZGuid jobPK)
			{
				var result = LoadNotReversed(apInvoiceNumberStartingWith, creditorPK, jobPK);
				return result.Length > 0 ? result[0] : null;
			}

			/// <summary>
			/// Will return the first record matching the parameters which is not reversed
			/// </summary>
			public APInvoice[] LoadNotReversed(ZString apInvoiceNumberStartingWith, ZGuid creditorPK, ZGuid jobPK)
			{
				return LoadAPInvoice(apInvoiceNumberStartingWith, creditorPK, jobPK, false);
			}

			public APInvoice[] LoadIncludingReversed(ZString apInvoiceNumberStartingWith, ZGuid creditorPK, ZGuid jobPK)
			{
				return LoadAPInvoice(apInvoiceNumberStartingWith, creditorPK, jobPK, true);
			}

			APInvoice[] LoadAPInvoice(ZString apInvoiceNumberStartingWith, ZGuid creditorPK, ZGuid jobPK, bool shouldIncludeReverseOne)
			{
				ZQuery linesFilter = new ZQuery(AccTransactionLinesSchema.AL_JH, jobPK);
				linesFilter.AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK);
				AccTransactionLinesCollection collection = new AccTransactionLinesCollection(Factory, linesFilter);
				collection.Load();

				ZQuery filter = new ZQuery(new ZQuery(AccTransactionHeaderSchema.PK, collection.GetFieldValues(AccTransactionLinesSchema.AL_AH)));
				filter.AddToFilter(AccTransactionHeaderSchema.AH_OH, creditorPK);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, SQLComparisonOperator.StartsWith, apInvoiceNumberStartingWith);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				if (!shouldIncludeReverseOne)
				{
					filter.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);
				}

				return Factory.Load<APInvoice>(filter);
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(APInvoice);
			}
		}

		protected override bool AH_OH_ReadOnly
		{
			get { return base.AH_OH_ReadOnly || fAH_OH_Readonly; }
		}

		protected override bool IsEnforcePostingAtFixedPlaceOfSupplyLevelRegistryEnabled => AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.GetValueWithoutFallback(Company.PK.ToGuid(), Guid.Empty, Guid.Empty);

		public void SetAH_OH_Readonly(bool isReadonly)
		{
			fAH_OH_Readonly = isReadonly;
		}

		bool fAH_OH_Readonly;

		public void SetReceiptPaymentAH_ReceiptTypeReadOnly(bool isReadOnly)
		{
			fReceiptPaymentAH_ReceiptTypeReadOnly = isReadOnly;
		}

		public bool ReceiptPaymentAH_ReceiptType_ReadOnly
		{
			get { return fReceiptPaymentAH_ReceiptTypeReadOnly; }
		}

		bool fReceiptPaymentAH_ReceiptTypeReadOnly;

		public ZString MatchedWithTNFJournalNum
		{
			get
			{
				var result = ZString.Empty;
				if (AH_OH.IsValid && Header != null && !AH_RX_NKTransactionCurrency.IsEmpty && (!AH_TransactionNum.IsEmpty || !AH_ChequeOrReference.IsEmpty))
				{
					result = Factory.GetCachedValue(GetCachingKey(), GetMatchedWithTNFJournalNum);
				}
				return result;
			}
		}

		string GetCachingKey()
		{
			return "MatchedTNFJNL:" + Header.OH_Code + AH_TransactionNum + AH_ChequeOrReference + AH_RX_NKTransactionCurrency;
		}

		ZString GetMatchedWithTNFJournalNum()
		{
			var query = new ZQuery();

			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			query.AddToFilter(AccTransactionHeaderSchema.AH_OH, AH_OH);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, AH_GC);
			query.AddToFilter(AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, AH_RX_NKTransactionCurrency);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, Constants.TransactionCategory.Codes.TransactionNotFound);
			query.AddToFilter(AccTransactionHeaderSchema.AH_ChequeOrReference, SQLComparisonOperator.NotEqual, ZString.Empty);
			query.AddToFilter(AccTransactionHeaderSchema.AH_FullyPaidDate, SQLComparisonOperator.Equal, null);
			query.AddToFilter(AccTransactionHeaderSchema.AH_OSTotal, SQLComparisonOperator.GreaterThan, 0);

			var referenceFilter = new ZQuery(AccTransactionHeaderSchema.AH_ChequeOrReference, AH_TransactionNum);
			referenceFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_ChequeOrReference, AH_ChequeOrReference);
			query.AddToFilter(referenceFilter);

			var matchedJournal = Factory.LoadTop1<APJournal>(query);
			return matchedJournal != null ? matchedJournal.AH_TransactionNum : ZString.Empty;
		}

		#region Events

		public delegate void HotChequeSelectedHandler(object sender, HotChequeLink link);
		public event HotChequeSelectedHandler DisplayHotCheques;

		public delegate void PaymentFieldsUneditableHandler(object sender, string message);
		public event PaymentFieldsUneditableHandler NotifyUserPaymentUneditable;

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);

				if (RelatedDraftInvoice != null)
				{
					result.Add(RelatedDraftInvoice);
				}

				return result.ToArray();
			}
		}

		#endregion

		#region Overriden Properties

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("d5608150-bc11-4ddf-b66c-e8cad7e2558a", "Accounts Payable Invoice"); }
		}

		#region AH_OH
		protected override ZGuid AH_OHCore
		{
			get { return base.AH_OHCore; }
			set
			{
				var previousAH_OHCore = AH_OHCore;

				if (HotChequeIsAlreadyImported)
				{
					if (value != AH_OHCore)
					{
						FireNotifyUserPaymentUneditable(HotChequeErrorMessages.AH_OHError);
					}
					ReceiptPaymentAH_ReceiptTypeInfo.RefreshBinding();
				}
				else
				{
					base.AH_OHCore = value;
					if (ShouldDefaultCharge)
					{
						APInvoiceLine newLine = (APInvoiceLine)Lines.AddNew();
						newLine.GenericCharge = Header.CompanyData.OB_AC_APDefaultChargeCode;
						InvoicingLineBaseValidation validation = newLine.Validation as InvoicingLineBaseValidation;
						if (validation != null)
						{
							validation.ValidateGenericCharge();
						}
					}
				}
				AH_OSTaxAmountInfo.RefreshBinding();

				if (previousAH_OHCore != AH_OHCore)
				{
					UpdatePaymentAddress(AH_OHCore);
				}
			}
		}

		bool ShouldDefaultCharge
		{
			get
			{
				return Header != null && !Header.CompanyData.OB_AC_APDefaultChargeCode.IsEmpty && Lines.Count == 0
						&& AllowDefaultChargeCodeLineToBeAdded && !IsReversing;
			}
		}

		#endregion

		#region ReceiptPaymentAH_ReceiptType

		[List("PaymentMethods")]
		public override ZString ReceiptPaymentAH_ReceiptType
		{
			get { return base.ReceiptPaymentAH_ReceiptType; }
			set
			{
				if (HotChequeIsAlreadyImported)
				{
					if (value != ReceiptPaymentAH_ReceiptType)
					{
						FireNotifyUserPaymentUneditable(HotChequeErrorMessages.AH_ReceiptTypeError);
					}
					ReceiptPaymentAH_ReceiptTypeInfo.RefreshBinding();
				}
				else
				{
					base.ReceiptPaymentAH_ReceiptType = value;

					InvoiceValidation invoiceValidation = Validation as InvoiceValidation;
					if (!IsValidationSuspended && invoiceValidation != null)
					{
						invoiceValidation.ValidateReceiptPaymentAK_AB();
					}

					if (ReceiptPaymentAH_ReceiptType != ZArchitecture.Core.ReceiptTypes.Cheque)
					{
						ReceiptPaymentAK_AB = ZGuid.Empty;
					}
					ReceiptPaymentAK_ABInfo.RefreshBinding();
					CheckNumberIsAutoAllocated();
				}
			}
		}

		protected override bool ReceiptPaymentAK_AB_ReadOnly
		{
			get { return !ReceiptPaymentAH_ReceiptType.IsEmpty && ReceiptPaymentAH_ReceiptType != ZArchitecture.Core.ReceiptTypes.Cheque; }
		}

		#endregion

		#region ReceiptPaymentAH_AB

		public override ZGuid ReceiptPaymentAH_AB
		{
			get { return base.ReceiptPaymentAH_AB; }
			set
			{
				if (HotChequeIsAlreadyImported)
				{
					if (value != ReceiptPaymentAH_AB)
					{
						FireNotifyUserPaymentUneditable(HotChequeErrorMessages.AH_ABError);
					}
					ReceiptPaymentAH_ABInfo.RefreshBinding();
				}
				else
				{
					base.ReceiptPaymentAH_AB = value;
				}
			}
		}

		#endregion

		#region ReceiptPaymentAK_AB

		public override ZGuid ReceiptPaymentAK_AB
		{
			get { return base.ReceiptPaymentAK_AB; }
			set
			{
				if (HotChequeIsAlreadyImported)
				{
					if (value != ReceiptPaymentAK_AB)
					{
						FireNotifyUserPaymentUneditable(HotChequeErrorMessages.AK_ABError);
					}
					ReceiptPaymentAK_ABInfo.RefreshBinding();
				}
				else
				{
					base.ReceiptPaymentAK_AB = value;
					CheckNumberIsAutoAllocated();
				}
			}
		}

		#endregion

		#region ReceiptPaymentAH_ChequeOrReference

		public override ZString ReceiptPaymentAH_ChequeOrReference
		{
			get { return base.ReceiptPaymentAH_ChequeOrReference; }
			set
			{
				if (HotChequeIsAlreadyImported)
				{
					if (value != ReceiptPaymentAH_ChequeOrReference)
					{
						FireNotifyUserPaymentUneditable(HotChequeErrorMessages.AH_ChequeOrReferenceError);
					}
					ReceiptPaymentAH_ChequeOrReferenceInfo.RefreshBinding();
				}
				else
				{
					if (ReceiptPaymentAH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque)
					{
						value = AccValidationHelper.PadChequeDigitsWithLeadingZeros(ReceiptPaymentBankAccount, value);
					}
					base.ReceiptPaymentAH_ChequeOrReference = value;
				}
			}
		}

		#endregion

		#region ReceiptPaymentAH_OSTotalAmount

		public override ZDecimal ReceiptPaymentAH_OSTotalAmount
		{
			get { return base.ReceiptPaymentAH_OSTotalAmount; }
			set
			{
				base.ReceiptPaymentAH_OSTotalAmount = value;

				APInvoiceValidation apInvoiceValidation = Validation as APInvoiceValidation;
				if (apInvoiceValidation != null)
				{
					apInvoiceValidation.ValidateAH_OSTotalAmount();
				}
				AH_OSTotalAmountInfo.RefreshBinding();
			}
		}

		#endregion

		#region IsInvoiceReceiptPayment

		public override ZBool IsInvoiceReceiptPayment
		{
			get { return base.IsInvoiceReceiptPayment; }
			set
			{
				ZBool wasInvoiceReceiptPayment = IsInvoiceReceiptPayment;
				base.IsInvoiceReceiptPayment = value;

				if (IsInvoiceReceiptPayment && !wasInvoiceReceiptPayment && AH_OH.IsValid)
				{
					AccHotChequeCollection hotCheques = GetActiveHotCheques();
					if (hotCheques.Count > 0)
					{
						FireDisplayHotCheques(hotCheques);
					}
				}

				if (wasInvoiceReceiptPayment && !IsInvoiceReceiptPayment)
				{
					ClearImportedHotCheque();
				}
				IsInvoiceReceiptPaymentInfo.RefreshBinding();
			}
		}

		#endregion

		#region ReceiptPaymentAddressContact

		protected bool ReceiptPaymentAddressOverride_ReadOnly
		{
			get { return !AccountingConfigurationRegistry.Instance.EditPaymentAddress.Value; }
		}

		protected bool ReceiptPaymentContactOverride_ReadOnly
		{
			get { return ReceiptPaymentAddressOverride_ReadOnly; }
		}

		ZGuid receiptPaymentAddressOverride;
		public virtual ZGuid ReceiptPaymentAddressOverride
		{
			get { return receiptPaymentAddressOverride; }
			set
			{
				if (!value.IsValid)
				{
					receiptPaymentAddressOverride = GetDefaultPaymentAddress(Header);
				}
				else
				{
					if (receiptPaymentAddressOverride != value)
					{
						receiptPaymentAddressOverride = value;

						var apInvoiceValidation = Validation as APInvoiceValidation;
						if (apInvoiceValidation != null)
						{
							apInvoiceValidation.ValidateReceiptPaymentAddressOverride();
						}

						ReceiptPaymentAddressOverrideInfo.RefreshBinding();
					}
				}
			}
		}

		public virtual ZPropertyInfo ReceiptPaymentAddressOverrideInfo
		{
			get { return GetZPropertyInfo(nameof(ReceiptPaymentAddressOverride), "Payment Address"); }
		}

		ZGuid receiptPaymentContactOverride;
		public virtual ZGuid ReceiptPaymentContactOverride
		{
			get { return receiptPaymentContactOverride; }
			set
			{
				if (receiptPaymentContactOverride != value)
				{
					receiptPaymentContactOverride = value;

					var apInvoiceValidation = Validation as APInvoiceValidation;
					if (apInvoiceValidation != null)
					{
						apInvoiceValidation.ValidateReceiptPaymentContactOverride();
					}

					ReceiptPaymentContactOverrideInfo.RefreshBinding();
				}
			}
		}

		public virtual ZPropertyInfo ReceiptPaymentContactOverrideInfo
		{
			get { return GetZPropertyInfo(nameof(ReceiptPaymentContactOverride), "Payment Contact"); }
		}

		#endregion

		#endregion

		#region Overrides

		protected override bool ValidateExpectedInvoiceTotalSecurityIsAllowed
		{
			get
			{
				return Env.Security.AllowAPInvoiceChangeDefaultExpectedTotalValue.IsAllowed;
			}
		}

		protected override bool InvertSigns
		{
			get { return true; }
		}

		protected override ZString Ledger
		{
			get { return ZArchitecture.Core.LedgerTypes.AccountsPayable; }
		}

		public override Type DependentTransactionLineType
		{
			get { return typeof(APInvoiceLine); }
		}

		protected override DependentTransactionLineCollection GetDependentLinesCollection()
		{
			return new APInvoiceLineCollection(this);
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return null; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForInternalRef
		{
			get { return AccountingNumberFountainWrapperFactory.Instance.APInvoiceInternalRef; }
		}

		protected override ReceiptPaymentBase NewReceiptPayment
		{
			get
			{
				return (ReceiptPaymentBase)Factory.New(typeof(APPayment));
			}
		}

		protected override Type TypeOfReverseTransaction
		{
			get { return typeof(APCreditNote); }
		}

		protected override Type TypeOfTransaction
		{
			get { return typeof(APInvoice); }
		}

		internal protected override BranchLevelPostingConfigurationRegistryItem EnforceBranchLevelPostingRegistryItem
		{
			get { return AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting; }
		}

		#region Validation

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			if (Factory.HasContext(BusinessContext.SavingIncompleteTransaction))
			{
				return new IncompleteInvoicingBaseValidation(this);
			}

			return new APInvoiceValidation(this);
		}

		#endregion

		protected override void JobRelatedLogicOnCopiedLine(InvoiceLine oldLine, InvoiceLine newLine)
		{
			newLine.AL_JH = oldLine.AL_JH;
		}

		#region Cash Invoice staff

		protected override ReceiptPaymentBase CreateReceiptPayment()
		{
			ReceiptPaymentBase payment = base.CreateReceiptPayment();
			ZGuid groupGuid = ZGuid.NewZGuid();
			AH_TransactionBelongsToGroup = groupGuid;
			AH_TransactionCount = 1;

			payment.AH_TransactionBelongsToGroup = groupGuid;
			payment.AH_TransactionCount = 2;
			payment.AH_OA_InvoiceAddressOverride = this.ReceiptPaymentAddressOverride;
			payment.AH_OC_InvoiceContactOverride = this.ReceiptPaymentContactOverride;

			return payment;
		}

		public APPayment PaymentForThisCashInvoice
		{
			get
			{
				APPayment payment = null;
				if (AH_TransactionBelongsToGroup.IsValid && !AH_IsCancelled)
				{
					ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, AH_TransactionBelongsToGroup);
					filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Payment);
					filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsPayable);
					filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCount, (byte)2);
					filter.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);
					filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, AH_GC);
					payment = Factory.LoadTop1<APPayment>(filter);
				}
				return payment;
			}
		}

		#endregion

		protected override void UnmatchCore(ZDecimal matchLinkAmount, ZDecimal matchLinkOSAmount)
		{
			base.UnmatchCore(matchLinkAmount, matchLinkOSAmount);
			APPayment payment = PaymentForThisCashInvoice;
			if (payment != null)
			{
				payment.AH_TransactionBelongsToGroup = ZGuid.Empty;
				AH_TransactionBelongsToGroup = ZGuid.Empty;
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			CostVarianceApprovalHelper.ClearLineAuthorisationCache();
			ClearLineChargeAmountSumCache();

			CollectIncompleteInvoiceInfo();

			base.OnFactorySavingBeforeTransactionCore();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				if (!Factory.HasContext(BusinessContext.IncompleteInvoiceSaving))
				{
					ClearApportionmentJobMutexes();
				}

				IncrementChequeCurrentNumber();
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Developer message only. And use string interpolation.")]
		void CollectIncompleteInvoiceInfo()
		{
			if ((ZString)AH_LedgerInfo.OriginalValue == LedgerTypes.IncompleteTransactions &&
				(ZString)AH_LedgerInfo.Value == LedgerTypes.AccountsPayable &&
				AH_TransactionType == TransactionTypes.Invoice)
			{
				var headerQuery = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable) { FetchOnlyFromLocalCache = true }.
						   AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
				var newInvoices = Factory.Load<AccTransactionHeader>(headerQuery).Where(x => !x.IsInDatabase);

				if (newInvoices.Any())
				{
					var messageBody = new StringBuilder().AppendLine((NoResString)"Incomplete AP Invoice to post:").AppendLine(this.GetTransactionHeaderInfo()).
														  AppendLine((NoResString)"> Line Count: " + Lines.Count).
														  AppendLine((NoResString)"New AP Invoice:").ToString();

					foreach (var invoice in newInvoices)
					{
						CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(invoice.PK,
							CriticalValidationInfoCollectorServiceKeyType.INTransactionWithoutLines, (() =>
							{
								var lineQuery = new ZQuery(AccTransactionLinesSchema.AL_AH, invoice.PK) { FetchOnlyFromLocalCache = true };
								var linesCount = Factory.Load<AccTransactionLines>(lineQuery).Length;
								return new StringBuilder(messageBody).
									AppendLine(invoice.GetTransactionHeaderInfo()).
									AppendLine($"> Line Count: {linesCount}").ToString();
							}));
					}
				}
			}
		}

		void IncrementChequeCurrentNumber()
		{
			AccChequeBook chequeBookCached = Factory.Load<AccChequeBook>(ReceiptPaymentAK_AB);
			if (chequeBookCached != null && !chequeBookCached.IsAutoPrint && ZDecimal.CanParseAsInteger(ReceiptPaymentAH_ChequeOrReference))
			{
				if (ZDecimal.Parse(ReceiptPaymentAH_ChequeOrReference) >= chequeBookCached.AK_CurrentNo
					&& (ZDecimal.Parse(ReceiptPaymentAH_ChequeOrReference) + 1) <= chequeBookCached.AK_LastNo)
				{
					AccChequeBook.UpdateCurrentNumber(chequeBookCached.PK, ZDecimal.Parse(ReceiptPaymentAH_ChequeOrReference) + 1);
				}
			}
		}

		public override void ReleaseAllMutexOnInvoice()
		{
			base.ReleaseAllMutexOnInvoice();
			ClearApportionmentJobMutexes();
		}

		protected override void OnSavingCore()
		{
			base.OnSavingCore();

			if (IsHotChequeImported && ReceiptPayment != null)
			{
				fImportedHotCheque.AQ_AH = ReceiptPayment.PK;
			}

			LogPaymentRequestedDateOrPaymentCriticalityChanged();
			AddInvoiceApprovalLog();
		}

		protected override ZString InvoiceApprovedLogReference
		{
			get { return (NoResString)"AP|INV|Approved and Posted"; }
		}

		public override ZBool IsChequeNumberAutoAllocated
		{
			get
			{
				if (ChequeBook != null)
				{
					return ChequeBook.IsAutoPrint && IsCheque && !IsHotChequeImported;
				}
				else
				{
					return ZBool.False;
				}
			}
		}

		public override bool HasApprovalRequest
		{
			get
			{
				return TransactionRelatedApprovalRequest != null;
			}
		}

		#region Invoice Amount Levels

		protected override ZString AuthorisationLevelCore()
		{
			ZString authorisationLevel = Res.GetString("6129bb29-0fde-4b22-8b6f-eb843928d544", "Not Defined");
			AmountBasedMultiLevelAuthorisationRequirement authorisationSettings = AuthorisationRequired;
			if (authorisationSettings != null)
			{
				authorisationLevel = authorisationSettings.AuthorisationRequirement;
			}
			return authorisationLevel;
		}

		protected AmountBasedMultiLevelAuthorisationRequirement AuthorisationRequiredCoreBase()
		{
			return base.AuthorisationRequiredCore();
		}

		protected override AmountBasedMultiLevelAuthorisationRequirement AuthorisationRequiredCore()
		{
			return CostVarianceApprovalHelper.AuthorisationRequiredCore();
		}

		protected override AuthorizationModeAndSettingsRegistryItem AuthorizationModeAndSettingsRegistry => null;

		protected override AmountBasedAuthorisationRequirementCollection AuthorizationSettingsCollection => CostVarianceApprovalHelper.AuthorizationSettingsCollection;

		protected override AmountBasedMultiLevelAuthorisationRequirement AuthorisationRequiredCore(ZDecimal localAmount)
		{
			return CostVarianceApprovalHelper.AuthorisationRequiredCore(localAmount);
		}

		protected override SecurityCheckpoint RetrieveLevelApprovalCheckPoint(string levelCode)
		{
			switch (levelCode)
			{
				case AuthorisationCodes.FirstApprovalRequiredOnly:
					return Env.Security.CostVarianceApprovalLevel1;
				case AuthorisationCodes.SecondApprovalRequiredOnly:
					return Env.Security.CostVarianceApprovalLevel2;
				default:
					return null;
			}
		}

		internal void SetDefaultFinalFlags()
		{
			if (!CostVarianceApprovalHelper.AutoTickFinalFlag)
			{
				return;
			}

			CostVarianceApprovalHelper.CalculateAuthorisationByLines();

			var totalRequirement = CostVarianceApprovalHelper.GetTotalAuthorisationRequirement();
			var totalAppropvalLevel = totalRequirement != null ? totalRequirement.AuthorisationRequirement.ToString() : AuthorisationCodes.NoApprovalRequired;
			var untickAll = CostVarianceApprovalHelper.MonitorTotalInvoiceVariance && (totalAppropvalLevel != AuthorisationCodes.NoApprovalRequired);

			foreach (APInvoiceLine line in Lines)
			{
				if (line.IsValidLineForCalculatingCostVarianceApproval)
				{
					if (untickAll)
					{
						line.AL_IsFinalChargeDefault = false;
						if (line.IsPopulatedFromImportedApportionment)
						{
							line.ApportionmentChargeImportedFrom.IsFinalDefault = false;
						}
					}
					else
					{
						var requirement = CostVarianceApprovalHelper.GetLineAuthorisationRequirement(line);
						var appropvalLevel = requirement != null ? requirement.AuthorisationRequirement.ToString() : AuthorisationCodes.NoApprovalRequired;

						line.AL_IsFinalChargeDefault = (appropvalLevel == AuthorisationCodes.NoApprovalRequired);

						if (line.IsPopulatedFromImportedApportionment)
						{
							line.ApportionmentChargeImportedFrom.IsFinalDefault = line.AL_IsFinalChargeDefault;
						}
					}
				}
			}
		}

		internal CostVarianceApprovalHelper CostVarianceApprovalHelper
		{
			get
			{
				return costVarianceApprovalHelper ?? (costVarianceApprovalHelper = new CostVarianceApprovalHelper(this));
			}
		}
		CostVarianceApprovalHelper costVarianceApprovalHelper;

#if DEBUG
		public void ClearCachedCostVarianceApproval_ForTestOnly()
		{
			CostVarianceApprovalHelper.ClearCachedCostVarianceApproval_ForTestOnly();
		}
#endif

		#region Line Authorisation

		public enum CostVarianceAuthorisationRequiredType
		{
			NoAuthorisationRequired,
			HasAuthorisationRights,
			AuthorisationRequired
		}

		public CostVarianceAuthorisationRequiredType GetLineCostVarianceAuthorisationRequired(APInvoiceLine line)
		{
			var result = CostVarianceAuthorisationRequiredType.NoAuthorisationRequired;
			var authorisationRequirement = CostVarianceApprovalHelper.GetLineAuthorisationRequirement(line);
			if (GetLevelAuthorizationRequired(authorisationRequirement))
			{
				result = CostVarianceAuthorisationRequiredType.AuthorisationRequired;
			}
			else if (Level1AuthorisationRequired(authorisationRequirement) || Level2AuthorisationRequired(authorisationRequirement))
			{
				result = CostVarianceAuthorisationRequiredType.HasAuthorisationRights;
			}
			return result;
		}

		public CostVarianceAuthorisationRequiredType TotalCostVarianceAuthorisationRequired
		{
			get
			{
				var result = CostVarianceAuthorisationRequiredType.NoAuthorisationRequired;
				var totalAuthorisationRequirement = CostVarianceApprovalHelper.GetTotalAuthorisationRequirement();

				if (GetLevelAuthorizationRequired(totalAuthorisationRequirement))
				{
					result = CostVarianceAuthorisationRequiredType.AuthorisationRequired;
				}
				else if (Level1AuthorisationRequired(totalAuthorisationRequirement) || Level2AuthorisationRequired(totalAuthorisationRequirement))
				{
					result = CostVarianceAuthorisationRequiredType.HasAuthorisationRights;
				}
				return result;
			}
		}

		public FunctionalitySuspender ClearLineAuthorisationCacheSuspender
		{
			get { return clearLineAuthorisationCacheSuspender ?? (clearLineAuthorisationCacheSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender clearLineAuthorisationCacheSuspender;

#if DEBUG
		internal int CalculateAuthorisationByLinesDBHitCount_ForTestOnly;

		internal void RefreshLineUnpostedCosts_ForTestOnly(HashSet<CostVarianceApprovalHelper.CostVarianceKey> keysToGetFromDB)
		{
			CostVarianceApprovalHelper.RefreshLineUnpostedCosts(keysToGetFromDB);
		}
#endif

		List<APInvoiceLine> GetLinesOutsideCompanyAuthorizationLevelBoundary(ZString maxARCompanyApprovalLevel)
		{
			var resultList = new List<APInvoiceLine>(Lines.Count);
			var lineAuthorisationRequirement = CostVarianceApprovalHelper.GetAuthorisationRequirements();
			var lineRequirementsOutsideMaxLevelBoundary = new Dictionary<CostVarianceApprovalHelper.CostVarianceKey, CostVarianceApprovalAuthorisationRequirement>(lineAuthorisationRequirement.Count);
			foreach (var currentRequirement in lineAuthorisationRequirement)
			{
				if (CompareLevelValues(currentRequirement.Value, maxARCompanyApprovalLevel) > 0)
				{
					lineRequirementsOutsideMaxLevelBoundary.Add(currentRequirement.Key, currentRequirement.Value);
				}
			}
			foreach (APInvoiceLine line in Lines)
			{
				if (CostVarianceApprovalHelper.GetLineAuthorisationObject(line, lineRequirementsOutsideMaxLevelBoundary) != null)
				{
					resultList.Add(line);
				}
			}
			return resultList;
		}

		static int CompareLevelValues(CostVarianceApprovalAuthorisationRequirement approvalRequirement, ZString maxLevel)
		{
			return Math.Sign(approvalRequirement.GetAuthorisationRequirementWeight(approvalRequirement.AuthorisationRequirement) - approvalRequirement.GetAuthorisationRequirementWeight(maxLevel));
		}

		#endregion

		#endregion

		#region Claim Creation

		string ApprovingWithClaim_MaxLevel;

		public override bool InitialiseApprovingWithClaim(InvoicingBase invoice)
		{
			ARInvoice arInvoice = invoice as ARInvoice;
			if (arInvoice == null)
			{
				return false;
			}

			ApprovingWithClaim_MaxLevel = arInvoice.MaxAuthorisationLevel;
			return true;
		}

		public override bool WasApprovingWithClaimInitialized
		{
			get
			{
				return ApprovingWithClaim_MaxLevel != null;
			}
		}

		bool CanApproveWithDefaultLines
		{
			get
			{
				CostVarianceApprovalAuthorisationRequirement authorisationRequirement = AuthorisationRequired as CostVarianceApprovalAuthorisationRequirement;
				return authorisationRequirement != null && authorisationRequirement.GetAuthorisationRequirementWeight(ApprovingWithClaim_MaxLevel) >= 0 &&
					CompareLevelValues(authorisationRequirement, ApprovingWithClaim_MaxLevel) > 0;
			}
		}

		public override AccQueryClaimBase CreateClaim(bool withDefaultLines)
		{
			APAccQueryClaim claim = null;
			var defaultLines = new List<APInvoiceLine>();
			if (!string.IsNullOrEmpty(ApprovingWithClaim_MaxLevel) && CanApproveWithDefaultLines) //this also recalculates authorisation levels for last data state.
			{
				defaultLines.AddRange(GetLinesOutsideCompanyAuthorizationLevelBoundary(ApprovingWithClaim_MaxLevel));
			}
			if (!withDefaultLines || defaultLines.Count > 0)
			{
				claim = Factory.New<APAccQueryClaim>();
				claim.AY_OH_Debtor = AH_OH;
				claim.AY_AH = PK;

				if (claim != null && withDefaultLines && claim.CheckIsRelatedCreditNoteAllowed().IsAllowed)
				{
					UACreditNote creditNote = claim.CreateAndAttachRelatedCreditNote();
					claim.AY_ShortDescriptionOfClaim = creditNote.AH_Desc;

					var linePostedUnpostedDifference = CostVarianceApprovalHelper.GetPostedUnpostedDifferences();
					foreach (APInvoiceLine line in defaultLines)
					{
						var localCostDifference = CostVarianceApprovalHelper.GetLineAuthorisationObject(line, linePostedUnpostedDifference);
						UACreditNoteLine uaLine = Factory.New<UACreditNoteLine>();
						using (uaLine.GetValidationSuspender())
						{
							creditNote.Lines.Add(uaLine);
							uaLine.SetValues(line, convertAmoutSignsBetweenTransactionTypes: false);
							uaLine.AL_OSExTaxAmount = Env.CurrentCompany.ExchangeRate.LocalToForeign(localCostDifference, uaLine.AL_ExchangeRate, uaLine.AL_RX_NKTransactionCurrency);
							uaLine.AL_LocalExTaxAmount = localCostDifference;
						}
					}
					claim.AY_QueryClaimAmount = creditNote.AH_OSTotalAmount;
				}
			}
			return claim;
		}

		#endregion

		protected override void SetChequeDetails()
		{
			ReceiptPaymentAH_ChequeDrawer = ZString.Empty;
			ReceiptPaymentAH_DrawerBank = ZString.Empty;
			ReceiptPaymentAH_DrawerBranch = ZString.Empty;
		}

		#endregion

		#region Defaults

		protected override ZGuid DefaultBankAccount
		{
			get
			{
				ZGuid bankPK = ZGuid.Empty;
				if (Header != null && Header.CompanyData != null)
				{
					AccBankAccount bank = Factory.Load(typeof(AccBankAccount), Header.CompanyData.OB_AB_APDefaultBankAccount) as AccBankAccount;
					if (bank != null && bank.AB_GC == GlbCompany.CurrentCompany.PK)
					{
						bankPK = Header.CompanyData.OB_AB_APDefaultBankAccount;
					}
				}
				return bankPK;
			}
		}

		#endregion

		#region Hot Cheques

		void FireDisplayHotCheques(AccHotChequeCollection hotCheques)
		{
			if (DisplayHotCheques != null)
			{
				HotChequeLink link = new HotChequeLink(hotCheques);
				DisplayHotCheques(this, link);
			}
		}

		void FireNotifyUserPaymentUneditable(string message)
		{
			if (NotifyUserPaymentUneditable != null && !fIsImportingHotCheque)
			{
				NotifyUserPaymentUneditable(this, message);
			}
		}

		public AccHotCheque ImportedHotCheque
		{
			get { return fImportedHotCheque; }
		}

		AccHotCheque fImportedHotCheque;

		public bool HotChequeIsAlreadyImported
		{
			get { return IsHotChequeImported && !fIsImportingHotCheque; }
		}

		public bool IsHotChequeImported
		{
			get { return fImportedHotCheque != null; }
		}

		public AccHotChequeCollection GetActiveHotCheques()
		{
			ZQuery query = new ZQuery(AccHotChequeSchema.AQ_OH, AH_OH);
			query.AddToFilter(AccHotChequeSchema.AQ_Cancelled, ZBool.False);
			query.AddToFilter(AccHotChequeSchema.AQ_AH, null);

			AccHotChequeCollection allHotCheques = new AccHotChequeCollection(Factory, query);
			allHotCheques.Load();

			AccHotChequeCollection hotChequesToReturn = new AccHotChequeCollection(Factory);
			foreach (AccHotCheque hotCheque in allHotCheques)
			{
				if (!HotChequeIsAlreadyUsedOnAnotherAPPayment(hotCheque))
				{
					hotChequesToReturn.Add(hotCheque);
				}
			}

			return hotChequesToReturn;
		}

		public void ImportSelectedHotCheque(AccHotCheque hotCheque)
		{
			if (hotCheque != null)
			{
				SetHotChequeInactiveWhenPosting(hotCheque);
				BeginImportingHotCheque();
				PopulateFieldsUsingHotCheque(hotCheque);
				FinishImportingHotCheque();
			}
		}

		void SetHotChequeInactiveWhenPosting(AccHotCheque hotCheque)
		{
			fImportedHotCheque = hotCheque;
		}

		void BeginImportingHotCheque()
		{
			fIsImportingHotCheque = true;
		}

		void PopulateFieldsUsingHotCheque(AccHotCheque hotCheque)
		{
			ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cheque;
			ReceiptPaymentAH_AB = (hotCheque.ChequeBook != null && hotCheque.ChequeBook.BankAccount != null) ? hotCheque.ChequeBook.BankAccount.PK : ZGuid.Empty;
			ReceiptPaymentAK_AB = hotCheque.AQ_AK;
			ExchangeRate.Currency = hotCheque.AQ_Calc_RX_NK;
			ReceiptPaymentAH_ChequeOrReference = hotCheque.AQ_ChequeNumber;
			ReceiptPaymentAH_OSTotalAmount = hotCheque.AQ_Amount;
		}

		void FinishImportingHotCheque()
		{
			fIsImportingHotCheque = false;
		}

		bool HotChequeIsAlreadyUsedOnAnotherAPPayment(AccHotCheque hotCheque)
		{
			ZQuery query = new ZQuery(AccTransactionHeaderSchema.AH_GC, hotCheque.ChequeBook.BankAccount.AB_GC);
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, new string[] { LedgerTypes.CashBook, LedgerTypes.AccountsPayable, LedgerTypes.AccountsReceivable });
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, new string[] { TransactionTypes.Payment, TransactionTypes.DirectPayment });
			query.AddToFilter(AccTransactionHeaderSchema.AH_AB, hotCheque.ChequeBook.AK_AB);
			query.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptType, ReceiptTypes.Cheque);
			query.AddToFilter(AccTransactionHeaderSchema.AH_ChequeOrReference, hotCheque.AQ_ChequeNumber);
			query.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, PK);

			APPayment otherAPPaymentUsingThisHotCheque = Factory.LoadTop1<APPayment>(query);
			return otherAPPaymentUsingThisHotCheque != null;
		}

		bool fIsImportingHotCheque;

		public static class HotChequeErrorMessages
		{
			public static string AH_OHError
			{
				get { return Res.GetString("506fe90c-e976-4c99-b601-2cfc3694ef8a", "Entered creditor must be the same as the creditor on the imported hot check.\r\nPlease click on the 'Clear Hot Check Details' in order to modify this field."); }
			}
			public static string AH_ReceiptTypeError
			{
				get { return Res.GetString("b0377370-763a-4a86-95e2-2fbc433fae47", "This payment must be a check payment because a hot check was imported.\r\nPlease click on the 'Clear Hot Check Details' in order to modify this field."); }
			}
			public static string AH_ABError
			{
				get { return Res.GetString("08ccccbd-d8c6-434f-a391-cd2d4635386b", "Entered bank must be the same as the bank on the imported hot check.\r\nPlease click on the 'Clear Hot Check Details' in order to modify this field."); }
			}
			public static string AK_ABError
			{
				get { return Res.GetString("969bde76-0fea-4e45-a7ad-4d05ebda1734", "Entered check book must be the same as the check book on the imported hot check.\r\nPlease click on the 'Clear Hot Check Details' in order to modify this field."); }
			}
			public static string AH_ChequeOrReferenceError
			{
				get { return Res.GetString("adcb6756-ffe3-488c-823e-8f6c3b46ff9b", "Entered check number must be the same as the reference number on the imported hot check.\r\nPlease click on the 'Clear Hot Check Details' in order to modify this field."); }
			}
			public static string AH_ActualAmountError
			{
				get { return Res.GetString("ffc909f7-1663-4627-90de-a545688e4eb5", "Entered amount must be the same as the amount used on the imported hot check."); }
			}

			public static string GetMaximumError(decimal maximumAmount)
			{
				return Res.GetString("a6b71ff5-2cfa-48c6-b1d3-554a1e10938d", "Entered amount must be less than or equal to the amount used on the imported hot check (") + Utilities.Round(maximumAmount, 2) + ")";
			}
		}

		public void ClearImportedHotCheque()
		{
			if (fImportedHotCheque != null)
			{
				fImportedHotCheque = null;
				ReceiptPaymentAK_AB = ZGuid.Empty;
				ReceiptPaymentAH_ChequeOrReference = ZString.Empty;
				ReceiptPaymentAH_OSTotalAmount = ZDecimal.Zero;
			}
		}

		#endregion

		#region Lookups

		#region Creditors

		public CreditorCollection Creditors
		{
			get { return fCreditors ?? (fCreditors = new CreditorCollection(Factory)); }
		}
		CreditorCollection fCreditors;

		#endregion

		#region Currencies

		public RefCurrencyCollection Currencies
		{
			get { return fCurrencies ?? (fCurrencies = new RefCurrencyCollection(Factory)); }
		}
		RefCurrencyCollection fCurrencies;

		#endregion

		#region TaxRates

		public AccTaxRateCollection TaxRates
		{
			get
			{
				if (fTaxRates == null)
				{
					ZQuery taxRatesFilter = new ZQuery(AccTaxRateSchema.AT_IsActive, true);
					fTaxRates = new AccTaxRateCollection(Factory, taxRatesFilter);
				}
				return fTaxRates;
			}
		}
		AccTaxRateCollection fTaxRates;

		#endregion

		#region PaymentOrganisationAddressesContacts

		public OrgAddressCollection PaymentOrganisationAddresses
		{
			get { return paymentOrganisationAddresses ?? (paymentOrganisationAddresses = new OrgAddressCollection(Factory)); }
		}
		OrgAddressCollection paymentOrganisationAddresses;

		public OrgContactCollection PaymentOrganisationContacts
		{
			get { return paymentOrganisationContacts ?? (paymentOrganisationContacts = new OrgContactCollection(Factory)); }
		}
		OrgContactCollection paymentOrganisationContacts;

		#endregion

		#endregion

		#region Other Properties and Methods

		public bool EnettAllowMultiCurrencyPaymentPropertyValue
		{
			get
			{
				bool result = false;

				if (EDIMessages.Count == 0)
				{
					EDIMessages.Load();
				}

				EDIMessage message = EDIMessages.GetLastMessage(EDIMessage.ApplicationCodes.eNett, "ENE", EDIMessage.Direction.Receive, EDIMessage.Status.Received, eNettMessageSubTypeList.Codes.GetNewInvoices);

				if (message != null)
				{
					Match match = Regex.Match(message.EM_MessageText, "(?:<AllowMultiCurrencyPayment>)(.*)(?:</AllowMultiCurrencyPayment>)");
					if (match.Success)
					{
						result = bool.Parse(match.Groups[1].ToString());
					}
				}

				return result;
			}
		}

		public ZString Calc_ChequeNumberIsAutoAllocatedLabel
		{
			get { return IsChequeNumberAutoAllocated && ReceiptPaymentAH_ChequeOrReference.IsEmpty ? AccountingConstants.ChequeLabelConstants.ChequeNumberIsAutoAllocatedLabel : ""; }
		}

		public ZPropertyInfo Calc_ChequeNumberIsAutoAllocatedLabelInfo
		{
			get { return GetZPropertyInfo(nameof(Calc_ChequeNumberIsAutoAllocatedLabel)); }
		}

		public ZString Calc_ChequeIsAutoPrintedLabel
		{
			get { return IsChequeNumberAutoAllocated && ReceiptPaymentAH_ChequeOrReference.IsEmpty ? AccountingConstants.ChequeLabelConstants.ChequeAutoPrintedLabel : ""; }
		}

		public ZPropertyInfo Calc_ChequeIsAutoPrintedLabelInfo
		{
			get { return GetZPropertyInfo(nameof(Calc_ChequeIsAutoPrintedLabel)); }
		}

		protected ZBool IsCheque
		{
			get
			{
				return ReceiptPaymentAH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque;
			}
		}

		void CheckNumberIsAutoAllocated()
		{
			Calc_ChequeIsAutoPrintedLabelInfo.RefreshBinding();
			Calc_ChequeNumberIsAutoAllocatedLabelInfo.RefreshBinding();
			if (IsChequeNumberAutoAllocated)
			{
				ReceiptPaymentAH_ChequeOrReference = ZString.Empty;
			}
			ReceiptPaymentAH_ChequeOrReferenceInfo.RefreshBinding();
		}

		protected bool ReceiptPaymentAH_ChequeOrReference_ReadOnly
		{
			get { return IsChequeNumberAutoAllocated; }
		}

		protected override bool AH_ChequeOrReference_ReadOnly
		{
			get
			{
				return Factory.HasContext(BusinessContext.PayableOrder) || base.AH_ChequeOrReference_ReadOnly;
			}
			set
			{
				base.AH_ChequeOrReference_ReadOnly = value;
			}
		}

		protected override bool AH_TransactionNum_ReadOnly
		{
			get
			{
				return Factory.HasContext(BusinessContext.PayableOrder) || base.AH_TransactionNum_ReadOnly;
			}
		}

		#region PaymentDate

		public override ZDateTime AH_DueDate
		{
			get { return base.AH_DueDate; }
			set
			{
				base.AH_DueDate = value;
				AH_RequisitionDate = AH_DueDate;
			}
		}

		#endregion

		#region PaymentCriticality

		internal ZString OriginalRequisitionStatusForNewBizo;

		[List("PaymentCriticalityList")]
		public override ZString AH_RequisitionStatus
		{
			get
			{
				return base.AH_RequisitionStatus;
			}
			set
			{
				if (value != AH_RequisitionStatus)
				{
					base.AH_RequisitionStatus = value;
					Validation.ValidateAH_RequisitionStatus();
				}
			}
		}

		public ICodeDescriptionPairList PaymentCriticalityList
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.PaymentRequisitionStatuses.Value;
			}
		}

		#endregion

		void LogPaymentRequestedDateOrPaymentCriticalityChanged()
		{
			if (((ZDateTime)AH_RequisitionDateInfo.OriginalValue).Date != AH_RequisitionDate.Date || (ZString)AH_RequisitionStatusInfo.OriginalValue != AH_RequisitionStatus)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				var log = Logs.AddNew(Events.EditedARecord, ZString.Format("Requisition Date: Was={0} Now={1} Requisition Status: Was={2} Now={3}", ((ZDateTime)AH_RequisitionDateInfo.OriginalValue).ToShortDateString(), AH_RequisitionDate.ToShortDateString(), AH_RequisitionStatusInfo.OriginalValue, AH_RequisitionStatus));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				log.SL_GS_NKUser = GlbStaff.CurrentUser.GS_Code;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			base.AH_RequisitionDate = AH_DueDate.Date;
			base.AH_RequisitionStatus = AccountingConfigurationRegistry.Instance.PaymentRequisitionStatuses.Value.DefaultCode;
			OriginalRequisitionStatusForNewBizo = AH_RequisitionStatus;
		}

		public bool AllowDefaultChargeCodeLineToBeAdded { get; set; }

		#endregion

		#region IChequeNumberAutoAllocation Members

		AccChequeBook IChequeNumberAutoAllocation.ChequeBook
		{
			get { return ChequeBook; }
		}

		ZBool IChequeNumberAutoAllocation.IsAutoAllocationEnabled
		{
			get
			{
				return IsChequeNumberAutoAllocated;
			}
		}

		void IChequeNumberAutoAllocation.AssignChequeNumber(string autoGeneratedChequeNumber)
		{
			ReceiptPaymentAH_ChequeOrReference = autoGeneratedChequeNumber;
			if (ReceiptPayment != null)
			{
				ReceiptPayment.AH_ChequeOrReference = autoGeneratedChequeNumber;
			}
		}

		ZBool IChequeNumberAutoAllocation.IsAllocationPerformed
		{
			get
			{
				return !ReceiptPaymentAH_ChequeOrReference.IsEmpty;
			}
		}

		Guid IChequeNumberAutoAllocation.Printing_ObjectPK
		{
			get
			{
				return (ReceiptPayment != null) ? ReceiptPayment.PK.ToGuid() : Guid.Empty;
			}
		}

		ZGuid IChequeNumberAutoAllocation.Printing_PrinterPK
		{
			get
			{
				return (ChequeBook != null) ? ChequeBook.AK_SQ : ZGuid.Empty;
			}
		}

		ZBool IChequeNumberAutoAllocation.ChequeIsAutoPrinted
		{
			get
			{
				return ChequeIsAutoPrinted;
			}
			set
			{
				ChequeIsAutoPrinted = value;
			}
		}
		ZBool ChequeIsAutoPrinted;

		void IChequeNumberAutoAllocation.AllocationOrPrintingFailed()
		{
			ChequeBook.Reload();
			ReceiptPaymentAH_ChequeOrReference = ZString.Empty;
			if (ReceiptPayment != null)
			{
				ReceiptPayment.AH_ChequeOrReference = ZString.Empty;
			}
		}

		#endregion

		#region IDocManagerSupport Members

		protected override InvoicingDocManagerInfo GetNewDocManagerInfo()
		{
			return docManagerInfo ?? (docManagerInfo = new APInvoiceDocManagerInfo(this, Constants.DocManagerCodes.PayableInvoice));
		}
		InvoicingDocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion

		#region ContainerDetailsForCompayExport

		public struct ContainerDetailsForInvoiceExport
		{
			public ContainerDetailsForInvoiceExport(ZString containerNumber, ZString terminalCode, ZDateTime pickupDate)
			{
				this.ContainerNumber = containerNumber;
				this.TerminalCode = terminalCode;
				this.PickupDate = pickupDate;
			}

			public readonly ZString ContainerNumber;
			public readonly ZString TerminalCode;
			public readonly ZDateTime PickupDate;
		}

		public ContainerDetailsForInvoiceExport ContainerDetailsForCompayExport
		{
			get { return fContainerDetailsForCompayExport; }
			set { fContainerDetailsForCompayExport = value; }
		}

		ContainerDetailsForInvoiceExport fContainerDetailsForCompayExport;

		#endregion

		#region GetLineChargeAmountSum

		const int LineChargeAmountSumBatchSize = 200;

		internal void ClearLineChargeAmountSumCache()
		{
			LineChargeJobCached.Clear();
			LineChargeAmountSumCache.Clear();
		}

		public class LineChargeAmountSum
		{
			public ZDecimal WIPAmountSum;
			public ZDecimal RevenueAmountSum;
			public ZDecimal CSTAmountSum;
		}

		Dictionary<string, LineChargeAmountSum> LineChargeAmountSumCache
		{
			get
			{
				return lineChargeAmountSumCache ?? (lineChargeAmountSumCache = new Dictionary<string, LineChargeAmountSum>());
			}
		}
		Dictionary<string, LineChargeAmountSum> lineChargeAmountSumCache;

		HashSet<ZGuid> LineChargeJobCached
		{
			get
			{
				return lineChargeJobCached ?? (lineChargeJobCached = new HashSet<ZGuid>());
			}
		}
		HashSet<ZGuid> lineChargeJobCached;

		public LineChargeAmountSum GetLineChargeAmountSumFromCache(APInvoiceLine line)
		{
			if (!line.AL_JH.IsValid || !line.AL_AC.IsValid)
			{
				return new LineChargeAmountSum();
			}

			var lineKey = line.AL_JH.ToStringKey() + line.AL_AC.ToStringKey() + line.AL_GB.ToStringKey() + line.AL_GE.ToStringKey();

			LineChargeAmountSum cachedAmountSum;
			if (!LineChargeAmountSumCache.TryGetValue(lineKey, out cachedAmountSum))
			{
				cachedAmountSum = new LineChargeAmountSum();
				LineChargeAmountSumCache.Add(lineKey, cachedAmountSum);

				RefreshLineChargeAmountSumCache(line.AL_JH); //the cachedAmountSum will be updated by BuildLineChargeAmountSumCache
			}
			return cachedAmountSum;
		}

		void RefreshLineChargeAmountSumCache(ZGuid lineJobPK)
		{
			var newJobs = this.Lines.Cast<InvoicingLineBase>().Select(x => x.AL_JH).Distinct().Union(new ZGuid[] { lineJobPK })
					.Where(x => x.IsValid && !LineChargeJobCached.Contains(x)).ToArray();

			if (newJobs.Any())
			{
				var decimals = GlbCompany.CurrentCompany.LocalCurrency.Decimals;
				var chunkedJobs = AccountingUtils.ChunksOf(newJobs, LineChargeAmountSumBatchSize);

				foreach (var groupOfJobs in chunkedJobs)
				{
					LineChargeJobCached.UnionWith(groupOfJobs);
					var lineCharges = LoadLineChargesByChunk(groupOfJobs);

					var groupedLineAmount = lineCharges.Select(x => new
					{
						AL_JH = (ZGuid)(x[AccTransactionLines.Schema.AL_JH]),
						AL_AC = (ZGuid)(x[AccTransactionLines.Schema.AL_AC]),
						AL_GB = (ZGuid)(x[AccTransactionLines.Schema.AL_GB]),
						AL_GE = (ZGuid)(x[AccTransactionLines.Schema.AL_GE]),
						AL_LineType = (ZString)(x[AccTransactionLines.Schema.AL_LineType]),
						AL_LineAmount = (ZDecimal)(x[AccTransactionLines.Schema.AL_LineAmount])
					})
					.GroupBy(x => new { x.AL_JH, x.AL_AC, x.AL_GB, x.AL_GE, x.AL_LineType })
					.Select(g => new
					{
						Key = g.Key.AL_JH.ToStringKey() + g.Key.AL_AC.ToStringKey() + g.Key.AL_GB.ToStringKey() + g.Key.AL_GE.ToStringKey(),
						AL_LineType = g.Key.AL_LineType,
						LineAmountSum = g.Sum(x => x.AL_LineAmount)
					}).ToArray();

					LineChargeAmountSum cachedObj;
					foreach (var lineAmount in groupedLineAmount)
					{
						if (!LineChargeAmountSumCache.TryGetValue(lineAmount.Key, out cachedObj))
						{
							cachedObj = new LineChargeAmountSum();
							LineChargeAmountSumCache.Add(lineAmount.Key, cachedObj);
						}

						if (lineAmount.AL_LineType == TransactionLineTypes.WIP)
						{
							cachedObj.WIPAmountSum = lineAmount.LineAmountSum;
							cachedObj.WIPAmountSum = cachedObj.WIPAmountSum.Round(decimals);
						}
						else if (lineAmount.AL_LineType == TransactionLineTypes.Revenue)
						{
							cachedObj.RevenueAmountSum = lineAmount.LineAmountSum;
							cachedObj.RevenueAmountSum = cachedObj.RevenueAmountSum.Round(decimals);
						}
						else if (lineAmount.AL_LineType == TransactionLineTypes.Cost)
						{
							cachedObj.CSTAmountSum = lineAmount.LineAmountSum;
							cachedObj.CSTAmountSum = cachedObj.CSTAmountSum.Round(decimals);
						}
					}
				}
			}
		}

		DynamicBusinessObjectCollection LoadLineChargesByChunk(IEnumerable<ZGuid> jobs)
		{
			const string sqlText =
@"SELECT AL_PK, AL_JH, AL_LineType, AL_AC, AL_GB, AL_GE, AL_LineAmount
FROM dbo.AccTransactionLines
WHERE AL_JH IN (SELECT Value FROM @JobPKs)
AND AL_LineType IN ('WIP','REV','CST')
AND (AL_LineType IN ('REV','CST') OR (AL_LineType = 'WIP' AND AL_ReverseDate IS NULL))

UNION ALL

SELECT JR_PK, JR_JH, 'WIP', JR_AC, JR_GB, JR_GE, -JR_LocalSellAmt
FROM dbo.JobCharge
WHERE JR_JH IN (SELECT Value FROM @JobPKs)
AND JR_AL_ARLine IS NULL 
AND JR_LocalSellAmt <> 0

";
			var query = new DynamicBusinessObjectCollection(Factory);
			if (jobs.Any())
			{
				var parameters = new[]
				{
					ZSqlParameter.New("@JobPKs", jobs, JobChargeSchema.JR_JH, true)
				};

				query.Load(sqlText, parameters);
#if DEBUG
				LoadLineChargesByChunkDBHitCount_ForTestOnly++;
#endif
			}

			return query;
		}

#if DEBUG
		internal int LoadLineChargesByChunkDBHitCount_ForTestOnly;
#endif

		#endregion

		#region PaymentOrganisationAddressWithContact

		public void UpdatePaymentAddress(ZGuid orgPK)
		{
			PaymentOrganisationAddressWithContact.OrgPK = orgPK;
		}

		public ZAddressWithContact PaymentOrganisationAddressWithContact
		{
			get { return paymentorganisationAddressWithContact ?? (paymentorganisationAddressWithContact = GetPaymentOrganisationAddressWithContact()); }
		}

		#region IAmending Members

		bool IAmending.IsAmendingTransaction => false;

		bool IAmending.IsOriginalTransaction => !AH_TransactionBelongsToGroup.IsValid;

		ITransaction IAmending.OriginalTransaction => throw new NotImplementedException();

		ZGuid[] IAmending.OriginalTransactionJobPKs => throw new NotImplementedException();

		ZGuid IAmending.OriginalTransactionAccountPK => throw new NotImplementedException();

		ZString IAmending.AmendingReason { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		ZString IAmending.AmendingReasonCode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		IAmending IAmending.GenerateAmendingTransaction(string transactionType)
		{
			return GenerateAmendingTransactionCore(transactionType);
		}

		protected virtual IAmending GenerateAmendingTransactionCore(string transactionType)
		{
			IAmending result = null;

			if (transactionType == TransactionTypes.CreditNote)
			{
				result = Factory.New<APCreditNote>();
			}
			if (result != null)
			{
				result.FlagAsCreatedAmending();
				((InvoicingBase)result).AH_TransactionBelongsToGroup = this.PK;
				result.PopulateFromOriginalTransaction();
			}

			return result;
		}

		void IAmending.FlagAsCreatedAmending()
		{
			throw new NotImplementedException();
		}

		#endregion

		ZAddressWithContact paymentorganisationAddressWithContact;

		ZAddressWithContact GetPaymentOrganisationAddressWithContact()
		{
			var addressWithContact = new ZAddressWithContact(ReceiptPaymentContactOverrideInfo, ReceiptPaymentAddressOverrideInfo);
			addressWithContact.GetDefaultAddress = GetDefaultPaymentAddress;

			return addressWithContact;
		}

		ZGuid GetDefaultPaymentAddress(IOrgHeader orgHeader)
		{
			OrgAddress defaultAddress = null;
			OrgHeader header = orgHeader as OrgHeader;

			if (header != null)
			{
				defaultAddress = header.AddressForSendingAPDocuments;
			}

			return defaultAddress == null ? ZGuid.Empty : defaultAddress.PK;
		}

		#endregion

		#region Cash Advance

		protected override void UpdateRelevantCashAdvanceRequestsCore()
		{
			if (this is IInvoiceAssociatedToCashAdvanceRequest cahUpdater)
			{
				var errorMessage = InvoiceValidation.RunCashAdvanceRelatedValidation(this);
				if (errorMessage.IsNullOrEmpty())
				{
					cahUpdater.Accept(new CashAdvanceRequestUpdateVisitor());
				}
				else
				{
					throw new CannotGenerateCashAdvanceJournalException(null, errorMessage);
				}
			}
		}

		protected internal override Journal.Journal[] LoadOverpaymentCAIJournals()
		{
			return Factory.Load<APJournal>(GetOverpaymentCAIJournalsQuery());
		}

		protected override void AcceptCore(ICashAdvanceRequestProcessingByInvoiceVisitor visitor)
		{
			visitor.Visit(this);
		}

		protected internal override bool IsCashAdvanceFunctionalityEnabled => ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>().IsPayablesCashAdvanceFunctionalityEnabled;

		protected internal override bool IsManualSettingOfCashAdvanceRequestStatusToPaidAllowed => ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>().IsManualSettingOfPayablesCashAdvanceRequestStatusToPaidAllowed;

		#endregion

		#region ISupportAccProcessLogging

		ZGuid ISupportAccProcessLogging.ParentId => RelatedDraftInvoice?.PK ?? ZGuid.Empty;

		string ISupportAccProcessLogging.ParentTableCode => RelatedDraftInvoice != null ? AccDraftInvoiceHeaderSchema.Constants.Prefix : string.Empty;

		bool ISupportAccProcessLogging.ShouldLog => RelatedDraftInvoice != null;

		IAccProcessLog[] ISupportAccProcessLogging.Logs => throw new NotImplementedException(); //Will be implemented in a future WI

		IAccProcessLogger ISupportAccProcessLogging.Logger => errorLogger ?? (errorLogger = (RelatedDraftInvoice != null ? new AccDraftInvoiceProcessingErrorLogger(Factory) : throw new NotImplementedException()));
		IAccProcessLogger errorLogger;

		AccDraftInvoiceHeader RelatedDraftInvoice => Factory.LoadTop1<AccDraftInvoiceHeader>(new ZQuery(AccDraftInvoiceHeaderSchema.AIH_AH_PostedTransactionHeader, PK));

		#endregion
	}
}
