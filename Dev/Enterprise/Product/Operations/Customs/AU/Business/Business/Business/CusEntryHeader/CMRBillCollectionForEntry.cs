
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRBillCollectionForEntry : BillCollectionForEntry
	{
		public CMRBillCollectionForEntry(Customs.Business.CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		public override void PopulateBills()
		{
			this.RemoveAll();
			foreach (BaseJobComInvoiceHeader invoice in entryHeader.InvoiceHeaders)
			{
				if (invoice.Bill != null)
				{
					if (invoice.Bill.ChildBills.Count > 0)
					{
						foreach (Bill bill in (BusinessObjectCollection)invoice.Bill.ChildBills)
						{
							if (!this.Contains(bill))
							{
								Add(bill);
							}
						}
					}
					else
					{
						if (!this.Contains(invoice.Bill))
						{
							Add(invoice.Bill);
						}
					}
				}
			}
			if (this.Count == 0)
			{
				AddRange(declaration.LowestBills);
			}
			this.Sort(Bill.Schema.CU_BillUniqueCode, ListSortDirection.Ascending);
		}
	}
}
