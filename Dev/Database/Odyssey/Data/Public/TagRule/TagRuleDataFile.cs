using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class TagRuleDataFile : EmbeddedDataFile
	{
		public TagRuleDataFile() : base(DataFileRelativePath, DataFileTables)
		{
		}

		internal TagRuleDataFile(string customDataFileRelativePath) : base(customDataFileRelativePath, DataFileTables)
		{
		}

		const string DataFileRelativePath = @"Public\TagRule\TagRule.xml";
		public override string ResourceRelativeName => "TagRule.TagRule.xml";

		static string[] DataFileTables
		{
			get
			{
				return new[]
				{
					TagDefinitionSchema.Constants.TableName,
					TagMagnitudeSchema.Constants.TableName,
					TagLinkSchema.Constants.TableName,
					StmModuleFilterSchema.Constants.TableName,
					StmModuleFilterUserDataSchema.Constants.TableName,
					TagRuleSchema.Constants.TableName,
				};
			}
		}

		protected override string SelectQuery
		{
			get
			{
				return @"
SELECT td.* FROM dbo.TagDefinition td
	WHERE TGD_IsSystem = 1
	ORDER BY 1;
SELECT TGM_PK, TGM_Code, TGM_Description, TGM_RuleRunSequence, TGM_TGD_Tag, TGM_SystemCreateTimeUtc, TGM_SystemLastEditTimeUtc, TGM_SystemCreateUser, TGM_SystemLastEditUser
	FROM dbo.TagMagnitude tm
	JOIN dbo.TagDefinition ON TGM_TGD_Tag = TGD_PK
	WHERE TGD_IsSystem = 1
	AND TGD_Code <> 'QUE'
	ORDER BY 1;
SELECT tl.* FROM dbo.TagLink tl
	JOIN dbo.TagRule on TGR_PK = TGL_ParentId and TGL_ParentTableCode = 'TGR'
	JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK
	JOIN dbo.TagDefinition on TGM_TGD_Tag = TGD_PK
	WHERE TGR_IsSystem = 1
	AND TGD_Code <> 'QUE'
UNION
SELECT tl.* FROM dbo.TagLink tl
	WHERE TGL_ParentId NOT IN (SELECT TGR_PK FROM dbo.TagRule)
	AND TGL_ParentTableCode = 'TGR'
	ORDER BY 1;
SELECT S9_PK, S9_FilterData, S9_FilterName, S9_FilterType, S9_IsPublished, S9_IsSystem, S9_ModuleID, S9_ParentID, S9_ParentTableCode, S9_SaveColumnLayout, S9_SystemCreateTimeUtc, S9_SystemLastEditTimeUtc, S9_SystemCreateUser, S9_SystemLastEditUser
	FROM dbo.StmModuleFilter stm
	JOIN dbo.TagRule on TGR_PK = S9_ParentID
	JOIN dbo.TagLink on TGR_PK = TGL_ParentId and TGL_ParentTableCode = 'TGR'
	JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK
	JOIN dbo.TagDefinition on TGM_TGD_Tag = TGD_PK
	WHERE TGR_IsSystem = 1
	AND TGD_Code <> 'QUE'
UNION
SELECT S9_PK, S9_FilterData, S9_FilterName, S9_FilterType, S9_IsPublished, S9_IsSystem, S9_ModuleID, S9_ParentID, S9_ParentTableCode, S9_SaveColumnLayout, S9_SystemCreateTimeUtc, S9_SystemLastEditTimeUtc, S9_SystemCreateUser, S9_SystemLastEditUser
	FROM dbo.StmModuleFilter stm
	WHERE S9_ParentID NOT IN (SELECT TGR_PK FROM dbo.TagRule)
	AND S9_ParentTableCode = 'TGR'
	ORDER BY 1;
SELECT dat.* FROM dbo.StmModuleFilterUserData dat
	JOIN dbo.StmModuleFilter on S0_S9 = S9_PK
	JOIN dbo.TagRule on TGR_PK = S9_ParentID
	JOIN dbo.TagLink on TGR_PK = TGL_ParentId and TGL_ParentTableCode = 'TGR'
	JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK
	JOIN dbo.TagDefinition on TGM_TGD_Tag = TGD_PK
	WHERE TGR_IsSystem = 1
	AND TGD_Code <> 'QUE'
UNION
SELECT dat.* FROM dbo.StmModuleFilterUserData dat
	JOIN dbo.StmModuleFilter on S0_S9 = S9_PK
	WHERE S9_ParentID NOT IN (SELECT TGR_PK FROM dbo.TagRule)
	AND S9_ParentTableCode = 'TGR'
	ORDER BY 1;
SELECT rul.* FROM dbo.TagRule rul
	JOIN dbo.TagLink on TGR_PK = TGL_ParentId and TGL_ParentTableCode = 'TGR'
	JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK
	JOIN dbo.TagDefinition on TGM_TGD_Tag = TGD_PK
	WHERE TGR_IsSystem = 1
	AND TGD_Code <> 'QUE'
	ORDER BY 1";
			}
		}
	}
}
