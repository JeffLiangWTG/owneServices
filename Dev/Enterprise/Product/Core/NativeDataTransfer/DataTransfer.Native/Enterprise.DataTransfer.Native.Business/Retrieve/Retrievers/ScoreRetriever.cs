using System;
using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;

namespace Enterprise.DataTransfer.Native.Business.Retrieve.Retrievers
{
	public class ScoreRetriever : Retriever
	{
		internal ScoreRetriever(AncillaryImportServices sessionServices)
			: base(sessionServices)
		{
		}

		// Should be able to implement after Entity Matching
		protected override IEnumerable<IEntity> FindEntities(EntitySetDefinition definition, IEnumerable<EntityCriteria> criterias)
		{
			throw new NotImplementedException("Score based retrieve is not currently supported.");
		}
	}
}
