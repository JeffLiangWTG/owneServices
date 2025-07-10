using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class ScheduledReportTestHelper
	{
		public static (IDisposable TempUserContext, ZGuid BranchPk) SwitchToNewBranch(BusinessObjectFactory factory, bool isActiveBranch = true)
		{
			var homeCompany = factory.NewWithValidTestData<GlbCompany>();
			homeCompany.CompanyName = "homeCompany";
			homeCompany.GC_Code = "CCC";

			var homeBranch = factory.NewWithValidTestData<GlbBranch>();
			homeBranch.GB_BranchName = "homeBranch";
			homeBranch.GB_Code = "BBB";
			homeBranch.GB_GC = homeCompany.PK;
			homeBranch.GB_IsActive = isActiveBranch;
			factory.Save();

			return (Env.SetTemporaryUserContext(Env.CurrentUserPK, homeBranch.PK.ToGuid(), Env.CurrentDepartmentPK), homeBranch.PK);
		}

		public byte[] PrepareScheduledReport(Dictionary<string, string> templateContent, Guid stmMenuItemPK, string creditorPk)
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", "", templateContent);
			using var pack = new DocumentPack(new BusinessObjectFactory().Load<StmMenuItem>(stmMenuItemPK));
			using var report = new Report(pack, excelTemplate);
			report.PrepareForRender();
			report.Parent.Language = Core.SharedConstants.Languages.ChineseSimplified;

			((FilterFieldValueSerialisable)report.ColumnHeadingManager.LinkedFilterFields["Orgs"]).ValueAsStringForSerialisation = creditorPk;
			((FilterFieldValueSerialisable)report.ColumnHeadingManager.LinkedFilterFields["Creditor"]).ValueAsStringForSerialisation = creditorPk;

			var reportSerializationInfo = new ReportSerializationInfo(null, report);
			return new ZBlob(Encoding.UTF8.GetBytes(JsonConverterHelper.Serialize(reportSerializationInfo)));
		}

		public void CreateTemplate(Guid templatePK, string templateName, Dictionary<string, string> templateContent)
		{
			var templateTestHelper = new TemplateTestHelper();

			foreach (var worksheetName in templateContent.Keys)
			{
				templateTestHelper.AddWorkSheet(worksheetName, templateContent[worksheetName]);
			}

			var templateBlob = templateTestHelper.CreateTemplateBlob();

			var sqlText = FormattableString.Invariant($@"
INSERT INTO {StmTemplateSchema.Constants.SqlSchemaName}.{StmTemplateSchema.Constants.TableName} 
({StmTemplateSchema.Constants.PK}, 
{StmTemplateSchema.Constants.SO_Name}, 
{StmTemplateSchema.Constants.SO_Template}, 
{StmTemplateSchema.Constants.SO_SystemCreateUser}, 
{StmTemplateSchema.Constants.SO_SystemCreateTimeUtc}, 
{StmTemplateSchema.Constants.SO_SystemLastEditUser}, 
{StmTemplateSchema.Constants.SO_SystemLastEditTimeUtc})
VALUES (@SO_PK, @SO_Name, @SO_Template, @SO_SystemCreateUser, @SO_SystemCreateTimeUtc, @SO_SystemLastEditUser, @SO_SystemLastEditTimeUtc)");

			using (var command = Db.Connection.Command(sqlText))
			{
				command.AddParameterBasedOnDbColumn("@SO_PK", templatePK, StmTemplateSchema.PK);
				command.AddParameterBasedOnDbColumn("@SO_Name", templateName, StmTemplateSchema.SO_Name);
				command.AddParameterBasedOnDbColumn("@SO_Template", templateBlob, StmTemplateSchema.SO_Template);
				command.AddParameterBasedOnDbColumn("@SO_SystemCreateUser", "AAA", StmTemplateSchema.SO_SystemCreateUser);
				command.AddParameterBasedOnDbColumn("@SO_SystemCreateTimeUtc", DateTime.Today, StmTemplateSchema.SO_SystemCreateTimeUtc);
				command.AddParameterBasedOnDbColumn("@SO_SystemLastEditUser", "AAA", StmTemplateSchema.SO_SystemLastEditUser);
				command.AddParameterBasedOnDbColumn("@SO_SystemLastEditTimeUtc", DateTime.Today, StmTemplateSchema.SO_SystemLastEditTimeUtc);
				command.ExecuteNonQuery();
			}
		}

		public void CreateMenuItem(Guid stmMenuItemPK, string menuItemName, bool isSystemDefined)
		{
			var sqlText = FormattableString.Invariant($@"
INSERT INTO {StmMenuItemSchema.Constants.SqlSchemaName}.{StmMenuItemSchema.Constants.TableName}
({StmMenuItemSchema.Constants.PK},
{StmMenuItemSchema.Constants.SU_MenuName},
{StmMenuItemSchema.Constants.SU_IsSystemDefined})
VALUES (@SU_PK, @SU_MenuName, @SU_IsSystemDefined)");

			using (var command = Db.Connection.Command(sqlText))
			{
				command.AddParameterBasedOnDbColumn("@SU_PK", stmMenuItemPK, StmMenuItemSchema.PK);
				command.AddParameterBasedOnDbColumn("@SU_MenuName", menuItemName, StmMenuItemSchema.SU_MenuName);
				command.AddParameterBasedOnDbColumn("@SU_IsSystemDefined", isSystemDefined, StmMenuItemSchema.SU_IsSystemDefined);
				command.ExecuteNonQuery();
			}
		}

		public void CreateStmMenuTemplatePivot(Guid stmMenuItemPK, Guid templatedPK)
		{
			var sqlText = FormattableString.Invariant($@"
INSERT INTO {StmMenuTemplatePivotSchema.Constants.SqlSchemaName}.{StmMenuTemplatePivotSchema.Constants.TableName}
({StmMenuTemplatePivotSchema.Constants.PK},
{StmMenuTemplatePivotSchema.Constants.SI_SU},
{StmMenuTemplatePivotSchema.Constants.SI_SO})
VALUES (NEWID(), @SI_SU, @SI_SO)");

			using (var command = Db.Connection.Command(sqlText))
			{
				command.AddParameterBasedOnDbColumn("@SI_SU", stmMenuItemPK, StmMenuTemplatePivotSchema.SI_SU);
				command.AddParameterBasedOnDbColumn("@SI_SO", templatedPK, StmMenuTemplatePivotSchema.SI_SO);
				command.ExecuteNonQuery();
			}
		}

		public void CreateScheduleTask(Guid scheduleTaskPK, Guid stmMenuItemPK, string period, byte[] scheduledReportContent)
		{
			var sqlText = FormattableString.Invariant($@"
INSERT INTO {StmScheduleTaskSchema.Constants.TableName} 
({StmScheduleTaskSchema.Constants.PK},
{StmScheduleTaskSchema.Constants.S5_TaskPeriod},
{StmScheduleTaskSchema.Constants.S5_ParentID},
{StmScheduleTaskSchema.Constants.S5_ScheduleState},
{StmScheduleTaskSchema.Constants.S5_ScheduleType},
{StmScheduleTaskSchema.Constants.S5_ParentTableCode},
{StmScheduleTaskSchema.Constants.S5_GB})
VALUES (@S5_PK, @S5_TaskPeriod, @S5_ParentID, @S5_ScheduleState, @S5_ScheduleType, @S5_ParentTableCode, @S5_GB)");

			using (var command = Db.Connection.Command(sqlText))
			{
				command.AddParameterBasedOnDbColumn("@S5_PK", scheduleTaskPK, StmScheduleTaskSchema.PK);
				command.AddParameterBasedOnDbColumn("@S5_TaskPeriod", period, StmScheduleTaskSchema.S5_TaskPeriod);
				command.AddParameterBasedOnDbColumn("@S5_ScheduleState", scheduledReportContent ?? (object)DBNull.Value, StmScheduleTaskSchema.S5_ScheduleState);
				command.AddParameterBasedOnDbColumn("@S5_ParentID", stmMenuItemPK, StmScheduleTaskSchema.S5_ParentID);
				command.AddParameterBasedOnDbColumn("@S5_ScheduleType", "REP", StmScheduleTaskSchema.S5_ScheduleType);
				command.AddParameterBasedOnDbColumn("@S5_ParentTableCode", "SU", StmScheduleTaskSchema.S5_ParentTableCode);
				command.AddParameterBasedOnDbColumn("@S5_GB", Env.CurrentBranchPK, StmScheduleTaskSchema.S5_GB);
				command.ExecuteNonQuery();
			}
		}
	}
}
