using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using Enterprise.DataTransfer.Native.DB.Sql;

namespace Enterprise.DataTransfer.Native.Common.Converters
{
	/// <summary>
	/// Convert Entity Criteria to Db Criteria
	/// </summary>
	static class CriteriaConverter
	{
		public static Criteria Convert(IEntityDefinition definition, EntityCriteria criteria)
		{
			var propertyName = criteria.PropertyName;
			if (!definition.PropertyDefinitions.HasDefinition(propertyName))
			{
				throw new NativeXMLUserVisibleException(string.Format("There is no FieldName '{0}' in table '{1}'.", propertyName, definition.TableName));
			}

			var property = definition.PropertyDefinitions[propertyName];
			var column = property.ColumnDef;
			return new Criteria
			{
				ColumnDefinition = column,
				ColumnName = column.Name,
				TableName = definition.TableName,
				Value = criteria.Value
			};
		}

		public static IEnumerable<Criteria> Convert(EntitySetDefinition definition, IEnumerable<EntityCriteria> criterias)
		{
			var result = new List<Criteria>();
			foreach (var criteria in criterias)
			{
				IEntityDefinition entityDef = definition.Entities.FindDefinition(criteria.EntityName);
				result.Add(Convert(entityDef, criteria));
			}
			return result;
		}

		public static Criteria Convert(Property property)
		{
			var column = property.Definition.ColumnDef;
			var criteria = new Criteria
			{
				ColumnName = column.Name,
				TableName = column.Table.Name,
				Value = property.Value
			};
			return criteria;
		}
	}
}
