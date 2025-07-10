using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public interface ISimilarityMatrixBootstrapperQueryExecutor : IIncidentAssociationQuery
	{
		void Commit(IEnumerable<SimilarityMatrixDataTransferObject> similarityMatrixData, ZGuid tfidfGuid, int batchSize = 1000);
		int DeleteExistingRows(Guid incidentGuid, int newVersion);
	}
}
