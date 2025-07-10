using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA
{
	public class RemoveDeclarationException060 : DataTransformation
	{
		public override string UserDescription => "Remove declaration exception code 060 unless there is a discrepancy in duty/tax";

		public RemoveDeclarationException060()
			: this(1000)
		{
		}

		internal RemoveDeclarationException060(int batchSize)
		{
			BatchSize = batchSize;
		}

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			if (IsCACustoms)
			{
				var watermark = ClusterKeyWatermark.Select() ?? ClusterKeyWatermark.CreateNew();
				var start = watermark.Value;
				var end = GetNextClusterKeyRangeRollingForward(start);

				do
				{
					ShowInfoMessage($"Processing batch with cluster key ranging from {start} to {end}...");
					using (var cmd = Db.Connection.Command(MainQuery))
					{
						cmd.AddParameter("@Start", SqlDbType.Int, start);
						cmd.AddParameter("@End", SqlDbType.Int, end);
						ShowInfoMessage($"\t{cmd.ExecuteNonQuery()} records processed.");
					}
					watermark.Value = start = end;
					end = GetNextClusterKeyRangeRollingForward(start);
					token.ThrowIfCancellationRequested();
				}
				while (start != end);

				watermark.Delete();
			}
		}

		int GetNextClusterKeyRangeRollingForward(int clusterKey)
		{
			var sql = @"
SELECT
	ISNULL(MAX(JE_ClusterKey), @ClusterKey)
FROM
(
	SELECT TOP (@BatchSize)
		JE_ClusterKey
	FROM
		dbo.JobDeclaration
	WHERE
		JE_ClusterKey > @ClusterKey
	ORDER BY
		JE_ClusterKey ASC
) AS range
";
			return Db.Connection.ExecuteScalar<int>(sql, x =>
			{
				x.AddParameter("@BatchSize", SqlDbType.Int, BatchSize);
				x.AddParameter("@ClusterKey", SqlDbType.Int, clusterKey);
			});
		}

		void ShowInfoMessage(string message) => manager?.ShowInfoMessage(message);

		internal bool IsCACustoms => Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'CA'");

		readonly int BatchSize;

		string MainQuery => @"
DECLARE @JE_PKs TABLE
(
	JE_PK UNIQUEIDENTIFIER NOT NULL,
	UNIQUE CLUSTERED (JE_PK)
)

BEGIN TRY
	BEGIN TRANSACTION

	UPDATE
		job
	SET
		job.JE_AddInfo = AddInfo.AddInfoValue
		, job.JE_SystemLastEditTimeUtc = GETUTCDATE()
		, job.JE_SystemLastEditUser = '~BP'
	OUTPUT
		INSERTED.JE_PK
	INTO 
		@JE_PKs
	FROM
		dbo.CAJobDeclaration AS idx
		JOIN dbo.JobDeclaration AS job ON job.JE_ClusterKey = idx.JE_ClusterKey
		JOIN dbo.CusEntryHeader ON CH_ClusterKey = idx.JE_ClusterKey AND CH_MessageType = 'CAD'
		OUTER APPLY dbo.csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline(idx.JE_AddInfo, 'DeclarationException') AS AddInfo
		OUTER APPLY
		(
			SELECT
				cw1Value = SUM(CASE WHEN CF_Source = 'CW1' AND CF_ChargeType IN ('DTY', 'GST', 'GSD', 'EXS', 'SIN', 'SIM') THEN CF_ChargeAmount ELSE 0 END), 
				cusValue = SUM(CASE WHEN CF_Source = 'CUS' AND CF_ChargeType IN ('CUD', 'GST', 'FET', 'ADD', 'CVD', 'SUR') THEN CF_ChargeAmount ELSE 0 END)
			FROM
				dbo.CusEntryLine
				JOIN dbo.CusEntryLineFee ON CF_CL = CL_PK AND CF_ClusterKey = idx.JE_ClusterKey
			WHERE
				CL_CH = CH_PK AND CL_ClusterKey = idx.JE_ClusterKey
		) a
	WHERE
		idx.JE_MessageType IN ('IMP', 'LVS')
		AND idx.JE_DeclarationException = '060'
		AND idx.JE_ClusterKey BETWEEN @Start AND @End
		AND job.JE_DataModel = 'CA'
		AND ISNULL(cw1Value, 0) = ISNULL(cusValue, 0)


	DELETE
		dbo.GenAddOnColumn
	WHERE
		XA_Name = 'CA_DeclarationException'
		AND XA_ParentID IN (SELECT JE_PK FROM @JE_PKs)

	COMMIT
END TRY
BEGIN CATCH
	IF (@@TRANCOUNT > 0)
		ROLLBACK TRANSACTION;
	THROW;
END CATCH
";
	}

	internal class ClusterKeyWatermark
	{
		const string Watermark = "RemoveDeclarationException060_Watermark";

		public int Value
		{
			get => _value;
			set
			{
				if (_value != value)
				{
					_value = value;
					Update();
				}
			}
		}

		int _value;

		ClusterKeyWatermark()
		{
		}

		public static ClusterKeyWatermark CreateNew()
		{
			var watermark = new ClusterKeyWatermark();

			try
			{
				var sql = @"
SELECT TOP 1
	JE_ClusterKey
FROM
	dbo.JobDeclaration 
WHERE
	JE_DataModel = 'CA'
	AND JE_MessageType IN ('IMP', 'LVS') 
	AND JE_SystemCreateTimeUtc <= @Start
ORDER BY
	JE_ClusterKey DESC
";
				watermark.Value = Db.Connection.ExecuteScalar<int>(sql, x => x.AddParameter("@Start", SqlDbType.SmallDateTime, new DateTime(2023, 01, 01)));
			}
			catch (ExecuteScalarReturnedNullException)
			{
				watermark.Value = 0;
			}

			return watermark;
		}

		public static ClusterKeyWatermark Select()
		{
			var watermark = new ClusterKeyWatermark();
			return int.TryParse(ExtProperty.Database.Select(Db.Connection, Watermark), out watermark._value) ? watermark : null;
		}

		void Update() => ExtProperty.Database.Update(Db.Connection, Watermark, _value.ToString());

		public void Delete() => ExtProperty.Database.Delete(Db.Connection, Watermark);
	}
}
