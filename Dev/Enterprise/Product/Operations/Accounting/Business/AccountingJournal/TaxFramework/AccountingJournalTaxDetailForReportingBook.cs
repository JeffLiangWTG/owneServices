using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public class AccountingJournalTaxDetailForReportingBook : AccountingJournalTaxDetail
	{
		protected AccountingJournalTaxDetailForReportingBook(BusinessObjectFactory factory, IGLMovementDetails glMovementDetails, AccReportingBook reportingBook, DataRow row)
			: base(factory, glMovementDetails)
		{
			dataRow = row;
			accReportingBook = reportingBook;
		}

		public static List<AccountingJournalTaxDetail> Create(BusinessObjectFactory factory, IReadOnlyCollection<IGLMovementDetails> collection, AccReportingBook reportingBook, DataRow[] rows)
		{
			var list = new List<AccountingJournalTaxDetail>();
			foreach (var element in collection)
			{
				var row = rows.FirstOrDefault(x => (string)x["GLAccountNum"] == element.GLAccount);
				var detail = new AccountingJournalTaxDetailForReportingBook(factory, element, reportingBook, row);
				list.Add(detail);
			}

			return list;
		}

		readonly DataRow dataRow;
		readonly AccReportingBook accReportingBook;

		public override ZString PostPeriod => string.IsNullOrEmpty(dataRow["PostPeriod"].ToString()) ? $"No period set up for post date {base.PostDate.ToString()} in {accReportingBook.CompanyOfPeriod.GC_Code} company." : dataRow["PostPeriod"].ToString();

		#region reporting book value

		public override ZBool IsMissingPeriodForReportingBook => !(dataRow["PostPeriod"] is int);

		public override ZString AlternateAccountNum => dataRow["OriginalAccount"].ToString();

		public ZString ORG => dataRow["OriginalAttribute_ORG"].ToString();

		public ZString OCG => dataRow["OriginalAttribute_OCG"].ToString();

		public ZString LFE => dataRow["OriginalAttribute_LFE"].ToString();

		public ZString LFO => dataRow["OriginalAttribute_LFO"].ToString();

		public ZString TIC => dataRow["OriginalAttribute_TIC"].ToString();

		public ZString SPR => dataRow["OriginalAttribute_SPR"].ToString();

		public override ZString AlternateAccountDesc
		{
			get
			{
				var result = dataRow["AlternateGLAccountDescription"] as string ?? string.Empty;
				if (AccountingMasterFilesRegistry.Instance.ReportingBookAccountingJournalPrintOption.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)
					.Cast<ReportingBookAccountingJournalPrintOption>().FirstOrDefault(x => x.ReportingBook == accReportingBook.PK)?.DisplayAttribute ?? false)
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

		#endregion
	}
}
