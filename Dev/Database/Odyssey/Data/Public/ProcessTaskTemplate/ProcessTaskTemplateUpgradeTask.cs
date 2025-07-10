using System;
using System.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class ProcessTaskTemplateUpgradeTask : EmbeddedUpgradeTask
	{
		public ProcessTaskTemplateUpgradeTask()
			: base(new ProcessTaskTemplateDataFile())
		{
		}

		protected override void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
			if (columnName != ProcessTaskTemplateSchema.P0_IsActive.Name)
			{
				base.UpdateColumn(columnName, targetRow, sourceRow);
			}
		}

		protected override void DoDelete(DataRow targetRow, ref int targetIndex)
		{
			if (targetRow.Table.TableName == ProcessTaskTemplateSchema.Constants.TableName)
			{
				foreach (var processTaskRow in GetChildRows(targetRow, ProcessTasksSchema.Constants.TableName, ProcessTasksSchema.Constants.P9_ParentID))
				{
					processTaskRow.Delete();
				}

				foreach (var processTemplateTriggerRow in GetChildRows(targetRow, ProcessTemplateTriggerSchema.Constants.TableName, ProcessTemplateTriggerSchema.Constants.P9T_P0_Template))
				{
					foreach (var triggerActionRow in GetChildRows(processTemplateTriggerRow, ProcessTaskNotificationSchema.Constants.TableName, ProcessTaskNotificationSchema.Constants.PQ_P9T_Trigger, ProcessTemplateTriggerSchema.Constants.TableName, ProcessTemplateTriggerSchema.Constants.PK))
					{
						triggerActionRow.Delete();
					}

					processTemplateTriggerRow.Delete();
				}
			}

			base.DoDelete(targetRow, ref targetIndex);
		}

		protected override void DoDeleteForSetup(DataRow targetRow, ref int targetIndex)
		{
			DoDelete(targetRow, ref targetIndex);
		}

		static DataRow[] GetChildRows(DataRow row, string childRelationTableName, string childFKColumnName, string parentRelationTableName = ProcessTaskTemplateSchema.Constants.TableName, string parentPKColumnName = ProcessTaskTemplateSchema.Constants.PK)
		{
			var dataSet = row.Table.DataSet;
			var relationName = FormattableString.Invariant($"{childRelationTableName}.{childFKColumnName}");
			var relation = dataSet.Relations[relationName];

			if (relation == null)
			{
				var parentColumn = dataSet.Tables[parentRelationTableName].Columns[parentPKColumnName];
				var childColumn = dataSet.Tables[childRelationTableName].Columns[childFKColumnName];

				relation = new DataRelation(relationName, parentColumn, childColumn);

				dataSet.Relations.Add(relation);
			}

			return row.GetChildRows(relation);
		}
	}
}
