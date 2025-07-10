using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class TagRuleUpgradeTask : EmbeddedUpgradeTask
	{
		public TagRuleUpgradeTask()
			: base(new TagRuleDataFile())
		{
		}

		static IEnumerable<string> ColumnsExcludedFromUpdate
		{
			get
			{
				yield return TagRuleSchema.Constants.TGR_IsActive;
				yield return TagMagnitudeSchema.Constants.TGM_VisualizationData;
				yield return TagMagnitudeSchema.Constants.TGM_NudgeAmount;
				yield return StmModuleFilterSchema.Constants.S9_GC;
				yield return StmModuleFilterSchema.Constants.S9_RelatedEntityID;
			}
		}

		protected override void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
			if (!ColumnsExcludedFromUpdate.Contains(columnName))
			{
				if (targetRow.Table.TableName == TagRuleSchema.Constants.TableName)
				{
					EnsureBranchAndDepartmentAreNotNull(sourceRow);
				}

				base.UpdateColumn(columnName, targetRow, sourceRow);
			}
		}

		protected override void DoInsert(DataRow sourceRow, DataTable targetTable, ref int targetIndex)
		{
			if (targetTable.TableName == TagRuleSchema.Constants.TableName)
			{
				EnsureBranchAndDepartmentAreNotNull(sourceRow);

				if ((Guid)sourceRow[TagRuleSchema.Constants.TGR_GB_Branch] != Guid.Empty && (Guid)sourceRow[TagRuleSchema.Constants.TGR_GE_Department] != Guid.Empty)
				{
					base.DoInsert(sourceRow, targetTable, ref targetIndex);
				}
			}
			else
			{
				base.DoInsert(sourceRow, targetTable, ref targetIndex);
			}
		}

		Guid? cachedDepartmentPK;
		Guid? cachedBranchPK;

		internal void EnsureBranchAndDepartmentAreNotNull(DataRow row)
		{
			EnsureColumnValueNotNull(row, TagRuleSchema.Constants.TGR_GE_Department, ref cachedDepartmentPK, @"
				SELECT 
					COALESCE(
						(SELECT GE_PK FROM dbo.GlbDepartment WHERE GE_Code = 'BRN'),
						(SELECT TOP 1 GE_PK FROM dbo.GlbDepartment)
					)
				");

			EnsureColumnValueNotNull(row, TagRuleSchema.Constants.TGR_GB_Branch, ref cachedBranchPK, @"
				SELECT 
					COALESCE(
						(SELECT S5_GB FROM dbo.StmScheduleTask WHERE S5_ScheduleType = 'TAG'),
						(SELECT TOP 1 GB_PK FROM dbo.GlbBranch)
					)
				");
		}

		static void EnsureColumnValueNotNull(DataRow row, string columnName, ref Guid? cachedDefaultValue, string defaultValueSelectStatement)
		{
			if (row[columnName] is DBNull)
			{
				if (cachedDefaultValue == null)
				{
					var value = Db.Connection.ExecuteScalar(defaultValueSelectStatement);

					cachedDefaultValue = value is Guid valueAsGuid ? valueAsGuid : Guid.Empty;
				}

				row[columnName] = cachedDefaultValue.Value;
			}
		}
	}
}
