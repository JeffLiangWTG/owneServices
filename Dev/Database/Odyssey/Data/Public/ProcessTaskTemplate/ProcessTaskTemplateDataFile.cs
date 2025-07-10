using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class ProcessTaskTemplateDataFile : EmbeddedDataFile
	{
		public ProcessTaskTemplateDataFile() : base(DataFileRelativePath, DataFileTables)
		{
		}

		internal ProcessTaskTemplateDataFile(string customDataFileRelativePath) : base(customDataFileRelativePath, DataFileTables)
		{
		}

		protected override string SelectQuery
		{
			get
			{
				return @"
SELECT * FROM dbo.ProcessTaskTemplate WHERE P0_IsSystem = 1 ORDER BY P0_PK;
SELECT * FROM dbo.ProcessTasks WHERE P9_ParentID IN (SELECT P0_PK FROM dbo.ProcessTaskTemplate WHERE P0_IsSystem = 1) ORDER BY P9_PK;
SELECT * FROM dbo.ProcessTemplateTrigger WHERE P9T_P0_Template IN (SELECT P0_PK FROM dbo.ProcessTaskTemplate WHERE P0_IsSystem = 1) ORDER BY P9T_PK;
SELECT * FROM dbo.ProcessTaskNotification WITH (FORCESEEK)
WHERE
	PQ_P9T_Trigger IN (SELECT P9T_PK FROM dbo.ProcessTemplateTrigger WHERE P9T_P0_Template IN (SELECT P0_PK FROM dbo.ProcessTaskTemplate WHERE P0_IsSystem = 1))
	OR PQ_P9 IN
	(
		SELECT P9_PK
		FROM
			dbo.ProcessTasks WITH (FORCESEEK, INDEX = NR_RC__P9_ParentID)
		WHERE P9_ParentTableCode='P0' AND P9_ParentID IN (SELECT P0_PK FROM dbo.ProcessTaskTemplate WHERE P0_IsSystem = 1))
ORDER BY
	PQ_PK;
";
			}
		}

		#region Implementation

		const string DataFileRelativePath = @"Public\ProcessTaskTemplate\ProcessTaskTemplate.xml";
		public override string ResourceRelativeName => "ProcessTaskTemplate.ProcessTaskTemplate.xml";

		static string[] DataFileTables => new string[]
		{
			ProcessTaskTemplateSchema.Constants.TableName,
			ProcessTasksSchema.Constants.TableName,
			ProcessTemplateTriggerSchema.Constants.TableName,
			ProcessTaskNotificationSchema.Constants.TableName,
		};

		#endregion
	}
}
