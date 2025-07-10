using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class UDRProvider : IXlsxProvider
	{
		public UDRProvider(UDRMessage jsonObject)
		{
			this.jsonObject = jsonObject;
		}
		readonly UDRMessage jsonObject;

		public ZString Timestamp => jsonObject.Timestamp;

		[XlsxField(1, "EORI")]
		public ZString Eori => jsonObject.Eori;

		[XlsxField(3, "Unpaid Orders")]
		public IReadOnlyCollection<UnpaidOrderProvider> UnpaidOrders => unpaidOrders ??= jsonObject.UnpaidOrders?.Select(x => new UnpaidOrderProvider(x)).ToArray() ?? Array.Empty<UnpaidOrderProvider>();
		IReadOnlyCollection<UnpaidOrderProvider> unpaidOrders;

		public ZBool HasUnpaidOrders => jsonObject.UnpaidOrders?.Count > 0;
	}
}
