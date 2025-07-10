using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class MainDbSchemaSynchronisationWrapperTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestRefreshDependentScripts()
		{
			using (AdoTestUtils.CreateDbDropExistingDisposable(DatabaseName))
			using (var connection = Db.NewAdminConnection(DatabaseName))
			{
				connection.ExecuteNonQuery("create table WorkItem (WKI_PK uniqueidentifier not null, WKI_Foo int)");
				var upgradeManagerMock = new Mock<IUpgradeManager>();
				var mockRefresher = new Mock<IDependentScriptRefresher>();

				var schemaSynchronisationWrapper = new MainDbSchemaSynchronisationWrapper(upgradeManagerMock.Object, DatabaseName, connection);
				using (new DisposableAction(() => schemaSynchronisationWrapper.DropTemplateDb()))
				{
					schemaSynchronisationWrapper.Run();
					schemaSynchronisationWrapper.RefreshDependentScripts(mockRefresher.Object);
				}

				mockRefresher.Verify(x => x.RefreshDependentScripts(connection, DatabaseName, "dbo", "WorkItem", upgradeManagerMock.Object), Times.Once);
			}
		}

		const string DatabaseName = nameof(MainDbSchemaSynchronisationWrapperTest);
		const string TemplateDbName = DatabaseName + "_TemplateDb";
	}
}
