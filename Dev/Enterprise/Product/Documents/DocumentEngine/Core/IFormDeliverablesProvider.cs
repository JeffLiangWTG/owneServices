using System.Collections.Generic;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngine
{
	public interface IFormDeliverablesProvider
	{
		IReadOnlyCollection<IDeliverable> GetDeliverables(IStmMenuItem menuItem, IDocumentSupportable documentSupportable);
	}
}
