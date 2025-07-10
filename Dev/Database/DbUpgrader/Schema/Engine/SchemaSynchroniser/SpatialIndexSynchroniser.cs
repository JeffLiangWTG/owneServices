using System.Collections.Generic;
using System.Globalization;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema
{
	class SpatialIndexSynchroniser : MetadataScriptRunner
	{
		public SpatialIndexSynchroniser(DbConnection upgConnection, string dbBeingUpgraded, string templateDb, IUpgradeTaskWorkflowLogger logger)
			: base(upgConnection, dbBeingUpgraded, templateDb, logger)
		{
		}

		/// <summary>
		/// --------------------------------------------------------------------------------------------------------
		/// -- Run scripts to DROP old and CREATE new spatial indexes
		/// --
		/// -- Note 1: Tags {0} and {1} are replaced by the actual
		/// --         DbBeingUpgraded and TemplateDb names at run time
		/// --
		/// -- Note 2: Only handles SPATIAL indexes.
		/// --
		/// --         Index TYPE information:
		/// --           4 => Spatial
		/// --
		/// -- Note 3: Indexes of tables that are not in the new schema are IGNORED
		/// --------------------------------------------------------------------------------------------------------
		/// </summary>
		public void SynchroniseAll()
		{
			var listToCreate = new List<SpatialIndexInfo>();
			var listToAlter = new List<SpatialIndexInfo>();
			var listToSetOptions = new List<SpatialIndexInfo>();
			var candidatesToDrop = new List<SpatialIndexInfo>();
			var listToDrop = new List<SpatialIndexInfo>();

			List<SpatialIndexInfo> templateIndexes = null;
			using (((ICurrentDbControl)upgConnection).UseDatabase(templateDb))
			{
				templateIndexes = SpatialIndexLoader.Load(upgConnection);
			}

			var currentIndexes = SpatialIndexLoader.Load(upgConnection);

			// merge indexes by name, than by definition
			// NOTE: assumed the lists are pre-ordered by SchemaName, TableName, IndexName
			var newInd = 0;
			var curInd = 0;
			while (newInd < templateIndexes.Count && curInd < currentIndexes.Count)
			{
				var template = templateIndexes[newInd];
				var current = currentIndexes[curInd];

				var diff = template.CompareTo(current);
				switch (diff)
				{
					case -3: // completely different
						listToCreate.Add(template);
						newInd++;

						break;
					case -2: // different with equal "SchemaName.TableName.IndexName"
						listToAlter.Add(template);
						newInd++;
						curInd++;

						break;
					case -1: // Equal but options that can be set without recreate are changed
						listToSetOptions.Add(template);
						newInd++;
						curInd++;

						break;
					case 0: // equal
						newInd++;
						curInd++;

						break;
					case 1: // Equal but options that can be set without recreate are changed
						listToSetOptions.Add(template);
						newInd++;
						curInd++;

						break;
					case 2: // different with equal "SchemaName.TableName.IndexName"
						listToAlter.Add(template);
						newInd++;
						curInd++;

						break;
					case 3: // completely different, can be skipped here because IndexScriptRunner drops old indexes before
						candidatesToDrop.Add(current);
						curInd++;

						break;
				}
			}

			if (newInd < templateIndexes.Count)
			{
				for (int i = newInd; i < templateIndexes.Count; i++)
				{
					listToCreate.Add(templateIndexes[i]);
				}
			}
			else
			{
				for (int i = curInd; i < currentIndexes.Count; i++)
				{
					candidatesToDrop.Add(currentIndexes[i]);
				}
			}

			if (candidatesToDrop.Count > 0)
			{
				foreach (var index in candidatesToDrop)
				{
					if (DbObjectCreator.TableExists(upgConnection, templateDb, index.TableName, index.SchemaName))
					{
						listToDrop.Add(index);
					}
				}

				if (listToDrop.Count > 0)
				{
					taskLogger.StartSubtask("Dropping old indexes");

					foreach (var index in listToDrop)
					{
						taskLogger.StartSubtask(string.Format(CultureInfo.InvariantCulture, "    (-) {0}", index.ComparableDefinition));
						upgConnection.ExecuteNonQuery(index.SQL_Drop);
					}
				}
			}

			if (listToAlter.Count > 0)
			{
				taskLogger.StartSubtask("Altering modified indexes");
				foreach (var index in listToAlter)
				{
					taskLogger.StartSubtask(string.Format(CultureInfo.InvariantCulture, "    (~) {0}", index.ComparableDefinition));
					index.Create(upgConnection);
				}
			}

			if (listToCreate.Count > 0)
			{
				taskLogger.StartSubtask("Creating new indexes");
				foreach (var index in listToCreate)
				{
					taskLogger.StartSubtask(string.Format(CultureInfo.InvariantCulture, "    (+) {0}", index.ComparableDefinition));
					upgConnection.ExecuteNonQuery(index.SQL_Create);
				}
			}

			if (listToSetOptions.Count > 0)
			{
				taskLogger.StartSubtask("Altering indexes with modified options");
				foreach (var index in listToSetOptions)
				{
					taskLogger.StartSubtask(string.Format(CultureInfo.InvariantCulture, "    (~) {0}", index.ComparableDefinition));
					upgConnection.ExecuteNonQuery(index.SQL_SET);
				}
			}
		}
	}
}

#region Test
#if DEBUG

// ********************************************************************************* //
// DB SYCHRONISATION TESTED BY Enterprise.DbUpgrader.Schema.SchemaUpgraderEngineTest //
// ********************************************************************************* //

#endif
#endregion // Test
