using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public interface ISimilarIncidentRepository
	{
		BusinessObjectFactory Factory { get; }

		int RepositoryVersion { get; }

		IEnumerable<IncidentSimilarityMatrix> SearchStoredIncidentSimilarities(SimilarIncidentSearchOptions options);

		IEnumerable<double> GetIdfVector();
		ZGuid SetIdfVector(ICollection<double> idf);

		IDictionary<string, int> GetTokenPairs();
		void SetTokenPairs(IDictionary<string, int> tokenPairs);
		int GetLatestVersion();

		void UpdateTfIdfTimestamps(IDictionary<Guid, DateTime> timestamps);
	}
}
