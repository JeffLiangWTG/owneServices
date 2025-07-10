using System;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Accounting
{
	public class UpdateJobChargeProFormaCostAndRevenue : DataTransformation
	{
		public override string UserDescription => "Update JR_ProFormaRevenue and JR_ProFormaCost columns of JobCharge table";

		public UpdateJobChargeProFormaCostAndRevenue() : this(1000)
		{
		}

		internal UpdateJobChargeProFormaCostAndRevenue(int batchSize) : base()
		{
			BatchSize = batchSize;
		}

		readonly int BatchSize;

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var jobChargeTableSize = ExtProperty.Table.Select(Db.Connection, JobChargeSchema.Constants.SqlSchemaName, JobChargeSchema.Constants.TableName, JobChargeTableSizePropertyName);
			var lastProcessedGuidString = ExtProperty.Table.Select(Db.Connection, JobChargeSchema.Constants.SqlSchemaName, JobChargeSchema.Constants.TableName, LastProcessedJobChargePropertyName);

			if (string.IsNullOrEmpty(jobChargeTableSize))
			{
				var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, JobChargeSchema.Constants.TableName);
				jobChargeTableSize = rowCount.ToString();
				ExtProperty.Table.Update(Db.Connection, JobChargeSchema.Constants.SqlSchemaName, JobChargeSchema.Constants.TableName, JobChargeTableSizePropertyName, jobChargeTableSize);
			}
			var count = long.Parse(jobChargeTableSize);
			Guid? lastProcessedGuid = string.IsNullOrEmpty(lastProcessedGuidString) ? null : Guid.Parse(lastProcessedGuidString);
			foreach (var chunk in GuidChunker.GenerateChunks(BatchSize, count, lastProcessedGuid))
			{
				var stopWatch = Stopwatch.StartNew();
				ProcessData(chunk.LowerBound, chunk.UpperBound);
				if (token.IsCancellationRequested || stopWatch.Elapsed.TotalMinutes > 1)
				{
					lastProcessedGuidString = chunk.UpperBound.ToString();
					ExtProperty.Table.Update(Db.Connection, JobChargeSchema.Constants.SqlSchemaName, JobChargeSchema.Constants.TableName, LastProcessedJobChargePropertyName, lastProcessedGuidString);
					manager.ShowInfoMessage($"PK of Last processed record: {lastProcessedGuidString}, total no of records: {jobChargeTableSize}.");
					token.ThrowIfCancellationRequested();
					stopWatch.Restart();
				}
			}
			ExtProperty.Table.Delete(Db.Connection, JobChargeSchema.Constants.SqlSchemaName, JobChargeSchema.Constants.TableName, LastProcessedJobChargePropertyName);
			ExtProperty.Table.Delete(Db.Connection, JobChargeSchema.Constants.SqlSchemaName, JobChargeSchema.Constants.TableName, JobChargeTableSizePropertyName);
		}

		void ProcessData(Guid lowerBound, Guid upperBound)
		{
			string sql = @"
DECLARE @FilteredCharges TABLE (
	JH_PK UNIQUEIDENTIFIER NOT NULL,
	JR_PK UNIQUEIDENTIFIER NOT NULL,
	JR_ProFormaCost BIT,
	JR_ProformaRevenue BIT
);

DECLARE @FilteredChargesWithParentTableCode TABLE (
	JR_PK UNIQUEIDENTIFIER NOT NULL,
	JH_ParentTableCode VARCHAR(3) COLLATE DATABASE_DEFAULT
);

INSERT INTO @FilteredCharges (JH_PK, JR_PK, JR_ProFormaCost,JR_ProformaRevenue )
SELECT JR_JH, JR_PK, JR_ProFormaCost, JR_ProformaRevenue
FROM dbo.JobCharge WITH (FORCESEEK, INDEX (PK_UX__JR_PK))
WHERE JR_PK BETWEEN @LowerBound AND @UpperBound;

INSERT INTO @FilteredChargesWithParentTableCode (JR_PK, JH_ParentTableCode)
SELECT FC.JR_PK, JH.JH_ParentTableCode
FROM @FilteredCharges FC
JOIN dbo.JobHeader JH WITH (FORCESEEK, INDEX (PK_UX__JH_PK)) ON FC.JH_PK = JH.JH_PK
WHERE ((FC.JR_ProFormaCost = 1 OR FC.JR_ProformaRevenue = 1) AND JH.JH_ParentTableCode <> 'TH')
   OR ((FC.JR_ProFormaCost = 0 OR FC.JR_ProformaRevenue = 0) AND JH.JH_ParentTableCode = 'TH');

UPDATE dbo.JobCharge
SET JR_ProFormaCost = IIF(FC.JH_ParentTableCode = 'TH' , 1, 0),
	JR_ProFormaRevenue = IIF(FC.JH_ParentTableCode = 'TH' , 1, 0),
	JR_SystemLastEditTimeUtc = GETUTCDATE(),
	JR_SystemLastEditUser = '~BP'
FROM @FilteredChargesWithParentTableCode FC
JOIN dbo.JobCharge JC ON FC.JR_PK = JC.JR_PK";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@LowerBound", lowerBound, JobChargeSchema.PK);
				cmd.AddParameterBasedOnDbColumn("@UpperBound", upperBound, JobChargeSchema.PK);

				cmd.ExecuteNonQuery();
			}
		}

		const string LastProcessedJobChargePropertyName = "LastProcessedGuidForUpdateJRProformaCostOrRevenue";
		const string JobChargeTableSizePropertyName = "JobChargeTableSize";
	}
}
