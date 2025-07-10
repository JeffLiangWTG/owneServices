using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class DSRProvider : IXlsxProvider
	{
		public DSRProvider(DSRMessage jsonObject)
		{
			this.jsonObject = jsonObject;
		}
		readonly DSRMessage jsonObject;

		public ZString Timestamp => jsonObject.Timestamp;

		[XlsxField(1, "EORI")]
		public ZString Eori => jsonObject.Eori;

		[XlsxField(2, "Date")]
		public ZDateTime Date
		{
			get
			{
				new ZString(jsonObject.Date).TryParseToDate(out var requestDate);
				return requestDate;
			}
		}

		[XlsxField(4, "Tax Total")]
		public ZDecimal TaxTotal => jsonObject.TaxTotal;

		[XlsxField(5, "Tax Breakdowns")]
		public IReadOnlyCollection<TaxBreakdownProvider> TaxBreakdowns => taxBreakdowns ??= jsonObject.TaxBreakdowns?.Select(x => new TaxBreakdownProvider(x)).ToArray() ?? Array.Empty<TaxBreakdownProvider>();
		IReadOnlyCollection<TaxBreakdownProvider> taxBreakdowns;

		public ZBool HasTaxBreakdowns => jsonObject.TaxBreakdowns?.Count > 0;
	}
}
