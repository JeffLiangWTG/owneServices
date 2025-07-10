using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common.HelperClasses;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement
{
	public class PopulateProcessHeaderJobCodeAndJobDescriptionFromJLW : DataTransformation
	{
		const string LastProcessedPK = "PopulateProcessHeaderJobCodeAndJobDescriptionFromJLW_LastProcessedPK";

		public override string UserDescription => "Populate Process Header Job Code And Job Description From Job-Level Workflow";

		const string UpdateSQL = @"
UPDATE header
SET
	FH_SystemLastEditTimeUtc = GETUTCDATE(),
	FH_SystemLastEditUser = '~BP',
	FH_JobCode = parent.FH_JobCode,
	FH_JobDescription = parent.FH_JobDescription
FROM dbo.ProcessHeader parent
JOIN dbo.ProcessHeader header 
	WITH (FORCESEEK, INDEX(FK_RX__FH_FH_ParentHeader))
	ON header.FH_FH_ParentHeader = parent.FH_PK
WHERE
	parent.FH_ParentId IS NOT NULL
	AND parent.FH_JobCode <> ''
	AND parent.FH_JobDescription <> ''
	AND parent.FH_ParentId BETWEEN @From AND @To
OPTION(FORCE ORDER, MAXDOP 1)
";

		void ProcessChunk(Guid from, Guid to)
		{
			using var command = Db.Connection.Command(UpdateSQL);
			command.AddParameter("@From", SqlDbType.UniqueIdentifier, from);
			command.AddParameter("@To", SqlDbType.UniqueIdentifier, to);
			command.ExecuteNonQuery();
		}

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, ProcessHeaderSchema.Constants.TableName);
			var operation = new GuidChunkingOperation(manager, 5000, rowCount, ProcessChunk, LastProcessedPK, token);
			operation.DoChunking();
		}
	}
}
