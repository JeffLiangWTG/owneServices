using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Data.SqlServer.Metadata.ComputedColumn;
using CargoWise.Database.Abstractions;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;
using CargoWise.DbUpgrader.Foundation;
using CargoWise.DbUpgrader.Scripts;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions;
using Enterprise.Build.Database.Script;
using Enterprise.DbUpgrader.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.DbUpgrader.Script
{
	class ViewAndRoutineCreator
	{
		#region Constructor

		public ViewAndRoutineCreator(IUpgradeManager manager, DbConnection upgConnection, string dbBeingUpgraded, IEnumerable<string> triggerTransformations = null)
		{
			this.manager = manager;
			this.upgConnection = upgConnection;
			this.dbBeingUpgraded = dbBeingUpgraded;

			this.triggerTransformations = (triggerTransformations == null || !triggerTransformations.Any())
				? new List<DbScript>()
				: triggerTransformations.Select(trgName => new DbScript(trgName, "", DbRoutineType.SqlTriggerTypeDesc)).ToList()
				;
		}

		protected readonly IUpgradeManager manager;
		protected readonly DbConnection upgConnection;
		protected readonly string dbBeingUpgraded;
		readonly List<DbScript> triggerTransformations;

		#endregion // Constructor

		public void Run()
		{
			manager.StartTask(String.Format("*** Database: {0} ***", dbBeingUpgraded));

			try
			{
				using (((ICurrentDbControl)upgConnection).UseDatabase(dbBeingUpgraded))
				{
					RecreateViewsAndRoutines();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string message = String.Format("Failed to recreate Views and Routines - Database [{0}].\r\n{1}", dbBeingUpgraded, ex.Message);
				throw new Exception(message, ex);
			}
		}

		internal void CheckTriggersAreEnabledAndSync()
		{
			manager.StartTask("Check all triggers are enabled and synchronised");

			var expectedList = GetViewAndRoutineScriptCollection()
				.Where(s => s.ObjectType == DbRoutineType.SqlTriggerTypeDesc);

			var messages = new List<string>();
			var triggers = new List<IDbScript>();

			var reservedSchemas = string.Join("','", Db.SqlReservedSchemas);
			var sql = FormattableString.Invariant($@"
SELECT
	sch_name = COALESCE(OBJECT_SCHEMA_NAME(trg.object_id), ''),
	trg_name = trg.name,
	trg_text = txt.definition
	, is_disabled = trg.is_disabled
FROM
	sys.triggers         AS trg
	JOIN sys.sql_modules AS txt ON txt.object_id = trg.object_id
WHERE 1=1
	AND trg.is_ms_shipped = 0
	AND trg.type = 'TR'
	AND COALESCE(OBJECT_SCHEMA_NAME(trg.object_id), '') NOT in ('{reservedSchemas}')
ORDER BY
	sch_name, trg_name

");

			upgConnection.ExecuteReader(sql, (reader) =>
			{
				var sch_name = (string)reader["sch_name"];
				var trg_name = (string)reader["trg_name"];
				var trg_text = (string)reader["trg_text"];
				var is_disabled = (bool)reader["is_disabled"];

				if (is_disabled)
				{
					if (expectedList.Any(trg => trg.SchemaName.Equals(sch_name, StringComparison.OrdinalIgnoreCase) && trg.Name.Equals(trg_name, StringComparison.OrdinalIgnoreCase)))
					{
						messages.Add(FormattableString.Invariant($"{GetKey(sch_name, trg_name)} is disabled"));
					}
				}

				triggers.Add(new DbScript(sch_name, trg_name, trg_text, DbRoutineType.SqlTriggerTypeDesc));
			});

			foreach (var trg in expectedList.AsParallel().Except(triggers.AsParallel(), DbScriptComparer.SchemaNameTypeText).ToList())
			{
				if (triggers.Any(trigger => trigger.SchemaName == trg.SchemaName && trigger.Name == trg.Name))
				{
					messages.Add(FormattableString.Invariant($"{GetKey(trg.SchemaName, trg.Name)} has different definition"));
				}
				else
				{
					messages.Add(FormattableString.Invariant($"{GetKey(trg.SchemaName, trg.Name)} was not found"));
				}
			}

			if (messages.Count > 0)
			{
				throw new InvalidOperationException("The following triggers are not synchronised:\r\n" + string.Join("\r\n", messages));
			}
		}

		protected void RecreateViewsAndRoutines()
		{
			manager.ShowInfoMessage("Getting create scripts");
			var expectedList = GetViewAndRoutineScriptCollection();

			manager.ShowInfoMessage("Dropping service broker objects");
			DropServiceBrokerObjects();

			manager.ShowInfoMessage("Comparing scripts");
			CompareViewsAndRoutines(expectedList, out var dropList, out var createList, out var refreshList);

			manager.ShowInfoMessage("Dropping old, modified and dependent scripts");
			DropViewsAndRoutines(dropList);

			manager.ShowInfoMessage("Creating new and modified scripts");
			CreateOrRefreshViewsAndRoutines(expectedList, createList, refreshList);
		}

		#region GetViewAndRoutineScriptCollection

		protected virtual DbRoutineScriptCollection GetViewAndRoutineScriptCollection()
		{
			var result = new DbRoutineScriptCollection();

			manager.ShowInfoMessage("\tBase scripts");
			AppendGeneralScriptsToCollection(result);

			manager.ShowInfoMessage("\tClient Specific scripts");
			AppendClientSpecificViewAndRoutineScriptsToCollection(result);

			return result;
		}

		protected void AppendGeneralScriptsToCollection(DbRoutineScriptCollection scriptCollection)
		{
			var scripts = CoreScriptIndex.GetScripts()
				.Where(script => IncludeDevelopmentOnlyScripts || !script.GetType().IsDefined(typeof(DevelopmentOnlyAttribute), inherit: false));

			foreach (IDbScript script in scripts)
			{
				scriptCollection.Add(script);
			}
		}

		protected virtual bool IncludeDevelopmentOnlyScripts =>
#if DEBUG
			true;
#else
			false;
#endif

		protected void AppendClientSpecificViewAndRoutineScriptsToCollection(DbRoutineScriptCollection scriptCollection)
		{
			var extensionObjects = GlobalServiceProvider.Instance.GetService<IExtensionObjectsSource>()?.ExtensionObjects;
			if (extensionObjects == null)
			{
				return;
			}

			var clientScripts = extensionObjects.ViewAndRoutineCreationScripts;

			foreach (var clientScript in clientScripts)
			{
				var script = clientScript is DatabaseIndexedViewCreateScript indexedViewScript
					? new IndexedViewDbScript(clientScript.ObjectName, clientScript.CreateScript, clientScript.ObjectType, indexedViewScript.IndexCreateScript)
					: new DbScript(clientScript.ObjectName, clientScript.CreateScript, clientScript.ObjectType);
				scriptCollection.AddClientSpecific(script);
			}
		}

		#endregion // GetViewAndRoutineScriptCollection

		protected void DropServiceBrokerObjects()
		{
			try
			{
				var sqlText = @"
					DECLARE @Commands nvarchar(max) = '';
					SELECT @Commands = @Commands + 'DROP SERVICE [' + name + '];' FROM sys.services WHERE service_id >= 2^16;
					SELECT @Commands = @Commands + 'DROP CONTRACT [' + name + '];' FROM sys.service_contracts WHERE service_Contract_id >= 2^16;
					EXEC sp_executesql @Commands;";
				new BatchRunner().RunCommandsGeneratedByQuery(upgConnection, sqlText);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				string errorMessage = String.Format("Failed to drop Service Broker objects.\r\n{0}\r\n", e.Message);
				throw new Exception(errorMessage, e);
			}
		}

		#region CompareViewsAndRoutines

		protected void CompareViewsAndRoutines(IEnumerable<IDbScript> expectedList, out IEnumerable<IDbScript> dropList, out IEnumerable<IDbScript> createList, out IEnumerable<IDbScript> refreshList)
		{
			// Get all objects from DB
			// listToRefresh is a list of non-schemabound objects that potentially should be refreshed after schema change
			var actualListFromDb = GetActualDbScripts(out var listToRefresh);

			// Get list of objects that do not need any changes (matched by Schema, Name, Type and Text)
			var skipList =
				actualListFromDb.AsParallel()
					.Intersect(expectedList.AsParallel(), DbScriptComparer.SchemaNameTypeText)
					.ToList();

			// Get list of candidates to drop
			var candidatesToDrop =
				actualListFromDb.AsParallel()
					.Except(skipList.AsParallel(), DbScriptComparer.SchemaNameType)
					.ToList();

			// Get list of client objects
			var clientList =
				candidatesToDrop.AsParallel()
					.Except(expectedList.AsParallel(), DbScriptComparer.SchemaName)
					.Where(s => s.Name.StartsWith("Client", StringComparison.OrdinalIgnoreCase) || s.Name.StartsWith("RptDt", StringComparison.OrdinalIgnoreCase));

			//Get skiped object name list
			var skipObjectNameListCopy = SkipObjectNameList;
			var skipObjectNameList =
				candidatesToDrop.AsParallel()
					.Except(expectedList.AsParallel(), DbScriptComparer.SchemaName)
					.Where(s => skipObjectNameListCopy.Contains(s.Name));

			// Get list to drop from candidates but client objects
			var listToDrop =
				candidatesToDrop.AsParallel()
				.Except(clientList, DbScriptComparer.SchemaNameType)
				.Except(skipObjectNameList, DbScriptComparer.SchemaNameType);

			// Get list of objects to drop in dependency order
			dropList = GetDropListInDependencyOrder(listToDrop.ToList());

			// After checking dependency list to skip may be changed
			skipList =
				skipList.AsParallel()
				.Except(dropList.AsParallel(), DbScriptComparer.SchemaNameType)
				.ToList();

			// Get list of objects to create in random order
			createList =
				expectedList.AsParallel()
					.Except(skipList.AsParallel(), DbScriptComparer.SchemaNameType)
					.ToList();

			// Get list of the objects to refresh in random order. All skipped objects (non-schemabound only) need to be refreshed
			refreshList =
				listToRefresh.AsParallel()
					.Except(dropList.AsParallel(), DbScriptComparer.SchemaNameType)
					.Intersect(expectedList.AsParallel(), DbScriptComparer.SchemaNameType)
					.ToList();
		}

		List<IDbScript> GetActualDbScripts(out IEnumerable<IDbScript> listToRefresh)
		{
			var reservedSchemas = string.Join("','", Db.SqlReservedSchemas);

			var sql = FormattableString.Invariant($@"
SELECT
	sch_name           = s.name COLLATE database_default,
	obj_name           = o.name COLLATE database_default,
	obj_type           = o.type_desc COLLATE database_default,
	obj_text           = ISNULL(m.definition, N'') COLLATE database_default,
	obj_is_schemabound = CONVERT(bit, m.is_schema_bound)
FROM
	sys.objects          AS o
	JOIN sys.schemas     AS s ON s.schema_id = o.schema_id
	JOIN sys.sql_modules AS m ON m.object_id = o.object_id
WHERE 1=1
	AND o.is_ms_shipped = 0
	AND s.name NOT in ('{reservedSchemas}')
	AND o.type in ('P', 'V', 'TR', 'FN', 'IF', 'TF')

UNION ALL

SELECT
	sch_name           = SCHEMA_NAME(q.schema_id),
	obj_name           = q.name,
	obj_type           = q.type_desc,
	obj_text           = '',
	obj_is_schemabound = CONVERT(bit, 1)
FROM
	sys.service_queues AS q
WHERE 1=1
	AND q.is_ms_shipped = 0

UNION ALL

SELECT
	sch_name           = 'dbo',
	obj_name           = mt.name,
	obj_type           = 'SERVICE_MESSAGE_TYPE',
	obj_text           = '',
	obj_is_schemabound = CONVERT(bit, 1)
FROM
	sys.service_message_types AS mt
WHERE 1=1
	AND mt.message_type_id >= 2^16

UNION ALL

SELECT
	sch_name           = '',
	obj_name           = tr.name,
	obj_type           = 'SQL_TRIGGER',
	obj_text           = ISNULL(m.definition, N''),
	obj_is_schemabound = CONVERT(bit, 0)
FROM
	sys.triggers AS tr
	JOIN sys.sql_modules AS m ON m.object_id = tr.object_id
WHERE 1=1
	AND tr.parent_id = 0 -- only DDL triggers
;
"
			);

			var actualList = new List<IDbScript>();
			var refreshList = new List<IDbScript>();
			upgConnection.ExecuteReader(sql, (reader) =>
			{
				var sch_name = reader.GetString(0);
				var obj_name = reader.GetString(1);
				var obj_type = reader.GetString(2);
				var obj_text = reader.GetString(3);
				var obj_is_schemabound = reader.GetBoolean(4);

				if (!obj_is_schemabound || obj_type == DbRoutineType.SqlViewTypeDesc)
				{
					var refreshScript = FormattableString.Invariant($"EXEC sys.sp_refreshsqlmodule '{sch_name.QuoteName()}.{obj_name.QuoteName()}';");
					if (string.IsNullOrEmpty(sch_name) && obj_type == DbRoutineType.SqlTriggerTypeDesc)
					{
						refreshScript = FormattableString.Invariant($"EXEC sys.sp_refreshsqlmodule @name = '{obj_name.QuoteName()}' , @namespace = 'DATABASE_DDL_TRIGGER';");
					}

					refreshList.Add(new DbScript(sch_name, obj_name
						, refreshScript + (obj_is_schemabound ? " -- schemabound" : string.Empty)
						, obj_type));
				}

				actualList.Add(new DbScript(sch_name, obj_name, obj_text, obj_type));
			});

			listToRefresh = refreshList;
			return actualList;
		}

		List<IDbScript> GetDropListInDependencyOrder(List<IDbScript> listToDrop)
		{
			var result = new List<IDbScript>();

			if (listToDrop.Count > 0)
			{
				var objects = String.Join(" OR ", listToDrop.Where(s => supportedObjectTypes.Contains(s.ObjectType)).Select(s => String.Format("sch.name = '{0}' AND obj.name = '{1}'", s.SchemaName, s.Name)));
				var messageTypes = String.Join(",", listToDrop.Where(s => s.ObjectType == DbRoutineType.SqlMessageTypeDesc).Select(s => String.Format("'{0}'", s.Name)));
				var queues = String.Join(",", listToDrop.Where(s => s.ObjectType == DbRoutineType.SqlQueueTypeDesc).Select(s => String.Format("'{0}'", s.Name)));
				var triggers = string.Join(",", listToDrop.Where(s => s.ObjectType == DbRoutineType.SqlTriggerTypeDesc).Select(s => $"'{s.Name}'"));

				var sql = String.Format(@"
WITH
	cte AS
		(
			SELECT
				obj_id     = obj.object_id,
				obj_schema = sch.name,
				obj_name   = obj.name,
				obj_type   = obj.type_desc,
				obj_level  = 1
			FROM
				sys.objects      AS obj
				JOIN sys.schemas AS sch ON sch.schema_id = obj.schema_id
			WHERE 1=1
				AND obj.is_ms_shipped = 0
				AND obj.type in ('P', 'V', 'TR', 'FN', 'TF', 'IF')
				AND obj.name NOT LIKE 'TG[_]%[_]UpdateAutoVersion'
				AND obj.name NOT LIKE 'TG[_]%[_]AuditDetailsAreNotMissing[_]Insert'
				AND obj.name NOT LIKE 'TG[_]%[_]AuditDetailsAreNotMissing[_]Update'
				AND obj.name NOT LIKE 'TG[_]%[_]SystemLastEditAuditInfoMustBeUpdated[_]Update'
				AND ({0})

			UNION ALL
			-- Recursive step
			SELECT
				obj_id     = dep.referencing_id,
				obj_schema = SCHEMA_NAME(obj.schema_id),
				obj_name   = obj.name,
				obj_type   = obj.type_desc,
				obj_level  = cte.obj_level + 1
			FROM
				cte
				JOIN sys.sql_expression_dependencies AS dep ON dep.referenced_id = cte.obj_id
				JOIN sys.objects                     AS obj ON obj.object_id = dep.referencing_id
			WHERE 1=1
				AND obj.is_ms_shipped = 0
				AND obj.type in ('P', 'V', 'FN', 'TF', 'IF')
				AND dep.is_schema_bound_reference = 1
				AND dep.referenced_minor_id = 0

			UNION ALL
			-- Recursive step
			SELECT
				obj_id     = obj.object_id,
				obj_schema = SCHEMA_NAME(obj.schema_id),
				obj_name   = obj.name,
				obj_type   = obj.type_desc,
				obj_level  = cte.obj_level + 1
			FROM
				cte
				JOIN sys.objects AS obj ON obj.parent_object_id = cte.obj_id
			WHERE 1=1
				AND obj.is_ms_shipped = 0
				AND obj.type in ('TR')
		),
	GroupedData AS
		(
			SELECT
				obj_schema, obj_name, obj_type
				, DropOrder = MAX(obj_level)
			FROM
				cte
			GROUP BY
				obj_schema, obj_name, obj_type
		)
SELECT
	obj_schema = obj_schema COLLATE database_default,
	obj_name   = obj_name COLLATE database_default,
	obj_type   = obj_type COLLATE database_default,
	obj_text =
		CASE obj_type
			WHEN 'SQL_STORED_PROCEDURE' THEN 'DROP PROCEDURE '
			WHEN 'VIEW'                 THEN 'DROP VIEW '
			WHEN 'SQL_TRIGGER'          THEN 'DROP TRIGGER '
			ELSE                             'DROP FUNCTION '
		END + QUOTENAME(obj_schema) + '.' + QUOTENAME(obj_name) + ';' COLLATE database_default,
	DropOrder =
		CASE obj_type
			WHEN 'SQL_TRIGGER' THEN 3000000 -- drop triggers first
			ELSE DropOrder
		END
FROM
	GroupedData

UNION ALL

SELECT
	obj_schema = 'dbo',
	obj_name   = mt.name,
	obj_type   = 'SERVICE_MESSAGE_TYPE',
	obj_text   = 'DROP MESSAGE TYPE ' + QUOTENAME(mt.name) + ';',
	DropOrder  = 2000000
FROM
	sys.service_message_types AS mt
WHERE 1=1
	AND mt.message_type_id >= 2^16
	AND {1}

UNION ALL

SELECT
	obj_schema = SCHEMA_NAME(q.schema_id),
	obj_name   = q.name,
	obj_type   = q.type_desc,
	obj_text   = 'DROP QUEUE ' + QUOTENAME(q.name) + ';',
	DropOrder  = 1000000
FROM
	sys.service_queues AS q
WHERE 1=1
	AND q.is_ms_shipped = 0
	AND {2}

UNION ALL

SELECT
	obj_schema = '',
	obj_name   = tr.name,
	obj_type   = tr.type_desc,
	obj_text   = 'DROP TRIGGER ' + QUOTENAME(tr.name) + ' ON DATABASE;',
	DropOrder  = 3000000
FROM
	sys.triggers AS tr
WHERE 1=1
	AND tr.parent_id = 0
	AND {3}

ORDER BY
	DropOrder DESC
;",
					(String.IsNullOrWhiteSpace(objects)) ? "1=2" : objects,
					(String.IsNullOrWhiteSpace(messageTypes)) ? "1=2" : String.Format("mt.name in ({0})", messageTypes),
					(String.IsNullOrWhiteSpace(queues)) ? "1=2" : String.Format("q.name in ({0})", queues),
					(String.IsNullOrWhiteSpace(triggers)) ? "1=2" : $"tr.name in ({triggers})");

				using (var reader = upgConnection.Command(sql).ExecuteReader())
				{
					while (reader.Read())
					{
						var obj_schema = reader.GetString(0);
						var obj_name = reader.GetString(1);
						var obj_type = reader.GetString(2);
						var obj_text = reader.GetString(3);

						result.Add(new DbScript(obj_schema, obj_name, obj_text, obj_type));
					}
				}
			}

			return result;
		}

		protected virtual List<string> SkipObjectNameList
		{
			get
			{
				var result = new List<string>();
				result.AddRange(new string[] {
					"tr_MScdc_ddl_event"
				});
				return result;
			}
		}

#endregion // CompareViewsAndRoutines

		protected void DropViewsAndRoutines(IEnumerable<IDbScript> dropList)
		{
			try
			{
				foreach (var item in dropList)
				{
					if (!triggerTransformations.Contains(item, DbScriptComparer.SchemaNameType))
					{
						manager.ShowInfoMessage("    (-) " + GetKey(item.SchemaName, item.Name) + " (" + item.ObjectType + ")");
						upgConnection.ExecuteNonQuery(item.Text);
					}
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				string errorMessage = String.Format("Failed to drop existing views/procedures/triggers/functions.\r\n{0}\r\n", e.Message);
				throw new Exception(errorMessage, e);
			}
		}

		protected void CreateOrRefreshViewsAndRoutines(IEnumerable<IDbScript> expectList, IEnumerable<IDbScript> createList, IEnumerable<IDbScript> refreshList)
		{
			// Get list to create / refresh in creation order
			var listToCreateOrRefresh = expectList.Join(createList.Union(refreshList),
				(expected) => expected,
				(toCreateRefresh) => toCreateRefresh,
				(expected, toCreateRefresh) => (expected, toCreateRefresh),
				DbScriptComparer.SchemaNameType);

			var lastScriptName = String.Empty;
			try
			{
				foreach (var item in listToCreateOrRefresh)
				{
					lastScriptName = GetKey(item.toCreateRefresh.SchemaName, item.toCreateRefresh.Name);

					var isRefresh = item.toCreateRefresh.Text.StartsWith("EXEC sys.sp_refreshsqlmodule", StringComparison.OrdinalIgnoreCase);
					if (isRefresh)
					{
						if (item.toCreateRefresh.ObjectType == DbRoutineType.SqlViewTypeDesc)
						{
							if (item.expected is DbCreateIndexedViewScript indexedView)
							{
								SynchroniseViewIndexes(indexedView);
							}
						}

						if (!item.toCreateRefresh.Text.EndsWith("-- schemabound"))
						{
							manager.ShowInfoMessage($"    (~) Refreshing module {lastScriptName} ({item.toCreateRefresh.ObjectType})");
							upgConnection.ExecuteNonQuery(item.toCreateRefresh.Text);
						}
					}
					else if (!triggerTransformations.Contains(item.toCreateRefresh, DbScriptComparer.SchemaNameType))
					{
						manager.ShowInfoMessage("    (+) " + lastScriptName + " (" + item.toCreateRefresh.ObjectType + ")");
						upgConnection.ExecuteNonQuery(item.toCreateRefresh.Text);

						if (item.toCreateRefresh is IIndexedViewDbScript view)
						{
							manager.ShowInfoMessage("        (~) Creating indexes for view: " + lastScriptName);
							upgConnection.ExecuteNonQuery(view.IndexCreateScript);
						}

						if (item.toCreateRefresh is IIndexedViewWithTemporaryIndexesOrColumns source)
						{
							manager.ShowInfoMessage("        (-) Dropping temporary indexes and columns used to create view: " + lastScriptName);
							DropIndexedViewTemporaryIndexesAndColumns(source);
						}
					}
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				throw new Exception(e.Message + "\r\nLast script name: " + lastScriptName + "\r\n", e);
			}
		}

		void SynchroniseViewIndexes(DbCreateIndexedViewScript view)
		{
			var actualIndexes = IndexLoader.Load(upgConnection, view.SchemaName, view.Name, indexName: null).ToDictionary(i => i.IndexName, StringComparer.OrdinalIgnoreCase);
			var newIndexes = view.Indexes.ToDictionary(i => i.IndexName, StringComparer.OrdinalIgnoreCase);
			var hasHeaderLogged = false;

			foreach (var actual in actualIndexes.Values.Except(newIndexes.Values).OrderByDescending(x => x.IsClustered).ThenBy(x => x.ComparableDefinition))
			{
				if (newIndexes.TryGetValue(actual.IndexName, out var newIndex) && actual.CompareTo(newIndex) is 1 or -1)
				{
					// The index already exists but can be altered without dropping
					continue;
				}

				LogHeader();
				manager.ShowInfoMessage($"        (-) {actual.Definition}");
				actual.Drop(upgConnection);
				if (actual.IsClustered)
				{
					// dropping clustered index results in dropping all nonclustered indexes, so we need to refresh actual list
					actualIndexes = IndexLoader.Load(upgConnection, view.SchemaName, view.Name, indexName: null).ToDictionary(i => i.IndexName, StringComparer.OrdinalIgnoreCase);
					break;
				}
			}

			foreach (var index in newIndexes.Values.Except(actualIndexes.Values).OrderByDescending(x => x.IsClustered).ThenBy(x => x.ComparableDefinition))
			{
				if (actualIndexes.TryGetValue(index.IndexName, out var actual) && index.CompareTo(actual) is 1 or -1)
				{
					// The index can be altered using ALTER INDEX ... SET (...) statement
					LogHeader();
					var alterCommand = index.SQL_SET;
					manager.ShowInfoMessage($"        (~) {alterCommand}");
					upgConnection.ExecuteNonQuery(alterCommand);
				}
				else
				{
					LogHeader();
					manager.ShowInfoMessage($"        (+) {index.Definition}");
					index.Create(upgConnection);
				}
			}

			return;

			void LogHeader()
			{
				if (!hasHeaderLogged)
				{
					manager.ShowInfoMessage($"    Synchronising indexes for indexed view: [{view.SchemaName}].[{view.Name}]");
					hasHeaderLogged = true;
				}
			}
		}

		public void CreateTemporaryScriptIndexes()
		{
			manager.StartTask(String.Format("*** Database: {0} ***", dbBeingUpgraded));

			try
			{
				using (((ICurrentDbControl)upgConnection).UseDatabase(dbBeingUpgraded))
				{
					manager.StartTask("Creating temporary indexes and columns for offline indexed view synchronisation");

					var expectedList = GetViewAndRoutineScriptCollection().Where(s => s is IIndexedViewDbScript indexedView && indexedView is IIndexedViewWithTemporaryIndexesOrColumns).ToList();

					manager.ShowInfoMessage("Comparing indexed views");
					CompareIndexedViews(expectedList, out var createList);

					manager.ShowInfoMessage("Creating temporary indexes and columns for offline indexed view synchronisation");
					CreateTemporaryIndexesAndColumnsForIndexedViews(expectedList, createList);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string message = $"Failed to create pre-indexed views - Database [{dbBeingUpgraded}].\r\n{ex.Message}";
				throw new Exception(message, ex);
			}
		}

		void CreateTemporaryIndexesAndColumnsForIndexedViews(IEnumerable<IDbScript> expectList, IEnumerable<IDbScript> createList)
		{
			// Get list to create / refresh in creation order
			var listToCreateOrRefresh = expectList.Join(createList,
				(expected) => expected,
				(toCreateRefresh) => toCreateRefresh,
				(expected, toCreateRefresh) => (expected, toCreateRefresh),
				DbScriptComparer.SchemaNameType);

			foreach (var item in listToCreateOrRefresh)
			{
				var lastScriptName = GetKey(item.toCreateRefresh.SchemaName, item.toCreateRefresh.Name);

				if (item.expected is IIndexedViewWithTemporaryIndexesOrColumns source)
				{
					var columnCreateScripts = source.TemporaryComputedColumns
						.GroupBy(c => (SchemaName: c.SchemaName, TableName: c.TableName))
						.Where(group => DbObjectCreator.TableExists(upgConnection, upgConnection.CurrentDatabase, group.Key.TableName, group.Key.SchemaName))
						.SelectMany(group =>
							group.Select(c =>
								{
									var databaseColInfo = ComputedColumnLoader.LoadTop1(upgConnection, c.SchemaName, c.TableName, c.ColumnName);
									if (databaseColInfo == null)
									{
										return c;
									}

									var dbFormatColInfo = ComputedColumnLoader.ToDatabaseFormat(upgConnection, c);
									if (!databaseColInfo.Equals(dbFormatColInfo))
									{
										new DbColumnDependencyRemover(c.SchemaName, c.TableName, c.ColumnName).DropRelateObjects(upgConnection);
										upgConnection.ExecuteNonQuery(c.DropDefinition);
										return c;
									}

									return null;
								})
								.WhereNotNull()
								.Select(c => c.AddDefinition))
						.ToArray();

					if (columnCreateScripts.Any())
					{
						manager.ShowInfoMessage("    (+) Creating temporary columns for view: " + lastScriptName);
						upgConnection.ExecuteNonQuery(string.Join("\r\n", columnCreateScripts));
					}

					var indexCreateScripts = source.TemporaryIndexes
						.GroupBy(c => (SchemaName: c.SchemaName, TableName: c.TableName))
						.Where(group => DbObjectCreator.TableExists(upgConnection, upgConnection.CurrentDatabase, group.Key.TableName, group.Key.SchemaName))
						.SelectMany(group =>
							group.Where(index =>
									DbObjectCreator.ColumnsExist(
										upgConnection,
										upgConnection.CurrentDatabase,
										group.Key.SchemaName,
										group.Key.TableName,
										index.KeyColumns.Union(index.IncludedColumns).Select(c => c.Name).ToArray()))
								.Select(indexInfo =>
								{
									var databaseIndexInfo = IndexLoader.LoadTop1(upgConnection, group.Key.SchemaName, group.Key.TableName, indexInfo.IndexName);
									if (databaseIndexInfo == null)
									{
										return indexInfo;
									}

									if (!databaseIndexInfo.Equals(indexInfo))
									{
										return IndexInfo.Builder.Copy(indexInfo).Option(IndexOptions.DROP_EXISTING, true).GetInfo();
									}

									return null;
								})
								.WhereNotNull()
								.Select(indexInfo => indexInfo.SQL_Create))
						.ToArray();

					if (indexCreateScripts.Any())
					{
						manager.ShowInfoMessage("        (~) Creating index for view: " + lastScriptName);
						foreach (var indexCreateScript in indexCreateScripts)
						{
							BacklogWaiter.WaitUntilBacklogIsAcceptable((elapsed, nextWaitTime, success, failureReason, bytesBacklog) =>
							{
								manager.ShowInfoMessage("            waiting for backlog to clear, " + (success ? bytesBacklog.BacklogDescription : failureReason) + ". Please run the LBK service task to reduce log fullness.");
							});

							upgConnection.ExecuteNonQuery(indexCreateScript);
						}
					}
				}
			}
		}

		BacklogWaiter BacklogWaiter => backlogWaiter ?? (backlogWaiter = new BacklogWaiter(new[] { CreateBacklogInfoProvider() }));
		BacklogWaiter backlogWaiter;

		protected virtual IBacklogInfoProvider CreateBacklogInfoProvider() => new LogFullnessProvider();

		void DropIndexedViewTemporaryIndexesAndColumns(IIndexedViewWithTemporaryIndexesOrColumns source)
		{
			upgConnection.ExecuteNonQuery(string.Join("\r\n", source.TemporaryIndexes.Select(c => c.SQL_Drop)));
			var columnDropScripts = source.TemporaryComputedColumns
				.GroupBy(c => (SchemaName: c.SchemaName, TableName: c.TableName))
				.Where(group => DbObjectCreator.TableExists(upgConnection, upgConnection.CurrentDatabase, group.Key.TableName, group.Key.SchemaName))
				.SelectMany(group => group.Select(c => c.DropDefinition))
				.ToList();
			if (columnDropScripts.Any())
			{
				upgConnection.ExecuteNonQuery(string.Join("\r\n", columnDropScripts));
			}
		}

		void CompareIndexedViews(IEnumerable<IDbScript> expectedList, out IEnumerable<IDbScript> createList)
		{
			// Get all relevant objects from DB
			var actualListFromDb = GetActualDbIndexedViews(expectedList);

			// Get list of objects that do not need any changes (matched by Schema, Name, Type and Text)
			var skipList =
				actualListFromDb.AsParallel()
					.Intersect(expectedList.AsParallel(), DbScriptComparer.SchemaNameTypeText)
					.ToList();

			// Get list of objects to create in random order
			createList =
				expectedList.AsParallel()
					.Except(skipList.AsParallel(), DbScriptComparer.SchemaNameType)
					.ToList();
		}

		IList<IDbScript> GetActualDbIndexedViews(IEnumerable<IDbScript> expectedList)
		{
			var expectedViews = string.Join("','", expectedList.Select(x => x.Name));
			var sql = $@"SELECT
	sch_name           = s.name COLLATE database_default,
	obj_name           = o.name COLLATE database_default,
	obj_type           = o.type_desc COLLATE database_default,
	obj_text           = m.definition COLLATE database_default
FROM
	sys.objects          AS o
	JOIN sys.schemas     AS s ON s.schema_id = o.schema_id
	JOIN sys.sql_modules AS m ON m.object_id = o.object_id
WHERE 1=1
	AND o.is_ms_shipped = 0
	AND o.type = 'V'
	and o.name IN ('{expectedViews}')
";

			var actualList = new List<IDbScript>();
			upgConnection.ExecuteReader(sql, reader =>
			{
				var sch_name = reader.GetString(0);
				var obj_name = reader.GetString(1);
				var obj_type = reader.GetString(2);
				var obj_text = reader.GetString(3);

				actualList.Add(new DbScript(sch_name, obj_name, obj_text, obj_type));
			});

			return actualList;
		}

		#region Implementation

		string GetKey(string schemaName, string objectName)
		{
			return schemaName.IsNullOrEmpty()
				? FormattableString.Invariant($"[{objectName}]")
				: FormattableString.Invariant($"[{schemaName}].[{objectName}]");
		}

		readonly SortedSet<string> supportedObjectTypes = new SortedSet<string>(new string[]
			{
				DbRoutineType.SqlProcedureTypeDesc,
				DbRoutineType.SqlViewTypeDesc,
				DbRoutineType.SqlTriggerTypeDesc,
				DbRoutineType.SqlFunctionScalarTypeDesc,
				DbRoutineType.SqlFunctionInlineTypeDesc,
				DbRoutineType.SqlFunctionTableTypeDesc,
			},
			StringComparer.OrdinalIgnoreCase);

		#endregion // Implementation
	}
}
