using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public interface IIncidentAssociationQuery
	{
		int NonQuery(string query, ZSqlParameterCollection parameters = null);
		IEnumerable<Dictionary<string, object>> QueryObjects(string query, ZSqlParameterCollection parameters = null);
		IEnumerable<Guid> GetIncidentGuidsBetween(DateTime fromDate, DateTime toDate);
		IEnumerable<Guid> GetNewAndUpdatedIncidentGuids(int version, DateTime fromDate, DateTime toDate);
		void UpdateTfIdfTimestamps(int version, IDictionary<Guid, DateTime> timestamps);
		IEnumerable<Guid> DeleteOldIncidentsFromIncidentSimilarityTfIdf(DateTime fromDate);
		void DeleteIncidentsFromIncidentSimilarityMatix(IEnumerable<Guid> incidents);
		void DeleteOldIncidentsFromIncidentSimilarityExclusion(DateTime fromDate);
	}
}
