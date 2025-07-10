using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class PCIProvider : IXlsxProvider
	{
		public PCIProvider(PCIMessage jsonObject)
		{
			this.jsonObject = jsonObject;
		}
		readonly PCIMessage jsonObject;

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
		public IReadOnlyCollection<PciPaidOrderProvider> PaidOrders => paidOrders ??= jsonObject.PaidOrders?.Select(x => new PciPaidOrderProvider(x)).ToArray() ?? Array.Empty<PciPaidOrderProvider>();
		IReadOnlyCollection<PciPaidOrderProvider> paidOrders;

		public ZBool HasPaidOrders => jsonObject.PaidOrders?.Count > 0;
	}
}
