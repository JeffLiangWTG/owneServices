using System.Linq;
using CargoWise.Database.Shared;

namespace CargoWise.Data.Testing
{
	using NUnit.Framework;

	public class SpatialIndexInfoIntegrationTest : TransactionedTestCase
	{
		public void TestAll_Geography()
		{
			var sch_1 = "test_schema_1";
			var sch_2 = "test_schema_2";
			var tab = "TestSpatialIndexInfo_Geography";
			var ind = "IX_Geography";
			var col = "T_Geography";

			Db.Connection.ExecuteNonQuery($"CREATE SCHEMA {sch_1} CREATE TABLE [{tab}] (T_PK int PRIMARY KEY CLUSTERED, [{col}] geography);");
			Db.Connection.ExecuteNonQuery($"CREATE SCHEMA {sch_2} CREATE TABLE [{tab}] (T_PK int PRIMARY KEY CLUSTERED, [{col}] geography);");

			var index_1 = SpatialIndexInfo.Builder.New(sch_1, tab, ind, TessellationScheme.GEOGRAPHY_AUTO_GRID)
				.Key(col)
				.CellsPerObject(16)
				.GetInfo()
				.Create(Db.Connection);

			var index_2 = SpatialIndexInfo.Builder.New(sch_2, tab, ind, TessellationScheme.GEOGRAPHY_GRID)
				.Key(col)
				.Option(IndexOptions.MAXDOP, 2)
				.Option(IndexOptions.DATA_COMPRESSION, DataCompression.PAGE)
				.Option(IndexOptions.SORT_IN_TEMPDB, true)
				.GetInfo()
				.Create(Db.Connection);

			var expectedDefinition =
				$"SPATIAL INDEX [{ind}] ON [{sch_1}].[{tab}] ([{col}]) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 16)";

			AssertEquals("Definition", expectedDefinition, index_1.Definition);

			expectedDefinition =
				$"SPATIAL INDEX [{ind}] ON [{sch_2}].[{tab}] ([{col}]) USING GEOGRAPHY_GRID WITH (CELLS_PER_OBJECT = 16, GRIDS = (MEDIUM, MEDIUM, MEDIUM, MEDIUM), SORT_IN_TEMPDB = ON, DATA_COMPRESSION = PAGE)";

			AssertEquals("Definition", expectedDefinition, index_2.Definition);

			var index = SpatialIndexLoader.LoadTop1(Db.Connection, null, null, ind);
			expectedDefinition =
				$"SPATIAL INDEX [{ind}] ON [{sch_1}].[{tab}] ([{col}]) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 16)";

			CombineAssertions(() =>
			{
				AssertEquals("Schema Name", sch_1, index.SchemaName);
				AssertEquals("Table Name", tab, index.TableName);
				AssertEquals("Index Name", ind, index.IndexName);
				AssertEquals("IndexTessellation", TessellationScheme.GEOGRAPHY_AUTO_GRID, index.IndexTessellation);
				AssertEquals("CellsPerObject", 16, index.CellsPerObject);
				AssertEquals("RequiresBox", false, index.RequiresBox);
				AssertEquals("Box", null, index.Box);
				AssertEquals("RequiresGrid", false, index.RequiresGrid);
				AssertEquals("Grid", null, index.Grid);
				AssertEquals("HasOptions", false, index.HasOptions);
				AssertEquals("Definition", expectedDefinition, index.Definition);
				AssertEquals("ToString", expectedDefinition, index.ToString());
			});

			index = SpatialIndexLoader.LoadTop1(Db.Connection, sch_2, tab, ind);
			expectedDefinition =
				$"SPATIAL INDEX [{ind}] ON [{sch_2}].[{tab}] ([{col}]) USING GEOGRAPHY_GRID WITH (CELLS_PER_OBJECT = 16, GRIDS = (MEDIUM, MEDIUM, MEDIUM, MEDIUM), DATA_COMPRESSION = PAGE)";

			CombineAssertions(() =>
			{
				AssertEquals("Schema Name", sch_2, index.SchemaName);
				AssertEquals("Table Name", tab, index.TableName);
				AssertEquals("Index Name", ind, index.IndexName);
				AssertEquals("IndexTessellation", TessellationScheme.GEOGRAPHY_GRID, index.IndexTessellation);
				AssertEquals("CellsPerObject", 16, index.CellsPerObject);
				AssertEquals("RequiresBox", false, index.RequiresBox);
				AssertEquals("Box", null, index.Box);
				AssertEquals("RequiresGrid", true, index.RequiresGrid);
				AssertEquals("Grid Level_1", GridSize.MEDIUM, index.Grid.Level_1);
				AssertEquals("Grid Level_2", GridSize.MEDIUM, index.Grid.Level_2);
				AssertEquals("Grid Level_3", GridSize.MEDIUM, index.Grid.Level_3);
				AssertEquals("Grid Level_4", GridSize.MEDIUM, index.Grid.Level_4);
				AssertEquals("HasOptions", true, index.HasOptions);
				AssertEquals("Options.Count", 1, index.Options.Count);
				var option = index.Options.Keys.First();
				AssertEquals("Definition", expectedDefinition, index.Definition);
				AssertEquals("ToString", expectedDefinition, index.ToString());
			});

			var expectedList = new string[]
			{
				$"SPATIAL INDEX [{ind}] ON [{sch_1}].[{tab}] ([{col}]) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 16)",
				$"SPATIAL INDEX [{ind}] ON [{sch_2}].[{tab}] ([{col}]) USING GEOGRAPHY_GRID WITH (CELLS_PER_OBJECT = 16, GRIDS = (MEDIUM, MEDIUM, MEDIUM, MEDIUM), DATA_COMPRESSION = PAGE)",
			};

			AssertContainsExactElementsInAnyOrder("All indexes", expectedList, SpatialIndexLoader.Load(Db.Connection, null, tab, null).Select(i => i.Definition));

			index_1.Drop(Db.Connection);
			index_2.Drop(Db.Connection);

			AssertNull(SpatialIndexLoader.LoadTop1(Db.Connection, sch_1, tab, ind));
			AssertNull(SpatialIndexLoader.LoadTop1(Db.Connection, sch_2, tab, ind));

			Db.Connection.ExecuteNonQuery($"DROP TABLE [{sch_1}].[{tab}];");
			Db.Connection.ExecuteNonQuery($"DROP SCHEMA [{sch_1}];");
			Db.Connection.ExecuteNonQuery($"DROP TABLE [{sch_2}].[{tab}];");
			Db.Connection.ExecuteNonQuery($"DROP SCHEMA [{sch_2}];");

			AssertNull(SpatialIndexLoader.LoadTop1(Db.Connection, sch_1, tab, ind));
			AssertNull(SpatialIndexLoader.LoadTop1(Db.Connection, sch_2, tab, ind));
		}

		public void TestAll_Geometry()
		{
			var sch_1 = "test_schema_1";
			var sch_2 = "test_schema_2";
			var tab = "TestSpatialIndexInfo_Geometry";
			var ind = "IX_Geometry";
			var col = "T_Geometry";

			Db.Connection.ExecuteNonQuery($"CREATE SCHEMA {sch_1} CREATE TABLE [{tab}] (T_PK int PRIMARY KEY CLUSTERED, [{col}] geometry);");
			Db.Connection.ExecuteNonQuery($"CREATE SCHEMA {sch_2} CREATE TABLE [{tab}] (T_PK int PRIMARY KEY CLUSTERED, [{col}] geometry);");

			var index_1 = SpatialIndexInfo.Builder.New(sch_1, tab, ind, TessellationScheme.GEOMETRY_AUTO_GRID)
				.Key(col)
				.CellsPerObject(16)
				.Box(1, 1, 2, 2)
				.GetInfo()
				.Create(Db.Connection);

			var index_2 = SpatialIndexInfo.Builder.New(sch_2, tab, ind, TessellationScheme.GEOMETRY_GRID)
				.Key(col)
				.Box(1, 1, 2, 2)
				.Option(IndexOptions.MAXDOP, 2)
				.Option(IndexOptions.DATA_COMPRESSION, DataCompression.PAGE)
				.Option(IndexOptions.SORT_IN_TEMPDB, true)
				.GetInfo()
				.Create(Db.Connection);

			var expectedDefinition =
				$"SPATIAL INDEX [{ind}] ON [{sch_1}].[{tab}] ([{col}]) USING GEOMETRY_AUTO_GRID WITH (CELLS_PER_OBJECT = 16, BOUNDING_BOX = (1, 1, 2, 2))";

			AssertEquals("Definition", expectedDefinition, index_1.Definition);

			expectedDefinition =
				$"SPATIAL INDEX [{ind}] ON [{sch_2}].[{tab}] ([{col}]) USING GEOMETRY_GRID WITH (CELLS_PER_OBJECT = 16, BOUNDING_BOX = (1, 1, 2, 2), GRIDS = (MEDIUM, MEDIUM, MEDIUM, MEDIUM), SORT_IN_TEMPDB = ON, DATA_COMPRESSION = PAGE)";

			AssertEquals("Definition", expectedDefinition, index_2.Definition);

			var index = SpatialIndexLoader.LoadTop1(Db.Connection, null, null, ind);
			expectedDefinition =
				$"SPATIAL INDEX [{ind}] ON [{sch_1}].[{tab}] ([{col}]) USING GEOMETRY_AUTO_GRID WITH (CELLS_PER_OBJECT = 16, BOUNDING_BOX = (1, 1, 2, 2))";

			CombineAssertions(() =>
			{
				AssertEquals("Schema Name", sch_1, index.SchemaName);
				AssertEquals("Table Name", tab, index.TableName);
				AssertEquals("Index Name", ind, index.IndexName);
				AssertEquals("IndexTessellation", TessellationScheme.GEOMETRY_AUTO_GRID, index.IndexTessellation);
				AssertEquals("CellsPerObject", 16, index.CellsPerObject);
				AssertEquals("RequiresBox", true, index.RequiresBox);
				AssertEquals("Box X_min", 1.0, index.Box.X_min);
				AssertEquals("Box Y_min", 1.0, index.Box.Y_min);
				AssertEquals("Box X_max", 2.0, index.Box.X_max);
				AssertEquals("Box Y_max", 2.0, index.Box.Y_max);
				AssertEquals("RequiresGrid", false, index.RequiresGrid);
				AssertEquals("Grid", null, index.Grid);
				AssertEquals("HasOptions", false, index.HasOptions);
				AssertEquals("Definition", expectedDefinition, index.Definition);
				AssertEquals("ToString", expectedDefinition, index.ToString());
			});

			index = SpatialIndexLoader.LoadTop1(Db.Connection, sch_2, tab, ind);
			expectedDefinition =
				$"SPATIAL INDEX [{ind}] ON [{sch_2}].[{tab}] ([{col}]) USING GEOMETRY_GRID WITH (CELLS_PER_OBJECT = 16, BOUNDING_BOX = (1, 1, 2, 2), GRIDS = (MEDIUM, MEDIUM, MEDIUM, MEDIUM), DATA_COMPRESSION = PAGE)";

			CombineAssertions(() =>
			{
				AssertEquals("Schema Name", sch_2, index.SchemaName);
				AssertEquals("Table Name", tab, index.TableName);
				AssertEquals("Index Name", ind, index.IndexName);
				AssertEquals("IndexTessellation", TessellationScheme.GEOMETRY_GRID, index.IndexTessellation);
				AssertEquals("CellsPerObject", 16, index.CellsPerObject);
				AssertEquals("RequiresBox", true, index.RequiresBox);
				AssertEquals("Box X_min", 1.0, index.Box.X_min);
				AssertEquals("Box Y_min", 1.0, index.Box.Y_min);
				AssertEquals("Box X_max", 2.0, index.Box.X_max);
				AssertEquals("Box Y_max", 2.0, index.Box.Y_max);
				AssertEquals("RequiresGrid", true, index.RequiresGrid);
				AssertEquals("Grid Level_1", GridSize.MEDIUM, index.Grid.Level_1);
				AssertEquals("Grid Level_2", GridSize.MEDIUM, index.Grid.Level_2);
				AssertEquals("Grid Level_3", GridSize.MEDIUM, index.Grid.Level_3);
				AssertEquals("Grid Level_4", GridSize.MEDIUM, index.Grid.Level_4);
				AssertEquals("HasOptions", true, index.HasOptions);
				AssertEquals("Options.Count", 1, index.Options.Count);
				var option = index.Options.Keys.First();
				AssertEquals("Definition", expectedDefinition, index.Definition);
				AssertEquals("ToString", expectedDefinition, index.ToString());
			});

			var expectedList = new string[]
			{
				$"SPATIAL INDEX [{ind}] ON [{sch_1}].[{tab}] ([{col}]) USING GEOMETRY_AUTO_GRID WITH (CELLS_PER_OBJECT = 16, BOUNDING_BOX = (1, 1, 2, 2))",
				$"SPATIAL INDEX [{ind}] ON [{sch_2}].[{tab}] ([{col}]) USING GEOMETRY_GRID WITH (CELLS_PER_OBJECT = 16, BOUNDING_BOX = (1, 1, 2, 2), GRIDS = (MEDIUM, MEDIUM, MEDIUM, MEDIUM), DATA_COMPRESSION = PAGE)",
			};

			AssertContainsExactElementsInAnyOrder("All indexes", expectedList, SpatialIndexLoader.Load(Db.Connection, null, tab, null).Select(i => i.Definition));

			index_1.Drop(Db.Connection);
			index_2.Drop(Db.Connection);

			AssertNull(SpatialIndexLoader.LoadTop1(Db.Connection, sch_1, tab, ind));
			AssertNull(SpatialIndexLoader.LoadTop1(Db.Connection, sch_2, tab, ind));

			Db.Connection.ExecuteNonQuery($"DROP TABLE [{sch_1}].[{tab}];");
			Db.Connection.ExecuteNonQuery($"DROP SCHEMA [{sch_1}];");
			Db.Connection.ExecuteNonQuery($"DROP TABLE [{sch_2}].[{tab}];");
			Db.Connection.ExecuteNonQuery($"DROP SCHEMA [{sch_2}];");

			AssertNull(SpatialIndexLoader.LoadTop1(Db.Connection, sch_1, tab, ind));
			AssertNull(SpatialIndexLoader.LoadTop1(Db.Connection, sch_2, tab, ind));
		}
	}
}
