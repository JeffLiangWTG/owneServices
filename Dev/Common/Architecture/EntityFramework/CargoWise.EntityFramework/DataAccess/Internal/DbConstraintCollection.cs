using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	public class DbConstraintCollection : IEnumerable<DbConstraint>
	{
		public DbConstraintCollection(IEnumerable<DbConstraint> elements)
		{
			list = new List<DbConstraint>(elements);
			columnsToConstraints = new Dictionary<string, DbConstraint[]>();
		}

		public DbConstraint this[int index]
		{
			get { return list[index]; }
		}

		public DbConstraint this[string constraintName]
		{
			get
			{
				DbConstraint result = null;
				foreach (DbConstraint constraint in list)
				{
					if (constraint.ConstraintName == constraintName)
					{
						result = constraint;
					}
				}
				return result;
			}
		}

		public int Count
		{
			get { return list.Count; }
		}

		public DbConstraint[] GetConstraintsOfType(DbConstraintType type)
		{
			List<DbConstraint> result = new List<DbConstraint>();
			foreach (DbConstraint constraint in this)
			{
				if ((constraint.Type & type) != 0)
				{
					result.Add(constraint);
				}
			}
			return result.ToArray();
		}

		public DbConstraint[] GetConstraintsForColumn(string columnName)
		{
			DbConstraint[] result;
			if (!columnsToConstraints.TryGetValue(columnName, out result))
			{
				List<DbConstraint> constraints = new List<DbConstraint>();
				foreach (DbConstraint constraint in list)
				{
					if (constraint.ColumnNames[0].ToLower() == columnName.ToLower())
					{
						constraints.Add(constraint);
					}
				}
				result = constraints.ToArray();
				columnsToConstraints[columnName] = result;
			}
			return result;
		}

		public DbConstraint[] GetConstraintsForColumnOfType(string columnName, DbConstraintType type)
		{
			List<DbConstraint> result = new List<DbConstraint>();
			foreach (DbConstraint constraint in GetConstraintsForColumn(columnName))
			{
				if ((constraint.Type & type) != 0)
				{
					result.Add(constraint);
				}
			}
			return result.ToArray();
		}

		readonly List<DbConstraint> list;
		readonly Dictionary<string, DbConstraint[]> columnsToConstraints;

		#region IEnumerable<DbConstraint> Members

		IEnumerator<DbConstraint> IEnumerable<DbConstraint>.GetEnumerator()
		{
			return list.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return list.GetEnumerator();
		}

		#endregion
	}
}
