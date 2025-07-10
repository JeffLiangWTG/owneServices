using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZSaveCommandTest : TransactionedTestCase
	{
		public void TestExecute()
		{
			Guid testPk1 = Guid.NewGuid();
			Guid testPk2 = Guid.NewGuid();
			Guid testPk3 = Guid.NewGuid();

			string sqlText = @"
				INSERT dbo.DummyBizo (Z0_PK, Z0_VarBinaryMax, Z0_VarCharMax, Z0_NVarCharMax) VALUES (@Pk1, 0xABCD, '', N'\u30A3\u30D5')
				INSERT dbo.DummyBizo (Z0_PK, Z0_VarBinaryMax, Z0_VarCharMax, Z0_NVarCharMax) VALUES (@Pk2, 0x123456, 'OLDVALUE', N'')
				INSERT dbo.DummyBizo (Z0_PK, Z0_VarBinaryMax, Z0_VarCharMax, Z0_NVarCharMax) VALUES (@Pk3, null, 'SAMEVALUE', N'\u30D4\u30A1\u30D4\u30A1\u30D4\u30A1')";

			DbCommand testCommand = MainDbConnection.Command(sqlText);
			testCommand.AddParameterBasedOnDbColumn("@Pk1", testPk1, DummyBizoSchema.PK);
			testCommand.AddParameterBasedOnDbColumn("@Pk2", testPk2, DummyBizoSchema.PK);
			testCommand.AddParameterBasedOnDbColumn("@Pk3", testPk3, DummyBizoSchema.PK);

			List<ILargeColumnSaver> testBlobSaverCollection = new List<ILargeColumnSaver>
			{
				// PK 1
				new ZBinarySaver(DummyBizoSchema.Constants.TableName, DummyBizoSchema.Constants.PK, testPk1, DummyBizoSchema.Z0_VarBinaryMax.Name, DummyBizoSchema.Z0_VarBinaryMax.SqlDbType, new ByteArrayStreamSource(new byte[6] { 10, 20, 30, 40, 50, 60 }), false),
				new ZTextSaver(DummyBizoSchema.Constants.TableName, DummyBizoSchema.Constants.PK, testPk1, DummyBizoSchema.Z0_VarCharMax.Name, DummyBizoSchema.Z0_VarCharMax.SqlDbType, new StringReaderSource("ABCDEFG")),
				new ZTextSaver(DummyBizoSchema.Constants.TableName, DummyBizoSchema.Constants.PK, testPk1, DummyBizoSchema.Z0_NVarCharMax.Name, DummyBizoSchema.Z0_NVarCharMax.SqlDbType, new StringReaderSource("\u30C1\u30C2\u30C3")),
				// PK 2
				new ZTextSaver(DummyBizoSchema.Constants.TableName, DummyBizoSchema.Constants.PK, testPk2, DummyBizoSchema.Z0_VarCharMax.Name, DummyBizoSchema.Z0_VarCharMax.SqlDbType, new StringReaderSource("XYZ")),
				// PK 3
				new ZBinarySaver(DummyBizoSchema.Constants.TableName, DummyBizoSchema.Constants.PK, testPk3, DummyBizoSchema.Z0_VarBinaryMax.Name, DummyBizoSchema.Z0_VarBinaryMax.SqlDbType, new ByteArrayStreamSource(new byte[5] { 99, 88, 77, 66, 55 }), false),
				new ZTextSaver(DummyBizoSchema.Constants.TableName, DummyBizoSchema.Constants.PK, testPk3, DummyBizoSchema.Z0_NVarCharMax.Name, DummyBizoSchema.Z0_NVarCharMax.SqlDbType, new StringReaderSource("\u3088\u3077"))
			};

			using (ZSaveCommand testSaveCommand = new ZSaveCommand(testCommand, testBlobSaverCollection))
			{
				AssertEquals("Row 1 should not exist yet", false, DoesDummyBizoRowExist(testPk1));
				AssertEquals("Row 2 should not exist yet", false, DoesDummyBizoRowExist(testPk2));
				AssertEquals("Row 3 should not exist yet", false, DoesDummyBizoRowExist(testPk3));

				int previousExecutedCommandCount = MainDbConnection.ExecutedCommandCount;

				testSaveCommand.Execute();

				int numberOfCommandsExecuted = MainDbConnection.ExecutedCommandCount - previousExecutedCommandCount;
				AssertEquals("Number of commands executed", 7, numberOfCommandsExecuted);

				// ----------
				// -- PK 1 --
				// ----------

				// Z0_VarBinaryMax should have been changed by a ZBlobSaver after inserted
				byte[] blob1 = (byte[])GetDummyBizoData(testPk1, DummyBizoSchema.Constants.Z0_VarBinaryMax);
				AssertEquals("Blob1 Length", 6, blob1.Length);
				AssertEquals("Blob1[0]", (short)10, blob1[0]);
				AssertEquals("Blob1[1]", (short)20, blob1[1]);
				AssertEquals("Blob1[2]", (short)30, blob1[2]);
				AssertEquals("Blob1[3]", (short)40, blob1[3]);
				AssertEquals("Blob1[4]", (short)50, blob1[4]);
				AssertEquals("Blob1[5]", (short)60, blob1[5]);

				// Z0_VarCharMax should have been changed by a ZTextSaver after inserted
				string text1 = (string)GetDummyBizoData(testPk1, DummyBizoSchema.Constants.Z0_VarCharMax);
				AssertEquals("Text1", "ABCDEFG", text1);

				// Z0_NVarCharMax should have been changed by a ZTextSaver after inserted
				string nText1 = (string)GetDummyBizoData(testPk1, DummyBizoSchema.Constants.Z0_NVarCharMax);
				AssertEquals("NText1", "\u30C1\u30C2\u30C3", nText1);

				// ----------
				// -- PK 2 --
				// ----------

				// Z0_VarBinaryMax should NOT have changed - There was no ZBlobSaver to change its value
				byte[] blob2 = (byte[])GetDummyBizoData(testPk2, DummyBizoSchema.Constants.Z0_VarBinaryMax);
				AssertEquals("Blob2 Length", 3, blob2.Length);
				AssertEquals("Blob2[0]", (short)0x12, blob2[0]);
				AssertEquals("Blob2[1]", (short)0x34, blob2[1]);
				AssertEquals("Blob2[2]", (short)0x56, blob2[2]);

				// Z0_VarCharMax should have been changed by a ZTextSaver after inserted
				string text2 = (string)GetDummyBizoData(testPk2, DummyBizoSchema.Constants.Z0_VarCharMax);
				AssertEquals("Text2", "XYZ", text2);

				// Z0_NVarCharMax should NOT have changed - There was no ZTextSaver to change its value
				string nText2 = (string)GetDummyBizoData(testPk2, DummyBizoSchema.Constants.Z0_NVarCharMax);
				AssertEquals("NText2", "", nText2);

				// ----------
				// -- PK 3 --
				// ----------

				// Z0_VarBinaryMax should have been changed by a BlobSaver after inserted
				byte[] blob3 = (byte[])GetDummyBizoData(testPk3, DummyBizoSchema.Constants.Z0_VarBinaryMax);
				AssertEquals("Blob3 Length", 5, blob3.Length);
				AssertEquals("Blob3[0]", (short)99, blob3[0]);
				AssertEquals("Blob3[1]", (short)88, blob3[1]);
				AssertEquals("Blob3[2]", (short)77, blob3[2]);
				AssertEquals("Blob3[3]", (short)66, blob3[3]);
				AssertEquals("Blob3[4]", (short)55, blob3[4]);

				// Z0_VarCharMax should NOT have changed - There was no ZTextSaver to change its value
				string text3 = (string)GetDummyBizoData(testPk3, DummyBizoSchema.Constants.Z0_VarCharMax);
				AssertEquals("Text3", "SAMEVALUE", text3);

				// Z0_NVarCharMax should have been changed by a ZTextSaver after inserted
				string nText3 = (string)GetDummyBizoData(testPk3, DummyBizoSchema.Constants.Z0_NVarCharMax);
				AssertEquals("NText3", "\u3088\u3077", nText3);
			}
		}

		public void TestExecuteWithStandardCommandConcurrencyError()
		{
			var testPk1 = Guid.NewGuid();
			var testPk2 = Guid.NewGuid();
			var testPk3 = Guid.NewGuid();

			var sqlText = @"
			  RAISERROR('{468389FD-0615-45B1-BCC2-1AA0A05F1395,True} ConcurrencyError', 16, 1)";

			var testCommand = MainDbConnection.Command(sqlText);

			testCommand.AddParameterBasedOnDbColumn("@Pk1", testPk1, DummyBizoSchema.PK);
			testCommand.AddParameterBasedOnDbColumn("@Pk2", testPk2, DummyBizoSchema.PK);
			testCommand.AddParameterBasedOnDbColumn("@Pk3", testPk3, DummyBizoSchema.PK);

			using (var testSaveCommand = new ZSaveCommand(testCommand, new List<ILargeColumnSaver>()))
			{
				try
				{
					testSaveCommand.Execute();
					Fail("Should have thrown exception");
				}
				catch (ZSaveCommandException e)
				{
					var innerEx = e.InnerException;
					AssertEquals("(2) Inner Exception is SqlException", true, innerEx is SqlException);
					AssertEquals("(2) Error message", "ConcurrencyError", e.Message);
					AssertEquals("(2) Error PK", new Guid("468389FD-0615-45B1-BCC2-1AA0A05F1395"), e.ErrorPk);
					AssertEquals("(2) IsConcurrencyError", true, e.IsConcurrencyError);
					AssertEquals("(2) IsTriggerException", false, e.IsTriggerException);
				}
			}
		}

		public void TestExecuteWithStandardCommandWithErrorContainingBrackets()
		{
			var testPk1 = Guid.NewGuid();
			var testPk2 = Guid.NewGuid();
			var testPk3 = Guid.NewGuid();

			var sqlText = @"
			  RAISERROR('{ZZZZZ} Some Error', 16, 1)";

			var testCommand = MainDbConnection.Command(sqlText);

			testCommand.AddParameterBasedOnDbColumn("@Pk1", testPk1, DummyBizoSchema.PK);
			testCommand.AddParameterBasedOnDbColumn("@Pk2", testPk2, DummyBizoSchema.PK);
			testCommand.AddParameterBasedOnDbColumn("@Pk3", testPk3, DummyBizoSchema.PK);

			using (var testSaveCommand = new ZSaveCommand(testCommand, new List<ILargeColumnSaver>()))
			{
				try
				{
					testSaveCommand.Execute();
					Fail("Should have thrown exception");
				}
				catch (ZSaveCommandException e)
				{
					var innerEx = e.InnerException;
					AssertEquals("(2) Inner Exception is SqlException", true, innerEx is SqlException);
					AssertEquals("(2) Error message", "{ZZZZZ} Some Error", e.Message);
					AssertEquals("(2) Error PK", Guid.Empty, e.ErrorPk);
					AssertEquals("(2) IsConcurrencyError", false, e.IsConcurrencyError);
					AssertEquals("(2) IsTriggerException", false, e.IsTriggerException);
				}
			}
		}

		public void TestExecuteWith_TriggerLikelyConcurrencyError()
		{
			var testPk1 = Guid.NewGuid();
			var testPk2 = Guid.NewGuid();
			var testPk3 = Guid.NewGuid();

			var sqlText = @"
			  RAISERROR('{1f1f8280-72a2-45da-be79-53dbeb788dc0,false,50000} TriggerLikelyConcurrencyError: Over-pick attempt.', 16, 1)";

			var testCommand = MainDbConnection.Command(sqlText);

			testCommand.AddParameterBasedOnDbColumn("@Pk1", testPk1, DummyBizoSchema.PK);
			testCommand.AddParameterBasedOnDbColumn("@Pk2", testPk2, DummyBizoSchema.PK);
			testCommand.AddParameterBasedOnDbColumn("@Pk3", testPk3, DummyBizoSchema.PK);

			using (var testSaveCommand = new ZSaveCommand(testCommand, new List<ILargeColumnSaver>()))
			{
				try
				{
					testSaveCommand.Execute();
					Fail("Should have thrown exception");
				}
				catch (ZSaveCommandException e)
				{
					var innerEx = e.InnerException;
					AssertEquals("(2) Inner Exception is SqlException", true, innerEx is SqlException);
					AssertEquals("(2) Error message", "Over-pick attempt.", e.Message);
					AssertEquals("(2) Error PK", "1f1f8280-72a2-45da-be79-53dbeb788dc0", e.ErrorPk.ToString());
					AssertEquals("(2) IsConcurrencyError", true, e.IsConcurrencyError);
					AssertEquals("(2) IsTriggerException", true, e.IsTriggerException);
				}
			}
		}

		public void TestExecuteWithLargeColumnSaveError()
		{
			Guid testPk1 = Guid.NewGuid();
			Guid testPk2 = Guid.NewGuid();
			Guid testPk3 = Guid.NewGuid();

			string sqlText = @"
				INSERT dbo.DummyBizo (Z0_PK, Z0_VarBinaryMax) VALUES (@Pk1, 0xABCD)
				INSERT dbo.DummyBizo (Z0_PK, Z0_VarCharMax) VALUES (@Pk2, '')";

			DbCommand testCommand = MainDbConnection.Command(sqlText);

			testCommand.AddParameterBasedOnDbColumn("@Pk1", testPk1, DummyBizoSchema.PK);
			testCommand.AddParameterBasedOnDbColumn("@Pk2", testPk2, DummyBizoSchema.PK);
			testCommand.AddParameterBasedOnDbColumn("@Pk3", testPk3, DummyBizoSchema.PK);

			//TestCommand.AddOutputParameter(ZSaveCommand.CurrentRowPkParamName, SqlDbType.UniqueIdentifier, 0, 0, 0, Guid.Empty);
			//TestCommand.AddOutputParameter(ZSaveCommand.IsConcurrencyParamName, SqlDbType.Char, 1, 0, 0, "Y");

			List<ILargeColumnSaver> testBlobSaverCollection = new List<ILargeColumnSaver>
			{
				new ZBinarySaver(DummyBizoSchema.Constants.TableName, DummyBizoSchema.Constants.PK, testPk1, DummyBizoSchema.Z0_VarBinaryMax.Name, DummyBizoSchema.Z0_VarBinaryMax.SqlDbType, new ByteArrayStreamSource(new byte[1] { 10 }), false),
				new ZTextSaver(DummyBizoSchema.Constants.TableName, DummyBizoSchema.Constants.PK, testPk2, DummyBizoSchema.Z0_VarCharMax.Name, DummyBizoSchema.Z0_VarCharMax.SqlDbType, new StringReaderSource("B")),
				new ZTextSaver(DummyBizoSchema.Constants.TableName, DummyBizoSchema.Constants.PK, testPk3, DummyBizoSchema.Z0_NVarCharMax.Name, DummyBizoSchema.Z0_NVarCharMax.SqlDbType, new StringReaderSource("C"))
			};

			using (ZSaveCommand testSaveCommand = new ZSaveCommand(testCommand, testBlobSaverCollection))
			{
				try
				{
					testSaveCommand.Execute();
					Fail("Should have thrown exception");
				}
				catch (ZSaveCommandException e)
				{
					Exception innerEx = e.InnerException;
					AssertEquals("(2) Error PK", testPk3, e.ErrorPk);
					AssertEquals("(2) IsConcurrencyError", false, e.IsConcurrencyError);
				}
			}
		}

		public void TestCloneFromUnloadedTextBlobFieldDoesntExplode()
		{
			var pk = ZGuid.NewZGuid();
			var insertSqlText = "INSERT dbo.DummyBizo (Z0_PK, Z0_VarCharMax) VALUES (@Pk1, @VarCharMaxText)";
			var testMessage = "You should see this in the error message if it BLOWS. This message needs to be large though... I am not sure what exact length but I've been told it needs to be big, SO.... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... END".PadRight(8001, '>');
			using (var insertCommand = MainDbConnection.Command(insertSqlText))
			{
				insertCommand.AddParameterBasedOnDbColumn("@Pk1", pk.ToGuid(), DummyBizoSchema.PK);
				insertCommand.AddParameterBasedOnDbColumn("@VarCharMaxText", testMessage, DummyBizoSchema.Z0_VarCharMax);
				insertCommand.ExecuteNonQuery();
			}

			var factory = new BusinessObjectFactory();

			var dummyBO1 = factory.Load<DummyBusinessObject>(pk);
			var dummyBO2 = (DummyBusinessObject)dummyBO1.Clone(); // Calls through to CopyPersistentValuesFrom() which hooks up the SqlTextReader to the target BO IF the BO is not loaded yet.

			factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Sanity Check: dummyBO1.Z0_VarCharMax", testMessage, dummyBO1.Z0_VarCharMax);
				AssertEquals("Real Result: dummyBO2.Z0_VarCharMax", testMessage, dummyBO2.Z0_VarCharMax);
			});
		}

		public void TestCloneFromUnloadedBinaryBlobFieldDoesntExplode()
		{
			var pk = ZGuid.NewZGuid();
			var insertSqlText = "INSERT dbo.DummyBizo (Z0_PK, Z0_VarBinaryMax) VALUES (@Pk1, @VarBinaryMaxText)";
			var testMessage = Encoding.UTF8.GetBytes("You should see this in the error message if it BLOWS. This message needs to be large though... I am not sure what exact length but I've been told it needs to be big, SO.... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... BIG bag OF stuff ... END".PadRight(8001, '>'));

			using (var insertCommand = MainDbConnection.Command(insertSqlText))
			{
				insertCommand.AddParameterBasedOnDbColumn("@Pk1", pk.ToGuid(), DummyBizoSchema.PK);
				insertCommand.AddParameterBasedOnDbColumn("@VarBinaryMaxText", testMessage, DummyBizoSchema.Z0_VarBinaryMax);
				insertCommand.ExecuteNonQuery();
			}

			var factory = new BusinessObjectFactory();

			var dummyBO1 = factory.Load<DummyBusinessObject>(pk);
			var dummyBO2 = (DummyBusinessObject)dummyBO1.Clone(); // Calls through to CopyPersistentValuesFrom() which hooks up the SqlTextReader to the target BO IF the BO is not loaded yet.

			factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Sanity Check: dummyBO1.Z0_VarBinaryMax", testMessage, dummyBO1.Z0_VarBinaryMax);
				AssertEquals("Real Result: dummyBO2.Z0_VarBinaryMax", testMessage, dummyBO2.Z0_VarBinaryMax);
			});
		}

		bool DoesDummyBizoRowExist(Guid rowPk)
		{
			using (DbCommand command = MainDbConnection.Command("SELECT count(*) FROM dbo.DummyBizo WHERE Z0_PK = @pk"))
			{
				command.AddParameterBasedOnDbColumn("@pk", rowPk, DummyBizoSchema.PK);
				return ((int)command.ExecuteScalar() == 1);
			}
		}

		object GetDummyBizoData(Guid rowPk, string columnName)
		{
			using (DbCommand command = MainDbConnection.Command(string.Format("SELECT {0} FROM dbo.DummyBizo WHERE Z0_PK = @pk", columnName)))
			{
				command.AddParameterBasedOnDbColumn("@pk", rowPk, DummyBizoSchema.PK);
				return command.ExecuteScalar();
			}
		}

		readonly DbConnection MainDbConnection = Db.Connection; // Required for testing ZArchitecture.DataAccess
	}
}
