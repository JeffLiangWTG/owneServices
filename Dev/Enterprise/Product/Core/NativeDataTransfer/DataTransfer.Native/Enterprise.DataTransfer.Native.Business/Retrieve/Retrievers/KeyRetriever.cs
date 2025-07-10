using System.Collections.Generic;
using System.Linq;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using Enterprise.Integration;

namespace Enterprise.DataTransfer.Native.Business.Retrieve.Retrievers
{
	public class KeyRetriever : Retriever
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string")]
		const string MultipleCriteriaItemWarning = "Only a single criteria item can be passed for Key based retrieve.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string")]
		const string EntityNameNotMatchException = "Key based retrieval only supports searching Key for Top Level Entity. \"{0}\" is not Top Level Entity for Entity Set \"{1}\"";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string")]
		const string NonPrimaryKeyException = "Key based retrieval only supports Primary or Candidate keys as field name. \"{0}\" is not a primary or candidate key for table \"{1}\"";

		internal KeyRetriever(AncillaryImportServices sessionServices)
			: base(sessionServices)
		{
		}

		public override void CheckCriterias(EntitySetDefinition definition, IEnumerable<EntityCriteria> criterias)
		{
			base.CheckCriterias(definition, criterias);
			if (criterias.Count() > 1)
			{
				sessionServices.Logger.Warning(MultipleCriteriaItemWarning);
			}

			var table = definition.Root.Table;
			var candidateKeys = table.SingleColumnUniqueConstraints;

			foreach (var criteria in criterias)
			{
				if (definition.Root.EntityName != criteria.EntityName)
				{
					var errorMsg = string.Format(EntityNameNotMatchException, criteria.EntityName, definition.Name);
					throw new NativeXMLUserVisibleException(errorMsg);
				}

				// Check if Criteria Property is Single Column CandidateKey
				if (!candidateKeys.Any(item => item.HumanName == criteria.PropertyName))
				{
					var errorMsg = string.Format(NonPrimaryKeyException, criteria.PropertyName, criteria.EntityName);
					throw new NativeXMLUserVisibleException(errorMsg);
				}
			}
		}

		protected override IEnumerable<IEntity> FindEntities(EntitySetDefinition definition, IEnumerable<EntityCriteria> criterias)
		{
			var criteria = criterias.First();

			yield return RetrieveOperation.FindByCandidateKey(criteria, definition.Root);
		}
	}
}
