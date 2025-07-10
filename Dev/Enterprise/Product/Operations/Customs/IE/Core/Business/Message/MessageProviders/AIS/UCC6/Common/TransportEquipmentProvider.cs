using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class TransportEquipmentProvider : IMTransportEquipment
	{
		public TransportEquipmentProvider(string containerId, string[] goodsReferences)
		{
			ContainerId = containerId;
			GoodsReferences = goodsReferences;
		}

		public string ContainerId { get; }

		public IReadOnlyCollection<string> GoodsReferences { get; }
	}
}
