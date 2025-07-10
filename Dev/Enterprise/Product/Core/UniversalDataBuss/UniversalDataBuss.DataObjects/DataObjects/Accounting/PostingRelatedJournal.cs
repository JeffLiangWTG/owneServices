using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public partial class PostingRelatedJournal : IDataObject
	{
		public PostingRelatedJournal()
		{
			PostingJournalDetailCollection = new List<PostingJournalDetail>();
		}

		public PostingRelatedJournal(IDataObjectWriterStrategy strategy)
			: this()
		{
			SetWriterStrategy(strategy);
		}

		[MaxLength(2)]
		public ZString Ledger { get; set; }

		[MaxLength(3)]
		public ZString TransactionCategory { get; set; }

		[MaxLength(3)]
		public ZString TransactionType { get; set; }

		public ZDecimal? ExchangeRate { get; set; }
		public GLAccount GLAccount { get; set; }
		public Currency LocalCurrency { get; set; }
		public Currency OSCurrency { get; set; }
		public OrganizationReference Organization { get; set; }
		public ZDecimal? LocalTotal { get; set; }
		public ZDecimal? OSTotal { get; set; }
		public ZDecimal? OutstandingAmount { get; set; }
		public ZDateTime? DueDate { get; set; }
		public ZDateTime? FullyPaidDate { get; set; }
		public ZDateTime? InvoiceDate { get; set; }
		public ZDateTime? PostDate { get; set; }
		[MaxLength(1024), AllowLineControlWhiteSpace]
		public ZString? Description { get; set; }
		public ZBool? IsCancelled { get; set; }
		public List<PostingJournalDetail> PostingJournalDetailCollection { get; private set; }
	}
}
