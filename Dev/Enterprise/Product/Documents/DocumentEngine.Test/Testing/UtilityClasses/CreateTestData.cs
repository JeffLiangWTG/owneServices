using System;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Data;
using CargoWise.IO;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	public static class TestData
	{
		public const string DocEngineTestTableName = "##DocEngineTest";
		public const string JobTempTableName = "##JobTest";
		public const string HeaderTempTableName = "##HeaderTest";
		public const string LinesTempTableName = "##LinesTest";
		public const string LinesEmptyTableName = "##EmptyLines";
		public const string DocEngineTestProcedure = "##DocEngineTestProcedure";

		public const int DocEngineTestTableRowCount = 160;

		public static Guid GetFirstGuidInTestTable()
		{
			return (Guid)CargoWise.Data.Db.Connection.ExecuteScalar("SELECT TOP 1 UnitTestID FROM " + DocEngineTestTableName);
		}

		public static void CreateDocEngineTestTable()
		{
			var sb = new StringBuilder();
			sb.Append(@"
IF EXISTS (SELECT null from tempdb.sys.objects where name like '" + DocEngineTestTableName + @"%')
  drop table " + DocEngineTestTableName + @"

CREATE TABLE " + DocEngineTestTableName + @" (
	[UnitTestID] [uniqueidentifier] NOT NULL DEFAULT (newid()),
	[CharField1] [char] (40) COLLATE database_default  NOT NULL DEFAULT (0),
	[CharField2] [char] (40) COLLATE database_default  NOT NULL DEFAULT (0),
	[CharField3] [char] (40) COLLATE database_default  NOT NULL DEFAULT (0),
	[CharField4] [char] (40) COLLATE database_default  NOT NULL DEFAULT (0),
	[DateField]  [datetime] NULL ,
	[CompressedImageField]  varbinary(max) NULL ,
	[UncompressedImageField]  varbinary(max) NULL ,
) ON [PRIMARY]
");
			sb.AppendLine("insert into " + DocEngineTestTableName + " values ");
			for (int i = 0; i < DocEngineTestTableRowCount; i++)
			{
				if (i > 0)
				{
					sb.Append(", ");
				}

				sb.AppendLine(" (newid(), '" + (i / 40).ToString() + "', '" + (i / 20).ToString() + "', '" + (i / 10).ToString() + "', '" + (i / 5).ToString() + "', '" + new DateTime(2001, 1, 1).AddDays(i / 30).ToString(CultureInfo.InvariantCulture) + "', @CompressedBytes, @UncompressedBytes)\n");
			}
			DbCommand cmd = Db.Connection.Command(sb.ToString());
			byte[] uncompressedBytes = new byte[] { (byte)'a', (byte)'b', (byte)'c' };
			byte[] compressedBytes = Compressor.Compress(uncompressedBytes);
			cmd.AddParameter("@UncompressedBytes", SqlDbType.Binary, uncompressedBytes.Length, uncompressedBytes);
			cmd.AddParameter("@CompressedBytes", SqlDbType.Binary, compressedBytes.Length, compressedBytes);
			cmd.ExecuteNonQuery();
		}

		public static void CreateJobTestTable()
		{
			string sqlStatement = @"
IF EXISTS (SELECT null from tempdb.sys.objects where name like '" + JobTempTableName + @"%')
  drop table " + JobTempTableName + @"

CREATE TABLE " + JobTempTableName + @" (
	[Number] [char] (40) NOT NULL DEFAULT (''),
	[ConsolNumber] [char] (40) NOT NULL DEFAULT (''),
	[Consignor] [char] (40) NOT NULL DEFAULT (''),
	[Consignee] [char] (40) NOT NULL DEFAULT (''),
	[OrderList] [char] (40) NOT NULL DEFAULT (''),
	[GoodsReceival] [char] (40) NOT NULL DEFAULT (''),
	[DocumentedWeight] [float] NOT NULL DEFAULT (''),
	[DocumentedVolume] [float] NOT NULL DEFAULT (''),
	[DocumentedChargeable] [float] NOT NULL DEFAULT (''),
	[PackageCount] [float] NOT NULL DEFAULT (''),
	[PackageUnit] [char] (40) NOT NULL DEFAULT (''),
	[ETD] [datetime] NOT NULL DEFAULT (''),
	[ETA] [datetime] NOT NULL DEFAULT (''),
	[Origin] [char] (40) NOT NULL DEFAULT (''),
	[Destination] [char] (40) NOT NULL DEFAULT (''),
	[GoodDescription] [char] (40) NOT NULL DEFAULT (''),
	[Agent] [char] (40) NOT NULL DEFAULT (''),
	[Vessel] [char] (40) NOT NULL DEFAULT (''),
	[Voyage] [char] (40) NOT NULL DEFAULT (''),
	[LloydsNumber] [char] (40) NOT NULL DEFAULT (''),
	[OceanBill] [char] (40) NOT NULL DEFAULT (''),
	[HouseBill] [char] (40) NOT NULL DEFAULT (''),
	[ContainerList] [char] (40) NOT NULL DEFAULT (''),
	[VESSELNAME] [char] (40) NOT NULL DEFAULT (''),
	[VOYAGENUMBER] [char] (40) NOT NULL DEFAULT (''),
	[FCLCUTOFF] [char] (40) NOT NULL DEFAULT (''),
	[WeightUnit] [float] (40) NOT NULL DEFAULT (''),
	[VolumeUnit] [float] (40) NOT NULL DEFAULT (''),
	[VolumeTotal] [float] (40) NOT NULL DEFAULT (''),
	[PackTotal] [float] (40) NOT NULL DEFAULT (''),
	[ChargeableUnit] [char] (40) NOT NULL DEFAULT (''),
) ON [PRIMARY]
";

			sqlStatement += "insert into " + JobTempTableName + " values ('Job1', 'Consol1', 'Consignor \"test company', 'Consignee test company', 'order1, order 2', 'GoodsReceival test', 55.36, 2.3, 65, 12, 'Box', '2003-11-12 8:30AM', '2004-1-1', 'Here', 'There', 'Test goods', 'Planet Express', 'Planet Express enterprise', 'Voyage 1', 'LLoyds1', 'oceana11', 'house11', 'container1, 2, 4, 1120','Vessel 1','Voyage 1','FCL Cut',12.34,12.34,12.34,1,'CNY')";
			Db.Connection.ExecuteNonQuery(sqlStatement);
		}

		public static void CreateHeaderTestTable()
		{
			string sqlStatement = @"
IF EXISTS (SELECT null from tempdb.sys.objects where name like '" + HeaderTempTableName + @"%')
  drop table " + HeaderTempTableName + @"

CREATE TABLE " + HeaderTempTableName + @" (
	[Number] [char] (40) NOT NULL DEFAULT (''),
	[BillToCode] [char] (40) NOT NULL DEFAULT (''),
	[BillToID] [uniqueidentifier] NOT NULL,
	[Currency] [char] (40) NOT NULL DEFAULT (''),
	[AmountExTax] [decimal] NOT NULL DEFAULT (''),
	[GST]  [decimal] NOT NULL DEFAULT (''),
	[AmountIncTax] [decimal] NOT NULL DEFAULT (''),
	[Date] [DateTime]  NOT NULL DEFAULT (''),
	[OrgID] [char] (40) NOT NULL,
	[OrgAddressID] [char] (40) NOT NULL,
	[JobDocAddressID] [char] (40) NOT NULL,
	[NullOrgID] [uniqueidentifier] NULL,
	[MultiLineText] [char] (100) NULL,
	[NoWhereClause] [char] (20) NULL,
	[SomeWordsInCaps] [varchar] (60) NULL,
	[UncompressedImageField]  varbinary(max) NULL,
	[CompressedImageField]  varbinary(max) NULL
) ON [PRIMARY]
";

			Guid orgPK = (Guid)CargoWise.Data.Db.Connection.ExecuteScalar("SELECT OH_PK FROM dbo.ORGHEADER WHERE OH_FULLNAME = 'MURGON LEATHER CO LIMITED'");
			CargoWise.Data.DbCommand command = CargoWise.Data.Db.Connection.Command("SELECT TOP 1 OA_PK FROM dbo.ORGADDRESS INNER JOIN dbo.ORGADDRESSCAPABILITY ON (OA_PK = PZ_OA) WHERE OA_OH = @OrgPK AND PZ_ADDRESSTYPE = 'OFC'");
			command.AddParameter("@OrgPK", SqlDbType.UniqueIdentifier, orgPK);
			Guid orgAddressPK = (Guid)command.ExecuteScalar();

			sqlStatement +=
				"insert into " + HeaderTempTableName + " values (" +
				"'Header1', " +
				"'CW1SYD', " +
				"'" + orgPK + "', " +
				"'AUD', " +
				"133.3, " +
				"13.3, " +
				"146.6, " +
				"'2003-10-2', " +
				"'" + orgPK + "', " +
				"'" + orgAddressPK + "', " +
				"'" + JobDocAddressPKString + "', " +
				"null, " +
				"'Line1 \n Line2 \n \n Line4', " +
				"'NoWhereClause', " +
				"'SOME  WORDS I N CAPS, SOME \r\nWORDS IN CAPS\n WITH BREAK LINE!', " +
				"@UncompressedBytes, " +
				"@CompressedBytes" +
				")";
			DbCommand cmd = Db.Connection.Command(sqlStatement);
			byte[] uncompressedBytes = new byte[] { (byte)'a', (byte)'b', (byte)'c' };
			byte[] compressedBytes = Compressor.Compress(uncompressedBytes);
			cmd.AddParameter("@UncompressedBytes", SqlDbType.Binary, uncompressedBytes.Length, uncompressedBytes);
			cmd.AddParameter("@CompressedBytes", SqlDbType.Binary, compressedBytes.Length, compressedBytes);
			cmd.ExecuteNonQuery();
		}

		public static void EmptyLinesTestTable()
		{
			Db.Connection.ExecuteNonQuery("delete from " + LinesTempTableName);
		}

		public const string JobDocAddressPKString = "A7604D44-A701-44A4-B18A-2596D60EE204";

		public static void CreateLinesTestTable()
		{
			var sb = new StringBuilder();
			sb.Append(@"
IF EXISTS (SELECT null from tempdb.sys.objects where name like '" + LinesTempTableName + @"%')
  drop table " + LinesTempTableName + @"

CREATE TABLE " + LinesTempTableName + @" (
	[Description] [char] (40) NOT NULL DEFAULT (''),
	[GSTRate] [decimal] NOT NULL DEFAULT (''),
	[AmountExTax] [decimal] NOT NULL DEFAULT (''),
	[GST]  [decimal] NOT NULL DEFAULT (''),
	[AmountIncTax] [decimal] NOT NULL DEFAULT (''),
	[Currency] [char] (40) NOT NULL DEFAULT (''),
	[AccountingGroupCode] [char] (5) NOT NULL DEFAULT (''),
	[AccountingGroupName] [char] (40) NOT NULL DEFAULT (''),
) ON [PRIMARY]
insert into " + LinesTempTableName + " values ");

			for (int i = 0; i < DocEngineTestTableRowCount; i++)
			{
				if (i > 0)
				{
					sb.Append(",");
				}
				sb.AppendLine("('Unit test Line number " + i + "', '" + (i % 10).ToString() + "', '" + (i * 3).ToString() + "', " + (i % 8).ToString() + ", " + (i * 3 + 10).ToString() + ", 'AUD', '" + (i % 5).ToString() + "', 'Accounting Group " + (i % 5).ToString() + "')");
			}
			Db.Connection.ExecuteNonQuery(sb.ToString());
		}

		public static void CreateEmptyLinesTestTable()
		{
			string sqlStatement = @"
IF EXISTS (SELECT null from tempdb.sys.objects where name like '" + LinesEmptyTableName + @"%')
  drop table " + LinesEmptyTableName + @"

CREATE TABLE " + LinesEmptyTableName + @" (
	[Description] [char] (40) NOT NULL DEFAULT (''),
	[GSTRate] [decimal] NOT NULL DEFAULT (''),
	[AmountExTax] [decimal] NOT NULL DEFAULT (''),
	[GST]  [decimal] NOT NULL DEFAULT (''),
	[AmountIncTax] [decimal] NOT NULL DEFAULT (''),
	[Currency] [char] (40) NOT NULL DEFAULT (''),
	[AccountingGroupName] [char] (40) NOT NULL DEFAULT (''),
) ON [PRIMARY]
";

			Db.Connection.ExecuteNonQuery(sqlStatement);
		}

		public static void CreateDocEngineTestProcedure()
		{
			var sqlStatement = @"
CREATE PROCEDURE " + DocEngineTestProcedure + @" AS
BEGIN
	SELECT COUNT(*) FROM " + DocEngineTestTableName +
" END";

			Db.Connection.ExecuteNonQuery(sqlStatement);
		}
	}
}
