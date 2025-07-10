using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class GlbDepartmentUpgradeTask : EmbeddedUpgradeTask
	{
		public GlbDepartmentUpgradeTask() : base(new GlbDepartmentDataFile())
		{
		}

		#region Overrides

		/// <summary>
		/// A new system row is to be inserted. Before that it must be checked if the new code is unique.
		/// If there is an user row (on DB) with the same code as the one to be inserted here, the user row have to be either:
		///   - Deleted if it's not being referenced by an FK or
		///   - Code is changed to a Dummy one and GE_IsActive of user department should NOT be changed 
		///   - System defined department with duplicate code will be added with GE_IsActive Set to �No� if user department is Active and with GE_IsActive Set to �Yes� otherwise)
		/// </summary>
		protected override void DoInsert(DataRow sourceRow, DataTable targetTable, ref int targetIndex)
		{
			if (!SamePkExists((Guid)sourceRow[GlbDepartmentSchema.Constants.PK]))
			{
				if (CheckAndResolveUniqueCodeConflict(sourceRow[GlbDepartmentSchema.Constants.GE_Code].ToString()))
				{
					sourceRow[GlbDepartmentSchema.Constants.GE_IsActive] = 0;
					sourceRow.AcceptChanges();
				}
				base.DoInsert(sourceRow, targetTable, ref targetIndex);
			}
		}

		/// <summary>
		/// Attempt to delete direct in the database the DataRow to be deleted here, in order to check if it's being referenced by an FK.
		///   - If the delete succeeds, DataRow is not touched (state should remain Unchanged).
		///   - If it fails, makes the DataRow inactive (Code is changed to a Dummy one and GE_IsActive is set to 0)
		/// </summary>
		protected override void DoDelete(DataRow targetRow, ref int targetIndex)
		{
			if (!DeleteDepartmentIfNotReferenced((Guid)targetRow[GlbDepartmentSchema.Constants.PK]))
			{
				string dummyCode = "~" + GetNextInactivatedCodeNumber().ToString();
				targetRow[GlbDepartmentSchema.Constants.GE_IsActive] = false;
				targetRow[GlbDepartmentSchema.Constants.GE_SystemCode] = false;
				targetRow[GlbDepartmentSchema.Constants.GE_Code] = dummyCode;
			}

			// Walk to next row on the target DataSet
			targetIndex++;
		}

		/// <summary>
		/// We do not update the IsActive field regarless if it is system defined or not
		/// as the client may have deactivated the department.
		/// </summary>
		protected override void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
			if (columnName != GlbDepartmentSchema.Constants.GE_IsActive)
			{
				if (columnName == GlbDepartmentSchema.Constants.GE_Code)
				{
					if (CheckAndResolveUniqueCodeConflict(sourceRow[GlbDepartmentSchema.Constants.GE_Code].ToString()))
					{
						sourceRow[GlbDepartmentSchema.Constants.GE_IsActive] = 0;
						sourceRow.AcceptChanges();
					}
				}
				base.UpdateColumn(columnName, targetRow, sourceRow);
			}
		}

		#endregion

		#region Implementation

		/// <summary>
		/// When a System Department is to be inserted or have its code changed,
		/// checks for an existing User Department with the same Code as the system one.
		/// If found, the User Department must be either:
		///   - Deleted if it's not being referenced by an FK or
		///   - Kept Active, but conflicting system department will be inserted as inanctive
		/// </summary>
		/// <param name="code"></param>
		/// <returns>
		/// TRUE : Conflict remains
		/// FALSE: No conflict/conflict has been resolved
		/// </returns>
		bool CheckAndResolveUniqueCodeConflict(string code)
		{
			string sqlText = "SELECT GE_PK FROM dbo.GlbDepartment WHERE GE_Code = @Code AND GE_SystemCode = 0";
			DbCommand cmd = Db.Connection.Command(sqlText);
			cmd.AddParameterBasedOnDbColumn("@Code", code, GlbDepartmentSchema.GE_Code);
			object pkObject = cmd.ExecuteScalar();

			if (pkObject != null)
			{
				Guid userDepartmentPk = (Guid)pkObject;
				if (!DeleteDepartmentIfNotReferenced(userDepartmentPk))
				{
					ChangeUserDepartmentCode(userDepartmentPk);
					return GetIsActiveValueForDepartment(userDepartmentPk);
				}
			}
			return false;
		}

		void ChangeUserDepartmentCode(Guid pk)
		{
			var triggerName = "TG_GlbDepartment_SystemLastEditAuditInfoMustBeUpdated_Update";
			var tableName = "dbo.GlbDepartment";

			var dummyCode = "~" + GetNextInactivatedCodeNumber().ToString();
			var sqlText = @"UPDATE dbo.GlbDepartment SET GE_Code = @Code, GE_SystemLastEditUser='E', GE_SystemLastEditTimeUtc=GETUTCDATE() WHERE GE_PK = @Pk;";
			sqlText = string.Format(CultureInfo.InvariantCulture, sqlText, triggerName, tableName);
			var cmd = Db.Connection.Command(sqlText);
			cmd.AddParameterBasedOnDbColumn("@Code", dummyCode, GlbDepartmentSchema.GE_Code);
			cmd.AddParameterBasedOnDbColumn("@Pk", pk, GlbDepartmentSchema.PK);
			cmd.ExecuteNonQuery();
		}

		bool GetIsActiveValueForDepartment(Guid pk)
		{
			string sqlText = String.Format("SELECT GE_IsActive FROM dbo.GlbDepartment WHERE GE_PK = '{0}'", pk.ToString());
			bool isActive = (bool)Db.Connection.ExecuteScalar(sqlText);
			return isActive;
		}

		/// <summary>
		/// Attempt to delete a department based on the given Department PK.
		/// </summary>
		/// <param name="pk">Department Primary Key</param>
		/// <returns>
		/// TRUE : Delete succeeded
		/// FALSE: Delete failed
		/// </returns>
		bool DeleteDepartmentIfNotReferenced(Guid pk)
		{
			bool result = false;

			try
			{
				string sqlText = "DELETE dbo.GlbDepartment WHERE GE_PK = @Pk";
				DbCommand cmd = Db.Connection.Command(sqlText);
				cmd.AddParameterBasedOnDbColumn("@Pk", pk, GlbDepartmentSchema.PK);
				cmd.ExecuteNonQuery();
				result = true;
			}
			catch (SqlException)
			{
				result = false;
			}

			return result;
		}

		int InactivatedCodeCount;
		int GetNextInactivatedCodeNumber()
		{
			if (InactivatedCodeCount == 0)
			{
				string sqlText = @"
					SELECT max(case when isnumeric(substring(GE_Code, 2, len(GE_Code))) = 1 then substring(GE_Code, 2, len(GE_Code)) else 0	end)
					FROM dbo.GlbDepartment 
					WHERE GE_Code like '~%'";
				var result = Db.Connection.ExecuteScalar(sqlText);
				InactivatedCodeCount = Convert.IsDBNull(result) ? 0 : (int)result;
			}

			InactivatedCodeCount++;
			return InactivatedCodeCount;
		}

		bool SamePkExists(Guid pk)
		{
			string sqlText = String.Format("SELECT GE_PK FROM dbo.GlbDepartment WHERE GE_PK = '{0}'", pk.ToString());
			object objPk = Db.Connection.ExecuteScalar(sqlText);
			return ((objPk as Guid?) != null);
		}

		#endregion
	}
}
