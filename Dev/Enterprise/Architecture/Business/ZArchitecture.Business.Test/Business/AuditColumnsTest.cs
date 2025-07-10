using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Environment;
using Moq;
using DummyLoggedSchema = Enterprise.ZArchitecture.Business.Testing.AutoDummyLogged.Schema;

namespace Enterprise.ZArchitecture.Business.Testing.Business
{
	sealed class AuditColumnsTest : TestCaseWithFactory
	{
		public void TestUpdateAuditColumnsGetUpdated()
		{
			// Arrange
			var pk = InsertNewDummyLogged();

			var loaded = Factory.Load<DummyLogged>(pk);
			loaded.ZL2_Description = Guid.NewGuid().ToString();

			// Act
			Factory.Save();

			// Assert
			AssertLastEditTimeAndUserUpdated(pk);
		}

		public void TestAuditTriggersBypassedForSameAuditValues()
		{
			// Arrange
			var pk = InsertNewDummyLogged(lastEditTimeUtc: "GetUtcDate()", lastEditUser: $"'{Env.CurrentUser.Initials}'");

			var loaded = Factory.Load<DummyLogged>(pk);
			loaded.ZL2_Description = Guid.NewGuid().ToString();

			AssertNoExceptionThrown(() =>
			{
				// Act
				Factory.Save();
			});
		}

		public void TestAuditTriggersBypassedForSystemColumnOnlyUpdate()
		{
			// Arrange
			var pk = InsertNewDummyLogged(lastEditUser: $"'{Env.CurrentUser.Initials}'");

			var loaded = Factory.Load<DummyLogged>(pk);
			loaded.ZL2_SystemLastEditTimeUtc = DateTime.UtcNow;

			AssertNoExceptionThrown(() =>
			{
				// Act
				Factory.Save();
			});
		}

		public void TestAuditTriggersBypassedForLightValidation()
		{
			// Arrange
			var pk = InsertNewDummyLogged(lastEditTimeUtc: "NULL", lastEditUser: "''");

			var loaded = Factory.Load<DummyLogged>(pk);
			((ILightValidationInternals)loaded).IsValid = true;

			AssertNoExceptionThrown(() =>
			{
				// Act
				Factory.Save();
			});
		}

		public void TestAuditTriggersBypassedForLargeBinaryUpdate()
		{
			// Arrange
			var pk = InsertNewDummyLogged();

			var loaded = Factory.Load<DummyLogged>(pk);
			loaded.ZL2_VarBinaryMax = new byte[MaxChunkSize + 1];

			AssertNoExceptionThrown(() =>
			{
				// Act
				Factory.Save();
			});
		}

		public void TestAuditTriggersBypassedForStreamSource()
		{
			// Arrange
			var pk = InsertNewDummyLogged();

			var loaded = Factory.Load<DummyLogged>(pk);
			loaded.SetZL2_VarBinaryMaxSource(new ByteArrayStreamSource(new byte[] { 1, 2, 3 }));

			AssertNoExceptionThrown(() =>
			{
				// Act
				Factory.Save();
			});
		}

		public void TestAuditTriggersBypassedForLargeTextUpdate()
		{
			// Arrange
			var pk = InsertNewDummyLogged();

			var loaded = Factory.Load<DummyLogged>(pk);
			loaded.ZL2_DescriptionNVarCharMax = new string('A', MaxChunkSize + 1);

			AssertNoExceptionThrown(() =>
			{
				// Act
				Factory.Save();
			});
		}

		public void TestAuditTriggersBypassedForTextReaderSource()
		{
			// Arrange
			var pk = InsertNewDummyLogged();

			var loaded = Factory.Load<DummyLogged>(pk);
			loaded.SetZL2_DescriptionNVarCharMaxSource(new StringReaderSource("test-data"));

			AssertNoExceptionThrown(() =>
			{
				// Act
				Factory.Save();
			});
		}

		public void TestAuditTriggersNotBypassedForDirectUpdateStatement()
		{
			// Arrange
			var pk = InsertNewDummyLogged();

			AssertExceptionThrown<SqlException>(() =>
			{
				// Act
				Db.Connection.ExecuteNonQuery($"UPDATE {DummyLoggedSchema.TableName} SET {DummyLoggedSchema.ZL2_Description} = '{Guid.NewGuid()}' WHERE {DummyLoggedSchema.PK} = '{pk}'");
			});
		}

		public void TestAuditTriggersNotBypassedForCustomTransactionParticipant()
		{
			// Arrange
			InsertNewDummyLogged();
			Factory.SaveInTransactionActions.Add(new BadUpdateAction());

			AssertExceptionThrown<SqlException>(() =>
			{
				// Act
				Factory.Save();
			});
		}

		static Guid InsertNewDummyLogged(string createTimeUtc = null, string createUser = null, string createBranch = null, string createDepartment = null, string lastEditTimeUtc = null, string lastEditUser = null)
		{
			var pk = Guid.NewGuid();

			Db.Connection.ExecuteNonQuery($"ALTER TABLE {DummyLoggedSchema.TableName} DISABLE TRIGGER ALL");

			Db.Connection.ExecuteNonQuery($@"INSERT INTO {DummyLoggedSchema.TableName}
			({DummyLoggedSchema.PK}, {DummyLoggedSchema.ZL2_SystemCreateTimeUtc}, {DummyLoggedSchema.ZL2_SystemCreateUser}, {DummyLoggedSchema.ZL2_SystemCreateBranch}, {DummyLoggedSchema.ZL2_SystemCreateDepartment}, {DummyLoggedSchema.ZL2_SystemLastEditTimeUtc}, {DummyLoggedSchema.ZL2_SystemLastEditUser})
			VALUES ('{pk}', {createTimeUtc ?? "'2001-01-01'"}, {createUser ?? "'USR'"}, {createBranch ?? "'BRN'"}, {createDepartment ?? "'DPT'"}, {lastEditTimeUtc ?? "'2001-01-01'"}, {lastEditUser ?? "'USR'"})");

			Db.Connection.ExecuteNonQuery($"ALTER TABLE {DummyLoggedSchema.TableName} ENABLE TRIGGER ALL");

			return pk;
		}

		static void AssertLastEditTimeAndUserUpdated(Guid dummyLoggedPk)
		{
			AssertGreaterThanOrEqualTo("LastEditTimeUtc should be updated",
				(DateTime)Db.Connection.ExecuteScalar($"SELECT {DummyLoggedSchema.ZL2_SystemLastEditTimeUtc} FROM {DummyLoggedSchema.TableName} WHERE {DummyLoggedSchema.PK} = '{dummyLoggedPk}'"),
				DateTime.UtcNow.AddMinutes(-1));

			AssertEquals("LastEditTimeUtc should be updated",
				Env.CurrentUser.Initials,
				Db.Connection.ExecuteScalar($"SELECT {DummyLoggedSchema.ZL2_SystemLastEditUser} FROM {DummyLoggedSchema.TableName} WHERE {DummyLoggedSchema.PK} = '{dummyLoggedPk}'"));
		}

		const int MaxChunkSize = 42000;

		class BadUpdateAction : SaveInTransactionAction
		{
			protected override IChangedTableNames SaveInTransaction()
			{
				Db.Connection.ExecuteNonQuery($"UPDATE {DummyLoggedSchema.TableName} SET {DummyLoggedSchema.ZL2_Description} = '{Guid.NewGuid()}'");
				return new ChangedTableNames(new[] { DummyLoggedSchema.TableName });
			}

			protected override bool IsInTransaction => false;

			protected override ITransactionManager BeginTransactionWithManager() => Mock.Of<ITransactionManager>();
		}
	}
}
