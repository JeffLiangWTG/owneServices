using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Documents
{
	public class UpdateScheduledReportTemplateForJobHistoryReportTransformation : DataTransformation
	{
		public override string UserDescription => "Update Report Schedule Task for Job History Report to not include FieldName for Date Range Field";

		const string TempTableName = "ClientTransformTable_WI00769394";
		const string JobHistoryReportPK = "CFD5875F-1F58-4E39-A775-AB5FEFF9D5CC";

		protected override void OfflinePostUpgradeTransform()
		{
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, TempTableName, $@"
CREATE TABLE dbo.{TempTableName}
(
	JHR_S5_PK UNIQUEIDENTIFIER NOT NULL,
	JHR_FailedBatch bit NOT NULL DEFAULT ((0)),
	JHR_FailedIndividually bit NOT NULL DEFAULT ((0))
);");

			var sql = $@"
BEGIN
	INSERT INTO dbo.{TempTableName} (JHR_S5_PK)
	SELECT
		S5_PK
	FROM
		dbo.StmScheduleTask
	WHERE
		S5_ParentID = @JobHistoryReportPK
		AND S5_ScheduleType = 'REP'
END
";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@JobHistoryReportPK", SqlDbType.UniqueIdentifier, new Guid(JobHistoryReportPK));
				cmd.ExecuteNonQuery();
			}
		}

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var assembly = Assembly.LoadFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Enterprise.DocumentEngine.dll"));

			RunTransformation(assembly, token, "Enterprise.DocumentEngine.Transformation.UpdateScheduledReportTemplateForJobHistoryReport");
		}

		void RunTransformation(Assembly assembly, CancellationToken token, string transformationClassName)
		{
			var transformType = assembly.GetType(transformationClassName, true);
			var transform = Activator.CreateInstance(transformType);
			transformType.GetMethod("Run").Invoke(transform, new object[] { new Action<string>(s => manager?.ShowInfoMessage(s)), token });
		}
	}
}
