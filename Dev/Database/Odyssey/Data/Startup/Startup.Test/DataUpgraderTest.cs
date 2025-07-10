using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Data.Testing;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Microsoft.SqlServer.Types;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Test
{
	public class DataUpgraderTest : TransactionedTestCase
	{
		public class DataUpgraderForTest : DataUpgraderForAllTasks
		{
			public DataUpgraderForTest(IUpgradeManager manager) : base(manager)
			{
			}

			protected override void InitialiseUpgradeTasks()
			{
				DocumentsUpgradeTaskForTest documentsTask = new DocumentsUpgradeTaskForTest();
				fUpgradeTaskList.Add(documentsTask);
			}

			protected override bool TaskIsRequired(IUpgradeTask task)
			{
				return true;
			}
		}

		public class DataUpgraderForAllTasks : DataUpgrader
		{
			public DataUpgraderForAllTasks()
				: this(new DummyUpgradeManager())
			{
			}

			internal DataUpgraderForAllTasks(IUpgradeManager manager)
				: base(manager, Db.Connection, new VersionLabel(0, 0))
			{
			}

			internal DataUpgraderForAllTasks(IUpgradeManager manager, VersionLabel version)
				: base(manager, Db.Connection, version)
			{
			}

			protected override bool TaskIsRequired(IUpgradeTask task)
			{
				return true;
			}
		}

		class DocumentsUpgradeTaskForTest : DocumentsUpgradeTask
		{
			public DocumentsUpgradeTaskForTest() : base(new DocumentsCompleteDataFile())
			{
			}

			public override bool IsRequired
			{
				get
				{
					return true;
				}
			}

			public override string TaskNameWhenUpgrading
			{
				get
				{
					return "TaskNameWhenUpgrading";
				}
			}
		}

		public void TestWhenUpgradeFails()
		{
			string insertSql = @"
				CREATE TABLE StmMenuRelatedObjectTest 
				(
					SK_SU uniqueidentifier
					CONSTRAINT StmMenuRelatedObjectTest_SK_SU_FK2_StmMenuItem_RRR_120N
					REFERENCES StmMenuItem (SU_PK)
				)
				INSERT dbo.StmMenuItem (SU_PK, SU_MenuName, SU_BusinessContext, SU_ContactType, SU_IsPublished, SU_IsSystemDefined, SU_IsClientSpecific, SU_MenuPath, SU_MenuShortCut, SU_MenuIndex, SU_GS_NKStaffCode, SU_FilterList, SU_DocumentDirection) VALUES('1B934998-649C-48F5-B452-89D885C85021', 'User Doc', 'Consol', '', 1, 1, 0, '', 'None', 0, '', '', '');
				INSERT StmMenuRelatedObjectTest (SK_SU) values ('1B934998-649C-48F5-B452-89D885C85021')";
			Db.Connection.ExecuteNonQuery(insertSql);
			UpgradeManagerForTestWithOutputBuffer manager = new UpgradeManagerForTestWithOutputBuffer();
			DataUpgrader testUpgrader = new DataUpgraderForTest(manager);
			try
			{
				testUpgrader.RunUpgrade();
				Fail("This test upgrade was supposed to fail");
			}
			catch (Exception e)
			{
				AssertEquals("Message line 0", "Starting " + testUpgrader.Name, manager.OutputTextCollection[0]);
				AssertEquals("Message last line", "TaskNameWhenUpgrading", manager.OutputTextCollection[manager.OutputTextCollection.Count - 1]);
				string[] errorLines = e.Message.Split('\n');
				AssertEquals("Error line 0", testUpgrader.Name + " failed.", errorLines[0].Trim());
				string expextLine1Start = "Error encountered while upgrading data for 'ExcelTemplates.DbUpgrader.Data.Documents.DocumentsComplete.xml' from version";
				Assert("Error line 1.\r\nExpected start: [" + expextLine1Start + "]\r\nBut line was: [" + errorLines[1].Trim() + "]", errorLines[1].Trim().StartsWith(expextLine1Start));
				string expected = "InnerException Message = The DELETE statement conflicted with the REFERENCE constraint \"StmMenuRelatedObjectTest_SK_SU_FK2_StmMenuItem_RRR_120N\".";
				var actual = errorLines[3].Trim();
				Assert("Error line 3.\r\nExpected start: [" + expected + "]\r\nBut line was: [" + actual + "]", errorLines[3].Trim().StartsWith(expected));
			}
		}

		public void TestWhenUpgradeSucceeds()
		{
			UpgradeManagerForTestWithOutputBuffer manager = new UpgradeManagerForTestWithOutputBuffer();
			DataUpgrader testUpgrader = new DataUpgraderForTest(manager);
			testUpgrader.RunUpgrade();
			AssertEquals("OutputTextCollection.Count", 6, manager.OutputTextCollection.Count);
			AssertEquals("Message line 0", "Starting " + testUpgrader.Name, manager.OutputTextCollection[0]);
			Assert("Message line 1, actual value is: " + manager.OutputTextCollection[1], manager.OutputTextCollection[1].StartsWith("Version:"));
			AssertEquals("Message line 3", "TaskNameWhenUpgrading", manager.OutputTextCollection[3]);
			AssertEquals("Message line 4 Errors", "Updating system data version", manager.OutputTextCollection[4]);
			AssertEquals("Message line 5", testUpgrader.Name + " completed.", manager.OutputTextCollection[5]);
		}

		public void TestAllUpdatesInDataUpgraderAreValidForCurrentSchema()
		{
			string sqlText = string.Format(@"
				-- GlbGroupRole
				DELETE dbo.GlbGroupRole WHERE GGR_GG_Group = 'a99e7f0e-8379-4f50-8560-9b4bb804c0de' AND GGR_RoleName = 'db_datawriter';
				DELETE dbo.GlbGroupRole WHERE GGR_GG_Group = '6f0eb310-fc5c-4696-9594-f8ce156542c6' AND GGR_RoleName = 'cwRestrictedReaderRole';
				DELETE dbo.GlbGroupRole WHERE GGR_GG_Group = '208068b6-3383-44bf-8e0d-dbd827f9d675' AND GGR_RoleName = 'db_backupoperator';
				");
			TestConnection.ExecuteNonQuery(sqlText);
			Type[] extraAllowedTypes = new Type[] { typeof(SqlGeography), };

#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			AppDomain.CurrentDomain.SetData("System.Data.DataSetDefaultAllowedTypes", extraAllowedTypes);
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.

			UpgradeManagerForTestWithOutputBuffer manager = new UpgradeManagerForTestWithOutputBuffer();
			DataUpgrader testUpgrader = new DataUpgraderForAllTasks(manager);
			testUpgrader.RunUpgrade();
			foreach (string text in manager.OutputTextCollection)
			{
				if (text.ToLower().IndexOf("error") >= 0)
				{
					Fail("Error found while running all data updates: " + System.Environment.NewLine + text);
				}
			}

			Assert(true);
		}

		public void TestCheckForStmNumberSequenceUpgradeTaskForExistingClient()
		{
			UpgradeManagerForTestWithOutputBuffer manager = new UpgradeManagerForTestWithOutputBuffer();
			DataUpgrader testUpgrader = new DataUpgraderForAllTasks(manager, new VersionLabel(1, 0));
			AssertUpgradeIncludes(testUpgrader.UpgradeTaskList, typeof(BaseData.System.StmNumberSequenceUpgradeTask));
			Assert(true);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			DocumentTablesCleaner.Clean();
		}

		void AssertUpgradeIncludes(IUpgradeTask[] taskList, Type type)
		{
			for (int i = 0; i < taskList.Length; i++)
			{
				if (taskList[i].GetType() == type)
				{
					return;
				}
			}

			Fail("Data updates do not include " + type.ToString());
		}
		#endregion
	}
}
