using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Core;
using CargoWise.Data;
using Enterprise.Integration.Billing;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Core
{
	[TestedType(typeof(TableSizeCollectorSub))]
	sealed class TableSizeCollectorTest : RefStlScriptWithDefaultsTest
	{
		/**
		 * Creating this sub class because TableSizeCollector collects space usage for all the tables, which is over 1100 tables as of 2024-01.
		 * For each table, the collected data is saved into field "AdditionalRefs" as a JSON string.
		 *
		 * When the unit test is executed, the "AssertAdditionalRefs" function in class "StlScriptTest" will trigger jsonObj.IsValid(schema)
		 * to verify the JSON data. The validation utilizes a Newtonsoft Json function. It has a limit of 1000 calls
		 * per hour under free tier, which is not enough to validate all the Json data for 1100 tables, and the unit test will fail.
		 *
		 * Therefore, this sub class is created to restrict the data collection on only a few selected tables. If the limit on Newtonsoft is removed
		 * in the future (or we purchase a license), this sub class can be removed, and we can then test all the tables.
		 */
		class TableSizeCollectorSub : TableSizeCollector
		{
			public override string WhereClause => "[Schema] = 'dbo' AND [Table] in (" + string.Join(",", TestTableList.Select(x => $"'{x}'")) + ")";
		}

		class SpaceUsed
		{
			public string Table { get; set; }
			public int RowCount { get; set; }
			public decimal ReservedSpaceMB { get; set; }
			public decimal DataSpaceMB { get; set; }
			public decimal IndexSpaceMB { get; set; }
			public decimal UnusedSpaceMB { get; set; }
		}

		class CollectorAdditionalRefData
		{
			public string Database { get; set; }
			public string Schema { get; set; }
			public string Table { get; set; }
			public int RowCount { get; set; }
			public int TotalPages { get; set; }
			public decimal TotalMB { get; set; }
			public decimal TotalGB { get; set; }
			public int TotalDataPages { get; set; }
			public decimal TotalDataMB { get; set; }
			public decimal TotalDataGB { get; set; }
			public int TotalIndexPages { get; set; }
			public decimal TotalIndexMB { get; set; }
			public decimal TotalIndexGB { get; set; }
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2024, 1);
		protected override bool IsMandatoryForMilestones => false;
		static readonly List<string> TestTableList = new List<string> { "TbsTestTable1", "TbsTestTable2", "TbsTestTable3", "TbsTestTable4", "StmNumberSequence", "GlbReleaseNote" };

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals(6, transactions.Count());
			AssertEquals("Schema cdc's tables should be not be collected", 0, transactions.Count(x => x.Reference1 == "cdc"));
			Assert("Reference1 should be the database name", transactions.All(x => x.Reference1 == Db.Connection.CurrentDatabase));
			Assert("Reference2 should be the schema name", transactions.All(x => x.Reference2 == JsonConvert.DeserializeObject<CollectorAdditionalRefData>(x.AdditionalRefs).Schema));
			Assert("Reference3 should be the table name", transactions.All(x => x.Reference3 == JsonConvert.DeserializeObject<CollectorAdditionalRefData>(x.AdditionalRefs).Table));

			//check collector table size calculation
			AssertSpaceCalculation(transactions);

			//table size sanity check with sp_spaceused
			AssertTableSpaceUsed(transactions);

			//assert table size increase trend
			AssertTableSizeTrend(transactions);
		}

		void AssertSpaceCalculation(IEnumerable<IStlTransaction> transactions)
		{
			var table1 = transactions.Single(x => x.Reference2 == "dbo" && x.Reference3 == "GlbReleaseNote");
			var table2 = transactions.Single(x => x.Reference2 == "dbo" && x.Reference3 == "StmNumberSequence");
			
			var actualData1 = JsonConvert.DeserializeObject<CollectorAdditionalRefData>(table1.AdditionalRefs);
			var actualData2 = JsonConvert.DeserializeObject<CollectorAdditionalRefData>(table2.AdditionalRefs);

			AssertSpaceCalculation(actualData1);
			AssertSpaceCalculation(actualData2);
		}

		void AssertSpaceCalculation(CollectorAdditionalRefData data)
		{
			CombineAssertions("Calculation of data size in MB and GB should be proportional to total pages", () =>
			{
				AssertEquals(data.TotalMB, Math.Round(data.TotalPages * 8m / 1024, 3, MidpointRounding.AwayFromZero));
				AssertEquals(data.TotalGB, Math.Round(data.TotalPages * 8m / 1024 / 1024, 3, MidpointRounding.AwayFromZero));
				AssertEquals(data.TotalDataMB, Math.Round(data.TotalDataPages * 8m / 1024, 3, MidpointRounding.AwayFromZero));
				AssertEquals(data.TotalDataGB, Math.Round(data.TotalDataPages * 8m / 1024 / 1024, 3, MidpointRounding.AwayFromZero));
				AssertEquals(data.TotalIndexMB, Math.Round(data.TotalIndexPages * 8m / 1024, 3, MidpointRounding.AwayFromZero));
				AssertEquals(data.TotalIndexGB, Math.Round(data.TotalIndexPages * 8m / 1024 / 1024, 3, MidpointRounding.AwayFromZero));
			});
		}

		void AssertTableSpaceUsed(IEnumerable<IStlTransaction> transactions)
		{
			foreach (var table in TestTableList)
			{
				var transaction = transactions.Single(x => x.Reference2 == "dbo" && x.Reference3 == table);
				var actualData = JsonConvert.DeserializeObject<CollectorAdditionalRefData>(transaction.AdditionalRefs);
				var expectedData = GetTableSpace(table);
				CombineAssertions(() =>
				{
					AssertEquals("sp_spaceused row count", expectedData.RowCount, actualData.RowCount);
					AssertEquals("sp_spaceused reserved space", Math.Round(expectedData.ReservedSpaceMB, 3, MidpointRounding.AwayFromZero), actualData.TotalMB);
					AssertEquals("sp_spaceused data space", Math.Round(expectedData.DataSpaceMB, 3, MidpointRounding.AwayFromZero), actualData.TotalDataMB);
					AssertEquals("sp_spaceused index space", Math.Round(expectedData.IndexSpaceMB, 3, MidpointRounding.AwayFromZero), actualData.TotalIndexMB);
				});
			}
		}

		void AssertTableSizeTrend(IEnumerable<IStlTransaction> transactions)
		{
			var table1 = transactions.Single(x => x.Reference2 == "dbo" && x.Reference3 == "TbsTestTable1");
			var table2 = transactions.Single(x => x.Reference2 == "dbo" && x.Reference3 == "TbsTestTable2");
			var table3 = transactions.Single(x => x.Reference2 == "dbo" && x.Reference3 == "TbsTestTable3");
			var table4 = transactions.Single(x => x.Reference2 == "dbo" && x.Reference3 == "TbsTestTable4");

			var actualData1 = JsonConvert.DeserializeObject<CollectorAdditionalRefData>(table1.AdditionalRefs);
			var actualData2 = JsonConvert.DeserializeObject<CollectorAdditionalRefData>(table2.AdditionalRefs);
			var actualData3 = JsonConvert.DeserializeObject<CollectorAdditionalRefData>(table3.AdditionalRefs);
			var actualData4 = JsonConvert.DeserializeObject<CollectorAdditionalRefData>(table4.AdditionalRefs);

			//table1 doesn't have any record, all the values should be 0
			AssertEquals(0, actualData1.RowCount);
			AssertEquals(0, actualData1.TotalPages);
			AssertEquals(0, actualData1.TotalDataPages);
			AssertEquals(0, actualData1.TotalIndexPages);

			//asset table row count
			AssertEquals(0, actualData1.RowCount);
			AssertEquals(1, actualData2.RowCount);
			AssertEquals(16, actualData3.RowCount);
			AssertEquals(24, actualData4.RowCount);

			//table2 values should be larger than table1
			AssertTableSizeGreaterThan(actualData1, actualData2);

			//table3 values should be larger than table2
			AssertTableSizeGreaterThan(actualData2, actualData3);

			//table4 values should be larger than table3
			AssertTableSizeGreaterThan(actualData3, actualData4);
		}

		void AssertTableSizeGreaterThan(CollectorAdditionalRefData smaller, CollectorAdditionalRefData larger)
		{
			CombineAssertions(() =>
			{
				AssertGreaterThan(larger.RowCount, smaller.RowCount);
				AssertGreaterThan(larger.TotalPages, smaller.TotalPages);
				AssertGreaterThan(larger.TotalDataPages, smaller.TotalDataPages);
				AssertGreaterThan(larger.TotalIndexPages, smaller.TotalIndexPages);
			});
		}

		protected override void PrepareTestData()
		{
			var sql = @"IF OBJECT_ID(N'dbo.TbsTestTable1', N'U') IS NOT NULL DROP TABLE [dbo].[TbsTestTable1];
						IF OBJECT_ID(N'dbo.TbsTestTable2', N'U') IS NOT NULL DROP TABLE [dbo].[TbsTestTable2];
						IF OBJECT_ID(N'dbo.TbsTestTable3', N'U') IS NOT NULL DROP TABLE [dbo].[TbsTestTable3];
						IF OBJECT_ID(N'dbo.TbsTestTable4', N'U') IS NOT NULL DROP TABLE [dbo].[TbsTestTable4];

						Create Table [dbo].[TbsTestTable1] (
							[Id] [UNIQUEIDENTIFIER] NOT NULL,
							[TestIndex] [NVARCHAR] (800) NULL,
							[TestData] [NVARCHAR](2048) NULL,
							[SystemCreateTimeUtc] [SMALLDATETIME] NULL,
							[SystemCreateUser] [VARCHAR](3) NOT NULL,
						)
						ALTER TABLE [dbo].[TbsTestTable1] ADD CONSTRAINT [PK_UC__Id1] PRIMARY KEY CLUSTERED ([Id] ASC)
						CREATE NONCLUSTERED INDEX [NR_RX__TestIndex1] ON [dbo].[TbsTestTable1]
						(
							[TestIndex] ASC
						)

						Create Table [dbo].[TbsTestTable2] (
							[Id] [UNIQUEIDENTIFIER] NOT NULL,
							[TestIndex] [NVARCHAR] (800) NULL,
							[TestData] [NVARCHAR](2048) NULL,
							[SystemCreateTimeUtc] [SMALLDATETIME] NULL,
							[SystemCreateUser] [VARCHAR](3) NOT NULL,
						)
						ALTER TABLE [dbo].[TbsTestTable2] ADD CONSTRAINT [PK_UC__Id2] PRIMARY KEY CLUSTERED ([Id] ASC)
						CREATE NONCLUSTERED INDEX [NR_RX__TestIndex2] ON [dbo].[TbsTestTable2]
						(
							[TestIndex] ASC
						)

						Create Table [dbo].[TbsTestTable3] (
							[Id] [UNIQUEIDENTIFIER] NOT NULL,
							[TestIndex] [NVARCHAR] (800) NULL,
							[TestData] [NVARCHAR](2048) NULL,
							[SystemCreateTimeUtc] [SMALLDATETIME] NULL,
							[SystemCreateUser] [VARCHAR](3) NOT NULL,
						)
						ALTER TABLE [dbo].[TbsTestTable3] ADD CONSTRAINT [PK_UC__Id3] PRIMARY KEY CLUSTERED ([Id] ASC)
						CREATE NONCLUSTERED INDEX [NR_RX__TestIndex3] ON [dbo].[TbsTestTable3]
						(
							[TestIndex] ASC
						)

						Create Table [dbo].[TbsTestTable4] (
							[Id] [UNIQUEIDENTIFIER] NOT NULL,
							[TestIndex] [NVARCHAR] (800) NULL,
							[TestData] [NVARCHAR](2048) NULL,
							[SystemCreateTimeUtc] [SMALLDATETIME] NULL,
							[SystemCreateUser] [VARCHAR](3) NOT NULL,
						)
						ALTER TABLE [dbo].[TbsTestTable4] ADD CONSTRAINT [PK_UC__Id4] PRIMARY KEY CLUSTERED ([Id] ASC)
						CREATE NONCLUSTERED INDEX [NR_RX__TestIndex4] ON [dbo].[TbsTestTable4]
						(
							[TestIndex] ASC
						)

						declare @data varchar(700) = 'MIIHNjCCBh6gAwIBAgIQBc8Q4rHSdF/HMjFFHUP9lzANBgkqhkiG9w0BAQsFADBg
							MQswCQYDVQQGEwJVUzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3
							d3cuZGlnaWNlcnQuY29tMR8wHQYDVQQDExZSYXBpZFNTTCBUTFMgUlNBIENBIEcx
							MB4XDTIzMDMyNTAwMDAwMFoXDTI0MDQyMDIzNTk1OVowHzEdMBsGA1UEAwwUKi53
							aXNldGVjaGdsb2JhbC5jb20wggIiMA0GCSqGSIb3DQEBAQUAA4ICDwAwggIKAoIC
							AQDaYPPs209Wy67qMXGd6bFo/jO7VXL+6tR3O7ua/KnRqqGSR2LGS+Tm3QRMLPuk
							Yp6kGuMvljNHD6VeYdr/ARh2XsIEWXCWXEWZLJyxOn1L5s5ruYI5NBP8ucotX77/
							dF8tvpqV4DOs1vnpggw3D9M17L5FkM5VHYddKoF62kHw2Dm0OvFCm4pgOdA02uOD
							Rk3ylBAFDrx0B7CTyrq2U+3lDeKCxF8EonJUhXE0ejtT/Jz1kKR7HqtrTlb1Zsaa
							XyjBJN3e2DVuAx8SrAZaG7jpMSKrt+x6DmdzfzHB9Gc4A9fFxF3zBoY8iCnYggo7
							MQswCQYDVQQGEwJVUzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3
							d3cuZGlnaWNlcnQuY29tMR8wHQYDVQQDExZSYXBpZFNTTCBUTFMgUlNBIENBIEcx
							MB4XDTIzMDMyNTAwMDAwMFoXDTI0MDQyMDIzNTk1OVowHzEdMBsGA1UEAwwUKi53
							aXNldGVjaGdsb2JhbC5jb20wggIiMA0GCSqGSIb3DQEBAQUAA4ICDwAwggIKAoIC
							AQDaYPPs209Wy67qMXGd6bFo/jO7VXL+6tR3O7ua/KnRqqGSR2LGS+Tm3QRMLPuk';

						INSERT INTO [dbo].[TbsTestTable2] VALUES ('00000001-0000-0000-0000-000000000001', @data + '1', @data, '2024-01-30 11:30:00', 'TST');

						INSERT INTO [dbo].[TbsTestTable3] VALUES ('00000001-0000-0000-0000-000000000001', @data + '1', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable3] VALUES ('00000001-0000-0000-0000-000000000002', @data + '2', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable3] VALUES ('00000001-0000-0000-0000-000000000003', @data + '3', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable3] VALUES ('00000001-0000-0000-0000-000000000004', @data + '4', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable3] VALUES ('00000001-0000-0000-0000-000000000005', @data + '5', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable3] VALUES ('00000001-0000-0000-0000-000000000006', @data + '6', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable3] VALUES ('00000001-0000-0000-0000-000000000007', @data + '7', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable3] VALUES ('00000001-0000-0000-0000-000000000008', @data + '8', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable3] VALUES ('00000001-0000-0000-0000-000000000009', @data + '9', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable3] VALUES ('00000001-0000-0000-0000-000000000010', @data + '10', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable3] VALUES ('00000001-0000-0000-0000-000000000011', @data + '11', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable3] VALUES ('00000001-0000-0000-0000-000000000012', @data + '12', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable3] VALUES ('00000001-0000-0000-0000-000000000013', @data + '13', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable3] VALUES ('00000001-0000-0000-0000-000000000014', @data + '14', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable3] VALUES ('00000001-0000-0000-0000-000000000015', @data + '15', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable3] VALUES ('00000001-0000-0000-0000-000000000016', @data + '16', @data, '2024-01-30 11:30:00', 'TST');
														
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000001', @data + '1', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000002', @data + '2', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000003', @data + '3', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000004', @data + '4', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000005', @data + '5', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000006', @data + '6', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000007', @data + '7', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000008', @data + '8', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000009', @data + '9', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000010', @data + '10', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000011', @data + '11', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000012', @data + '12', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000013', @data + '13', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000014', @data + '14', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000015', @data + '15', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000016', @data + '16', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000017', @data + '17', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000018', @data + '18', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000019', @data + '19', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000020', @data + '20', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000021', @data + '21', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000022', @data + '22', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000023', @data + '23', @data, '2024-01-30 11:30:00', 'TST');
						INSERT INTO [dbo].[TbsTestTable4] VALUES ('00000001-0000-0000-0000-000000000024', @data + '24', @data, '2024-01-30 11:30:00', 'TST');";

			TestConnection.ExecuteNonQuery(sql);
		}

		SpaceUsed GetTableSpace(string tableName)
		{
			SpaceUsed spaceUsed = new SpaceUsed();
			var sql = $"exec sp_spaceused 'dbo.{tableName}'";
			TestConnection.ExecuteReader(sql, x =>
			{
				spaceUsed.Table = x.GetString(0);
				spaceUsed.RowCount = int.Parse(x.GetString(1));
				spaceUsed.ReservedSpaceMB = decimal.Parse(x.GetString(2).Replace(" KB", "")) / 1024;
				spaceUsed.DataSpaceMB = decimal.Parse(x.GetString(3).Replace(" KB", "")) / 1024;
				spaceUsed.IndexSpaceMB = decimal.Parse(x.GetString(4).Replace(" KB", "")) / 1024;
				spaceUsed.UnusedSpaceMB = decimal.Parse(x.GetString(5).Replace(" KB", "")) / 1024;
			});

			return spaceUsed;
		}
	}
}
