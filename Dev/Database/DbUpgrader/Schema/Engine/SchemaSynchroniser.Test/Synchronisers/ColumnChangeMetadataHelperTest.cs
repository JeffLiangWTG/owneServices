using System;
using System.Linq;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.Testing;

sealed class ColumnChangeMetadataHelperTest : TestCase
{
	AdminConnection adminConnection;
	IDisposable dropDb;
	IDisposable useMasterDb;
	const string SchemaName = "dbo";
	const string TableName = "TestTable";

	public void TestGetColumnChangeMetadata()
	{
		// Act
		var columns = ColumnChangeMetadataHelper.GetTableColumnMetadata(adminConnection, SchemaName, TableName);

		// Assert
		var formattedColumns = columns
			.Select(c => string.Join(", ", c.TableSchema, c.TableName, c.FullAddColumnDeclaration))
			.ToArray();
		AssertContainsExactElementsInAnyOrder(
			[
				"dbo, TestTable, [PK] uniqueidentifier NOT NULL",
				"dbo, TestTable, [ColBitSparse] bit SPARSE NULL",
				"dbo, TestTable, [ColSmallInt] smallint NOT NULL CONSTRAINT [DF_TestTable_ColSmallInt] DEFAULT ((0))",
				"dbo, TestTable, [ColInt] int NOT NULL CONSTRAINT [DF_TestTable_ColInt] DEFAULT ((0))",
				"dbo, TestTable, [ColChar] char(3) NOT NULL CONSTRAINT [DF_TestTable_ColChar] DEFAULT ('ABC')",
				"dbo, TestTable, [ColNChar] nchar(4) NOT NULL CONSTRAINT [DF_TestTable_ColNChar] DEFAULT (N'')",
				"dbo, TestTable, [ColVarChar] varchar(5) NOT NULL CONSTRAINT [DF_TestTable_ColVarChar] DEFAULT ('')",
				"dbo, TestTable, [ColNVarChar] nvarchar(6) NOT NULL CONSTRAINT [DF_TestTable_ColNVarChar] DEFAULT (N'')",
				"dbo, TestTable, [ColDateTime] datetime NOT NULL",
				"dbo, TestTable, [ColDecimal] decimal(10, 5) NULL",
			],
			formattedColumns);
	}

	protected override void SetUp()
	{
		base.SetUp();

		adminConnection = Db.NewAdminConnection(Db.ServerName, Db.SqlMasterDb);
		dropDb = AdoTestUtils.CreateDbDropExistingDisposable(adminConnection, nameof(ColumnChangeMetadataHelperTest), nameof(ColumnChangeMetadataHelperTest));
		useMasterDb = ((ICurrentDbControl)adminConnection).UseDatabase(nameof(ColumnChangeMetadataHelperTest));

		adminConnection.ExecuteNonQuery($@"
CREATE TABLE {SchemaName}.{TableName} (
	PK UNIQUEIDENTIFIER NOT NULL,
	ColBitSparse BIT SPARSE NULL,
	ColSmallInt SMALLINT DEFAULT 0 NOT NULL,
	ColInt INT DEFAULT 0 NOT NULL,
	ColChar CHAR(3) DEFAULT 'ABC' NOT NULL,
	ColNChar NCHAR(4) DEFAULT N'' NOT NULL,
	ColVarChar VARCHAR(5) DEFAULT '' NOT NULL,
	ColNVarChar NVARCHAR(6) DEFAULT N'' NOT NULL,
	ColDateTime DATETIME NOT NULL,
	ColDecimal DECIMAL(10,5) NULL
);
");
	}

	protected override void TearDown()
	{
		useMasterDb.Dispose();
		dropDb.Dispose();
		adminConnection.Dispose();

		base.TearDown();
	}
}
