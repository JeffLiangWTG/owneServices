using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook.DirectPayment
{
	public partial class DirectPayment : DirectTransactionHeaderBase, IDocumentSupportable, IDirectPayment, IDirectDebitBatchTransaction, IChequeNumberAutoAllocation,
		IDocManagerSupport, ICanUpdateChequeNumber, ITemplateCopyable, IEDocsParsingSupport
	{
		public DirectPayment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AH_InvoiceAmount), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AH_OSTotal), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AH_GSTAmount), ConcurrencyPolicy.Strict);
		}

		#region Transaction Lines

		protected override DependentTransactionLineCollection GetDependentLinesCollection()
		{
			return new DirectPaymentLineCollection(this, Factory);
		}

		public override Type DependentTransactionLineType
		{
			get { return typeof(DirectPaymentLine); }
		}

		#endregion

		#region Override

		protected override ZPropertyInfo[] GetPropertiesWithStrictConcurrency()
		{
			var additionalPropertiesWithStrictConcurrency = new ZPropertyInfo[] { AH_InvoiceAmountInfo, AH_OSTotalInfo, AH_GSTAmountInfo };
			return base.GetPropertiesWithStrictConcurrency().Concat(additionalPropertiesWithStrictConcurrency).ToArray();
		}

		protected override List<string> GetWritableProperties()
		{
			List<string> result = base.GetWritableProperties();
			if (string.IsNullOrEmpty(AH_ReceiptBatchNo))
			{
				result.Add("IncludeInTheBatch");
			}
			return result;
		}

		protected override AccTransactionHeaderLookups GetNewLookups()
		{
			return new DirectPaymentLookups(this);
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("d6cd0357-3c43-47b5-966f-b34f3b4c9c14", "Direct Payment"); }
		}

		public override ZDecimal Debit
		{
			get { return DebitForDirectReceiptPayment; }
		}

		public override ZDecimal Credit
		{
			get { return CreditForDirectReceiptPayment; }
		}

		public override ZString DirectDebitNumber
		{
			get { return AH_ReceiptBatchNo; }
		}

		public override ZString AH_ChequeOrReference
		{
			get
			{
				return base.AH_ChequeOrReference;
			}
			set
			{
				if (AH_ReceiptType == ReceiptTypes.Cheque && BankAccount != null)
				{
					value = AccValidationHelper.PadChequeDigitsWithLeadingZeros(BankAccount, value);
				}
				base.AH_ChequeOrReference = value;
			}
		}

		public override ZGuid AH_AB
		{
			get { return base.AH_AB; }
			set
			{
				if (base.AH_AB != value)
				{
					ResetChequeBook();
				}

				base.AH_AB = value;

				if (BankAccount == null)
				{
					ResetChequeBook();
				}
			}
		}

		[List("PaymentMethods")]
		public override ZString AH_ReceiptType
		{
			get { return base.AH_ReceiptType; }
			set
			{
				base.AH_ReceiptType = value;

				AH_DrawerBankInfo.RefreshBinding();
				AH_DrawerBranchInfo.RefreshBinding();

				if (value != ReceiptTypes.Cheque)
				{
					if (value == ReceiptTypes.Cash)
					{
						AH_ChequeDrawer = "CASH";
					}

					ResetChequeBook();
					DirectTransactionHeaderBaseValidation directTransactionValiation = Validation as DirectTransactionHeaderBaseValidation;
					if (directTransactionValiation != null)
					{
						directTransactionValiation.ValidateAH_ChequeOrReference();
					}
				}
			}
		}

		protected override bool AH_DrawerBank_ReadOnly
		{
			get { return base.AH_DrawerBank_ReadOnly || !(AH_ReceiptType == ReceiptTypes.DirectDebit && BankAccount != null && BankAccount.AB_AllowAutoDDR); }
		}

		protected override bool AH_DrawerBranch_ReadOnly
		{
			get { return base.AH_DrawerBranch_ReadOnly || !(AH_ReceiptType == ReceiptTypes.DirectDebit && BankAccount != null && BankAccount.AB_AllowAutoDDR); }
		}

		protected override bool InvertSigns
		{
			get { return true; }
		}

		protected override ZString Ledger
		{
			get { return LedgerTypes.CashBook; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return AccountingNumberFountainWrapperFactory.Instance.DirectPaymentNo; }
		}

		protected override ZString TransactionType
		{
			get { return TransactionTypes.DirectPayment; }
		}

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new DirectPaymentValidation(this);
		}

		protected bool ChequeBookPK_ReadOnly
		{
			get { return (AH_ReceiptType != ReceiptTypes.Cheque || IsReverseTransaction); }
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			bool chequeNumberWasAllocated = (ChequeBook != null) && ChequeBook.IsAutoPrint && IsCheque;

			if (saveSucceeded && CanIncrementChequeBookNumber && !chequeNumberWasAllocated)
			{
				AccChequeBook.UpdateCurrentNumber(ChequeBook.PK, ZDecimal.Parse(AH_ChequeOrReference) + 1);
			}

			if (!IsInDatabase && !saveSucceeded && !AH_TransactionNum.IsEmpty)
			{
				AH_TransactionNum = ZString.Empty;
			}
		}

		public ZBool CanIncrementChequeBookNumber
		{
			get
			{
				return ChequeBook != null && ZDecimal.CanParseAsInteger(AH_ChequeOrReference) &&
					GetChequeNumberStatus() == ChequeNumberStatus.GreaterThanOrEqualToCurrentNum;
			}
		}

		public ChequeNumberStatus GetChequeNumberStatus()
		{
			ChequeNumberStatus returnStatus = ChequeNumberStatus.NoChequeNumber;

			if (!ChequeBookPK.IsEmpty)
			{
				if (ChequeBook != null && ZDecimal.CanParseAsInteger(AH_ChequeOrReference))
				{
					if (ZDecimal.Parse(AH_ChequeOrReference) >= ChequeBook.AK_CurrentNo)
					{
						returnStatus = ChequeNumberStatus.GreaterThanOrEqualToCurrentNum;
					}
					else if (ChequeBook.BankAccount != null && ChequeBook.BankAccount.IsChequeNumberUsedByCancelledPayment(AH_ChequeOrReference))   // allow the save but don't increment current number of cheque book
					{
						returnStatus = ChequeNumberStatus.CancelledPayment;
					}
					else    // do gui validation saying that the number is less than current number
					{
						returnStatus = ChequeNumberStatus.LessThanCurrentNum;
					}
				}
			}

			return returnStatus;
		}

		protected override ZBool IsChequeNumberAutoAllocated
		{
			get
			{
				if (ChequeBook != null)
				{
					return ChequeBook.IsAutoPrint && IsCheque;
				}
				else
				{
					return ZBool.False;
				}
			}
		}

		protected override void CheckNumberIsAutoAllocated()
		{
			base.CheckNumberIsAutoAllocated();

			Calc_ChequeIsAutoPrintedLabelInfo.RefreshBinding();
			Calc_ChequeNumberIsAutoAllocatedLabelInfo.RefreshBinding();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				fIsAutoLogged = true;
			}
		}

		bool fIsAutoLogged = true;

		protected override AutologState AutoLoggingState => fIsAutoLogged
			? AutologState.AutoLogged
			: AutologState.NotLogged;

		internal protected override BranchLevelPostingConfigurationRegistryItem EnforceBranchLevelPostingRegistryItem
		{
			get { return AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting; }
		}

		protected override bool IsEnforcePostingAtFixedPlaceOfSupplyLevelRegistryEnabled => AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions.GetValueWithoutFallback(Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
		
		#endregion

		#region Public Members

		public void UpdateChequeNumber(string newChequeNumber, bool withReprint)
		{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Logs.AddNew(Events.EditedARecord, PaymentApprovalBulkProcessor.GetCheckNumberUpdatedMessage(AH_ChequeOrReference, newChequeNumber, withReprint));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			fIsAutoLogged = false;
			AH_ChequeOrReference = newChequeNumber;
		}

		public ZString Calc_ChequeNumberIsAutoAllocatedLabel
		{
			get { return IsChequeNumberAutoAllocated && !IsInDatabase ? AccountingConstants.ChequeLabelConstants.ChequeNumberIsAutoAllocatedLabel : ""; }
		}

		public ZPropertyInfo Calc_ChequeNumberIsAutoAllocatedLabelInfo
		{
			get { return GetZPropertyInfo(nameof(Calc_ChequeNumberIsAutoAllocatedLabel)); }
		}

		public ZString Calc_ChequeIsAutoPrintedLabel
		{
			get { return IsChequeNumberAutoAllocated && !IsInDatabase ? AccountingConstants.ChequeLabelConstants.ChequeAutoPrintedLabel : ""; }
		}

		public ZPropertyInfo Calc_ChequeIsAutoPrintedLabelInfo
		{
			get { return GetZPropertyInfo(nameof(Calc_ChequeIsAutoPrintedLabel)); }
		}

		public ZString BankAccountNumber
		{
			get { return BankAccount != null ? BankAccount.AB_AccountNum : ZString.Empty; }
		}

		public override ZString AH_AKCode
		{
			get { return ChequeBook == null ? ZString.Empty : ChequeBook.AK_Code; }
		}

		#endregion

		#region IDocumentSupportable Members

		public override DocumentSupporter DocumentSupporter
		{
			get { return new DirectPaymentDocumentSupporter(this); }
		}

		#endregion

		#region IDocManagerSupport Members

		public override DocManagerInfo DocManagerInfo
		{
			get { return new DocManagerInfo(this, Core.Constants.DocManagerCodes.CashBook); }
		}

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

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
			AH_ChequeOrReference = autoGeneratedChequeNumber;
		}

		ZBool IChequeNumberAutoAllocation.IsAllocationPerformed
		{
			get
			{
				return !AH_ChequeOrReference.IsEmpty;
			}
		}

		Guid IChequeNumberAutoAllocation.Printing_ObjectPK
		{
			get
			{
				return PK.ToGuid();
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
			AH_ChequeOrReference = ZString.Empty;
		}

		#endregion

		#region IDirectDebitBatchTransaction Members

		public OrgHeaderCollection Headers
		{
			get { return Lookups.Headers; }
		}

		public ZString Code
		{
			get { return AH_Ledger; }
		}

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(nameof(Code)); }
		}

		public ZString PayeeBankAccountNumber
		{
			get { return AH_DrawerBank; }
		}

		public ZPropertyInfo PayeeBankAccountNumberInfo
		{
			get { return GetZPropertyInfo(nameof(PayeeBankAccountNumber)); }
		}

		public ZString PayeeBankBranchName => ZString.Empty;

		public ZPropertyInfo PayeeBankBranchNameInfo => GetZPropertyInfo(nameof(PayeeBankBranchName));

		public ZString PayeeBankAddress1 => ZString.Empty;

		public ZPropertyInfo PayeeBankAddress1Info => GetZPropertyInfo(nameof(PayeeBankAddress1));

		public ZString PayeeBankAddress2 => ZString.Empty;

		public ZPropertyInfo PayeeBankAddress2Info => GetZPropertyInfo(nameof(PayeeBankAddress2));

		public ZString PayeeBankAddress3 => ZString.Empty;

		public ZPropertyInfo PayeeBankAddress3Info => GetZPropertyInfo(nameof(PayeeBankAddress3));

		public ZString AccountTitle
		{
			get { return AH_ChequeDrawer; }
		}

		public ZPropertyInfo AccountTitleInfo
		{
			get { return GetZPropertyInfo(nameof(AccountTitle)); }
		}

		public ZString PayeeBankBSB
		{
			get { return AH_DrawerBranch; }
		}

		public ZPropertyInfo PayeeBankBSBInfo
		{
			get { return GetZPropertyInfo(nameof(PayeeBankBSB)); }
		}

		public ZString AccountCurrency
		{
			get { return string.Empty; }
		}

		public ZPropertyInfo AccountCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(AccountCurrency)); }
		}

		public ZString PayeeBankName
		{
			get { return string.Empty; }
		}

		public ZPropertyInfo PayeeBankNameInfo
		{
			get { return GetZPropertyInfo(nameof(PayeeBankName)); }
		}

		public ZString PayeeBankSwift
		{
			get { return string.Empty; }
		}

		public ZPropertyInfo PayeeBankSwiftInfo
		{
			get { return GetZPropertyInfo(nameof(PayeeBankSwift)); }
		}

		public ZString PayeeIBANNumber
		{
			get { return string.Empty; }
		}

		public ZPropertyInfo PayeeIBANNumberInfo
		{
			get { return GetZPropertyInfo(nameof(PayeeIBANNumber)); }
		}

		public ZString PayeeCountryCode
		{
			get { return string.Empty; }
		}

		public ZPropertyInfo PayeeCountryCodeInfo
		{
			get { return GetZPropertyInfo(nameof(PayeeCountryCode)); }
		}

		ZBool fIncludeInTheBatch;

		public ZBool IncludeInTheBatch
		{
			get { return fIncludeInTheBatch; }
			set
			{
				if (value != fIncludeInTheBatch)
				{
					SetNonPersistentPropertyValue(IncludeInTheBatchInfo, ref fIncludeInTheBatch, value);
					if (DDRCollection != null)
					{
						var sign = fIncludeInTheBatch ? 1 : -1;
						ZDecimal amountToUpdate = AH_OSTotalAmount * sign;
						ZDecimal localAmountToUpdate = AH_LocalTotalAmount * sign;
						DDRCollection.UpdateSelectedTotal(amountToUpdate, localAmountToUpdate);
					}
				}
			}
		}

		public ZPropertyInfo IncludeInTheBatchInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeInTheBatch)); }
		}

		DirectDebitBatchLineCollection fDDRCollection;
		public DirectDebitBatchLineCollection DDRCollection
		{
			get
			{
				if (fDDRCollection == null)
				{
					fDDRCollection = new DirectDebitBatchLineCollection(Factory);
					foreach (BusinessObjectCollection parentCollection in ((IBusinessObjectInternals)this).ParentCollections)
					{
						DirectDebitBatchLineCollection parentDDRCollection = parentCollection as DirectDebitBatchLineCollection;
						if (parentDDRCollection != null)
						{
							fDDRCollection = parentDDRCollection;
							break;
						}
					}
				}
				return fDDRCollection;
			}
		}

		bool IDirectDebitBatchComponent.ShouldValidateDirectDebitBatchComponent
		{
			get { return (fDDRCollection != null && fDDRCollection.DDRHeader != null && fDDRCollection.DDRHeader.ShouldValidateDirectDebitBatchComponent); }
		}

		public ZBool AllowAutoDDR
		{
			get { return !(AH_ChequeDrawer.IsEmpty || AH_DrawerBank.IsEmpty || AH_DrawerBranch.IsEmpty); }
		}

		public ZPropertyInfo AllowAutoDDRInfo
		{
			get { return GetZPropertyInfo(nameof(AllowAutoDDR)); }
		}

		ZString IDirectDebitBatchTransaction.AH_ReceiptType
		{
			get
			{
				return base.AH_ReceiptType;
			}

			set
			{
				SetReceiptTypeOnly(value);
			}
		}
		public ZString BankCreateUser => ZString.Empty;
		public ZPropertyInfo BankCreateUserInfo => GetZPropertyInfo(nameof(BankCreateUser));

		public ZDateTime BankCreateTimeLocal => ZDateTime.Empty;
		public ZPropertyInfo BankCreateTimeLocalInfo => GetZPropertyInfo(nameof(BankCreateTimeLocal));

		public ZString BankLastEditUser => ZString.Empty;
		public ZPropertyInfo BankLastEditUserInfo => GetZPropertyInfo(nameof(BankLastEditUser));

		public ZDateTime BankLastEditTimeLocal => ZDateTime.Empty;
		public ZPropertyInfo BankLastEditTimeLocalInfo => GetZPropertyInfo(nameof(BankLastEditTimeLocal));

		void IDirectDebitBatchTransaction.ValidateAutoDDR()
		{
			DirectPaymentValidation directPaymentValiation = Validation as DirectPaymentValidation;
			if (directPaymentValiation != null)
			{
				directPaymentValiation.ValidateAllowAutoDDR();
			}
		}

		void IDirectDebitBatchTransaction.ValidateBankBSB()
		{
			DirectPaymentValidation directPaymentValiation = Validation as DirectPaymentValidation;
			if (directPaymentValiation != null)
			{
				directPaymentValiation.ValidatePayeeBankBSB();
			}
		}

		void IDirectDebitBatchTransaction.ValidateBankAccountNumber()
		{
			DirectPaymentValidation directPaymentValiation = Validation as DirectPaymentValidation;
			if (directPaymentValiation != null)
			{
				directPaymentValiation.ValidatePayeeBankAccountNumber();
			}
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new AccountingDocManagerInfo(this, Core.Constants.DocManagerCodes.CashBook)); }
		}
		AccountingDocManagerInfo docManagerInfo;

		#endregion

		#region ITemplateCopyable Members

		protected override TransactionHeader CopyTransaction()
		{
			var fields = new string[] {
				AutoAccTransactionLines.Schema.AL_AG,
				AutoAccTransactionLines.Schema.AL_GB,
				AutoAccTransactionLines.Schema.AL_GE,
				AutoAccTransactionLines.Schema.AL_Desc,
				TransactionLine.Schema.AL_OSExTaxAmount,
				AutoAccTransactionLines.Schema.AL_AT,
				AutoAccTransactionLines.Schema.AL_A9_VATClass,
				TransactionLine.Schema.AL_OSTaxAmount
			};

			DirectPayment copyOfCurrent = (DirectPayment)Factory.New(GetType());
			copyOfCurrent.AH_AB = this.AH_AB;
			copyOfCurrent.AH_ReceiptType = this.AH_ReceiptType;
			copyOfCurrent.AH_Desc = this.AH_Desc;
			copyOfCurrent.AH_ChequeDrawer = this.AH_ChequeDrawer;

			foreach (DirectPaymentLine line in Lines.Cast<DirectPaymentLine>())
			{
				var newLine = copyOfCurrent.Lines.AddNew();
				newLine.CopyValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(line, fields);
				newLine.AL_TaxDate = ZDate.Today;
			}

			return copyOfCurrent;
		}

		#endregion

		#region Implementation

		protected AccValidationHelper AccValidationHelper
		{
			get
			{
				if (fAccValidationHelper == null)
				{
					fAccValidationHelper = new AccValidationHelper();
				}
				return fAccValidationHelper;
			}
		}
		AccValidationHelper fAccValidationHelper;

		public bool IsCheque
		{
			get { return AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cheque; }
		}

		#endregion

	}

	public class DirectPaymentDocumentSupporter : TransactionHeader.TransactionHeaderDocumentSupporter
	{
		public DirectPaymentDocumentSupporter(DirectPayment directPayment)
			: base(directPayment)
		{
		}

		protected DirectPayment DirectPayment
		{
			get { return (DirectPayment)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CBDirectPayment; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Enterprise.Core.Constants.DataContext.GenericFreightJob)
			{
				return DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, DirectPayment);
			}
			else if (dataContext == Core.Constants.DataContext.AccountingVoucher)
			{
				return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
			}
			else
			{
				return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.DirectPayment, DirectPayment) };
			}
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			List<Core.Constants.DataContext> result = new List<Core.Constants.DataContext>(base.GetSupportedDataContexts());
			result.Add(Core.Constants.DataContext.GenericFreightJob);
			return result.ToArray();
		}
	}
}
