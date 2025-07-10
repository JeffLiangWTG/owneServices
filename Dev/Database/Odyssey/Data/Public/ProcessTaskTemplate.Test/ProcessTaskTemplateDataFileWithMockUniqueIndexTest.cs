using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class ProcessTaskTemplateDataFileWithMockUniqueIndexTest : TransactionedTestCase
	{
		/// <summary>
		/// Only tasks which implement IFixReferencesAndDuplicates will have contraints disabled.
		/// For all other tasks, they rely on our saving algorithm to apply changes to the database in dependency order.
		/// </summary>
		public void TestDataFileDoesNotDisableConstraintsOrIndexes()
		{
			string sqlText = String.Format(@"
				CREATE UNIQUE INDEX [{0}] ON [ProcessTaskTemplate] ({1});
				UPDATE TOP (1) ProcessTaskTemplate SET P0_PK = newid(), P0_IsSystem = 0, P0_Name = 'Different'
				WHERE 1=1
					AND P0_PK not in (SELECT P9_ParentID FROM dbo.ProcessTasks WHERE P9_ParentID is not null)
					AND P0_PK not in (SELECT P9T_P0_Template FROM dbo.ProcessTemplateTrigger);
				",
				ProcessTaskTemplateDataFileForTesting.TestUniqueIndexName,
				ProcessTaskTemplateDataFileForTesting.TestUniqueIndexColumnList);
			TestConnection.ExecuteNonQuery(sqlText);

			var upgradeTask = new ProcessTaskTemplateUpgradeTaskForTesting();

			try
			{
				upgradeTask.Run();
				Fail("Should throw exception");
			}
			catch (Exception ex)
			{
				AssertEquals("Exception = Cannot insert duplicate key?", true, ex.Message.Contains("Cannot insert duplicate key"));
				AssertEquals("Violated index = " + ProcessTaskTemplateDataFileForTesting.TestUniqueIndexName, true, ex.Message.Contains(ProcessTaskTemplateDataFileForTesting.TestUniqueIndexName));
			}
		}

		class ProcessTaskTemplateDataFileForTesting : ProcessTaskTemplateDataFile
		{
			public ProcessTaskTemplateDataFileForTesting() : base(DataFileRelativePath)
			{
			}

			const string DataFileRelativePath = @"Public\ProcessTaskTemplate.Testing\ProcessTaskTemplate.xml";
			public override string ResourceRelativeName => "ProcessTaskTemplate.Testing.ProcessTaskTemplate.xml";

			protected override List<UniqueIndexInfo> GetUniqueIndexesToDropBeforeSaveAndRecreateAfterwards()
			{
				var ruleUniqueIndex = new UniqueIndexInfo(
					ProcessTaskTemplateSchema.Constants.TableName,
					TestUniqueIndexName,
					TestUniqueIndexColumnList
					);
				var result = new List<UniqueIndexInfo>();
				result.Add(ruleUniqueIndex);
				return result;
			}

			public const string TestUniqueIndexName = "ProcessTaskTemplate_80E80127C2584C849F5E0136AD78B707";
			public const string TestUniqueIndexColumnList = "P0_ProcessType,P0_SubType1,P0_SubType2,P0_SubType3";
		}

		class ProcessTaskTemplateUpgradeTaskForTesting : EmbeddedUpgradeTask
		{
			public ProcessTaskTemplateUpgradeTaskForTesting()
				: base(new ProcessTaskTemplateDataFileForTesting())
			{
			}
		}
	}
}
