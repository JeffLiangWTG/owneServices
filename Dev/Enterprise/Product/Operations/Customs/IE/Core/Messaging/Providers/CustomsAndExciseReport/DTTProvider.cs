using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class DTTProvider : IXlsxProvider
	{
		public DTTProvider(DTTMessage jsonObject)
		{
			this.jsonObject = jsonObject;
		}
		readonly DTTMessage jsonObject;

		public ZString Timestamp => jsonObject.Timestamp;

		[XlsxField(1, "EORI")]
		public ZString Eori => jsonObject.Eori;

		[XlsxField(3, "Day")]
		public ZDateTime Day
		{
			get
			{
				new ZString(jsonObject.Day).TryParseToDate(out var requestDate);
				return requestDate;
			}
		}

		[XlsxField(4, "Tax Details")]
		public IReadOnlyCollection<TaxDetailProvider> TaxDetails => taxDetails ??= jsonObject.TaxDetails?.Select(x => new TaxDetailProvider(x)).ToArray() ?? Array.Empty<TaxDetailProvider>();
		IReadOnlyCollection<TaxDetailProvider> taxDetails;

		public ZBool HasTaxDetails => jsonObject.TaxDetails?.Count > 0;
	}
}
