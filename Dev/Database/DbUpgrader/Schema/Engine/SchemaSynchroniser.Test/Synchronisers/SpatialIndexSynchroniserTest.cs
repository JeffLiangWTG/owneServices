using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	public class SpatialIndexSynchroniserTest : TransactionedTestCase
	{
		public void TestSynchronizeSpatialIndexes()
		{
			// Arrange
			const string dbName = "TestSynchronizeSpatialIndexes";
			const string templateDb = "DBUPG_TestSynchronizeSpatialIndexes";

			var logBuilder = new StringBuilder();
			var loggerMock = new Mock<IUpgradeTaskWorkflowLogger>();
			loggerMock.Setup(s => s.StartSubtask(It.IsAny<string>())).Callback((string str) => logBuilder.Append(str));

			using (AdoTestUtils.CreateDbDropExistingDisposable(dbName, dbName))
			using (AdoTestUtils.CreateDbDropExistingDisposable(templateDb, dbName))
			using (var connection = Db.NewAdminConnection(dbName))
			{
				using (((ICurrentDbControl)connection).UseDatabase(templateDb))
				{
					// Template
					_ = connection.ExecuteNonQuery(@"
CREATE TABLE dbo.Table1
(
	S_PK        uniqueidentifier NOT NULL,
	S_Geometry  geometry             NULL,
	S_Geography geography            NULL,

	CONSTRAINT PK_Table1 PRIMARY KEY CLUSTERED (S_PK)
);

CREATE SPATIAL INDEX IX_Spatial_Geometry       ON Table1 (S_Geometry)  USING GEOMETRY_GRID       WITH (BOUNDING_BOX = (1, 1, 2, 2));
CREATE SPATIAL INDEX IX_Spatial_Geography      ON Table1 (S_Geography) USING GEOGRAPHY_GRID      WITH (GRIDS = (LOW, LOW, LOW, LOW));
CREATE SPATIAL INDEX IX_Spatial_Geography_Auto ON Table1 (S_Geography) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 5);
CREATE SPATIAL INDEX IX_Spatial_Geography_2    ON Table1 (S_Geography) USING GEOGRAPHY_AUTO_GRID WITH (ALLOW_PAGE_LOCKS = OFF);
");
				}

				using (((ICurrentDbControl)connection).UseDatabase(dbName))
				{
					// Main DB
					_ = connection.ExecuteNonQuery(@"
CREATE TABLE dbo.Table1
(
	S_PK        uniqueidentifier NOT NULL,
	S_Geometry  geometry             NULL,
	S_Geography geography            NULL,

	CONSTRAINT PK_Table1 PRIMARY KEY CLUSTERED (S_PK)
);

CREATE SPATIAL INDEX IX_Spatial_Geometry_Auto  ON Table1 (S_Geometry)  USING GEOMETRY_AUTO_GRID  WITH (BOUNDING_BOX = (1, 1, 2, 2));
CREATE SPATIAL INDEX IX_Spatial_Geography      ON Table1 (S_Geography) USING GEOGRAPHY_GRID      WITH (GRIDS = (LOW, LOW, LOW, LOW));
CREATE SPATIAL INDEX IX_Spatial_Geography_Auto ON Table1 (S_Geography) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 2);
CREATE SPATIAL INDEX IX_Spatial_Geography_2    ON Table1 (S_Geography) USING GEOGRAPHY_AUTO_GRID;
");
				}

				var synchroniser = new SpatialIndexSynchroniser(connection, dbName, templateDb, loggerMock.Object);

				// Act
				synchroniser.SynchroniseAll();

				// Assert
				var expectedIndexes = new[]
				{
					"SPATIAL INDEX [IX_Spatial_Geometry] ON [dbo].[Table1] ([S_Geometry]) USING GEOMETRY_GRID WITH (CELLS_PER_OBJECT = 16, BOUNDING_BOX = (1, 1, 2, 2), GRIDS = (MEDIUM, MEDIUM, MEDIUM, MEDIUM))",
					"SPATIAL INDEX [IX_Spatial_Geography] ON [dbo].[Table1] ([S_Geography]) USING GEOGRAPHY_GRID WITH (CELLS_PER_OBJECT = 16, GRIDS = (LOW, LOW, LOW, LOW))",
					"SPATIAL INDEX [IX_Spatial_Geography_Auto] ON [dbo].[Table1] ([S_Geography]) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 5)",
					"SPATIAL INDEX [IX_Spatial_Geography_2] ON [dbo].[Table1] ([S_Geography]) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 12, ALLOW_PAGE_LOCKS = OFF)",
				};

				AssertContainsExactElementsInAnyOrder("List of indexes", SpatialIndexLoader.Load(connection, "dbo", "Table1").ConvertAll(c => c.Definition), expectedIndexes);

				var logs = logBuilder.ToString();
				AssertContains("Dropping old indexes", logs);
				AssertContains("(-) [dbo].[Table1].[IX_Spatial_Geometry_Auto]", logs);
				AssertContains("Altering modified indexes", logs);
				AssertContains("(~) [dbo].[Table1].[IX_Spatial_Geography_Auto]", logs);
				AssertContains("Creating new indexes", logs);
				AssertContains("(+) [dbo].[Table1].[IX_Spatial_Geometry]", logs);
				AssertContains("Altering indexes with modified options", logs);
				AssertContains("(~) [dbo].[Table1].[IX_Spatial_Geography_2]", logs);
			}
		}
	}
}
