using System;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Script.Test
{
	sealed class ViewAndRoutineCreatorTest : TransactionedTestCase
	{
		/// <summary>
		/// Views, Procedures, Functions, Triggers and ServiceQueues which names start with "Client" are reserved for client-specific objects.
		/// The full naming convention is: Client[ClientName][ObjectName] - eg. ClientWoolworths_vw_LandedCostAccounting
		/// These objects are to be ignored by DbUpgrader and never deleted. 
		/// NO objects with this naming convetion should be shipped with the base Enterprise.
		/// </summary>
		public void TestNoEnterpriseViewProcFunctionTriggerOrSeviceQueueNamesStartsWithClient()
		{
			string sqlText = @"
				DECLARE @ClientObjectNames varchar(8000)
				SET @ClientObjectNames = ''

				SELECT @ClientObjectNames = @ClientObjectNames + name + char(13) + char(10)
				FROM sys.objects
				WHERE type in ('V', 'P', 'FN','TF','IF', 'TR', 'SQ')
				AND name LIKE 'Client%'

				SELECT @ClientObjectNames";

			string clientObjectNames = Db.Connection.ExecuteScalar(sqlText).ToString();
			AssertEquals("Found some objects names containing the word [client]:\r\n", "", clientObjectNames);
		}

		public void TestClientIndexedViews()
		{
			var mockExtensionObjectSource = new Mock<IExtensionObjectsSource>();
			mockExtensionObjectSource.Setup(x => x.ExtensionObjects).Returns(new ExtensionObjects(
				ImmutableArray<DatabaseObjectCreateScript>.Empty,
				ImmutableArray.Create<DatabaseViewAndRoutineCreateScript>(new DatabaseIndexedViewCreateScript("Name1", "create view", "create index", "drop view", "view"))));

			var serviceProvider = GlobalServiceProvider.Instance;
			var mockServiceProvider = new Mock<IServiceProvider>();
			mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns<Type>(x => serviceProvider.GetService(x));
			mockServiceProvider.Setup(x => x.GetService(typeof(IExtensionObjectsSource))).Returns(mockExtensionObjectSource.Object);

			using (GlobalServiceProvider.Configure(mockServiceProvider.Object))
			{
				var testCreator = new ViewAndRoutineCreatorForTesting(TestConnection.CurrentDatabase, TestConnection);
				var scripts = testCreator.GetViewAndRoutineScriptCollection_Exposed();
				var clientScript1 = scripts.Last() as IndexedViewDbScript;
				AssertType<IndexedViewDbScript>(clientScript1);
				AssertEquals("create index", clientScript1.IndexCreateScript);
			}
		}

		public void TestDoNotDropMsCdcTrigger()
		{
			var testCreator = new ViewAndRoutineCreatorForTesting(TestConnection.CurrentDatabase, TestConnection);
			var expectedList = testCreator.GetViewAndRoutineScriptCollection_Exposed();
			testCreator.CompareViewsAndRoutinesExposed(expectedList, out var dropList, out var createList, out var refreshList);

			AssertCollectionNotContains("To drop list", "tr_MScdc_ddl_event", dropList);
		}

		public void TestCompareViewsAndRoutinesNoExceptionIfEncryptedStoredProcedureNotInExpectedList()
		{
			// Arrange
			var procedureName = "MyTestProcedure001122";
			CreateOrAlterProcedureWithEncryption("dbo", procedureName);

			var testCreator = new ViewAndRoutineCreatorForTesting(TestConnection.CurrentDatabase, TestConnection);

			// Act
			testCreator.Run();

			// Assert
			Assert(!testCreator.ObjectExists(procedureName));
		}

		public void TestCompareViewsAndRoutinesNoExceptionIfStoredProcedureInExpectedListIsEncrypted()
		{
			// Arrange
			var testCreator = new ViewAndRoutineCreatorForTesting(TestConnection.CurrentDatabase, TestConnection);
			var expectedList = testCreator.GetViewAndRoutineScriptCollection_Exposed();

			var storedProcedure = expectedList.First((x) => x.ObjectType == DbRoutineType.SqlProcedureTypeDesc);
			CreateOrAlterProcedureWithEncryption(storedProcedure.SchemaName, storedProcedure.Name);

			// Act
			testCreator.Run();

			// Assert
			Assert(storedProcedure.Text.Equals(GetScriptDefinitionFromDb(storedProcedure.SchemaName, storedProcedure.Name)));
		}

		void CreateOrAlterProcedureWithEncryption(string schemaName, string storedProcedureName)
		{
			string sqlText = $@"
					CREATE OR ALTER PROCEDURE {schemaName.QuoteName()}.{storedProcedureName.QuoteName()}
					WITH ENCRYPTION
					AS
					BEGIN
						SELECT 1
					END;
				";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		string GetScriptDefinitionFromDb(string schemaName, string objectName)
		{
			var sqlText = @"
				SELECT
					definition = ISNULL(m.definition, N'')
				FROM
					sys.objects AS obj
					LEFT JOIN sys.sql_modules AS m ON m.object_id = obj.object_id
				WHERE 1=1
					AND obj.schema_id = SCHEMA_ID(@schName)
					AND obj.name = @objName
			";

			return Convert.ToString(TestConnection.ExecuteScalar(sqlText, command =>
			{
				command.AddParameter("@schName", SqlDbType.NVarChar, 128, schemaName);
				command.AddParameter("@objName", SqlDbType.NVarChar, 128, objectName);
			}));
		}
	}
}
