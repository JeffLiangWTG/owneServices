using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class RefPackTypeUpgradeTask : EmbeddedUpgradeTask
	{
		public RefPackTypeUpgradeTask() : base(new RefPackTypeDataFile())
		{
		}

		internal RefPackTypeUpgradeTask(EmbeddedDataFile resourceDataFile) : base(resourceDataFile)
		{
		}

		#region Insert, Update and Delete - Overrides

		/// A new system row is to be inserted. Before that we must check if the new code is unique.
		/// If there is a user row (on DB) with the same code as the one to be inserted here, either:
		///   - User row is updatable, we can delete and replace it with the system row
		///   - User row is not updatable, we don't insert the system row
		protected override void DoInsert(DataRow sourceRow, DataTable targetTable, ref int targetIndex)
		{
			// If PK conflicts with existing one, inserts record but with a new PK
			if (IsPkConflicted(sourceRow, targetTable))
			{
				sourceRow[RefPackTypeSchema.Constants.PK] = Guid.NewGuid();
			}

			if (CheckAndResolveUniqueCodeConflict(targetTable, sourceRow[RefPackTypeSchema.Constants.F3_Code].ToString()))
			{
				base.DoInsert(sourceRow, targetTable, ref targetIndex);
			}
		}

		/// When updating a system row, if a system row exits with the same code as a non-updatable user row,
		/// we delete the system row and only keep the user row.
		protected override void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
			if ((bool)targetRow[RefPackTypeSchema.Constants.F3_IsUpdatable])
			{
				if (columnName == RefPackTypeSchema.Constants.F3_Code && !CheckAndResolveUniqueCodeConflict(targetRow.Table, sourceRow[RefPackTypeSchema.Constants.F3_Code].ToString()))
				{
					DeleteRefPackType((Guid)targetRow[RefPackTypeSchema.Constants.PK]);
				}
				else
				{
					base.UpdateColumn(columnName, targetRow, sourceRow);
				}
			}
		}

		/// A datarow is to be deleted here. 
		///   - If it's an updatable/deletable row, we delete it
		///   - If row is not updatable/deletable, we just skip its deletion
		protected override void DoDelete(DataRow targetRow, ref int targetIndex)
		{
			if ((bool)targetRow[RefPackTypeSchema.Constants.F3_IsUpdatable])
			{
				DeleteRefPackType((Guid)targetRow[RefPackTypeSchema.Constants.PK]);
			}

			targetIndex++;
		}

		#endregion

		#region Insert, Update and Delete

		bool IsPkConflicted(DataRow sourceRow, DataTable targetTable)
		{
			string filterExpression = string.Format("{0} = '{1}'", RefPackTypeSchema.Constants.PK, sourceRow[RefPackTypeSchema.Constants.PK].ToString());
			DataRow[] matchingPkTargetRows = targetTable.Select(filterExpression);
			return (matchingPkTargetRows.Length > 0);
		}

		/// <summary>
		/// Check for code conflict and tries to resolve it.
		/// Will return false if a conflict was detected but not resolved
		/// </summary>
		protected bool CheckAndResolveUniqueCodeConflict(DataTable targetTable, string code)
		{
			bool conflictResolved = true;

			var matchedCodeTargetRows = targetTable.Select($"{RefPackTypeSchema.Constants.F3_Code} = '{code}'");
			if (matchedCodeTargetRows.Length > 0)
			{
				if ((bool)matchedCodeTargetRows[0][RefPackTypeSchema.Constants.F3_IsUpdatable])
				{
					var pk = (Guid)matchedCodeTargetRows[0][RefPackTypeSchema.Constants.PK];
					if (!deletedPKList.Contains(pk))
					{
						matchedCodeTargetRows[0].Delete();
						deletedPKList.Add(pk);
					}
				}
				else
				{
					conflictResolved = false;
				}
			}
			return conflictResolved;
		}

		void DeleteRefPackType(Guid pk)
		{
			if (!deletedPKList.Contains(pk))
			{
				string sqlText = "DELETE dbo.RefPackType WHERE F3_PK = @Pk";
				DbCommand cmd = Db.Connection.Command(sqlText);
				cmd.AddParameterBasedOnDbColumn("@Pk", pk, RefPackTypeSchema.PK);
				cmd.ExecuteNonQuery();
				deletedPKList.Add(pk);
			}
		}

		readonly List<Guid> deletedPKList = new List<Guid>();

		#endregion
	}
}
