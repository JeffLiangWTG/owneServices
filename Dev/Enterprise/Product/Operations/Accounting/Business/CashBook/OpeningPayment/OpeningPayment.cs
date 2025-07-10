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

namespace Enterprise.Accounting.Business.CashBook.OpeningPayment
{
	public class OpeningPayment : TransactionHeader, ICashBook, IDocManagerSupport, IEDocsParsingSupport
	{
		public OpeningPayment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			new LocalForeignDataEntry(AH_RX_NKTransactionCurrencyInfo, AH_ExchangeRateInfo, AH_LocalExTaxAmountInfo, AH_OSExTaxAmountInfo, ExchangeRate as ZAccExchangeRate);
		}

		public bool SubmittedFromForm { get; set; }

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AH_Desc = Res.GetString("b0ca65a1-d756-45da-a8b2-472c73357526", "Opening payment");
			AH_ReceiptType = AccountingConfigurationRegistry.Instance.DefaultCashBookPaymentType.Value;
		}

		#endregion

		#region Validation

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new OpeningPaymentValidation(this);
		}

		#endregion

		#region Property Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("f22fb987-9f2c-4ff7-b204-1c6178924758", "Opening Payment"); }
		}

		#region AH_ReceiptType

		[List("PaymentMethods")]
		public override ZString AH_ReceiptType
		{
			get { return base.AH_ReceiptType; }
			set
			{
				base.AH_ReceiptType = value;
				var defaultReferenceNumber = AccountingConfigurationRegistry.Instance.GetReferenceNumberFromType(value);
				AH_ChequeOrReference = !defaultReferenceNumber.IsEmpty ? defaultReferenceNumber : ZString.Empty;
			}
		}

		#endregion

		#region AH_OH

		protected override ZGuid AH_OHCore
		{
			get { return base.AH_OHCore; }
			set
			{
				base.AH_OHCore = value;
				ZGuid defaultBank = new ReceiptPaymentDefaultsAR(this).GetDefaultBankAccount();
				if (defaultBank.IsValid)
				{
					AH_AB = defaultBank;
				}
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

					if (IsCashAccountType)
					{
						AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
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

		//[ReadOnly(true)]
		//public override ZString AH_TransactionNum
		//{
		//    get { return base.AH_TransactionNum; }
		//    set { base.AH_TransactionNum = value; }
		//}

		protected override bool AH_TransactionNum_ReadOnly
		{
			get { return true; }
		}

		//[ReadOnly(true)]
		//public override ZString AH_RX_NKTransactionCurrency
		//{
		//    get { return base.AH_RX_NKTransactionCurrency; }
		//    set { base.AH_RX_NKTransactionCurrency = value; }
		//}

		public override bool AH_RX_NKTransactionCurrency_ReadOnly
		{
			get { return true; }
		}

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
			return IsSavedSuccessfully || base.GetPropertiesReadOnlyState(property);
		}

		protected override bool InvertSigns
		{
			//TODO: Still needs to be correctly implemented.
			get { return false; }
		}

		protected override ZString Ledger
		{
			get { return Enterprise.ZArchitecture.Core.LedgerTypes.CashBook; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber
		{
			get { return AccountingNumberFountainWrapperFactory.Instance.OpeningPaymentNo; }
		}

		protected override ZString TransactionType
		{
			get { return Enterprise.ZArchitecture.Core.TransactionTypes.OpeningPayment; }
		}

		public override ZDecimal Debit
		{
			get { return DebitForNormalReceiptPayment; }
		}

		public override ZDecimal Credit
		{
			get { return CreditForNormalReceiptPayment; }
		}

		public override ExchangeRateType RateType
		{
			get { return ExchangeRateType.Buy; }
		}

		protected override AccTransactionHeaderLookups GetNewLookups()
		{
			return new OpeningPaymentLookups(this);
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new AccountingDocManagerInfo(this, Constants.DocManagerCodes.OpeningPayment)); }
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
