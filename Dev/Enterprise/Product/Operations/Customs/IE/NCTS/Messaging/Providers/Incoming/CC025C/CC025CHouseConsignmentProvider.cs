using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC025CHouseConsignmentProvider
	{
		public CC025CHouseConsignmentProvider(HouseConsignmentType02 houseConsignment)
		{
			houseConsignmentType = Argument.NotNull(houseConsignment, nameof(houseConsignment));
		}
		readonly HouseConsignmentType02 houseConsignmentType;

		public ZInt SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumberCached, () => int.TryParse(houseConsignmentType.SequenceNumber, out var tryParseIntResult) ? tryParseIntResult : 0);
		CachedValue<int> sequenceNumberCached;

		public ZString ReleaseType => houseConsignmentType.ReleaseType;

		public IReadOnlyCollection<CC025CConsignmentItemProvider> ConsignmentItems => consignmentItems ?? (consignmentItems = houseConsignmentType.ConsignmentItem?.Select(x => new CC025CConsignmentItemProvider(x)).ToArray() ?? Array.Empty<CC025CConsignmentItemProvider>());
		IReadOnlyCollection<CC025CConsignmentItemProvider> consignmentItems;
	}
}
