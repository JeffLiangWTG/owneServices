using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using static System.FormattableString;
using static Enterprise.Core.Constants;

namespace Enterprise.DbHealth.Check
{
	class RestoreChecker : IChecker
	{
		public string Description => "Check whether the database has been restored recently";

		public void Check(DbConnection connection, DbHealthWarningList warningList, ILogger logger)
		{
			if (!EnvProxy.IsHostedWithCargowise && IsExistPortugalCompany(connection))
			{
				var dbName = connection.CurrentDatabase;
				var dateFrom = ZDateTime.UtcNow.AddMonths(-1);

				var lastDatabaseRestoreDate = ObjectFactory.Get<ISystemDataRegistry>().LastDatabaseRestoreDate;
				if (lastDatabaseRestoreDate != DateTime.MinValue && lastDatabaseRestoreDate > dateFrom)
				{
					var warning = Invariant($"Database [{dbName}] has been restored on {lastDatabaseRestoreDate}.");
					warningList.Add(GetRestoreCheckerStandardWarning(dbName, warning));
				}

				var lastUTCDate = ObjectFactory.Get<IAccountingRegistryProvider>().LastUTCDateToDisableComplianceBookAfterDbRestored;
				if (lastUTCDate != DateTime.MinValue && lastUTCDate > dateFrom)
				{
					var accountingWarning = Invariant($"Due to a Database Restore, all compliance sequences in all Portugal Login Companies have been disabled on {lastUTCDate}. This is to cope with Portugal local compliance laws. Please create new compliance books for Portugal companies if necessary.");
					warningList.Add(GetRestoreCheckerStandardWarning(dbName, accountingWarning));
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		bool IsExistPortugalCompany(DbConnection connection)
		{
			var sql = $@"
SELECT ISNULL((
    SELECT TOP 1 1 FROM dbo.GlbCompany 
    WHERE GC_IsActive = 1 AND GC_RN_NKCountryCode = '{CountryCodes.Portugal}'
), 0)";
			using (var cmd = connection.Command(sql))
			{
				return (int)cmd.ExecuteScalar() > 0;
			}
		}

		DbHealthWarning GetRestoreCheckerStandardWarning(string dbName, string description)
		{
			var action = $"{ProductName} will disable compliance Books in all Portugal Login Companies";
			DatabaseWarning warning = new DatabaseWarning(dbName, DatabaseWarning.RestoreWarning, description, action);
			return warning;
		}
	}
}
