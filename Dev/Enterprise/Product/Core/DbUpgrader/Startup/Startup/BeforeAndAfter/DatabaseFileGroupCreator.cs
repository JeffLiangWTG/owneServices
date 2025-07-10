using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Startup
{
	class DatabaseFileGroupCreator
	{
		public DatabaseFileGroupCreator(AdminConnection connection, string mainDbName)
		{
			this.connection = connection;
			this.mainDbName = mainDbName;
		}

		readonly AdminConnection connection;
		readonly string mainDbName;

		public void Create(string groupName, string fileSuffix, int numberOfFiles = 1, int? initialSizeInGb = null, int? fileGrowthSize = null, string fileGrowthSizeUnit = "")
		{
			var filePathSqlVariableName = "@NewFilePath";

			var sqlText = String.Format(CultureInfo.InvariantCulture, @"
BEGIN TRY
	DECLARE @GroupDataSpaceId int = (SELECT data_space_id FROM [{0}].sys.filegroups WHERE name = '{1}');

	IF (@GroupDataSpaceId is null)
	BEGIN
		ALTER DATABASE [{0}] ADD FILEGROUP [{1}];
		SET @GroupDataSpaceId = (SELECT data_space_id FROM [{0}].sys.filegroups WHERE name = '{1}');
	END

	IF not exists(SELECT null FROM [{0}].sys.database_files WHERE data_space_id = @GroupDataSpaceId)
	BEGIN
		DECLARE @PrimaryDataSpaceId int = (SELECT TOP (1) data_space_id FROM [{0}].sys.filegroups WHERE is_default = 1);
		DECLARE @PrimaryFilePath nvarchar(1000) = (SELECT TOP (1) physical_name FROM [{0}].sys.database_files WHERE data_space_id = @PrimaryDataSpaceId);
		DECLARE {2} nvarchar(1000) =
			REVERSE(
				SUBSTRING(
					REVERSE(@PrimaryFilePath),
					CHARINDEX('\', REVERSE(@PrimaryFilePath)),
					1000
				)
			);

		{3}
	END
END TRY  
BEGIN CATCH  
	THROW
END CATCH ",
				mainDbName,
				groupName,
				filePathSqlVariableName,
				GetFileNamesSQLText(groupName, fileSuffix, numberOfFiles, initialSizeInGb, fileGrowthSize, fileGrowthSizeUnit, filePathSqlVariableName)
			);

			using (var cmd = connection.Command(sqlText))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public void ChangeFileGrowthSize(string fileGroupName, int newFileGrowthSize, string newfileGrowthSizeUnit)
		{
			var fileInfo = GetFileGroupInfo(fileGroupName);

			foreach (var file in fileInfo)
			{
				if (!file.MatchFileGrowthSizeInfo(newFileGrowthSize, newfileGrowthSizeUnit))
				{
					using (var cmd = connection.Command(string.Format(CultureInfo.InvariantCulture, "ALTER DATABASE {0} MODIFY FILE ( NAME = N'{1}', FILEGROWTH = {2}{3} )", mainDbName, file.LogicalFileName, newFileGrowthSize, newfileGrowthSizeUnit)))
					{
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		public IEnumerable<FileInfo> GetFileGroupInfo(string fileGroupName)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"SELECT	DB_name(S.database_id) AS DatabaseName
																			,CAST(CAST(G.name AS VARBINARY(256)) AS sysname) AS FileGroupName
																			,S.[name] AS LogicalName
																			,S.physical_name AS FileName
																			,s.is_percent_growth
																			,CASE s.is_percent_growth WHEN 1 THEN S.[growth] ELSE (cast(S.[growth] as decimal(18, 6)) * 8)/(1024*1024) END AS Growth
																			,CASE s.is_percent_growth WHEN 1 THEN '%' ELSE 'GB' END AS GrowthUnit
																	FROM	sys.master_files AS S
																			LEFT JOIN sys.filegroups AS G ON ((S.type = 2 OR S.type = 0) AND (S.drop_lsn IS NULL)) AND (S.data_space_id=G.data_space_id)
																	WHERE	DB_name(S.database_id) = '{0}'
																			{1}",
																			connection.CurrentDatabase,
																			!string.IsNullOrEmpty(fileGroupName) ? string.Format(CultureInfo.InvariantCulture, " AND CAST(CAST(G.name AS VARBINARY(256)) AS sysname) = '{0}'", fileGroupName) : string.Empty);

			var dt = DataUtils.GetDataTableFromQuery(connection, sql);

			var files = dt.Select().Select(r => new FileInfo
			{
				DBName = Convert.ToString(r["DatabaseName"], CultureInfo.InvariantCulture),
				FileGroupName = Convert.ToString(r["FileGroupName"], CultureInfo.InvariantCulture),
				LogicalFileName = Convert.ToString(r["LogicalName"], CultureInfo.InvariantCulture),
				FileName = Convert.ToString(r["FileName"], CultureInfo.InvariantCulture),
				IsPercentGrowth = (Convert.ToInt32(r["is_percent_growth"], CultureInfo.InvariantCulture) == 1),
				Growth = Convert.ToDecimal(r["Growth"], CultureInfo.InvariantCulture),
				GrowthUnit = Convert.ToString(r["GrowthUnit"], CultureInfo.InvariantCulture),
			});

			return files;
		}

		string GetFileNamesSQLText(string groupName, string fileSuffix, int numberOfFiles, int? initialSizeInGb, decimal? fileGrowthSize, string fileGrowthSizeUnit, string filePathSqlVariableName)
		{
			var sqlTextBuilder = new StringBuilder();
			for (int i = 0; i < numberOfFiles; i++)
			{
				var numericSuffix = i > 0 ? i.ToString(CultureInfo.InvariantCulture) : string.Empty;
				var sizeClause = initialSizeInGb.HasValue ? ", SIZE = " + initialSizeInGb.Value.ToString(CultureInfo.InvariantCulture) + "GB" : "";
				var growthClause = fileGrowthSize.HasValue ? ", FILEGROWTH = " + fileGrowthSize.Value.ToString(CultureInfo.InvariantCulture) + fileGrowthSizeUnit : "";

				sqlTextBuilder.AppendLine($@"EXEC (N'
						ALTER DATABASE [{mainDbName}] ADD FILE (
							NAME = [{mainDbName}_{fileSuffix}{numericSuffix}],
							FILENAME = ''' + {filePathSqlVariableName} + '{mainDbName}_{fileSuffix}{numericSuffix}.ndf''
							{sizeClause}
							{growthClause}
						) TO FILEGROUP [{groupName}];');");
			}

			return sqlTextBuilder.ToString();
		}

		public class FileInfo
		{
			public FileInfo()
			{
			}

			public string DBName { get; set; }
			public string FileGroupName { get; set; }
			public string LogicalFileName { get; set; }
			public string FileName { get; set; }
			public bool IsPercentGrowth { get; set; }
			public decimal Growth { get; set; }
			public string GrowthUnit { get; set; }

			public bool MatchFileGrowthSizeInfo(int fileGrowthSizeToMatch, string fileGrowthSizeUnitToMatch)
			{
				var result = false;

				result = (IsPercentGrowth && fileGrowthSizeUnitToMatch == "%" && fileGrowthSizeToMatch == Growth)
							|| (!IsPercentGrowth && fileGrowthSizeUnitToMatch == "MB" && fileGrowthSizeToMatch == GetGrowthSizeInMB())
							|| (!IsPercentGrowth && fileGrowthSizeUnitToMatch == "GB" && fileGrowthSizeToMatch == Growth);

				return result;
			}

			public bool IsFileGrowthSizeLessThanMinimum(int minFileGrowthSize, string fileGrowthSizeUnit)
			{
				var result = false;

				result = (IsPercentGrowth && fileGrowthSizeUnit == "%" && minFileGrowthSize > Growth)
							|| (!IsPercentGrowth && fileGrowthSizeUnit == "MB" && minFileGrowthSize > GetGrowthSizeInMB())
							|| (!IsPercentGrowth && fileGrowthSizeUnit == "GB" && minFileGrowthSize > Growth);

				return result;
			}

			decimal GetGrowthSizeInMB()
			{
				return !IsPercentGrowth ? Growth * 1024 : 0;
			}

			public override string ToString()
			{
				return string.Format(CultureInfo.InvariantCulture, "Database:{0}-File Group:{1}-File Name:{2}-FileGrowth:{3}-Unit:{4}", DBName, FileGroupName, LogicalFileName, Growth.ToString("0.0000", CultureInfo.InvariantCulture), GrowthUnit);
			}
		}
	}
}
