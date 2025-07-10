namespace Enterprise.DbUpgrader.Schema
{
	using System;
	using System.Data;
	using System.Globalization;
	using CargoWise.Data;
	using CargoWise.DbUpgrader.Foundation;
	using Enterprise.DbUpgrader.Shared;

	public class XmlSchemaSynchroniser : MetadataScriptRunner
	{
		public XmlSchemaSynchroniser(DbConnection upgConnection, IUpgradeTaskWorkflowLogger taskLogger, string dbBeingUpgraded, string templateDb)
			: base(upgConnection, dbBeingUpgraded, templateDb, taskLogger)
		{
		}

		public void SynchroniseXmlSchemas()
		{
			taskLogger.ActivateSubtaskProgress(2);

			taskLogger.StartSubtask("Drop old/modified XML schemas");
			DropOldAndModifiedSchemas();

			taskLogger.StartSubtask("Create new/ recreate modified schemas");
			CreateNewAndModifiedSchemas();
		}

		void DropOldAndModifiedSchemas()
		{
			ColumnSynchroniser colSynchroniser = new ColumnSynchroniser(upgConnection, taskLogger, dbBeingUpgraded, templateDb);

			DataTable oldOrModifiedXmlSchemas = GetDataTableFromQueryReplacingDbNames(OldOrModifiedXmlSchemasScriptRaw);

			foreach (DataRow xmlSchemaRow in oldOrModifiedXmlSchemas.Rows)
			{
				string xsdName = xmlSchemaRow["XsdName"].ToString();

				taskLogger.ShowInfoMessage("\t(-) " + xsdName);

				UnlinkRelatedXmlColumns(xsdName);

				string sqlText = String.Format("DROP XML SCHEMA COLLECTION [{0}];", xsdName);
				upgConnection.ExecuteNonQuery(sqlText);
			}
		}

		void CreateNewAndModifiedSchemas()
		{
			DataTable newXmlSchemas = GetDataTableFromQueryReplacingDbNames(NewXmlSchemasScriptRaw);

			foreach (DataRow xmlSchemaRow in newXmlSchemas.Rows)
			{
				string xsdName = xmlSchemaRow["XsdName"].ToString();
				string xsdSchema = xmlSchemaRow["XsdSchema"].ToString();

				taskLogger.ShowInfoMessage("\t(+) " + xsdName);
				string sqlText = String.Format("CREATE XML SCHEMA COLLECTION [{0}] AS '{1}';", xsdName, xsdSchema);
				upgConnection.ExecuteNonQuery(sqlText);
			}
		}

		const string NewXmlSchemasScriptRaw = @"
			EXEC [{1}]..sp_executesql N'
			SELECT
				newxsd.name as XsdName,
				XML_SCHEMA_NAMESPACE(''dbo'', newxsd.name) as XsdSchema
			FROM
				sys.xml_schema_collections newxsd
			WHERE
				newxsd.schema_id = (SELECT sch.schema_id FROM sys.schemas sch WHERE sch.name = ''dbo'')
				AND newxsd.name NOT IN (
					SELECT curxsd.name
					FROM [{0}].sys.xml_schema_collections curxsd
				)
			';";

		const string OldOrModifiedXmlSchemasScriptRaw = @"
			EXEC (N'
				CREATE TABLE #ModifiedXsd (XsdName sysname COLLATE DATABASE_DEFAULT, XsdSchema varchar(max) COLLATE DATABASE_DEFAULT);

				EXEC [{0}]..sp_executesql N''
					INSERT #ModifiedXsd
					SELECT
						newxsd.name,
						convert(varchar(max), XML_SCHEMA_NAMESPACE(''''dbo'''', newxsd.name))
					FROM
						sys.xml_schema_collections newxsd
					WHERE
						newxsd.schema_id = (SELECT sch.schema_id FROM sys.schemas sch WHERE sch.name = ''''dbo'''')'';

				EXEC [{1}]..sp_executesql N''
					DELETE #ModifiedXsd
					FROM
						#ModifiedXsd modxsd
						INNER JOIN sys.xml_schema_collections newxsd ON newxsd.name = modxsd.XsdName
					WHERE
						modxsd.XsdSchema = convert(varchar(max), XML_SCHEMA_NAMESPACE(''''dbo'''', newxsd.name))'';

				SELECT * FROM #ModifiedXsd;
			');";

		#region Unlink XML Columns related to an XML schema

		/// <summary>
		/// Should drop column dependent objects first. e.g:
		///  - The index 'IX_TableWithXmlFields_T1_Xml02' is dependent on column 'T1_Xml02'.
		///  - The object 'DF__TableWith__T1_Xm__7E6CC920' is dependent on column 'T1_Xml03'.
		///  (ALTER TABLE ALTER COLUMN T1_Xml03 failed because one or more objects access this column)
		/// </summary>
		/// <param name="xmlSchema"></param>
		void UnlinkRelatedXmlColumns(string xmlSchema)
		{
			DataTable columnsToAlter = GetXmlColumnsLinkedToAnXmlSchema(xmlSchema);

			foreach (DataRow row in columnsToAlter.Rows)
			{
				ColumnChangeMetadata columnMetadata = new ColumnChangeMetadata(row);

				taskLogger.ShowInfoMessage("\t(unlink related column) " + columnMetadata.TableSchema + "." + columnMetadata.TableName + "." + columnMetadata.ColumnName);
				var columnDependencyRemover = new DbColumnDependencyRemover(columnMetadata.TableSchema, columnMetadata.TableName, columnMetadata.ColumnName);
				columnDependencyRemover.DropRelateObjects(upgConnection);

				string sqlText = String.Format("ALTER TABLE [{0}].[{1}] ALTER COLUMN {2}", columnMetadata.TableSchema, columnMetadata.TableName, columnMetadata.ColumnDeclaration);
				upgConnection.ExecuteNonQuery(sqlText);
			}
		}

		DataTable GetXmlColumnsLinkedToAnXmlSchema(string xmlSchema)
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture, SelectXmlColumnsLinkedToAnXmlSchema, dbBeingUpgraded);
			DataTable result = null;
			using (var cmd = upgConnection.Command(sqlText))
			{
				cmd.AddParameter("@XsdName", SqlDbType.NVarChar, xmlSchema);
				result = DataUtils.GetDataTableFromCommand(cmd);
			}
			return result;
		}

		const string SelectXmlColumnsLinkedToAnXmlSchema = @"
			SELECT
				CurTabSchema.name TabSchema
				, CurTab.name TabName
				, CurCol.name ColName
				, 'XML' ColType
				, CASE CurCol.is_nullable
						WHEN 1 THEN 'NULL'
						ELSE 'NOT NULL'
					END ColNullOrNotNull
			FROM
				[{0}].sys.tables CurTab
				INNER JOIN [{0}].sys.columns CurCol ON CurCol.object_id = CurTab.object_id
				INNER JOIN [{0}].sys.schemas CurTabSchema ON CurTabSchema.schema_id = CurTab.schema_id
				INNER JOIN [{0}].sys.column_xml_schema_collection_usages XsdUsage
					ON XsdUsage.object_id = CurTab.object_id AND XsdUsage.column_id = CurCol.column_id
				INNER JOIN [{0}].sys.xml_schema_collections Xsd
					ON Xsd.xml_collection_id = XsdUsage.xml_collection_id
				WHERE
				-- Only tables from dbo schema
				CurTabSchema.name = 'dbo'
				-- Ignore microsoft-shipped tables
				AND CurTab.is_ms_shipped = 0
				-- Only columns linked to given XML schema
				AND Xsd.name = @XsdName
			ORDER BY
				CurTab.name";

		#endregion
	}
}
