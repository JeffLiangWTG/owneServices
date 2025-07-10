using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public abstract class SingleDecLineProvider : ImportDecLineProvider, ISingleDecLine
	{
		protected SingleDecLineProvider(CusEntryLine entryLine) : base(entryLine)
		{
		}

		public decimal InvoiceAmount => CachedValueHelper.GetValue(ref invoiceAmount, () => InvoiceLines.Sum(x => x.JI_LinePrice));
		CachedValue<decimal> invoiceAmount;

		public IAmount ForeignTradeStatisticsAmount => CachedValueHelper.GetValue(ref foreignTradeStatisticsAmount, () =>
		{
			var invoiceLine = InvoiceLines.FirstOrDefault(l => l.JI_CustomsSecondQuantity != ZDecimal.Zero);
			if (invoiceLine != null)
			{
				var quantity = InvoiceLines.Sum(x => x.JI_CustomsSecondQuantity);
				return new AmountProvider(quantity, invoiceLine.JI_CustomsSecondUnitQty);
			}
			else
			{
				return null;
			}
		});
		CachedValue<IAmount> foreignTradeStatisticsAmount;
	}
}
