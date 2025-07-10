using System.Data;
using System.Linq;
using System.Numerics;
using CargoWise.Common;
using CargoWise.Data;
using static System.FormattableString;

namespace Enterprise.DataTools.DbBackupAndRestore.Business.Common
{
	public class DbHeaderOnlyReader : IDbHeaderOnlyReader
	{
		public IDbBackupFileInfo ReadHeaderOnly(DbConnection connection, string filePath)
		{
			Argument.NotNullOrEmpty(filePath, nameof(filePath));

			var sql = Invariant($@"RESTORE HEADERONLY FROM DISK = N'{filePath}'");
			var backupInfoTable = ZArchitecture.Core.Utilities.GetDataTableFromQuery(connection, sql);
			if (backupInfoTable.Rows.Count <= 0)
			{
				return default;
			}

			var backupInfoHeader = backupInfoTable.Rows.Cast<DataRow>().FirstOrDefault();
			if (backupInfoHeader == null)
			{
				return default;
			}

			return new DbBackupFileInfo(
				(string)backupInfoHeader[DbBackupFileInfo.Columns.DatabaseName],
				BigInteger.Parse(backupInfoHeader[DbBackupFileInfo.Columns.LastLSN].ToString()));
		}

		class DbBackupFileInfo : IDbBackupFileInfo
		{
			public DbBackupFileInfo(string dbName, BigInteger lastLSN)
			{
				DatabaseName = dbName;
				LastLSN = lastLSN;
			}

			public string DatabaseName { get; }
			public BigInteger LastLSN { get; }

			public static class Columns
			{
				public const string DatabaseName = nameof(DatabaseName);
				public const string LastLSN = nameof(LastLSN);
			}
		}
	}
}
