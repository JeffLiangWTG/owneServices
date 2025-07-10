using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.DocumentWrappers
{
	public class DocJobRevenueJournal : DocTransactionHeader
	{
		DocJobRevenueJournal(JobRevenueJournal journal, BusinessObjectFactory factoryToWrap)
			: base(journal, factoryToWrap)
		{
		}

		public static DocJobRevenueJournal New(JobRevenueJournal journal, BusinessObjectFactory factoryToWrap)
		{
			if (journal == null)
			{
				return null;
			}
			else
			{
				return new DocJobRevenueJournal(journal, factoryToWrap);
			}
		}

		JobRevenueJournal Journal
		{
			get { return (JobRevenueJournal)WrappedObject; }
		}

		public DocGenericTransactionLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new DocGenericTransactionLineCollection(Journal.Factory);
					foreach (JobRevenueJournalLine line in Journal.JournalLines)
					{
						lines.Add(DocGenericTransactionLine.New(line, Factory));
					}
				}
				return lines;
			}
		}
		DocGenericTransactionLineCollection lines;

		protected override DocGenericTransactionLineCollection GetLinesForInvoiceCore()
		{
			return Lines;
		}

		#region Custom Fields

		protected override ZDecimal GetTotalDebitAmount() => Lines.Cast<DocGenericTransactionLine>().Sum(line => line.DebitAmountDecimal);

		protected override ZDecimal GetTotalCreditAmount() => Lines.Cast<DocGenericTransactionLine>().Sum(line => line.CreditAmountDecimal);

		#endregion
	}
}
