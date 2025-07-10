using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class CFXVoucherLineProvider : VoucherLineProvider
	{
		public CFXVoucherLineProvider(AccTransactionLines transactionLine)
			: base(transactionLine)
		{
		}

		public VoucherLine VoucherCFXLine
		{
			get
			{
				if (fVoucherCFXLine == null)
				{
					fVoucherCFXLine = new VoucherLine(TransactionHeader);
					SetVoucherCFXLine(fVoucherCFXLine);
				}
				return fVoucherCFXLine;
			}
		}

		protected override AccGLHeader GetGLAccountFromLine()
		{
			AccGLHeader gLAccount = null;
			if (TransactionLine.GLHeader != null)
			{
				gLAccount = TransactionLine.GLHeader;
			}
			else if (TransactionLine.AL_AC.IsValid)
			{
				if (TransactionLine.AL_LineType == TransactionLineTypes.Revenue)
				{
					gLAccount = TransactionLine.ChargeCode.RevenueAccount;
				}
				else if (TransactionLine.AL_LineType == TransactionLineTypes.Cost)
				{
					gLAccount = TransactionLine.ChargeCode.CostAccount;
				}
				else if (TransactionLine.AL_LineType == TransactionLineTypes.WIP)
				{
					gLAccount = TransactionLine.ChargeCode.WIPAccount;
				}
				else if (TransactionLine.AL_LineType == TransactionLineTypes.Accrual)
				{
					gLAccount = TransactionLine.ChargeCode.AccrualAccount;
				}
			}
			return gLAccount;
		}

		VoucherLine fVoucherCFXLine;

		void SetVoucherCFXLine(VoucherLine line)
		{
			line.AccountPK = GetCFXAccountPK();
			line.AdditionalAccountDescription += GetOrganisationCodeFromLine();
			line.VoucherType = GetVoucherType();
			line.VoucherNumber = GetVoucherNumber();
			line.DebitAmount = GetDebitCFXAmount();
			line.CreditAmount = GetCreditCFXAmount();
			line.OSDebitAmount = GetOSDebitCFXAmount();
			line.OSCreditAmount = GetOSCreditCFXAmount();
			line.VoucherDate = GetVoucherDate();
			line.Description = GetDescription();
			line.CurrencyCode = GetCurrencyCode();
			line.ForeignCurrencyAmount = GetForeignCurrencyAmount();
			line.ExchangeRate = GetExchangeRate();
			line.DepartmentCode = GetDepartmentCode();
			line.BranchCode = GetBranchCode();
		}

		ZGuid GetCFXAccountPK()
		{
			ZGuid accountPK = ZGuid.Empty;
			var transactionType = TransactionHeader.AH_TransactionType;

			if (transactionType == TransactionTypes.Journal && GLControlAccounts.Instance.CFXAccount == null)
			{
				throw new MissingGLHeaderException(AccountingConfigurationRegistry.Instance.CFXAccount.Caption);
			}
			if (transactionType == TransactionTypes.JobRevenueJournal && GLControlAccounts.Instance.JobRevenueJournalControlAccount == null)
			{
				throw new MissingGLHeaderException(AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.Caption);
			}

			AccGLHeader gLAccount = transactionType == TransactionTypes.Journal ? GLControlAccounts.Instance.CFXAccount : GLControlAccounts.Instance.JobRevenueJournalControlAccount;
			if (gLAccount != null)
			{
				accountPK = gLAccount.PK;
			}
			return accountPK;
		}

		ZDecimal GetCreditCFXAmount()
		{
			return VoucherDebitCreditLookUp.GetDebit();
		}

		ZDecimal GetDebitCFXAmount()
		{
			return VoucherDebitCreditLookUp.GetCredit();
		}

		ZDecimal GetOSCreditCFXAmount()
		{
			return VoucherDebitCreditLookUp.GetOSDebit();
		}

		ZDecimal GetOSDebitCFXAmount()
		{
			return VoucherDebitCreditLookUp.GetOSCredit();
		}
	}
}
