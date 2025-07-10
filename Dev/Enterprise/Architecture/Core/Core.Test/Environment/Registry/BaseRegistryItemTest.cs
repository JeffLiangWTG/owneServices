using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class BaseRegistryItemTest : TransactionedTestCase
	{
		public void TestEachThreadGetsADifferentInstance()
		{
			RegistryDataAccessor instance1 = null;
			RegistryDataAccessor instance2 = null;

			try
			{
				var thread1 = new Thread(() =>
				{
					instance1 = RegistryDataAccessor.DisposableInstance;
				});

				var thread2 = new Thread(() =>
				{
					instance2 = RegistryDataAccessor.DisposableInstance;
				});

				thread1.Start();
				thread2.Start();

				thread1.Join(1000);
				thread2.Join(1000);

				AssertNotNull(instance1);
				AssertNotNull(instance2);
				AssertNotEquals("Each thread should get its own instance", instance1, instance2);
			}
			finally
			{
				instance1?.Dispose();
				instance2?.Dispose();
			}
		}

		public void TestGetNamesOfRegistryItemsReferencingPK()
		{
			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				Guid testGuid = Guid.NewGuid();

				GuidRegistryItem item1 = new GuidRegistryItem("TestItem1", (MultilingualString)null, null, null, RegistryStorageFlags.All);
				GuidRegistryItem item2 = new GuidRegistryItem("TestItem2", (MultilingualString)null, null, null, RegistryStorageFlags.All);

				string[] names = registryDataAccessor.GetNamesOfRegistryItemsReferencingPK(testGuid);
				AssertEquals("Names.Length", 0, names.Length);

				item1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testGuid);
				item2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testGuid);

				names = registryDataAccessor.GetNamesOfRegistryItemsReferencingPK(testGuid);
				AssertEquals("Names.Length", 2, names.Length);
				AssertEquals("Names[0]", "TestItem1", names[0]);
				AssertEquals("Names[1]", "TestItem2", names[1]);
			}
		}

		public void TestGetItemNamesInGroup()
		{
			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				byte[] testValue = Encoding.Unicode.GetBytes("TestValue");
				registryDataAccessor.SetBinaryValue("TestGroupItem1", EnvProxy.Instance.CurrentUser.PK, Guid.Empty, testValue, Guid.Empty, "BIN", false, false);
				registryDataAccessor.SetBinaryValue("TestGroupItem2", EnvProxy.Instance.CurrentUser.PK, Guid.Empty, testValue, Guid.Empty, "BIN", false, false);
				registryDataAccessor.SetBinaryValue("TestGroupItem3", EnvProxy.Instance.CurrentUser.PK, Guid.Empty, testValue, Guid.Empty, "BIN", false, false);

				string[] names = registryDataAccessor.GetItemNamesInGroup("TestGroup", Guid.Empty);
				AssertEquals("Names.Length", 0, names.Length);

				names = registryDataAccessor.GetItemNamesInGroup("TestGroup", EnvProxy.Instance.CurrentUser.PK);
				AssertEquals("Names.Length", 3, names.Length);
				AssertEquals("Names[0]", "Item1", names[0]);
				AssertEquals("Names[1]", "Item2", names[1]);
				AssertEquals("Names[2]", "Item3", names[2]);
			}
		}

		public void TestDeleteRecord()
		{
			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				var recordName = Guid.NewGuid().ToString();
				var testValue = Encoding.Unicode.GetBytes("TestValue");

				registryDataAccessor.SetBinaryValue(recordName, EnvProxy.Instance.CurrentUser.PK, Guid.Empty, testValue, Guid.Empty, "STR", false, false);
				AssertEquals("TestValue", Encoding.Unicode.GetString(registryDataAccessor.GetBinaryValue(recordName, EnvProxy.Instance.CurrentUser.PK, Guid.Empty)));

				registryDataAccessor.DeleteRecord(recordName, EnvProxy.Instance.CurrentUser.PK, Guid.Empty);
				AssertEquals(null, registryDataAccessor.GetBinaryValue(recordName, EnvProxy.Instance.CurrentUser.PK, Guid.Empty));

				registryDataAccessor.DeleteRecord(recordName, EnvProxy.Instance.CurrentUser.PK, Guid.Empty);
				AssertEquals(null, registryDataAccessor.GetBinaryValue(recordName, EnvProxy.Instance.CurrentUser.PK, Guid.Empty));
			}
		}

		public void TestBinaryValue()
		{
			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				// retrieving it
				byte[] retrievedValue1 = registryDataAccessor.GetBinaryValue("TestBinary", Guid.Empty, Guid.Empty);
				AssertNull("BinaryValue", retrievedValue1);

				// setting it
				byte[] testValue = Encoding.Unicode.GetBytes("TestValue");
				registryDataAccessor.SetBinaryValue("TestBinary", Guid.Empty, Guid.Empty, testValue, Guid.Empty, "BIN", false, false);

				// retrieving it
				byte[] retrievedValue2 = registryDataAccessor.GetBinaryValue("TestBinary", Guid.Empty, Guid.Empty);
				AssertEquals("BinaryValue", "TestValue", Encoding.Unicode.GetString(retrievedValue2));

				// deleting it
				registryDataAccessor.SetBinaryValue("TestBinary", Guid.Empty, Guid.Empty, null, Guid.Empty, "BIN", false, false);
				byte[] retrievedValue3 = registryDataAccessor.GetBinaryValue("TestBinary", Guid.Empty, Guid.Empty);
				AssertNull("BinaryValue", retrievedValue3);
			}
		}

		public void TestGetBinaryValue_Fallback()
		{
			using (var command = TestConnection.Command("DROP PROCEDURE [dbo].[DataRegGetValueNOD]"))
			{
				command.ExecuteNonQuery();
			}

			var expectedQuery =
				$"""
				SELECT SD_BinaryValue FROM [{Db.DatabaseName}].[dbo].[StmData] WHERE SD_Name = @Name
				Params
				@Name: 'TestBinary'
				
				""";

			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			using (TestConnection.TrackExecutedCommands())
			{
				registryDataAccessor.SetBinaryValue("TestBinary", Guid.Empty, Guid.Empty, Encoding.Unicode.GetBytes("TestValue"), Guid.Empty, "BIN", false, false);
				var actualValue = registryDataAccessor.GetBinaryValue("TestBinary", Guid.Empty, Guid.Empty);

				CombineAssertions(() =>
				{
					AssertEquals("BinaryValue", "TestValue", Encoding.Unicode.GetString(actualValue));
					AssertCollectionContains("Database schema should be defined", expectedQuery, TestConnection.ExecutedCommands);
				});
			}
		}

		public void TestSetBinaryValueWithEmptyGuid()
		{
			var registryName = "Test_" + Guid.NewGuid();
			var selectGuidSql = $"select {StmDataSchema.Constants.SD_GuidValue} from {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName} " +
								$"where {StmDataSchema.Constants.SD_Name} = @name";

			using (var command = TestConnection.Command(selectGuidSql))
			{
				command.AddParameterBasedOnDbColumn("@name", registryName, StmDataSchema.SD_Name);
				using (var reader = command.ExecuteReader())
				{
					Assert("Should not have any records in db", !reader.Read());
				}
			}

			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				registryDataAccessor.SetBinaryValue(registryName, Guid.Empty, Guid.Empty, Encoding.Unicode.GetBytes("TestValue"), Guid.Empty, "BIN", false, false);

				using (var command = TestConnection.Command(selectGuidSql))
				{
					command.AddParameterBasedOnDbColumn("@name", registryName, StmDataSchema.SD_Name);
					using (var reader = command.ExecuteReader())
					{
						var count = 0;
						while (reader.Read())
						{
							var guidValue = reader[StmDataSchema.Constants.SD_GuidValue];
							AssertEquals("SD_GuidValue should be NULL", DBNull.Value, guidValue);

							count++;
						}
						AssertEquals("Should have 1 record in db", 1, count);
					}
				}

				var newGuidValue = Guid.NewGuid();
				registryDataAccessor.SetBinaryValue(registryName, Guid.Empty, Guid.Empty, Encoding.Unicode.GetBytes("TestValue"), newGuidValue, "BIN", false, false);

				using (var command = TestConnection.Command(selectGuidSql))
				{
					command.AddParameterBasedOnDbColumn("@name", registryName, StmDataSchema.SD_Name);
					using (var reader = command.ExecuteReader())
					{
						var count = 0;
						while (reader.Read())
						{
							var guidValue = reader[StmDataSchema.Constants.SD_GuidValue];
							AssertEquals("SD_GuidValue should have our guid", guidValue, guidValue);

							count++;
						}
						AssertEquals("Should have 1 record in db", 1, count);
					}
				}
			}
		}

		public void TestSetBinaryValueWithAuditDetails()
		{
			var sql = $@"SELECT COUNT(*) FROM {StmDataSchema.Constants.SqlSchemaName}.{StmDataSchema.Constants.TableName}
WHERE {StmDataSchema.Constants.SD_Name} = @name
	AND {StmDataSchema.Constants.SD_SystemCreateTimeUtc} IS NOT NULL
	AND {StmDataSchema.Constants.SD_SystemLastEditTimeUtc} IS NOT NULL
	AND {StmDataSchema.Constants.SD_SystemCreateUser} <> ''
	AND {StmDataSchema.Constants.SD_SystemLastEditUser} <> ''";

			var environmentMock = new Mock<IEnvironment>();
			var envMock = new Mock<IEnv>();
			var userMock = new Mock<IUser>();
			userMock.Setup(x => x.Initials).Returns("~BP");
			environmentMock.Setup(x => x.CurrentUser).Returns(userMock.Object);
			envMock.Setup(x => x.Instance).Returns(environmentMock.Object);

			using (EnvProxy.SetTemporaryEnvForTest(envMock.Object))
			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				byte[] retrievedValue1 = registryDataAccessor.GetBinaryValue("TestBinary", Guid.Empty, Guid.Empty);
				AssertNull("BinaryValue", retrievedValue1);

				byte[] testValue = Encoding.Unicode.GetBytes("TestValue");
				registryDataAccessor.SetBinaryValue("TestBinary", Guid.Empty, Guid.Empty, testValue, Guid.Empty, "BIN", false, false);

				using (var command = TestConnection.Command(sql))
				{
					command.AddParameterBasedOnDbColumn("@name", "TestBinary", StmDataSchema.SD_Name);
					AssertEquals(1, (int)command.ExecuteScalar());
				}
			}
		}

		public void TestGetAuditColumnValue()
		{
			var ownerGuid = Guid.NewGuid();
			var departmentGuid = Guid.NewGuid();
			AssertGetAuditColumnValue(Guid.Empty, Guid.Empty, new DateTime(2022, 1, 1), new DateTime(2022, 2, 2));
			AssertGetAuditColumnValue(Guid.Empty, departmentGuid, new DateTime(2022, 3, 3), new DateTime(2022, 4, 4));
			AssertGetAuditColumnValue(ownerGuid, Guid.Empty, new DateTime(2022, 5, 5), new DateTime(2022, 6, 6));
			AssertGetAuditColumnValue(ownerGuid, departmentGuid, new DateTime(2022, 7, 7), new DateTime(2022, 8, 8));
		}

		void AssertGetAuditColumnValue(Guid owner, Guid department, DateTime createDateTime, DateTime lastEditDateTime)
		{
			var sql = @"INSERT dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue, SD_SystemCreateTimeUtc, SD_SystemCreateUser, SD_SystemLastEditTimeUtc, SD_SystemLastEditUser) VALUES
(NEWID(), 'TestReg', @Owner, @Department, 'BIN', @Value, @SystemCreateTimeUtc, '~BP', @SystemLastEditTimeUtc, 'BP~')";

			using (var cmd = TestConnection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@Value", Encoding.Unicode.GetBytes("TEST"), StmDataSchema.SD_BinaryValue);
				cmd.AddParameterBasedOnDbColumn("@Owner", owner == Guid.Empty ? DBNull.Value : owner, StmDataSchema.SD_Owner);
				cmd.AddParameterBasedOnDbColumn("@Department", department == Guid.Empty ? DBNull.Value : department, StmDataSchema.SD_DepartmentGuid);
				cmd.AddParameterBasedOnDbColumn("@SystemCreateTimeUtc", createDateTime, StmDataSchema.SD_SystemCreateTimeUtc);
				cmd.AddParameterBasedOnDbColumn("@SystemLastEditTimeUtc", lastEditDateTime, StmDataSchema.SD_SystemLastEditTimeUtc);
				cmd.ExecuteNonQuery();
			}

			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				AssertEquals("SystemCreateDateTimeUtc", createDateTime, registryDataAccessor.GetAuditColumnValue("TestReg", "SD_SystemCreateTimeUtc", owner, department));
				AssertEquals("SystemCreateUser", "~BP", registryDataAccessor.GetAuditColumnValue("TestReg", "SD_SystemCreateUser", owner, department));
				AssertEquals("SystemLastEditTimeUtc", lastEditDateTime, registryDataAccessor.GetAuditColumnValue("TestReg", "SD_SystemLastEditTimeUtc", owner, department));
				AssertEquals("SystemLastEditUser", "BP~", registryDataAccessor.GetAuditColumnValue("TestReg", "SD_SystemLastEditUser", owner, department));
			}
		}

		public void TestGetBinaryValueFallBack()
		{
			Guid companyPK = Guid.NewGuid();
			Guid departmentPK = Guid.NewGuid();
			Guid branchPK = Guid.NewGuid();

			SetBinaryValue(Guid.Empty, Guid.Empty, "SystemWide");
			AssertEquals("SystemWide", "SystemWide", GetBinaryValueFallBack(companyPK, branchPK, departmentPK));

			SetBinaryValue(Guid.Empty, departmentPK, "SystemDepartmentWide");
			AssertEquals("SystemDepartmentWide", "SystemDepartmentWide", GetBinaryValueFallBack(companyPK, branchPK, departmentPK));

			SetBinaryValue(companyPK, Guid.Empty, "CompanyWide");
			AssertEquals("CompanyWide", "CompanyWide", GetBinaryValueFallBack(companyPK, branchPK, departmentPK));

			SetBinaryValue(companyPK, departmentPK, "CompanyDepartmentWide");
			AssertEquals("CompanyDepartmentWide", "CompanyDepartmentWide", GetBinaryValueFallBack(companyPK, branchPK, departmentPK));

			SetBinaryValue(branchPK, Guid.Empty, "BranchWide");
			AssertEquals("BranchWide", "BranchWide", GetBinaryValueFallBack(companyPK, branchPK, departmentPK));

			SetBinaryValue(branchPK, departmentPK, "BranchDepartmentWide");
			AssertEquals("BranchDepartmentWide", "BranchDepartmentWide", GetBinaryValueFallBack(companyPK, branchPK, departmentPK));
		}

		public void TestIsPKReferencedByRegistry()
		{
			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			using (var anotherRegistryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				var testPK = Guid.NewGuid();
				AssertEquals("IsPKReferencedByRegistry", false, registryDataAccessor.IsPKReferencedByRegistry(testPK));

				var testPKBlob = Encoding.Unicode.GetBytes(testPK.ToString());
				anotherRegistryDataAccessor.SetBinaryValue("TestPK", Guid.Empty, Guid.Empty, testPKBlob, testPK, "BIN", false, false);

				AssertEquals("IsPKReferencedByRegistry", true, registryDataAccessor.IsPKReferencedByRegistry(testPK));

				AssertEquals("Empty Guid", false, registryDataAccessor.IsPKReferencedByRegistry(Guid.Empty));
			}
		}

		public void TestSystemLevelValueReturnsProperly()
		{
			Guid companyPK = Guid.NewGuid();
			Guid departmentPK = Guid.NewGuid();

			byte[] companyBlob = System.Text.Encoding.ASCII.GetBytes("Company");
			byte[] departmentBlob = System.Text.Encoding.ASCII.GetBytes("Department");
			const string RegistryDataAccessorName = "RegistryDataAccessor";

			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				registryDataAccessor.SetBinaryValue(RegistryDataAccessorName, Guid.Empty, Guid.Empty, null, Guid.Empty, "BIN", true, false);
				registryDataAccessor.SetBinaryValue(RegistryDataAccessorName, companyPK, Guid.Empty, companyBlob, Guid.Empty, "BIN", true, false);
				registryDataAccessor.SetBinaryValue(RegistryDataAccessorName, Guid.Empty, departmentPK, departmentBlob, Guid.Empty, "BIN", true, false);

				AssertEquals("GetBinaryValue at system level", null, registryDataAccessor.GetBinaryValue(RegistryDataAccessorName, Guid.Empty, Guid.Empty));
			}
		}

		public void TestGetBinaryExceptionHandlerThrowsExceptionWhenTimeoutExpired()
		{
			var timeoutException = SqlExceptionBuilder.CreateSqlException(
				SqlExceptionBuilder.CreateSqlErrorCollection(
					SqlExceptionBuilder.CreateSqlError(-2, 0, 11, Db.ServerName, "Timeout expired. The timeout period elapsed prior to completion of the operation or the server is not responding.", "", 0)));

			var accessor = new DummyRegistryDataAccessor(Db.Connection);
			using (var command = Db.Connection.Command(""))
			{
				command.AddParameterBasedOnDbColumn("@Name", "RegistryName", StmDataSchema.SD_Name);
				Assert("When GetBinaryValue fails by timeout it must throw the exception to avoid bad decision making", !accessor.HandleSqlExceptionGettingBinaryValue(command, timeoutException).Handled);
			}
		}

		[ExpectExceptionMessage(typeof(System.Data.Common.DbException), "Invalid object name 'dbo.StmData'.")]
		public void TestGetBinaryValueThrowsException()
		{
			DropStmDataTable();
			DummyRegistry.GetBinaryValue("", Guid.Empty, Guid.Empty);
		}

		[ExpectExceptionMessage(typeof(System.Data.Common.DbException), "Invalid object name 'dbo.StmData'.")]
		public void TestGetBinaryValueFallBackThrowsException()
		{
			DropStmDataTable();
			DummyRegistry.GetBinaryValueFallBack("", TimeSpan.Zero, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		[ExpectExceptionMessage(typeof(System.Data.Common.DbException), "Invalid object name 'dbo.StmData'.")]
		public void TestIsPKReferencedByRegistryThrowsException()
		{
			DropStmDataTable();
			DummyRegistry.IsPKReferencedByRegistry(Guid.NewGuid());
		}

		public class NonMainDatabaseTest : TestCase
		{
			[UseSnapshotProtection]
			public void TestGetBinaryValue()
			{
				// Arrange
				CombineAssertions(() =>
				{
					TestGetBinaryValue("RandomDb");
					TestGetBinaryValue("TestDb123");
				});

				void TestGetBinaryValue(string targetDb)
				{
					const string testRegistryItem = "TestBinary";
					var departmentGuid = Guid.NewGuid();
					var ownerGuid = Guid.NewGuid();
					var testBinary = Encoding.Unicode.GetBytes(testRegistryItem);
					using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
					{
						registryDataAccessor.SetBinaryValue(testRegistryItem, ownerGuid, departmentGuid, testBinary, Guid.Empty, "BIN", false, false);
					}

					var errorReporterMock = new Mock<IErrorReporter>();
					using (AdoTestUtils.CreateDbDropExistingDisposable(targetDb, Db.DatabaseName))
					using (((ICurrentDbControl)Db.Connection).UseDatabase(targetDb))
					using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
					using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
					{
						// Act
						var result = registryDataAccessor.GetBinaryValue(testRegistryItem, ownerGuid, departmentGuid);

						// Assert
						var savedValue = Encoding.Unicode.GetString(result);
						AssertEquals(testRegistryItem, savedValue);
						errorReporterMock.Verify(x => x.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
						errorReporterMock.VerifyNoOtherCalls();
					}
				}
			}
		}

		#region Has Value

		public void TestHasValue()
		{
			AssertHasValue(Guid.Empty, Guid.Empty, true);
			AssertHasValue(Guid.Empty, EnvProxy.Instance.CurrentDepartment.PK, true);
			AssertHasValue(Guid.Empty, Guid.NewGuid(), false);
			AssertHasValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, true);
			AssertHasValue(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentDepartment.PK, true);
			AssertHasValue(Guid.NewGuid(), Guid.Empty, false);
			AssertHasValue(EnvProxy.Instance.CurrentCompany.PK, Guid.NewGuid(), false);
			AssertHasValue(EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, true);
			AssertHasValue(EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, true);
			AssertHasValue(EnvProxy.Instance.CurrentBranch.PK, Guid.NewGuid(), false);
		}

		void AssertHasValue(Guid ownerPK, Guid departmentPK, bool expectedHasValue)
		{
			string sqlText = string.Format("DELETE FROM {0} WHERE SD_Name = @name", StmDataSchema.Constants.TableName);

			using (DbCommand command = Db.Connection.Command(sqlText))
			{
				command.AddParameterBasedOnDbColumn("@name", "TestBinary", StmDataSchema.SD_Name);
				command.ExecuteNonQuery();
			}

			SetBinaryValue(ownerPK, departmentPK, "");
			AssertEquals("HasValue()", expectedHasValue, DummyRegistry.HasValue("TestBinary"));
		}

		#endregion

		#region TestClearValueDoesNotDeleteRecord

		public void TestClearValueDoesNotDeleteRecord()
		{
			const string sql = "select Count(*) from dbo.StmData where SD_Name = @name and SD_Owner is null and SD_DepartmentGuid is null";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@name", "RegItem1", StmDataSchema.SD_Name);
				AssertEquals("There should not be records yet", 0, (int)command.ExecuteScalar());
			}

			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				registryDataAccessor.SetBinaryValue("RegItem1", Guid.Empty, Guid.Empty, Encoding.Unicode.GetBytes("abracadabra"), Guid.Empty, "BIN", false, false);
				using (DbCommand command = Db.Connection.Command(sql))
				{
					command.AddParameterBasedOnDbColumn("@name", "RegItem1", StmDataSchema.SD_Name);
					AssertEquals("One record should be added", 1, (int)command.ExecuteScalar());
				}

				registryDataAccessor.SetBinaryValue("RegItem1", Guid.Empty, Guid.Empty, null, Guid.Empty, "BIN", false, false);
				using (DbCommand command = Db.Connection.Command(sql))
				{
					command.AddParameterBasedOnDbColumn("@name", "RegItem1", StmDataSchema.SD_Name);
					AssertEquals("Record should not be deleted when null value is set", 1, (int)command.ExecuteScalar());
				}
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			DummyRegistry = new DummyRegistryDataAccessor(Db.Connection);
		}

		void SetBinaryValue(Guid ownerPK, Guid departmentPK, string testValueString)
		{
			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				byte[] testValue = Encoding.Unicode.GetBytes(testValueString);
				registryDataAccessor.SetBinaryValue("TestBinary", ownerPK, departmentPK, testValue, Guid.Empty, "BIN", false, false);
			}
		}

		string GetBinaryValueFallBack(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				byte[] result = registryDataAccessor.GetBinaryValueFallBack("TestBinary", TimeSpan.Zero, companyPK, branchPK, departmentPK);
				return Encoding.Unicode.GetString(result);
			}
		}

		void DropStmDataTable()
		{
			RunCommandsGeneratedByQuery(Db.Connection, GetSchemaBoundObjectsToDropSql(StmDataSchema.Constants.TableName));
			const string sql = "DROP TABLE " + StmDataSchema.Constants.TableName;
			Db.Connection.ExecuteNonQuery(sql);
		}

		static string GetSchemaBoundObjectsToDropSql(string objectName, string columnName = null)
		{
			return string.Format(@"
WITH
	cte AS
		(
			SELECT
				obj_level = 1,
				obj_id    = d.referencing_id,
				obj_type  = o.type
			FROM
				sys.sql_expression_dependencies AS d
				JOIN sys.objects                AS o ON o.object_id = d.referencing_id
			WHERE 1=1
				AND d.is_schema_bound_reference = 1
				AND d.referenced_id = OBJECT_ID('[{0}]')
				{1}
				AND o.type in ('P', 'V', 'TR', 'FN', 'IF', 'TF')

			UNION ALL

			SELECT
				obj_level = cte.obj_level + 1,
				obj_id    = d.referencing_id,
				obj_type  = o.type
			FROM
				cte
				JOIN sys.sql_expression_dependencies AS d ON d.referenced_id = cte.obj_id
				JOIN sys.objects                     AS o ON o.object_id = d.referencing_id
			WHERE 1=1
				AND d.is_schema_bound_reference = 1
				AND d.referenced_minor_id = 0
				AND o.type in ('P', 'V', 'TR', 'FN', 'IF', 'TF')
		),
	cte_distinct AS
		(
			SELECT
				obj_id, obj_type
				, obj_level =
					CASE obj_type
						WHEN 'TR' THEN 3000000 -- drop triggers first
						ELSE MAX(obj_level)
					END
			FROM
				cte
			GROUP BY
				obj_id, obj_type
		)
SELECT
	drop_stmt =
		CASE obj_type
			WHEN 'P'  THEN 'DROP PROCEDURE' -- SQL Stored Procedure
			WHEN 'V'  THEN 'DROP VIEW'      -- View
			WHEN 'TR' THEN 'DROP TRIGGER'   -- SQL DML trigger
			WHEN 'FN' THEN 'DROP FUNCTION'  -- SQL scalar function
			WHEN 'IF' THEN 'DROP FUNCTION'  -- SQL inline table-valued function
			WHEN 'TF' THEN 'DROP FUNCTION'  -- SQL table-valued-function
		END + ' [' + OBJECT_SCHEMA_NAME(obj_id) + '].[' + OBJECT_NAME(obj_id) + '];'
FROM
	cte_distinct
ORDER BY
	obj_level DESC;
",
				objectName,
				(columnName == null)
					? "AND d.referenced_minor_id = 0"
					: string.Format("AND COL_NAME(d.referenced_id, d.referenced_minor_id) = '{0}'", columnName));
		}

		string lastCommandRunFromQuery = "";
		DummyRegistryDataAccessor DummyRegistry;

		public void RunCommandsGeneratedByQuery(DbConnection conn, string generatingQuery)
		{
			var tableOfCommands = DataUtils.GetDataTableFromQuery(conn, generatingQuery);
			var commands = tableOfCommands.Rows.Cast<DataRow>().Select(r =>
				new KeyValuePair<string, string>((r.Table.Columns.Count > 1 ? r[1].ToString() : null), r[0].ToString())
			);
			RunCollectionOfSqlCommands(conn, commands);
		}

		public void RunCollectionOfSqlCommands(DbConnection conn, IEnumerable<KeyValuePair<string, string>> commandsToRun)
		{
			try
			{
				DoRunCollectionOfSqlCommands(conn, commandsToRun);
			}
			catch (System.Data.Common.DbException ex)
			{
				if (string.IsNullOrEmpty(lastCommandRunFromQuery))
				{
					throw;
				}
				else
				{
					string errorMessage = ex.Message + "\r\nLast executed command: " + lastCommandRunFromQuery;
				}
			}
		}

		/// <summary>
		/// 1/ Receives an enumerable collection of SQL commands to run.
		/// 2/ Runs commands by batches of aprox. 5Kb.
		/// The last executed command is stored in case of a failure to be appended to the error message.
		/// </summary>
		void DoRunCollectionOfSqlCommands(DbConnection conn, IEnumerable<KeyValuePair<string, string>> commandsToRun)
		{
			lastCommandRunFromQuery = "";
			var script = new StringBuilder();

			foreach (var cmdDescriptionAndText in commandsToRun)
			{
				if (!string.IsNullOrEmpty(cmdDescriptionAndText.Value))
				{
					if (string.IsNullOrEmpty(cmdDescriptionAndText.Key))
					{
						script.AppendLine(cmdDescriptionAndText.Value);

						if (script.Length > 5000)
						{
							lastCommandRunFromQuery = script.ToString();
							conn.ExecuteNonQuery(lastCommandRunFromQuery);
							script = new StringBuilder();
						}
					}
					else
					{
						lastCommandRunFromQuery = cmdDescriptionAndText.Value;
						conn.ExecuteNonQuery(lastCommandRunFromQuery);
					}
				}
			}

			if (script.Length > 0)
			{
				lastCommandRunFromQuery = script.ToString();
				conn.ExecuteNonQuery(lastCommandRunFromQuery);
			}

			lastCommandRunFromQuery = "";
		}

		class DummyRegistryDataAccessor : RegistryDataAccessor
		{
			public DummyRegistryDataAccessor(DbConnection connection) : base(null, connection) { }
		}

		#endregion
	}
}
