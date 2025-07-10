using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	public static class GenericTestHelper
	{
		/// <summary>
		/// Generate all possible combination given n digit binary.
		/// Example: given digit = 2 will return 00, 01, 10, 11
		/// </summary>
		public static List<List<bool>> GenerateBinaryCombination(int digit)
		{
			var binaryCombinations = new List<List<bool>>() { };

			if (digit == 0)
			{
				return binaryCombinations;
			}

			int maxNumber = (1 << digit);

			for (int number = 0; number < maxNumber; ++number)
			{
				var numberInBinary = new BitArray(new int[] { number })
					.Cast<bool>()
					.Take(digit)
					.ToList();
				binaryCombinations.Add(numberInBinary);
			}
			return binaryCombinations;
		}

		/// <summary>
		/// Generate all permutations given items list.
		/// Example: given 1, 2, 3 will return [1, 2, 3], [1, 3, 2], [2, 3, 1], [2, 1, 3], [3, 1, 2], [3, 2, 1]
		/// </summary>
		public static List<List<T>> GeneratePermutation<T>(List<T> items)
		{
			T[] current_permutation = new T[items.Count];
			bool[] in_selection = new bool[items.Count];

			List<List<T>> results = new List<List<T>>();

			PermuteItems(items, in_selection, current_permutation, results, 0);

			return results;
		}

		#region AssertNumberOfActiveBusinessObjectCollections

		// SGEN for DataTransfers.Busines project doesn't like this assert method to be in TestCaseWithFactory
		// it gives some non sensitive errors about missing references despite all references it complains about being present
		public static int GetNumberOfActiveBusinessObjectCollections<T>(BusinessObjectFactory factory)
			where T : BusinessObject
		{
			return ActiveBusinessObjectCollectionIndexCache<T>.GetInstance(factory).All.Count();
		}

		#endregion

		#region AssertSparseColumnsAreEmptyAfterSetDefaultValues

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Test Mode")]
		public static ZString AssertSparseColumnsAreEmptyAfterSetDefaultValues(DbConnection testConnection, BusinessObject bizo, List<ZString> excludedColumns)
		{
			var result = ZString.Empty;
			var tableName = bizo.TableName;

			var sparseColumns = new List<string>();
			string sqlText = @"
SELECT col.name as ColumnName FROM sys.columns col
INNER JOIN sys.tables tbl ON col.object_id = tbl.object_id
WHERE col.is_sparse = 1 AND tbl.name = @tableName";

			using (var cmd = testConnection.Command(sqlText))
			{
				cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, tableName);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						sparseColumns.Add(reader["ColumnName"].ToString());
					}
				}
			}

			var columnsInExcludedColumnsButIsNotSparseColumn = excludedColumns.Where(x => !sparseColumns.Contains(x));
			if (columnsInExcludedColumnsButIsNotSparseColumn.Any())
			{
				var columnsString = ZString.Join(", ", columnsInExcludedColumnsButIsNotSparseColumn.ToArray());
				result = ZString.Format("The following columns in the GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues but it is not a sparse column. \r\nTableName: {0}. \r\nColums: {1}.", tableName, columnsString);
			}

			if (result.IsEmpty)
			{
				var columnsThatNeedToRemoveSparse = new List<ZString>();
				foreach (var column in sparseColumns.Where(x => !excludedColumns.Contains(x)))
				{
					if (!((IZType)bizo[column]).IsDefault)
					{
						columnsThatNeedToRemoveSparse.Add(column);
					}
				}

				if (columnsThatNeedToRemoveSparse.Count > 0)
				{
					var columnsString = ZString.Join(", ", columnsThatNeedToRemoveSparse.ToArray());
					result = ZString.Format("You have added a default value to a sparse column. This would indicate the column should not be sparse as it will always have a value. \r\nTableName: {0}. \r\nColums: {1}.", tableName, columnsString);
				}
			}

			return result;
		}

		#endregion

		#region implementation

		static void PermuteItems<T>(List<T> items, bool[] in_selection, T[] current_permutation, List<List<T>> results, int next_position)
		{
			if (next_position == items.Count)
			{
				results.Add(current_permutation.ToList());
			}
			else
			{
				for (int i = 0; i < items.Count; i++)
				{
					if (!in_selection[i])
					{
						in_selection[i] = true;
						current_permutation[next_position] = items[i];
						PermuteItems(items, in_selection, current_permutation, results, next_position + 1);
						in_selection[i] = false;
					}
				}
			}
		}

		#endregion
	}
}
