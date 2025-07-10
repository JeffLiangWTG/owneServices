using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public sealed class DailyBreakdownProvider : IXlsxProvider
	{
		public DailyBreakdownProvider(DailyBreakdown dailyBreakdown)
		{
			this.dailyBreakdown = Argument.NotNull(dailyBreakdown, nameof(dailyBreakdown));
		}

		readonly DailyBreakdown dailyBreakdown;

		[XlsxField(1, "Date")]
		public ZDateTime Date
		{
			get
			{
				new ZString(dailyBreakdown.Date).TryParseToDate(out var dailyBreakdownDate);
				return dailyBreakdownDate;
			}
		}

		[XlsxField(2, "Tax Total")]
		public ZDecimal TaxTotal => dailyBreakdown.TaxTotal;
	}
}
