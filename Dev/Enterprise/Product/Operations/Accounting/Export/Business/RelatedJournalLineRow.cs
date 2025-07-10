using System;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Export.Business
{
	public class RelatedJournalLineRow
	{
		public Guid PK { get; set; }
		public ZString Ledger { get; set; }
		public ZString TransactionCategory { get; set; }
		public ZString TransactionType { get; set; }
		public ZDateTime? DueDate { get; set; }
		public ZDecimal? ExchangeRate { get; set; }
		public ZDateTime? FullyPaidDate { get; set; }
		public GLAccount GLAccount { get; set; }
		public ZDecimal? LocalTotal { get; set; }
		public Currency LocalCurrency { get; set; }
		public OrganizationReference Organization { get; set; }
		public Currency OSCurrency { get; set; }
		public bool? IsCancelled { get; set; }
		public ZDecimal? OSTotal { get; set; }
		public ZDecimal? OutstandingAmount { get; set; }
		public ZDateTime? PostDate { get; set; }
		public ZDateTime? InvoiceDate { get; set; }
		public ZString? Description { get; set; }

		public override string ToString()
		{
			return string.Format((NoResString)"PK={0} Description={1} PostDate={2} LocalTotal={3} OSTotal={4}",
						PK, Description, PostDate, LocalTotal, OSTotal);
		}
	}
}
