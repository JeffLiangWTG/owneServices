using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class LatestSchemaDbTemplateTest : TestCase
	{
		public void TestCreateDropExisting()
		{
			Test(true, false);
			Test(false, true);

			void Test(bool managerKeyExists, bool expectedDbCreated)
			{
				// Arranges
				const string TemplateDbName = "TemplateDbForTest";
				var manager = new Mock<IUpgradeManager>();
				manager.Setup(x => x.ManagerKeyExists(It.IsAny<DbConnection>(), TemplateDbName)).Returns(managerKeyExists);
				var creator = new TemplateDbCreatorForTest(manager.Object, TemplateDbName);

				AssertEquals(false, Db.Connection.DatabaseExists(creator.DbName_Exposed));

				using (new DisposableAction(
					// Act
					() => ((IAuxiliaryDbCreator)creator).CreateDropExisting(),
					// Cleanup
					() => ((IAuxiliaryDbCreator)creator).Drop()))
				{
					AssertEquals(expectedDbCreated, Db.Connection.DatabaseExists(creator.DbName_Exposed));
					AssertEquals("DelayedDurability", expectedDbCreated, Db.Connection.Exists($"from sys.databases where name = '{creator.DbName_Exposed}' and DELAYED_DURABILITY_DESC = 'FORCED'"));
				}
			}
		}

		public void TestCreateDropExisting_SetReadOnly_WithRetry()
		{
			const string TemplateDbName = "TemplateDbForTest";
			var manager = new Mock<IUpgradeManager>();
			manager.Setup(x => x.ManagerKeyExists(It.IsAny<DbConnection>(), TemplateDbName)).Returns(false);
			var creator = new TemplateDbCreatorForRetryTest(manager.Object, TemplateDbName);

			creator.PrepareFunc = (conn) => conn.PrepareRetryContextForTest(
				condition: (sqlText, executionCount) => sqlText.Contains(" SET READ_ONLY"),
				action: (sqlText, executionCount) =>
				{
					if (executionCount < 2)
					{
						throw SqlExceptionBuilder.CreateSqlException(1222, "Lock request timeout exceeded");
					}
				});
			creator.AssertAction = (conn) => AssertEquals(2, conn.RetryContext_ForTest.ExecutionCount);

			using (new DisposableAction(() => ((IAuxiliaryDbCreator)creator).Drop()))
			{
				((IAuxiliaryDbCreator)creator).CreateDropExisting();

				manager.Verify(x => x.ShowInfoMessage(It.Is<string>(s => s.StartsWith("\t    Retrying in"))), Times.Once());
			}
		}

		public class TemplateDbCreatorForRetryTest : TemplateDbCreatorForTest
		{
			public TemplateDbCreatorForRetryTest(IUpgradeManager manager, string templateDbName)
				: base(manager, templateDbName)
			{
			}

			public Func<AdminConnection, IDisposable> PrepareFunc { get; set; }

			public Action<AdminConnection> AssertAction { get; set; }

			protected override void CreateDropExisting_Core(AdminConnection conn)
			{
				using (PrepareFunc?.Invoke(conn))
				{
					base.CreateDropExisting_Core(conn);

					AssertAction?.Invoke(conn);
				}
			}
		}
	}
}
