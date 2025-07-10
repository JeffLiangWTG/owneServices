using System.IO;
using CargoWise.BuildTools;
using CargoWise.Data;
using NUnit.Framework.Dat;

namespace Enterprise.Builder.Generator
{
	class DatabaseRestorer : Restorer
	{
		public string DevelopmentDbBackupPath
		{
			get
			{
				var developmentDbBackupPathOverride = System.Environment.GetEnvironmentVariable("WTG_DevelopmentDbBackupPath");
				if (!string.IsNullOrWhiteSpace(developmentDbBackupPathOverride))
				{
					return developmentDbBackupPathOverride;
				}

				return Path.Combine(DatServerConnection.DatFileSharePath, "DatabaseBackups", CurrentDatDbBackupPrefix.GetValue() + ".bak");
			}
		}

		public override void Restore()
		{
			CreateAndRestore(Db.ServerName, Db.DatabaseName, DevelopmentDbBackupPath);
		}
	}
}
