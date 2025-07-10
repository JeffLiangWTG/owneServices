using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.SqlClr.Registration;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Assemblies
{
	public abstract class DatabaseAssembliesUpgrader : BaseUpgrader
	{
		protected DatabaseAssembliesUpgrader(IUpgradeManager manager, DbConnection upgConnection, VersionLabel versionBeforeUpgrade, IEnumerable<SqlAssemblyClrObjectInfo> registeredClrObjects)
			: base(manager, upgConnection, versionBeforeUpgrade)
		{
			RegisteredClrObjects = new List<SqlAssemblyClrObjectInfo>(registeredClrObjects);
			dbAssemblies = new Lazy<SqlAssemblies>(LoadDbAssemblies);
		}

		public List<SqlAssemblyClrObjectInfo> RegisteredClrObjects { get; }

		internal SqlAssemblies DbAssemblies => dbAssemblies.Value;

		public bool HasStoredAssemblies => SqlAssemblies.HasStoredAssemblies(upgConnection.GetIDbConnection(), upgConnection.GetIDbTransaction());

		public override bool IsUpgradeRequired => base.IsUpgradeRequired &&
			(versionBeforeUpgrade.CompareTo(LatestVersion) != 0 || DbAssemblies.Values.Any(assembly => !assembly.ExistsInDatabase || assembly.Any(proc => !proc.Value.ExistsInDatabase)));

		public override int EstimatedNumberOfTasks => 6;

		protected override VersionLabel LatestVersion => SqlClrAssembliesVersion.Application;

		protected virtual string DatabaseName => upgConnection.CurrentDatabase;

		public override string Name => FormattableString.Invariant($"SQL Assemblies Upgrade (database: {DatabaseName})");

		public override IEnumerable<string> SecondaryDatabasesToUpgrade => Array.Empty<string>();

		public override bool RequiresApplicationLockout => false;

		protected override void DoUpgrade()
		{
			UpgradeSpecificDatabase(DbAssemblies, upgConnection);
		}

		public const string lockRequestTimeOutString = "Database Assemblies Upgrade has timed out. Please try again.";
		private protected virtual SqlAssemblies LoadDbAssemblies()
		{
			var retries = 0;
			while (true)
			{
				try
				{
					return LoadModel(upgConnection, RegisteredClrObjects);
				}
				catch (SqlException ex)
				{
					++retries;
					if (retries >= 3)
					{
						foreach (SqlError error in ex.Errors)
						{
							if (error.Number == 1222)
							{
								ShowTaskError(lockRequestTimeOutString);
								break;
							}
						}
						throw;
					}
				}
			}
		}

		private protected void UpgradeSpecificDatabase(SqlAssemblies assemblies, DbConnection connection)
		{
			if (versionBeforeUpgrade.CompareTo(LatestVersion) != 0)
			{
				DropClrObjects(assemblies, connection, true);
				ModifyAssemblies(assemblies, connection, true);
				CreateClrObjects(assemblies, connection, true);
			}
			else if (assemblies.Values.Any(assembly => !assembly.ExistsInDatabase || assembly.Any(proc => !proc.Value.ExistsInDatabase)))
			{
				DropClrObjects(assemblies, connection, true);
				ModifyAssemblies(assemblies, connection, true);
				CreateClrObjects(assemblies, connection, true);
			}
			else if (assemblies.Values.Any(assembly => assembly.IsFileChanged))
			{
				DropClrObjects(assemblies, connection, false);
				ModifyAssemblies(assemblies, connection, false);
				CreateClrObjects(assemblies, connection, false);
			}

			ShowInfoMessage(".");
		}

		readonly Lazy<SqlAssemblies> dbAssemblies;

		#region CLR Deployment

		private protected virtual SqlAssemblies LoadModel(DbConnection connection, List<SqlAssemblyClrObjectInfo> clrObjects)
		{
			var assemblies = new SqlAssemblies();
			assemblies.LoadStoredAssemblyInfo(connection.GetIDbConnection(), connection.GetIDbTransaction());
			foreach (var registeredclrObject in clrObjects)
			{
				var assembly = assemblies.Register(registeredclrObject.AssemblyName, registeredclrObject.PermissionSet);
				assembly.Register(registeredclrObject);
			}

			return assemblies;
		}

		void DropClrObjects(SqlAssemblies assemblies, DbConnection connection, bool versionChanged)
		{
			StartTask("Dropping out of date assembly CLR Objects");

			foreach (var sqlAssembly in assemblies.Values.Where(assembly => assembly.ExistsInDatabase))
			{
				foreach (var sqlClrObject in sqlAssembly.Values.Where(o => o.ExistsInDatabase))
				{
					var requiredAction = versionChanged ? DatabaseAction.Drop : DatabaseAction.Alter;

					// drop dependent schemabound scripts
					var sql = UpgraderUtils.GetSchemaBoundObjectsToDropSql(Db.SqlDbOwnerSchema, sqlClrObject.StoredClrObjectSignature.Name);
					new BatchRunner().RunCommandsGeneratedByQuery(connection, sql);

					StartSubtask(ChangeIndicator(requiredAction) + " CLR Object " + sqlClrObject.StoredClrObjectSignature.Name);
					sqlClrObject.DropClrObjectInDatabase(connection.GetIDbConnection(), connection.GetIDbTransaction());
				}
			}
		}

		void CreateClrObjects(SqlAssemblies assemblies, DbConnection connection, bool versionChanged)
		{
			StartTask("Creating new assembly CLR Objects");
			foreach (var sqlAssembly in assemblies.Values.Where(assembly => assembly.IsRegistered))
			{
				foreach (var sqlClrObject in sqlAssembly.Values.Where(o => o.IsRegistered))
				{
					var requiredAction = versionChanged ? DatabaseAction.Create : DatabaseAction.Alter;

					StartSubtask(ChangeIndicator(requiredAction) + " CLR Object " + sqlClrObject.RegisteredClrObjectSignature.Name);
					sqlClrObject.CreateClrObjectInDatabase(connection.GetIDbConnection(), connection.GetIDbTransaction());
				}
			}
		}

		void ModifyAssemblies(SqlAssemblies assemblies, DbConnection connection, bool versionChanged)
		{
			StartTask("Modifying assemblies");
			if (versionChanged)
			{
				foreach (var sqlAssembly in assemblies.Values.Where(assembly => assembly.ExistsInDatabase))
				{
					StartSubtask(ChangeIndicator(DatabaseAction.Drop) + " Assembly " + sqlAssembly.Name);
					sqlAssembly.DropAssemblyInDatabase(connection.GetIDbConnection(), connection.GetIDbTransaction());
				}

				foreach (var sqlAssembly in assemblies.Values.Where(assembly => assembly.IsRegistered))
				{
					StartSubtask(ChangeIndicator(DatabaseAction.Create) + " Assembly " + sqlAssembly.Name);
					sqlAssembly.CreateAssemblyInDatabase(connection.GetIDbConnection(), connection.GetIDbTransaction(), sqlAssembly.GuessPermissionSet());
				}
			}
			else
			{
				foreach (var sqlAssembly in assemblies.Values.Where(assembly => assembly.IsFileChanged))
				{
					StartSubtask(ChangeIndicator(DatabaseAction.Alter) + " Assembly " + sqlAssembly.Name);
					sqlAssembly.AlterAssemblyInDatabase(connection.GetIDbConnection(), connection.GetIDbTransaction(), sqlAssembly.GuessPermissionSet());
				}
			}
		}

		internal static string ChangeIndicator(DatabaseAction action)
		{
			switch (action)
			{
				case DatabaseAction.Create:
					return "+";
				case DatabaseAction.Alter:
					return "~";
				case DatabaseAction.Drop:
					return "-";
				default:
					return "";
			}
		}

		#endregion
	}
}
