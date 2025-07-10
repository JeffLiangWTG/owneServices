using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class PSRProvider : IXlsxProvider
	{
		public PSRProvider(PSRMessage jsonObject)
		{
			this.jsonObject = jsonObject;
		}
		readonly PSRMessage jsonObject;

		public ZString Timestamp => jsonObject.Timestamp;

		[XlsxField(1, "EORI")]
		public ZString Eori => jsonObject.Eori;

		[XlsxField(2, "Period")]
		public ZDateTime Period
		{
			get
			{
				new ZString(jsonObject.Period).TryParseToDate(out var requestDate);
				return requestDate;
			}
		}

		[XlsxField(3, "Tax Total")]
		public ZDecimal TaxTotal => jsonObject.TaxTotal;

		[XlsxField(4, "Tax Breakdowns")]
		public IReadOnlyCollection<TaxBreakdownProvider> TaxBreakdowns => taxBreakdowns ?? (taxBreakdowns = jsonObject.TaxBreakdowns?.Select(x => new TaxBreakdownProvider(x)).ToArray() ?? Array.Empty<TaxBreakdownProvider>());
		IReadOnlyCollection<TaxBreakdownProvider> taxBreakdowns;

		public ZBool HasTaxBreakdowns => jsonObject.TaxBreakdowns?.Count > 0;

		[XlsxField(5, "Daily Breakdowns")]
		public IReadOnlyCollection<DailyBreakdownProvider> DailyBreakdowns => dailyBreakdowns ?? (dailyBreakdowns = jsonObject.DailyBreakdowns?.Select(x => new DailyBreakdownProvider(x)).ToArray() ?? Array.Empty<DailyBreakdownProvider>());
		IReadOnlyCollection<DailyBreakdownProvider> dailyBreakdowns;

		public ZBool HasDailyBreakdowns => jsonObject.DailyBreakdowns?.Count > 0;
	}
}
