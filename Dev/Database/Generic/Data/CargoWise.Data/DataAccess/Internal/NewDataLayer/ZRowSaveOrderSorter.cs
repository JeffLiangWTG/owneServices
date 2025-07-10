using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CargoWise.EntityFramework
{
#if DEBUG
	public
#endif
	class ZRowSaveOrderSorter
	{
		#region RowDependency

		class RowDependency
		{
			public DataRow Row { get; set; }

			public List<RowDependency> Parents
			{
				get { return parents ?? (parents = new List<RowDependency>()); }
			}
			List<RowDependency> parents;

			public List<RowDependency> Children
			{
				get { return children ?? (children = new List<RowDependency>()); }
			}
			List<RowDependency> children;

			public void AddChild(RowDependency child)
			{
				children.Add(child);
				child.Parents.Add(this);
			}

			public bool IsDependentOnParent(RowDependency rowDependency)
			{
				if (isInRecursiveCheck)
				{
					throw new NotSupportedException(string.Format("Circular dependency references found for row {0} ({1}).", Row.Table.TableName, Row[0]));
				}

				isInRecursiveCheck = true;
				try
				{
					return Parents.Any(parent => parent == rowDependency || parent.IsDependentOnParent(rowDependency));
				}
				finally
				{
					isInRecursiveCheck = false;
				}
			}

			bool isInRecursiveCheck;
		}

		#endregion

		#region RowComparer

		class RowComparer : IComparer<DataRow>
		{
			public RowComparer(Dictionary<DataRow, RowDependency> dependencies)
			{
				this.dependencies = dependencies;
			}

			readonly Dictionary<DataRow, RowDependency> dependencies;

			public int Compare(DataRow x, DataRow y)
			{
				RowDependency rowDependencyX, rowDependencyY;
				if (dependencies.TryGetValue(x, out rowDependencyX) && dependencies.TryGetValue(y, out rowDependencyY))
				{
					if (rowDependencyX.IsDependentOnParent(rowDependencyY))
					{
						return 1;
					}
					if (rowDependencyY.IsDependentOnParent(rowDependencyX))
					{
						return -1;
					}
				}

				if (x.Table == y.Table)
				{
					if (x.RowState == DataRowState.Deleted && y.RowState != DataRowState.Deleted)
					{
						return -1;
					}
					if (x.RowState == DataRowState.Added && y.RowState != DataRowState.Added)
					{
						return 1;
					}
					if (x.RowState == DataRowState.Modified)
					{
						if (y.RowState == DataRowState.Added)
						{
							return -1;
						}
						if (y.RowState == DataRowState.Deleted)
						{
							return 1;
						}
					}
				}

				return 0;
			}
		}

		#endregion

		public void Sort(IList<DataRow> rows)
		{
			var dependencies = GetRowsDependencies(rows);
			foreach (var rowDependency in dependencies)
			{
				if (rowDependency.Value.IsDependentOnParent(rowDependency.Value))
				{
					throw new NotSupportedException(string.Format("Circular dependency references found for row {0} ({1}).", rowDependency.Key.Table.TableName, GetRowPK(rowDependency.Key)));
				}
			}

			SortRows(rows, new RowComparer(dependencies));
		}

		void SortRows(IList<DataRow> rows, RowComparer rowComparer)
		{
			for (int i = 0, repeatCounter = rows.Count; i < rows.Count - 1; i++)
			{
				bool hasChanges = false;

				for (int j = i + 1; j < rows.Count; j++)
				{
					if (i != j)
					{
						var x = rows[i];
						var y = rows[j];
						if (rowComparer.Compare(x, y) > 0)
						{
							rows[i] = y;
							rows[j] = x;
							hasChanges = true;
						}
					}
				}

				if (hasChanges && repeatCounter > 0)
				{
					i--;
					repeatCounter--;
				}
				else
				{
					repeatCounter = rows.Count - i;
				}
			}
		}

		Dictionary<DataRow, RowDependency> GetRowsDependencies(IList<DataRow> rows)
		{
			var dependencies = new Dictionary<DataRow, RowDependency>();
			for (int i = 0; i < rows.Count; i++)
			{
				var x = rows[i];
				if (RowHasChangedFKs(x))
				{
					for (int j = 0; j < rows.Count; j++)
					{
						if (j != i)
						{
							var y = rows[j];
							if (RowXIsReferencingRowYWithChangedFK(x, y, DataRowVersion.Current))
							{
								AddParentDependency(x, y, dependencies);
							}
							if (RowXIsReferencingRowYWithChangedFK(x, y, DataRowVersion.Original))
							{
								AddChildDependency(x, y, dependencies);
							}
						}
					}
				}
			}
			return dependencies;
		}

		void AddParentDependency(DataRow childRow, DataRow parentRow, Dictionary<DataRow, RowDependency> dependencies)
		{
			AddChildDependency(parentRow, childRow, dependencies);
		}

		void AddChildDependency(DataRow parentRow, DataRow childRow, Dictionary<DataRow, RowDependency> dependencies)
		{
			RowDependency parentDependency;
			if (!dependencies.TryGetValue(parentRow, out parentDependency))
			{
				parentDependency = new RowDependency { Row = parentRow };
				dependencies.Add(parentRow, parentDependency);
			}

			RowDependency childDependency;
			if (!dependencies.TryGetValue(childRow, out childDependency))
			{
				childDependency = new RowDependency { Row = childRow };
				dependencies.Add(childRow, childDependency);
			}

			if (!parentDependency.Children.Contains(childDependency))
			{
				parentDependency.AddChild(childDependency);
			}
		}

		bool RowXIsReferencingRowYWithChangedFK(DataRow x, DataRow y, DataRowVersion rowVersion)
		{
			if (!x.HasVersion(rowVersion))
			{
				return false;
			}

			var yPK = GetRowPK(y);
			var fkColumnNames = GetTableFKColumnNames(x.Table);
			var fullRowIsChanged = !x.HasVersion(DataRowVersion.Original) || !x.HasVersion(DataRowVersion.Current);

			return
				fkColumnNames
					.Where(columnName => fullRowIsChanged || !x[columnName, DataRowVersion.Original].Equals(x[columnName, DataRowVersion.Current]))
					.Any(columnName => x[columnName, rowVersion].Equals(yPK));
		}

		string[] GetTableFKColumnNames(DataTable table)
		{
			if (tableFKColumnNames == null)
			{
				tableFKColumnNames = new Dictionary<string, string[]>();
			}
			string[] fkColumns;
			if (!tableFKColumnNames.TryGetValue(table.TableName, out fkColumns))
			{
				fkColumns = ZRowRelationshipManager.GetFKColumnNames(table).ToArray();
				tableFKColumnNames.Add(table.TableName, fkColumns);
			}
			return fkColumns;
		}

		bool RowHasChangedFKs(DataRow row)
		{
			var fkColumnNames = GetTableFKColumnNames(row.Table);
			if (fkColumnNames.Length > 0)
			{
				if (row.RowState == DataRowState.Added || row.RowState == DataRowState.Deleted)
				{
					return true;
				}

				if (row.HasVersion(DataRowVersion.Original) && row.HasVersion(DataRowVersion.Current))
				{
					return fkColumnNames.Any(columnName => !row[columnName, DataRowVersion.Original].Equals(row[columnName, DataRowVersion.Current]));
				}
			}
			return false;
		}

		object GetRowPK(DataRow row)
		{
			if (tablePKColumnNames == null)
			{
				tablePKColumnNames = new Dictionary<string, string>();
			}
			string pkColumnName;
			if (!tablePKColumnNames.TryGetValue(row.Table.TableName, out pkColumnName))
			{
				pkColumnName = row.Table.PrimaryKey.Length > 0 ? row.Table.PrimaryKey[0].ColumnName : row.Table.Columns[0].ColumnName;
				tablePKColumnNames.Add(row.Table.TableName, pkColumnName);
			}

			if (!string.IsNullOrEmpty(pkColumnName))
			{
				return row.HasVersion(DataRowVersion.Current) ? row[pkColumnName, DataRowVersion.Current] : row[pkColumnName, DataRowVersion.Original];
			}
			return null;
		}

		Dictionary<string, string[]> tableFKColumnNames;
		Dictionary<string, string> tablePKColumnNames;
	}
}
