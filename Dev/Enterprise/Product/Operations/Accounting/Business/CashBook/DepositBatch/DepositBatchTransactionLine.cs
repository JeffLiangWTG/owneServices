using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch
{
	public partial class DepositBatchTransactionLine : TransactionHeader, IObsoleteValidation
	{
		public DepositBatchTransactionLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (AH_TransactionType.IsEmpty)
			{
				// AccTransactionHeader is not valid with empty AH_TransactionType.
				// Default type (Journal) fails with crtical validation "AR JNL with an empty GL account field." in test AccTransactionHeaderBusinessObjectTest.TestComplianceSequenceFromSubType.
				AH_TransactionType = TransactionTypes.Receipt;
			}
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif

		#region Transaction Header Implementation

		protected override List<string> GetWritableProperties()
		{
			return new List<string> { "IsSelected" };
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("c5bc6b6b-35d1-442b-ad78-3830efcf36ae", "Deposit Batch Line"); }
		}

		protected override bool InvertSigns
		{
			get
			{
				return AH_Ledger != LedgerTypes.CashBook;
			}
		}

		protected override ZString TransactionType
		{
			get { return AH_TransactionType; }
		}

		protected override ZString Ledger
		{
			get { return AH_Ledger; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return null; }
		}

		#endregion

		#region Validation

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			if (Parent != null && Parent.IsReverseTransaction)
			{
				return GetNewReversalValidation();
			}
			else
			{
				return new DepositBatchTransactionLineValidation(this);
			}
		}

		protected override AccTransactionHeaderValidation GetNewEmptyValidation()
		{
			return GetNewValidationCore();
		}

		#endregion

		#region Properties

		public DepositBatch Parent
		{
			get { return fParent; }
			set { fParent = value; }
		}

		DepositBatch fParent;

		public ZBool IsSelected
		{
			get { return fIsSelected; }
			set
			{
				SetNonPersistentPropertyValue(IsSelectedInfo, ref fIsSelected, value);
				if (Parent != null)
				{
					Parent.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IsSelectedInfo
		{
			get { return GetZPropertyInfo(nameof(IsSelected)); }
		}

		protected bool IsSelected_ReadOnly
		{
			get { return Parent != null && Parent.IsInDatabase; }
		}

		ZBool fIsSelected = true;

		protected override bool GetPropertiesReadOnlyState(PropertyDescriptor property)
		{
			bool result = false;
			if (property != null)
			{
				if (property.Name == "IsSelected")
				{
					result = CargoWise.ComponentModel.MetaData.GetReadOnlyExcludingMethodProvider(this, property);
				}
				else
				{
					result = base.GetPropertiesReadOnlyState(property);
				}
			}
			return result;
		}

		public ZString BranchCode
		{
			get { return base.Branch != null ? base.Branch.GB_Code : ZString.Empty; }
		}

		public ZPropertyInfo BranchCodeInfo
		{
			get { return GetZPropertyInfo(nameof(BranchCode)); }
		}

		public ZString BankCode
		{
			get { return BankAccount != null ? BankAccount.AB_Code : ZString.Empty; }
		}

		public ZPropertyInfo BankCodeInfo
		{
			get { return GetZPropertyInfo(nameof(BankCode)); }
		}

		public ZString BankAccountNumber
		{
			get { return BankAccount != null ? BankAccount.AB_AccountNum : ZString.Empty; }
		}

		public ZPropertyInfo BankAccountNumberInfo
		{
			get { return GetZPropertyInfo(nameof(BankAccountNumber)); }
		}

		public ZString BankName
		{
			get { return BankAccount != null ? BankAccount.AB_Desc : ZString.Empty; }
		}

		public ZPropertyInfo BankNameInfo
		{
			get { return GetZPropertyInfo(nameof(BankName)); }
		}

		public ZString CurrencyCode
		{
			get { return BankAccount != null ? BankAccount.AB_RX_NKAccountCurrency : ZString.Empty; }
		}

		public ZPropertyInfo CurrencyCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CurrencyCode)); }
		}

		[DecimalPlaces(nameof(BankCurrencyDecimalsAsInt))]
		public ZDecimal TotalDepositAmount
		{
			get
			{
				return IsBankCurrencyLocal ? CorrectSigns(AH_LocalTotal) : CorrectSigns(AH_OSTotal);
			}
		}

		public bool IsBankCurrencyLocal
		{
			get
			{
				return CurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
		}

		public ZPropertyInfo TotalDepositAmountInfo
		{
			get { return GetZPropertyInfo(nameof(TotalDepositAmount)); }
		}

		decimal CorrectSigns(decimal oSTotal)
		{
			return oSTotal * (AH_TransactionType == TransactionTypes.Receipt ? -1 : 1);
		}

		protected override bool AH_TransactionType_ReadOnly
		{
			get { return true; }
		}

		protected override bool AH_InvoiceDate_ReadOnly
		{
			get { return true; }
		}

		protected override bool AH_TransactionNum_ReadOnly
		{
			get { return true; }
		}

		protected bool AH_OSTotal_ReadOnly
		{
			get { return true; }
		}

		protected bool AH_Ledger_ReadOnly
		{
			get { return true; }
		}

		protected bool AH_ReceiptType_ReadOnly
		{
			get { return true; }
		}

		protected override bool AH_ChequeOrReference_ReadOnly
		{
			get { return true; }
		}

		protected bool AH_ChequeDrawer_ReadOnly
		{
			get { return true; }
		}

		protected bool AH_DrawerBank_ReadOnly
		{
			get { return true; }
		}

		protected bool AH_DrawerBranch_ReadOnly
		{
			get { return true; }
		}

		#endregion
	}
}
