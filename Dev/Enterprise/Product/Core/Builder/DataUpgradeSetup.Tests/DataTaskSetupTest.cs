using System;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.BuildTools;
using CargoWise.BuildTools.Testing;
using CargoWise.Data;
using Enterprise.DbUpgrader.Data.Testing;
using NUnit.Framework;

namespace Enterprise.Builder.DataUpgradeSetup.Testing
{
	public class DataTaskSetupTest : TransactionedTestCase
	{
		public void TestCheckOutWithoutCheckingOutControllerThrowsException()
		{
			try
			{
				TestController.TestTaskSetup.CheckOut();
				Fail("ControllerNotCheckedOutByMeException is expected to be thrown, but was not.");
			}
			catch (ControllerNotCheckedOutByMeException)
			{
				Assert("Exception expected", true);
			}
		}

		public void TestNotCheckedOutByMeExceptions()
		{
			int exceptionsCaught = 0;

			try
			{
				UndoCheckOutWithoutCheckingOutControllerThrowsException();
			}
			catch (ControllerNotCheckedOutByMeException)
			{
				exceptionsCaught++;
			}

			try
			{
				SaveDataToSourceFileWithoutCheckingOutControllerThrowsException();
			}
			catch (ControllerNotCheckedOutByMeException)
			{
				exceptionsCaught++;
			}

			AssertEquals("Exception count - each method should have thrown ControllerNotCheckedOutByMeException", 2, exceptionsCaught);
		}

		void UndoCheckOutWithoutCheckingOutControllerThrowsException()
		{
			TestController.TestTaskSetup.UndoCheckOut();
		}

		void SaveDataToSourceFileWithoutCheckingOutControllerThrowsException()
		{
			TestController.TestTaskSetup.SaveDataToSourceFile();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCheckOut()
		{
			AssertEquals("Is Controller Checked-out by me (1)", false, TestController.IsCheckedOutByMe);
			AssertEquals("Is DataFile Checked-out by me (1)", false, TestController.TestTaskSetup.IsCheckedOutByMe);
			AssertEquals("Task Setup State (1)", TaskStateEnum.Start, TestController.TestTaskSetup.State);

			DataSet retrievedData = TestController.TestDataFile.LoadDataFromDatabase();
			AssertEquals("Table Count", 3, retrievedData.Tables.Count);
			AssertEquals("Row Count", 0, retrievedData.Tables["StmMenuItem"].Rows.Count);

			TestController.CheckOut();
			TestController.TestTaskSetup.CheckOut();

			AssertEquals("Is Controller Checked-out by me (2)", true, TestController.IsCheckedOutByMe);
			AssertEquals("Is DataFile Checked-out by me (2)", true, TestController.TestTaskSetup.IsCheckedOutByMe);
			AssertEquals("Task Setup State (2)", TaskStateEnum.OutUnchanged, TestController.TestTaskSetup.State);

			retrievedData = TestController.TestDataFile.LoadDataFromDatabase();
			AssertEquals("StmMenuItem Row Count", 1, retrievedData.Tables["StmMenuItem"].Rows.Count);
			AssertEquals("StmTemplate Row Count", 3, retrievedData.Tables["StmTemplate"].Rows.Count);
			AssertEquals("StmMenuTemplatePivot Row Count", 1, retrievedData.Tables["StmMenuTemplatePivot"].Rows.Count);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCheckOutNonExclusively()
		{
			AssertEquals("Is Controller Checked-out by me (1)", false, TestController.IsCheckedOutByMe);
			AssertEquals("Is DataFile Checked-out by me (1)", false, TestController.TestTaskSetup.IsCheckedOutByMe);
			AssertEquals("Task Setup State (1)", TaskStateEnum.Start, TestController.TestTaskSetup.State);

			DataSet retrievedData = TestController.TestDataFile.LoadDataFromDatabase();
			AssertEquals("Table Count", 3, retrievedData.Tables.Count);
			AssertEquals("Row Count", 0, retrievedData.Tables["StmMenuItem"].Rows.Count);

			TestController.CheckOut();
			TestController.TestTaskSetup.CheckOut();

			AssertEquals("Is Controller Checked-out by me (2)", true, TestController.IsCheckedOutByMe);
			AssertEquals("Is DataFile Checked-out by me (2)", true, TestController.TestTaskSetup.IsCheckedOutByMe);
			AssertEquals("Task Setup State (2)", TaskStateEnum.OutUnchanged, TestController.TestTaskSetup.State);

			retrievedData = TestController.TestDataFile.LoadDataFromDatabase();
			AssertEquals("StmMenuItem Row Count", 1, retrievedData.Tables["StmMenuItem"].Rows.Count);
			AssertEquals("StmTemplate Row Count", 3, retrievedData.Tables["StmTemplate"].Rows.Count);
			AssertEquals("StmMenuTemplatePivot Row Count", 1, retrievedData.Tables["StmMenuTemplatePivot"].Rows.Count);
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUndoCheckOut()
		{
			AssertEquals("Is Controller Checked-out by me (1)", false, TestController.IsCheckedOutByMe);
			AssertEquals("Is DataFile Checked-out by me (1)", false, TestController.TestTaskSetup.IsCheckedOutByMe);
			AssertEquals("Task Setup State (1)", TaskStateEnum.Start, TestController.TestTaskSetup.State);

			TestController.CheckOut();
			TestController.TestTaskSetup.CheckOut();

			AssertEquals("Is Controller Checked-out by me (2)", true, TestController.IsCheckedOutByMe);
			AssertEquals("Is DataFile Checked-out by me (2)", true, TestController.TestTaskSetup.IsCheckedOutByMe);
			AssertEquals("Task Setup State (2)", TaskStateEnum.OutUnchanged, TestController.TestTaskSetup.State);

			string sqlText = @"
												DELETE dbo.StmMenuTemplatePivot
												DELETE dbo.StmMenuItem
												DELETE dbo.StmTemplate";
			Db.Connection.ExecuteNonQuery(sqlText);

			TestController.TestTaskSetup.SaveDataToSourceFile();

			DataSet data = TestController.TestTaskSetup.ExposedLoadDataFromSourceFile();
			AssertEquals("Table Count", 3, data.Tables.Count);
			AssertEquals("StmMenuItem Row Count", 0, data.Tables["StmMenuItem"].Rows.Count);
			AssertEquals("StmTemplate Row Count", 0, data.Tables["StmTemplate"].Rows.Count);
			AssertEquals("StmMenuTemplatePivot Row Count", 0, data.Tables["StmMenuTemplatePivot"].Rows.Count);

			TestController.TestTaskSetup.UndoCheckOut();
			AssertEquals("Is Controller Checked-out by me (3)", true, TestController.IsCheckedOutByMe);
			AssertEquals("Is DataFile Checked-out by me (3)", false, TestController.TestTaskSetup.IsCheckedOutByMe);
			AssertEquals("Task Setup State (3)", TaskStateEnum.InUnchanged, TestController.TestTaskSetup.State);

			DataSet dataAfterUndo = TestController.TestTaskSetup.ExposedLoadDataFromSourceFile();
			AssertEquals("StmMenuItem Row Count", 1, dataAfterUndo.Tables["StmMenuItem"].Rows.Count);
			AssertEquals("StmTemplate Row Count", 3, dataAfterUndo.Tables["StmTemplate"].Rows.Count);
			AssertEquals("StmMenuTemplatePivot Row Count", 1, dataAfterUndo.Tables["StmMenuTemplatePivot"].Rows.Count);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveDataToSourceFile()
		{
			TestController.CheckOut();
			TestController.TestTaskSetup.CheckOut();

			string sqlText = @"
															UPDATE dbo.StmMenuItem SET SU_MenuName = 'TestCheckIn Menu'
															UPDATE dbo.StmTemplate SET SO_DataContext = 'TestCheckIn Context'
															DELETE dbo.StmMenuTemplatePivot";
			Db.Connection.ExecuteNonQuery(sqlText);

			AssertEquals("Task Setup State (1)", TaskStateEnum.OutUnchanged, TestController.TestTaskSetup.State);
			AssertEquals("CurrentDataFileVersionNumber", 0, TestController.TestTaskSetup.ExposedVersionInXmlSourceFile);

			TestController.TestTaskSetup.SaveDataToSourceFile();

			AssertEquals("NewDataFileVersionNumber", 1, TestController.TestTaskSetup.ExposedVersionInXmlSourceFile);
			AssertEquals("Task Setup State (2)", TaskStateEnum.OutModified, TestController.TestTaskSetup.State);

			DataSet data = TestController.TestTaskSetup.ExposedLoadDataFromSourceFile();
			AssertEquals("Table Count", 3, data.Tables.Count);
			AssertEquals("Menu Name", "TestCheckIn Menu", data.Tables["StmMenuItem"].Rows[0]["SU_MenuName"].ToString());
			AssertEquals("Template2 Context", "TestCheckIn Context", data.Tables["StmTemplate"].Rows[0]["SO_DataContext"].ToString());
			AssertEquals("Template2 Context", "TestCheckIn Context", data.Tables["StmTemplate"].Rows[1]["SO_DataContext"].ToString());
			AssertEquals("Template3 Context", "TestCheckIn Context", data.Tables["StmTemplate"].Rows[2]["SO_DataContext"].ToString());
			AssertEquals("StmMenuTemplatePivot Row Count", 0, data.Tables["StmMenuTemplatePivot"].Rows.Count);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveDataToSourceFile_AuditAndAutoVersionAndRowVersionColumnsAreNotIncluded()
		{
			TestController.CheckOut();
			TestController.TestTaskSetup.CheckOut();

			var sqlText = @"
UPDATE dbo.StmMenuItem SET SU_MenuName = 'TestCheckIn Menu'
UPDATE dbo.StmTemplate SET SO_DataContext = 'TestCheckIn Context'
DELETE dbo.StmMenuTemplatePivot";
			Db.Connection.ExecuteNonQuery(sqlText);
			Db.Connection.ExecuteNonQuery(@"
ALTER TABLE dbo.StmMenuItem ADD SU_AutoVersion smallint NOT NULL DEFAULT 0
ALTER TABLE dbo.StmTemplate ADD SO_AutoVersion smallint NOT NULL DEFAULT 0
ALTER TABLE dbo.StmMenuTemplatePivot ADD SI_AutoVersion smallint NOT NULL DEFAULT 0");

			AssertEquals("Task Setup State (1)", TaskStateEnum.OutUnchanged, TestController.TestTaskSetup.State);
			AssertEquals("CurrentDataFileVersionNumber", 0, TestController.TestTaskSetup.ExposedVersionInXmlSourceFile);

			TestController.TestTaskSetup.SaveDataToSourceFile();

			AssertEquals("NewDataFileVersionNumber", 1, TestController.TestTaskSetup.ExposedVersionInXmlSourceFile);
			AssertEquals("Task Setup State (2)", TaskStateEnum.OutModified, TestController.TestTaskSetup.State);

			DataSet data = TestController.TestTaskSetup.ExposedLoadDataFromSourceFile();
			AssertEquals("Table Count", 3, data.Tables.Count);

			var stmMenuItemTable = data.Tables["StmMenuItem"];
			var stmTemplateTable = data.Tables["StmTemplate"];
			var stmMenuTemplatePivotTable = data.Tables["StmMenuTemplatePivot"];
			var columnsToExclude = new[]
			{
			"_AutoVersion",
			"_RowVersion",
			"_SystemCreateTimeUtc",
			"_SystemCreateUser",
			"_SystemLastEditTimeUtc",
			"_SystemLastEditUser",
		};

			CombineAssertions(() =>
			{
				foreach (var column in columnsToExclude)
				{
					AssertNull($"{stmMenuItemTable.TableName}-{column}",
						stmMenuItemTable.Columns.Cast<DataColumn>().SingleOrDefault(c => c.ColumnName.EndsWith(column, StringComparison.InvariantCulture)));
					AssertNull($"{stmTemplateTable.TableName}-{column}",
						stmTemplateTable.Columns.Cast<DataColumn>().SingleOrDefault(c => c.ColumnName.EndsWith(column, StringComparison.InvariantCulture)));
					AssertNull($"{stmMenuTemplatePivotTable.TableName}-{column}",
						stmMenuTemplatePivotTable.Columns.Cast<DataColumn>().SingleOrDefault(c => c.ColumnName.EndsWith(column, StringComparison.InvariantCulture)));
				}
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveDataToCompressedFile()
		{
			File.Copy(TestFileConstants.DefaultTestDataFileBasePath + @"Shared.Test\TestFiles\TestDataFile.xml.gz", FilePathHelperTest.CompressedTestDataFilePath);
			SourceControl.EnterpriseDatabase.AddFile(FilePathHelperTest.CompressedTestDataFilePath);

			DataUpgradeSetupControllerCompressedForTest testController = new DataUpgradeSetupControllerCompressedForTest();

			testController.CheckOut();
			testController.TestTaskSetup.CheckOut();

			string sqlText = @"
															UPDATE dbo.StmMenuItem SET SU_MenuName = 'TestCheckIn Menu'
															UPDATE dbo.StmTemplate SET SO_DataContext = 'TestCheckIn Context'
															DELETE dbo.StmMenuTemplatePivot";
			Db.Connection.ExecuteNonQuery(sqlText);

			AssertEquals("Task Setup State (1)", TaskStateEnum.OutUnchanged, testController.TestTaskSetup.State);
			AssertEquals("CurrentDataFileVersionNumber", 0, testController.TestTaskSetup.ExposedVersionInXmlSourceFile);

			testController.TestTaskSetup.SaveDataToSourceFile();

			AssertEquals("NewDataFileVersionNumber", 1, testController.TestTaskSetup.ExposedVersionInXmlSourceFile);
			AssertEquals("Task Setup State (2)", TaskStateEnum.OutModified, testController.TestTaskSetup.State);

			DataSet data = testController.TestTaskSetup.ExposedLoadDataFromSourceFile();
			AssertEquals("Table Count", 3, data.Tables.Count);
			AssertEquals("Menu Name", "TestCheckIn Menu", data.Tables["StmMenuItem"].Rows[0]["SU_MenuName"].ToString());
			AssertEquals("Template2 Context", "TestCheckIn Context", data.Tables["StmTemplate"].Rows[0]["SO_DataContext"].ToString());
			AssertEquals("Template2 Context", "TestCheckIn Context", data.Tables["StmTemplate"].Rows[1]["SO_DataContext"].ToString());
			AssertEquals("Template3 Context", "TestCheckIn Context", data.Tables["StmTemplate"].Rows[2]["SO_DataContext"].ToString());
			AssertEquals("StmMenuTemplatePivot Row Count", 0, data.Tables["StmMenuTemplatePivot"].Rows.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestVersionNumberOnlyChangesIfXmlChanges()
		{
			TestController.CheckOut();
			TestController.TestTaskSetup.CheckOut();
			//TestController.TestTaskSetup.SaveDataToSourceFile();

			// ---------------------------------------------------
			// Modifing data: version number should be incremented
			// ---------------------------------------------------
			string sqlText = @"
															UPDATE dbo.StmMenuItem SET SU_MenuName = 'TestCheckIn Menu'
															UPDATE dbo.StmTemplate SET SO_DataContext = 'TestCheckIn Context'
															DELETE dbo.StmMenuTemplatePivot";
			Db.Connection.ExecuteNonQuery(sqlText);

			int oldVersion = TestController.TestTaskSetup.ExposedVersionInXmlSourceFile;
			TestController.TestTaskSetup.SaveDataToSourceFile();
			AssertEquals("NewDataFileVersionNumber", oldVersion + 1, TestController.TestTaskSetup.ExposedVersionInXmlSourceFile);

			//TestController.TestTaskSetup.CheckOut();
			//TestController.TestTaskSetup.SaveDataToSourceFile();
			//OldVersion = TestController.TestTaskSetup.ExposedVersionInXmlSourceFile;

			//// ---------------------------------------------------------
			//// No data changes: version number should NOT be incremented
			//// ---------------------------------------------------------
			//AssertEquals("Task Setup State (1)", TaskStateEnum.OutUnchanged, TestController.TestTaskSetup.State);
			//AssertEquals("CurrentDataFileVersionNumber", OldVersion, TestController.TestTaskSetup.ExposedVersionInXmlSourceFile);

			//TestController.TestTaskSetup.SaveDataToSourceFile();

			//AssertEquals("NewDataFileVersionNumber", OldVersion, TestController.TestTaskSetup.ExposedVersionInXmlSourceFile);
			//AssertEquals("Task Setup State (2)", TaskStateEnum.OutUnchanged, TestController.TestTaskSetup.State);
		}

		#region Implementation

		protected DataUpgradeSetupControllerForTest TestController;

		protected override void SetUp()
		{
			base.SetUp();
			MockSourceControl.Setup();
			DocumentTablesCleaner.Clean();
			TestController = new DataUpgradeSetupControllerForTest();
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockSourceControl.TearDown();
		}

		#endregion
	}
}

