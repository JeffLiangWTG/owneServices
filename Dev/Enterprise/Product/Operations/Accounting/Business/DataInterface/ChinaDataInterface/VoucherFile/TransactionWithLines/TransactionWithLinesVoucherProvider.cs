using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public abstract class TransactionWithLinesVoucherProvider : VoucherProvider
	{
		public TransactionWithLinesVoucherProvider(AccTransactionHeader invoice, IControlAccountProvider controlAccount)
			: base(invoice, controlAccount)
		{
			Transaction = invoice;
		}

		public TransactionWithLinesVoucherProvider(AccTransactionHeader invoice)
			: this(invoice, new ControlAccountProvider())
		{
		}

		public override VoucherLine[] VoucherLines
		{
			get { return fVoucherLines ?? (fVoucherLines = GetInvoiceVoucherLines()); }
#if DEBUG
			set
			{
				fVoucherLines = value;
			}
#endif
		}

		protected AccTransactionLinesCollection fLines;
		protected int fMaxVoucherLineNo;

		protected override ZDecimal GetControlCreditAmount()
		{
			return VoucherDebitCreditLookUp.GetDebit() == 0 ? 0m : VoucherDebitCreditLookUp.GetDebitGST() + VoucherDebitCreditLookUp.GetDebit() - VoucherDebitCreditLookUp.GetCreditGST();
		}

		protected override ZDecimal GetControlDebitAmount()
		{
			return VoucherDebitCreditLookUp.GetCredit() == 0 ? 0m : VoucherDebitCreditLookUp.GetCreditGST() + VoucherDebitCreditLookUp.GetCredit() - VoucherDebitCreditLookUp.GetDebitGST();
		}

		protected override ZDecimal GetOSCreditAmount()
		{
			return IsTaxApplied() ? (ZDecimal)(VoucherDebitCreditLookUp.GetOSCredit() - GetGSTOSCreditAmount()) : VoucherDebitCreditLookUp.GetOSCredit();
		}

		protected override ZDecimal GetOSDebitAmount()
		{
			return IsTaxApplied() ? (ZDecimal)(VoucherDebitCreditLookUp.GetOSDebit() - GetGSTOSDebitAmount()) : VoucherDebitCreditLookUp.GetOSDebit();
		}

		protected virtual ZDecimal GetGSTOSCreditAmount()
		{
			return VoucherDebitCreditLookUp.GetCreditGST() == 0 ? 0 : Env.CurrentCompany.ExchangeRate.LocalToForeign(VoucherDebitCreditLookUp.GetCreditGST(), GetExchangeRate(), GetCurrencyCode());
		}

		protected virtual ZDecimal GetGSTOSDebitAmount()
		{
			return VoucherDebitCreditLookUp.GetDebitGST() == 0 ? 0 : Env.CurrentCompany.ExchangeRate.LocalToForeign(VoucherDebitCreditLookUp.GetDebitGST(), GetExchangeRate(), GetCurrencyCode());
		}

		protected override ZGuid GetGLAccountPKFromTransactionHeader()
		{
			return ZGuid.Empty;
		}

		protected virtual ZGuid GetGSTAccountPK()
		{
			ControlAccountProvider.SetTransaction(Transaction);
			return ControlAccountProvider.GST;
		}

		protected virtual ZString GetGSTAccountDescription()
		{
			ControlAccountProvider.SetTransaction(Transaction);
			return GetLocalAccountDescription(ControlAccountProvider.GST);
		}

		protected VoucherLine GetGSTVoucherLine()
		{
			VoucherLine voucherLine = new VoucherLine(Transaction);
			voucherLine.AccountPK = GetGSTAccountPK();
			voucherLine.AccountDescription = GetGSTAccountDescription();
			voucherLine.VoucherType = VoucherTypeLookUp.GetVoucherType();
			voucherLine.VoucherNumber = VoucherNumberLookUp.GetVoucherNumber();
			voucherLine.DebitAmount = VoucherDebitCreditLookUp.GetDebitGST();
			voucherLine.CreditAmount = VoucherDebitCreditLookUp.GetCreditGST();
			voucherLine.VoucherDate = GetVoucherDate();
			voucherLine.Description = GetVoucherDescription();
			voucherLine.CurrencyCode = GetCurrencyCode();
			voucherLine.ForeignCurrencyAmount = GetForeignCurrencyAmount();
			voucherLine.ExchangeRate = GetExchangeRate();
			voucherLine.OSDebitAmount = GetGSTOSDebitAmount();
			voucherLine.OSCreditAmount = GetGSTOSCreditAmount();

			return voucherLine;
		}

		protected virtual VoucherLine[] GetInvoiceVoucherLines()
		{
			int count = 0;
			count = SetLineDetails(count);

			fVoucherLines[count] = new VoucherLine(Transaction);
			SetControlAccountVoucherLine(fVoucherLines[count]);
			count++;

			if (IsTaxApplied())
			{
				fVoucherLines[count] = GetGSTVoucherLine();
			}

			return (fVoucherLines.OrderBy(x => x.CreditAmount != 0)).ToArray();
		}

		protected virtual AccTransactionLinesCollection Lines
		{
			get
			{
				if (fLines == null)
				{
					ZQuery filter = new ZQuery(AccTransactionLinesSchema.AL_AH, Transaction.PK);
					filter.AddToFilter(AccTransactionLinesSchema.AL_GC, Transaction.AH_GC);
					fLines = new AccTransactionLinesCollection(Transaction.Factory, filter);
					fLines.Load();

					List<ZGuid> linesToRemove = (from AccTransactionLines line in fLines
												 where line.ChargeCode != null && line.ChargeCode.IsComment
												 select line.PK).ToList();

					foreach (ZGuid pK in linesToRemove)
					{
						fLines.Remove(pK);
					}
				}
				return fLines;
			}
		}

		protected override int MaxVoucherLineNo
		{
			get
			{
				fMaxVoucherLineNo = Lines.Count + 1;
				if (IsTaxApplied())
				{
					fMaxVoucherLineNo++;
				}
				return fMaxVoucherLineNo;
			}
		}

		protected int SetLineDetails(int count)
		{
			fVoucherLines = new VoucherLine[MaxVoucherLineNo];
			foreach (InvoiceVoucherLineProvider voucherLineProvider in from AccTransactionLines transactionLine in Lines
																																 select new InvoiceVoucherLineProvider(transactionLine))
			{
				voucherLineProvider.IncludeOrganisationCode = IncludeOrganisationCode;
				voucherLineProvider.IncludeJobNumber = IncludeJobNumber;

				if (voucherLineProvider != null)
				{
					fVoucherLines[count] = voucherLineProvider.VoucherLine;
					count++;
				}
			}
			return count;
		}

		bool IsTaxApplied()
		{
			return (Transaction.AH_GSTAmount != 0m);
		}
	}
}
