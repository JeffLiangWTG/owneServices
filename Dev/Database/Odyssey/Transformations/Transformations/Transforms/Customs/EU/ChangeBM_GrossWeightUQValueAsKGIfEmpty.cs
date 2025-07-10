using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;

public class ChangeBM_GrossWeightUQValueAsKGIfEmpty : DataTransformation, ITransformationIndexProvider
{
	public override string UserDescription => "Update BM_GrossWeightUQ as default value KG when BM_GrossWeight is not null.";

	const int BatchSize = 1000;
	const string Query = @"
							UPDATE dbo.CusInBondMoveHeader
							SET BM_GrossWeightUQ = 'KG',
								BM_SystemLastEditTimeUtc = GetUtcDate(),
								BM_SystemLastEditUser = 'E'
							WHERE BM_GrossWeightUQ = ''
							AND BM_GrossWeight <> 0";

	protected override void OnlinePreUpgradeTransform()
	{
		if (DbObjectCreator.TableExists(Db.Connection, CusInBondMoveHeaderSchema.Constants.TableName)
				&& DbObjectCreator.ColumnExists(Db.Connection, CusInBondMoveHeaderSchema.Constants.TableName, CusInBondMoveHeaderSchema.Constants.BM_GrossWeightUQ)
				&& DbObjectCreator.ColumnExists(Db.Connection, CusInBondMoveHeaderSchema.Constants.TableName, CusInBondMoveHeaderSchema.Constants.BM_GrossWeight))
		{
			long totalCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, CusInBondMoveHeaderSchema.Constants.TableName);

			string sqlText = Query + "AND BM_PK >= @StartGuid AND BM_PK <= @EndGuid";
			var guidChunks = GuidChunker.GenerateChunks(BatchSize, totalCount, null);

			foreach (var chunk in guidChunks)
			{
				var startGuid = chunk.LowerBound;
				var endGuid = chunk.UpperBound;

				Db.Connection.ExecuteNonQuery(sqlText, cmd =>
				{
					cmd.AddParameter("StartGuid", SqlDbType.UniqueIdentifier, startGuid);
					cmd.AddParameter("EndGuid", SqlDbType.UniqueIdentifier, endGuid);
				});
			}
		}
	}

	protected override void OfflinePostUpgradeTransform()
	{
		Db.Connection.ExecuteNonQuery(Query);
	}

	public TransformationIndexProvider IndexProvider
	{
		get
		{
			var indexProvider = new TransformationIndexProvider(this);

			indexProvider.New(CusInBondMoveHeaderSchema.Instance)
				.Key(CusInBondMoveHeaderSchema.Constants.BM_GrossWeightUQ, CusInBondMoveHeaderSchema.Constants.BM_GrossWeight)
				.Include(CusInBondMoveHeaderSchema.Constants.BM_SystemLastEditTimeUtc, CusInBondMoveHeaderSchema.Constants.BM_SystemLastEditUser)
				.Where("[BM_GrossWeightUQ]='' AND [BM_GrossWeight]<>(0)")
				.GetInfo();
			return indexProvider;
		}
	}
}
