using System.Data;
using CargoWise.Data;
using Enterprise.Upgrades;

namespace Enterprise.DbUpgrader.Startup.BeforeAndAfter.DotNetPrerequisiteCheck
{
	public interface IDotNetPrerequisiteDataProvider
	{
		string Prefix { get; }
		DataTable GetVersionsRecords(DbConnection connection);
		void RemoveOldVersionsRecords();
	}

	public abstract class BasePrerequisiteDataProvider : IDotNetPrerequisiteDataProvider
	{
		public abstract string Prefix
		{
			get;
		}

		public DataTable GetVersionsRecords(DbConnection connection)
		{
			var sqlText = $"SELECT SD_Name, CAST(SD_BinaryValue AS varchar(MAX)) AS value FROM dbo.StmData WHERE SD_Name LIKE '{SearchString()}'";

			var dbRuntimesRecords = DataUtils.GetDataTableFromQuery(connection, sqlText);
			return dbRuntimesRecords;
		}

		public void RemoveOldVersionsRecords()
		{
			Db.Connection.ExecuteNonQuery($"DELETE FROM dbo.StmData WHERE SD_Name LIKE '{SearchString()}' AND RIGHT(CAST(SD_BinaryValue AS varchar(max)), 19) < CAST(GETUTCDATE()-15 AS date)");
		}

		string SearchString()
		{
			var searchString = $"{Prefix.Replace("_", "[_]")}|%";
			return searchString;
		}
	}

	public class DotNetCorePrerequisiteDataProvider : BasePrerequisiteDataProvider
	{
		public override string Prefix
		{
			get { return PcWithDotNetCoreRuntimesRecord.RegistryPrefix; }
		}
	}

	public class DotNetPrerequisiteDataProvider : BasePrerequisiteDataProvider
	{
		public override string Prefix
		{
			get { return PcWithDotNetVersionRecord.RegistryPrefix; }
		}
	}
}
