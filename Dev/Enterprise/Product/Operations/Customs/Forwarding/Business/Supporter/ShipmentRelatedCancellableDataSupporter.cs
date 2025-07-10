using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.Customs.Forwarding.Business
{
	public sealed class ShipmentRelatedCancellableDataSupporter : BaseRelatedCancellableDataSupporter, IShipmentRelatedCancellableDataSupporter
	{
		protected override void RegisterHandlersCore(List<LoadDataHandler> handlerList)
		{
			handlerList.Add((parent, value) => new CusInBondHeadersHandler().Load(parent, value));
		}
	}
}
