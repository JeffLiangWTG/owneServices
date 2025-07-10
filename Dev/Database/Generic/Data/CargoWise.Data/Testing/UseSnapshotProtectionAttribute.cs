#if DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
#pragma warning disable CA1813 //Supressed the warning as another class inherits from this class
	public class UseSnapshotProtectionAttribute : TestSetupAttribute
#pragma warning restore CA1813
	{
		public UseSnapshotProtectionAttribute(bool skipTransaction = false, int lockTimeout = 300)
		{
			SkipTransaction = skipTransaction;
			LockTimeout = lockTimeout;
			AddDatabaseToList(databaseNameSuffix: string.Empty);
		}

		public bool SkipTransaction { get; }
		public int LockTimeout { get; }

		public UseSnapshotProtectionAttribute(RefDbTypeEnum refDbType, string refCountryCode, bool skipTransaction = false, int lockTimeout = 300)
		{
			SkipTransaction = skipTransaction;
			LockTimeout = lockTimeout;
			Argument.NotNullOrEmpty(refCountryCode, nameof(refCountryCode));
			var dbName = ((IPhysicalRefDbLocation)adminConnection).GetReferenceDatabaseName(refDbType, refCountryCode);
			databaseAndSnapshotNames.Add(dbName, "DATREF-" + dbName);
		}

		public UseSnapshotProtectionAttribute(string databaseNameSuffix, bool skipTransaction = false, int lockTimeout = 300)
		{
			SkipTransaction = skipTransaction;
			LockTimeout = lockTimeout;
			AddDatabaseToList(databaseNameSuffix);
		}

		public UseSnapshotProtectionAttribute(DatabaseType[] dbTypes, bool skipTransaction = false, int lockTimeout = 300, string refCountryCode = null)
		{
			SkipTransaction = skipTransaction;
			LockTimeout = lockTimeout;
			if (dbTypes.IsNullOrEmpty())
			{
				AddDatabaseToList(string.Empty);
			}
			else
			{
				foreach (var dbType in dbTypes)
				{
					switch (dbType)
					{
						case DatabaseType.Main:
							AddDatabaseToList(string.Empty);
							break;
						case DatabaseType.Audit:
							AddDatabaseToList(Db.AuditDatabaseSuffix);
							break;
						case DatabaseType.EDW:
							AddDatabaseToList(Db.EdwDatabaseSuffix);
							break;
						case DatabaseType.BI:
							AddDatabaseToList(Db.AuditDatabaseSuffix);
							AddDatabaseToList(Db.EdwDatabaseSuffix);
							break;
						case DatabaseType.ExclusiveRef:
							Argument.NotNullOrEmpty(refCountryCode, nameof(refCountryCode));
							var exclusiveRefDbNameSuffix = RefDbTableNameResolver.GetExclusiveRefDbNameSuffix(RefDbTypeEnum.Customs, refCountryCode);
							AddDatabaseToList(exclusiveRefDbNameSuffix);
							break;
						case DatabaseType.SingleSharedRef:
							var singleRefDatabaseName = RefDbTableNameResolver.SingleRefDatabaseName;
							databaseAndSnapshotNames.Add(singleRefDatabaseName, GetSnapshotName(singleRefDatabaseName));
							break;
						default:
							throw new ArgumentException($"{dbType} is not supported.");
					}
				}
			}
		}

		void AddDatabaseToList(string databaseNameSuffix)
		{
			var dbName = Db.DatabaseName + databaseNameSuffix;
			databaseAndSnapshotNames.Add(dbName, GetSnapshotName(dbName));
		}

		readonly Dictionary<string, string> databaseAndSnapshotNames = [];

		public override void SetUp(TestCase testCase)
		{
			adminConnection ??= Db.NewAdminConnection();
			adminConnectionReferenceCount++;

			foreach (var dbName in databaseAndSnapshotNames.Keys)
			{
				if (!tokens.ContainsKey(dbName))
				{
					var snapshotName = databaseAndSnapshotNames[dbName];
					tokens.Add(dbName, SnapshotCreator.CreateSnapshot(adminConnection, Db.Connection.CloseConnection, dbName, snapshotName, LockTimeout, mainDbName: Db.DatabaseName));
				}
			}
		}

		public static string GetSnapshotName(string dbName) => dbName + "-SS";

		public override void TearDown(TestCase testCase)
		{
			using (Db.DisableSchemaVersionCheck())
			{
				foreach (var entry in tokens)
				{
					try
					{
						entry.Value?.Dispose();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ErrorReporter.ReportOnce("Error caught on object dispose.", ex);
					}
				}
				tokens.Clear();
				adminConnectionReferenceCount--;
				if (adminConnectionReferenceCount == 0)
				{
					Release(ref adminConnection);
				}
			}
		}

		public static bool IsProtected
		{
			get { return tokens.ContainsKey(Db.DatabaseName); }
		}

		public static IEnumerable<string> SnapshotDatabases
		{
			get { return tokens.Select(t => t.Key); }
		}

		static void Release<T>(ref T disposable)
			where T : class, IDisposable
		{
			if (disposable != null)
			{
				try
				{
					disposable.Dispose();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce("Error caught on object dispose.", ex);
				}

				disposable = null;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static AdminConnection adminConnection;

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static int adminConnectionReferenceCount = 0;

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static readonly Dictionary<string, IDisposable> tokens = [];
	}
}

#endif
