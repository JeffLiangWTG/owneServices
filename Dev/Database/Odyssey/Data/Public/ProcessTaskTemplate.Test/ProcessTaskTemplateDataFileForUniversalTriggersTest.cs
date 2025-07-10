using System;
using System.Data;
using System.Linq;
using System.Text;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class ProcessTaskTemplateDataFileForUniversalTriggersTest : TransactionedTestCase
	{
		public void TestLoadDataFromDatabase_ShouldIncludeUniversalTriggers_AndTheirCompletionTriggerActions()
		{
			ProcessTaskTemplateDataFileTest.ClearTables();
			InsertTestData();

			var dataFile = new ProcessTaskTemplateDataFile();
			var dataSet = dataFile.LoadDataFromDatabase();

			AssertContainsExactElementsInAnyOrder(new[]
			{
				ProcessTaskTemplateSchema.Constants.TableName,
				ProcessTasksSchema.Constants.TableName,
				ProcessTemplateTriggerSchema.Constants.TableName,
				ProcessTaskNotificationSchema.Constants.TableName,
			}, dataSet.Tables.Cast<DataTable>().Select(t => t.TableName));

			AssertTemplateContents("System-defined Universal Template - should include it and its completion trigger actions", dataSet, systemUniversalTemplatePK, true, TriggerInfo.ForUniversalTrigger(universalTriggerPK1_1, triggerAction1_1), TriggerInfo.ForUniversalTrigger(universalTriggerPK1_2, triggerAction1_2));
			AssertTemplateContents("Non-system Universal Template - should be excluded entirely", dataSet, nonSystemUniversalTemplatePK, shouldContainTemplate: false);
			AssertTemplateContents("System-defined Non-Universal Template - should not include completion trigger actions, because it's never done that before, and the old ways are the best",
				dataSet, systemNonUniversalTemplatePK, true, TriggerInfo.ForRegularTrigger(nonUniversalTriggerPK1), TriggerInfo.ForRegularTrigger(nonUniversalTriggerPK2));
		}

		#region Implementation

		Guid systemUniversalTemplatePK;
		Guid nonSystemUniversalTemplatePK;
		Guid systemNonUniversalTemplatePK;

		Guid universalTriggerPK1_1;
		Guid universalTriggerPK1_2;
		Guid universalTriggerPK2_1;
		Guid universalTriggerPK2_2;
		Guid nonUniversalTriggerPK1;
		Guid nonUniversalTriggerPK2;

		Guid triggerAction1_1;
		Guid triggerAction1_2;
		Guid triggerAction2_1;
		Guid triggerAction2_2;
		Guid triggerAction3_1;
		Guid triggerAction3_2;

		void InsertTestData()
		{
			systemUniversalTemplatePK = Guid.NewGuid();
			nonSystemUniversalTemplatePK = Guid.NewGuid();
			systemNonUniversalTemplatePK = Guid.NewGuid();

			universalTriggerPK1_1 = Guid.NewGuid();
			universalTriggerPK1_2 = Guid.NewGuid();
			universalTriggerPK2_1 = Guid.NewGuid();
			universalTriggerPK2_2 = Guid.NewGuid();
			nonUniversalTriggerPK1 = Guid.NewGuid();
			nonUniversalTriggerPK2 = Guid.NewGuid();

			triggerAction1_1 = Guid.NewGuid();
			triggerAction1_2 = Guid.NewGuid();
			triggerAction2_1 = Guid.NewGuid();
			triggerAction2_2 = Guid.NewGuid();
			triggerAction3_1 = Guid.NewGuid();
			triggerAction3_2 = Guid.NewGuid();

			var insertSql = new StringBuilder();

			insertSql.Append(BMDbTestHelper.GetWorkflowTemplateInsertSql(systemUniversalTemplatePK, "System-defined Universal Template", "CST", isSystem: true, isUniversal: true));
			insertSql.Append(BMDbTestHelper.GetWorkflowTemplateInsertSql(nonSystemUniversalTemplatePK, "Non-system Universal Template", "CST", isSystem: false, isUniversal: true));
			insertSql.Append(BMDbTestHelper.GetWorkflowTemplateInsertSql(systemNonUniversalTemplatePK, "System-defined Non-Universal Template", "CST", isSystem: true, isUniversal: false));

			insertSql.Append(WorkflowDbTestHelper.GetProcessTemplateTriggerInsertSql(universalTriggerPK1_1, systemUniversalTemplatePK, "Univigger #1", "ADD", 1));
			insertSql.Append(WorkflowDbTestHelper.GetProcessTemplateTriggerInsertSql(universalTriggerPK1_2, systemUniversalTemplatePK, "Univigger #2", "TAG", 2));
			insertSql.Append(WorkflowDbTestHelper.GetProcessTemplateTriggerInsertSql(universalTriggerPK2_1, nonSystemUniversalTemplatePK, "Univigger #3", "XFR", 1));
			insertSql.Append(WorkflowDbTestHelper.GetProcessTemplateTriggerInsertSql(universalTriggerPK2_2, nonSystemUniversalTemplatePK, "Univigger #4", "Z69", 2));

			insertSql.Append(WorkflowDbTestHelper.GetTriggerInsertSql(nonUniversalTriggerPK1, systemNonUniversalTemplatePK, "P0", "Non-univigger", "JOP"));
			insertSql.Append(WorkflowDbTestHelper.GetTriggerInsertSql(nonUniversalTriggerPK2, systemNonUniversalTemplatePK, "P0", "Non-univigger", "JCL"));

			insertSql.Append(WorkflowDbTestHelper.GetUniversalCompletionTriggerActionInsertSql(triggerAction1_1, universalTriggerPK1_1, "FLD"));
			insertSql.Append(WorkflowDbTestHelper.GetUniversalCompletionTriggerActionInsertSql(triggerAction1_2, universalTriggerPK1_2, "FLD"));
			insertSql.Append(WorkflowDbTestHelper.GetUniversalCompletionTriggerActionInsertSql(triggerAction2_1, universalTriggerPK2_1, "FLD"));
			insertSql.Append(WorkflowDbTestHelper.GetUniversalCompletionTriggerActionInsertSql(triggerAction2_2, universalTriggerPK2_2, "FLD"));

			insertSql.Append(WorkflowDbTestHelper.GetCompletionTriggerActionInsertSql(triggerAction3_1, nonUniversalTriggerPK1, "FLD"));
			insertSql.Append(WorkflowDbTestHelper.GetCompletionTriggerActionInsertSql(triggerAction3_2, nonUniversalTriggerPK2, "FLD"));

			TestConnection.ExecuteNonQuery(insertSql.ToString());
		}

		class TriggerInfo
		{
			internal static TriggerInfo ForUniversalTrigger(Guid triggerPK, Guid triggerActionPK)
			{
				return new TriggerInfo
				{
					TableName = ProcessTemplateTriggerSchema.Constants.TableName,
					PK = triggerPK,
					PKColumnName = ProcessTemplateTriggerSchema.Constants.PK,
					TriggerActionPK = triggerActionPK,
					TriggerActionFKColumnName = ProcessTaskNotificationSchema.Constants.PQ_P9T_Trigger,
				};
			}

			internal static TriggerInfo ForRegularTrigger(Guid triggerPK)
			{
				return new TriggerInfo
				{
					TableName = ProcessTasksSchema.Constants.TableName,
					PK = triggerPK,
					PKColumnName = ProcessTasksSchema.Constants.PK,
					TriggerActionFKColumnName = ProcessTaskNotificationSchema.Constants.PQ_P9,
				};
			}

			TriggerInfo()
			{
			}

			internal string TableName { get; set; }
			internal Guid PK { get; set; }
			internal string PKColumnName { get; set; }
			internal Guid? TriggerActionPK { get; set; }
			internal string TriggerActionFKColumnName { get; set; }
		}

		static void AssertTemplateContents(string message, DataSet dataSet, Guid templatePK, bool shouldContainTemplate, params TriggerInfo[] triggers)
		{
			CombineAssertions(message, () =>
			{
				AssertDataSetContainsRowByValue("Template row", dataSet, ProcessTaskTemplateSchema.Constants.TableName, ProcessTaskTemplateSchema.PK.Name, templatePK, shouldContainTemplate);

				foreach (var triggerInfo in triggers)
				{
					AssertDataSetContainsRowByValue("Trigger row should exist", dataSet, triggerInfo.TableName, triggerInfo.PKColumnName, triggerInfo.PK);

					if (triggerInfo.TriggerActionPK == null)
					{
						AssertDataSetContainsRowByValue("Completion trigger action row referring to this parent trigger should NOT exist", dataSet, ProcessTaskNotificationSchema.Constants.TableName, triggerInfo.TriggerActionFKColumnName, triggerInfo.TriggerActionPK, shouldContain: false);
					}
					else
					{
						AssertDataSetContainsRowByValue("Completion trigger action row should exist", dataSet, ProcessTaskNotificationSchema.Constants.TableName, ProcessTaskNotificationSchema.PK.Name, triggerInfo.TriggerActionPK);
					}
				}
			});
		}

		static void AssertDataSetContainsRowByValue(string message, DataSet dataSet, string tableName, string columnName, object columnValue, bool shouldContain = true)
		{
			var row = dataSet.Tables[tableName].Rows.Cast<DataRow>().SingleOrDefault(r => r[columnName].Equals(columnValue));

			if (shouldContain)
			{
				AssertNotNull(message, row);
			}
			else
			{
				AssertNull(message, row);
			}
		}

		#endregion
	}
}
