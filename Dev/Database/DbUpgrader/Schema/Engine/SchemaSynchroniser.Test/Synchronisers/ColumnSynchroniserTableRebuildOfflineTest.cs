using System.Linq;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Database.Abstractions;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class ColumnSynchroniserTableRebuildOfflineTest : TestCaseWithMockMainDbAndTemplateDb
	{
		protected override IAuxiliaryDbCreator GetMockMainDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockMainDb, new string[] {
				ExtProperty.TableDefinition,

				@"CREATE TABLE DataTest (
					PK uniqueidentifier NOT NULL, 
					Data varbinary(max) NULL,
					XXX int NOT NULL,
					CONSTRAINT PK_TestColumnPreSynchroniser PRIMARY KEY NONCLUSTERED (PK));",
			});
		}

		protected override IAuxiliaryDbCreator GetMockTemplateDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockTemplateDb, new string[] {
				@"CREATE TABLE DataTest (
					PK uniqueidentifier NOT NULL, 
					Data varbinary(max) NULL,
					CONSTRAINT PK_TestColumnPreSynchroniser PRIMARY KEY NONCLUSTERED (PK));"
			});
		}

		public void TestAlterColumn()
		{
			try
			{
				using (((ICurrentDbControl)TestConnection).UseDatabase(mockMainDb))
				{
					TestConnection.BeginTransaction();
					var persister = GlobalServiceProvider.Instance.GetRequiredService<ITableRebuildPersisterFactory>().Get(TestConnection);
					persister.Clear();

					new ColumnSynchroniserForTesting(TestConnection, mockMainDb, mockTemplateDb).DropAlterAndAddColumns();
					AssertEquals(0, persister.GetTablesToRebuild().Count());
				}
			}
			finally
			{
				TestConnection.RollbackTransaction();
			}
		}
	}
}
