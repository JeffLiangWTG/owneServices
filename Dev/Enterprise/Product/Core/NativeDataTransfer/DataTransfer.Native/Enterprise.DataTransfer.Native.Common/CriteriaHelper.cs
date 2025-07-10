using System.Collections.Generic;
using System.Data;
using System.Linq;
using Enterprise.DataTransfer.Native.Common.Converters;
using Enterprise.DataTransfer.Native.Common.Definitions.Associations;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.DataTransfer.Native.DB.Sql;
using Enterprise.DataTransfer.Native.Utils;

namespace Enterprise.DataTransfer.Native.Common
{
	static class CriteriaHelper
	{
		public static IEnumerable<Criteria> GetCriteriaUsingAllTagsSpecifiedInData(IEntity entity, ERConverter converter, params string[] exclusions)
		{
			return GetCriteriaUsingAllTagsSpecifiedInData(entity, converter)
				.Where(x => !exclusions.Contains(x.ColumnName));
		}

		public static IEnumerable<Criteria> GetCriteriaUsingAllTagsSpecifiedInData(IEntity entity, ERConverter converter)
		{
			var tempRow = converter.Convert(entity);
			try
			{
				return GetCriteriaFromRelatedEntities(converter, entity, tempRow)
					.Union(GetCriteriaFromProperties(entity, tempRow))
					.Where(x => !entity.Definition.UniqueCriteriaExclusionsContains(x.ColumnName))
					.ToList();
			}
			finally
			{
				tempRow.Delete();
			}
		}

		public static IEnumerable<Criteria> GetCriteriaFromProperties(IEntity entity, ERConverter converter, params string[] exclusions)
		{
			var tempRow = converter.Convert(entity);

			try
			{
				var result = GetCriteriaFromProperties(entity, tempRow)
					.Where(x => !entity.Definition.UniqueCriteriaExclusionsContains(x.ColumnName)
						&& !exclusions.Contains(x.ColumnName))
					.ToList();
				return result;
			}
			finally
			{
				tempRow.Delete();
			}
		}

		public static IEnumerable<Criteria> GetCriteriaFromSpecifiedColumnNames(IEntity entity, ERConverter converter, bool isWithCriteriaFromRelatedEntity, params string[] columnNames)
		{
			var tempRow = converter.Convert(entity);
			try
			{
				IEnumerable<Criteria> criterias = columnNames.Select(columnName => new Criteria { TableName = entity.EntityName, ColumnName = columnName, Value = tempRow[columnName] }).ToArray();
				if (isWithCriteriaFromRelatedEntity)
				{
					criterias = GetCriteriaFromRelatedEntities(converter, entity, tempRow).Union(criterias).ToArray();
				}
				return criterias;
			}
			finally
			{
				tempRow.Delete();
			}
		}

		public static IEnumerable<IEnumerable<Criteria>> GetCriteriaFromCandidateKey(IEntity entity, ERConverter converter)
		{
			var allCandidateKeysCriterias = new List<List<Criteria>>();

			var tempRow = converter.Convert(entity);
			try
			{
				foreach (CandidateKey candidateKey in tempRow.CandidateKeys())
				{
					var criterias = new List<Criteria>();
					if (candidateKey != null)
					{
						foreach (var cell in candidateKey.Values)
						{
							criterias.Add((Criteria)cell);
						}
					}
					allCandidateKeysCriterias.Add(criterias);
				}
			}
			finally
			{
				tempRow.Delete();
			}

			return allCandidateKeysCriterias;
		}

		static IEnumerable<Criteria> GetCriteriaFromRelatedEntities(ERConverter converter, IEntity entity, DataRow tempRow)
		{
			var criteriaFromParents =
				from parent in entity.Parents
				join association in entity.Definition.AssociationCollection.ParentAssociations on (EntityDefinition)parent.Definition equals association.To
				where !(association is ManyToManyAssociation)
				let columnName = association.GetRelation(parent.Definition).Keys[0].FromKey.Name
				select new Criteria { TableName = entity.TableName, ColumnName = columnName, Value = tempRow[columnName] };
			var criteriaFromChildren =
				from child in entity.Children
				join association in entity.Definition.AssociationCollection.ChildAssociations on (EntityDefinition)child.Definition equals association.From
				where !(association is ManyToManyAssociation) && !association.AdditionalKey.IsEmpty() && !association.AdditionalKeyRef.IsEmpty()
				select new Criteria { TableName = entity.TableName, ColumnName = association.AdditionalKeyRef, Value = converter.GetValue(child, association.AdditionalKey) };
			return criteriaFromParents.Union(criteriaFromChildren);
		}

		static IEnumerable<Criteria> GetCriteriaFromProperties(IEntity entity, DataRow tempRow)
		{
			return from property in entity.Properties
				   let columnName = property.Definition.ColumnDef.Name
				   let columnType = property.Definition.ColumnDef.DataType
				   where property.Definition.ColumnDef.Type != ColumnType.PrimaryKey
				   select new Criteria { TableName = entity.TableName, ColumnName = columnName, Value = tempRow[columnName], IsBinary = (columnType == DbDataType.VarBinary || columnType == DbDataType.Binary) };
		}
	}
}
