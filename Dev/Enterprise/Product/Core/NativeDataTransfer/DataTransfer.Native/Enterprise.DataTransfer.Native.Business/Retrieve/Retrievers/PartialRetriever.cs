using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;

namespace Enterprise.DataTransfer.Native.Business.Retrieve.Retrievers
{
	public class PartialRetriever : Retriever
	{
		internal PartialRetriever(AncillaryImportServices sessionServices)
			: base(sessionServices)
		{
		}

		protected override IEnumerable<IEntity> FindEntities(EntitySetDefinition definition, IEnumerable<EntityCriteria> criterias)
		{
			return RetrieveOperation.FindByCriterias(criterias, definition);
		}
	}
}
