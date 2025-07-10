using System;
using CargoWise.Common;
using CargoWise.Loader.Common;

namespace Enterprise.Loader
{
	public class DatabaseConnectionInitializer : InstallationItem
	{
		public DatabaseConnectionInitializer(Installation installation) : base(installation)
		{
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			if (DatabaseHelperInstance != null)
			{
				return InstallationResult.OK();
			}

			InstallationResult result = InstallationResult.Error("Could not connect to database.");
			ChangeCurrentTaskDescription("Connecting to database");

			try
			{
				var helper = CreateNewEnterpriseDatabaseHelper();

				if (helper.OpenConnection())
				{
					DatabaseHelperInstance = helper;
					result = InstallationResult.OK();
				}
				else if (helper.ConnectionException != null)
				{
					var firstException = helper.ConnectionException;

					if (helper.OpenConnection(true))
					{
						DatabaseHelperInstance = helper;
						result = InstallationResult.OK();
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result = InstallationResult.Error("Could not connect to database. " + Environment.NewLine + Environment.NewLine + ex.ToString());
			}

			return result;
		}

		protected virtual EnterpriseDatabaseHelper CreateNewEnterpriseDatabaseHelper()
		{
			return new EnterpriseDatabaseHelper((EnterpriseConfiguration)Installation.Configuration);
		}

		protected override bool NeedsToInstallCore()
		{
			return DatabaseHelperInstance == null;
		}

		[ThreadStatic]
		static EnterpriseDatabaseHelper databaseHelperInstance;

		public static EnterpriseDatabaseHelper DatabaseHelperInstance
		{
			get { return databaseHelperInstance; }
			set { databaseHelperInstance = value; }
		}
	}
}

