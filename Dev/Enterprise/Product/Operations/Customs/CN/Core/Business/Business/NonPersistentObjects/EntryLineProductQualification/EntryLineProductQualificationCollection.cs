using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CN.Business
{
	public class EntryLineProductQualificationCollection : NonPersistentBusinessObjectCollection<EntryLineProductQualification>
	{
		public EntryLineProductQualificationCollection(CusEntryLine cusEntryLine) : base(cusEntryLine.Factory)
		{
			this.cusEntryLine = Argument.NotNull(cusEntryLine, nameof(cusEntryLine));
		}
		readonly CusEntryLine cusEntryLine;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;

		public override void Load()
		{
			RemoveAndDeleteAll();

			var qualifications = cusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.CIQProductQualifications.Cast<CIQProductQualification>())
					.GroupBy(q => new { q.CSI_Code, q.CSI_ReferenceNumber, q.CSI_LineNo, q.CSI_UnitOfQuantity });

			var sequence = 1;

			foreach (var groupedQualifications in qualifications)
			{
				Add(new EntryLineProductQualification(cusEntryLine, groupedQualifications.ToList(), sequence++));
			}
		}
	}
}
