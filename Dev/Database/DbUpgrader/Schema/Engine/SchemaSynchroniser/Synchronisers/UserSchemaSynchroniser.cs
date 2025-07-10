namespace Enterprise.DbUpgrader.Schema
{
	using System;
	using CargoWise.Data;
	using CargoWise.DbUpgrader.Foundation;

	class UserSchemaSynchroniser : MetadataScriptRunner
	{
		public UserSchemaSynchroniser(DbConnection upgConnection, string dbBeingUpgraded, string templateDb, IUpgradeTaskWorkflowLogger taskLogger)
			: base(upgConnection, dbBeingUpgraded, templateDb, taskLogger)
		{
		}

		/// <summary>
		/// -------------------------------------------------------------------------------------------------
		/// -- CREATE new user schemas
		/// --
		/// -- Note: Tags {0} and {1} are replaced by the actual
		/// --       DbBeingUpgraded and TemplateDb names at run time
		/// -------------------------------------------------------------------------------------------------
		/// </summary>
		public void CreateNewUserSchemas()
		{
			string sqlText = GetScriptReplacingDbNames(@"
				SELECT
					NewSch.name
				FROM
					[{1}].sys.schemas NewSch
					INNER JOIN [{1}].sys.database_principals dp ON dp.principal_id = NewSch.principal_id
					LEFT JOIN [{0}].sys.schemas CurSch ON CurSch.name = NewSch.name
				WHERE
					dp.is_fixed_role = 0
					AND NewSch.name not like '%\[%' ESCAPE '\'
					AND NewSch.name not like '%]%'
					AND CurSch.name is null
			");

			var newSchemas = DataUtils.GetListOfValuesFromQuery(upgConnection, sqlText);

			foreach (string newSchema in newSchemas)
			{
				taskLogger.ShowInfoMessage("\t  (+) " + newSchema);
				upgConnection.ExecuteNonQuery(String.Format("CREATE SCHEMA [{0}]", newSchema));
			}
		}

		/// <summary>
		/// -------------------------------------------------------------------------------------------------
		/// -- DROP old empty user schemas
		/// --
		/// -- Note: Tags {0} and {1} are replaced by the actual
		/// --       DbBeingUpgraded and TemplateDb names at run time
		/// -------------------------------------------------------------------------------------------------
		/// </summary>
		public void DropOldUserSchemas()
		{
			string sqlText = GetScriptReplacingDbNames(@"
				SELECT CurSch.name
				FROM
					[{0}].sys.schemas CurSch
					LEFT JOIN [{1}].sys.schemas NewSch ON NewSch.name = CurSch.name
					LEFT JOIN [{0}].sys.objects ChildObj ON ChildObj.schema_id = CurSch.schema_id
				WHERE
					NewSch.name is null
					AND ChildObj.schema_id is null");

			var oldSchemas = DataUtils.GetListOfValuesFromQuery(upgConnection, sqlText);

			foreach (string oldSchema in oldSchemas)
			{
				taskLogger.ShowInfoMessage("\t  (-) " + oldSchema);
				upgConnection.ExecuteNonQuery(String.Format("DROP SCHEMA [{0}]", oldSchema.Replace("]", "]]")));
			}
		}
	}
}
