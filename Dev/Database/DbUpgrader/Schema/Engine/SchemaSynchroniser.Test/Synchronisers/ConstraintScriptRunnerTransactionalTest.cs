using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class ConstraintScriptRunnerTransactionalTest : TransactionedTestCase
	{
		public void TestEnsureForeignKeyConstraintsAreEnabledAndTrusted()
		{
			// Arrange
			PrepareDisabledConstraintsForTest(TestConnection);
			var runner = new ConstraintScriptRunner(TestConnection, TestConnection.CurrentDatabase, null);

			// Act
			runner.EnsureForeignKeyConstraintsAreEnabledAndTrusted();

			// Assert
			AssertConstraintIsEnabledAndIsTrustedStatus(TestConnection, "TestFk_AB2A60351471460F92CE7C6EA5A9E221_Col1", true, true);
			AssertConstraintIsEnabledAndIsTrustedStatus(TestConnection, "TestFk_AB2A60351471460F92CE7C6EA5A9E221_Col2", true, true);
			AssertConstraintIsEnabledAndIsTrustedStatus(TestConnection, "TestFk_AB2A60351471460F92CE7C6EA5A9E221_Col3", true, true);
		}

		public void TestEnsureCheckConstraintsAreEnabledAndTrusted()
		{
			TestEnsureCheckConstraintsAreEnabledAndTrusted(
				TestConnection,
				expectedIsTrusted: true);
		}

		internal static void TestEnsureCheckConstraintsAreEnabledAndTrusted(
			DbConnection dbConnection,
			bool expectedIsTrusted)
		{
			// Arrange
			PrepareDisabledConstraintsForTest(dbConnection);
			var logger = new Mock<IUpgradeTaskWorkflowLogger>();
			var runner = new ConstraintScriptRunner(dbConnection, dbConnection.CurrentDatabase, null, logger.Object);

			var sequence = new MockSequence();

			if (expectedIsTrusted)
			{
				logger.InSequence(sequence).Setup(logger =>
					logger.ShowInfoMessage("\t(~) ALTER TABLE [dbo].[TestCheck_AB2A60351471460F92CE7C6EA5A9E221] WITH CHECK CHECK CONSTRAINT [CH_TestCheck_AB2A60351471460F92CE7C6EA5A9E221]"));
			}

			logger.InSequence(sequence).Setup(logger =>
					logger.ShowInfoMessage("\t(~) ALTER TABLE [dbo].[TestCheck_AB2A60351471460F92CE7C6EA5A9E221] WITH NOCHECK CHECK CONSTRAINT [CH_TestCheck_1FFD86662112437D9506910B9430943F_NoCheck]"));

			logger.InSequence(sequence).Setup(logger =>
					logger.ShowInfoMessage("\t(~) ALTER TABLE [dbo].[TestCheck_AB2A60351471460F92CE7C6EA5A9E221] WITH CHECK CHECK CONSTRAINT [CH_TestCheck_6905C705967C41518C26523D2B2B20C3]"));

			// Act
			runner.EnsureCheckConstraintsAreEnabled();

			// Assert
			AssertConstraintIsEnabledAndIsTrustedStatus(dbConnection, "CH_TestCheck_AB2A60351471460F92CE7C6EA5A9E221", true, expectedIsTrusted);
			AssertConstraintIsEnabledAndIsTrustedStatus(dbConnection, "CH_TestCheck_6905C705967C41518C26523D2B2B20C3", true, expectedIsTrusted);
			AssertConstraintIsEnabledAndIsTrustedStatus(dbConnection, "CH_TestCheck_37128AA4C5B448FEBE369133B8A0E26C", true, true);

			AssertConstraintIsEnabledAndIsTrustedStatus(dbConnection, "CH_TestCheck_1FFD86662112437D9506910B9430943F_NoCheck", true, false);
			AssertConstraintIsEnabledAndIsTrustedStatus(dbConnection, "CH_TestCheck_8E8BB73158494D77A62E8102CB3EB953_NoCheck", true, false);
			AssertConstraintIsEnabledAndIsTrustedStatus(dbConnection, "CH_TestCheck_4003643A29F3479CA67CF0AD30FAD928_NoCheck", true, true);

			logger.VerifyAll();
			logger.VerifyNoOtherCalls();
		}

		internal static void PrepareDisabledConstraintsForTest(DbConnection dbConnection)
		{
			string sqlText = @"
				CREATE TABLE TestCheck_AB2A60351471460F92CE7C6EA5A9E221
				(
					Col1 UNIQUEIDENTIFIER NOT NULL,
					Col2 UNIQUEIDENTIFIER NOT NULL,
					Col3 UNIQUEIDENTIFIER NOT NULL,
					CONSTRAINT PK_TestCheck_AB2A60351471460F92CE7C6EA5A9E221 PRIMARY KEY NONCLUSTERED (Col1),
					CONSTRAINT CH_TestCheck_AB2A60351471460F92CE7C6EA5A9E221 CHECK (Col1 is not null),
					CONSTRAINT CH_TestCheck_1FFD86662112437D9506910B9430943F_NoCheck CHECK (Col1 is not null),
					CONSTRAINT CH_TestCheck_6905C705967C41518C26523D2B2B20C3 CHECK (Col2 is not null),
					CONSTRAINT CH_TestCheck_8E8BB73158494D77A62E8102CB3EB953_NoCheck CHECK (Col2 is not null),
					CONSTRAINT CH_TestCheck_37128AA4C5B448FEBE369133B8A0E26C CHECK (Col3 is not null),
					CONSTRAINT CH_TestCheck_4003643A29F3479CA67CF0AD30FAD928_NoCheck CHECK (Col3 is not null),
				);
				CREATE TABLE TestFk_AB2A60351471460F92CE7C6EA5A9E221
				(
					Col1 UNIQUEIDENTIFIER NULL,
					Col2 UNIQUEIDENTIFIER NULL,
					Col3 UNIQUEIDENTIFIER NULL,
					CONSTRAINT TestFk_AB2A60351471460F92CE7C6EA5A9E221_Col1 FOREIGN KEY (Col1) REFERENCES TestCheck_AB2A60351471460F92CE7C6EA5A9E221(Col1),
					CONSTRAINT TestFk_AB2A60351471460F92CE7C6EA5A9E221_Col2 FOREIGN KEY (Col2) REFERENCES TestCheck_AB2A60351471460F92CE7C6EA5A9E221(Col1),
					CONSTRAINT TestFk_AB2A60351471460F92CE7C6EA5A9E221_Col3 FOREIGN KEY (Col3) REFERENCES TestCheck_AB2A60351471460F92CE7C6EA5A9E221(Col1),
				);
				ALTER TABLE TestCheck_AB2A60351471460F92CE7C6EA5A9E221 NOCHECK CONSTRAINT CH_TestCheck_AB2A60351471460F92CE7C6EA5A9E221;
				ALTER TABLE TestCheck_AB2A60351471460F92CE7C6EA5A9E221 NOCHECK CONSTRAINT CH_TestCheck_1FFD86662112437D9506910B9430943F_NoCheck;
				ALTER TABLE TestCheck_AB2A60351471460F92CE7C6EA5A9E221 NOCHECK CONSTRAINT CH_TestCheck_6905C705967C41518C26523D2B2B20C3;
				ALTER TABLE TestCheck_AB2A60351471460F92CE7C6EA5A9E221 NOCHECK CONSTRAINT CH_TestCheck_8E8BB73158494D77A62E8102CB3EB953_NoCheck;
				ALTER TABLE TestCheck_AB2A60351471460F92CE7C6EA5A9E221 WITH NOCHECK CHECK CONSTRAINT CH_TestCheck_6905C705967C41518C26523D2B2B20C3;
				ALTER TABLE TestCheck_AB2A60351471460F92CE7C6EA5A9E221 WITH NOCHECK CHECK CONSTRAINT CH_TestCheck_8E8BB73158494D77A62E8102CB3EB953_NoCheck;
				ALTER TABLE TestCheck_AB2A60351471460F92CE7C6EA5A9E221 WITH CHECK CHECK CONSTRAINT CH_TestCheck_37128AA4C5B448FEBE369133B8A0E26C;
				ALTER TABLE TestCheck_AB2A60351471460F92CE7C6EA5A9E221 WITH CHECK CHECK CONSTRAINT CH_TestCheck_4003643A29F3479CA67CF0AD30FAD928_NoCheck;
				ALTER TABLE TestFk_AB2A60351471460F92CE7C6EA5A9E221 NOCHECK CONSTRAINT TestFk_AB2A60351471460F92CE7C6EA5A9E221_Col1;
				ALTER TABLE TestFk_AB2A60351471460F92CE7C6EA5A9E221 NOCHECK CONSTRAINT TestFk_AB2A60351471460F92CE7C6EA5A9E221_Col2;
				ALTER TABLE TestFk_AB2A60351471460F92CE7C6EA5A9E221 CHECK CONSTRAINT TestFk_AB2A60351471460F92CE7C6EA5A9E221_Col2;";
			dbConnection.ExecuteNonQuery(sqlText);

			AssertConstraintIsEnabledAndIsTrustedStatus(dbConnection, "CH_TestCheck_AB2A60351471460F92CE7C6EA5A9E221", false, false);
			AssertConstraintIsEnabledAndIsTrustedStatus(dbConnection, "CH_TestCheck_1FFD86662112437D9506910B9430943F_NoCheck", false, false);
			AssertConstraintIsEnabledAndIsTrustedStatus(dbConnection, "CH_TestCheck_6905C705967C41518C26523D2B2B20C3", true, false);
			AssertConstraintIsEnabledAndIsTrustedStatus(dbConnection, "CH_TestCheck_8E8BB73158494D77A62E8102CB3EB953_NoCheck", true, false);
			AssertConstraintIsEnabledAndIsTrustedStatus(dbConnection, "CH_TestCheck_37128AA4C5B448FEBE369133B8A0E26C", true, true);
			AssertConstraintIsEnabledAndIsTrustedStatus(dbConnection, "CH_TestCheck_4003643A29F3479CA67CF0AD30FAD928_NoCheck", true, true);
			AssertConstraintIsEnabledAndIsTrustedStatus(dbConnection, "TestFk_AB2A60351471460F92CE7C6EA5A9E221_Col1", false, false);
			AssertConstraintIsEnabledAndIsTrustedStatus(dbConnection, "TestFk_AB2A60351471460F92CE7C6EA5A9E221_Col2", true, false);
			AssertConstraintIsEnabledAndIsTrustedStatus(dbConnection, "TestFk_AB2A60351471460F92CE7C6EA5A9E221_Col3", true, true);
		}

		internal static void AssertConstraintIsEnabledAndIsTrustedStatus(DbConnection dbConnection, string constraintName, bool expectedIsEnabled, bool expectedIsTrusted)
		{
			string sqlText = String.Format(@"
				SELECT fk.is_disabled, fk.is_not_trusted
					FROM sys.foreign_keys fk
					WHERE fk.name = '{0}'
				UNION ALL
				SELECT ch.is_disabled, ch.is_not_trusted
					FROM sys.check_constraints ch
					WHERE ch.name = '{0}'",
				constraintName);

			using (var reader = dbConnection.Command(sqlText).ExecuteReader())
			{
				reader.Read();
				bool actualIsEnabled = !reader.GetBoolean(0);
				bool actualIsTrusted = !reader.GetBoolean(1);
				AssertEquals(String.Format("Constraint {0} is enabled?", constraintName), expectedIsEnabled, actualIsEnabled);
				AssertEquals(String.Format("Constraint {0} is trusted?", constraintName), expectedIsTrusted, actualIsTrusted);
			}
		}
	}
}
