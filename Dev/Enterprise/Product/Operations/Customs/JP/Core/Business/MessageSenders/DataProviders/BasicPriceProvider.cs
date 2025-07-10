using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Outbound;

namespace Enterprise.Customs.JP.Business
{
	public class BasicPriceProvider : IBasicPrice
	{
		public BasicPriceProvider(CusEntryLine cusEntryLine)
		{
			Argument.NotNull(cusEntryLine, nameof(cusEntryLine));
			this.cusEntryLine = cusEntryLine;
		}

		readonly CusEntryLine cusEntryLine;

		public decimal? Coefficient => null;

		public IMoney BasicPrice => TryGetMoneyProvider(cusEntryLine);

		MoneyProvider TryGetMoneyProvider(CusEntryLine entryLine) => entryLine != null && entryLine.IsInDatabase ? new MoneyProvider(entryLine, nameof(entryLine.FOB)) : null;
	}
}
