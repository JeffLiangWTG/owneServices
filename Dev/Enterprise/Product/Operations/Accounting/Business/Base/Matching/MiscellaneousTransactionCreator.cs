using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public abstract class MiscellaneousTransactionCreator : NonPersistentBusinessObject, IObsoleteValidation
	{
		public MiscellaneousTransactionCreator(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Methods for Instantiation

		public abstract Overpayment GetNewOverpayment();
		public abstract Discount GetNewDiscount();
		public abstract ExchangeDifference GetNewExchangeDifference();
		public abstract Journal GetNewBankFee();

		#endregion

		#region Methods for Setting Values

		#region CreateOverpayment

		public Overpayment CreateOverpayment(ZDecimal oSAmount, ZDecimal exchangeRate, OrgHeader orgBizO)
		{
			Overpayment newOVP = GetNewOverpayment();
			newOVP.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newOVP.AH_OH = orgBizO.PK;
			newOVP.AH_Desc = AccountingConstants.MatchingDefaultDesc.OverpaymentDesc;
			newOVP.AH_InvoiceDate = ZDateTime.Now;
			newOVP.AH_DueDate = ZDateTime.Now;
			newOVP.AH_PostDate = ZDateTime.Now;

			newOVP.AH_GB = GlbBranch.CurrentBranch.PK;
			newOVP.AH_GE = GlbDepartment.CurrentDepartment.PK;
			newOVP.AH_TransactionCategory = "";
			newOVP.AH_TransactionCreatedByMatching = true;

			newOVP.AH_ExchangeRate = exchangeRate;
			newOVP.AH_OSExTaxAmount = oSAmount;
			newOVP.BindableOSAmount = oSAmount;
			// NewOVP.AH_LocalExTaxAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(OSAmount, ExchangeRate);
			// setting this should set Outstanding Amount too

			((IMatching)newOVP).OSPartialPaymentAmount = oSAmount;
			SetReadOnlyValues(newOVP, false);

			return newOVP;
		}

		#endregion

		#region CreateDiscount

		public Discount CreateDiscount(ZDecimal amount, OrgHeader orgBizO)
		{
			Discount newDSC = GetNewDiscount();
			newDSC.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newDSC.AH_OH = orgBizO.PK;
			newDSC.AH_Desc = AccountingConstants.MatchingDefaultDesc.DiscountDesc;
			newDSC.AH_InvoiceDate = ZDateTime.Now;
			newDSC.AH_DueDate = ZDateTime.Now;
			newDSC.AH_PostDate = ZDateTime.Now;
			newDSC.AH_GB = GlbBranch.CurrentBranch.PK;
			newDSC.AH_GE = GlbDepartment.CurrentDepartment.PK;
			newDSC.AH_TransactionCategory = "";
			newDSC.AH_TransactionCreatedByMatching = true;

			newDSC.AH_OSTotal = amount;

			newDSC.AH_InvoiceAmount = amount;
			newDSC.AH_OutstandingAmount = amount;
			newDSC.AH_ExchangeRate = 1M;

			SetReadOnlyValues(newDSC);
			return newDSC;
		}

		#endregion

		#region CreateExchangeDifference

		public ExchangeDifference CreateExchangeDifference(ZDecimal amount, OrgHeader orgBizO)
		{
			ExchangeDifference newEXX = GetNewExchangeDifference();
			newEXX.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newEXX.AH_OH = orgBizO.PK;
			newEXX.AH_Desc = AccountingConstants.MatchingDefaultDesc.ExchangeDifferenceDesc;
			newEXX.AH_InvoiceDate = ZDateTime.Now;
			newEXX.AH_DueDate = ZDateTime.Now;
			newEXX.AH_PostDate = ZDateTime.Now;
			newEXX.AH_GB = GlbBranch.CurrentBranch.PK;
			newEXX.AH_GE = GlbDepartment.CurrentDepartment.PK;
			newEXX.AH_TransactionCategory = "";
			newEXX.AH_TransactionCreatedByMatching = true;

			newEXX.AH_OSTotal = amount;
			newEXX.AH_InvoiceAmount = amount;
			newEXX.AH_OutstandingAmount = amount;
			newEXX.AH_ExchangeRate = 1M;
			SetReadOnlyValues(newEXX);
			return newEXX;
		}

		#endregion

		#region CreateBankFee

		public Journal CreateBankFee(OrgHeader orgBizO)
		{
			Journal newBankFee = GetNewBankFee();
			newBankFee.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newBankFee.AH_OH = orgBizO.PK;
			newBankFee.AH_Desc = AccountingConstants.MatchingDefaultDesc.BankFeeDesc;
			newBankFee.AH_AG = (Guid)AccountingConfigurationRegistry.Instance.FinanceChargesAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			newBankFee.DebitCreditSign = DebitCreditDataEntry.DR;
			newBankFee.AH_InvoiceDate = ZDateTime.Now.Date;
			newBankFee.AH_PostDate = ZDateTime.Now.Date;
			newBankFee.AH_GB = GlbBranch.CurrentBranch.PK;
			newBankFee.AH_GE = GlbDepartment.CurrentDepartment.PK;
			newBankFee.AH_TransactionCreatedByMatching = true;

			SetReadOnlyValues(newBankFee, false);
			return newBankFee;
		}

		#endregion

		#endregion

		#region Method for Setting Readonly

		void SetReadOnlyValues(TransactionHeader header)
		{
			SetReadOnlyValues(header, true);
		}

		void SetReadOnlyValues(TransactionHeader header, bool rxValue)
		{
			header.IsMiscellaneousTransaction = true;
			header.AH_RX_NKTransactionCurrency_ReadOnly = rxValue;
		}

		#endregion
	}
}
