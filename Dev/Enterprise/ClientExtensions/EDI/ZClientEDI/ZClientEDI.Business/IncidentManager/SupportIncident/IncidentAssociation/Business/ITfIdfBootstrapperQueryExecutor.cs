using System;
using System.Collections.Generic;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public interface ITfIdfBootstrapperQueryExecutor
	{
		void UpdateTfidfPairs(IEnumerable<TfidfPairDataTransferObject> updatePairs, int batchSize);
		IEnumerable<Guid> GetIncidentGuidsBetween(DateTime fromDate, DateTime toDate);
	}
}