using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.Customs.Forwarding.Business
{
	public sealed class ConsolRelatedCancellableDataSupporter : BaseRelatedCancellableDataSupporter, IConsolRelatedCancellableDataSupporter
	{
		protected override void RegisterHandlersCore(List<LoadDataHandler> handlerList)
		{
			handlerList.Add((parent, value) => new CusInBondHeadersHandler().Load(parent, value));
			handlerList.Add((parent, value) => new JPAFRHeadersHandler().Load(parent, value));
			handlerList.Add((parent, value) => new AsycudaManifestHeadersHandler().Load(parent, value));
			handlerList.Add((parent, value) => new CusSCAOceanBillsHandler().Load(parent, value));
		}
	}
}
