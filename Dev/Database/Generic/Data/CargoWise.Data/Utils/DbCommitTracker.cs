#if DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Data
{
	public static class DbCommitTracker
	{
		internal static void OnExecute(DbCommand command)
		{
			Argument.NotNull(command, nameof(command));

			if (TestingState.IsRunningTests
				&& command.DbConnection is var dbConnection
				&& !dbConnection.IgnoreCommitTracker
				&& command.CommandText is var commandText
				&& updateRegex.IsMatch(commandText))
			{
				var dbNames = SqlSynonymNameResolver.GetReferencedDatabasesFromSynonyms(commandText)
					.Concat([GetDatabaseName(command)])
					.ToArray();

				foreach (var dbName in dbNames.Where(x => !string.IsNullOrEmpty(x)))
				{
					lock (lockObj)
					{
						if (RefDbTableNameResolver.IsExclusiveDatabase(Db.DatabaseName, dbName))
						{
							continue;
						}

						var commandsTable = dbConnection.IsInTransaction ? updatesInTransaction : updates;
						commandsTable.Add(dbConnection.ObjectID, dbName, BuildCommandStringWithParameters(command));
					}
				}
			}
		}

		static string GetDatabaseName(DbCommand command)
		{
			// If we aren't in context of CW1 application and current db isn't master, then current db might be the main db.
			if (!Db.DatabaseNameIsInitialized)
			{
				return (!string.Equals(command.Connection.Database, Db.SqlMasterDb, StringComparison.OrdinalIgnoreCase))
					? command.Connection.Database
					: null;
			}

			foreach (var dbName in new[] { Db.DatabaseName, Db.AuditDatabaseName, Db.EdwDatabaseName })
			{
				if (string.Equals(command.Connection.Database, dbName, StringComparison.OrdinalIgnoreCase)
					|| command.CommandText.IndexOf(dbName, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return dbName;
				}
			}

			return null;
		}

		internal static void OnRollback(DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot 

			lock (lockObj)
			{
				updatesInTransaction.Remove(connection.ObjectID);
			}
		}

		internal static void OnCommit(DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			if (TestingState.IsRunningTests)
			{
				lock (lockObj)
				{
					var commandsInTransaction = updatesInTransaction.GetCommands(connection.ObjectID);
					updates.AddRange(commandsInTransaction);
					updatesInTransaction.Remove(connection.ObjectID);
				}
			}
		}

		public static void Reset()
		{
			lock (lockObj)
			{
				updates.Reset();
				updatesInTransaction.Reset();
			}
		}

		public static void Reset(string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));
			lock (lockObj)
			{
				updates.Remove(dbName);
				updatesInTransaction.Remove(dbName);
			}
		}

		public static void Ignore(string dbOrTableName)
		{
			Argument.NotNullOrEmpty(dbOrTableName, nameof(dbOrTableName));

			lock (lockObj)
			{
				updates.Ignore(dbOrTableName);
			}
		}

		public static IEnumerable<string> CommittedUpdates
		{
			get { return updates.CommittedUpdates; }
		}

		static string BuildCommandStringWithParameters(DbCommand command)
		{
			var sb = new StringBuilder();
			sb.Append(command.CommandText);

			var parameterStrings = SqlEventTracker.GetSqlParameterStrings(command.InternalCommand);

			if (parameterStrings.Any())
			{
				sb.Append($"{System.Environment.NewLine}{System.Environment.NewLine}Parameters:");
				foreach (var parameterString in parameterStrings)
				{
					sb.Append($"{System.Environment.NewLine}   {parameterString}");
				}
			}

			return sb.ToString();
		}

		static readonly object lockObj = new();
		static readonly CommandList updates = new();
		static readonly CommandList updatesInTransaction = new();
		static readonly Regex updateRegex = new(@"\b(?<!GRANT\s+(?:[\w,\s]+\s+)?)(?<!TO\s+\[)(insert(?!(\s+into)?\s+@\w+)(?!\s+\w+\s+constraint)|update(?!\s+\w+\s+constraint)|delete(?!\s+cascade))\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		class CommandList
		{
			public IEnumerable<Command> GetCommands(int objectId)
			{
				return commands.Where(c => c.ObjectId == objectId);
			}

			public void Add(int objectId, string databaseName, string commandText)
			{
				commands.Add(new Command(objectId, databaseName, commandText));
			}

			public void AddRange(IEnumerable<Command> commandList)
			{
				commands.AddRange(commandList);
			}

			public void Reset()
			{
				commands.Clear();
			}

			public void Remove(string dbName)
			{
				for (var i = commands.Count - 1; i >= 0; i--)
				{
					var commit = commands[i];
					if (commit.DatabaseName.Equals(dbName, StringComparison.OrdinalIgnoreCase))
					{
						commands.RemoveAt(i);
					}
				}
			}

			public void Remove(int objectId)
			{
				for (var i = commands.Count - 1; i >= 0; i--)
				{
					var commit = commands[i];
					if (commit.ObjectId == objectId)
					{
						commands.RemoveAt(i);
					}
				}
			}

			public void Ignore(string dbOrTableName)
			{
				for (var i = commands.Count - 1; i >= 0; i--)
				{
					var commit = commands[i];
					if (commit.CommandText.Contains(dbOrTableName))
					{
						commands.RemoveAt(i);
					}
				}
			}

			public IEnumerable<string> CommittedUpdates
			{
				get { return commands.Select(c => $"Database: {c.DatabaseName}\r\n\r\n{c.CommandText}"); }
			}

			readonly List<Command> commands = new List<Command>();

			internal class Command
			{
				public Command(int objectId, string dbName, string commandText)
				{
					ObjectId = objectId;
					DatabaseName = dbName;
					CommandText = commandText;
				}
				public readonly int ObjectId;
				public readonly string DatabaseName;
				public readonly string CommandText;
			}
		}
	}
}
#endif
