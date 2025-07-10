using System;
using CargoWise.Common;
using Enterprise.Accounting.Business;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport
{
	public class JCDTableAndPartitionObject
	{
		public JCDTableAndPartitionObject(JCDDBObjectInfo dbObject)
		{
			Argument.NotNull(dbObject, "JCDDBObjectInfo");
			this.dbObjectInfo = dbObject;
		}
		readonly JCDDBObjectInfo dbObjectInfo;

		public string Name => dbObjectInfo.Name;

		public string CreateSQLText { get; set; }

		public string CheckAndDropSQLText { get; set; }

		public bool IsTemporary => dbObjectInfo.IsTemporary;

		public bool IsTable => (dbObjectInfo.Type == JCDDBObjectInfo.TABLE);

		public int Sequence { get; set; }

		public static JCDTableAndPartitionObject[] GetTableAndPartitionInfo(int startingPeriod = 0)
		{
			var dbObjectInfoList = JCDDBObjectInfoList.Instance;

			return new[]
			{
				GetRptDtUnprocessedAccTransactionLines(dbObjectInfoList),
				GetPartition(dbObjectInfoList, startingPeriod),
				GetRptDtUnprocessedReversedAL(dbObjectInfoList),
				GetRptDtJobCostingData(dbObjectInfoList)
			};
		}

		static JCDTableAndPartitionObject GetRptDtUnprocessedAccTransactionLines(JCDDBObjectInfoList dbObjectInfoList)
		{
			return new JCDTableAndPartitionObject(dbObjectInfoList["RptDtUnprocessedAccTransactionLines"])
			{
				CreateSQLText = @"CREATE TABLE RptDtUnprocessedAccTransactionLines (UL_RowNumber bigint IDENTITY(1,1) NOT NULL, UL_ALPK uniqueidentifier NOT NULL);
								  ALTER TABLE RptDtUnprocessedAccTransactionLines SET (LOCK_ESCALATION = DISABLE);
								  CREATE CLUSTERED INDEX [NR_RC__Clustered_UL_RowNumber] ON [dbo].[RptDtUnprocessedAccTransactionLines] ([UL_RowNumber] ASC);",

				CheckAndDropSQLText = @"IF OBJECT_ID(N'RptDtUnprocessedAccTransactionLines') IS NOT NULL
										BEGIN
											DROP TABLE RptDtUnprocessedAccTransactionLines
										END",
				Sequence = 1
			};
		}

		static JCDTableAndPartitionObject GetPartition(JCDDBObjectInfoList dbObjectInfoList, int startingPeriod)
		{
			return new JCDTableAndPartitionObject(dbObjectInfoList["PS_AccountingPeriodCompany"])
			{
				CreateSQLText = FormattableString.Invariant($@"	DECLARE @sqlcmd nvarchar(max), @ids varchar(max);
									SELECT @ids = coalesce(@ids + ', ', '') +  a.AM_Key  FROM (SELECT '''' + convert(char(6), AM_Period) + convert(char(36), AM_GC_Company) + '''' as AM_Key from dbo.AccPeriodManagement where AM_Period >= {startingPeriod}) a
									SET @ids = '''99999900000000-0000-0000-0000-000000000000''' +  IIF(@ids IS NULL, '', ', ' + @ids) ;
									SET @sqlcmd = N'CREATE PARTITION FUNCTION PF_AccountingPeriodCompany(char(42)) AS RANGE LEFT FOR VALUES (' + @ids + N')' ;
									EXEC SP_EXECUTESQL @sqlcmd;

									CREATE PARTITION SCHEME PS_AccountingPeriodCompany AS PARTITION PF_AccountingPeriodCompany ALL TO ([REPORTGROUP]);"),

				CheckAndDropSQLText = @"IF EXISTS(SELECT * FROM sys.partition_schemes ps where NAME = 'PS_AccountingPeriodCompany')
										BEGIN
											DROP PARTITION SCHEME PS_AccountingPeriodCompany
										END

										IF EXISTS(SELECT null FROM sys.partition_functions WHERE  NAME = 'PF_AccountingPeriodCompany')
										BEGIN
											DROP PARTITION FUNCTION PF_AccountingPeriodCompany
										END",

				Sequence = 2
			};
		}

		static JCDTableAndPartitionObject GetRptDtUnprocessedReversedAL(JCDDBObjectInfoList dbObjectInfoList)
		{
			return new JCDTableAndPartitionObject(dbObjectInfoList["RptDtUnprocessedReversedAL"])
			{
				CreateSQLText = @"CREATE TABLE RptDtUnprocessedReversedAL(URL_ALPK uniqueidentifier NOT NULL);
								ALTER TABLE RptDtUnprocessedReversedAL SET (LOCK_ESCALATION = DISABLE);
								CREATE UNIQUE CLUSTERED INDEX FK_UC__Clustered_URL_ALPK ON RptDtUnprocessedReversedAL(URL_ALPK ASC);",

				CheckAndDropSQLText = @"IF OBJECT_ID(N'RptDtUnprocessedReversedAL') IS NOT NULL
										BEGIN
											DROP TABLE RptDtUnprocessedReversedAL
										END",

				Sequence = 3
			};
		}

		static JCDTableAndPartitionObject GetRptDtJobCostingData(JCDDBObjectInfoList dbObjectInfoList)
		{
			return new JCDTableAndPartitionObject(dbObjectInfoList[MainTableName])
			{
				CreateSQLText = "EXEC CreateJobCostingDataTableAndCCI @isStagingTable = 0",

				CheckAndDropSQLText = FormattableString.Invariant($@"IF OBJECT_ID(N'{MainTableName}', N'U') IS NOT NULL
										BEGIN
											DROP TABLE {MainTableName}
										END;
										IF OBJECT_ID(N'{StagingTableName}', N'U') IS NOT NULL
										BEGIN
											DROP TABLE {StagingTableName}
										END"),

				Sequence = 4
			};
		}

		public const string MainTableName = "RptDtJobCostingData";
		public const string StagingTableName = "RptDtJobCostingDataStaging";
	}
}
