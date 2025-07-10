using System;
using System.Collections.Generic;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public interface IMetaIncidentSimilarityTfIdfProvider
	{
		IMetaIncidentSimilarityTfIdf FindByIncidentGuid(int version, Guid incidentMainPkGuid);
		IEnumerable<IMetaIncidentSimilarityTfIdf> FindByVectorGuids(IEnumerable<Guid> tfIdfPkGuids);
		IEnumerable<IMetaIncidentSimilarityTfIdf> Load(int version);

		int CountNewOrUpdatedIncidentTfIdfs(int version);
		int CountProcesedIncidentTfIdfs(int version);
		IEnumerable<IMetaIncidentSimilarityTfIdf> LoadTopNewOrUpdatedIncidentTfIdfs(int version, int topN);
		IEnumerable<List<IMetaIncidentSimilarityTfIdf>> LoadProcessedIncidentTfIdfsInChunks(int version, int chunkSize);

		void Save(IEnumerable<IMetaIncidentSimilarityTfIdf> metaISVs, int batchSize = 1000);
	}
}
