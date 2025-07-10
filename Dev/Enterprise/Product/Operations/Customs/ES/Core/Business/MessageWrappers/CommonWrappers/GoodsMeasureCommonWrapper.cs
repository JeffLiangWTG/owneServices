using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class GoodsMeasureCommonWrapper : IGoodsMeasureCommon
	{
		public GoodsMeasureCommonWrapper(ZDecimal grossWeight, ZDecimal netWeight)
		{
			GrossWeight = grossWeight;
			NetWeight = netWeight;
		}

		public GoodsMeasureCommonWrapper(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
			Argument.GreaterThan(entryLine.InvoiceLines.Count, 0, nameof(entryLine.InvoiceLines));

			GrossWeight = entryLine.EffectiveGrossWeight.InKilogramsSafe.Round(WeightMaxDecimals);
			NetWeight = entryLine.EffectiveCustomsWeight.InKilogramsSafe.Round(WeightMaxDecimals);
		}

		protected virtual int WeightMaxDecimals => 3;

		protected readonly CusEntryLine entryLine;

		public ZDecimal GrossWeight { get; }

		public ZDecimal NetWeight { get; }
	}
}
