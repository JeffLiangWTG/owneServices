using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	public class ReferenceDbSynonymRecreateDirector : BaseRefDbSynonymSynchroniser, IUpgradeTaskWorkflowLogger
	{
		public ReferenceDbSynonymRecreateDirector(IUpgradeContext upgradeContext, DbConnection connection)
		{
			this.upgradeContext = upgradeContext ?? throw new ArgumentNullException(nameof(upgradeContext));
			this.connection = connection;
		}

		readonly DbConnection connection;

		#region Synchronise database synonyms

		public override void SynchroniseReferenceDbSynonyms()
		{
			var factories = ReferenceDbUpgradeDirector.ReferenceDbUpgraderFactories;
			var usedSynonymPrefixes = new List<string>();
			percentComplete = 0;
			percentCompleteIncrement = 100 / (factories.Count() + 1);

			foreach (var factory in factories)
			{
				if (!isCancelled)
				{
					var upgrader = factory.New(upgradeContext, connection, this);
					SynchroniseSpecificReferenceDbSynonyms(upgrader);
					usedSynonymPrefixes.Add(upgrader.SynonymPrefix);
					percentComplete += percentCompleteIncrement;
				}
			}

			if (!isCancelled)
			{
				DropOldRefDbSynonyms(connection, usedSynonymPrefixes);
			}

			if (!isCancelled)
			{
				var refDbRecreator = new RefDatabaseSynonymRecreator(connection);
				SynchroniseRefDatabaseDbSynonyms(refDbRecreator);
			}

			((IPhysicalRefDbLocation)connection).ClearRefDbNameBuffers();
		}

		public void SynchroniseRefDatabaseDbSynonyms(RefDatabaseSynonymRecreator refDbRecreator)
		{
			var dbDescription = string.Concat("[", RefDbTableNameResolver.SingleRefDatabaseName, "]");
			try
			{
				if (connection.DatabaseExists(RefDbTableNameResolver.SingleRefDatabaseName))
				{
					OnStatusChange(new StatusChangedEventArgs(percentComplete, RefDbTableNameResolver.SingleRefDatabaseName, StatusCode.SynchronisingSynonyms));
					refDbRecreator.RecreateSynonym();
					successes.Add(dbDescription);
				}
				else
				{
					errors.Add(string.Concat(dbDescription, " - Database does not exist."));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				errors.Add(string.Concat(dbDescription, " - ", ex.Message));
			}
		}

		void SynchroniseSpecificReferenceDbSynonyms(ReferenceDbUpgrader upgrader)
		{
			var dbDescription = string.Format("{0} {1} [{2}]", upgrader.CountryCode, upgrader.DatabaseType, upgrader.DbName);

			try
			{
				if (connection.DatabaseExists(upgrader.DbName))
				{
					SynchroniseSynonyms(upgrader, dbDescription);
				}
				else if (CanDatabaseBeCreatedFromScratch(upgrader))
				{
					CreateUpgradeAndSynchroniseSynonyms(upgrader, dbDescription);
				}
				else
				{
					errors.Add(string.Concat(dbDescription, " - Database does not exist and cannot be created from scratch."));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				errors.Add(string.Concat(dbDescription, " - ", ex.Message));
			}
		}

		bool CanDatabaseBeCreatedFromScratch(ReferenceDbUpgrader upgrader)
		{
			bool result = (
				!RefDbTableNameResolver.IsSharedDatabase(upgrader.DbName)
				|| upgrader.CountryCode.Equals("IN", StringComparison.OrdinalIgnoreCase)
				|| upgrader.CountryCode.Equals("ZZ", StringComparison.OrdinalIgnoreCase)
			);

			return result;
		}

		void SynchroniseSynonyms(ReferenceDbUpgrader upgrader, string dbDescription)
		{
			OnStatusChange(new StatusChangedEventArgs(percentComplete, dbDescription, StatusCode.SynchronisingSynonyms));
			upgrader.SynchroniseSynonyms();
			successes.Add(dbDescription);
		}

		void CreateUpgradeAndSynchroniseSynonyms(ReferenceDbUpgrader upgrader, string dbDescription)
		{
			OnStatusChange(new StatusChangedEventArgs(percentComplete, dbDescription, StatusCode.CreatingDatabase));
			upgrader.CreateAndUpgradeIfRequired();
			newReferenceDbs.Add(dbDescription);
			successes.Add(dbDescription);
		}

		internal static void DropOldRefDbSynonyms(DbConnection cleanupSynonymConnection, IEnumerable<string> usedSynonymPrefixes)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT 'DROP SYNONYM [' + OBJECT_SCHEMA_NAME (object_id) + '].[' + sn.name + '];' FROM sys.synonyms sn WHERE sn.name like '{0}%'{1}",
				RefDbTableNameResolver.RefDbAffix,
				string.Join("", usedSynonymPrefixes.Select(usp => " AND sn.name not like '" + DataUtils.ReplaceSqlLikeWildcard(usp) + "%'"))
			);

			var batchRunner = new BatchRunner();
			batchRunner.RunCommandsGeneratedByQuery(cleanupSynonymConnection, sqlText);
		}

		#endregion

		#region Errors, successes, new collections

		public override bool HasErrors()
		{
			return (errors.Count != 0);
		}

		public override bool HasSuccesses()
		{
			return (successes.Count != 0);
		}

		public override IEnumerable<string> Errors
		{
			get { return errors; }
		}
		readonly List<string> errors = new List<string>();

		public override IEnumerable<string> Successes
		{
			get { return successes; }
		}
		readonly List<string> successes = new List<string>();

		public override IEnumerable<string> NewReferenceDbs
		{
			get { return newReferenceDbs; }
		}
		readonly List<string> newReferenceDbs = new List<string>();

		#endregion

		#region Status change and cancel events

		public override void Cancelled(object sender, EventArgs e)
		{
			isCancelled = true;
			OnStatusChange(new StatusChangedEventArgs(percentComplete, string.Empty, StatusCode.Cancelling));
		}

		public override bool IsCancelled
		{
			get { return isCancelled; }
		}

		public bool isCancelled;
		int percentComplete;
		int percentCompleteIncrement;

		#endregion

		void IUpgradeTaskWorkflowLogger.StartTask(string task)
		{
		}

		void IUpgradeTaskWorkflowLogger.StartSubtask(string subtask)
		{
		}

		void IUpgradeTaskWorkflowLogger.ShowTaskError(string errorMessage)
		{
			errors.Add(errorMessage);
		}

		void IUpgradeTaskWorkflowLogger.ActivateTaskProgress(int numOfTasks)
		{
		}

		void IUpgradeTaskWorkflowLogger.ActivateSubtaskProgress(int numOfSubtasks)
		{
		}

		void IUpgradeTaskWorkflowLogger.ShowInfoMessage(string infoMessage)
		{
		}

		readonly IUpgradeContext upgradeContext;
	}
}
