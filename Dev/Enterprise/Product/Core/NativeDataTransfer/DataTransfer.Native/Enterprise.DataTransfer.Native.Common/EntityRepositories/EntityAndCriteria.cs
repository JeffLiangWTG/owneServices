using System.Collections.Generic;
using System.Linq;
using Enterprise.DataTransfer.Native.DB.Sql;

namespace Enterprise.DataTransfer.Native.Common
{
	/// <summary>
	/// An entity plus a list of Criteria used to match it.
	/// </summary>
	class EntityAndCriteria : IColumnValueSet
	{
		public EntityAndCriteria(IEntity entity, IEnumerable<Criteria> criteriaList)
		{
			Entity = entity;
			CriteriaListOrderedByName = criteriaList?.OrderBy(x => x.ColumnName).ToList();
			hashCode = CalculateHashCode();
		}

		public IEntity Entity { get; }
		public IList<Criteria> CriteriaListOrderedByName { get; }
		readonly int hashCode;

		public int ColumnCount => CriteriaListOrderedByName != null ? CriteriaListOrderedByName.Count : 0;
		public bool Equals(IColumnValueSet other) => ColumnValueSetComparer.IsEqual(this, other);

		public override int GetHashCode() => hashCode;

		int CalculateHashCode()
		{
			int result = 0;
			if (CriteriaListOrderedByName != null)
			{
				foreach (var criteria in CriteriaListOrderedByName)
				{
					result = ColumnValueSetComparer.AppendHashCode(result, criteria.Value);
				}
			}
			return result;
		}

		public object GetValue(int columnIndex) => CriteriaListOrderedByName[columnIndex].Value;
	}
}
