using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.PreUpgrade.Testing
{
	sealed class RenameTableTransformationTest : TransactionedTestCase
	{
		public void TestTableRenameTransform()
		{
			PrepareTestData();

			new RenameTableTransformationForTesting().Run();
			AssertTransformationResults();

			new RenameTableTransformationForTesting().Run();
			AssertTransformationResults();
		}

		void PrepareTestData()
		{
			var sqlText = @"
CREATE TABLE[TableOld1]
(
	ColPk                         int NOT NULL,
	ColUk                         int NOT NULL,
	ColFk                         int NOT NULL,
	ColWithCheck                  int NOT NULL,
	ColWithDefault                int NOT NULL,
	ColWithIndex                  int NOT NULL,
	ColWithStats                  int NOT NULL,
	ColWithProgrammabilityObjects int NOT NULL,
	ColUk1                        int NOT NULL,
	ColUk2                        int NOT NULL,
);

ALTER TABLE[TableOld1] WITH CHECK ADD
	CONSTRAINT[PK_TableOld1]      PRIMARY KEY CLUSTERED(ColPk),
	CONSTRAINT[UK_TableOld1]      UNIQUE(ColUk),
	CONSTRAINT[FK_TableOld1]      FOREIGN KEY (ColFk)REFERENCES[TableOld1](ColPk),
	CONSTRAINT[Check01_TableOld1] CHECK(ColWithCheck != 0),
	CONSTRAINT[Check02_TableOld1] CHECK(SYSUTCDATETIME() > '2012-11-25'),
	CONSTRAINT[Default_TableOld1] DEFAULT 0 FOR ColWithDefault
;

	CREATE UNIQUE NONCLUSTERED INDEX[Index_TableOld1] ON[TableOld1](ColWithIndex);
	CREATE NONCLUSTERED INDEX[Index2_ColUk_ColWithIndex] ON[TableOld1](ColUk, ColWithIndex);
	CREATE NONCLUSTERED INDEX[Index3_ColUk] ON[TableOld1](ColUk) INCLUDE(ColWithIndex);
	CREATE NONCLUSTERED INDEX[Index4_Filtered_ColUk] ON[TableOld1](ColUk) WHERE(ColWithIndex > 0);

	CREATE NONCLUSTERED INDEX[IndexRename1_ColUk1]              ON[TableOld1](ColUk1);
	CREATE NONCLUSTERED INDEX[IndexRename2_ColUk1_ColUk]        ON[TableOld1](ColUk1, ColUk);
	CREATE NONCLUSTERED INDEX[IndexRename3_ColUk1_ColUk_ColUk2] ON[TableOld1](ColUk1, ColUk, ColUk2);
	CREATE NONCLUSTERED INDEX[IndexRename4_ColUk_ColUk1]        ON[TableOld1](ColUk, ColUk1);

	CREATE STATISTICS[Stat_TableOld1] ON[TableOld1](ColWithStats);
	CREATE STATISTICS[Stat_Filtered_TableOld1] ON[TableOld1](ColUk) WHERE(ColWithStats > 0);

	INSERT[TableOld1] VALUES
	(1, 1, 1, 1, 1, 1, 1, 1, 1, 1),
	(2, 2, 2, 2, 2, 2, 2, 2, 2, 2)
;

CREATE TABLE[TableOld2]
(
	ColPk                         int NOT NULL,
)
";
			TestConnection.ExecuteNonQuery(sqlText);

			TestConnection.ExecuteNonQuery("CREATE VIEW [View_TableOld1] WITH SCHEMABINDING AS SELECT ColWithProgrammabilityObjects FROM dbo.[TableOld1];");
			TestConnection.ExecuteNonQuery("CREATE VIEW [View2_TableOld1] WITH SCHEMABINDING AS SELECT ColWithProgrammabilityObjects FROM dbo.[View_TableOld1];");
			TestConnection.ExecuteNonQuery("CREATE FUNCTION [Function_TableOld1]() RETURNS TABLE WITH SCHEMABINDING AS RETURN SELECT ColWithProgrammabilityObjects FROM dbo.[TableOld1];");
			TestConnection.ExecuteNonQuery("CREATE FUNCTION [Function2_TableOld1]() RETURNS TABLE AS RETURN SELECT ColWithProgrammabilityObjects FROM dbo.[TableOld1];");
			TestConnection.ExecuteNonQuery("CREATE PROCEDURE [Proc_TableOld1] AS SELECT ColWithProgrammabilityObjects FROM dbo.[TableOld1];");
			TestConnection.ExecuteNonQuery("CREATE TRIGGER [Trigger_TableOld1] ON [TableOld1] FOR INSERT AS IF UPDATE(ColWithProgrammabilityObjects) RETURN;");
		}

		void AssertTransformationResults()
		{
			AssertEquals(false, DbObjectCreator.TableExists(TestConnection, "TableOld1"));
			AssertEquals(false, DbObjectCreator.TableExists(TestConnection, "TableOld2"));
			AssertEquals(true, DbObjectCreator.TableExists(TestConnection, "TableNew1"));
			AssertEquals(true, DbObjectCreator.TableExists(TestConnection, "TableNew2"));

			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, "TableNew1", "ColPK"));
			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, "TableNew1", "ColUk"));
			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, "TableNew1", "ColFK"));
			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, "TableNew1", "ColWithCheck"));
			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, "TableNew1", "ColWithDefault"));
			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, "TableNew1", "ColWithIndex"));
			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, "TableNew1", "ColWithStats"));
			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, "TableNew1", "ColWithProgrammabilityObjects"));
			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, "TableNew1", "ColUk1"));
			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, "TableNew1", "ColUk2"));

			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, "TableNew2", "ColPK"));
			AssertEquals(false, DbObjectCreator.ColumnExists(TestConnection, "TableNew2", "ColUk"));
			AssertEquals(false, DbObjectCreator.ColumnExists(TestConnection, "TableNew2", "ColFK"));
			AssertEquals(false, DbObjectCreator.ColumnExists(TestConnection, "TableNew2", "ColWithCheck"));
			AssertEquals(false, DbObjectCreator.ColumnExists(TestConnection, "TableNew2", "ColWithDefault"));
			AssertEquals(false, DbObjectCreator.ColumnExists(TestConnection, "TableNew2", "ColWithIndex"));
			AssertEquals(false, DbObjectCreator.ColumnExists(TestConnection, "TableNew2", "ColWithStats"));
			AssertEquals(false, DbObjectCreator.ColumnExists(TestConnection, "TableNew2", "ColWithProgrammabilityObjects"));
			AssertEquals(false, DbObjectCreator.ColumnExists(TestConnection, "TableNew2", "ColUk1"));
			AssertEquals(false, DbObjectCreator.ColumnExists(TestConnection, "TableNew2", "ColUk2"));

			//assert data
			AssertEquals(2, (int)Db.Connection.ExecuteScalar(string.Format("SELECT Count(*) FROM TableNew1")));
		}

		class RenameTableTransformationForTesting : RenameTableTransformation
		{
			public override string UserDescription
			{
				get { return "RenameTableTransformationForTesting"; }
			}

			protected internal override IEnumerable<IRenameTableTransformationInfo> RenameTableInfoList
			{
				get
				{
					yield return new RenameTableTransformationInfo("TableOld1", "TableNew1");
					yield return new RenameTableTransformationInfo("TableOld2", "TableNew2");
				}
			}
		}
	}
}
