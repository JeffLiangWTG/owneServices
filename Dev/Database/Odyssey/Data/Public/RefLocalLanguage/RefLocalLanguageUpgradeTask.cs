using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class RefLocalLanguageUpgradeTask : EmbeddedUpgradeTask
	{
		public RefLocalLanguageUpgradeTask() : base(new RefLocalLanguageDataFile())
		{
		}

		#region Overrides

		protected override void DoInsert(DataRow sourceRow, DataTable targetTable, ref int targetIndex)
		{
			var pkOfChildLanguages = DeleteUserDefinedLanguageIfAlreadyExist(sourceRow);
			base.DoInsert(sourceRow, targetTable, ref targetIndex);
			AttachChildLanguagesAfterChange(sourceRow, pkOfChildLanguages);
		}

		protected override void DoDelete(DataRow targetRow, ref int targetIndex)
		{
			DetachChildLanguagesBeforeChange(targetRow);
			// Walk to next row on the target DataSet
			targetIndex++;
		}

		protected override void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
			var pkOfChildLanguages = DeleteUserDefinedLanguageIfAlreadyExist(sourceRow);
			base.UpdateColumn(columnName, targetRow, sourceRow);
			AttachChildLanguagesAfterChange(sourceRow, pkOfChildLanguages);
		}

		#endregion

		#region Implementation

		List<Guid> DetachChildLanguagesBeforeChange(DataRow sourceRow)
		{
			var pkOfChildLanguages = new List<Guid>();
			var sqlText = FormattableString.Invariant($@"SELECT {RefLocalLanguageSchema.Constants.PK} FROM {RefLocalLanguageSchema.Constants.SqlSchemaName}.{RefLocalLanguageSchema.Constants.TableName} 
WHERE {RefLocalLanguageSchema.Constants.RA_Code} = @Code 
AND {RefLocalLanguageSchema.Constants.RA_RN_NKCountryCode} = @Country");
			var cmd = Db.Connection.Command(sqlText);
			cmd.AddParameterBasedOnDbColumn("@Code", sourceRow[RefLocalLanguageSchema.Constants.RA_Code], RefLocalLanguageSchema.RA_Code);
			cmd.AddParameterBasedOnDbColumn("@Country", sourceRow[RefLocalLanguageSchema.Constants.RA_RN_NKCountryCode], RefLocalLanguageSchema.RA_RN_NKCountryCode);
			var parentPk = cmd.ExecuteScalar();

			if (parentPk != null)
			{
				sqlText = FormattableString.Invariant($@"SELECT {RefLocalLanguageSchema.Constants.PK} FROM {RefLocalLanguageSchema.Constants.SqlSchemaName}.{RefLocalLanguageSchema.Constants.TableName}
						WHERE {RefLocalLanguageSchema.Constants.RA_RA_ParentLanguage} = @Parent");
				cmd = Db.Connection.Command(sqlText);
				cmd.AddParameterBasedOnDbColumn("@Parent", parentPk, RefLocalLanguageSchema.PK);
				var reader = cmd.ExecuteReader();
				try
				{
					while (reader.Read())
					{
						pkOfChildLanguages.Add((Guid)reader[RefLocalLanguageSchema.Constants.PK]);
					}
				}
				finally
				{
					reader.Close();
				}

				sqlText = FormattableString.Invariant($@"UPDATE {RefLocalLanguageSchema.Constants.SqlSchemaName}.{RefLocalLanguageSchema.Constants.TableName}
SET {RefLocalLanguageSchema.Constants.RA_RA_ParentLanguage} = '{Guid.Empty}'
WHERE {RefLocalLanguageSchema.Constants.RA_RA_ParentLanguage} = @Parent");
				cmd = Db.Connection.Command(sqlText);
				cmd.AddParameterBasedOnDbColumn("@Parent", (Guid)parentPk, RefLocalLanguageSchema.PK);
				cmd.ExecuteNonQuery();
			}

			return pkOfChildLanguages;
		}

		List<Guid> DeleteUserDefinedLanguageIfAlreadyExist(DataRow sourceRow)
		{
			var pkOfChildLanguages = DetachChildLanguagesBeforeChange(sourceRow);
			var sqlText = FormattableString.Invariant($@"DELETE {RefLocalLanguageSchema.Constants.SqlSchemaName}.{RefLocalLanguageSchema.Constants.TableName} 
WHERE {RefLocalLanguageSchema.Constants.RA_Code} = @Code 
AND {RefLocalLanguageSchema.Constants.RA_RN_NKCountryCode} = @Country 
AND {RefLocalLanguageSchema.Constants.RA_IsSystem} = @IsSystem");
			var cmd = Db.Connection.Command(sqlText);
			cmd.AddParameterBasedOnDbColumn("@Code", sourceRow[RefLocalLanguageSchema.Constants.RA_Code], RefLocalLanguageSchema.RA_Code);
			cmd.AddParameterBasedOnDbColumn("@Country", sourceRow[RefLocalLanguageSchema.Constants.RA_RN_NKCountryCode], RefLocalLanguageSchema.RA_RN_NKCountryCode);
			cmd.AddParameterBasedOnDbColumn("@IsSystem", false, RefLocalLanguageSchema.RA_IsSystem);
			cmd.ExecuteNonQuery();
			return pkOfChildLanguages;
		}

		void AttachChildLanguagesAfterChange(DataRow sourceRow, List<Guid> pkOfChildLanguages)
		{
			if (pkOfChildLanguages.Count > 0)
			{
				var pkParam = string.Empty;
				foreach (var pk in pkOfChildLanguages)
				{
					pkParam += pkParam.Length > 0 ? "," : "" + FormattableString.Invariant($"'{pk.ToString()}'");
				}
				var sqlText = FormattableString.Invariant($@"UPDATE {RefLocalLanguageSchema.Constants.SqlSchemaName}.{RefLocalLanguageSchema.Constants.TableName}
SET {RefLocalLanguageSchema.Constants.RA_RA_ParentLanguage} = @Parent
WHERE {RefLocalLanguageSchema.Constants.PK} in ({pkParam})");
				var cmd = Db.Connection.Command(sqlText);
				cmd.AddParameterBasedOnDbColumn("@Parent", sourceRow[RefLocalLanguageSchema.Constants.PK], RefLocalLanguageSchema.PK);
				cmd.ExecuteNonQuery();
			}
		}

		#endregion
	}
}
