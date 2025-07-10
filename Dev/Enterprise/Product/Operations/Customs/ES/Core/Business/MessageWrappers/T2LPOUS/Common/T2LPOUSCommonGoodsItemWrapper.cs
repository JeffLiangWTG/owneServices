using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class T2LPOUSCommonGoodsItemWrapper : IT2LPOUSCommonGoodsItem
	{
		public T2LPOUSCommonGoodsItemWrapper(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
			Argument.GreaterThan(entryLine.InvoiceLines.Count, 0, nameof(entryLine.InvoiceLines));

			GoodsItemNumber = entryLine.CL_LineNumber;
		}
		protected readonly CusEntryLine entryLine;

		public ZInt GoodsItemNumber { get; }
	}
}
