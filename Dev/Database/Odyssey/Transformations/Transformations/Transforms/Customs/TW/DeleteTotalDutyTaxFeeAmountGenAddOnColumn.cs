using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.TW
{
	public class DeleteTotalDutyTaxFeeAmountGenAddOnColumn : DataTransformation
	{
		readonly int batchSize = 5000;

		public override string UserDescription => "Delete TotalDutyTaxFeeAmount from GenAddOnColumn";

		string DeleteSql => $@"
				DELETE Top ({batchSize}) dbo.GenAddOnColumn
				WHERE XA_ParentTableCode = 'C9'
					AND XA_Name = 'TotalDutyTaxFeeAmount'
					AND EXISTS(SELECT NULL
								FROM dbo.CusEntryPayInfo
									JOIN dbo.CusEntryHeader
									ON C9_Clusterkey = CH_ClusterKey AND C9_CH = CH_PK
								WHERE XA_ParentID = C9_PK
									AND CH_DataModel = 'TW')
				OPTION (MAXDOP 1)
				SELECT @@ROWCOUNT;";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			if (Db.Connection.Exists("FROM dbo.GlbCompany WHERE GC_RN_NKCountryCode = 'TW'"))
			{
				while (ProcessDelete())
				{
					token.ThrowIfCancellationRequested();
				}
			}
		}

		bool ProcessDelete()
		{
			var rowsDeleted = Db.Connection.ExecuteScalar<int>(DeleteSql);
			return rowsDeleted >= batchSize;
		}
	}
}
