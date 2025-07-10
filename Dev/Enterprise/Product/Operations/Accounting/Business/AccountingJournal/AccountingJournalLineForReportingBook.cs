using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public class AccountingJournalLineForReportingBook : AccountingJournalLine
	{
		public AccountingJournalLineForReportingBook(DataRow row, AccReportingBook reportingBook, AccTransactionLines transactionLine)
			 : base(transactionLine)
		{
			Argument.NotNull(row, nameof(dataRow));
			this.dataRow = row;
			this.reportingBook = reportingBook;
		}

		readonly AccReportingBook reportingBook;

		public ZGuid ReportingBookPK => reportingBook.PK;

		readonly DataRow dataRow;

		public ZString AlternateGLAccount => dataRow["AlternateGLAccount"].ToString();

		public ZString ParentAccountNum => dataRow["GLAccountNum"].ToString();

		public override GlbBranch Branch => Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, BranchCode);

		public ZString BranchCode => dataRow["BranchCode"].ToString();

		public ZString DeptCode => dataRow["DepartmentCode"].ToString();

		public override GlbDepartment Department => Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, DeptCode);

		[DecimalPlaces("LocalDecimals")]
		public ZDecimal LocalDebitAmount => (decimal)dataRow["GLAmountLocalDebit"] > 0 ? (decimal)dataRow["GLAmountLocalDebit"] : decimal.Zero;

		[DecimalPlaces("LocalDecimals")]
		public ZDecimal LocalCreditAmount => (decimal)dataRow["GLAmountLocalCredit"] > 0 ? (decimal)dataRow["GLAmountLocalCredit"] : decimal.Zero;

		[DecimalPlaces("LocalDecimals")]
		public ZDecimal LocalBalance => (decimal)dataRow["GLAmountLocalBalance"];

		[DecimalPlaces("LocalDecimals")]
		public ZDecimal GLAmountOSDebit => (decimal)dataRow["GLAmountOSDebit"] > 0 ? (decimal)dataRow["GLAmountOSDebit"] : decimal.Zero;

		[DecimalPlaces("LocalDecimals")]
		public ZDecimal GLAmountOSCredit => (decimal)dataRow["GLAmountOSCredit"] > 0 ? (decimal)dataRow["GLAmountOSCredit"] : decimal.Zero;

		public ZDateTime GlPostDate => (DateTime)dataRow["PostDate"];

		public ZString CurrencyCode => dataRow["Currency"].ToString();

		public override RefCurrency Currency => Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, CurrencyCode);

		public ZString ReportingBookPeriod => dataRow["PostPeriod"] is int ? ((int)dataRow["PostPeriod"]).ToString() : $"No period set up for post date {GlPostDate.Date.ToString()} in {reportingBook.CompanyOfPeriod.GC_Code} company.";

		public ZBool IsMissingPeriodForReportingBook => !(dataRow["PostPeriod"] is int);

		public ZString ORG => dataRow["OriginalAttribute_ORG"].ToString();

		public ZString OCG => dataRow["OriginalAttribute_OCG"].ToString();

		public ZString LFE => dataRow["OriginalAttribute_LFE"].ToString();

		public ZString LFO => dataRow["OriginalAttribute_LFO"].ToString();

		public ZString TIC => dataRow["OriginalAttribute_TIC"].ToString();

		public ZString SPR => dataRow["OriginalAttribute_SPR"].ToString();

		public ZString AlternateGLAccountDesc
		{
			get
			{
				var result = dataRow["AlternateGLAccountDescription"].ToString();
				if (AccountingMasterFilesRegistry.Instance.ReportingBookAccountingJournalPrintOption.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)
					.Cast<ReportingBookAccountingJournalPrintOption>().FirstOrDefault(x => x.ReportingBook == reportingBook.PK)?.DisplayAttribute ?? false)
				{
					result = result + (ORG.IsEmpty ? "" : $" ORG[{ORG}]")
						+ (OCG.IsEmpty ? "" : $" OCG[{OCG}]")
						+ (LFE.IsEmpty ? "" : $" LFE[{LFE}]")
						+ (LFO.IsEmpty ? "" : $" LFO[{LFO}]")
						+ (TIC.IsEmpty ? "" : $" TIC[{TIC}]")
						+ (SPR.IsEmpty ? "" : $" SPR[{SPR}]");
				}
				return result;
			}
		}
	}
}
