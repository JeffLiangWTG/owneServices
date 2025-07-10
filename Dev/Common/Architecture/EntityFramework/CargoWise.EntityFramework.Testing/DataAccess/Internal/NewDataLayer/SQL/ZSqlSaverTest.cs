using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Microsoft.SqlServer.Types;
using Microsoft.Win32;
using Moq;
using NUnit.Framework;
using static CargoWise.EntityFramework.Testing.ZSaverTest;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZSqlSaverTest : TransactionedTestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestSaveSplitsOutput()
		{
			DataSet data = new DataSet();
			ZSqlConnectionInfo connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
			ZSqlLoader loader = new ZSqlLoader(data, connectionInfo, GetNewSchemaResolver());
			DummyZSqlSaver saver = new DummyZSqlSaver(data, GetNewSchemaResolver());
			saver.RowsToPostPerSqlStatementForTest = 2;
			DataTable table = loader.GetTable(DummyBizoSchema.Constants.TableName);

			for (int i = 0; i < 11; i++)
			{
				AddDummyRow1(table);
				table.Rows[i][DummyBizoSchema.Z0_Number.Name] = 1;
			}

			// potentially +1 DB hit to check the Registry
			var useParameters = ObjectFactory.Get<IEntityFrameworkSettings>().ParameterizeInsertAndUpdateStatements;

			int countBefore = Db.Connection.ExecutedCommandCount;
			saver.Save();
			int countAfter = Db.Connection.ExecutedCommandCount;
			AssertEquals("Number of DB hits", 6, countAfter - countBefore);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestSaveSplitsOutputWithBlobLargerThanMaxChunkSize()
		{
			DataSet data = new DataSet();
			ZSqlConnectionInfo connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
			ZSqlLoader loader = new ZSqlLoader(data, connectionInfo, GetNewSchemaResolver());
			DummyZSqlSaver saver = new DummyZSqlSaver(data, GetNewSchemaResolver());
			saver.RowsToPostPerSqlStatementForTest = 2;
			DataTable table = loader.GetTable(DummyBizoSchema.Constants.TableName);

			for (int i = 0; i < 5; i++)
			{
				AddDummyRow1(table);
				table.Rows[i][DummyBizoSchema.Z0_Number.Name] = 1;
			}

			// potentially +1 DB hit to check the Registry
			var useParameters = ObjectFactory.Get<IEntityFrameworkSettings>().ParameterizeInsertAndUpdateStatements;

			int previousMaxChunkSize = ZLargeColumnSaver.MaxChunkSize;
			int countBefore = Db.Connection.ExecutedCommandCount;

			try
			{
				ZLargeColumnSaver.MaxChunkSize = 100;
				saver.Save();
			}
			finally
			{
				ZLargeColumnSaver.MaxChunkSize = previousMaxChunkSize;
			}

			int countAfter = Db.Connection.ExecutedCommandCount;
			AssertEquals("Number of DB hits", 8, countAfter - countBefore);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestUpdateSQLDoesNotIncludeRowsThatHaveChangesThatCancelEachOtherOut()
		{
			DataSet data = new DataSet();
			ZSqlConnectionInfo connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
			ZSqlLoader loader = new ZSqlLoader(data, connectionInfo, GetNewSchemaResolver());
			DummyZSqlSaver saver = new DummyZSqlSaver(data, GetNewSchemaResolver());

			DataTable table = loader.GetTable(DummyBizoSchema.Constants.TableName);

			for (int i = 0; i < 10; i++)
			{
				AddDummyRow1(table);
				table.Rows[i][DummyBizoSchema.Z0_Number.Name] = 1;
			}

			saver.Save();
			AssertEquals("Dummies inserted", 10, GetDummyCountInDB());
			data.AcceptChanges();

			for (int i = 0; i < 10; i++)
			{
				table.Rows[i][DummyBizoSchema.Z0_Number.Name] = 2;
				table.Rows[i][DummyBizoSchema.Z0_Number.Name] = 1; // revert back to original value
			}

			table.Rows[5][DummyBizoSchema.Z0_Number.Name] = 100; // only one with real change

			using (ZSaveCommand command = saver.MakeCommand())
			{
				for (int i = 0; i < 10; i++)
				{
					if (i == 5)
					{
						Assert("Row with change should be updated", command.StandardCommandText.IndexOf(table.Rows[i][DummyBizoSchema.PK.Name].ToString()) != -1);
					}
					else
					{
						Assert("Row with no real change should not be updated", command.StandardCommandText.IndexOf(table.Rows[i][DummyBizoSchema.PK.Name].ToString()) == -1);
					}
				}

				saver.Save();
				CheckDummiesMatchWhenReloaded(data);
			}
		}

		#region SwapUniqueIndex

		void AddUniqueIndexToDummyBizo()
		{
			Db.Connection.ExecuteNonQuery(string.Format("CREATE UNIQUE NONCLUSTERED INDEX [NR_UX_{0}_{1}_{2}_{3}] ON [dbo].[{0}] ([{1}] ASC, [{2}] ASC, [{3}] ASC)", DummyBizoSchema.Constants.TableName, DummyBizoSchema.Z0_Number.Name, DummyBizoSchema.Z0_Date.Name, DummyBizoSchema.Z0_Code.Name));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestUpdateSwapUniqueIndexConstraintColumns()
		{
			AddUniqueIndexToDummyBizo();

			DataSet data = new DataSet();
			ZSqlConnectionInfo connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
			ZSqlLoader loader = new ZSqlLoader(data, connectionInfo, GetNewSchemaResolver());
			DummyZSqlSaver saver = new DummyZSqlSaver(data, GetNewSchemaResolver());

			DataTable table = loader.GetTable(DummyBizoSchema.Constants.TableName);

			for (int i = 0; i < 4; i++)
			{
				AddDummyRow1(table);
				table.Rows[i][DummyBizoSchema.Z0_Number.Name] = i;
			}

			saver.Save();
			AssertEquals("Dummies inserted", 4, GetDummyCountInDB());
			data.AcceptChanges();

			for (int i = 0; i < 2; i++)
			{
				table.Rows[i][DummyBizoSchema.Z0_Number.Name] = 1 - i;
			}
			table.Rows[2][DummyBizoSchema.Z0_Number.Name] = 12;
			table.Rows[3][DummyBizoSchema.Z0_Number.Name] = 13;

			saver.Save();
			CheckDummiesMatchWhenReloaded(data);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestUpdateSwapUniqueIndexConstraintColumnsWithOtherColumnsInOneRowChanged()
		{
			AddUniqueIndexToDummyBizo();

			DataSet data = new DataSet();
			ZSqlConnectionInfo connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
			ZSqlLoader loader = new ZSqlLoader(data, connectionInfo, GetNewSchemaResolver());
			DummyZSqlSaver saver = new DummyZSqlSaver(data, GetNewSchemaResolver());

			DataTable table = loader.GetTable(DummyBizoSchema.Constants.TableName);

			for (int i = 0; i < 2; i++)
			{
				AddDummyRow1(table);
				var flagNum = i;
				table.Rows[i][DummyBizoSchema.Z0_Number.Name] = flagNum;
				table.Rows[i][DummyBizoSchema.Z0_Code.Name] = "C" + flagNum;
			}

			saver.Save();
			AssertEquals("Dummies inserted", 2, GetDummyCountInDB());
			data.AcceptChanges();

			for (int i = 0; i < 2; i++)
			{
				var flagNum = 1 - i;
				table.Rows[i][DummyBizoSchema.Z0_Number.Name] = flagNum;
				table.Rows[i][DummyBizoSchema.Z0_Code.Name] = "C" + flagNum;
			}
			table.Rows[0][DummyBizoSchema.Z0_AnotherNumber.Name] = 99;

			saver.Save();
			CheckDummiesMatchWhenReloaded(data);
		}

		#endregion
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestCopesWithSavesOfLotsOfRows()
		{
			DataSet data = new DataSet();

			for (int i = 0; i < 250; i++)
			{
				AddDummyData(data);
			}

			new ZSqlSaver(data, ConnectionInfo, GetNewSchemaResolver()).Save();
			AssertEquals("Lots of dummies inserted", 500, GetDummyCountInDB());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestRowsToPostPerSqlStatement()
		{
			var max = ObjectFactory.Get<IEntityFrameworkSettings>().RowsToPostPerSqlStatement;
			var data = new DataSet();
			var saver = new DummyZSqlSaver(data, GetNewSchemaResolver());
			AssertEquals(max, saver.RowsToPostPerSqlStatementForTest);

			var newValue = ++EnvProxy.Instance.Registry.ZSqlSaverRowsToPostPerSqlStatement;
			saver = new DummyZSqlSaver(data, GetNewSchemaResolver());
			AssertEquals(newValue, saver.RowsToPostPerSqlStatementForTest);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestRowExistsInDatabaseCore()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var dummy1 = factory.NewWithValidTestData<DummyBusinessObject>();
			factory.Save();
			var dummy2 = factory.NewWithValidTestData<DummyBusinessObject>();

			var sqlSaver = new DummyZSqlSaver(new DataSet(), ConnectionInfo, GetNewSchemaResolver());

			AssertEquals("Should find row in db for dummy1", true, sqlSaver.RowExistsInDatabaseForConcurrencyHandlingForTest(((INeedRow)dummy1).Row));
			AssertEquals("Should not find row in db for dummy2", false, sqlSaver.RowExistsInDatabaseForConcurrencyHandlingForTest(((INeedRow)dummy2).Row));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:Do Not Use DataSet", Justification = "<Pending>")]
		public void TestHasDataSourceColunm()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var dummy1 = factory.NewWithValidTestData<DummyBoWithSource>();
			var saver = new DummyZSqlSaver(new DataSet(), ObjectFactory.Get<IApplicationSchemaResolver>());

			AssertEquals("Should return false for null row", false, saver.HasDataSourceColumnForTest(null));
			AssertEquals("Should return false for no SourceSet", false, saver.HasDataSourceColumnForTest(dummy1.Row));

			using (var m = new MemoryStream(new byte[] { 1, 2, 3 }))
			{
				dummy1.SetSourceForTest(new StreamSource(m), DummyBizoSchema.Z0_VarBinaryMax);
				AssertEquals("If has SourceSet, Should return true", true, saver.HasDataSourceColumnForTest(dummy1.Row));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestParseErrorPkFromExceptionMessage()
		{
			var saverBase = new ZSqlSaverBase(new DataSet(), Db.Connection, ObjectFactory.Get<IApplicationSchemaResolver>());

			var method = saverBase.GetType().GetMethod("ParseErrorPkFromExceptionMessage", BindingFlags.Instance | BindingFlags.NonPublic);

			var pk1 = Guid.NewGuid();
			var pk2 = Guid.NewGuid();
			var pk3 = Guid.NewGuid();
			var pk4 = Guid.NewGuid();

			object[] singlePk = { pk1.ToString() };
			var result = (HashSet<Guid>)method.Invoke(saverBase, singlePk);
			var expected = new HashSet<Guid> { pk1 };
			AssertContainsExactElementsInAnyOrder("single pk can be parsed", expected, result);

			object[] singlePkWithCurlyBrackets = { "{" + pk1.ToString() + "}" };
			result = (HashSet<Guid>)method.Invoke(saverBase, singlePkWithCurlyBrackets);
			expected = new HashSet<Guid> { pk1 };
			AssertContainsExactElementsInAnyOrder("single pk with curly brackets can be parsed", expected, result);

			object[] singlePkWithRoundBrackets = { "(" + pk1.ToString() + ")" };
			result = (HashSet<Guid>)method.Invoke(saverBase, singlePkWithRoundBrackets);
			expected = new HashSet<Guid> { pk1 };
			AssertContainsExactElementsInAnyOrder("single pk with round brackets can be parsed", expected, result);

			object[] multiple2Pks = { string.Join(",", pk1, pk2) };
			result = (HashSet<Guid>)method.Invoke(saverBase, multiple2Pks);
			expected = new HashSet<Guid> { pk1, pk2 };
			AssertContainsExactElementsInAnyOrder("multiple pks can be parsed", expected, result);

			object[] multiple4Pks = { string.Join(",", pk1,  pk2, pk3, pk4)  };
			result = (HashSet<Guid>)method.Invoke(saverBase, multiple4Pks);
			expected = new HashSet<Guid> { pk1, pk2, pk3, pk4 };
			AssertContainsExactElementsInAnyOrder("multiple pks can be parsed", expected, result);

			object[] multiplePksWithCurlyBrackets = { string.Join(",", "{" + pk1 + "}", "{" + pk2 + "}", "{" + pk3 + "}") };
			result = (HashSet<Guid>)method.Invoke(saverBase, multiplePksWithCurlyBrackets);
			expected = new HashSet<Guid> { pk1, pk2, pk3 };
			AssertContainsExactElementsInAnyOrder("multiple pks with curly brackets can be parsed", expected, result);

			object[] multiplePksWithRoundBrackets = { string.Join(",", "(" + pk1 + ")", "(" + pk2 + ")", "(" + pk3 + ")") };
			result = (HashSet<Guid>)method.Invoke(saverBase, multiplePksWithRoundBrackets);
			expected = new HashSet<Guid> { pk1, pk2, pk3 };
			AssertContainsExactElementsInAnyOrder("multiple pks with curly brackets can be parsed", expected, result);

			object[] incorrectPkFormat = { pk1.ToString() + "1" };
			result = (HashSet<Guid>)method.Invoke(saverBase, incorrectPkFormat);
			expected = new HashSet<Guid>();
			AssertContainsExactElementsInAnyOrder("error format string cannot be parsed", expected, result);

			object[] incorrectPkFormat2 = { pk1.ToString() + ",," + pk2.ToString() };
			result = (HashSet<Guid>)method.Invoke(saverBase, incorrectPkFormat2);
			expected = new HashSet<Guid>();
			AssertContainsExactElementsInAnyOrder("error format string cannot be parsed", expected, result);

			object[] incorrectPkFormat3 = { pk1.ToString() + "," };
			result = (HashSet<Guid>)method.Invoke(saverBase, incorrectPkFormat3);
			expected = new HashSet<Guid>();
			AssertContainsExactElementsInAnyOrder("error format string cannot be parsed", expected, result);

			object[] incorrectPkFormat4 = { "," + pk1.ToString() };
			result = (HashSet<Guid>)method.Invoke(saverBase, incorrectPkFormat4);
			expected = new HashSet<Guid>();
			AssertContainsExactElementsInAnyOrder("error format string cannot be parsed", expected, result);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestGetRowsWithErrorFromException()
		{
			var saverBase = new ZSqlSaverBase(new DataSet(), Db.Connection, ObjectFactory.Get<IApplicationSchemaResolver>());

			var method = saverBase.GetType().GetMethod("GetRowsWithErrorFromException", BindingFlags.Instance | BindingFlags.NonPublic);

			var pk1 = Guid.NewGuid();
			var pk2 = Guid.NewGuid();
			var pk3 = Guid.NewGuid();

			var dataRows = new List<DataRow>();
			var table = new DataTable("BlahBlah");
			var col = new DataColumn("PK", typeof(Guid));
			table.Columns.Add(col);
			table.PrimaryKey = new DataColumn[] { col };

			var row1 = table.NewRow();
			row1["PK"] = pk1;
			dataRows.Add(row1);

			var row2 = table.NewRow();
			row2["PK"] = pk2;
			dataRows.Add(row2);

			var row3 = table.NewRow();
			row3["PK"] = pk3;
			dataRows.Add(row3);

			var messageWithMultipleGuid = string.Join(",", pk1, pk2, pk3);

			var multiRowInsertMockPk = "DeadBeef-Add1-Add2-Add3-DeadBeefC0de";
			var exception = new ZSaveCommandException(
				new Exception(messageWithMultipleGuid),
				Guid.Parse(multiRowInsertMockPk),
				false
			);

			var result = (List<DataRow>)method.Invoke(saverBase, new object[] { exception, dataRows, 0 });
			var expected = new List<DataRow> { row1, row2, row3 };
			AssertContainsExactElementsInAnyOrder("guids in the message will be parsed to find error rows when error PK in ZSaveCommandException is MultiRowInsertMockPk", expected, result);

			result = (List<DataRow>)method.Invoke(saverBase, new object[] { exception, dataRows, 1 });
			expected = new List<DataRow> { row2, row3 };
			AssertContainsExactElementsInAnyOrder("guids in the message will be parsed to find error rows when error PK in ZSaveCommandException is MultiRowInsertMockPk", expected, result);

			result = (List<DataRow>)method.Invoke(saverBase, new object[] { exception, dataRows, 2 });
			expected = new List<DataRow> { row3 };
			AssertContainsExactElementsInAnyOrder("guids in the message will be parsed to find error rows when error PK in ZSaveCommandException is MultiRowInsertMockPk", expected, result);

			exception = new ZSaveCommandException(
				new Exception(messageWithMultipleGuid),
				pk2,
				false
			);

			result = (List<DataRow>)method.Invoke(saverBase, new object[] { exception, dataRows, 0 });
			expected = new List<DataRow> { row2 };
			AssertContainsExactElementsInAnyOrder("error pk in ZSaveCommandException will be used to find rows with error when it is not the same with MultiRowInsertMockPk", expected, result);

			exception = new ZSaveCommandException(
				new Exception(messageWithMultipleGuid),
				Guid.Empty,
				false
			);
			result = (List<DataRow>)method.Invoke(saverBase, new object[] { exception, dataRows, 0 });
			expected = new List<DataRow>();
			AssertContainsExactElementsInAnyOrder("return empty row list when error PK in ZSaveCommandException is empty", expected, result);
		}

		#region Performance Tests

		abstract class InsertPerformanceTester : TransactionedTestCase
		{
			public string TimeTakenForEachRun = "";
			public double MinRunTime;

			[DeveloperOnlyTest]
			public void TestSpeed()
			{
				for (LoopIterator = 0; LoopIterator <= NumberOfRunsForPerformanceTesting; LoopIterator++)
				{
					ZSqlDataAccessor accessor = SetupAccessorToSave();
					var speedTime = new Stopwatch();
					accessor.Save();

					if (LoopIterator == 0)
					{
						MinRunTime = speedTime.ElapsedTicks;
						TimeTakenForEachRun += "\r\n" + LoopIterator + " - " + MinRunTime + " (before GC and 30s sleep)";
					}
					else
					{
						var runTime = speedTime.Elapsed;
						MinRunTime = runTime.TotalSeconds < MinRunTime ? runTime.TotalSeconds : MinRunTime;
						TimeTakenForEachRun += "\r\n" + LoopIterator + " - " + runTime;
					}
				}

				AssertPerformance();
			}

			protected void GarbageCollect()
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
			}

			protected int LoopIterator;
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]

			protected virtual ZSqlDataAccessor SetupAccessorToSave()
			{
				return SetupAccessorWithInsertPerformanceDataSet(new DataSet());
			}

			protected virtual double MaxTimeForOneRun
			{
				get { return 0.28; }
			}

			protected virtual string TypeOfOperation
			{
				get { return "Insert"; }
			}

			protected virtual int NumberOfDummiesForPerformanceTesting => 80;
			protected virtual int NumberOfRunsForPerformanceTesting => 15;

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
			protected ZSqlDataAccessor SetupAccessorWithInsertPerformanceDataSet(DataSet data, bool enableBulkCopy = false)
			{
				ZSqlDataAccessor accessor = new ZSqlDataAccessor(data, new ZSqlConnectionInfo(Db.Connection, ""));
				DataTable table = accessor.GetTable(DummyBizoSchema.Constants.TableName);
				for (int i = 0; i < NumberOfDummiesForPerformanceTesting; i++)
				{
					DataRow row = table.NewRow();
					row[DummyBizoSchema.PK.Name] = Guid.NewGuid();
					row[DummyBizoSchema.Z0_Description.Name] = i.ToString();
					row[DummyBizoSchema.Z0_Number.Name] = i;
					row[DummyBizoSchema.Z0_AnotherNumber.Name] = 2;
					row[DummyBizoSchema.Z0_Decimal.Name] = (decimal)i;
					row[DummyBizoSchema.Z0_AnotherDecimal.Name] = (decimal)i % 4;
					row[DummyBizoSchema.Z0_Bool.Name] = false;
					row[DummyBizoSchema.Z0_Short.Name] = (short)101;
					row[DummyBizoSchema.Z0_Byte.Name] = (byte)51;
					row[DummyBizoSchema.Z0_VarBinaryMax.Name] = (byte[])ZBlob.FromAscii(ZString.Replicate('A', 1000));
					row[DummyBizoSchema.Z0_Money.Name] = 0;
					row[DummyBizoSchema.Z0_VarCharMax.Name] = "";
					row[DummyBizoSchema.Z0_NVarCharMax.Name] = "";
					row[DummyBizoSchema.Z0_NVarChar.Name] = "";
					row[DummyBizoSchema.Z0_IsValid.Name] = false;
					table.Rows.Add(row);
				}

				if (enableBulkCopy)
				{
					table.ExtendedProperties[typeof(BulkCopySetting)] = new BulkCopySetting(1, 10000, true);
				}

				return accessor;
			}

			protected double AdjustTimeLimitForProcessor(double limit)
			{
				double result = limit;

				RegistryKey key = Registry.LocalMachine.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0");
				int processorSpeed = (int)key.GetValue("~MHz");

				if (processorSpeed < 2600)
				{
					result = limit * 1.8;
				}

				return result;
			}

			protected double ProcessorAdjustedMaxTimeForOneRun
			{
				get { return AdjustTimeLimitForProcessor(MaxTimeForOneRun); }
			}

			protected void AssertPerformance()
			{
				Assert(TypeOfOperation + " performance is slow.\r\nShortest run time should be less than " + ProcessorAdjustedMaxTimeForOneRun +
						"s but is " + MinRunTime + "s." + "\r\nEach run took: " + TimeTakenForEachRun,
						MinRunTime <= ProcessorAdjustedMaxTimeForOneRun);
			}
		}

		sealed class UpdatePeformanceTester : InsertPerformanceTester
		{
			protected override double MaxTimeForOneRun
			{
				get { return 0.20; }
			}

			protected override string TypeOfOperation
			{
				get { return "Update"; }
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
			protected override ZSqlDataAccessor SetupAccessorToSave()
			{
				DataSet data = new DataSet();
				ZSqlDataAccessor accessor = SetupAccessorWithInsertPerformanceDataSet(data);
				accessor.Save();
				data.AcceptChanges();

				foreach (DataRow row in data.Tables[0].Rows)
				{
					row[DummyBizoSchema.Z0_Description.Name] = new Random().Next(99999).ToString();
				}

				return accessor;
			}
		}

		sealed class DeletePerformanceTester : InsertPerformanceTester
		{
			protected override double MaxTimeForOneRun
			{
				get { return 0.20; }
			}

			protected override string TypeOfOperation
			{
				get { return "Delete"; }
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
			protected override ZSqlDataAccessor SetupAccessorToSave()
			{
				DataSet data = new DataSet();
				ZSqlDataAccessor accessor = SetupAccessorWithInsertPerformanceDataSet(data);
				accessor.Save();
				data.AcceptChanges();

				foreach (DataRow row in data.Tables[0].Rows)
				{
					row.Delete();
				}

				return accessor;
			}
		}

		sealed class BulkInsertPerformanceTester : InsertPerformanceTester
		{
			protected override double MaxTimeForOneRun
			{
				get { return 0.1; }
			}

			protected override string TypeOfOperation
			{
				get { return "Bulk Insert"; }
			}

			protected override int NumberOfDummiesForPerformanceTesting => 10000;

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
			protected override ZSqlDataAccessor SetupAccessorToSave()
			{
				DataSet data = new DataSet();
				ZSqlDataAccessor accessor = SetupAccessorWithInsertPerformanceDataSet(data, true);
				return accessor;
			}
		}

		[DeveloperOnlyTest]
		public void TestUpdateSpeed()
		{
			new UpdatePeformanceTester().TestSpeed();
		}

		[DeveloperOnlyTest]
		public void TestDeleteSpeed()
		{
			new DeletePerformanceTester().TestSpeed();
		}

		[DeveloperOnlyTest]
		public void TestBulkInsertSpeed()
		{
			new BulkInsertPerformanceTester().TestSpeed();
		}

		#endregion

		#region Test Simple Update/Insert/Delete

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestInsert()
		{
			DataSet data = new DataSet();
			AddDummyData(data);

			new ZSqlSaver(data, ConnectionInfo, GetNewSchemaResolver()).Save();
			AssertEquals("Two inserted", 2, GetDummyCountInDB());
			CheckDummiesMatchWhenReloaded(data);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestBulkInsert()
		{
			var data = new DataSet();
			AddDummyData(data, enableBulkCopy: true);
			using (var bulkCopyEventTracker = new SqlBulkCopyEventTracker())
			{
				new ZSqlSaver(data, ConnectionInfo, GetNewSchemaResolver()).Save();

				CombineAssertions("Bulk Insert Sql event should have been recorded", () =>
				{
					Assert(bulkCopyEventTracker.HasBulkCopyEvent("DummyBizo"));
					Assert(bulkCopyEventTracker.HasBulkCopyEvent("DummyDependentBizo"));
				});
			}

			AssertEquals("Two Thousands inserted", 2000, GetDummyCountInDB());
			CheckDummiesMatchWhenReloaded(data);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestUpdate() //a
		{
			DataSet insertData = new DataSet();
			AddDummyData(insertData);
			new ZSqlSaver(insertData, ConnectionInfo, GetNewSchemaResolver()).Save();
			AssertEquals("Two inserted", 2, GetDummyCountInDB());

			DataSet data = new DataSet();
			ZSqlLoader loader = new ZSqlLoader(data, ConnectionInfo, GetNewSchemaResolver());
			ZSqlSaver saver = new ZSqlSaver(data, ConnectionInfo, GetNewSchemaResolver());

			DataTable table = loader.GetTable(DummyBizoSchema.Constants.TableName);
			ZDataQuery[] queries = { new ZDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyBizoSchema.Constants.TableName, OrderFilter) };
			loader.LoadPersistentRowsIntoDataSet(queries);
			AssertEquals("Two loaded", 2, table.Rows.Count);

			table.Rows[0][DummyBizoSchema.Z0_Description.Name] = "BREAKFAST";
			table.Rows[1][DummyBizoSchema.Z0_Description.Name] = "DINNER";

			DataTable dependentTable = loader.GetTable(DummyDependentBizoSchema.Constants.TableName);
			ZDataQuery[] queries2 = { new ZDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyDependentBizoSchema.Constants.TableName, new ZQuery()) };
			loader.LoadPersistentRowsIntoDataSet(queries2);
			dependentTable.Rows[0][DummyDependentBizoSchema.ZD1_Code.Name] = "DRUG";

			AssertEquals("1 loaded", 1, dependentTable.Rows.Count);

			saver.Save();

			CheckDummiesMatchWhenReloaded(data);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestDelete()
		{
			DataSet insertData = new DataSet();
			AddDummyData(insertData);
			new ZSqlSaver(insertData, ConnectionInfo, GetNewSchemaResolver()).Save();
			AssertEquals("Two inserted", 2, GetDummyCountInDB());

			DataSet data = new DataSet();
			ZSqlLoader loader = new ZSqlLoader(data, ConnectionInfo, GetNewSchemaResolver());
			ZSqlSaver saver = new ZSqlSaver(data, ConnectionInfo, GetNewSchemaResolver());

			DataTable dependentTable = loader.GetTable(DummyDependentBizoSchema.Constants.TableName);
			ZDataQuery[] queries = { new ZDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyDependentBizoSchema.Constants.TableName, new ZQuery()) };
			loader.LoadPersistentRowsIntoDataSet(queries);

			DataTable table = loader.GetTable(DummyBizoSchema.Constants.TableName);
			ZDataQuery[] queries2 = { new ZDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyBizoSchema.Constants.TableName, OrderFilter) };
			loader.LoadPersistentRowsIntoDataSet(queries2);

			table.Rows[0].Delete();
			saver.Save();
			AssertEquals("One lonely dummy left", 1, GetDummyCountInDB());

			CheckDummiesMatchWhenReloaded(data);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestSaveStringWithEscapeSequence()
		{
			const string text =
@"aaa\\\\



";

			var data = new DataSet();
			var dummyTable = new ZSqlLoader(data, ConnectionInfo, GetNewSchemaResolver()).GetTable(DummyBizoSchema.Constants.TableName);
			AddDummyRow1(dummyTable)[DummyBizoSchema.Constants.Z0_Description] = text;
			new ZSqlSaver(data, ConnectionInfo, GetNewSchemaResolver()).Save();

			data = new DataSet();
			var loader = new ZSqlLoader(data, ConnectionInfo, GetNewSchemaResolver());
			dummyTable = loader.GetTable(DummyBizoSchema.Constants.TableName);
			loader.LoadPersistentRowsIntoDataSet(new ZDataQuery(ConnectionInfo, DummyBizoSchema.Constants.TableName, new ZQuery()));
			AssertEquals(1, dummyTable.Rows.Count);
			AssertEquals(text, dummyTable.Rows[0][DummyBizoSchema.Constants.Z0_Description]);

			dummyTable.Rows[0][DummyBizoSchema.Constants.Z0_Number] = 1234567890;
			var saver = new ZSqlSaver(data, ConnectionInfo, GetNewSchemaResolver());
			AssertNoExceptionThrown(() => saver.Save());
		}

		#region With Blob Larger Than MaxChunkSize

		public void TestInsertWithBlobLargerThanMaxChunkSize()
		{
			int previousMaxChunkSize = ZLargeColumnSaver.MaxChunkSize;

			try
			{
				ZLargeColumnSaver.MaxChunkSize = 100;
				TestInsert();
			}
			finally
			{
				ZLargeColumnSaver.MaxChunkSize = previousMaxChunkSize;
			}
		}

		public void TestUpdateWithBlobLargerThanMaxChunkSize()
		{
			int previousMaxChunkSize = ZLargeColumnSaver.MaxChunkSize;

			try
			{
				ZLargeColumnSaver.MaxChunkSize = 100;
				TestUpdate();
			}
			finally
			{
				ZLargeColumnSaver.MaxChunkSize = previousMaxChunkSize;
			}
		}

		public void TestDeleteWithBlobLargerThanMaxChunkSize()
		{
			int previousMaxChunkSize = ZLargeColumnSaver.MaxChunkSize;

			try
			{
				ZLargeColumnSaver.MaxChunkSize = 100;
				TestDelete();
			}
			finally
			{
				ZLargeColumnSaver.MaxChunkSize = previousMaxChunkSize;
			}
		}

		#endregion

		#region TestBuildCommandMaximumLength

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestBuildCommandMaximumLength()
		{
			DataSet dataSet = new DataSet();
			ZSqlLoader loader = new ZSqlLoader(dataSet, new ZSqlConnectionInfo(Db.Connection, ""), GetNewSchemaResolver());

			DataTable table = loader.GetTable(DummyBizoSchema.Constants.TableName);
			DataRow row1 = AddDummyRow1(table);
			DataRow row2 = AddDummyRow2(table);

			BuildResult buildResult = new DummyZSqlSaver(dataSet, GetNewSchemaResolver()).BuildCommandExposed(new List<DataRow> { row1 }, 0, 1, 42000);
			int maximumSize = buildResult.Command.StandardCommandText.Length + 10;
			buildResult.Command.Dispose();

			buildResult = new DummyZSqlSaver(dataSet, GetNewSchemaResolver()).BuildCommandExposed(new List<DataRow> { row1, row2 }, 0, 2, maximumSize);
			using (buildResult.Command)
			{
				Assert("Maximul length should not be exceeded", buildResult.Command.StandardCommandText.Length < maximumSize);
				AssertEquals("Only one row should fit into maximum length", 1, buildResult.Rows);
			}
		}

		#endregion

		#region ConcurrencyPolicy

		public void TestUpdateWithConcurrencyPolicy_Z0_NVarCharMax()
		{
			AssertUpdateWithConcurrencyPolicy(DummyBizoSchema.Constants.Z0_NVarCharMax);
		}

		public void TestUpdateWithConcurrencyPolicy_Z0_VarBinaryMax()
		{
			AssertUpdateWithConcurrencyPolicy(DummyBizoSchema.Constants.Z0_VarBinaryMax);
		}

		public void TestUpdateWithConcurrencyPolicy_Z0_VarCharMax()
		{
			AssertUpdateWithConcurrencyPolicy(DummyBizoSchema.Constants.Z0_VarCharMax);
		}

		public void TestUpdateWithConcurrencyPolicy_Z0_Xml()
		{
			AssertUpdateWithConcurrencyPolicy(DummyBizoSchema.Constants.Z0_Xml);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		void AssertUpdateWithConcurrencyPolicy(string columnName)
		{
			var data = new DataSet();

			AddDummyData(data);

			new ZSqlSaver(data, ConnectionInfo, GetNewSchemaResolver()).Save();
			AssertEquals("Two inserted", data.Tables[DummyBizoSchema.Constants.TableName].Rows.Count, GetDummyCountInDB());

			data = new DataSet();
			var loader = new ZSqlLoader(data, ConnectionInfo, GetNewSchemaResolver());
			var saver = new ZSqlSaver(data, ConnectionInfo, GetNewSchemaResolver());

			var table = loader.GetTable(DummyBizoSchema.Constants.TableName);

			ZDataQuery[] queries = { new ZDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyBizoSchema.Constants.TableName, OrderFilter) };
			loader.LoadPersistentRowsIntoDataSet(queries);

			AssertEquals("Two loaded", 2, table.Rows.Count);

			table.Rows[0][DummyBizoSchema.Z0_Description.Name] = "BREAKFAST";
			table.Rows[1][DummyBizoSchema.Z0_Description.Name] = "DINNER";

			table.Rows[0].SetConcurrencyPolicy(columnName, ConcurrencyPolicy.Strict);
			table.Rows[1].SetConcurrencyPolicy(columnName, ConcurrencyPolicy.Strict);

			var dependentTable = loader.GetTable(DummyDependentBizoSchema.Constants.TableName);

			ZDataQuery[] queries2 = { new ZDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyDependentBizoSchema.Constants.TableName, new ZQuery()) };
			loader.LoadPersistentRowsIntoDataSet(queries2);

			dependentTable.Rows[0][DummyDependentBizoSchema.ZD1_Code.Name] = "DRUG";

			AssertEquals("1 loaded", 1, dependentTable.Rows.Count);

			saver.Save();

			CheckDummiesMatchWhenReloaded(data);
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		int GetDummyCountInDB()
		{
			ZSqlConnectionInfo connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
			ZSqlLoader loader = new ZSqlLoader(new DataSet(), connectionInfo, GetNewSchemaResolver());
			return loader.GetCount(new ZCountDataQuery(connectionInfo, DummyBizoSchema.Constants.TableName, new ZQuery()));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		void CheckDummiesMatchWhenReloaded(DataSet data)
		{
			ZSqlLoader reLoader = new ZSqlLoader(new DataSet(), ConnectionInfo, GetNewSchemaResolver());

			ZDataQuery[] queries1 = { new ZDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyBizoSchema.Constants.TableName, OrderFilter) };
			reLoader.LoadPersistentRowsIntoDataSet(queries1);

			ZQuery dummyDependentOrderFilter = new ZQuery() { OrderBy = DummyDependentBizoSchema.ZD1_Number.Name };
			ZDataQuery[] queries2 = { new ZDataQuery(new ZSqlConnectionInfo(Db.Connection, ""), DummyDependentBizoSchema.Constants.TableName, dummyDependentOrderFilter) };
			reLoader.LoadPersistentRowsIntoDataSet(queries2);

			if (data.Tables[DummyBizoSchema.Constants.TableName] != null)
			{
				CheckTablesMatch(data.Tables[DummyBizoSchema.Constants.TableName], reLoader.GetTable(DummyBizoSchema.Constants.TableName));
			}

			if (data.Tables[DummyDependentBizoSchema.Constants.TableName] != null)
			{
				CheckTablesMatch(data.Tables[DummyDependentBizoSchema.Constants.TableName], reLoader.GetTable(DummyDependentBizoSchema.Constants.TableName));
			}
		}

		void CheckTablesMatch(DataTable table, DataTable reLoadedTable)
		{
			int k = 0;
			for (int i = 0; i < table.Rows.Count; i++)
			{
				if (table.Rows[i].RowState != DataRowState.Deleted)
				{
					var reloadedRow = reLoadedTable.Rows.Find(table.Rows[i][0]);
					for (int j = 0; j < table.Columns.Count; j++)
					{
						if (table.Rows[i][j] is byte[])
						{
							AssertEquals("Same value after save & load", (byte[])table.Rows[i][j], (byte[])reloadedRow[j]);
						}
						else if (table.Rows[i][j] is SqlGeography)
						{
							AssertEquals("Same value after save & load", table.Rows[i][j].ToString(), reloadedRow[j].ToString());
						}
						else
						{
							AssertEquals("Same value after save & load", table.Rows[i][j], reloadedRow[j]);
						}
					}
					k++;
				}
			}

			AssertEquals("Reloaded row count matches count of undeleted rows in the original dataset", k, reLoadedTable.Rows.Count);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		void AddDummyData(DataSet data, bool enableBulkCopy = false)
		{
			ZSqlConnectionInfo connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");

			ZSqlLoader loader = new ZSqlLoader(data, connectionInfo, GetNewSchemaResolver());

			DataTable table1 = loader.GetTable(DummyBizoSchema.Constants.TableName);
			AddDummyRow1(table1);
			AddDummyRow2(table1);

			DataTable table2 = loader.GetTable(DummyDependentBizoSchema.Constants.TableName);
			AddDummyDependentRow(table2);

			if (enableBulkCopy)
			{
				var bulkCopySetting = new BulkCopySetting(1, 10000, true);
				table1.ExtendedProperties[typeof(BulkCopySetting)] = bulkCopySetting;
				table2.ExtendedProperties[typeof(BulkCopySetting)] = bulkCopySetting;
				for (int i = 0; i < 999; i++)
				{
					AddDummyRow1(table1);
					AddDummyRow2(table1);
				}
			}
		}

		DataRow AddDummyRow1(DataTable table)
		{
			DataRow row = table.NewRow();
			row[DummyBizoSchema.PK.Name] = Guid.NewGuid();
			row[DummyBizoSchema.Z0_Number.Name] = 1;
			row[DummyBizoSchema.Z0_AnotherNumber.Name] = 2;
			row[DummyBizoSchema.Z0_Decimal.Name] = (decimal)11;
			row[DummyBizoSchema.Z0_AnotherDecimal.Name] = (decimal)21;
			row[DummyBizoSchema.Z0_Bool.Name] = false;
			row[DummyBizoSchema.Z0_Short.Name] = (short)101;
			row[DummyBizoSchema.Z0_Byte.Name] = (byte)51;
			row[DummyBizoSchema.Z0_VarBinaryMax.Name] = (byte[])ZBlob.FromAscii(ZString.Replicate('A', 1000));
			row[DummyBizoSchema.Z0_Description.Name] = "CHICKEN";
			row[DummyBizoSchema.Z0_Xml.Name] = "<Value>\u5e72</Value>";
			row[DummyBizoSchema.Z0_Money.Name] = 0;
			row[DummyBizoSchema.Z0_VarCharMax.Name] = ZString.Replicate('B', 15);
			row[DummyBizoSchema.Z0_NVarCharMax.Name] = ZString.Replicate('\u307F', 15);
			row[DummyBizoSchema.Z0_NVarChar.Name] = "";
			row[DummyBizoSchema.Z0_IsValid.Name] = false;
			row[DummyBizoSchema.Z0_IsSystem.Name] = true;
			table.Rows.Add(row);

			return row;
		}

		DataRow AddDummyRow2(DataTable table)
		{
			DataRow row = table.NewRow();
			row[DummyBizoSchema.PK.Name] = Guid.NewGuid();
			row[DummyBizoSchema.Z0_Number.Name] = 3;
			row[DummyBizoSchema.Z0_AnotherNumber.Name] = 4;
			row[DummyBizoSchema.Z0_Decimal.Name] = (decimal)12;
			row[DummyBizoSchema.Z0_AnotherDecimal.Name] = (decimal)22;
			row[DummyBizoSchema.Z0_Bool.Name] = true;
			row[DummyBizoSchema.Z0_Short.Name] = (short)102;
			row[DummyBizoSchema.Z0_Byte.Name] = (byte)52;
			row[DummyBizoSchema.Z0_VarBinaryMax.Name] = (byte[])ZBlob.FromAscii(ZString.Replicate('B', 1000));
			row[DummyBizoSchema.Z0_Description.Name] = "TONIGHT";
			row[DummyBizoSchema.Z0_Xml.Name] = "<Action>I DANCE</Action>";
			row[DummyBizoSchema.Z0_Money.Name] = 0;
			row[DummyBizoSchema.Z0_VarCharMax.Name] = "";
			row[DummyBizoSchema.Z0_NVarCharMax.Name] = "";
			row[DummyBizoSchema.Z0_NVarChar.Name] = "";
			row[DummyBizoSchema.Z0_IsValid.Name] = false;
			row[DummyBizoSchema.Z0_IsSystem.Name] = true;
			table.Rows.Add(row);

			return row;
		}

		void AddDummyDependentRow(DataTable table)
		{
			DataRow row = table.NewRow();
			row[DummyDependentBizoSchema.PK.Name] = Guid.NewGuid();
			row[DummyDependentBizoSchema.ZD1_Code.Name] = "ABC";
			row[DummyDependentBizoSchema.ZD1_Number.Name] = 235235;
			table.Rows.Add(row);
		}

		ZQuery OrderFilter
		{
			get
			{
				ZQuery orderFilter = new ZQuery();
				orderFilter.OrderBy = DummyBizoSchema.Z0_Description.Name;
				return orderFilter;
			}
		}

		class DummyZSqlSaver : ZSqlSaver
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
			public DummyZSqlSaver(DataSet data, IApplicationSchemaResolver schemaResolver)
				: base(data, new ZSqlConnectionInfo(Db.Connection, ""), schemaResolver)
			{
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
			public DummyZSqlSaver(DataSet data, ZSqlConnectionInfo connectionInfo, IApplicationSchemaResolver schemaResolver)
				: base(data, connectionInfo, schemaResolver)
			{
			}

			public ZSaveCommand MakeCommand()
			{
				var rows = GetModifiedPersistentRowsInSaveOrder(ObjectFactory.Get<IApplicationSchemaResolver>());
				return BuildCommand(rows, 0, rows.Count, 100000, canConsolidateInserts: true).Command;
			}

			public BuildResult BuildCommandExposed(IList<DataRow> rows, int startingIndex, int maximumRows, int maximumBytes, bool canConsolidateInserts = true, bool bulkUpdate = false)
			{
				return BuildCommand(rows, startingIndex, maximumRows, maximumBytes, canConsolidateInserts, bulkUpdate);
			}

			public int RowsToPostPerSqlStatementForTest
			{
				get { return RowsToPostPerSqlStatement; } set { RowsToPostPerSqlStatement = value; }
			}

			public int BytesToPostPerSqlStatementForTest => BytesToPostPerSqlStatement;

			public int MaxRowsPerMultiRowInsertStatementForTest => MaxRowsPerMultiRowInsertStatement;

			public bool RowExistsInDatabaseForConcurrencyHandlingForTest(DataRow row) => RowExistsInDatabaseForConcurrencyHandling(row);

			public bool HasDataSourceColumnForTest(DataRow row) => HasDataSourceColumn(row);
		}

		class ZSqlSaverForTransactionExceptionThrowTest : ZSqlSaver
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
			public ZSqlSaverForTransactionExceptionThrowTest(DataSet data, IApplicationSchemaResolver schemaResolver)
				: base(data, new ZSqlConnectionInfo(Db.Connection, ""), schemaResolver)
			{
			}

			protected override int SortAndResaveRows(IList<DataRow> rows, IList<DataRow> duplicateUniqueIndexKeyRows)
			{
				ShouldThrowTransactionExceptionWhenSaveSplitRows = true;
				return base.SortAndResaveRows(rows, duplicateUniqueIndexKeyRows);
			}

			protected override int SaveSplitRows(IList<DataRow> rows, int startingIndex, bool canConsolidateInserts = true, bool isBulkUpdate = false)
			{
				if (ShouldThrowTransactionExceptionWhenSaveSplitRows)
				{
					throw new ZDataException(new TransactionException("Testing: TransactionException"), rows[0], Db.Connection);
				}

				var sqlException = SqlExceptionBuilder.CreateSqlException(
					SqlExceptionBuilder.CreateSqlErrorCollection(
						SqlExceptionBuilder.CreateSqlError(2601, byte.MaxValue, byte.MinValue, "", "Data failed to save because unique index conflict 'NR_UX__P9L_ParentId_P9L_ParentTableCode_P9L_P9T_TemplateTrigger'.", "", 1)
					));
				throw new ZDataException(sqlException, rows[0], Db.Connection);
			}

			public bool ShouldThrowTransactionExceptionWhenSaveSplitRows;
		}

		#endregion

		#region Test Exceptions Thrown Correctly

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestDbConcurrencyExceptionOnUpdate()
		{
			DataSet data = new DataSet();
			AddDummyData(data);
			ZSqlSaver saver = new ZSqlSaver(data, ConnectionInfo, GetNewSchemaResolver());
			saver.Save();
			data.AcceptChanges();

			DataRow row = data.Tables[DummyBizoSchema.Constants.TableName].Rows[0];
			row[DummyBizoSchema.Z0_Description.Name] = "SaveME!!";

			Db.Connection.ExecuteNonQuery("UPDATE " + row.Table.TableName + " SET " + DummyBizoSchema.Z0_Description.Name + " = 'VIOLATE ME!' "
					+ "WHERE " + DummyBizoSchema.PK.Name + " = @pk",
					cmd => cmd.AddParameterBasedOnDbColumn("@pk", ZDataUtils.GetPK(row), DummyBizoSchema.PK));

			try
			{
				saver.Save();
				Fail("Should have had an exception");
			}
			catch (ZDataConcurrencyException e)
			{
				AssertEquals("Should have got row with error back", row, e.Row);
				AssertEquals("FriendlyMessage should be correct", string.Empty, e.FriendlyMessage);
			}
			catch (Exception e)
			{
				Fail("Should have got ZDataConcurrencyException, not: " + e.ToString());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestTriggerConcurrencyException()
		{
			var data = new DataSet();
			AddDummyData(data);
			var saver = new ZSqlSaver(data, ConnectionInfo, GetNewSchemaResolver());
			saver.Save();
			data.AcceptChanges();

			Db.Connection.ExecuteNonQuery(@$"
CREATE TRIGGER TG_Test
   ON {DummyBizoSchema.Constants.TableName}
   AFTER UPDATE
AS 
BEGIN
	RAISERROR('TriggerLikelyConcurrencyError: Over-pick attempt.', 16, 1)
	ROLLBACK
END
");

			var row = data.Tables[DummyBizoSchema.Constants.TableName].Rows[0];
			row[DummyBizoSchema.Z0_Description.Name] = "SaveME!!";

			try
			{
				saver.Save();
				Fail("Should have had an exception");
			}
			catch (ZDataConcurrencyException e)
			{
				AssertEquals("FriendlyMessage should be correct", "Over-pick attempt.", e.FriendlyMessage);
				AssertEquals("Should have got row with error back", row, e.Row);
			}
			catch (Exception e)
			{
				Fail("Should have got ZDataConcurrencyException, not: " + e.ToString());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestDbReferentialIntegrityViolation()
		{
			DataSet data = new DataSet();
			ZSqlLoader loader = new ZSqlLoader(data, ConnectionInfo, GetNewSchemaResolver());

			DataTable miscServTable = loader.GetTable(OrgMiscServSchema.Constants.TableName);
			ZQuery filter = new ZQuery();
			filter.MaximumRows = 1;
			filter.OrderBy = OrgMiscServSchema.PK.Name;
			loader.LoadPersistentRowsIntoDataSet(new ZDataQuery(ConnectionInfo, OrgMiscServSchema.Constants.TableName, filter));
			AssertEquals("One MiscServ loaded from DB", 1, miscServTable.Rows.Count);

			DataRow row = miscServTable.Rows[0];
			row[OrgMiscServSchema.OM_OH.Name] = Guid.NewGuid(); // integrity violation

			try
			{
				new ZSqlSaver(data, ConnectionInfo, GetNewSchemaResolver()).Save();
				Fail("Should have had an exception");
			}
			catch (ZDataConcurrencyException e)
			{
				Fail("Should have got ZDataException not ZDataConcurrencyException: " + e.ToString());
			}
			catch (ZDataException e)
			{
				AssertEquals("Should have got row with error back", row, e.Row);
			}
			catch
			{
				Fail("Should have got ZDataException");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestReallyBadDbExceptionsAreStillWrappedByZDataExceptionAndGetRowBack()
		{
			var originalResolver = ObjectFactory.Get<IApplicationSchemaResolver>();
			var mockResolver = new Mock<IApplicationSchemaResolver>(MockBehavior.Strict);

			Func<string, string, SchemaColumn> getSchemaColumn = (columnName, tableName) =>
			{
				if (columnName == DummyBizoSchema.PK.Name)
				{
					return DummyBizoSchema.PK;
				}

				if (columnName == DummyBizoSchema.Z0_Code.Name)
				{
					return DummyBizoSchema.Z0_Code;
				}

				if (columnName == DummyDependentBizoSchema.ZD1_Z0.Name)
				{
					return DummyDependentBizoSchema.ZD1_Z0;
				}

				return originalResolver.GetSchemaColumnSafe(columnName, tableName);
			};

			mockResolver.Setup(x => x.GetSchemaColumnSafe(It.IsAny<string>(), It.IsAny<string>())).Returns(getSchemaColumn);
			mockResolver.Setup(x => x.GetSchemaColumn(It.IsAny<string>(), It.IsAny<string>())).Returns(getSchemaColumn);
			mockResolver.Setup(x => x.GetSchemaColumns(It.IsAny<string>())).Returns<string>(tableName =>
			{
				if (tableName.Equals("DummyBizO"))
				{
					tableName = DummyBizoSchema.Constants.TableName;
				}
				if (tableName.Equals("DummyDependentBizO"))
				{
					tableName = DummyDependentBizoSchema.Constants.TableName;
				}
				return originalResolver.GetSchemaColumns(tableName);
			});
			mockResolver.Setup(x => x.GetTableSchemaFromColumnNamePrefix(It.IsAny<string>())).Returns<string>(prefix => originalResolver.GetTableSchemaFromColumnNamePrefix(prefix));
			mockResolver.Setup(x => x.GetPkColumn(It.IsAny<string>())).Returns<string>(tableName => originalResolver.GetPkColumn(tableName));
			mockResolver.Setup(x => x.GetTableSchema(It.IsAny<string>())).Returns<string>(tableName =>
			{
				if (tableName.Contains("DummyBizO"))
				{
					tableName = DummyBizoSchema.Constants.TableName;
				}
				if (tableName.Contains("DummyDependentBizO"))
				{
					tableName = DummyDependentBizoSchema.Constants.TableName;
				}
				return originalResolver.GetTableSchema(tableName);
			});

			using (ObjectFactory.Substitute(mockResolver.Object))
			{
				var data = new DataSet();
				AddDummyData(data);
				var junkTable = new DataTable("DummyBizO");
				var junkRow = junkTable.NewRow();
				junkTable.Columns.Add(DummyBizoSchema.Constants.PK, typeof(Guid));
				junkTable.Columns.Add(DummyBizoSchema.Constants.Z0_Code, typeof(string));
				junkTable.PrimaryKey = new DataColumn[] { junkTable.Columns[DummyBizoSchema.Constants.PK] };
				junkRow[DummyBizoSchema.Z0_Code.Name] = "whatever!"; // does not cause any problems anymore due to truncating the value during parameterization to match the DB column type - varchar(5)
				junkRow[DummyBizoSchema.PK.Name] = Guid.NewGuid();
				junkTable.Rows.Add(junkRow);
				data.Tables.Add(junkTable);

				junkTable = new DataTable("DummyDependentBizO");
				var junkRow2 = junkTable.NewRow();
				junkTable.Columns.Add(DummyDependentBizoSchema.Constants.ZD1_Z0, typeof(Guid));
				junkRow2[DummyDependentBizoSchema.Constants.ZD1_Z0] = Guid.NewGuid(); // wrong parent PK causes exception
				junkTable.Rows.Add(junkRow2);
				data.Tables.Add(junkTable);

				var useParameters = ObjectFactory.Get<IEntityFrameworkSettings>().ParameterizeInsertAndUpdateStatements;

				try
				{
					new ZSqlSaver(data, ConnectionInfo, ObjectFactory.Get<IApplicationSchemaResolver>()).Save();
					Fail("Should have had an exception");
				}
				catch (ZDataConcurrencyException ex)
				{
					Fail("Should have got ZDataException not ZDataConcurrencyException: " + ex.ToString());
				}
				catch (ZDataException ex)
				{
					if (useParameters)
					{
						AssertEquals("Should have got row with error back", junkRow2, ex.Row);
					}
					else
					{
						AssertEquals("Should have got row with error back", junkRow, ex.Row);
					}
				}
				catch (Exception ex)
				{
					Fail("Should have got ZDataException, but got:" + Environment.NewLine + ex.ToString());
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestSqlExceptionWillThrowCorrectly()
		{
			// Arrange
			DataSet data = new DataSet();
			AddDummyData(data);
			var saver = new ZSqlSaverForTransactionExceptionThrowTest(data, GetNewSchemaResolver());

			// Arrange: pre-condition
			saver.ShouldThrowTransactionExceptionWhenSaveSplitRows = false;
			AssertExceptionThrown<ZDataException>(() => saver.Save());
			try
			{
				saver.ShouldThrowTransactionExceptionWhenSaveSplitRows = false;
				// Act
				saver.Save();
			}
			catch (ZDataException e)
			{
				// Assert
				AssertEquals(true, e.InnerException is SqlException && e.InnerException.Message != "Testing: TransactionException");
			}
		}

		#endregion

		#region Test Multi-Row Insert

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestCommandIsSplitToIsolateMultiRowInserts()
		{
			var connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
			var data = new DataSet();
			var loader = new ZSqlLoader(data, connectionInfo, GetNewSchemaResolver());
			var saver = new ZSqlSaver(data, connectionInfo, GetNewSchemaResolver());

			// Setup initial test data
			SetupMultiRowInsertInitialData(loader);

			// potentially +1 DB hit to check the Registry
			var useParameters = ObjectFactory.Get<IEntityFrameworkSettings>().ParameterizeInsertAndUpdateStatements;

			int cmdCountBefore = Db.Connection.ExecutedCommandCount;
			saver.Save();
			int cmdCountAfter = Db.Connection.ExecutedCommandCount;
			data.AcceptChanges();
			AssertEquals("Number of DB commands issued - 1st save", 2, cmdCountAfter - cmdCountBefore);

			// Add, modify and delete test rows
			InsertDeleteUpdateTestRows(data);

			cmdCountBefore = Db.Connection.ExecutedCommandCount;
			saver.Save();
			cmdCountAfter = Db.Connection.ExecutedCommandCount;
			data.AcceptChanges();
			AssertEquals("Number of DB commands issued - 2nd save", 3, cmdCountAfter - cmdCountBefore);

			// Check dataset matches data in the database
			CheckDummiesMatchWhenReloaded(data);
		}

		void SetupMultiRowInsertInitialData(ZSqlLoader loader)
		{
			var dummyBizoTable = loader.GetTable(DummyBizoSchema.Constants.TableName);
			var dummyDependentBizoTable = loader.GetTable(DummyDependentBizoSchema.Constants.TableName);

			// Add 2 DummyBizo rows
			var dummyBizoRow1 = dummyBizoTable.NewRow();
			dummyBizoRow1[DummyBizoSchema.PK.Name] = Guid.NewGuid();
			dummyBizoRow1[DummyBizoSchema.Z0_Number.Name] = 1;
			dummyBizoRow1[DummyBizoSchema.Z0_Description.Name] = "Z01_UNO";
			dummyBizoTable.Rows.Add(dummyBizoRow1);
			var dummyBizoRow2 = dummyBizoTable.NewRow();
			dummyBizoRow2[DummyBizoSchema.PK.Name] = Guid.NewGuid();
			dummyBizoRow2[DummyBizoSchema.Z0_Number.Name] = 2;
			dummyBizoRow2[DummyBizoSchema.Z0_Description.Name] = "Z02_DUE";
			dummyBizoTable.Rows.Add(dummyBizoRow2);

			// Add 1 DummyDependentBizo row
			var dependentRow1 = dummyDependentBizoTable.NewRow();
			dependentRow1[DummyDependentBizoSchema.PK.Name] = Guid.NewGuid();
			dependentRow1[DummyDependentBizoSchema.ZD1_Number.Name] = 1;
			dummyDependentBizoTable.Rows.Add(dependentRow1);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		void InsertDeleteUpdateTestRows(DataSet data)
		{
			var table1 = data.Tables[DummyBizoSchema.Constants.TableName];
			var table2 = data.Tables[DummyDependentBizoSchema.Constants.TableName];

			var row11 = table1.NewRow();
			row11[DummyBizoSchema.PK.Name] = Guid.NewGuid();
			row11[DummyBizoSchema.Z0_Number.Name] = 11;
			row11[DummyBizoSchema.Z0_Description.Name] = "Z11_UNDICI";
			table1.Rows.Add(row11);

			var row12 = table1.NewRow();
			row12[DummyBizoSchema.PK.Name] = Guid.NewGuid();
			row12[DummyBizoSchema.Z0_Number.Name] = 12;
			row12[DummyBizoSchema.Z0_Description.Name] = "Z12_DODICI";
			table1.Rows.Add(row12);

			table1.Rows[1].Delete();

			var row13 = table1.NewRow();
			row13[DummyBizoSchema.PK.Name] = Guid.NewGuid();
			row13[DummyBizoSchema.Z0_Number.Name] = 13;
			row13[DummyBizoSchema.Z0_Description.Name] = "Z13_TREDICI";
			table1.Rows.Add(row13);

			table1.Rows[0][DummyBizoSchema.Z0_Description.Name] = "~SomeNewDesc~";

			var row21 = table2.NewRow();
			row21[DummyDependentBizoSchema.PK.Name] = Guid.NewGuid();
			row21[DummyDependentBizoSchema.ZD1_Number.Name] = 21;
			row21[DummyDependentBizoSchema.ZD1_Z0.Name] = row12[DummyBizoSchema.PK.Name];
			table2.Rows.Add(row21);

			var row22 = table2.NewRow();
			row22[DummyDependentBizoSchema.PK.Name] = Guid.NewGuid();
			row22[DummyDependentBizoSchema.ZD1_Number.Name] = 22;
			row22[DummyDependentBizoSchema.ZD1_Z0.Name] = row13[DummyBizoSchema.PK.Name];
			table2.Rows.Add(row22);

			var row14 = table1.NewRow();
			row14[DummyBizoSchema.PK.Name] = Guid.NewGuid();
			row14[DummyBizoSchema.Z0_Number.Name] = 14;
			row14[DummyBizoSchema.Z0_Description.Name] = "Z14_QUATTORDICI";
			table1.Rows.Add(row14);

			var row15 = table1.NewRow();
			row15[DummyBizoSchema.PK.Name] = Guid.NewGuid();
			row15[DummyBizoSchema.Z0_Number.Name] = 15;
			row15[DummyBizoSchema.Z0_Description.Name] = "Z15_QUINDICI";
			table1.Rows.Add(row15);

			var row16 = table1.NewRow();
			row16[DummyBizoSchema.PK.Name] = Guid.NewGuid();
			row16[DummyBizoSchema.Z0_Number.Name] = 16;
			row16[DummyBizoSchema.Z0_Description.Name] = "Z16_SEDICI";
			table1.Rows.Add(row16);

			var row23 = table2.NewRow();
			row23[DummyDependentBizoSchema.PK.Name] = Guid.NewGuid();
			row23[DummyDependentBizoSchema.ZD1_Number.Name] = 23;
			row23[DummyDependentBizoSchema.ZD1_Z0.Name] = row15[DummyBizoSchema.PK.Name];
			table2.Rows.Add(row23);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestRetriesRowByRowToDetermineErrorPkIfMultiRowInsertFails()
		{
			var connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
			var data = new DataSet();
			var loader = new ZSqlLoader(data, ConnectionInfo, GetNewSchemaResolver());
			var dummyBizoTable = loader.GetTable(DummyBizoSchema.Constants.TableName);
			var dummyDependentBizoTable = loader.GetTable(DummyDependentBizoSchema.Constants.TableName);

			// Add a DummyBizo row
			var dummyBizoRow = dummyBizoTable.NewRow();
			dummyBizoRow[DummyBizoSchema.PK.Name] = Guid.NewGuid();
			dummyBizoTable.Rows.Add(dummyBizoRow);

			// Add 3 DummyDependentBizo rows, the 2nd with an invalid FK reference.
			var dependentRow1 = dummyDependentBizoTable.NewRow();
			dependentRow1[DummyDependentBizoSchema.PK.Name] = Guid.NewGuid();
			dependentRow1[DummyDependentBizoSchema.ZD1_Z0.Name] = dummyBizoTable.Rows[0][DummyBizoSchema.PK.Name];
			dummyDependentBizoTable.Rows.Add(dependentRow1);
			var dependentRow2 = dummyDependentBizoTable.NewRow();
			dependentRow2[DummyDependentBizoSchema.PK.Name] = Guid.NewGuid();
			dependentRow2[DummyDependentBizoSchema.ZD1_Z0.Name] = Guid.NewGuid();
			dummyDependentBizoTable.Rows.Add(dependentRow2);
			var dependentRow3 = dummyDependentBizoTable.NewRow();
			dependentRow3[DummyDependentBizoSchema.PK.Name] = Guid.NewGuid();
			dependentRow3[DummyDependentBizoSchema.ZD1_Z0.Name] = dummyBizoTable.Rows[0][DummyBizoSchema.PK.Name];
			dummyDependentBizoTable.Rows.Add(dependentRow3);

			try
			{
				new ZSqlSaver(data, connectionInfo, GetNewSchemaResolver()).Save();
				Fail("Should throw exception");
			}
			catch (Exception ex)
			{
				string expectedStart = string.Format(
					"Error from Data layer: TableName=DummyDependentBizo, PK={0}, RowState=Added",
					dependentRow2[DummyDependentBizoSchema.PK.Name].ToString());
				AssertEquals("Exception starts with failed row info\r\n" + ex.Message, true, ex.Message.StartsWith(expectedStart));

				string expectedFkText = string.Format("ZD1_Z0 = {0}", dependentRow2[DummyDependentBizoSchema.ZD1_Z0.Name].ToString());
				AssertEquals("Exception contains failed FK\r\n" + ex.Message, true, ex.Message.Contains(expectedFkText));

				var sqlError = new DbErrorMatch((SqlException)ex.InnerException);
				AssertEquals("InnerException type", DbErrorType.InsertConflictedWithForeignKey, sqlError.ExceptionType);
				string fkConstraint = "DummyDependentBizo_ZD1_Z0_FK2_DummyBizo_RRR_120N";
				AssertEquals("InnerException contains restricting FK?\r\n" + ex.InnerException.Message, true, ex.InnerException.Message.Contains(fkConstraint));
			}

			// Check DummyBizo and 1st DummyDependentBizo rows were inserted only.
			var reloader = new ZSqlLoader(new DataSet(), ConnectionInfo, GetNewSchemaResolver());
			var queries = new ZDataQuery[] {
				new ZDataQuery(connectionInfo, DummyBizoSchema.Constants.TableName, new ZQuery()),
				new ZDataQuery(connectionInfo, DummyDependentBizoSchema.Constants.TableName, new ZQuery())
			};
			reloader.LoadPersistentRowsIntoDataSet(queries);

			var reloadedDummyBizo = reloader.GetTable(DummyBizoSchema.Constants.TableName);
			AssertEquals("DummyBizo row inserted", 1, reloadedDummyBizo.Rows.Count);
			AssertEquals("Reloaded DummyBizo PK", dummyBizoRow[DummyBizoSchema.PK.Name], reloadedDummyBizo.Rows[0][DummyBizoSchema.PK.Name]);

			var reloadedDummyDependentBizo = reloader.GetTable(DummyDependentBizoSchema.Constants.TableName);
			AssertEquals("Reloaded DummyDependentBizo row count", 1, reloadedDummyDependentBizo.Rows.Count);
			AssertEquals("Reloaded DummyDependentBizo PK", dependentRow1[DummyDependentBizoSchema.PK.Name], reloadedDummyDependentBizo.Rows[0][DummyDependentBizoSchema.PK.Name]);
		}

		#region Trigger Rolled Back Transaction

		public void TestMultipleInsertsThrowsCorrectExceptionIfTriggerRollsBackTransaction()
		{
			AssertMultipleInsertsThrowsCorrectExceptionIfTriggerRollsBackTransaction();
		}

		[UseSnapshotProtection]
		public void TestMultipleInsertsThroughBulkCopyThrowsCorrectExceptionIfTriggerRollsBackTransaction()
		{
			using (RunNonTransactioned())
			{
				AssertMultipleInsertsThrowsCorrectExceptionIfTriggerRollsBackTransaction(true);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		void AssertMultipleInsertsThrowsCorrectExceptionIfTriggerRollsBackTransaction(bool enableBulkCopy = false)
		{
			string triggerSQL = @"
CREATE TRIGGER TG_DummyBizo on DummyBizo AFTER INSERT
AS
BEGIN
	RAISERROR('Error in the trigger', 16, 1)
	ROLLBACK TRANSACTION
END";
			Db.Connection.ExecuteNonQuery(triggerSQL);

			var connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
			var data = new DataSet();
			var loader = new ZSqlLoader(data, ConnectionInfo, GetNewSchemaResolver());
			var dummyTable = loader.GetTable(DummyBizoSchema.Constants.TableName);

			var row1 = dummyTable.NewRow();
			row1[DummyBizoSchema.PK.Name] = Guid.NewGuid();
			dummyTable.Rows.Add(row1);

			var row2 = dummyTable.NewRow();
			row2[DummyBizoSchema.PK.Name] = Guid.NewGuid();
			dummyTable.Rows.Add(row2);

			if (enableBulkCopy)
			{
				var bulkCopySetting = new BulkCopySetting(1, 10000, true, fireTriggers: true);
				dummyTable.ExtendedProperties[typeof(BulkCopySetting)] = bulkCopySetting;
			}

			var saver = new ZSqlSaver(data, connectionInfo, GetNewSchemaResolver());
			try
			{
				saver.Save();
				Fail("Exception should be thrown");
			}
			catch (ZDataException ex)
			{
				AssertContains("Error in the trigger", ex.InnerException?.Message);
			}
			catch (Exception)
			{
				Fail("Exception should be of type ZDataException.");
			}
		}

		public void TestMultipleInsertsExceptionParsedIfTriggerThrows()
		{
			AssertMultipleInsertsExceptionParsedIfTriggerThrows();
		}

		public void TestBulkInsertsExceptionParsedIfTriggerThrows()
		{
			AssertMultipleInsertsExceptionParsedIfTriggerThrows(true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		void AssertMultipleInsertsExceptionParsedIfTriggerThrows(bool enableBulkCopy = false)
		{
			string triggerSQL = $@"
CREATE TRIGGER TG_DummyBizo on DummyBizo AFTER INSERT
AS
BEGIN
	DECLARE @message VARCHAR(36);
	SELECT @message = CAST(InsertedRateEntry.{DummyBizoSchema.PK.Name} AS CHAR(36)) FROM Inserted AS InsertedRateEntry;
	THROW 58008, @message, 1;
END";
			Db.Connection.ExecuteNonQuery(triggerSQL);

			var connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
			var data = new DataSet();
			var loader = new ZSqlLoader(data, ConnectionInfo, GetNewSchemaResolver());
			var dummyTable = loader.GetTable(DummyBizoSchema.Constants.TableName);

			var row1 = dummyTable.NewRow();
			row1[DummyBizoSchema.PK.Name] = Guid.NewGuid();
			dummyTable.Rows.Add(row1);

			var row2 = dummyTable.NewRow();
			row2[DummyBizoSchema.PK.Name] = Guid.NewGuid();
			dummyTable.Rows.Add(row2);

			if (enableBulkCopy)
			{
				var bulkCopySetting = new BulkCopySetting(1, 10000, true, fireTriggers: true);
				dummyTable.ExtendedProperties[typeof(BulkCopySetting)] = bulkCopySetting;
			}

			var saver = new ZSqlSaver(data, connectionInfo, GetNewSchemaResolver());
			try
			{
				saver.Save();
				Fail("Exception should be thrown");
			}
			catch (ZDataException ex)
			{
				Assert("Exception's message needs to have sensible info", !ex.Message.StartsWith("<ROW IS NULL>"));
			}
			catch (Exception)
			{
				Fail("Exception should be of type either ZDataException");
			}
		}

		#endregion

		#endregion

		#region Test insert-delete rows with complex relation

		public void TestInsertDeleteRowsWithComplexRelation()
		{
			AssertInsertDeleteRowsWithComplexRelation();
		}

		public void TestBulkInsertDeleteRowsWithComplexRelation()
		{
			AssertInsertDeleteRowsWithComplexRelation(true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		void AssertInsertDeleteRowsWithComplexRelation(bool enableBulkCopy = false)
		{
			// Prepare data

			CreateUniqueIndex(DummyBizoSchema.Constants.TableName, DummyBizoSchema.Constants.Z0_Code);

			var dataSet = new DataSet();
			var connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
			var loader = new ZSqlLoader(dataSet, connectionInfo, GetNewSchemaResolver());

			DataTable dummies = loader.GetTable(DummyBizoSchema.Constants.TableName);
			var dummy1 = CreateSimpleDummyRow(dummies, "AAA");
			var dummy2 = CreateSimpleDummyRow(dummies, "BBB");

			DataTable dependents = loader.GetTable(DummyDependentBizoSchema.Constants.TableName);
			var dependent1 = CreateSimpleDependentRow(dependents, "111", (Guid)dummy1[DummyBizoSchema.Constants.PK]);
			var dependent2 = CreateSimpleDependentRow(dependents, "222", (Guid)dummy2[DummyBizoSchema.Constants.PK]);

			if (enableBulkCopy)
			{
				var bulkCopySetting = new BulkCopySetting(1, 10000, true);
				dummies.ExtendedProperties[typeof(BulkCopySetting)] = bulkCopySetting;
				dependents.ExtendedProperties[typeof(BulkCopySetting)] = bulkCopySetting;
			}

			new ZSqlSaver(dataSet, connectionInfo, GetNewSchemaResolver()).Save();
			dataSet.AcceptChanges();

			// Make changes

			var dummy3 = CreateSimpleDummyRow(dummies, "AAA");
			dummy1.Delete();
			dependent1[DummyDependentBizoSchema.Constants.ZD1_Z0] = dummy2[DummyBizoSchema.Constants.PK];
			dependent2[DummyDependentBizoSchema.Constants.ZD1_Z0] = dummy3[DummyBizoSchema.Constants.PK];

			AssertNoExceptionThrown("Should save all changes without errors", () => new ZSqlSaver(dataSet, connectionInfo, GetNewSchemaResolver()).Save());

			// Check result

			var resultData = new DataSet();
			var resultLoader = new ZSqlLoader(resultData, connectionInfo, GetNewSchemaResolver());
			DataTable resultDummies = resultLoader.GetTable(DummyBizoSchema.Constants.TableName);
			DataTable resultDependents = resultLoader.GetTable(DummyDependentBizoSchema.Constants.TableName);
			resultLoader.LoadPersistentRowsIntoDataSet(
				new List<ZDataQuery> {
					new ZDataQuery(connectionInfo, DummyBizoSchema.Constants.TableName, new ZQuery()),
					new ZDataQuery(connectionInfo, DummyDependentBizoSchema.Constants.TableName, new ZQuery())
				});

			AssertEquals(2, resultDummies.Rows.Count);
			AssertTableHasRowWithValues(resultDummies, new[] { new KeyValuePair<string, object>(DummyBizoSchema.Constants.PK, dummy2[DummyBizoSchema.Constants.PK]) });
			AssertTableHasRowWithValues(resultDummies, new[] { new KeyValuePair<string, object>(DummyBizoSchema.Constants.PK, dummy3[DummyBizoSchema.Constants.PK]) });

			AssertEquals(2, resultDependents.Rows.Count);
			AssertTableHasRowWithValues(resultDependents,
				new[]
				{
					new KeyValuePair<string, object>(DummyDependentBizoSchema.Constants.PK, dependent1[DummyDependentBizoSchema.Constants.PK]),
					new KeyValuePair<string, object>(DummyDependentBizoSchema.Constants.ZD1_Z0, dummy2[DummyBizoSchema.Constants.PK])
				});
			AssertTableHasRowWithValues(resultDependents,
				new[]
				{
					new KeyValuePair<string, object>(DummyDependentBizoSchema.Constants.PK, dependent2[DummyDependentBizoSchema.Constants.PK]),
					new KeyValuePair<string, object>(DummyDependentBizoSchema.Constants.ZD1_Z0, dummy3[DummyBizoSchema.Constants.PK])
				});
		}

		void CreateUniqueIndex(string tableName, string columnName)
		{
			Db.Connection.ExecuteNonQuery(string.Format("CREATE UNIQUE NONCLUSTERED INDEX [NR_UX_{0}_{1}] ON [dbo].[{0}] ([{1}] ASC)", tableName, columnName));
		}

		DataRow CreateSimpleDummyRow(DataTable dummies, string code)
		{
			var dummy = dummies.NewRow();
			dummy[DummyBizoSchema.Constants.PK] = Guid.NewGuid();
			dummy[DummyBizoSchema.Constants.Z0_Code] = code;
			dummies.Rows.Add(dummy);
			return dummy;
		}

		DataRow CreateSimpleDependentRow(DataTable dependents, string code, Guid dummyFK)
		{
			var dependent = dependents.NewRow();
			dependent[DummyDependentBizoSchema.Constants.PK] = Guid.NewGuid();
			dependent[DummyDependentBizoSchema.Constants.ZD1_Code] = code;
			dependent[DummyDependentBizoSchema.Constants.ZD1_Z0] = dummyFK;
			dependents.Rows.Add(dependent);
			return dependent;
		}

		void AssertTableHasRowWithValues(DataTable table, IEnumerable<KeyValuePair<string, object>> keyValuePairs)
		{
			Assert(table.Rows.Cast<DataRow>().Any(row => keyValuePairs.All(pair => row[pair.Key].Equals(pair.Value))));
		}

		#endregion

		#region Parameterized Insert and Update

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestParameterization_SimpleInsert()
		{
			var factory = new BusinessObjectFactory();
			var dummy_1 = factory.New<DummyBusinessObject>();
			var dummy_2 = factory.New<DummyBusinessObject>();
			dummy_2.Z0_Geography = new ZGeography("-222 22");
			var dummy_2_1 = factory.New<DummyDependantBusinessObject>();
			dummy_2_1.ZD1_Z0 = dummy_2.PK;

			var rows = new List<DataRow>()
				{
					dummy_1.Row,
					dummy_2.Row,
					dummy_2_1.Row,
				};

			var saver = new DummyZSqlSaver(new DataSet(), GetNewSchemaResolver());
			var mockENtityFrameworkSettings = TestEntityFrameworkSettings.Get();

			using (GlobalServiceProvider.Configure(TestEntityFrameworkSettings.GetMockServiceProvider().Object))
			{
				mockENtityFrameworkSettings.ParameterizeInsertAndUpdateStatements = true;

				var result = saver.BuildCommandExposed(rows, 0, saver.RowsToPostPerSqlStatementForTest, saver.BytesToPostPerSqlStatementForTest, canConsolidateInserts: false);
				using (var command = result.Command)
				{
					var expected = Expected_SimpleInsert_Parameters(dummy_1.PK, dummy_2.PK, dummy_2_1.PK);
					AssertMultilineASCIIEquals("Insert statement should be equal", expected, command.StandardCommandText);
				}
			}

			using (GlobalServiceProvider.Configure(TestEntityFrameworkSettings.GetMockServiceProvider().Object))
			{
				mockENtityFrameworkSettings.ParameterizeInsertAndUpdateStatements = false;

				var result = saver.BuildCommandExposed(rows, 0, saver.RowsToPostPerSqlStatementForTest, saver.BytesToPostPerSqlStatementForTest, canConsolidateInserts: false);
				using (var command = result.Command)
				{
					var expected = Expected_SimpleInsert_Literals(dummy_1.PK, dummy_2.PK, dummy_2_1.PK);
					AssertMultilineASCIIEquals("Insert statement should be equal", expected, command.StandardCommandText);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestParameterization_MultiRowInsert()
		{
			var factory = new BusinessObjectFactory();
			var dummy_1 = factory.New<DummyBusinessObject>();
			var dummy_2 = factory.New<DummyBusinessObject>();
			var dummy_2_1 = factory.New<DummyDependantBusinessObject>();
			dummy_2_1.ZD1_Z0 = dummy_2.PK;

			var rows = new List<DataRow>()
				{
					dummy_1.Row,
					dummy_2.Row,
					dummy_2_1.Row,
				};

			var saver = new DummyZSqlSaver(new DataSet(), GetNewSchemaResolver());
			var mockENtityFrameworkSettings = TestEntityFrameworkSettings.Get();

			using (GlobalServiceProvider.Configure(TestEntityFrameworkSettings.GetMockServiceProvider().Object))
			{
				mockENtityFrameworkSettings.ParameterizeInsertAndUpdateStatements = true;

				var result = saver.BuildCommandExposed(rows, 0, saver.RowsToPostPerSqlStatementForTest, saver.BytesToPostPerSqlStatementForTest, canConsolidateInserts: true);
				using (var command = result.Command)
				{
					var expected = Expected_MultiRowInsert_Parameters(dummy_1.PK, dummy_2.PK, dummy_2_1.PK);
					AssertMultilineASCIIEquals("Insert statement should be equal", expected, command.StandardCommandText);
				}
			}

			using (GlobalServiceProvider.Configure(TestEntityFrameworkSettings.GetMockServiceProvider().Object))
			{
				mockENtityFrameworkSettings.ParameterizeInsertAndUpdateStatements = false;

				var result = saver.BuildCommandExposed(rows, 0, saver.RowsToPostPerSqlStatementForTest, saver.BytesToPostPerSqlStatementForTest, canConsolidateInserts: true);
				using (var command = result.Command)
				{
					var expected = Expected_MultiRowInsert_Literals(dummy_1.PK, dummy_2.PK, dummy_2_1.PK);
					AssertMultilineASCIIEquals("Insert statement should be equal", expected, command.StandardCommandText);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestParameterization_UpdateBulk()
		{
			var factory = new BusinessObjectFactory();
			var dummy_1 = factory.New<DummyBusinessObject>();
			var dummy_2 = factory.New<DummyBusinessObject>();
			var dummy_3 = factory.New<DummyBusinessObject>();
			factory.Save();

			dummy_1.Z0_Code = "AAA";
			dummy_2.Z0_Code = "BBB";
			dummy_3.Z0_Code = "CCC";

			var rows = new List<DataRow>()
				{
					dummy_1.Row,
					dummy_2.Row,
					dummy_3.Row,
				};

			var saver = new DummyZSqlSaver(new DataSet(), GetNewSchemaResolver());
			var mockENtityFrameworkSettings = TestEntityFrameworkSettings.Get();

			using (GlobalServiceProvider.Configure(TestEntityFrameworkSettings.GetMockServiceProvider().Object))
			{
				mockENtityFrameworkSettings.ParameterizeInsertAndUpdateStatements = true;

				var result = saver.BuildCommandExposed(rows, 0, saver.RowsToPostPerSqlStatementForTest, saver.BytesToPostPerSqlStatementForTest, canConsolidateInserts: true, bulkUpdate: true);
				using (var command = result.Command)
				{
					var expected = Expected_Update_ParametersBulk(dummy_1.PK, dummy_2.PK, dummy_3.PK);
					AssertMultilineASCIIEquals("Update statement should be equal", expected, command.StandardCommandText);
				}
			}
		}

		public void TestUpdateGeographyWithoutConcurrencyError()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();
			factory.Save();

			var updateSql = $"UPDATE dbo.DummyBizo SET Z0_Geography = geography::Point(-34.00293, 150.87012 , 4326) WHERE Z0_PK = @pk";
			Db.Connection.ExecuteNonQuery(updateSql, cmd => cmd.AddParameterBasedOnDbColumn("@pk", dummy.PK.ToGuid(), DummyBizoSchema.PK));

			var dummyReloaded = factory.LoadFromDatabase(typeof(DummyBusinessObject), dummy.PK) as DummyBusinessObject;
			dummyReloaded.Z0_Geography = ZGeography.CreatePoint(-33.9165057591259, 151.19541141);
			AssertNoExceptionThrown("Should not throw concurrency exception", () => factory.Save());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestParameterization_Update()
		{
			var factory = new BusinessObjectFactory();
			var dummy_1 = factory.New<DummyBusinessObject>();
			var dummy_2 = factory.New<DummyBusinessObject>();
			dummy_2.Z0_Geography = new ZGeography("-222 22");
			factory.Save();

			dummy_1.Z0_Code = "AAA";
			dummy_1.Z0_SmallDateTime = new ZDateTime(2014, 11, 11);
			dummy_1.Z0_Geography = new ZGeography("-111 11");

			dummy_2.Z0_Code = "BBB";
			dummy_2.Z0_Description = "new description";
			dummy_2.Z0_Geography = ZGeography.Empty;

			var rows = new List<DataRow>()
				{
					dummy_1.Row,
					dummy_2.Row,
				};

			var saver = new DummyZSqlSaver(new DataSet(), GetNewSchemaResolver());
			var mockENtityFrameworkSettings = TestEntityFrameworkSettings.Get();

			using (GlobalServiceProvider.Configure(TestEntityFrameworkSettings.GetMockServiceProvider().Object))
			{
				mockENtityFrameworkSettings.ParameterizeInsertAndUpdateStatements = true;

				var result = saver.BuildCommandExposed(rows, 0, saver.RowsToPostPerSqlStatementForTest, saver.BytesToPostPerSqlStatementForTest, canConsolidateInserts: true);
				using (var command = result.Command)
				{
					var expected = Expected_Update_Parameters(dummy_1.PK, dummy_2.PK);
					AssertMultilineASCIIEquals("Update statement should be equal", expected, command.StandardCommandText);
				}
			}

			using (GlobalServiceProvider.Configure(TestEntityFrameworkSettings.GetMockServiceProvider().Object))
			{
				mockENtityFrameworkSettings.ParameterizeInsertAndUpdateStatements = false;

				var result = saver.BuildCommandExposed(rows, 0, saver.RowsToPostPerSqlStatementForTest, saver.BytesToPostPerSqlStatementForTest, canConsolidateInserts: true);
				using (var command = result.Command)
				{
					var expected = Expected_Update_Literals(dummy_1.PK, dummy_2.PK);
					AssertMultilineASCIIEquals("Update statement should be equal", expected, command.StandardCommandText);
				}
			}
		}

		string Expected_SimpleInsert_Parameters(ZGuid pk_1, ZGuid pk_2, ZGuid pk_3)
		{
			return string.Format(
@"DECLARE @PK varchar(38), @row_count int;
BEGIN TRY

SET @PK = '{0}';
DECLARE @1_118 geography = geography::STGeomFromText('POINT EMPTY', 4326);
EXEC sys.sp_executesql N'INSERT dbo.DummyBizo (Z0_PK, Z0_AddInfo, Z0_AnotherDate, Z0_AnotherDecimal, Z0_AnotherNumber, Z0_BitFalse, Z0_BitFiltered, Z0_BitTrue, Z0_Bool, Z0_Byte, Z0_Code, Z0_Date, Z0_DateOnly, Z0_DateTimeOffset, Z0_Decimal, Z0_Description, Z0_FK_Code, Z0_Geography, Z0_Guid, Z0_IsSystem, Z0_IsValid, Z0_Long, Z0_Money, Z0_NAddInfo, Z0_Number, Z0_NVarChar, Z0_NVarCharMax, Z0_Short, Z0_SmallDateTime, Z0_SparseBit, Z0_SparseByte, Z0_SparseChar, Z0_SparseDate, Z0_SparseDateTime, Z0_SparseDateTimeOffset, Z0_SparseDecimal, Z0_SparseGuid, Z0_SparseLong, Z0_SparseMoney, Z0_SparseNumber, Z0_SparseNVarChar, Z0_SparseShort, Z0_SparseSmallDateTime, Z0_SparseTime, Z0_SparseVarBinaryMax, Z0_SparseVarChar, Z0_SparseXml, Z0_Time, Z0_VarBinaryMax, Z0_VarCharMax, Z0_Xml) VALUES
	(@11, @12, @13, @14, @15, @16, @17, @18, @19, @110, @111, @13, @113, @114, @115, @116, @117, @118, @119, @120, @121, @122, @123, @124, @125, @126, @127, @128, @129, @130, @131, @132, @113, @13, @114, @136, @119, @138, @139, @140, @141, @142, @129, @144, @145, @132, @147, @144, @145, @150, @151)
'
, N'@11 uniqueidentifier, @12 varchar(4096), @13 datetime, @14 decimal(18,3), @15 int, @16 bit, @17 bit, @18 bit, @19 char(1), @110 tinyint, @111 varchar(5), @113 date, @114 datetimeoffset(7), @115 decimal(18,0), @116 varchar(100), @117 varchar(5), @118 geography, @119 uniqueidentifier, @120 bit, @121 char(1), @122 bigint, @123 money, @124 nvarchar(4000), @125 int, @126 nvarchar(20), @127 nvarchar(max), @128 smallint, @129 smalldatetime, @130 bit, @131 tinyint, @132 varchar(5), @136 decimal(18,3), @138 bigint, @139 money, @140 int, @141 nvarchar(20), @142 smallint, @144 time, @145 varbinary(max), @147 xml, @150 varchar(max), @151 xml'
, @11 = '{0}', @12 = '', @13 = NULL, @14 = 0, @15 = 0, @16 = 0, @17 = 0, @18 = 1, @19 = 'N', @110 = 0, @111 = 'NCODE', @113 = NULL, @114 = NULL, @115 = 0, @116 = 'Default', @117 = '', @118 = @1_118, @119 = NULL, @120 = 1, @121 = 'N', @122 = 0, @123 = 0, @124 = N'', @125 = 0, @126 = N'', @127 = N'', @128 = 0, @129 = NULL, @130 = NULL, @131 = NULL, @132 = NULL, @136 = NULL, @138 = NULL, @139 = NULL, @140 = NULL, @141 = NULL, @142 = NULL, @144 = NULL, @145 = NULL, @147 = NULL, @150 = '', @151 = N'';

SET @PK = '{1}';
DECLARE @2_118 geography = geography::STGeomFromText('POINT (-222 22)', 4326);
EXEC sys.sp_executesql N'INSERT dbo.DummyBizo (Z0_PK, Z0_AddInfo, Z0_AnotherDate, Z0_AnotherDecimal, Z0_AnotherNumber, Z0_BitFalse, Z0_BitFiltered, Z0_BitTrue, Z0_Bool, Z0_Byte, Z0_Code, Z0_Date, Z0_DateOnly, Z0_DateTimeOffset, Z0_Decimal, Z0_Description, Z0_FK_Code, Z0_Geography, Z0_Guid, Z0_IsSystem, Z0_IsValid, Z0_Long, Z0_Money, Z0_NAddInfo, Z0_Number, Z0_NVarChar, Z0_NVarCharMax, Z0_Short, Z0_SmallDateTime, Z0_SparseBit, Z0_SparseByte, Z0_SparseChar, Z0_SparseDate, Z0_SparseDateTime, Z0_SparseDateTimeOffset, Z0_SparseDecimal, Z0_SparseGuid, Z0_SparseLong, Z0_SparseMoney, Z0_SparseNumber, Z0_SparseNVarChar, Z0_SparseShort, Z0_SparseSmallDateTime, Z0_SparseTime, Z0_SparseVarBinaryMax, Z0_SparseVarChar, Z0_SparseXml, Z0_Time, Z0_VarBinaryMax, Z0_VarCharMax, Z0_Xml) VALUES
	(@11, @12, @13, @14, @15, @16, @17, @18, @19, @110, @111, @13, @113, @114, @115, @116, @117, @118, @119, @120, @121, @122, @123, @124, @125, @126, @127, @128, @129, @130, @131, @132, @113, @13, @114, @136, @119, @138, @139, @140, @141, @142, @129, @144, @145, @132, @147, @144, @145, @150, @151)
'
, N'@11 uniqueidentifier, @12 varchar(4096), @13 datetime, @14 decimal(18,3), @15 int, @16 bit, @17 bit, @18 bit, @19 char(1), @110 tinyint, @111 varchar(5), @113 date, @114 datetimeoffset(7), @115 decimal(18,0), @116 varchar(100), @117 varchar(5), @118 geography, @119 uniqueidentifier, @120 bit, @121 char(1), @122 bigint, @123 money, @124 nvarchar(4000), @125 int, @126 nvarchar(20), @127 nvarchar(max), @128 smallint, @129 smalldatetime, @130 bit, @131 tinyint, @132 varchar(5), @136 decimal(18,3), @138 bigint, @139 money, @140 int, @141 nvarchar(20), @142 smallint, @144 time, @145 varbinary(max), @147 xml, @150 varchar(max), @151 xml'
, @11 = '{1}', @12 = '', @13 = NULL, @14 = 0, @15 = 0, @16 = 0, @17 = 0, @18 = 1, @19 = 'N', @110 = 0, @111 = 'NCODE', @113 = NULL, @114 = NULL, @115 = 0, @116 = 'Default', @117 = '', @118 = @2_118, @119 = NULL, @120 = 1, @121 = 'N', @122 = 0, @123 = 0, @124 = N'', @125 = 0, @126 = N'', @127 = N'', @128 = 0, @129 = NULL, @130 = NULL, @131 = NULL, @132 = NULL, @136 = NULL, @138 = NULL, @139 = NULL, @140 = NULL, @141 = NULL, @142 = NULL, @144 = NULL, @145 = NULL, @147 = NULL, @150 = '', @151 = N'';

SET @PK = '{2}';
EXEC sys.sp_executesql N'INSERT dbo.DummyDependentBizo (ZD1_PK, ZD1_Code, ZD1_Number, ZD1_NumberUnit, ZD1_NumberUnitCode, ZD1_Z0, ZD1_Z0_NKCode) VALUES
	(@11, @12, @13, @14, @15, @16, @17)
'
, N'@11 uniqueidentifier, @12 varchar(5), @13 int, @14 uniqueidentifier, @15 varchar(3), @16 uniqueidentifier, @17 varchar(5)'
, @11 = '{2}', @12 = '', @13 = 0, @14 = NULL, @15 = '', @16 = '{1}', @17 = '';

END TRY
BEGIN CATCH
	DECLARE @ErrMsg nvarchar(4000) = ERROR_MESSAGE(), @Severity int = ERROR_SEVERITY(), @ErrNum int = ERROR_NUMBER();

	IF (@Severity > 18) THROW;
	ELSE
	BEGIN
		DECLARE @Msg nvarchar(4000) = '{{' + @PK + ',' + case when @ErrMsg = '~ConcurrencyError~' then 'True' else 'False' end + ',' + cast(@ErrNum as varchar(10)) + '}} ' + @ErrMsg;
		RAISERROR('%s', @Severity, 1, @Msg);
		IF (XACT_STATE()) = -1 ROLLBACK;
	END
END CATCH", pk_1, pk_2, pk_3);
		}

		string Expected_SimpleInsert_Literals(ZGuid pk_1, ZGuid pk_2, ZGuid pk_3)
		{
			// TODO: Update this when everything builds.
			return string.Format(
@"DECLARE @PK varchar(38), @row_count int;
BEGIN TRY

SET @PK = '{0}';
DECLARE @1_118 geography = geography::STGeomFromText('POINT EMPTY', 4326);
INSERT dbo.DummyBizo (Z0_PK, Z0_AddInfo, Z0_AnotherDate, Z0_AnotherDecimal, Z0_AnotherNumber, Z0_BitFalse, Z0_BitFiltered, Z0_BitTrue, Z0_Bool, Z0_Byte, Z0_Code, Z0_Date, Z0_DateOnly, Z0_DateTimeOffset, Z0_Decimal, Z0_Description, Z0_FK_Code, Z0_Geography, Z0_Guid, Z0_IsSystem, Z0_IsValid, Z0_Long, Z0_Money, Z0_NAddInfo, Z0_Number, Z0_NVarChar, Z0_NVarCharMax, Z0_Short, Z0_SmallDateTime, Z0_SparseBit, Z0_SparseByte, Z0_SparseChar, Z0_SparseDate, Z0_SparseDateTime, Z0_SparseDateTimeOffset, Z0_SparseDecimal, Z0_SparseGuid, Z0_SparseLong, Z0_SparseMoney, Z0_SparseNumber, Z0_SparseNVarChar, Z0_SparseShort, Z0_SparseSmallDateTime, Z0_SparseTime, Z0_SparseVarBinaryMax, Z0_SparseVarChar, Z0_SparseXml, Z0_Time, Z0_VarBinaryMax, Z0_VarCharMax, Z0_Xml) VALUES
	('{0}', '', NULL, 0, 0, 0, 0, 1, 'N', 0, 'NCODE', NULL, NULL, NULL, 0, 'Default', '', @1_118, NULL, 1, 'N', 0, 0, N'', 0, N'', N'', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '', N'');

SET @PK = '{1}';
DECLARE @2_118 geography = geography::STGeomFromText('POINT (-222 22)', 4326);
INSERT dbo.DummyBizo (Z0_PK, Z0_AddInfo, Z0_AnotherDate, Z0_AnotherDecimal, Z0_AnotherNumber, Z0_BitFalse, Z0_BitFiltered, Z0_BitTrue, Z0_Bool, Z0_Byte, Z0_Code, Z0_Date, Z0_DateOnly, Z0_DateTimeOffset, Z0_Decimal, Z0_Description, Z0_FK_Code, Z0_Geography, Z0_Guid, Z0_IsSystem, Z0_IsValid, Z0_Long, Z0_Money, Z0_NAddInfo, Z0_Number, Z0_NVarChar, Z0_NVarCharMax, Z0_Short, Z0_SmallDateTime, Z0_SparseBit, Z0_SparseByte, Z0_SparseChar, Z0_SparseDate, Z0_SparseDateTime, Z0_SparseDateTimeOffset, Z0_SparseDecimal, Z0_SparseGuid, Z0_SparseLong, Z0_SparseMoney, Z0_SparseNumber, Z0_SparseNVarChar, Z0_SparseShort, Z0_SparseSmallDateTime, Z0_SparseTime, Z0_SparseVarBinaryMax, Z0_SparseVarChar, Z0_SparseXml, Z0_Time, Z0_VarBinaryMax, Z0_VarCharMax, Z0_Xml) VALUES
	('{1}', '', NULL, 0, 0, 0, 0, 1, 'N', 0, 'NCODE', NULL, NULL, NULL, 0, 'Default', '', @2_118, NULL, 1, 'N', 0, 0, N'', 0, N'', N'', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '', N'');

SET @PK = '{2}';
INSERT dbo.DummyDependentBizo (ZD1_PK, ZD1_Code, ZD1_Number, ZD1_NumberUnit, ZD1_NumberUnitCode, ZD1_Z0, ZD1_Z0_NKCode) VALUES
	('{2}', '', 0, NULL, '', '{1}', '');

END TRY
BEGIN CATCH
	DECLARE @ErrMsg nvarchar(4000) = ERROR_MESSAGE(), @Severity int = ERROR_SEVERITY(), @ErrNum int = ERROR_NUMBER();

	IF (@Severity > 18) THROW;
	ELSE
	BEGIN
		DECLARE @Msg nvarchar(4000) = '{{' + @PK + ',' + case when @ErrMsg = '~ConcurrencyError~' then 'True' else 'False' end + ',' + cast(@ErrNum as varchar(10)) + '}} ' + @ErrMsg;
		RAISERROR('%s', @Severity, 1, @Msg);
		IF (XACT_STATE()) = -1 ROLLBACK;
	END
END CATCH", pk_1, pk_2, pk_3);
		}

		string Expected_MultiRowInsert_Parameters(ZGuid pk_1, ZGuid pk_2, ZGuid pk_3)
		{
			return string.Format(
@"DECLARE @PK varchar(38), @row_count int;
BEGIN TRY

SET @PK = 'DeadBeef-Add1-Add2-Add3-DeadBeefC0de';
DECLARE @1_118 geography = geography::STGeomFromText('POINT EMPTY', 4326);
DECLARE @1_218 geography = geography::STGeomFromText('POINT EMPTY', 4326);
EXEC sys.sp_executesql N'INSERT dbo.DummyBizo (Z0_PK, Z0_AddInfo, Z0_AnotherDate, Z0_AnotherDecimal, Z0_AnotherNumber, Z0_BitFalse, Z0_BitFiltered, Z0_BitTrue, Z0_Bool, Z0_Byte, Z0_Code, Z0_Date, Z0_DateOnly, Z0_DateTimeOffset, Z0_Decimal, Z0_Description, Z0_FK_Code, Z0_Geography, Z0_Guid, Z0_IsSystem, Z0_IsValid, Z0_Long, Z0_Money, Z0_NAddInfo, Z0_Number, Z0_NVarChar, Z0_NVarCharMax, Z0_Short, Z0_SmallDateTime, Z0_SparseBit, Z0_SparseByte, Z0_SparseChar, Z0_SparseDate, Z0_SparseDateTime, Z0_SparseDateTimeOffset, Z0_SparseDecimal, Z0_SparseGuid, Z0_SparseLong, Z0_SparseMoney, Z0_SparseNumber, Z0_SparseNVarChar, Z0_SparseShort, Z0_SparseSmallDateTime, Z0_SparseTime, Z0_SparseVarBinaryMax, Z0_SparseVarChar, Z0_SparseXml, Z0_Time, Z0_VarBinaryMax, Z0_VarCharMax, Z0_Xml) VALUES
	(@11, @12, @13, @14, @15, @16, @17, @18, @19, @110, @111, @13, @113, @114, @115, @116, @117, @118, @119, @120, @121, @122, @123, @124, @125, @126, @127, @128, @129, @130, @131, @132, @113, @13, @114, @136, @119, @138, @139, @140, @141, @142, @129, @144, @145, @132, @147, @144, @145, @150, @151),
	(@21, @22, @13, @24, @25, @26, @27, @28, @29, @210, @211, @13, @113, @114, @215, @216, @217, @218, @119, @220, @221, @222, @223, @224, @225, @226, @227, @228, @129, @130, @131, @132, @113, @13, @114, @136, @119, @138, @139, @140, @141, @142, @129, @144, @145, @132, @147, @144, @145, @250, @251)
'
, N'@11 uniqueidentifier, @12 varchar(4096), @13 datetime, @14 decimal(18,3), @15 int, @16 bit, @17 bit, @18 bit, @19 char(1), @110 tinyint, @111 varchar(5), @113 date, @114 datetimeoffset(7), @115 decimal(18,0), @116 varchar(100), @117 varchar(5), @118 geography, @119 uniqueidentifier, @120 bit, @121 char(1), @122 bigint, @123 money, @124 nvarchar(4000), @125 int, @126 nvarchar(20), @127 nvarchar(max), @128 smallint, @129 smalldatetime, @130 bit, @131 tinyint, @132 varchar(5), @136 decimal(18,3), @138 bigint, @139 money, @140 int, @141 nvarchar(20), @142 smallint, @144 time, @145 varbinary(max), @147 xml, @150 varchar(max), @151 xml
  , @21 uniqueidentifier, @22 varchar(4096), @24 decimal(18,3), @25 int, @26 bit, @27 bit, @28 bit, @29 char(1), @210 tinyint, @211 varchar(5), @215 decimal(18,0), @216 varchar(100), @217 varchar(5), @218 geography, @220 bit, @221 char(1), @222 bigint, @223 money, @224 nvarchar(4000), @225 int, @226 nvarchar(20), @227 nvarchar(max), @228 smallint, @250 varchar(max), @251 xml'
, @11 = '{0}', @12 = '', @13 = NULL, @14 = 0, @15 = 0, @16 = 0, @17 = 0, @18 = 1, @19 = 'N', @110 = 0, @111 = 'NCODE', @113 = NULL, @114 = NULL, @115 = 0, @116 = 'Default', @117 = '', @118 = @1_118, @119 = NULL, @120 = 1, @121 = 'N', @122 = 0, @123 = 0, @124 = N'', @125 = 0, @126 = N'', @127 = N'', @128 = 0, @129 = NULL, @130 = NULL, @131 = NULL, @132 = NULL, @136 = NULL, @138 = NULL, @139 = NULL, @140 = NULL, @141 = NULL, @142 = NULL, @144 = NULL, @145 = NULL, @147 = NULL, @150 = '', @151 = N''
, @21 = '{1}', @22 = '', @24 = 0, @25 = 0, @26 = 0, @27 = 0, @28 = 1, @29 = 'N', @210 = 0, @211 = 'NCODE', @215 = 0, @216 = 'Default', @217 = '', @218 = @1_218, @220 = 1, @221 = 'N', @222 = 0, @223 = 0, @224 = N'', @225 = 0, @226 = N'', @227 = N'', @228 = 0, @250 = '', @251 = N'';

END TRY
BEGIN CATCH
	DECLARE @ErrMsg nvarchar(4000) = ERROR_MESSAGE(), @Severity int = ERROR_SEVERITY(), @ErrNum int = ERROR_NUMBER();

	IF (@Severity > 18) THROW;
	ELSE
	BEGIN
		DECLARE @Msg nvarchar(4000) = '{{' + @PK + ',' + case when @ErrMsg = '~ConcurrencyError~' then 'True' else 'False' end + ',' + cast(@ErrNum as varchar(10)) + '}} ' + @ErrMsg;
		RAISERROR('%s', @Severity, 1, @Msg);
		IF (XACT_STATE()) = -1 ROLLBACK;
	END
END CATCH", pk_1, pk_2, pk_3);
		}

		string Expected_MultiRowInsert_Literals(ZGuid pk_1, ZGuid pk_2, ZGuid pk_3)
		{
			return string.Format(
@"DECLARE @PK varchar(38), @row_count int;
BEGIN TRY

SET @PK = 'DeadBeef-Add1-Add2-Add3-DeadBeefC0de';
DECLARE @1_118 geography = geography::STGeomFromText('POINT EMPTY', 4326);
DECLARE @1_218 geography = geography::STGeomFromText('POINT EMPTY', 4326);
INSERT dbo.DummyBizo (Z0_PK, Z0_AddInfo, Z0_AnotherDate, Z0_AnotherDecimal, Z0_AnotherNumber, Z0_BitFalse, Z0_BitFiltered, Z0_BitTrue, Z0_Bool, Z0_Byte, Z0_Code, Z0_Date, Z0_DateOnly, Z0_DateTimeOffset, Z0_Decimal, Z0_Description, Z0_FK_Code, Z0_Geography, Z0_Guid, Z0_IsSystem, Z0_IsValid, Z0_Long, Z0_Money, Z0_NAddInfo, Z0_Number, Z0_NVarChar, Z0_NVarCharMax, Z0_Short, Z0_SmallDateTime, Z0_SparseBit, Z0_SparseByte, Z0_SparseChar, Z0_SparseDate, Z0_SparseDateTime, Z0_SparseDateTimeOffset, Z0_SparseDecimal, Z0_SparseGuid, Z0_SparseLong, Z0_SparseMoney, Z0_SparseNumber, Z0_SparseNVarChar, Z0_SparseShort, Z0_SparseSmallDateTime, Z0_SparseTime, Z0_SparseVarBinaryMax, Z0_SparseVarChar, Z0_SparseXml, Z0_Time, Z0_VarBinaryMax, Z0_VarCharMax, Z0_Xml) VALUES
	('{0}', '', NULL, 0, 0, 0, 0, 1, 'N', 0, 'NCODE', NULL, NULL, NULL, 0, 'Default', '', @1_118, NULL, 1, 'N', 0, 0, N'', 0, N'', N'', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '', N''),
	('{1}', '', NULL, 0, 0, 0, 0, 1, 'N', 0, 'NCODE', NULL, NULL, NULL, 0, 'Default', '', @1_218, NULL, 1, 'N', 0, 0, N'', 0, N'', N'', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, '', N'');

END TRY
BEGIN CATCH
	DECLARE @ErrMsg nvarchar(4000) = ERROR_MESSAGE(), @Severity int = ERROR_SEVERITY(), @ErrNum int = ERROR_NUMBER();

	IF (@Severity > 18) THROW;
	ELSE
	BEGIN
		DECLARE @Msg nvarchar(4000) = '{{' + @PK + ',' + case when @ErrMsg = '~ConcurrencyError~' then 'True' else 'False' end + ',' + cast(@ErrNum as varchar(10)) + '}} ' + @ErrMsg;
		RAISERROR('%s', @Severity, 1, @Msg);
		IF (XACT_STATE()) = -1 ROLLBACK;
	END
END CATCH", pk_1, pk_2, pk_3);
		}

		string Expected_Update_ParametersBulk(ZGuid pk_1, ZGuid pk_2, ZGuid pk_3)
		{
			return string.Format(
@"DECLARE @PK varchar(38), @row_count int;
BEGIN TRY

SET @PK = 'Baada555-Cafe-babe-4b1d-CafeBabe4b1d';
DECLARE @0_14 geography = geography::STGeomFromText('POINT EMPTY', 4326);
DECLARE @1_14 geography = geography::STGeomFromText('POINT EMPTY', 4326);
DECLARE @2_14 geography = geography::STGeomFromText('POINT EMPTY', 4326);
EXEC sys.sp_executesql N'UPDATE dbo.DummyBizo SET
	Z0_Code = CASE Z0_PK
	WHEN @0_0 THEN @0_10
	WHEN @1_0 THEN @1_10
	WHEN @2_0 THEN @2_10
	END
FROM
	dbo.DummyBizo WITH (INDEX([PK_UX__Z0_PK]), UPDLOCK)
WHERE
	Z0_PK in (@0_0,@1_0,@2_0)
	AND
	(
		(
			1=1
			AND Z0_PK = @0_0
	AND Z0_AddInfo = @0_1
	AND Z0_AnotherDate is NULL
	AND Z0_AnotherDecimal = @0_2
	AND Z0_AnotherNumber = @0_3
	AND Z0_BitFalse = @0_4
	AND Z0_BitFiltered = @0_5
	AND Z0_BitTrue = @0_6
	AND Z0_Bool = @0_7
	AND Z0_Byte = @0_8
	AND (Z0_Code = @0_9 OR Z0_Code = @0_10)
	AND Z0_Date is NULL
	AND Z0_DateOnly is NULL
	AND Z0_DateTimeOffset is NULL
	AND Z0_Decimal = @0_11
	AND Z0_Description = @0_12
	AND Z0_FK_Code = @0_13
	AND geography::STGeomFromText(Z0_Geography.STAsText(), 4326).STEquals(@0_14) = 1
	AND Z0_Guid is NULL
	AND Z0_IsSystem = @0_15
	AND Z0_IsValid = @0_16
	AND Z0_Long = @0_17
	AND Z0_Money = @0_18
	AND Z0_NAddInfo = @0_19
	AND Z0_Number = @0_20
	AND Z0_NVarChar = @0_21
	AND Z0_Short = @0_22
	AND Z0_SmallDateTime is NULL
	AND Z0_SparseBit is NULL
	AND Z0_SparseByte is NULL
	AND Z0_SparseChar is NULL
	AND Z0_SparseDate is NULL
	AND Z0_SparseDateTime is NULL
	AND Z0_SparseDateTimeOffset is NULL
	AND Z0_SparseDecimal is NULL
	AND Z0_SparseGuid is NULL
	AND Z0_SparseLong is NULL
	AND Z0_SparseMoney is NULL
	AND Z0_SparseNumber is NULL
	AND Z0_SparseNVarChar is NULL
	AND Z0_SparseShort is NULL
	AND Z0_SparseSmallDateTime is NULL
	AND Z0_SparseTime is NULL
	AND Z0_SparseVarChar is NULL
	AND Z0_Time is NULL
		)
		OR
		(
			1=1
			AND Z0_PK = @1_0
	AND Z0_AddInfo = @1_1
	AND Z0_AnotherDate is NULL
	AND Z0_AnotherDecimal = @1_2
	AND Z0_AnotherNumber = @1_3
	AND Z0_BitFalse = @1_4
	AND Z0_BitFiltered = @1_5
	AND Z0_BitTrue = @1_6
	AND Z0_Bool = @1_7
	AND Z0_Byte = @1_8
	AND (Z0_Code = @1_9 OR Z0_Code = @1_10)
	AND Z0_Date is NULL
	AND Z0_DateOnly is NULL
	AND Z0_DateTimeOffset is NULL
	AND Z0_Decimal = @1_11
	AND Z0_Description = @1_12
	AND Z0_FK_Code = @1_13
	AND geography::STGeomFromText(Z0_Geography.STAsText(), 4326).STEquals(@1_14) = 1
	AND Z0_Guid is NULL
	AND Z0_IsSystem = @1_15
	AND Z0_IsValid = @1_16
	AND Z0_Long = @1_17
	AND Z0_Money = @1_18
	AND Z0_NAddInfo = @1_19
	AND Z0_Number = @1_20
	AND Z0_NVarChar = @1_21
	AND Z0_Short = @1_22
	AND Z0_SmallDateTime is NULL
	AND Z0_SparseBit is NULL
	AND Z0_SparseByte is NULL
	AND Z0_SparseChar is NULL
	AND Z0_SparseDate is NULL
	AND Z0_SparseDateTime is NULL
	AND Z0_SparseDateTimeOffset is NULL
	AND Z0_SparseDecimal is NULL
	AND Z0_SparseGuid is NULL
	AND Z0_SparseLong is NULL
	AND Z0_SparseMoney is NULL
	AND Z0_SparseNumber is NULL
	AND Z0_SparseNVarChar is NULL
	AND Z0_SparseShort is NULL
	AND Z0_SparseSmallDateTime is NULL
	AND Z0_SparseTime is NULL
	AND Z0_SparseVarChar is NULL
	AND Z0_Time is NULL
		)
		OR
		(
			1=1
			AND Z0_PK = @2_0
	AND Z0_AddInfo = @2_1
	AND Z0_AnotherDate is NULL
	AND Z0_AnotherDecimal = @2_2
	AND Z0_AnotherNumber = @2_3
	AND Z0_BitFalse = @2_4
	AND Z0_BitFiltered = @2_5
	AND Z0_BitTrue = @2_6
	AND Z0_Bool = @2_7
	AND Z0_Byte = @2_8
	AND (Z0_Code = @2_9 OR Z0_Code = @2_10)
	AND Z0_Date is NULL
	AND Z0_DateOnly is NULL
	AND Z0_DateTimeOffset is NULL
	AND Z0_Decimal = @2_11
	AND Z0_Description = @2_12
	AND Z0_FK_Code = @2_13
	AND geography::STGeomFromText(Z0_Geography.STAsText(), 4326).STEquals(@2_14) = 1
	AND Z0_Guid is NULL
	AND Z0_IsSystem = @2_15
	AND Z0_IsValid = @2_16
	AND Z0_Long = @2_17
	AND Z0_Money = @2_18
	AND Z0_NAddInfo = @2_19
	AND Z0_Number = @2_20
	AND Z0_NVarChar = @2_21
	AND Z0_Short = @2_22
	AND Z0_SmallDateTime is NULL
	AND Z0_SparseBit is NULL
	AND Z0_SparseByte is NULL
	AND Z0_SparseChar is NULL
	AND Z0_SparseDate is NULL
	AND Z0_SparseDateTime is NULL
	AND Z0_SparseDateTimeOffset is NULL
	AND Z0_SparseDecimal is NULL
	AND Z0_SparseGuid is NULL
	AND Z0_SparseLong is NULL
	AND Z0_SparseMoney is NULL
	AND Z0_SparseNumber is NULL
	AND Z0_SparseNVarChar is NULL
	AND Z0_SparseShort is NULL
	AND Z0_SparseSmallDateTime is NULL
	AND Z0_SparseTime is NULL
	AND Z0_SparseVarChar is NULL
	AND Z0_Time is NULL
		)
	);
SET @0 = @@ROWCOUNT;
'
, N'@0 int out
,@0_0 uniqueidentifier, @0_1 varchar(4096), @0_2 decimal(18,3), @0_3 int, @0_4 bit, @0_5 bit, @0_6 bit, @0_7 char(1), @0_8 tinyint, @0_9 varchar(5), @0_10 varchar(5), @0_11 decimal(18,0), @0_12 varchar(100), @0_13 varchar(5), @0_14 geography, @0_15 bit, @0_16 char(1), @0_17 bigint, @0_18 money, @0_19 nvarchar(4000), @0_20 int, @0_21 nvarchar(20), @0_22 smallint
,@1_0 uniqueidentifier, @1_1 varchar(4096), @1_2 decimal(18,3), @1_3 int, @1_4 bit, @1_5 bit, @1_6 bit, @1_7 char(1), @1_8 tinyint, @1_9 varchar(5), @1_10 varchar(5), @1_11 decimal(18,0), @1_12 varchar(100), @1_13 varchar(5), @1_14 geography, @1_15 bit, @1_16 char(1), @1_17 bigint, @1_18 money, @1_19 nvarchar(4000), @1_20 int, @1_21 nvarchar(20), @1_22 smallint
,@2_0 uniqueidentifier, @2_1 varchar(4096), @2_2 decimal(18,3), @2_3 int, @2_4 bit, @2_5 bit, @2_6 bit, @2_7 char(1), @2_8 tinyint, @2_9 varchar(5), @2_10 varchar(5), @2_11 decimal(18,0), @2_12 varchar(100), @2_13 varchar(5), @2_14 geography, @2_15 bit, @2_16 char(1), @2_17 bigint, @2_18 money, @2_19 nvarchar(4000), @2_20 int, @2_21 nvarchar(20), @2_22 smallint
'
, @0 = @row_count out
,@0_0 = '{0}', @0_1 = '', @0_2 = 0, @0_3 = 0, @0_4 = 0, @0_5 = 0, @0_6 = 1, @0_7 = 'N', @0_8 = 0, @0_9 = 'NCODE', @0_10 = 'AAA', @0_11 = 0, @0_12 = 'Default', @0_13 = '', @0_14 = @0_14, @0_15 = 1, @0_16 = 'N', @0_17 = 0, @0_18 = 0, @0_19 = N'', @0_20 = 0, @0_21 = N'', @0_22 = 0
,@1_0 = '{1}', @1_1 = '', @1_2 = 0, @1_3 = 0, @1_4 = 0, @1_5 = 0, @1_6 = 1, @1_7 = 'N', @1_8 = 0, @1_9 = 'NCODE', @1_10 = 'BBB', @1_11 = 0, @1_12 = 'Default', @1_13 = '', @1_14 = @1_14, @1_15 = 1, @1_16 = 'N', @1_17 = 0, @1_18 = 0, @1_19 = N'', @1_20 = 0, @1_21 = N'', @1_22 = 0
,@2_0 = '{2}', @2_1 = '', @2_2 = 0, @2_3 = 0, @2_4 = 0, @2_5 = 0, @2_6 = 1, @2_7 = 'N', @2_8 = 0, @2_9 = 'NCODE', @2_10 = 'CCC', @2_11 = 0, @2_12 = 'Default', @2_13 = '', @2_14 = @2_14, @2_15 = 1, @2_16 = 'N', @2_17 = 0, @2_18 = 0, @2_19 = N'', @2_20 = 0, @2_21 = N'', @2_22 = 0
;
if (@row_count <> 3) RAISERROR('~ConcurrencyError~', 16, 1);

END TRY
BEGIN CATCH
	DECLARE @ErrMsg nvarchar(4000) = ERROR_MESSAGE(), @Severity int = ERROR_SEVERITY(), @ErrNum int = ERROR_NUMBER();

	IF (@Severity > 18) THROW;
	ELSE
	BEGIN
		DECLARE @Msg nvarchar(4000) = '{{' + @PK + ',' + case when @ErrMsg = '~ConcurrencyError~' then 'True' else 'False' end + ',' + cast(@ErrNum as varchar(10)) + '}} ' + @ErrMsg;
		RAISERROR('%s', @Severity, 1, @Msg);
		IF (XACT_STATE()) = -1 ROLLBACK;
	END
END CATCH", pk_1, pk_2, pk_3);
		}

		string Expected_Update_Parameters(ZGuid pk_1, ZGuid pk_2)
		{
			return string.Format(
@"DECLARE @PK varchar(38), @row_count int;
BEGIN TRY

SET @PK = '{0}';
DECLARE @1_15 geography = geography::STGeomFromText('POINT EMPTY', 4326);
DECLARE @1_16 geography = geography::STGeomFromText('POINT (-111 11)', 4326);
EXEC sys.sp_executesql N'UPDATE dbo.DummyBizo SET
	Z0_Code = @11,
	Z0_Geography = @16,
	Z0_SmallDateTime = @25
FROM
	dbo.DummyBizo WITH (INDEX([PK_UX__Z0_PK]), UPDLOCK)
WHERE 1=1
	AND Z0_PK = @1
	AND Z0_AddInfo = @2
	AND Z0_AnotherDate is NULL
	AND Z0_AnotherDecimal = @3
	AND Z0_AnotherNumber = @4
	AND Z0_BitFalse = @5
	AND Z0_BitFiltered = @6
	AND Z0_BitTrue = @7
	AND Z0_Bool = @8
	AND Z0_Byte = @9
	AND (Z0_Code = @10 OR Z0_Code = @11)
	AND Z0_Date is NULL
	AND Z0_DateOnly is NULL
	AND Z0_DateTimeOffset is NULL
	AND Z0_Decimal = @12
	AND Z0_Description = @13
	AND Z0_FK_Code = @14
	AND (geography::STGeomFromText(Z0_Geography.STAsText(), 4326).STEquals(@15) = 1 OR geography::STGeomFromText(Z0_Geography.STAsText(), 4326).STEquals(@16) = 1)
	AND Z0_Guid is NULL
	AND Z0_IsSystem = @17
	AND Z0_IsValid = @18
	AND Z0_Long = @19
	AND Z0_Money = @20
	AND Z0_NAddInfo = @21
	AND Z0_Number = @22
	AND Z0_NVarChar = @23
	AND Z0_Short = @24
	AND (Z0_SmallDateTime is NULL OR Z0_SmallDateTime = @25)
	AND Z0_SparseBit is NULL
	AND Z0_SparseByte is NULL
	AND Z0_SparseChar is NULL
	AND Z0_SparseDate is NULL
	AND Z0_SparseDateTime is NULL
	AND Z0_SparseDateTimeOffset is NULL
	AND Z0_SparseDecimal is NULL
	AND Z0_SparseGuid is NULL
	AND Z0_SparseLong is NULL
	AND Z0_SparseMoney is NULL
	AND Z0_SparseNumber is NULL
	AND Z0_SparseNVarChar is NULL
	AND Z0_SparseShort is NULL
	AND Z0_SparseSmallDateTime is NULL
	AND Z0_SparseTime is NULL
	AND Z0_SparseVarChar is NULL
	AND Z0_Time is NULL;
SET @0 = @@ROWCOUNT;
'
, N'@0 int out, @1 uniqueidentifier, @2 varchar(4096), @3 decimal(18,3), @4 int, @5 bit, @6 bit, @7 bit, @8 char(1), @9 tinyint, @10 varchar(5), @11 varchar(5), @12 decimal(18,0), @13 varchar(100), @14 varchar(5), @15 geography, @16 geography, @17 bit, @18 char(1), @19 bigint, @20 money, @21 nvarchar(4000), @22 int, @23 nvarchar(20), @24 smallint, @25 smalldatetime'
, @0 = @row_count out, @1 = '{0}', @2 = '', @3 = 0, @4 = 0, @5 = 0, @6 = 0, @7 = 1, @8 = 'N', @9 = 0, @10 = 'NCODE', @11 = 'AAA', @12 = 0, @13 = 'Default', @14 = '', @15 = @1_15, @16 = @1_16, @17 = 1, @18 = 'N', @19 = 0, @20 = 0, @21 = N'', @22 = 0, @23 = N'', @24 = 0, @25 = '2014-11-11 00:00:00.000';
if (@row_count = 0) RAISERROR('~ConcurrencyError~', 16, 1);

SET @PK = '{1}';
DECLARE @2_16 geography = geography::STGeomFromText('POINT (-222 22)', 4326);
DECLARE @2_17 geography = geography::STGeomFromText('POINT EMPTY', 4326);
EXEC sys.sp_executesql N'UPDATE dbo.DummyBizo SET
	Z0_Code = @11,
	Z0_Description = @14,
	Z0_Geography = @17
FROM
	dbo.DummyBizo WITH (INDEX([PK_UX__Z0_PK]), UPDLOCK)
WHERE 1=1
	AND Z0_PK = @1
	AND Z0_AddInfo = @2
	AND Z0_AnotherDate is NULL
	AND Z0_AnotherDecimal = @3
	AND Z0_AnotherNumber = @4
	AND Z0_BitFalse = @5
	AND Z0_BitFiltered = @6
	AND Z0_BitTrue = @7
	AND Z0_Bool = @8
	AND Z0_Byte = @9
	AND (Z0_Code = @10 OR Z0_Code = @11)
	AND Z0_Date is NULL
	AND Z0_DateOnly is NULL
	AND Z0_DateTimeOffset is NULL
	AND Z0_Decimal = @12
	AND (Z0_Description = @13 OR Z0_Description = @14)
	AND Z0_FK_Code = @15
	AND (geography::STGeomFromText(Z0_Geography.STAsText(), 4326).STEquals(@16) = 1 OR geography::STGeomFromText(Z0_Geography.STAsText(), 4326).STEquals(@17) = 1)
	AND Z0_Guid is NULL
	AND Z0_IsSystem = @18
	AND Z0_IsValid = @19
	AND Z0_Long = @20
	AND Z0_Money = @21
	AND Z0_NAddInfo = @22
	AND Z0_Number = @23
	AND Z0_NVarChar = @24
	AND Z0_Short = @25
	AND Z0_SmallDateTime is NULL
	AND Z0_SparseBit is NULL
	AND Z0_SparseByte is NULL
	AND Z0_SparseChar is NULL
	AND Z0_SparseDate is NULL
	AND Z0_SparseDateTime is NULL
	AND Z0_SparseDateTimeOffset is NULL
	AND Z0_SparseDecimal is NULL
	AND Z0_SparseGuid is NULL
	AND Z0_SparseLong is NULL
	AND Z0_SparseMoney is NULL
	AND Z0_SparseNumber is NULL
	AND Z0_SparseNVarChar is NULL
	AND Z0_SparseShort is NULL
	AND Z0_SparseSmallDateTime is NULL
	AND Z0_SparseTime is NULL
	AND Z0_SparseVarChar is NULL
	AND Z0_Time is NULL;
SET @0 = @@ROWCOUNT;
'
, N'@0 int out, @1 uniqueidentifier, @2 varchar(4096), @3 decimal(18,3), @4 int, @5 bit, @6 bit, @7 bit, @8 char(1), @9 tinyint, @10 varchar(5), @11 varchar(5), @12 decimal(18,0), @13 varchar(100), @14 varchar(100), @15 varchar(5), @16 geography, @17 geography, @18 bit, @19 char(1), @20 bigint, @21 money, @22 nvarchar(4000), @23 int, @24 nvarchar(20), @25 smallint'
, @0 = @row_count out, @1 = '{1}', @2 = '', @3 = 0, @4 = 0, @5 = 0, @6 = 0, @7 = 1, @8 = 'N', @9 = 0, @10 = 'NCODE', @11 = 'BBB', @12 = 0, @13 = 'Default', @14 = 'new description', @15 = '', @16 = @2_16, @17 = @2_17, @18 = 1, @19 = 'N', @20 = 0, @21 = 0, @22 = N'', @23 = 0, @24 = N'', @25 = 0;
if (@row_count = 0) RAISERROR('~ConcurrencyError~', 16, 1);

END TRY
BEGIN CATCH
	DECLARE @ErrMsg nvarchar(4000) = ERROR_MESSAGE(), @Severity int = ERROR_SEVERITY(), @ErrNum int = ERROR_NUMBER();

	IF (@Severity > 18) THROW;
	ELSE
	BEGIN
		DECLARE @Msg nvarchar(4000) = '{{' + @PK + ',' + case when @ErrMsg = '~ConcurrencyError~' then 'True' else 'False' end + ',' + cast(@ErrNum as varchar(10)) + '}} ' + @ErrMsg;
		RAISERROR('%s', @Severity, 1, @Msg);
		IF (XACT_STATE()) = -1 ROLLBACK;
	END
END CATCH", pk_1, pk_2);
		}

		string Expected_Update_Literals(ZGuid pk_1, ZGuid pk_2)
		{
			return string.Format(
@"DECLARE @PK varchar(38), @row_count int;
BEGIN TRY

SET @PK = '{0}';
DECLARE @1_15 geography = geography::STGeomFromText('POINT EMPTY', 4326);
DECLARE @1_16 geography = geography::STGeomFromText('POINT (-111 11)', 4326);
UPDATE dbo.DummyBizo SET
	Z0_Code = 'AAA',
	Z0_Geography = @1_16,
	Z0_SmallDateTime = '2014-11-11 00:00:00.000'
FROM
	dbo.DummyBizo WITH (INDEX([PK_UX__Z0_PK]), UPDLOCK)
WHERE 1=1
	AND Z0_PK = '{0}'
	AND Z0_AddInfo = ''
	AND Z0_AnotherDate is NULL
	AND Z0_AnotherDecimal = 0
	AND Z0_AnotherNumber = 0
	AND Z0_BitFalse = 0
	AND Z0_BitFiltered = 0
	AND Z0_BitTrue = 1
	AND Z0_Bool = 'N'
	AND Z0_Byte = 0
	AND (Z0_Code = 'NCODE' OR Z0_Code = 'AAA')
	AND Z0_Date is NULL
	AND Z0_DateOnly is NULL
	AND Z0_DateTimeOffset is NULL
	AND Z0_Decimal = 0
	AND Z0_Description = 'Default'
	AND Z0_FK_Code = ''
	AND (geography::STGeomFromText(Z0_Geography.STAsText(), 4326).STEquals(@1_15) = 1 OR geography::STGeomFromText(Z0_Geography.STAsText(), 4326).STEquals(@1_16) = 1)
	AND Z0_Guid is NULL
	AND Z0_IsSystem = 1
	AND Z0_IsValid = 'N'
	AND Z0_Long = 0
	AND Z0_Money = 0
	AND Z0_NAddInfo = N''
	AND Z0_Number = 0
	AND Z0_NVarChar = N''
	AND Z0_Short = 0
	AND (Z0_SmallDateTime is NULL OR Z0_SmallDateTime = '2014-11-11 00:00:00.000')
	AND Z0_SparseBit is NULL
	AND Z0_SparseByte is NULL
	AND Z0_SparseChar is NULL
	AND Z0_SparseDate is NULL
	AND Z0_SparseDateTime is NULL
	AND Z0_SparseDateTimeOffset is NULL
	AND Z0_SparseDecimal is NULL
	AND Z0_SparseGuid is NULL
	AND Z0_SparseLong is NULL
	AND Z0_SparseMoney is NULL
	AND Z0_SparseNumber is NULL
	AND Z0_SparseNVarChar is NULL
	AND Z0_SparseShort is NULL
	AND Z0_SparseSmallDateTime is NULL
	AND Z0_SparseTime is NULL
	AND Z0_SparseVarChar is NULL
	AND Z0_Time is NULL;
if @@ROWCOUNT = 0 RAISERROR('~ConcurrencyError~', 16, 1);

SET @PK = '{1}';
DECLARE @2_16 geography = geography::STGeomFromText('POINT (-222 22)', 4326);
DECLARE @2_17 geography = geography::STGeomFromText('POINT EMPTY', 4326);
UPDATE dbo.DummyBizo SET
	Z0_Code = 'BBB',
	Z0_Description = 'new description',
	Z0_Geography = @2_17
FROM
	dbo.DummyBizo WITH (INDEX([PK_UX__Z0_PK]), UPDLOCK)
WHERE 1=1
	AND Z0_PK = '{1}'
	AND Z0_AddInfo = ''
	AND Z0_AnotherDate is NULL
	AND Z0_AnotherDecimal = 0
	AND Z0_AnotherNumber = 0
	AND Z0_BitFalse = 0
	AND Z0_BitFiltered = 0
	AND Z0_BitTrue = 1
	AND Z0_Bool = 'N'
	AND Z0_Byte = 0
	AND (Z0_Code = 'NCODE' OR Z0_Code = 'BBB')
	AND Z0_Date is NULL
	AND Z0_DateOnly is NULL
	AND Z0_DateTimeOffset is NULL
	AND Z0_Decimal = 0
	AND (Z0_Description = 'Default' OR Z0_Description = 'new description')
	AND Z0_FK_Code = ''
	AND (geography::STGeomFromText(Z0_Geography.STAsText(), 4326).STEquals(@2_16) = 1 OR geography::STGeomFromText(Z0_Geography.STAsText(), 4326).STEquals(@2_17) = 1)
	AND Z0_Guid is NULL
	AND Z0_IsSystem = 1
	AND Z0_IsValid = 'N'
	AND Z0_Long = 0
	AND Z0_Money = 0
	AND Z0_NAddInfo = N''
	AND Z0_Number = 0
	AND Z0_NVarChar = N''
	AND Z0_Short = 0
	AND Z0_SmallDateTime is NULL
	AND Z0_SparseBit is NULL
	AND Z0_SparseByte is NULL
	AND Z0_SparseChar is NULL
	AND Z0_SparseDate is NULL
	AND Z0_SparseDateTime is NULL
	AND Z0_SparseDateTimeOffset is NULL
	AND Z0_SparseDecimal is NULL
	AND Z0_SparseGuid is NULL
	AND Z0_SparseLong is NULL
	AND Z0_SparseMoney is NULL
	AND Z0_SparseNumber is NULL
	AND Z0_SparseNVarChar is NULL
	AND Z0_SparseShort is NULL
	AND Z0_SparseSmallDateTime is NULL
	AND Z0_SparseTime is NULL
	AND Z0_SparseVarChar is NULL
	AND Z0_Time is NULL;
if @@ROWCOUNT = 0 RAISERROR('~ConcurrencyError~', 16, 1);

END TRY
BEGIN CATCH
	DECLARE @ErrMsg nvarchar(4000) = ERROR_MESSAGE(), @Severity int = ERROR_SEVERITY(), @ErrNum int = ERROR_NUMBER();

	IF (@Severity > 18) THROW;
	ELSE
	BEGIN
		DECLARE @Msg nvarchar(4000) = '{{' + @PK + ',' + case when @ErrMsg = '~ConcurrencyError~' then 'True' else 'False' end + ',' + cast(@ErrNum as varchar(10)) + '}} ' + @ErrMsg;
		RAISERROR('%s', @Severity, 1, @Msg);
		IF (XACT_STATE()) = -1 ROLLBACK;
	END
END CATCH", pk_1, pk_2);
		}

		#endregion // Parameterized Insert and Update

		#region Parameterized Delete

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestParameterization_Delete()
		{
			var factory = new BusinessObjectFactory();

			var dummy_1 = factory.New<DummyBusinessObject>();
			dummy_1.Z0_Code = "AAA";
			dummy_1.Z0_SmallDateTime = new ZDateTime(2014, 11, 11);
			dummy_1.Z0_Geography = new ZGeography("-111 11");

			var dummy_2 = factory.New<DummyBusinessObject>();
			dummy_2.Z0_Code = "BBB";
			dummy_2.Z0_Description = "new description";

			factory.Save();

			dummy_1.Z0_SmallDateTime = ZDateTime.Empty;
			dummy_1.Z0_Description = "new desc";
			dummy_1.Z0_Geography = ZGeography.Empty;

			dummy_2.Z0_Description = "";
			dummy_2.Z0_Geography = new ZGeography("-222 22");

			dummy_1.Delete();
			dummy_2.Delete();

			var rows = new List<DataRow>()
				{
					dummy_1.Row,
					dummy_2.Row,
				};

			var saver = new DummyZSqlSaver(new DataSet(), GetNewSchemaResolver());
			var result = saver.BuildCommandExposed(rows, 0, saver.RowsToPostPerSqlStatementForTest, saver.BytesToPostPerSqlStatementForTest, canConsolidateInserts: true);
			using (var command = result.Command)
			{
				var expected = Expected_Delete_Parameters(dummy_1.PK, dummy_2.PK);
				AssertMultilineASCIIEquals("Update statement should be equal", expected, command.StandardCommandText);
			}
		}

		string Expected_Delete_Parameters(ZGuid pk_1, ZGuid pk_2)
		{
			return string.Format(CultureInfo.InvariantCulture,
@"DECLARE @PK varchar(38), @row_count int;
BEGIN TRY

SET @PK = '{0}';
DECLARE @1_18 geography = geography::STGeomFromText('POINT (-111 11)', 4326);
EXEC sys.sp_executesql N'DELETE FROM dbo.DummyBizo
WHERE 1=1
	AND Z0_PK = @1
	AND ISNULL(NULLIF(Z0_AddInfo, @2), NULLIF(@2, Z0_AddInfo)) is NULL
	AND ISNULL(NULLIF(Z0_AnotherDate, @3), NULLIF(@3, Z0_AnotherDate)) is NULL
	AND ISNULL(NULLIF(Z0_AnotherDecimal, @4), NULLIF(@4, Z0_AnotherDecimal)) is NULL
	AND ISNULL(NULLIF(Z0_AnotherNumber, @5), NULLIF(@5, Z0_AnotherNumber)) is NULL
	AND ISNULL(NULLIF(Z0_BitFalse, @6), NULLIF(@6, Z0_BitFalse)) is NULL
	AND ISNULL(NULLIF(Z0_BitFiltered, @7), NULLIF(@7, Z0_BitFiltered)) is NULL
	AND ISNULL(NULLIF(Z0_BitTrue, @8), NULLIF(@8, Z0_BitTrue)) is NULL
	AND ISNULL(NULLIF(Z0_Bool, @9), NULLIF(@9, Z0_Bool)) is NULL
	AND ISNULL(NULLIF(Z0_Byte, @10), NULLIF(@10, Z0_Byte)) is NULL
	AND ISNULL(NULLIF(Z0_Code, @11), NULLIF(@11, Z0_Code)) is NULL
	AND ISNULL(NULLIF(Z0_Date, @12), NULLIF(@12, Z0_Date)) is NULL
	AND ISNULL(NULLIF(Z0_DateOnly, @13), NULLIF(@13, Z0_DateOnly)) is NULL
	AND ISNULL(NULLIF(Z0_DateTimeOffset, @14), NULLIF(@14, Z0_DateTimeOffset)) is NULL
	AND ISNULL(NULLIF(Z0_Decimal, @15), NULLIF(@15, Z0_Decimal)) is NULL
	AND ISNULL(NULLIF(Z0_Description, @16), NULLIF(@16, Z0_Description)) is NULL
	AND ISNULL(NULLIF(Z0_FK_Code, @17), NULLIF(@17, Z0_FK_Code)) is NULL
	AND ISNULL(NULLIF(Z0_Geography.STAsText(), @18.STAsText()), NULLIF(@18.STAsText(), Z0_Geography.STAsText())) is NULL
	AND ISNULL(NULLIF(Z0_Guid, @19), NULLIF(@19, Z0_Guid)) is NULL
	AND ISNULL(NULLIF(Z0_IsSystem, @20), NULLIF(@20, Z0_IsSystem)) is NULL
	AND ISNULL(NULLIF(Z0_Long, @22), NULLIF(@22, Z0_Long)) is NULL
	AND ISNULL(NULLIF(Z0_Money, @23), NULLIF(@23, Z0_Money)) is NULL
	AND ISNULL(NULLIF(Z0_NAddInfo, @24), NULLIF(@24, Z0_NAddInfo)) is NULL
	AND ISNULL(NULLIF(Z0_Number, @25), NULLIF(@25, Z0_Number)) is NULL
	AND ISNULL(NULLIF(Z0_NVarChar, @26), NULLIF(@26, Z0_NVarChar)) is NULL
	AND ISNULL(NULLIF(Z0_Short, @28), NULLIF(@28, Z0_Short)) is NULL
	AND ISNULL(NULLIF(Z0_SmallDateTime, @29), NULLIF(@29, Z0_SmallDateTime)) is NULL
	AND ISNULL(NULLIF(Z0_SparseBit, @30), NULLIF(@30, Z0_SparseBit)) is NULL
	AND ISNULL(NULLIF(Z0_SparseByte, @31), NULLIF(@31, Z0_SparseByte)) is NULL
	AND ISNULL(NULLIF(Z0_SparseChar, @32), NULLIF(@32, Z0_SparseChar)) is NULL
	AND ISNULL(NULLIF(Z0_SparseDate, @33), NULLIF(@33, Z0_SparseDate)) is NULL
	AND ISNULL(NULLIF(Z0_SparseDateTime, @34), NULLIF(@34, Z0_SparseDateTime)) is NULL
	AND ISNULL(NULLIF(Z0_SparseDateTimeOffset, @35), NULLIF(@35, Z0_SparseDateTimeOffset)) is NULL
	AND ISNULL(NULLIF(Z0_SparseDecimal, @36), NULLIF(@36, Z0_SparseDecimal)) is NULL
	AND ISNULL(NULLIF(Z0_SparseGuid, @37), NULLIF(@37, Z0_SparseGuid)) is NULL
	AND ISNULL(NULLIF(Z0_SparseLong, @38), NULLIF(@38, Z0_SparseLong)) is NULL
	AND ISNULL(NULLIF(Z0_SparseMoney, @39), NULLIF(@39, Z0_SparseMoney)) is NULL
	AND ISNULL(NULLIF(Z0_SparseNumber, @40), NULLIF(@40, Z0_SparseNumber)) is NULL
	AND ISNULL(NULLIF(Z0_SparseNVarChar, @41), NULLIF(@41, Z0_SparseNVarChar)) is NULL
	AND ISNULL(NULLIF(Z0_SparseShort, @42), NULLIF(@42, Z0_SparseShort)) is NULL
	AND ISNULL(NULLIF(Z0_SparseSmallDateTime, @43), NULLIF(@43, Z0_SparseSmallDateTime)) is NULL
	AND ISNULL(NULLIF(Z0_SparseTime, @44), NULLIF(@44, Z0_SparseTime)) is NULL
	AND ISNULL(NULLIF(Z0_SparseVarChar, @46), NULLIF(@46, Z0_SparseVarChar)) is NULL
	AND ISNULL(NULLIF(Z0_Time, @48), NULLIF(@48, Z0_Time)) is NULL
;
SET @0 = @@ROWCOUNT;
'
, N'@0 int out, @1 uniqueidentifier, @2 varchar(4096), @3 datetime, @4 decimal(18,3), @5 int, @6 bit, @7 bit, @8 bit, @9 char(1), @10 tinyint, @11 varchar(5), @12 datetime, @13 date, @14 datetimeoffset(7), @15 decimal(18,0), @16 varchar(100), @17 varchar(5), @18 geography, @19 uniqueidentifier, @20 bit, @22 bigint, @23 money, @24 nvarchar(4000), @25 int, @26 nvarchar(20), @28 smallint, @29 smalldatetime, @30 bit, @31 tinyint, @32 varchar(5), @33 date, @34 datetime, @35 datetimeoffset(7), @36 decimal(18,3), @37 uniqueidentifier, @38 bigint, @39 money, @40 int, @41 nvarchar(20), @42 smallint, @43 smalldatetime, @44 time, @46 varchar(20), @48 time'
, @0 = @row_count out, @1 = '{0}', @2 = '', @3 = NULL, @4 = 0, @5 = 0, @6 = 0, @7 = 0, @8 = 1, @9 = 'N', @10 = 0, @11 = 'AAA', @12 = NULL, @13 = NULL, @14 = NULL, @15 = 0, @16 = 'Default', @17 = '', @18 = @1_18, @19 = NULL, @20 = 1, @22 = 0, @23 = 0, @24 = N'', @25 = 0, @26 = N'', @28 = 0, @29 = '2014-11-11 00:00:00.000', @30 = NULL, @31 = NULL, @32 = NULL, @33 = NULL, @34 = NULL, @35 = NULL, @36 = NULL, @37 = NULL, @38 = NULL, @39 = NULL, @40 = NULL, @41 = NULL, @42 = NULL, @43 = NULL, @44 = NULL, @46 = NULL, @48 = NULL;
if (@row_count = 0) RAISERROR('~ConcurrencyError~', 16, 1);

SET @PK = '{1}';
DECLARE @2_18 geography = geography::STGeomFromText('POINT EMPTY', 4326);
EXEC sys.sp_executesql N'DELETE FROM dbo.DummyBizo
WHERE 1=1
	AND Z0_PK = @1
	AND ISNULL(NULLIF(Z0_AddInfo, @2), NULLIF(@2, Z0_AddInfo)) is NULL
	AND ISNULL(NULLIF(Z0_AnotherDate, @3), NULLIF(@3, Z0_AnotherDate)) is NULL
	AND ISNULL(NULLIF(Z0_AnotherDecimal, @4), NULLIF(@4, Z0_AnotherDecimal)) is NULL
	AND ISNULL(NULLIF(Z0_AnotherNumber, @5), NULLIF(@5, Z0_AnotherNumber)) is NULL
	AND ISNULL(NULLIF(Z0_BitFalse, @6), NULLIF(@6, Z0_BitFalse)) is NULL
	AND ISNULL(NULLIF(Z0_BitFiltered, @7), NULLIF(@7, Z0_BitFiltered)) is NULL
	AND ISNULL(NULLIF(Z0_BitTrue, @8), NULLIF(@8, Z0_BitTrue)) is NULL
	AND ISNULL(NULLIF(Z0_Bool, @9), NULLIF(@9, Z0_Bool)) is NULL
	AND ISNULL(NULLIF(Z0_Byte, @10), NULLIF(@10, Z0_Byte)) is NULL
	AND ISNULL(NULLIF(Z0_Code, @11), NULLIF(@11, Z0_Code)) is NULL
	AND ISNULL(NULLIF(Z0_Date, @12), NULLIF(@12, Z0_Date)) is NULL
	AND ISNULL(NULLIF(Z0_DateOnly, @13), NULLIF(@13, Z0_DateOnly)) is NULL
	AND ISNULL(NULLIF(Z0_DateTimeOffset, @14), NULLIF(@14, Z0_DateTimeOffset)) is NULL
	AND ISNULL(NULLIF(Z0_Decimal, @15), NULLIF(@15, Z0_Decimal)) is NULL
	AND ISNULL(NULLIF(Z0_Description, @16), NULLIF(@16, Z0_Description)) is NULL
	AND ISNULL(NULLIF(Z0_FK_Code, @17), NULLIF(@17, Z0_FK_Code)) is NULL
	AND ISNULL(NULLIF(Z0_Geography.STAsText(), @18.STAsText()), NULLIF(@18.STAsText(), Z0_Geography.STAsText())) is NULL
	AND ISNULL(NULLIF(Z0_Guid, @19), NULLIF(@19, Z0_Guid)) is NULL
	AND ISNULL(NULLIF(Z0_IsSystem, @20), NULLIF(@20, Z0_IsSystem)) is NULL
	AND ISNULL(NULLIF(Z0_Long, @22), NULLIF(@22, Z0_Long)) is NULL
	AND ISNULL(NULLIF(Z0_Money, @23), NULLIF(@23, Z0_Money)) is NULL
	AND ISNULL(NULLIF(Z0_NAddInfo, @24), NULLIF(@24, Z0_NAddInfo)) is NULL
	AND ISNULL(NULLIF(Z0_Number, @25), NULLIF(@25, Z0_Number)) is NULL
	AND ISNULL(NULLIF(Z0_NVarChar, @26), NULLIF(@26, Z0_NVarChar)) is NULL
	AND ISNULL(NULLIF(Z0_Short, @28), NULLIF(@28, Z0_Short)) is NULL
	AND ISNULL(NULLIF(Z0_SmallDateTime, @29), NULLIF(@29, Z0_SmallDateTime)) is NULL
	AND ISNULL(NULLIF(Z0_SparseBit, @30), NULLIF(@30, Z0_SparseBit)) is NULL
	AND ISNULL(NULLIF(Z0_SparseByte, @31), NULLIF(@31, Z0_SparseByte)) is NULL
	AND ISNULL(NULLIF(Z0_SparseChar, @32), NULLIF(@32, Z0_SparseChar)) is NULL
	AND ISNULL(NULLIF(Z0_SparseDate, @33), NULLIF(@33, Z0_SparseDate)) is NULL
	AND ISNULL(NULLIF(Z0_SparseDateTime, @34), NULLIF(@34, Z0_SparseDateTime)) is NULL
	AND ISNULL(NULLIF(Z0_SparseDateTimeOffset, @35), NULLIF(@35, Z0_SparseDateTimeOffset)) is NULL
	AND ISNULL(NULLIF(Z0_SparseDecimal, @36), NULLIF(@36, Z0_SparseDecimal)) is NULL
	AND ISNULL(NULLIF(Z0_SparseGuid, @37), NULLIF(@37, Z0_SparseGuid)) is NULL
	AND ISNULL(NULLIF(Z0_SparseLong, @38), NULLIF(@38, Z0_SparseLong)) is NULL
	AND ISNULL(NULLIF(Z0_SparseMoney, @39), NULLIF(@39, Z0_SparseMoney)) is NULL
	AND ISNULL(NULLIF(Z0_SparseNumber, @40), NULLIF(@40, Z0_SparseNumber)) is NULL
	AND ISNULL(NULLIF(Z0_SparseNVarChar, @41), NULLIF(@41, Z0_SparseNVarChar)) is NULL
	AND ISNULL(NULLIF(Z0_SparseShort, @42), NULLIF(@42, Z0_SparseShort)) is NULL
	AND ISNULL(NULLIF(Z0_SparseSmallDateTime, @43), NULLIF(@43, Z0_SparseSmallDateTime)) is NULL
	AND ISNULL(NULLIF(Z0_SparseTime, @44), NULLIF(@44, Z0_SparseTime)) is NULL
	AND ISNULL(NULLIF(Z0_SparseVarChar, @46), NULLIF(@46, Z0_SparseVarChar)) is NULL
	AND ISNULL(NULLIF(Z0_Time, @48), NULLIF(@48, Z0_Time)) is NULL
;
SET @0 = @@ROWCOUNT;
'
, N'@0 int out, @1 uniqueidentifier, @2 varchar(4096), @3 datetime, @4 decimal(18,3), @5 int, @6 bit, @7 bit, @8 bit, @9 char(1), @10 tinyint, @11 varchar(5), @12 datetime, @13 date, @14 datetimeoffset(7), @15 decimal(18,0), @16 varchar(100), @17 varchar(5), @18 geography, @19 uniqueidentifier, @20 bit, @22 bigint, @23 money, @24 nvarchar(4000), @25 int, @26 nvarchar(20), @28 smallint, @29 smalldatetime, @30 bit, @31 tinyint, @32 varchar(5), @33 date, @34 datetime, @35 datetimeoffset(7), @36 decimal(18,3), @37 uniqueidentifier, @38 bigint, @39 money, @40 int, @41 nvarchar(20), @42 smallint, @43 smalldatetime, @44 time, @46 varchar(20), @48 time'
, @0 = @row_count out, @1 = '{1}', @2 = '', @3 = NULL, @4 = 0, @5 = 0, @6 = 0, @7 = 0, @8 = 1, @9 = 'N', @10 = 0, @11 = 'BBB', @12 = NULL, @13 = NULL, @14 = NULL, @15 = 0, @16 = 'new description', @17 = '', @18 = @2_18, @19 = NULL, @20 = 1, @22 = 0, @23 = 0, @24 = N'', @25 = 0, @26 = N'', @28 = 0, @29 = NULL, @30 = NULL, @31 = NULL, @32 = NULL, @33 = NULL, @34 = NULL, @35 = NULL, @36 = NULL, @37 = NULL, @38 = NULL, @39 = NULL, @40 = NULL, @41 = NULL, @42 = NULL, @43 = NULL, @44 = NULL, @46 = NULL, @48 = NULL;
if (@row_count = 0) RAISERROR('~ConcurrencyError~', 16, 1);

END TRY
BEGIN CATCH
	DECLARE @ErrMsg nvarchar(4000) = ERROR_MESSAGE(), @Severity int = ERROR_SEVERITY(), @ErrNum int = ERROR_NUMBER();

	IF (@Severity > 18) THROW;
	ELSE
	BEGIN
		DECLARE @Msg nvarchar(4000) = '{{' + @PK + ',' + case when @ErrMsg = '~ConcurrencyError~' then 'True' else 'False' end + ',' + cast(@ErrNum as varchar(10)) + '}} ' + @ErrMsg;
		RAISERROR('%s', @Severity, 1, @Msg);
		IF (XACT_STATE()) = -1 ROLLBACK;
	END
END CATCH
", pk_1, pk_2);
		}

		#endregion // Parameterized Delete

		#region Implementation

		ZSqlConnectionInfo ConnectionInfo => new ZSqlConnectionInfo(Db.Connection, "");

		protected override void SetUp()
		{
			base.SetUp();
			AssertEquals("Initial state - no dummies", 0, GetDummyCountInDB());
		}

		IApplicationSchemaResolver GetNewSchemaResolver() => ObjectFactory.Get<IApplicationSchemaResolver>();

		#endregion
	}

	#region ZSqlSaverWithComplexSaveTest

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
	sealed class ZSqlSaverWithComplexSaveTest : TransactionedTestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		void DeleteTestRows(DataSet dataSetToPost)
		{
			dataSetToPost.Tables["TestTable1"].Rows[0].Delete();
			dataSetToPost.Tables["TestTable2"].Rows[0].Delete();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		void InsertAndUpdateTestRows(DataSet dataSetToPost, Guid table1NewPK, Guid table2NewPK)
		{
			DataRow t1Row = dataSetToPost.Tables["TestTable1"].NewRow();
			t1Row["T1_PK"] = table1NewPK;
			t1Row["T1_string"] = string.Empty;
			dataSetToPost.Tables["TestTable1"].Rows.Add(t1Row);

			DataRow t2Row = dataSetToPost.Tables["TestTable2"].NewRow();
			t2Row["T2_PK"] = table2NewPK;
			dataSetToPost.Tables["TestTable2"].Rows.Add(t2Row);

			t1Row["T1_T2"] = table2NewPK;
			t2Row["T2_T3"] = DBNull.Value;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		DataSet CreateTestDataSet(bool enableBulkCopy)
		{
			DataSet result = new DataSet();
			DataTable table1 = new DataTable("TestTable1");
			DataColumn testColumnT1_PK = new DataColumn("T1_PK", typeof(Guid));
			DataColumn testColumnT1_T2 = new DataColumn("T1_T2", typeof(Guid));
			DataTable table2 = new DataTable("TestTable2");
			DataColumn testColumnT2_PK = new DataColumn("T2_PK", typeof(Guid));
			DataColumn testColumnT2_T3 = new DataColumn("T2_T3", typeof(Guid));
			DataTable table3 = new DataTable("TestTable3");
			DataColumn testColumnT3_PK = new DataColumn("T3_PK", typeof(Guid));
			DataColumn testColumnT3_T4 = new DataColumn("T3_T4", typeof(Guid));
			DataTable table4 = new DataTable("TestTable4");
			DataColumn testColumnT4_PK = new DataColumn("T4_PK", typeof(Guid));

			table1.Columns.Add(testColumnT1_PK);
			table1.Columns.Add(testColumnT1_T2);
			table2.Columns.Add(testColumnT2_PK);
			table2.Columns.Add(testColumnT2_T3);
			table3.Columns.Add(testColumnT3_PK);
			table3.Columns.Add(testColumnT3_T4);
			table4.Columns.Add(testColumnT4_PK);

			DataColumn testColumnT1_string = new DataColumn("T1_string");
			table1.Columns.Add(testColumnT1_string);

			table1.PrimaryKey = new DataColumn[] { testColumnT1_PK };
			table2.PrimaryKey = new DataColumn[] { testColumnT2_PK };
			table3.PrimaryKey = new DataColumn[] { testColumnT3_PK };
			table4.PrimaryKey = new DataColumn[] { testColumnT4_PK };

			result.Tables.Add(table1);
			result.Tables.Add(table2);
			result.Tables.Add(table3);
			result.Tables.Add(table4);

			DataRow t1Row = table1.NewRow();
			t1Row["T1_PK"] = Guid.NewGuid();
			t1Row["T1_string"] = string.Empty;

			DataRow t2Row = table2.NewRow();
			t2Row["T2_PK"] = Guid.NewGuid();
			t1Row["T1_T2"] = t2Row["T2_PK"];

			DataRow t3Row = table3.NewRow();
			t3Row["T3_PK"] = Guid.NewGuid();
			t2Row["T2_T3"] = t3Row["T3_PK"];

			DataRow t4Row = table4.NewRow();
			t4Row["T4_PK"] = Guid.NewGuid();
			t3Row["T3_T4"] = t4Row["T4_PK"];
			table4.Rows.Add(t4Row);
			table3.Rows.Add(t3Row);
			table2.Rows.Add(t2Row);
			table1.Rows.Add(t1Row);

			if (enableBulkCopy)
			{
				var bulkCopySetting = new BulkCopySetting(1, 10000, true);
				table2.ExtendedProperties[typeof(BulkCopySetting)] = bulkCopySetting;
				table4.ExtendedProperties[typeof(BulkCopySetting)] = bulkCopySetting;
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		DataSet CreateSelfRelationDataSet()
		{
			DataSet result = new DataSet();
			DataTable selfRelationDataTable = result.Tables.Add("TestSelfRelationTable");
			DataColumn t0_PKColumn = new DataColumn("T0_PK", typeof(Guid));
			DataColumn t0_T0Column = new DataColumn("T0_T0", typeof(Guid));
			selfRelationDataTable.Columns.Add(t0_PKColumn);
			selfRelationDataTable.Columns.Add(t0_T0Column);
			selfRelationDataTable.PrimaryKey = new DataColumn[] { t0_PKColumn };
			return result;
		}

		readonly Guid PK1 = Guid.NewGuid();
		readonly Guid PK2 = Guid.NewGuid();
		readonly Guid PK3 = Guid.NewGuid();
		readonly Guid PK4 = Guid.NewGuid();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		void FillDataSetInsert(DataSet selfRelationDataSet, bool enableBulkCopy = false)
		{
			if (enableBulkCopy)
			{
				var bulkCopySetting = new BulkCopySetting(1, 10000, true);
				selfRelationDataSet.Tables[0].ExtendedProperties[typeof(BulkCopySetting)] = bulkCopySetting;
			}

			selfRelationDataSet.Tables[0].Rows.Add(new object[] { PK2, PK1 });
			selfRelationDataSet.Tables[0].Rows.Add(new object[] { PK3, PK2 });
			selfRelationDataSet.Tables[0].Rows.Add(new object[] { PK1, DBNull.Value });
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		void ModifyTestDataSetForUpdate(DataSet selfRelationDataSet)
		{
			// Row0 -> Row1
			// Row1 -> ____
			// Row2 -> Row0
			selfRelationDataSet.Tables[0].Rows[0]["T0_T0"] = PK3;
			selfRelationDataSet.Tables[0].Rows[1]["T0_T0"] = DBNull.Value;
			selfRelationDataSet.Tables[0].Rows[2]["T0_T0"] = PK2;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		void ModifyTestDataSetForDelete(DataSet selfRelationDataSet)
		{
			selfRelationDataSet.Tables[0].Rows[0].Delete();
			selfRelationDataSet.Tables[0].Rows[1].Delete();
			selfRelationDataSet.Tables[0].Rows[2].Delete();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		void ModifyTestDataSetForUpdateDelete(DataSet selfRelationDataSet)
		{
			selfRelationDataSet.Tables[0].Rows[0]["T0_T0"] = DBNull.Value;
			selfRelationDataSet.Tables[0].Rows[1].Delete();
			selfRelationDataSet.Tables[0].Rows[2]["T0_T0"] = PK2;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		void ModifyTestDataSetForInsertDelete(DataSet selfRelationDataSet)
		{
			selfRelationDataSet.Tables[0].Rows[1].Delete();
			selfRelationDataSet.Tables[0].Rows.Add(new object[] { PK4, PK2 });
		}

		public void TestWriteToDatabase()
		{
			AssertWriteToDatabase();
		}

		public void TestWriteToDatabaseThroughBulkInsert()
		{
			AssertWriteToDatabase(true);
		}

		void AssertWriteToDatabase(bool enableBulkCopy = false)
		{
			var dataSetToPost = CreateTestDataSet(enableBulkCopy);
			var saver = new ZSqlSaver(dataSetToPost, ConnectionInfo, GetNewSchemaResolver());

			if (enableBulkCopy)
			{
				using (var sqlBulkCopyEventTracker = new SqlBulkCopyEventTracker())
				{
					SaveAndAssertInsertedRows();
					Assert("TestTable2 performed bulk insert", sqlBulkCopyEventTracker.HasBulkCopyEvent("TestTable2"));
					Assert("TestTable4 performed bulk insert", sqlBulkCopyEventTracker.HasBulkCopyEvent("TestTable4"));
				}
			}
			else
			{
				SaveAndAssertInsertedRows();
			}

			// Test post with inserts, updates and deletes
			var table1DeletedRowPK = (Guid)dataSetToPost.Tables["TestTable1"].Rows[0]["T1_PK"];
			var table2DeletedRowPK = (Guid)dataSetToPost.Tables["TestTable2"].Rows[0]["T2_PK"];
			var table1NewPK = Guid.NewGuid();
			var table2NewPK = Guid.NewGuid();

			DeleteTestRows(dataSetToPost);
			InsertAndUpdateTestRows(dataSetToPost, table1NewPK, table2NewPK);

			saver.Save();

			Assert("Test Table1 old row Deleted", !IsRowInDB("TestTable1", "T1_PK", table1DeletedRowPK));
			Assert("Test Table2 old row Deleted", !IsRowInDB("TestTable2", "T2_PK", table2DeletedRowPK));

			Assert("TestTable1 new row inserted", IsRowInDB("TestTable1", "T1_PK", table1NewPK));
			Assert("TestTable2 new row inserted", IsRowInDB("TestTable2", "T2_PK", table2NewPK));

			AssertEquals("TestTable1 Update", table2NewPK, (Guid)GetFKValue("TestTable1", "T1_PK", table1NewPK, "T1_T2"));
			AssertEquals("TestTable2 Update", DBNull.Value, GetFKValue("TestTable2", "T2_PK", table2NewPK, "T2_T3"));

			void SaveAndAssertInsertedRows()
			{
				// Test initial post - only inserts
				saver.Save();
				dataSetToPost.AcceptChanges();

				Assert("TestTable1 row inserted", IsRowInDB("TestTable1", "T1_PK", (Guid)dataSetToPost.Tables["TestTable1"].Rows[0]["T1_PK"]));
				Assert("TestTable2 row inserted", IsRowInDB("TestTable2", "T2_PK", (Guid)dataSetToPost.Tables["TestTable2"].Rows[0]["T2_PK"]));
				Assert("TestTable3 row inserted", IsRowInDB("TestTable3", "T3_PK", (Guid)dataSetToPost.Tables["TestTable3"].Rows[0]["T3_PK"]));
				Assert("TestTable4 row inserted", IsRowInDB("TestTable4", "T4_PK", (Guid)dataSetToPost.Tables["TestTable4"].Rows[0]["T4_PK"]));
			}
		}

		public void TestSelfRelationInsert()
		{
			DataSet testDataSet = CreateSelfRelationDataSet();
			FillDataSetInsert(testDataSet);
			ZSqlSaver saver = new ZSqlSaver(testDataSet, ConnectionInfo, GetNewSchemaResolver());
			saver.Save();
			testDataSet.AcceptChanges();
			AssertSelfRelationRowCount("Insert", 3);
		}

		public void TestSelfRelationBulkInsert()
		{
			DataSet testDataSet = CreateSelfRelationDataSet();
			FillDataSetInsert(testDataSet, true);
			ZSqlSaver saver = new ZSqlSaver(testDataSet, ConnectionInfo, GetNewSchemaResolver());
			saver.Save();
			testDataSet.AcceptChanges();
			AssertSelfRelationRowCount("Insert", 3);
		}

		public void TestSelfRelationUpdate()
		{
			DataSet testDataSet = CreateSelfRelationDataSet();
			FillDataSetInsert(testDataSet);
			ZSqlSaver saver = new ZSqlSaver(testDataSet, ConnectionInfo, GetNewSchemaResolver());
			saver.Save();
			testDataSet.AcceptChanges();
			AssertSelfRelationRowCount("Insert", 3);
			ModifyTestDataSetForUpdate(testDataSet);
			saver.Save();
			AssertSelfRelationRowCount("Update", 3);
		}

		public void TestSelfRelationDelete()
		{
			DataSet testDataSet = CreateSelfRelationDataSet();
			FillDataSetInsert(testDataSet);
			ZSqlSaver saver = new ZSqlSaver(testDataSet, ConnectionInfo, GetNewSchemaResolver());
			saver.Save();
			testDataSet.AcceptChanges();
			ModifyTestDataSetForDelete(testDataSet);
			saver.Save();
			AssertSelfRelationRowCount("Delete", 0);
		}

		public void TestSelfRelationUpdateDelete()
		{
			DataSet testDataSet = CreateSelfRelationDataSet();
			FillDataSetInsert(testDataSet);
			ZSqlSaver saver = new ZSqlSaver(testDataSet, ConnectionInfo, GetNewSchemaResolver());
			saver.Save();
			testDataSet.AcceptChanges();
			ModifyTestDataSetForUpdateDelete(testDataSet);
			saver.Save();
			AssertSelfRelationRowCount("Update and Delete", 2);
		}

		public void TestSelfRelationInsertDelete()
		{
			DataSet testDataSet = CreateSelfRelationDataSet();
			FillDataSetInsert(testDataSet);
			ZSqlSaver saver = new ZSqlSaver(testDataSet, ConnectionInfo, GetNewSchemaResolver());
			saver.Save();
			testDataSet.AcceptChanges();
			ModifyTestDataSetForInsertDelete(testDataSet);
			saver.Save();
			AssertSelfRelationRowCount("Insert and Delete", 3);
		}

		#region Implementation

		object GetFKValue(string tableName, string pKName, Guid pKValue, string fKColumnName)
		{
			return Db.Connection.ExecuteScalar("SELECT " + fKColumnName + " FROM " + tableName + " WHERE " + pKName +
					" = '" + pKValue.ToString() + "'");
		}

		bool IsRowInDB(string tableName, string pKName, Guid pKValue)
		{
			int returnedCount = (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM " + tableName + " WHERE " + pKName +
					" = '" + pKValue.ToString() + "'");
			return (returnedCount > 0);
		}

		void CreateTestDataBaseSchema()
		{
			string sqlText = @"
					CREATE TABLE TestTable1 (T1_PK UniqueIdentifier NOT NULL, T1_T2 UniqueIdentifier NULL, T1_STRING char(100))
					ALTER TABLE TestTable1 Add Constraint PrimaryKeyT1 Primary Key (T1_PK)
					CREATE TABLE TestTable2 (T2_PK UniqueIdentifier NOT NULL, T2_T3 UniqueIdentifier NULL)
					ALTER TABLE TestTable2 Add Constraint PrimaryKeyT2 Primary Key (T2_PK)
					CREATE TABLE TestTable3 (T3_PK UniqueIdentifier  NOT NULL, T3_T4 UniqueIdentifier NOT NULL)
					ALTER TABLE TestTable3 Add Constraint PrimaryKeyT3 Primary Key (T3_PK)
					CREATE TABLE TestTable4 (T4_PK UniqueIdentifier NOT NULL)
					ALTER TABLE TestTable4 Add Constraint PrimaryKeyT4 Primary Key (T4_PK)
					ALTER TABLE TestTable1 Add Constraint ForeignKeyT1_T2 Foreign Key (T1_T2) References TestTable2 (T2_PK)
					ALTER TABLE TestTable2 Add Constraint ForeignKeyT2_T3 Foreign Key (T2_T3) References TestTable3 (T3_PK)
					ALTER TABLE TestTable3 Add Constraint ForeignKeyT3_T4 Foreign Key (T3_T4) References TestTable4 (T4_PK)

					CREATE TABLE TestTableWithDate (T5_PK UniqueIdentifier NOT NULL, T5_Date DateTime NOT NULL)
					";

			Db.Connection.ExecuteNonQuery(sqlText);
		}

		void CreateTestSelfRelationDataBaseSchema()
		{
			string sqlText = @"
					CREATE TABLE TestSelfRelationTable (T0_PK UniqueIdentifier NOT NULL, T0_T0 UniqueIdentifier NULL)
					ALTER TABLE TestSelfRelationTable Add Constraint SRPrimary Primary Key (T0_PK)
					ALTER TABLE TestSelfRelationTable Add Constraint SRForeignKey Foreign Key (T0_T0) References TestSelfRelationTable (T0_PK)
					";

			Db.Connection.ExecuteNonQuery(sqlText);
		}

		int GetTestSelfRelationsRowCount()
		{
			string sqlText = @"SELECT COUNT(*) from TestSelfRelationTable";
			return (int)Db.Connection.ExecuteScalar(sqlText);
		}

		void AssertSelfRelationRowCount(string action, int rowCount)
		{
			AssertEquals("Failed to complete action : " + action + ".  Row Count incorrect", rowCount, GetTestSelfRelationsRowCount());
		}

		protected override void TearDown()
		{
			resolverDisposable.Dispose();
		}

		Mock<IApplicationSchemaResolver> resolver;
		IDisposable resolverDisposable;

		ZSqlConnectionInfo ConnectionInfo => new ZSqlConnectionInfo(Db.Connection, "");

		protected override void SetUp()
		{
			base.SetUp();

			var originalResolver = ObjectFactory.Get<IApplicationSchemaResolver>();
			resolver = new Mock<IApplicationSchemaResolver>(MockBehavior.Strict);

			Func<string, ITableSchema> getTableSchema = (string tableName) =>
			{
				switch (tableName)
				{
					case "TestTable1":
						return TestTable1Schema.Instance;
					case "TestTable2":
						return TestTable2Schema.Instance;
					case "TestTable3":
						return TestTable3Schema.Instance;
					case "TestTable4":
						return TestTable4Schema.Instance;
					case "TestSelfRelationTable":
						return TestSelfRelationTableSchema.Instance;
					default:
						return originalResolver.GetTableSchema(tableName);
				}
			};

			Func<string, string, SchemaColumn> getSchemaColumn = (string columnName, string tableName) =>
			{
				SchemaColumn result = null;

				ITableSchema tableSchema = getTableSchema(tableName);
				if (tableSchema != null)
				{
					result = tableSchema.GetSchemaColumn(columnName);
				}
				return result;
			};

			Func<string, string, bool> schemaColumnExists = (string columnName, string tableName) =>
			{
				return getSchemaColumn(columnName, tableName) != null;
			};

			Func<string, SchemaColumnCollection> getSchemaColumns = (string tableName) =>
			{
				ITableSchema tableSchema = getTableSchema(tableName);
				if (tableSchema == null)
				{
					throw new InvalidTableNameException(tableName);
				}
				else
				{
					return tableSchema.All;
				}
			};

			resolver.Setup(x => x.GetSchemaColumn(It.IsAny<string>(), It.IsAny<string>())).Returns(getSchemaColumn);
			resolver.Setup(x => x.GetSchemaColumnSafe(It.IsAny<string>(), It.IsAny<string>())).Returns(getSchemaColumn);
			resolver.Setup(x => x.GetSchemaColumns(It.IsAny<string>())).Returns(getSchemaColumns);
			resolver.Setup(x => x.SchemaColumnExists(It.IsAny<string>(), It.IsAny<string>())).Returns(schemaColumnExists);
			resolver.Setup(x => x.GetTableSchema(It.IsAny<string>())).Returns(getTableSchema);
			resolverDisposable = ObjectFactory.Substitute(resolver.Object);

			CreateTestDataBaseSchema();
			CreateTestSelfRelationDataBaseSchema();
		}

		IApplicationSchemaResolver GetNewSchemaResolver() => ObjectFactory.Get<IApplicationSchemaResolver>();

		#endregion
	}

	#endregion
}
