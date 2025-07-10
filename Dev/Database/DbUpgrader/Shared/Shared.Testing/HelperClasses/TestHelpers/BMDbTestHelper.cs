using System;
using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;

#if DEBUG

namespace Enterprise.DbUpgrader.Shared
{
	public static class BMDbTestHelper
	{
		const string MENTAgedScoreQueryInsertSQL = @"
			INSERT INTO dbo.MENTAgedScoreQuery
			(
				MAQ_PK, MAQ_Code, MAQ_SystemCreateTimeUtc, MAQ_SystemLastEditTimeUtc, MAQ_QueryDescription, MAQ_SystemCreateUser, MAQ_SystemLastEditUser
			)
			VALUES
			(
				'{0}', '{1}', GETDATE(), GETDATE(), 'boop', '~BP', '~BP'
			)";

		public static string GetMENTAgedScoreQueryInsertSQL(Guid pk, string code)
		{
			return string.Format(CultureInfo.InvariantCulture, MENTAgedScoreQueryInsertSQL, pk, code);
		}

		public static string GetMENTAgedScoreExtractionInsertSQL(Guid pk, Guid query, string name = "Some extraction")
		{
			return string.Format(CultureInfo.InvariantCulture, @"
				INSERT INTO dbo.MENTAgedScoreExtraction
				(
					MEX_PK, MEX_MAQ, MEX_Name
				)
				VALUES
				(
					'{0}', '{1}', '{2}'
				)", pk, query, name);
		}

		public const string MENTAgedScoreVisualisationInsertSQL = @"
			INSERT INTO dbo.MENTAgedScoreVisualisation
			(
				MVI_PK, MVI_MEX, MVI_IsCustomised, MVI_GraphType
			)
			VALUES
			(
				'{0}', '{1}', {2}, 'COL'
			)";

		public static InsertStatement GetProcessHeaderInsertSql(Guid pk, Guid? parentHeaderPK, Guid? parentId, Guid? templatePK = null, string completionStatement = "You Better Werkflow", string parentTableCode = "WKI", string status = "OPN", Guid componentPK = default(Guid), string category = "UDF", string workflowType = "WKI")
		{
			var component = componentPK == default(Guid) ? "NULL" : "'" + componentPK + "'";

			return new InsertStatement(
				@"
				INSERT dbo.ProcessHeader (
					FH_PK,
					FH_FH_ParentHeader,
					FH_ParentId,
					FH_ParentTableCode,
					FH_P0_Template,
					FH_CompletionStatement,
					FH_SystemCreateTimeUtc,
					FH_SystemLastEditTimeUtc,
					FH_WorkflowType,
					FH_Status,
					FH_FC_CurrentComponent,
					FH_Category,
					FH_SystemCreateUser,
					FH_SystemLastEditUser
				) VALUES",
				$@"
				(
					'{pk}',
					{DbTextFormatTestHelper.EncloseInQuotesOrNullString(parentHeaderPK)},
					{DbTextFormatTestHelper.EncloseInQuotesOrNullString(parentId)},
					'{parentTableCode}',
					{DbTextFormatTestHelper.EncloseInQuotesOrNullString(templatePK)},
					'{completionStatement}',
					GETDATE(),
					GETDATE(),
					'{workflowType}',
					'{status}',
					{component},
					'{category}',
					'~BP',
					'~BP'
				)
				");
		}

		public static InsertStatement GetProcessTasksInsertSql(Guid taskPK, Guid? workflowPK, Guid? parentID, string parentTableCode, string status, string type = "UDF", string assignedStaff = null, Guid? requiredCapability = null, DateTime? estimatedDuration = null)
		{
			return new InsertStatement(
				@"
				INSERT dbo.ProcessTasks (
					P9_PK, P9_FH_ProcessHeader, P9_ParentID, P9_ParentTableCode, P9_Status, P9_Type, P9_GS_NKAssignedStaffMember, P9_G4_RequiredCapability, P9_EstDuration, P9_SystemCreateTimeUtc, P9_SystemCreateUser, P9_SystemLastEditTimeUtc, P9_SystemLastEditUser
				) VALUES",
				string.Format(CultureInfo.InvariantCulture, @"
				(
					'{0}', {1}, {2}, '{3}', '{4}', '{5}', '{6}', {7}, {8}, GetUtcDate(), '~BP', GetUtcDate(), '~BP'
				)
				",
				taskPK,
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(workflowPK),
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(parentID),
				parentTableCode,
				status,
				type,
				assignedStaff,
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(requiredCapability),
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(estimatedDuration)));
		}

		public const string ProcessTaskInsertSql = @"
INSERT dbo.ProcessTasks (P9_PK, P9_Type, P9_Status, P9_EstDuration, P9_EstimateVariationFactor, P9_EstimatedTimeToComplete, P9_FH_ProcessHeader, P9_ParentID, P9_ParentTableCode, P9_SystemCreateTimeUtc, P9_SystemLastEditTimeUtc, P9_SystemCreateUser, P9_SystemLastEditUser)
VALUES ('{0}', 'UDF', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', 'OH', GetUtcDate(), GetUtcDate(), '~BP', '~BP')
";

		public const string ProcessTaskClosedInsertSql = @"
INSERT dbo.ProcessTasks (P9_PK, P9_Type, P9_Status, P9_EstDuration, P9_EstimateVariationFactor, P9_EstimatedTimeToComplete, P9_FH_ProcessHeader, P9_CompletedTimeUtc, P9_ParentID, P9_ParentTableCode, P9_SystemCreateTimeUtc, P9_SystemLastEditTimeUtc, P9_SystemCreateUser, P9_SystemLastEditUser)
VALUES ('{0}', 'UDF', '{1}', '{2}', '{3}', '{4}', '{5}', {6}, '{7}', 'OH', GetUtcDate(), GetUtcDate(), '~BP', '~BP')
";

		public const string ProcessJobHeaderInsertSql = @"
			INSERT dbo.ProcessHeader (
				FH_PK, FH_FH_ParentHeader, FH_ParentId, FH_SystemCreateTimeUtc, FH_SystemLastEditTimeUtc, FH_WorkflowType, FH_SystemCreateUser, FH_SystemLastEditUser
			) VALUES (
				'{0}', null, '{1}', GETDATE(), GETDATE(), 'WKI', '~BP', '~BP'
			)
			";

		public const string ProcessHeaderWithReleaseDetailsInsertSql = @"
			INSERT dbo.ProcessHeader (
				FH_PK, FH_FH_ParentHeader, FH_ParentId, FH_FC_CurrentComponent, FH_ReleaseDateTime, FH_SystemCreateTimeUtc, FH_SystemLastEditTimeUtc, FH_WorkflowType, FH_SystemCreateUser, FH_SystemLastEditUser
			) VALUES (
				'{0}', '{1}', '{2}', '{3}', '{4}', GETDATE(), GETDATE(), 'WKI', '~BP', '~BP'
			)
			";

		public const string ProcessHeaderWithNullReleaseDateTimeInsertSql = @"
			INSERT dbo.ProcessHeader (
				FH_PK, FH_FH_ParentHeader, FH_ParentId, FH_FC_CurrentComponent, FH_ReleaseDateTime, FH_SystemCreateTimeUtc, FH_SystemLastEditTimeUtc, FH_WorkflowType, FH_SystemCreateUser, FH_SystemLastEditUser
			) VALUES (
				'{0}', '{1}', '{2}', '{3}', NULL, GETDATE(), GETDATE(), 'WKI', '~BP', '~BP'
			)
			";

		public static InsertStatement GetProcessHeaderLinkInsertSql(Guid linkPK, Guid headerFrom, Guid headerTo, string linkType, bool syncBufferPenetration = false)
		{
			return new InsertStatement(
				@"
				INSERT dbo.ProcessHeaderLink (
					FP_PK, FP_FH_HeaderFrom, FP_FH_HeaderTo, FP_LinkType, FP_SynchroniseBufferPenetration, FP_SystemCreateTimeUtc, FP_SystemCreateUser, FP_SystemLastEditTimeUtc, FP_SystemLastEditUser
				) VALUES",
				string.Format(CultureInfo.InvariantCulture, @"
				(
					'{0}', '{1}', '{2}', '{3}', {4}, GetUtcDate(), '~BP', GetUtcDate(), '~BP'
				)
				", linkPK, headerFrom, headerTo, linkType, syncBufferPenetration ? 1 : 0));
		}

		public static InsertStatement GetShapeInsertSql(Guid pk, string name, Guid? relatedEntityPK = null, string approvedBy = "", string shapeType = "SHP", string layoutData = "", string relatedEntityTableCode = "FH", Guid? parentShape = null, Guid? rootShape = null)
		{
			if ((parentShape == null || rootShape == null) && !shapeType.In("DIA", "SYS"))
			{
				throw new ArgumentException("If supplying a null parent or root shape, the shape type must be either DIA or SYS. It was: " + shapeType, nameof(shapeType));
			}

			return new InsertStatement(
				@"
				INSERT dbo.BMNCNShape
				(
					BNS_PK,
					BNS_Name,
					BNS_RelatedEntityID,
					BNS_RelatedEntityTableCode,
					BNS_GS_NKApprovedBy,
					BNS_ShapeType,
					BNS_LayoutData,
					BNS_SystemCreateTimeUtc,
					BNS_Status,
					BNS_BNS_ParentShape,
					BNS_BNS_RootShape,
					BNS_SystemCreateUser,
					BNS_SystemLastEditTimeUtc,
					BNS_SystemLastEditUser
				)
				VALUES",
				string.Format(CultureInfo.InvariantCulture, @"
				(
					'{0}',
					'{1}',
					{2},
					'{3}',
					'{4}',
					'{5}',
					'{6}',
					GETDATE(),
					'UNK',
					{7},
					{8},
					'~BP',
					GETUTCDATE(),
					'~BP'
				)",
				pk,
				name,
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(relatedEntityPK),
				relatedEntityPK == null ? "" : relatedEntityTableCode,
				approvedBy,
				shapeType,
				layoutData,
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(parentShape),
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(rootShape)));
		}

		public static InsertStatement GetShapeScheduleInsertSql(Guid shapePK, DateTime? earliestStartTime = null, DateTime? latestStartTime = null, DateTime? earliestFinishTime = null,
			DateTime? latestFinishTime = null, DateTime? scheduledStartTime = null, DateTime? scheduledFinishTime = null, int durationMinutes = 0, bool isBuffered = false, char scaleUnit = 'D',
			int scaleMag = 1, int resIncrement = 1, Guid? branch = null, Guid? department = null, decimal bufferPenetrationPercent = 0m)
		{
			return new InsertStatement(
				@"
				INSERT dbo.BMNCNSchedule (
					BNC_PK,
					BNC_BNS_Shape,
					BNC_EarliestStartUtc,
					BNC_LatestStartUtc,
					BNC_EarliestFinishUtc,
					BNC_LatestFinishUtc,
					BNC_ScheduledStartUtc,
					BNC_ScheduledFinishUtc,
					BNC_DurationMinutes,
					BNC_IsBuffered,
					BNC_ScaleUnit,
					BNC_ScaleMagnitude,
					BNC_ResolutionIncrement,
					BNC_GB_Branch,
					BNC_GE_Department,
					BNC_BufferPenetrationPercent,
					BNC_SystemCreateTimeUtc,
					BNC_SystemCreateUser,
					BNC_SystemLastEditTimeUtc,
					BNC_SystemLastEditUser
				) VALUES",
				string.Format(CultureInfo.InvariantCulture, @"
				(
					NEWID(), '{0}', {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, '{9}', {10}, {11}, {12}, {13}, {14}, GetUtcDate(), '~BP', GetUtcDate(), '~BP'
				)
				",
				shapePK,
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(earliestStartTime),
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(latestStartTime),
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(earliestFinishTime),
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(latestFinishTime),
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(scheduledStartTime),
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(scheduledFinishTime),
				durationMinutes,
				DbTextFormatTestHelper.BooleanToBitString(isBuffered),
				scaleUnit,
				scaleMag,
				resIncrement,
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(branch),
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(department),
				bufferPenetrationPercent));
		}

		public static InsertStatement GetAttachmentInsertSql(Guid pk, Guid? fromShapePK, Guid toShapePK, Guid ownerShapePK, string attachmentType, string approvedBy = "", string layoutData = "")
		{
			return new InsertStatement(
				@"
				INSERT dbo.BMNCNAttachment
				(
					BNA_PK,
					BNA_BNS_FromShape,
					BNA_BNS_ToShape,
					BNA_BNS_Owner,
					BNA_Type,
					BNA_SystemCreateTimeUtc,
					BNA_GS_NKApprovedBy,
					BNA_LayoutData,
					BNA_SystemCreateUser,
					BNA_SystemLastEditTimeUtc,
					BNA_SystemLastEditUser
				)
				VALUES",
				string.Format(CultureInfo.InvariantCulture, @"
				(
					'{0}',
					{1},
					'{2}',
					'{3}',
					'{4}',
					GetUtcDate(),
					'{5}',
					'{6}',
					'~BP',
					GetUtcDate(),
					'~BP'
				)
				", pk, DbTextFormatTestHelper.EncloseInQuotesOrNullString(fromShapePK), toShapePK, ownerShapePK, attachmentType, approvedBy, layoutData));
		}

		public const string BMSystemInsertSql = @"
			INSERT dbo.BMSystem (
				FS_PK, FS_Name, FS_SystemCreateTimeUtc, FS_SystemLastEditTimeUtc, FS_SystemCreateUser, FS_SystemLastEditUser
			) VALUES (
				'{0}', '{1}', GETDATE(), GETDATE(), '~BP', '~BP'
			)
			";

		public static string GetBMSystemInsertSql(Guid systemPk, string name)
		{
			return string.Format(CultureInfo.InvariantCulture,
				BMSystemInsertSql,
				systemPk,
				name);
		}

		public const string BMComponentInsertSql = @"
			INSERT dbo.BMComponent (
				FC_PK, FC_FS_System, FC_Type, FC_Name, FC_BufferTimespanInMinutes, FC_SystemCreateTimeUtc, FC_SystemLastEditTimeUtc, FC_GB_AgingBranch, FC_GE_AgingDepartment, FC_SystemCreateUser, FC_SystemLastEditUser
			) VALUES (
				'{0}', '{1}', '{2}', '{3}', {4}, GETDATE(), GETDATE(), '{5}', '{6}', '~BP', '~BP'
			)
			";

		public const string BMComponentWithParentComponentInsertSql = @"
			INSERT dbo.BMComponent (
				FC_PK, FC_FS_System, FC_Type, FC_Name, FC_BufferTimespanInMinutes, FC_FC_ParentComponent, FC_SystemCreateTimeUtc, FC_GB_AgingBranch, FC_GE_AgingDepartment, FC_SystemLastEditTimeUtc, FC_SystemCreateUser, FC_SystemLastEditUser
			) VALUES (
				'{0}', '{1}', '{2}', '{3}', {4}, '{5}', '{6}', '{7}', '{8}', GETDATE(), '~BP', '~BP'
			)
			";

		public static string GetBMComponentInsertSql(Guid componentPk, Guid systemPk, string type, string name, Guid parentComponentPk, DateTime createdTime, Guid? branchPK = null, Guid? departmentPK = null, int timespanInMinutes = 5760)
		{
			if (branchPK == null)
			{
				branchPK = GetABranchPK(Db.Connection);
			}

			if (departmentPK == null)
			{
				departmentPK = GetADepartmentPK(Db.Connection);
			}

			return string.Format(CultureInfo.InvariantCulture,
				BMComponentWithParentComponentInsertSql,
				componentPk,
				systemPk,
				type,
				name,
				type == "BUC" ? 0 : timespanInMinutes,
				parentComponentPk,
				createdTime,
				branchPK,
				departmentPK);
		}

		public static string GetBMComponentInsertSql(Guid componentPk, Guid systemPk, string type, string name, Guid? branchPK = null, Guid? departmentPK = null, int timespanInMinutes = 5760)
		{
			if (branchPK == null)
			{
				branchPK = GetABranchPK(Db.Connection);
			}

			if (departmentPK == null)
			{
				departmentPK = GetADepartmentPK(Db.Connection);
			}

			return string.Format(CultureInfo.InvariantCulture,
				BMComponentInsertSql,
				componentPk,
				systemPk,
				type,
				name,
				type == "BUC" ? 0 : timespanInMinutes,
				branchPK,
				departmentPK);
		}

		public static string GetBMComponentLinkInsertSql(Guid pk, Guid componentFrom, Guid componentTo, int sequence = 1)
		{
			return string.Format(CultureInfo.InvariantCulture, @"
			INSERT dbo.BMComponentLink (
				FL_PK, FL_FC_ComponentFrom, FL_FC_ComponentTo, FL_Sequence, FL_SystemCreateTimeUtc, FL_SystemLastEditTimeUtc, FL_SystemCreateUser, FL_SystemLastEditUser
			) VALUES (
				'{0}', '{1}', '{2}', {3}, GetUtcDate(), GetUtcDate(), '~BP', '~BP'
			)
			", pk, componentFrom, componentTo, sequence);
		}

		public static string GetBMComponentResourceLinkInsertSql(Guid pk, Guid componentPK, string resourceCode)
		{
			return string.Format(CultureInfo.InvariantCulture, @"
				INSERT dbo.BMComponentResourceLink (
					FD_PK, FD_FC_Component, FD_GS_NKResource, FD_SystemCreateTimeUtc, FD_SystemLastEditTimeUtc, FD_SystemCreateUser, FD_SystemLastEditUser
				) VALUES (
					'{0}', '{1}', '{2}', GetUtcDate(), GetUtcDate(), '~BP', '~BP'
				)
				", pk, componentPK, resourceCode);
		}

		public static string GetBMComponentAcceptabilityBandInsertSql(Guid bandPk, string name, DateTime createdTime)
		{
			return string.Format(CultureInfo.InvariantCulture, @"
				INSERT dbo.BMComponentAcceptabilityBand
				(
					BAB_PK, BAB_IsActive, BAB_Name, BAB_SystemCreateTimeUtc, BAB_SystemLastEditTimeUtc, BAB_SystemCreateUser, BAB_SystemLastEditUser
				)
				VALUES
				(
					'{0}', 1, '{1}', '{2}', GETDATE(), '~BP', '~BP'
				)
				",
				bandPk,
				name,
				createdTime);
		}

		public static string GetBMControlCustomisationInsertSql(Guid customisationPK, string name, string layoutData = null)
		{
			return string.Format(CultureInfo.InvariantCulture, @"
				INSERT dbo.BMControlCustomisation (
					FM_PK,
					FM_Name,
					FM_LayoutData,
					FM_SystemCreateTimeUtc,
					FM_SystemLastEditTimeUtc,
					FM_SystemCreateUser,
					FM_SystemLastEditUser,
				) VALUES (
					'{0}', '{1}', '{2}', GetUtcDate(), GetUtcDate(), '~BP', '~BP'
				)
				", customisationPK, name, layoutData);
		}

		public static string GetBMControlCustomisationLinkInsertSql(Guid linkPK, Guid customisationPK, Guid parentPK, string parentTableCode, string controlType = "TAS", string jobType = "")
		{
			return string.Format(CultureInfo.InvariantCulture, @"
				INSERT dbo.BMControlCustomisationLink (
					FML_PK,
					FML_FM_ControlCustomisation,
					FML_ParentId,
					FML_ParentTableCode,
					FML_ControlType,
					FML_JobType,
					FML_SystemCreateTimeUtc,
					FML_SystemLastEditTimeUtc,
					FML_SystemCreateUser,
					FML_SystemLastEditUser,
				) VALUES (
					'{0}', '{1}', '{2}', '{3}', '{4}', '{5}', GetUtcDate(), GetUtcDate(), '~BP', '~BP'
				)
				", linkPK, customisationPK, parentPK, parentTableCode, controlType, jobType);
		}

		public static string GetBMBoardInsertSql(Guid boardPK, Guid systemPK, string name = "Anne Board")
		{
			return string.Format(CultureInfo.InvariantCulture, @"
				INSERT dbo.BMBoard (
					MB_PK, MB_FS_System, MB_Name, MB_SystemCreateTimeUtc, MB_SystemCreateUser, MB_SystemLastEditTimeUtc, MB_SystemLastEditUser
				) VALUES (
					'{0}', '{1}', '{2}', GetUtcDate(), '~BP', GetUtcDate(), '~BP'
				)
				", boardPK, systemPK, name);
		}

		public static string GetBMBoardSlideshowInsertSql(Guid slideshowPK, string staffCode, string name = "Sloideshoe")
		{
			return string.Format(CultureInfo.InvariantCulture, @"
				INSERT dbo.BMBoardSlideShow (
					MD_PK, MD_Name, MD_IsPublished, MD_GS_NKStaffCode, MD_SystemCreateTimeUtc, MD_SystemLastEditTimeUtc, MD_SystemCreateUser, MD_SystemLastEditUser
				) VALUES (
					'{0}', '{1}', 1, '{2}', GetUtcDate(), GetUtcDate(), '~BP', '~BP'
				)
				", slideshowPK, name, staffCode);
		}

		public static string GetBMBoardSectionInsertSql(Guid sectionPK, Guid boardPK, Guid? componentPK = null, string layoutDataXML = null)
		{
			return string.Format(CultureInfo.InvariantCulture, @"
				INSERT dbo.BMBoardSection (
					MS_PK, MS_MB_Board, MS_FC_Component, MS_LayoutData, MS_SystemCreateTimeUtc, MS_SystemLastEditTimeUtc, MS_SystemCreateUser, MS_SystemLastEditUser
				) VALUES (
					'{0}', '{1}', {2}, '{3}', GetUtcDate(), GetUtcDate(), '~BP', '~BP'
				)
				",
				sectionPK,
				boardPK,
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(componentPK),
				layoutDataXML
				);
		}

		public static string GetBMBoardSectionWithComponentInsertSQL(Guid sectionPK, Guid componentPK, Guid boardPK, string sectionXML)
		{
			return string.Format(CultureInfo.InvariantCulture, @"
				INSERT dbo.BMBoardSection
					(MS_PK, MS_FC_Component, MS_MB_Board, MS_LayoutData, MS_GG_ReleaseGroup, MS_SectionType, MS_SystemCreateTimeUtc, MS_SystemLastEditTimeUtc, MS_SystemCreateUser, MS_SystemLastEditUser)
				VALUES
					('{0}', '{1}', '{2}', '{3}', null, 'CMP', GetUtcDate(), GetUtcDate(), '~BP', '~BP')
				", sectionPK, componentPK, boardPK, sectionXML);
		}

		public static string GetBMSystemRelatedStmSataInsertSql(Guid systemPK, string dataName, string content)
		{
			return string.Format(CultureInfo.InvariantCulture, @"
INSERT dbo.StmData (
	SD_PK, SD_Name, SD_Owner, SD_BinaryValue, SD_SystemCreateTimeUtc, SD_SystemLastEditTimeUtc, SD_SystemCreateUser, SD_SystemLastEditUser
) VALUES (
	NEWID(), '{0}', '{1}', dbo.CLRCompressStringAsBytes('{2}'), GetUtcDate(), GetUtcDate(), '~BP', '~BP'
)
", dataName + "_" + Guid.NewGuid(), systemPK, content);
		}

		public static string GetStmModuleFilterInsertSql(Guid pk, Guid? relatedEntity, string moduleId = "BMFilterRule", string filterName = "A layout", string filterType = "", string filterData = "0x1234", bool isPublished = true, bool isSystem = false, Guid? companyPK = null)
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				StmModuleFilterInsertSql,
				pk,
				moduleId,
				filterName,
				filterType,
				filterData,
				DbTextFormatTestHelper.BooleanToBitString(isPublished),
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(relatedEntity),
				DbTextFormatTestHelper.BooleanToBitString(isSystem),
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(companyPK));
		}

		public static string GetStmModuleFilterRuleInsertSql(Guid pk, Guid parentId, string parentTableCode, string moduleId = "BMFilterRule", string filterName = "", string filterData = "0x1234", bool isPublished = true, bool isSystem = false)
		{
			return string.Format(
				CultureInfo.InvariantCulture,
				StmModuleFilterRuleInsertSql,
				pk,
				moduleId,
				filterName.QuoteName('\''),
				filterData,
				DbTextFormatTestHelper.BooleanToBitString(isPublished),
				DbTextFormatTestHelper.BooleanToBitString(isSystem),
				parentId,
				parentTableCode);
		}

		const string StmModuleFilterInsertSql = @"
			INSERT dbo.StmModuleFilter (
				S9_PK, S9_ModuleID, S9_FilterName, S9_FilterType, S9_FilterData, S9_IsPublished, S9_RelatedEntityID, S9_IsSystem, S9_GC, S9_SystemCreateTimeUtc, S9_SystemLastEditTimeUtc, S9_SystemCreateUser, S9_SystemLastEditUser
			) VALUES (
				'{0}', '{1}', '{2}', '{3}', CONVERT(varbinary(max), {4}), {5}, {6}, {7}, {8}, GetUtcDate(), GetUtcDate(), '~BP', '~BP'
			)
			";

		const string StmModuleFilterRuleInsertSql = @"
			INSERT dbo.StmModuleFilter (
				S9_PK, S9_ModuleID, S9_FilterName, S9_FilterType, S9_FilterData, S9_IsPublished, S9_IsSystem, S9_ParentID, S9_ParentTableCode, S9_SystemCreateTimeUtc, S9_SystemLastEditTimeUtc, S9_SystemCreateUser, S9_SystemLastEditUser
			) VALUES (
				'{0}', '{1}', '{2}', 'FRU', CONVERT(varbinary(max), {3}), {4}, {5}, '{6}', '{7}', GetUtcDate(), GetUtcDate(), '~BP', '~BP'
			)
			";

		public const string StmModuleFilterUserDataInsertSql = @"
			INSERT dbo.StmModuleFilterUserData (
				S0_PK, S0_S9, S0_RelatedEntityID, S0_RelatedEntityTableCode, S0_FilterDataValues, S0_SystemCreateTimeUtc, S0_SystemLastEditTimeUtc, S0_SystemCreateUser, S0_SystemLastEditUser
			) VALUES (
				'{0}', '{1}', '{2}', '{3}', CONVERT(varbinary(max), {4}), GetUtcDate(), GetUtcDate(), '~BP', '~BP'
			)
			";

		public const string TagDefinitionInsertSql = @"
			INSERT dbo.TagDefinition (
				TGD_PK, TGD_Code, TGD_SystemCreateTimeUtc, TGD_SystemCreateUser, TGD_SystemLastEditTimeUtc, TGD_SystemLastEditUser
			) VALUES (
				'{0}', '{1}', GetUtcDate(), '~BP', GetUtcDate(), '~BP'
			)
			";

		public const string TagMagnitudeInsertSql = @"
			INSERT dbo.TagMagnitude (
				TGM_PK, TGM_Code, TGM_TGD_Tag, TGM_SystemCreateTimeUtc, TGM_SystemLastEditTimeUtc, TGM_SystemCreateUser, TGM_SystemLastEditUser
			) VALUES (
				'{0}', '{1}', '{2}', GetUtcDate(), GetUtcDate(), '~BP', '~BP'
			)
			";

		public const string TagMagnitudeWithDescriptionInsertSql = @"
			INSERT dbo.TagMagnitude (
				TGM_PK, TGM_Code, TGM_TGD_Tag, TGM_Description, TGM_SystemCreateTimeUtc, TGM_SystemLastEditTimeUtc, TGM_SystemCreateUser, TGM_SystemLastEditUser
			) VALUES (
				'{0}', '{1}', '{2}', '{3}', GetUtcDate(), GetUtcDate(), '~BP', '~BP'
			)
			";

		public static string GetTagDefinitionInsertSql(Guid pk, string code)
		{
			return string.Format(CultureInfo.InvariantCulture, TagDefinitionInsertSql, pk, code);
		}

		public static string GetTagMagnitudeInsertSql(Guid pk, string code, Guid definitionPK)
		{
			return string.Format(CultureInfo.InvariantCulture, TagMagnitudeInsertSql, pk, code, definitionPK);
		}

		public static string GetTagRuleInsertSql(Guid pk, string name, string actionType, Guid? branchPK = null, Guid? departmentPK = null)
		{
			if (branchPK == null)
			{
				branchPK = GetABranchPK(Db.Connection);
			}

			if (departmentPK == null)
			{
				departmentPK = GetADepartmentPK(Db.Connection);
			}

			return string.Format(CultureInfo.InvariantCulture, @"
				INSERT dbo.TagRule
				(
					TGR_PK, TGR_Name, TGR_ActionType, TGR_GB_Branch, TGR_GE_Department, TGR_SystemCreateTimeUtc, TGR_SystemLastEditTimeUtc, TGR_SystemCreateUser, TGR_SystemLastEditUser
				)
				VALUES
				(
					'{0}', '{1}', '{2}', '{3}', '{4}', GetUtcDate(), GetUtcDate(), '~BP', '~BP'
				)
				",
				pk, name, actionType, branchPK, departmentPK);
		}

		public static string GetTagLinkInsertSql(Guid pk, Guid magnitude, Guid parentID, int sequence = 0, string parentTableCode = "FH", string description = "")
		{
			return $@"
				INSERT dbo.TagLink (
					TGL_PK, TGL_TGM_Magnitude, TGL_ParentId, TGL_ParentTableCode, TGL_SystemCreateTimeUtc, TGL_Sequence, TGL_Description, TGL_SystemLastEditTimeUtc, TGL_SystemCreateUser, TGL_SystemLastEditUser
				) VALUES (
					'{pk}', '{magnitude}', '{parentID}', '{parentTableCode}', GetUtcDate(), {sequence}, '{description}', GetUtcDate(), '~BP', '~BP'
				)";
		}

		public static string GetWorkshopInsertSql(Guid pk, string code = "AAA")
		{
			return string.Format(CultureInfo.InvariantCulture, @"
				INSERT AnalyzerWorkshopEvent (
					LWE_PK, LWE_Code, LWE_Status, LWE_FromDateUtc, LWE_ToDateUtc, LWE_SystemCreateTimeUtc, LWE_SystemCreateUser, LWE_SystemLastEditTimeUtc, LWE_SystemLastEditUser
				) VALUES (
					'{0}', '{1}', 'OPN', GetUtcDate() - 1, GetUtcDate() + 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'
				)
				", pk, code);
		}

		public static string GetWorkshopTeamInsertSql(Guid pk, Guid workshopPK, int teamNumber, string name = "Vapple", string resultsXml = "")
		{
			return string.Format(CultureInfo.InvariantCulture, @"
				INSERT AnalyzerWorkshopTeam (
					LWT_PK, LWT_LWE_Event, LWT_TeamName, LWT_WorkshopResults, LWT_SystemCreateTimeUtc, LWT_SystemLastEditTimeUtc, LWT_GS_NKLoginUser, LWT_SystemCreateUser, LWT_SystemLastEditUser, LWT_TeamNumber
				) VALUES (
					'{0}', '{1}', '{2}', '{3}', GetUtcDate(), GetUtcDate(), 'E', 'E', 'E', {4}
				)
				", pk, workshopPK, name, resultsXml, teamNumber);
		}

		public static string GetWorkshopDesignInsertSql(Guid pk, Guid workshopPK, string name = "Snarry", Guid boardPK = default(Guid))
		{
			var result = new StringBuilder();

			if (boardPK == default(Guid))
			{
				var systemPK = Guid.NewGuid();
				boardPK = Guid.NewGuid();

				result.AppendFormat(CultureInfo.InvariantCulture, BMSystemInsertSql, systemPK, "Hrrrrnngh");
				result.AppendFormat(CultureInfo.InvariantCulture, GetBMBoardInsertSql(boardPK, systemPK));
			}

			result.AppendFormat(CultureInfo.InvariantCulture, @"
				INSERT AnalyzerDesign (
					LWD_PK, LWD_LWE_Event, LWD_Name, LWD_MB_Board, LWD_SystemCreateTimeUtc, LWD_SystemCreateUser, LWD_SystemLastEditTimeUtc, LWD_SystemLastEditUser
				) VALUES (
					'{0}', '{1}', '{2}', '{3}', GetUtcDate(), '~BP', GetUtcDate(), '~BP'
				)
				", pk, workshopPK, name, boardPK);

			return result.ToString();
		}

		public static string GetWorkshopRunInsertSql(Guid pk, Guid designPK, Guid teamPK, Guid boardPK = default(Guid))
		{
			var result = new StringBuilder();

			if (boardPK == default(Guid))
			{
				var systemPK = Guid.NewGuid();
				boardPK = Guid.NewGuid();

				result.AppendFormat(CultureInfo.InvariantCulture, BMSystemInsertSql, systemPK, "Hrrrrnngh");
				result.AppendFormat(CultureInfo.InvariantCulture, GetBMBoardInsertSql(boardPK, systemPK));
			}

			result.AppendFormat(CultureInfo.InvariantCulture, @"
				INSERT AnalyzerRun (
					LWR_PK, LWR_LWD_ExperimentDesign, LWR_SystemCreateTimeUtc, LWR_SystemCreateUser, LWR_LWT_StartingTeam, LWR_CurrentPace, LWR_MB_Board, LWR_SystemLastEditTimeUtc, LWR_SystemLastEditUser
				) VALUES (
					'{0}', '{1}', GETDATE(), 'E', '{2}', 1, '{3}', GETDATE(), 'E'
				)
				", pk, designPK, teamPK, boardPK);

			return result.ToString();
		}

		public static string GetDesignWorkflowTemplateInsertSql(Guid pk, Guid designPK, Guid templatePK)
		{
			return string.Format(CultureInfo.InvariantCulture, @"
				INSERT AnalyzerDesignWorkflowTemplate (
					LWF_PK, LWF_LWD_ExperimentDesign, LWF_P0_Template, LWF_DemandQuantity, LWF_SystemCreateTimeUtc, LWF_SystemLastEditTimeUtc, LWF_SystemCreateUser, LWF_SystemLastEditUser
				) VALUES (
					'{0}', '{1}', '{2}', 1, GetUtcDate(), GetUtcDate(), '~BP', '~BP'
				)
				", pk, designPK, templatePK);
		}

		public static string GetWorkflowTemplateInsertSql(Guid pk, string name, string processType = "WKI",
			string subType1 = "", string subType2 = "", string subType3 = "", string subType4 = "", string subType5 = "",
			Guid? clientPK = null, string loadPort = "", string dischargePort = "", Guid? branchPK = null, Guid? deptPK = null, Guid? warehousePK = null, Guid? companyPK = null,
			bool isSystem = false, bool isUniversal = false, bool isPartial = false, bool isActive = true,
			DateTime? createTime = null)
		{
			return string.Format(CultureInfo.InvariantCulture, @"
				INSERT dbo.ProcessTaskTemplate (
					P0_PK,
					P0_ProcessType,
					P0_Name,
					P0_SubType1,
					P0_SubType2,
					P0_SubType3,
					P0_SubType4,
					P0_SubType5,
					P0_OH_Client,
					P0_LoadPortCountry,
					P0_DischargePortCountry,
					P0_GB,
					P0_GE,
					P0_WW,
					P0_GC,
					P0_IsSystem,
					P0_IsUniversal,
					P0_IsPartialTemplate,
					P0_IsActive,
					P0_SystemCreateTimeUTC,
					P0_SystemCreateUser,
					P0_SystemLastEditTimeUtc,
					P0_SystemLastEditUser
				) VALUES (
					'{0}',
					'{1}',
					'{2}',
					'{3}',
					'{4}',
					'{5}',
					'{6}',
					'{7}',
					{8},
					'{9}',
					'{10}',
					{11},
					{12},
					{13},
					{14},
					'{15}',
					'{16}',
					'{17}',
					'{18}',
					{19},
					'~BP',
					GetUtcDate(),
					'~BP'
				)
				", pk, processType, name,
				subType1, subType2, subType3, subType4, subType5,
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(clientPK), loadPort, dischargePort,
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(branchPK),
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(deptPK),
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(warehousePK),
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(companyPK),
				DbTextFormatTestHelper.BooleanToBitString(isSystem),
				DbTextFormatTestHelper.BooleanToBitString(isUniversal),
				DbTextFormatTestHelper.BooleanToBitString(isPartial),
				DbTextFormatTestHelper.BooleanToBitString(isActive),
				DbTextFormatTestHelper.EncloseInQuotesOrNullString(createTime)
				);
		}

		public static string GetGlbStaffInsertSql(Guid pk, string code, string loginName)
		{
			return string.Format(CultureInfo.InvariantCulture, @"
				INSERT dbo.GlbStaff (
					GS_PK,
					GS_Code,
					GS_LoginName,
					GS_SystemCreateTimeUtc,
					GS_SystemCreateUser,
					GS_SystemLastEditTimeUtc,
					GS_SystemLastEditUser
				) VALUES (
					'{0}',
					'{1}',
					'{2}',
					GetUtcDate(),
					'~BP',
					GetUtcDate(),
					'~BP'
				)
				", pk, code, loginName.QuoteEscapedName('\''));
		}

		public static string GetGlbCapabilityInsertSql(Guid pk, string code, string membershipRequirements = null)
		{
			return string.Format(CultureInfo.InvariantCulture, @"
				INSERT dbo.GlbCapability (
					G4_PK,
					G4_Code,
					G4_MembershipRequirements,
					G4_SystemCreateTimeUtc,
					G4_SystemLastEditTimeUtc,
					G4_SystemCreateUser,
					G4_SystemLastEditUser
				) VALUES (
					'{0}',
					'{1}',
					'{2}',
					GetUtcDate(),
					GetUtcDate(),
					'~BP',
					'~BP'
				)
				", pk, code, membershipRequirements);
		}

		public static string GetGlbGroupInsertSql(Guid pk, string code)
		{
			return string.Format(CultureInfo.InvariantCulture, @"
				INSERT dbo.GlbGroup (
					GG_PK,
					GG_Code,
					GG_SystemCreateTimeUtc,
					GG_SystemCreateUser,
					GG_SystemLastEditTimeUtc,
					GG_SystemLastEditUser
				) VALUES (
					'{0}',
					'{1}',
					GetUtcDate(),
					'~BP',
					GetUtcDate(),
					'~BP'
				)
				", pk, code);
		}

		public static string GetSystemReleaseGroupInsertSql(Guid pk, Guid systemPK, Guid groupPK)
		{
			return string.Format(CultureInfo.InvariantCulture, @"
				INSERT INTO dbo.BMSystemReleaseGroup (
					FSG_PK,
					FSG_FS_System,
					FSG_GG_Group,
					FSG_ResourceCountdownTime,
					FSG_DeterminationParameters,
					FSG_SystemCreateTimeUtc,
					FSG_SystemLastEditTimeUtc,
					FSG_SystemCreateUser,
					FSG_SystemLastEditUser
				) VALUES (
					'{0}',
					'{1}',
					'{2}',
					'{3}',
					'{4}',
					GetUtcDate(),
					GetUtcDate(),
					'~BP',
					'~BP'
				)				
				", pk, systemPK, groupPK, null, "");
		}

		public static string GetGlbCompanyInsertSql(Guid pk, string code, string name, string address, string countryCode, string currencyCode = "AUD")
		{
			return string.Format(CultureInfo.InvariantCulture, @"
				INSERT INTO dbo.GlbCompany (
					GC_PK,
					GC_Code,
					GC_Name,
					GC_Address1,
					GC_RN_NKCountryCode,
					GC_RX_NKLocalCurrency,
					GC_SystemCreateTimeUtc,
					GC_SystemLastEditTimeUtc,
					GC_SystemCreateUser,
					GC_SystemLastEditUser
				) VALUES (
					'{0}',
					'{1}',
					'{2}',
					'{3}',
					'{4}',
					'{5}',
					GetUtcDate(),
					GetUtcDate(),
					'~BP',
					'~BP'
				)				
				", pk, code, name, address, countryCode, currencyCode);
		}

		public static Guid GetABranchPK(DbConnection connection)
		{
			return (Guid)connection.ExecuteScalar("SELECT TOP 1 GB_PK FROM dbo.GlbBranch");
		}

		public static Guid GetADepartmentPK(DbConnection connection)
		{
			return (Guid)connection.ExecuteScalar("SELECT TOP 1 GE_PK FROM dbo.GlbDepartment");
		}

		public static Guid GetWorkQueueTagGroupPK(DbConnection connection)
		{
			return (Guid)connection.ExecuteScalar("SELECT TGD_PK FROM dbo.TagDefinition WHERE TGD_Code = 'QUE'");
		}

		public static string GetABCSoftwareActionInsertSql(Guid pk, string actionType = "SQL", string description = "Love Action", string triggerCondition = "")
		{
			return string.Format(CultureInfo.InvariantCulture, @"
				INSERT INTO ABCSoftwareAction
				(KS_PK, KS_ActionType, KS_Description, KS_TriggerCondition, KS_SystemCreateTimeUtc, KS_SystemCreateUser, KS_SystemLastEditTimeUtc, KS_SystemLastEditUser)
				VALUES
				('{0}', '{1}', '{2}', '{3}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')",
				pk, actionType, description, triggerCondition);
		}

		public static string GetABCSoftwareConsequenceInsertSql(Guid pk, Guid actionPK, string staffCode, string consequenceType = "TOA", string consequence = "")
		{
			return string.Format(CultureInfo.InvariantCulture, @"
				INSERT INTO ABCSoftwareConsequence
				(KQ_PK, KQ_KS_Action, KQ_ConsequenceType, KQ_Consequence, KQ_GS_NKStaff, KQ_SystemCreateTimeUtc, KQ_SystemCreateUser, KQ_SystemLastEditTimeUtc, KQ_SystemLastEditUser)
				VALUES
				('{0}', '{1}', '{2}', '{3}', '{4}', , GETUTCDATE(), 'E', GETUTCDATE(), 'E')",
				pk, actionPK, consequenceType, consequence, staffCode);
		}
	}
}

#endif
