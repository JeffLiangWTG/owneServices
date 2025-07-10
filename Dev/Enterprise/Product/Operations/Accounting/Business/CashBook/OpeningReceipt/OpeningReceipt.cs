using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook.OpeningReceipt
{
	public class OpeningReceipt : TransactionHeader, ICashBook, IDocManagerSupport, IEDocsParsingSupport
	{
		public OpeningReceipt(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			new LocalForeignDataEntry(AH_RX_NKTransactionCurrencyInfo, AH_ExchangeRateInfo, AH_LocalExTaxAmountInfo, AH_OSExTaxAmountInfo, ExchangeRate as ZAccExchangeRate);
		}

		public bool SubmittedFromForm { get; set; }

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AH_Desc = Res.GetString("46cc47e6-f3a6-4a1d-a3da-e4aa884055b8", "Opening Receipt");
			AH_RX_NKTransactionCurrency_ReadOnly = true;
			AH_ReceiptType = AccountingConfigurationRegistry.Instance.DefaultCashBookReceiptType.Value;
		}

		#endregion

		#region Validation

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new OpeningReceiptValidation(this);
		}

		#endregion

		#region Property Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("46cc47e6-f3a6-4a1d-a3da-e4aa884055b8", "Opening Receipt"); }
		}

		#region AH_OH

		protected override ZGuid AH_OHCore
		{
			get { return base.AH_OHCore; }
			set
			{
				base.AH_OHCore = value;
				if (Header != null)
				{
					AH_ChequeDrawer = Header.CompanyData.OB_ARPreviousChequeDrawer;
					AH_DrawerBank = Header.CompanyData.OB_ARPreviousChequeDrawerBank;
					AH_DrawerBranch = Header.CompanyData.OB_ARPreviousChequeDrawerBankBranch;
					ZGuid defaultBank = new ReceiptPaymentDefaultsAR(this).GetDefaultBankAccount();
					if (defaultBank.IsValid)
					{
						AH_AB = defaultBank;
					}
				}
			}
		}

		#endregion

		#region AH_ReceiptType

		[List("ReceiptMethods")]
		public override ZString AH_ReceiptType
		{
			get { return base.AH_ReceiptType; }
			set
			{
				base.AH_ReceiptType = value;
				AH_ChequeOrReference = DefaultChequeOrReference;
			}
		}

		#endregion

		#region AH_AB

		public override ZGuid AH_AB
		{
			get { return base.AH_AB; }
			set
			{
				base.AH_AB = value;
				if (BankAccount != null && SubmittedFromForm)
				{
					ExchangeRate.Currency = BankAccount.AB_RX_NKAccountCurrency;

					if (BankAccount.IsCashAccount)
					{
						AH_ReceiptType = ReceiptTypes.Cash;
					}
				}
			}
		}

		#endregion

		#region SetOutstandingLocalAmountAfterLocalExTaxAmountSet

		protected override void SetOutstandingLocalAmountAfterLocalExTaxAmountSet()
		{
			SetOutstandingLocalAmountAfterLocalExTaxAmountSetCore();
		}

		#endregion

		#region AH_TransactionNum

		protected override bool AH_TransactionNum_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#endregion

		#region Implementation

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			IsSavedSuccessfully = saveSucceeded;
			RefreshBinding();
		}

		bool IsSavedSuccessfully { get; set; }

		protected override bool GetPropertiesReadOnlyState(PropertyDescriptor property)
		{
			bool result = IsSavedSuccessfully;
			if (!result && property != null)
			{
				switch (property.Name)
				{
					case Schema.AH_ChequeDrawer:
					case Schema.AH_DrawerBank:
					case Schema.AH_DrawerBranch:
						result = (AH_ReceiptType != ReceiptTypes.Cheque) || IsInDatabase || IsReverseTransaction;
						break;
				}
			}
			result = result || base.GetPropertiesReadOnlyState(property);
			return result;
		}

		string DefaultChequeOrReference
		{
			get
			{
				var defaultReferenceNumber = AccountingConfigurationRegistry.Instance.GetReferenceNumberFromType(AH_ReceiptType);
				return AH_ChequeOrReference = !defaultReferenceNumber.IsEmpty ? defaultReferenceNumber : AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.Cash ? (ZString)ZArchitecture.Core.ReceiptTypes.Cash : ZString.Empty;
			}
		}

		protected override bool InvertSigns
		{
			//TODO: Still needs to be correctly implemented.
			get { return true; }
		}

		protected override ZString Ledger
		{
			get { return LedgerTypes.CashBook; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return AccountingNumberFountainWrapperFactory.Instance.OpeningReceiptNo; }
		}

		protected override ZString TransactionType
		{
			get { return TransactionTypes.OpeningReceipt; }
		}

		public override ZDecimal Debit
		{
			get { return DebitForNormalReceiptPayment; }
		}

		public override ZDecimal Credit
		{
			get { return CreditForNormalReceiptPayment; }
		}

		protected override AccTransactionHeaderLookups GetNewLookups()
		{
			return new OpeningReceiptLookups(this);
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new AccountingDocManagerInfo(this, Constants.DocManagerCodes.OpeningReceipt)); }
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
	}
}
