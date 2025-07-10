using System;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Accounting.Export.Business
{
	public class TransactionHeaderRow
	{
		public Guid PK { get; set; }
		public TransactionInfo Info { get; set; }
		public OrganizationReference Organization { get; set; }
		public GLAccount GLAccount { get; set; }
		public GLAccount BankGLAccount { get; set; }
		public byte TransactionCount { get; set; }

		public TransactionHeaderRow(IDataObjectWriterStrategy strategy)
			: this(new TransactionInfo(strategy))
		{
		}

		public TransactionHeaderRow(TransactionInfo info)
		{
			PK = Guid.Empty;
			Info = info;
			if (Info.PostingJournalCollection == null)
			{
				Info.SetPostingJournalCollection(() => new System.Collections.Generic.List<PostingJournal>());
			}
		}

		public override string ToString()
		{
			return string.Format("PK={0}", PK);
		}
	}
}
