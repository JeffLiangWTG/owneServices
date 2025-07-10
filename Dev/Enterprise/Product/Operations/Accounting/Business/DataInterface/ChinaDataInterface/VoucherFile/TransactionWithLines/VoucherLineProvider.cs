using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public abstract class VoucherLineProvider
	{
		public VoucherLineProvider(AccTransactionLines transactionLine)
		{
			this.TransactionLine = transactionLine;
			this.TransactionHeader = transactionLine.TransactionHeader;
			Factory = transactionLine.Factory;
		}

		public VoucherLine VoucherLine
		{
			get
			{
				if (fVoucherLine == null)
				{
					fVoucherLine = new VoucherLine(TransactionHeader);
					SetVoucherLine(fVoucherLine);
				}
				return fVoucherLine;
			}
		}

		public ZBool IncludeTransactionLineDescription
		{
			get
			{
				return fIncludeTransactionLineDescription;
			}
			set
			{
				fIncludeTransactionLineDescription = value;
			}
		}

		ZBool fIncludeTransactionLineDescription;

		public ZBool IncludeOrganisationCode
		{
			get
			{
				return fIncludeOrganisationCode;
			}
			set
			{
				fIncludeOrganisationCode = value;
			}
		}

		ZBool fIncludeOrganisationCode;

		public ZBool IncludeJobNumber
		{
			get
			{
				return fIncludeJobNumber;
			}
			set
			{
				fIncludeJobNumber = value;
			}
		}

		ZBool fIncludeJobNumber;

		protected abstract AccGLHeader GetGLAccountFromLine();

		protected virtual ZString GetOrganisationCodeFromLine()
		{
			return ZString.Empty;
		}

		protected AccTransactionLines TransactionLine;
		protected AccTransactionHeader TransactionHeader;
		protected BusinessObjectFactory Factory;
		protected VoucherLine fVoucherLine;

		protected ZGuid GetAccountPK()
		{
			ZGuid accountPK = ZGuid.Empty;
			AccGLHeader gLAccount = GetGLAccountFromLine();

			if (gLAccount != null)
			{
				accountPK = gLAccount.PK;
			}

			return accountPK;
		}

		protected ZDateTime GetVoucherDate()
		{
			return TransactionLine.AL_PostDate;
		}

		protected ZDecimal GetDebitAmount()
		{
			return VoucherDebitCreditLookUp.GetDebit();
		}

		protected ZDecimal GetCreditAmount()
		{
			return VoucherDebitCreditLookUp.GetCredit();
		}

		protected ZDecimal GetOSCreditAmount()
		{
			return TransactionLine.AL_GSTVAT != 0m ? (ZDecimal)(VoucherDebitCreditLookUp.GetOSCredit() - GetGSTOSCreditAmount()) : VoucherDebitCreditLookUp.GetOSCredit();
		}

		protected ZDecimal GetOSDebitAmount()
		{
			return TransactionLine.AL_GSTVAT != 0m ? (ZDecimal)(VoucherDebitCreditLookUp.GetOSDebit() - GetGSTOSDebitAmount()) : VoucherDebitCreditLookUp.GetOSDebit();
		}

		ZDecimal GetGSTOSCreditAmount()
		{
			return VoucherDebitCreditLookUp.GetCreditGST() == 0
					? 0.0
					: (ZDecimal)(GetCurrencyCode() == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency
									? 0m
									: Env.CurrentCompany.ExchangeRate.LocalToForeign(VoucherDebitCreditLookUp.GetCreditGST(),
																					 GetExchangeRate(), GetCurrencyCode()));
		}

		ZDecimal GetGSTOSDebitAmount()
		{
			return VoucherDebitCreditLookUp.GetDebitGST() == 0
					? 0.0
					: (ZDecimal)(GetCurrencyCode() == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency
									? 0m
									: Env.CurrentCompany.ExchangeRate.LocalToForeign(VoucherDebitCreditLookUp.GetDebitGST(),
																					 GetExchangeRate(), GetCurrencyCode()));
		}

		protected ZString GetCurrencyCode()
		{
			return VoucherDebitCreditLookUp.GetCurrencyCode();
		}

		protected ZDecimal GetForeignCurrencyAmount()
		{
			return VoucherDebitCreditLookUp.GetForeignCurrencyAmount();
		}

		protected ZDecimal GetExchangeRate()
		{
			return VoucherDebitCreditLookUp.GetExchangeRate();
		}

		protected ZString GetBranchCode()
		{
			var branch = TransactionLine.Branch;
			return branch != null ? branch.GB_Code : ZString.Empty;
		}

		protected ZString GetDepartmentCode()
		{
			var department = TransactionLine.Department;
			return department != null ? department.GE_Code : ZString.Empty;
		}

		protected void SetVoucherLine(VoucherLine voucherLine)
		{
			voucherLine.AccountPK = GetAccountPK();
			voucherLine.VoucherType = GetVoucherType();
			voucherLine.VoucherNumber = GetVoucherNumber();
			voucherLine.DebitAmount = GetDebitAmount();
			voucherLine.CreditAmount = GetCreditAmount();
			voucherLine.OSDebitAmount = GetOSDebitAmount();
			voucherLine.OSCreditAmount = GetOSCreditAmount();
			voucherLine.VoucherDate = GetVoucherDate();
			voucherLine.Description = GetDescription();
			voucherLine.CurrencyCode = GetCurrencyCode();
			voucherLine.ForeignCurrencyAmount = GetForeignCurrencyAmount();
			voucherLine.ExchangeRate = GetExchangeRate();
			voucherLine.JobNumber = VoucherDebitCreditLookUp.GetJobNumber();
			voucherLine.InvoiceNumber = VoucherDebitCreditLookUp.GetInvoiceNumber();
			voucherLine.BranchCode = GetBranchCode();
			voucherLine.DepartmentCode = GetDepartmentCode();

			voucherLine.ChargeCode = VoucherDebitCreditLookUp.ChargeCode;
			voucherLine.ChargeCodeDesc = VoucherDebitCreditLookUp.ChargeCodeDesc;
			voucherLine.ChargeCodeLocalLangDesc = VoucherDebitCreditLookUp.ChargeCodeLocalLangDesc;
		}

		protected VoucherDebitCreditLookUp VoucherDebitCreditLookUp
		{
			get
			{
				if (fVoucherDebitCreditLookUp == null)
				{
					fVoucherDebitCreditLookUp = new VoucherDebitCreditLookUp(TransactionLine);
				}
				return fVoucherDebitCreditLookUp;
			}
		}

		VoucherDebitCreditLookUp fVoucherDebitCreditLookUp;

		protected ZString GetVoucherType()
		{
			if (VoucherTypeLookUp == null)
			{
				VoucherTypeLookUp = new VoucherTypeLookUp(TransactionLine.TransactionHeader);
			}
			return VoucherTypeLookUp.GetVoucherType();
		}

		VoucherTypeLookUp VoucherTypeLookUp;

		protected ZString GetVoucherNumber()
		{
			if (VoucherNumberLookUp == null)
			{
				VoucherNumberLookUp = new VoucherNumberLookUp(TransactionLine.TransactionHeader);
			}
			return VoucherNumberLookUp.GetVoucherNumber();
		}

		VoucherNumberLookUp VoucherNumberLookUp;

		protected VoucherDescriptionLookUp VoucherDescriptionLookUp;

		protected ZString GetDescription()
		{
			return GetDescription(TransactionLine.TransactionHeader);
		}

		protected ZString GetDescription(AccTransactionHeader transactionHeader)
		{
			if (VoucherDescriptionLookUp == null)
			{
				VoucherDescriptionLookUp = new VoucherDescriptionLookUp(transactionHeader);
			}

			ZString headerDescription = VoucherDescriptionLookUp.GetDescription();
			ZString transactionLineDescription = VoucherDebitCreditLookUp.GetTransactionLineDescription();
			ZString jobNumber = VoucherDebitCreditLookUp.GetJobNumber();

			return DataInterfaceUtils.GetVoucherDescription(headerDescription, transactionLineDescription, jobNumber, IncludeJobNumber);
		}
	}
}