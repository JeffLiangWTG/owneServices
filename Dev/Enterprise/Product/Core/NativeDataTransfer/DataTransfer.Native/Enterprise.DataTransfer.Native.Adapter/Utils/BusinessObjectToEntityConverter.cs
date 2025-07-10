using System;
using System.Data;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Exceptions;
using Enterprise.DataTransfer.Native.Common.Operations;

namespace Enterprise.DataTransfer.Native.Adapter.Utils
{
	public class BusinessObjectToEntityConverter
	{
		public BusinessObjectToEntityConverter()
		{
			entityRepository = new RetrieveOperation(new AncillaryImportServices { RequiresAdditionOfActionEqualsMerge = true });
		}
		readonly RetrieveOperation entityRepository;

		#region Dependency Injected

		public IDefinitionFinder DefinitionFinder { get; set; }

		#endregion

		public IEntity GetEntity(RowID rowID, string entitySetOverride = null, Func<DataTable, DataRow> filterOnMultiRowResult = null)
		{
			var info = entitySetOverride != null
				? DefinitionFinder.FindByEntitySetName(entitySetOverride)
				: DefinitionFinder.FindByTopTableName(rowID.TableName);

			return GetEntity(rowID, info, filterOnMultiRowResult);
		}

		IEntity GetEntity(RowID rowID, EntitySetDefinition info, Func<DataTable, DataRow> filterOnMultiRowResult)
		{
			var entity = entityRepository.FindByInternalPK(rowID.PK, info.Root, filterOnMultiRowResult) ?? throw new RowNotFoundException(rowID.TableName, rowID.PK);

			entity.Action = EntityAction.MERGE;

			return entity;
		}
	}
}
