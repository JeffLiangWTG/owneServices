using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocAccountingVoucher : DocumentWrapper, IGenericTransactionHeaderPlugIn
	{
		BusinessObjectFactory fFactory;
		VoucherProviderFactory fVoucherFactory;
		internal VoucherProvider VoucherProvider;

		DocAccountingVoucher(VoucherProvider voucherProvider, BusinessObjectFactory factory)
			: base(voucherProvider, factory)
		{
			this.VoucherProvider = voucherProvider;
		}

		public override string ToString()
		{
			return "";
		}

		public static DocAccountingVoucher New(AccTransactionHeader transaction, BusinessObjectFactory factory)
		{
			if (transaction == null)
			{
				return null;
			}
			else
			{
				VoucherProvider provider = new VoucherProviderFactory(factory).GetProvider(transaction);
				DocAccountingVoucher wrapperToReturn = new DocAccountingVoucher(provider, factory);
				wrapperToReturn.fFactory = transaction.Factory;
				return wrapperToReturn;
			}
		}

		public static DocAccountingVoucher New(VoucherProvider voucherProvider, BusinessObjectFactory factory)
		{
			if (voucherProvider == null)
			{
				return null;
			}
			else
			{
				DocAccountingVoucher wrapperToReturn = new DocAccountingVoucher(voucherProvider, factory);
				wrapperToReturn.fFactory = factory;
				return wrapperToReturn;
			}
		}

		public ZInt Period
		{
			get
			{
				return GetPeriodNoOnly(VoucherProvider.Period);
			}
		}

		public ZInt PeriodYear
		{
			get
			{
				AccountingPeriodCalculator fAccountingPeriodCalculator = new AccountingPeriodCalculator(fFactory);
				return fAccountingPeriodCalculator.GetLastDayForPeriod(VoucherProvider.Period).Year;
			}
		}

		public ZString VoucherNumber
		{
			get
			{
				return VoucherProvider.VoucherLines[0].VoucherNumberPrefix + VoucherProvider.VoucherLines[0].VoucherType.Replace("-", "") + VoucherProvider.VoucherLines[0].VoucherNumber;
			}
		}

		public DocAccountingVoucherLineCollection VoucherLines
		{
			get
			{
				DocAccountingVoucherLineCollection fVoucherLines = new DocAccountingVoucherLineCollection(Factory);

				foreach (VoucherLine line in GetPrintingVoucherLines())
				{
					fVoucherLines.Add(DocAccountingVoucherLine.New(line, line.Factory));
				}
				return fVoucherLines;
			}
		}

		public DocAccountingVoucherLineCollection VoucherLinesGroupByGLAccountCurrencyDescription
		{
			get
			{
				var fvoucherlines = new DocAccountingVoucherLineCollection(Factory);

				var lines = GetPrintingVoucherLines();
				var filteredLines = lines.Where(x => !string.IsNullOrEmpty(x.GLAccountNumber));

				var groupedLines = GetGroupedVoucherLines(filteredLines).ToList();
				groupedLines.AddRange(lines.Where(x => string.IsNullOrEmpty(x.GLAccountNumber)));

				foreach (var line in groupedLines.OrderBy(x => x.IsCredit))
				{
					fvoucherlines.Add(DocAccountingVoucherLine.New(line, Factory));
				}
				return fvoucherlines;
			}
		}

		VoucherLine[] GetGroupedVoucherLines(IEnumerable<VoucherLine> lines)
		{
			var groupedLines = lines.GroupBy(x => new { x.GLAccountNumber, x.Description, x.CurrencyCode, x.IsDebit })
				.Select(y =>
				{
					var voucherLine = lines.First().TransactionHeader != null ? new VoucherLine(lines.First().TransactionHeader) : new VoucherLine();
					voucherLine.AccountPK = y.First().AccountPK;
					voucherLine.Description = y.First().Description;
					voucherLine.AdditionalAccountDescription = y.First().AdditionalAccountDescription;
					voucherLine.CurrencyCode = y.First().CurrencyCode;
					voucherLine.ExchangeRate = y.First().ExchangeRate;
					voucherLine.CreditAmount = y.Sum(z => z.CreditAmount);
					voucherLine.DebitAmount = y.Sum(z => z.DebitAmount);
					voucherLine.OSCreditAmount = y.Sum(z => z.OSCreditAmount);
					voucherLine.OSDebitAmount = y.Sum(z => z.OSDebitAmount);
					voucherLine.BranchCode = y.First().BranchCode;
					voucherLine.DepartmentCode = y.First().DepartmentCode;

					return voucherLine;
				});
			return groupedLines.ToArray();
		}

		public ZString VoucherPostDateYear
		{
			get { return VoucherPostDate.Year.ToString(); }
		}

		public ZString VoucherPostDateMonth
		{
			get { return VoucherPostDate.Month.ToString(); }
		}

		public ZString VoucherPostDateDay
		{
			get { return VoucherPostDate.Day.ToString(); }
		}

		public ZDateTime VoucherPostDate
		{
			get
			{
				return VoucherProvider.PostDate;
			}
		}

		public ZString PostedBy
		{
			get
			{
				return VoucherProvider.PostedBy;
			}
		}

		public ZString EnterBy { get { return VoucherProvider.EnterBy; } }

		public ZString Reviewer { get { return VoucherProvider.Reviewer; } }

		public ZString Cashier { get { return VoucherProvider.Cashier; } }

		public ZByte NumberOfSupportingDocuments { get { return VoucherProvider.NumberOfSupportingDocuments; } }

		public ZString VoucherPrintedBy
		{
			get
			{
				return GlbStaff.CurrentUser.GS_FullName;
			}
		}

		public ZString TransactionBranchCode
		{
			get { return VoucherProvider.BranchCode; }
		}

		public ZString TransactionCompanyName
		{
			get { return VoucherProvider.CompanyName; }
		}

		public ZDecimal TotalDebitAmount
		{
			get { return VoucherProvider.TotalDebitAmount; }
		}

		public ZDecimal TotalCreditAmount
		{
			get { return VoucherProvider.TotalCreditAmount; }
		}

		protected VoucherProviderFactory VoucherFactory
		{
			get
			{
				if (fVoucherFactory == null)
				{
					fVoucherFactory = new VoucherProviderFactory(fFactory);
				}
				return fVoucherFactory;
			}
		}

		public ZInt GetPeriodNoOnly(ZInt period)
		{
			if (period < 0)
			{
				return 0;
			}
			else
			{
				ZString periodNo = period.ToString();
				ZString truncatedPeriod = periodNo.Right(2);
				return ZInt.Parse(truncatedPeriod);
			}
		}

		GenericTransactionHeaderSupporter IGenericTransactionHeaderPlugIn.HeaderSupporter
		{
			get { return fAccountingVoucherSupporter ?? (fAccountingVoucherSupporter = new DocAccountingVoucherSupporter(this)); }
		}
		DocAccountingVoucherSupporter fAccountingVoucherSupporter;

		IEnumerable<VoucherLine> GetPrintingVoucherLines() => VoucherProvider?.VoucherLines.Where(x => x.DebitAmount != 0 || x.CreditAmount != 0) ?? Array.Empty<VoucherLine>();

		class DocAccountingVoucherSupporter : GenericTransactionHeaderSupporter
		{
			public DocAccountingVoucherSupporter(DocAccountingVoucher parent)
			{
				Parent = parent;
			}
			protected readonly DocAccountingVoucher Parent;

			protected internal override ZString GetTransactionType()
			{
				return Parent.VoucherProvider.TransactionType;
			}

			protected internal override DocAccountingVoucher GetVoucher()
			{
				return Parent;
			}
		}
	}
}
