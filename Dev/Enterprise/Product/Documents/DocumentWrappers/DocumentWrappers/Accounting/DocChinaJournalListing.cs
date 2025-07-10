using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.DataInterface;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("used in documents DataContext")]
	public class DocChinaJournalListing : DocumentWrapper
	{
		readonly ChinaJournalListing ChinaJournalListing;

		DocChinaJournalListing(ChinaJournalListing chinaJournalListing, BusinessObjectFactory factory)
			: base(chinaJournalListing, factory)
		{
			this.ChinaJournalListing = chinaJournalListing;
		}

		public override string ToString()
		{
			return "";
		}

		public static DocChinaJournalListing New(ChinaJournalListing chinaJournalListing, BusinessObjectFactory factory)
		{
			return chinaJournalListing != null ? new DocChinaJournalListing(chinaJournalListing, factory) : null;
		}

		public ZInt Period
		{
			get
			{
				return new AccountingPeriodCalculator(Factory).GetPeriodFromDate(ChinaJournalListing.FromDate);
			}
		}

		public ZDateTime FromDate
		{
			get
			{
				return ChinaJournalListing.FromDate;
			}
		}
		public ZDateTime EndDate
		{
			get
			{
				return ChinaJournalListing.EndDate;
			}
		}

		public ZString Branch
		{
			get
			{
				return ChinaJournalListing.BranchCode;
			}
		}

		public DocChinaJournalListingLineCollection VoucherLines
		{
			get
			{
				var fVoucherLines = new DocChinaJournalListingLineCollection(Factory);

				if (ChinaJournalListing != null)
				{
					foreach (ChinaJournal line in ChinaJournalListing.AccountingVouchers)
					{
						fVoucherLines.Add(DocChinaJournalListingLine.New(line, line.Factory));
					}
				}
				return fVoucherLines;
			}
		}
	}
}
