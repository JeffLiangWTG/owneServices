using System.Collections.Generic;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Data.SqlClr.Registration;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Assemblies
{
	public abstract class BiDatabaseAssembliesUpgrader : DatabaseAssembliesUpgrader
	{
		protected BiDatabaseAssembliesUpgrader(IUpgradeManager manager, DbConnection upgConnection, DbConnection biConnection, VersionLabel versionBeforeUpgrade, IEnumerable<SqlAssemblyClrObjectInfo> registeredClrObjects)
			: base(manager, upgConnection, versionBeforeUpgrade, registeredClrObjects)
		{
			this.biConnection = biConnection;
		}
		readonly DbConnection biConnection;

		public override bool IsUpgradeRequired
		{
			get
			{
				return (BiDatabaseSchemaUpgradeRequired || base.IsUpgradeRequired) && BiRequirementChecker.Instance.IsBiDatabaseUpgradeRequired(upgConnection, biConnection);
			}
		}

		public bool BiDatabaseSchemaUpgradeRequired
		{
			get
			{
				var result = true;

				if (biConnection != null)
				{
					if (biConnection.DatabaseExists(DatabaseName))
					{
						var mainDbSchemaVersion = SchemaVersion.Application.ToString();
						var biDbSchemaVersion = DataUtils.LoadDbExtendedProperty(biConnection, BiConstants.MainDbSchemaVersionExtPtyName, DatabaseName);

						result = (mainDbSchemaVersion != biDbSchemaVersion);
					}
				}
				else
				{
					result = false;
				}

				return result;
			}
		}

		protected override void DoUpgrade()
		{
			using (((ICurrentDbControl)biConnection).UseDatabase(DatabaseName))
			{
				UpgradeSpecificDatabase(DbAssemblies, biConnection);
			}
		}

		private protected override SqlAssemblies LoadDbAssemblies()
		{
			if (biConnection != null && biConnection.DatabaseExists(DatabaseName))
			{
				using (((ICurrentDbControl)biConnection).UseDatabase(DatabaseName))
				{
					return LoadModel(biConnection, RegisteredClrObjects);
				}
			}
			else
			{
				return new SqlAssemblies();
			}
		}
	}
}
