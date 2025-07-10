using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public class EntryLineUniversalRate : Customs.Business.EntryLineUniversalRate
	{
		public EntryLineUniversalRate(CusEntryLine entryLine)
			: base(entryLine)
		{
		}

		protected override IEnumerable<(ZString uq, ZDecimal qty)> GetUnitOfMeasureValues()
		{
			foreach (JobComInvoiceLine invoiceLine in EntryLine.InvoiceLines)
			{
				var uq = invoiceLine.JI_CustomsUnitQty;
				if (!uq.IsEmpty)
				{
					yield return (uq, invoiceLine.JI_CustomsQuantity);
				}
				uq = invoiceLine.JI_CustomsSecondUnitQty;
				if (!uq.IsEmpty)
				{
					yield return (uq, invoiceLine.JI_CustomsSecondQuantity);
				}
			}
		}
	}
}
