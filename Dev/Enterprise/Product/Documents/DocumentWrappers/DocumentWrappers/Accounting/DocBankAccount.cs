using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocBankAccount : DocBaseWrapper
	{
		DocBankAccount(AccBankAccount accBankAccount, BusinessObjectFactory factoryToWrap)
			: base(accBankAccount, factoryToWrap)
		{
		}

		public static DocBankAccount New(BusinessObjectFactory factory, ZGuid pK)
		{
			return New(factory.Load<AccBankAccount>(pK), factory);
		}

		public static DocBankAccount New(AccBankAccount accBankAccount, BusinessObjectFactory factoryToWrap)
		{
			if (accBankAccount == null)
			{
				return null;
			}
			else
			{
				return new DocBankAccount(accBankAccount, factoryToWrap);
			}
		}

		public static DocBankAccount New(RefCurrency accountCurrency, ZGuid accountBranchPK, BusinessObjectFactory factoryToWrap)
		{
			var branch = factoryToWrap.Load<GlbBranch>(accountBranchPK);
			AccBankAccount bankAccount = AccBankAccount.GetDefaultReceiptBankAccount(accountCurrency.RX_Code, branch, factoryToWrap);

			if (bankAccount == null)
			{
				return null;
			}
			else
			{
				return new DocBankAccount(bankAccount, factoryToWrap);
			}
		}

		AccBankAccount AccBankAccount
		{
			get { return (AccBankAccount)WrappedObject; }
		}

		public override string ToString()
		{
			return Code;
		}

		protected override ZString DocManagerUniqueID
		{
			get { return Code; }
		}

		#region Bank Account Fields

		public ZString Description
		{
			get { return AccBankAccount.AB_Desc; }
		}

		public ZString AccountEFTUserID
		{
			get { return AccBankAccount.AB_AccountEFTUserID; }
		}

		public ZString AccountNum
		{
			get { return AccBankAccount.AB_AccountNum; }
		}

		public ZString UniqueAccountNum
		{
			get { return AccBankAccount.AB_FullAccountNumber; }
		}

		public DocGLAccount GLAccount
		{
			get { return DocGLAccount.New(AccBankAccount.GLHeader, Factory); }
		}

		public ZBool AllowAutoDDR
		{
			get { return AccBankAccount.AB_AllowAutoDDR; }
		}

		public ZString AutoDDRFormat
		{
			get { return AccBankAccount.AB_AutoDDRFormat; }
		}

		public ZString BankAbbreviation
		{
			get { return AccBankAccount.AB_BankAbbreviation; }
		}

		public ZString BankAddress
		{
			get { return AccBankAccount.AB_BankAddress; }
		}

		public ZString BankName
		{
			get { return AccBankAccount.AB_BankName; }
		}

		public ZString BSB
		{
			get { return AccBankAccount.AB_BSB; }
		}

		public ZDecimal ClosingBalance
		{
			get { return AccBankAccount.AB_ClosingBalance; }
		}

		public ZDecimal ClosingOSBalance
		{
			get { return AccBankAccount.AB_ClosingOSBalance; }
		}

		public ZString Code
		{
			get { return AccBankAccount.AB_Code; }
		}

		public ZString Desc
		{
			get { return AccBankAccount.AB_Desc; }
		}

		public ZBool DetailedDepositSlip
		{
			get { return AccBankAccount.AB_DetailedDepositSlip; }
		}

		public DocBranch Branch
		{
			get { return DocBranch.New(AccBankAccount.Branch, Factory); }
		}

		public DocCompany Company
		{
			get { return DocCompany.New(AccBankAccount.Company, Factory); }
		}

		public ZBool IsActive
		{
			get { return AccBankAccount.AB_IsActive; }
		}

		public ZBool IsDefaultReceiptBankAccount
		{
			get { return AccBankAccount.AB_IsDefaultReceiptBankAccount; }
		}

		public ZDecimal OpenBalance
		{
			get { return AccBankAccount.AB_OpenBalance; }
		}

		public ZDecimal OpenOSBalance
		{
			get { return AccBankAccount.AB_OpenOSBalance; }
		}

		public ZInt OpenPeriod
		{
			get { return AccBankAccount.AB_OpenPeriod; }
		}

		public DocCurrency Currency
		{
			get { return DocCurrency.New(AccBankAccount.AccountCurrency, Factory); }
		}

		public ZDecimal StatementBalance
		{
			get { return AccBankAccount.AB_StatementBalance; }
		}

		public ZString SWIFT
		{
			get { return AccBankAccount.AB_SWIFT; }
		}

		public ZString AccountName
		{
			get { return AccBankAccount.AB_BankAccountName; }
		}

		public ZString IBAN
		{
			get { return AccBankAccount.IBAN; }
		}
		#endregion
	}
}
