using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CashBook.DirectReceipt
{
	public class DirectReceipt : DirectTransactionHeaderBase, IDirectReceipt, IDepositBatch, IDocManagerSupport, ITemplateCopyable, IEDocsParsingSupport
	{
		public DirectReceipt(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region RelatedDepositBatch

		protected DepositBatch.DepositBatch fRelatedDepositBatch;
		public DepositBatch.DepositBatch RelatedDepositBatch
		{
			get
			{
				if (fRelatedDepositBatch == null && !AH_ReceiptBatchNo.IsEmpty)
				{
					ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
					filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, AH_ReceiptBatchNo);
					filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
					fRelatedDepositBatch = Factory.LoadTop1<DepositBatch.DepositBatch>(filter);
				}
				return fRelatedDepositBatch;
			}
		}

		#endregion

		#region Transaction Lines

		protected override DependentTransactionLineCollection GetDependentLinesCollection()
		{
			return new DirectReceiptLineCollection(this, Factory);
		}

		public override Type DependentTransactionLineType
		{
			get { return typeof(DirectReceiptLine); }
		}

		#endregion

		#region Overrides

		protected override AccTransactionHeaderLookups GetNewLookups()
		{
			return new DirectReceiptLookups(this);
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("58cf94e3-de31-4728-9a7a-160b29ce436a", "Direct Receipt"); }
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			HandleRelatedDepositBatch();
		}

		protected override bool InvertSigns
		{
			get { return false; }
		}

		protected override ZString Ledger
		{
			get { return LedgerTypes.CashBook; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return AccountingNumberFountainWrapperFactory.Instance.DirectReceiptNo; }
		}

		protected override ZString TransactionType
		{
			get { return Enterprise.ZArchitecture.Core.TransactionTypes.DirectReceipt; }
		}

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new DirectReceiptValidation(this);
		}

		internal protected override BranchLevelPostingConfigurationRegistryItem EnforceBranchLevelPostingRegistryItem
		{
			get { return AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting; }
		}

		protected override bool IsEnforcePostingAtFixedPlaceOfSupplyLevelRegistryEnabled => AccountingConfigurationRegistry.Instance.EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions.GetValueWithoutFallback(Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
		
		#endregion

		#region Properties

		[List("ReceiptMethods")]
		public override ZString AH_ReceiptType
		{
			get { return base.AH_ReceiptType; }
			set
			{
				if (base.AH_ReceiptType != value)
				{
					base.AH_ReceiptType = value;

					AH_ChequeDrawerInfo.RefreshBinding();
					AH_DrawerBankInfo.RefreshBinding();
					AH_DrawerBranchInfo.RefreshBinding();
				}
			}
		}

		protected override bool AH_ChequeDrawer_ReadOnly
		{
			get { return base.AH_ChequeDrawer_ReadOnly || AH_ReceiptType != ZArchitecture.Core.ReceiptTypes.Cheque; }
		}

		protected override bool AH_DrawerBank_ReadOnly
		{
			get { return base.AH_DrawerBank_ReadOnly || AH_ReceiptType != ZArchitecture.Core.ReceiptTypes.Cheque; }
		}

		protected override bool AH_DrawerBranch_ReadOnly
		{
			get { return base.AH_DrawerBranch_ReadOnly || AH_ReceiptType != ZArchitecture.Core.ReceiptTypes.Cheque; }
		}

		protected bool ChequeBookPK_ReadOnly
		{
			get { return true; }
		}

		public override ZDecimal Debit
		{
			get { return DebitForDirectReceiptPayment; }
		}

		public override ZDecimal Credit
		{
			get { return CreditForDirectReceiptPayment; }
		}

		public override ZString DepositBatchNumber
		{
			get { return AH_ReceiptBatchNo; }
		}

		protected override bool IsOverrideTaxBranchSecurityAllowed => Env.Security.NewCashBookAllowOverrideTaxBranch.IsAllowedWithConstraint();

		#endregion

		#region IDepositBatch Members

		ZBool IDepositBatch.IsDirectCreditAndNotOpeningReceipt
		{
			get { return IsDirectCredit; }
		}

		public ZDateTime CompayReceiptBatchDate
		{
			get { return ZDateTime.Empty; }
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new AccountingDocManagerInfo(this, Constants.DocManagerCodes.DirectReceipt)); }
		}
		AccountingDocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

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

			DirectReceipt copyOfCurrent = (DirectReceipt)Factory.New(GetType());
			copyOfCurrent.AH_AB = this.AH_AB;
			copyOfCurrent.AH_ReceiptType = this.AH_ReceiptType;
			copyOfCurrent.AH_Desc = this.AH_Desc;

			foreach (DirectReceiptLine line in Lines.Cast<DirectReceiptLine>())
			{
				var newLine = copyOfCurrent.Lines.AddNew();
				newLine.CopyValuesFrom_OnlyFieldsValidToCopyAndInValidOrder(line, fields);
				newLine.AL_TaxDate = ZDate.Today;
			}

			return copyOfCurrent;
		}

		#endregion

		#region Implementation

		protected virtual void HandleRelatedDepositBatch()
		{
			if (!IsInDatabase && IsDirectCredit && !AH_IsCancelled)
			{
				DepositBatchCreator depBatCreator = new DepositBatchCreator(this);
				depBatCreator.CreateDepositBatch();
			}
		}

		public bool IsDirectCredit
		{
			get
			{
				return AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.DirectCredit ||
					AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.InterestReceived ||
					AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.eNettDirectCredit ||
					AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.AccountMaintenanceFee ||
					AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.BankDepositFee ||
					AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.BankDebitTax ||
					AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.InterestPaid ||
					AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.PeriodicPayment ||
					AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.StampDuty ||
					AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.MiscellaneousReceipt ||
					AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.MiscellaneousFees;
			}
		}

		#endregion
	}
}
