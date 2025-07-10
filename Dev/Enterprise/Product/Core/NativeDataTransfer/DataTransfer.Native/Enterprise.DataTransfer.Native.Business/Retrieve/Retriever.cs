using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using Enterprise.DataTransfer.Native.Common.Operations;
using Enterprise.DataTransfer.Native.Utils;

namespace Enterprise.DataTransfer.Native.Business.Retrieve
{
	public abstract class Retriever
	{
		protected Retriever(AncillaryImportServices sessionServices)
		{
			this.sessionServices = sessionServices;
		}
		protected AncillaryImportServices sessionServices;

		#region Dependency

		internal RetrieveOperation RetrieveOperation;

		#endregion

		public IEnumerable<IEntity> Retrieve(EntitySetDefinition definition, IEnumerable<EntityCriteria> criterias)
		{
			CheckCriterias(definition, criterias);
			return FindEntities(definition, criterias).Where(entity => entity != null);
		}

		public virtual void CheckCriterias(EntitySetDefinition definition, IEnumerable<EntityCriteria> criterias)
		{
			if (!criterias.Any())
			{
				throw new NativeXMLUserVisibleException("No Criteria elements");
			}

			foreach (var criteria in criterias)
			{
				if (criteria.EntityName.IsEmpty())
				{
					throw new NativeXMLUserVisibleException("Empty entity name");
				}

				if (!criteria.EntityName.StartsWith(definition.Root.EntityName))
				{
					throw new NativeXMLUserVisibleException(FormattableString.Invariant($"Criteria Entity=\"{criteria.EntityName}\" is not a valid Criteria element of {definition.Root.EntityName}"));
				}

				if (criteria.PropertyName.IsEmpty())
				{
					throw new NativeXMLUserVisibleException("Empty property name");
				}
			}
		}

		protected abstract IEnumerable<IEntity> FindEntities(EntitySetDefinition definition, IEnumerable<EntityCriteria> criterias);
	}
}
