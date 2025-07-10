using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	public class DocAccountingVoucherLine : DocumentWrapper
	{
		protected VoucherLine VoucherLine;

		DocAccountingVoucherLine(VoucherLine voucherLine)
			: base(voucherLine, voucherLine.Factory)
		{
			this.VoucherLine = voucherLine;
		}

		public static DocAccountingVoucherLine New(VoucherLine voucherLine, BusinessObjectFactory factory)
		{
			return voucherLine != null ? new DocAccountingVoucherLine(voucherLine) : null;
		}

		AccGLHeader GLHeader
		{
			get { return Factory.Load<AccGLHeader>(VoucherLine.AccountPK); }
		}

		public ZString ParentGLAccountAndDescription
		{
			get
			{
				if (GLHeader == null)
				{
					return ZString.Empty;
				}

				return GLHeader.AG_AccountNum + " - " + GLHeader.AG_DescriptionMultilingual;
			}
		}

		public ZString GLAccountNumAndDescription
		{
			get
			{
				if (VoucherLine.TransactionHeader != null && VoucherLine.TransactionHeader.AH_Ledger == LedgerTypes.JobCosting && VoucherLine.TransactionHeader.AH_TransactionType == TransactionTypes.JobRevenueJournal
						&& (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.China || GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Taiwan))
				{
					return string.Format(CultureInfo.InvariantCulture, "{0} - {1} ({2}, {3})", VoucherLine.AccountNumber, VoucherLine.AdditionalAccountDescription, VoucherLine.BranchCode, VoucherLine.DepartmentCode);
				}
				else
				{
					return string.Format(CultureInfo.InvariantCulture, "{0} - {1}", VoucherLine.AccountNumber, VoucherLine.AdditionalAccountDescription);
				}
			}
		}

		public ZString Appendix
		{
			get
			{
				return VoucherLine.Appendix;
			}
		}

		public ZDecimal CreditAmount
		{
			get
			{
				return VoucherLine.CreditAmount;
			}
		}

		public ZDecimal DebitAmount
		{
			get
			{
				return VoucherLine.DebitAmount;
			}
		}

		public ZString TransactionDecription
		{
			get { return VoucherLine.Description; }
		}

		public ZDateTime VoucherDate
		{
			get
			{
				return VoucherLine.VoucherDate;
			}
		}

		public ZString VoucherNumber
		{
			get
			{
				return VoucherLine.VoucherNumber;
			}
		}

		public ZString VoucherType
		{
			get
			{
				return VoucherLine.VoucherType;
			}
		}

		public ZString CurrencyCode
		{
			get { return VoucherLine.CurrencyCode; }
		}

		public ZDecimal OSCreditAmount
		{
			get { return VoucherLine.OSCreditAmount; }
		}

		public ZDecimal OSDebitAmount
		{
			get { return VoucherLine.OSDebitAmount; }
		}

		public ZDecimal ForeignCurrencyAmount
		{
			get { return VoucherLine.ForeignCurrencyAmount; }
		}

		public ZDecimal ExchangeRate
		{
			get { return VoucherLine.ExchangeRate; }
		}

		public override string ToString()
		{
			return ZString.Empty;
		}
	}
}
