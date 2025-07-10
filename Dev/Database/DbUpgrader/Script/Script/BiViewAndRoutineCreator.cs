using System.Collections.Generic;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Script
{
	abstract class BiViewAndRoutineCreator : ViewAndRoutineCreator
	{
		protected BiViewAndRoutineCreator(IUpgradeManager manager, DbConnection upgConnection, string upgradingDb)
			: base(manager, upgConnection, upgradingDb)
		{ }

		public bool DoesBiDatabaseExist()
		{
			return upgConnection.DatabaseExists(dbBeingUpgraded);
		}

		protected override DbRoutineScriptCollection GetViewAndRoutineScriptCollection()
		{
			var result = new DbRoutineScriptCollection();
			AppendScriptsToCollection(result);
			return result;
		}

		protected virtual void AppendScriptsToCollection(DbRoutineScriptCollection scriptCollection)
		{
			var biScriptCollection = GetScriptCollection();

			foreach (IDbScript biScript in biScriptCollection)
			{
				scriptCollection.Add(biScript);
			}
		}

		protected abstract IEnumerable<IDbScript> GetScriptCollection();
	}

	class AuditViewAndRoutineCreator : BiViewAndRoutineCreator
	{
		public static AuditViewAndRoutineCreator New(IUpgradeManager manager, DbConnection upgConnection, string mainDbName)
		{
			string auditDatabaseName = mainDbName + Db.AuditDatabaseSuffix;
			return new AuditViewAndRoutineCreator(manager, upgConnection, auditDatabaseName);
		}

		protected AuditViewAndRoutineCreator(IUpgradeManager manager, DbConnection upgConnection, string upgradingDb)
			: base(manager, upgConnection, upgradingDb)
		{
		}

		protected override IEnumerable<IDbScript> GetScriptCollection() => CoreAuditScriptIndex.GetScripts();
	}

	class EdwViewAndRoutineCreator : BiViewAndRoutineCreator
	{
		public static EdwViewAndRoutineCreator New(IUpgradeManager manager, DbConnection upgConnection, string mainDbName)
		{
			string edwDatabaseName = mainDbName + Db.EdwDatabaseSuffix;
			return new EdwViewAndRoutineCreator(manager, upgConnection, edwDatabaseName);
		}

		protected EdwViewAndRoutineCreator(IUpgradeManager manager, DbConnection upgConnection, string upgradingDb)
			: base(manager, upgConnection, upgradingDb)
		{
		}

		protected override IEnumerable<IDbScript> GetScriptCollection() => CoreEdwScriptIndex.GetScripts();

		protected override void AppendScriptsToCollection(DbRoutineScriptCollection scriptCollection)
		{
			var biScriptCollection = GetScriptCollection();
			AddBiScriptsToCollection(scriptCollection, biScriptCollection);
		}

		protected virtual void AddBiScriptsToCollection(DbRoutineScriptCollection scriptCollection, IEnumerable<IDbScript> biScriptCollection)
		{
			foreach (IDbScript biScript in biScriptCollection)
			{
				scriptCollection.Add(biScript);
			}

			if (upgConnection.DatabaseExists(dbBeingUpgraded) &&
				!string.IsNullOrEmpty(DataUtils.LoadDbExtendedProperty(upgConnection, BiConstants.MainDbSchemaVersionExtPtyName, dbBeingUpgraded)))
			{
				foreach (IDbScript biScript in CoreEdwScriptIndex.GetModelScripts())
				{
					scriptCollection.Add(biScript);
				}
			}
		}
	}
}
