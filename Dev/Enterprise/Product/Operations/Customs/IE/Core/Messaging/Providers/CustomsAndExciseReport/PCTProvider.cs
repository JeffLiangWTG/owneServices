using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class PCTProvider : IXlsxProvider
	{
		public PCTProvider(PCTMessage jsonObject)
		{
			this.jsonObject = jsonObject;
		}
		readonly PCTMessage jsonObject;

		public ZString Timestamp => jsonObject.Timestamp;

		[XlsxField(1, "EORI")]
		public ZString Eori => jsonObject.Eori;

		[XlsxField(3, "Period")]
		public ZDateTime Period
		{
			get
			{
				new ZString(jsonObject.Period).TryParseToDate(out var requestDate);
				return requestDate;
			}
		}

		[XlsxField(4, "Paid Orders")]
		public IReadOnlyCollection<PctPaidOrderProvider> PaidOrders => paidOrders ??= jsonObject.PaidOrders?.Select(x => new PctPaidOrderProvider(x)).ToArray() ?? Array.Empty<PctPaidOrderProvider>();
		IReadOnlyCollection<PctPaidOrderProvider> paidOrders;

		public ZBool HasPaidOrders => jsonObject.PaidOrders?.Count > 0;
	}
}
