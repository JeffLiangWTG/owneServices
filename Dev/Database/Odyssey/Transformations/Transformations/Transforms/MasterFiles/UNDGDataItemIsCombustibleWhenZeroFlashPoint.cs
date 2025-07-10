using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.MasterFiles
{
	public class UNDGDataItemIsCombustibleFalseWhenZeroFlashPoint : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "In Old DG Items make IsCombustible false when flash point is 0.0 else true.";

		const int BatchSize = 1000;
		const string Query = @"
							UPDATE dbo.UNDGDataItem
							SET DI_IsCombustible = CASE 
									WHEN DI_DGFlashPoint = 0 AND DI_IsCombustible = 1 THEN 0
									WHEN DI_DGFlashPoint <> 0 AND DI_IsCombustible = 0 THEN 1
									ELSE DI_IsCombustible
									END,
								DI_SystemLastEditTimeUtc = DI_SystemLastEditTimeUtc, 
								DI_SystemLastEditUser = DI_SystemLastEditUser
							WHERE ((DI_DGFlashPoint = 0 AND DI_IsCombustible = 1)
								OR (DI_DGFlashPoint <> 0 AND DI_IsCombustible = 0))";

		protected override void OnlinePreUpgradeTransform()
		{
			base.OnlinePreUpgradeTransform();
			if (DbObjectCreator.TableExists(Db.Connection, UNDGDataItemSchema.Constants.TableName)
				&& DbObjectCreator.ColumnExists(Db.Connection, UNDGDataItemSchema.Constants.TableName, UNDGDataItemSchema.Constants.DI_IsCombustible)
				&& DbObjectCreator.ColumnExists(Db.Connection, UNDGDataItemSchema.Constants.TableName, UNDGDataItemSchema.Constants.DI_DGFlashPoint)
				&& DbObjectCreator.ColumnExists(Db.Connection, UNDGDataItemSchema.Constants.TableName, UNDGDataItemSchema.Constants.DI_SystemLastEditTimeUtc)
				&& DbObjectCreator.ColumnExists(Db.Connection, UNDGDataItemSchema.Constants.TableName, UNDGDataItemSchema.Constants.DI_SystemLastEditUser))
			{
				long totalCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, UNDGDataItemSchema.Constants.TableName);

				string sqlText = Query + "AND DI_PK >= @StartGuid AND DI_PK <= @EndGuid";
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

		#region ITransformationIndexProvider

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				if (DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, UNDGDataItemSchema.Constants.TableName)
					&& DbObjectCreator.ColumnExists(Db.Connection, UNDGDataItemSchema.Constants.TableName, UNDGDataItemSchema.Constants.DI_IsCombustible)
					&& DbObjectCreator.ColumnExists(Db.Connection, UNDGDataItemSchema.Constants.TableName, UNDGDataItemSchema.Constants.DI_DGFlashPoint))
				{
					indexProvider.New(UNDGDataItemSchema.Instance)
						.Key(UNDGDataItemSchema.Constants.DI_IsCombustible, UNDGDataItemSchema.Constants.DI_DGFlashPoint)
						.Include(UNDGDataItemSchema.Constants.DI_SystemLastEditTimeUtc, UNDGDataItemSchema.Constants.DI_SystemLastEditUser)
						.Where($"[{UNDGDataItemSchema.Constants.DI_IsCombustible}]=(1) AND [{UNDGDataItemSchema.Constants.DI_DGFlashPoint}]=(0)")
						.GetInfo();

					indexProvider.New(UNDGDataItemSchema.Instance)
						.Key(UNDGDataItemSchema.Constants.DI_IsCombustible, UNDGDataItemSchema.Constants.DI_DGFlashPoint)
						.Include(UNDGDataItemSchema.Constants.DI_SystemLastEditTimeUtc, UNDGDataItemSchema.Constants.DI_SystemLastEditUser)
						.Where($"[{UNDGDataItemSchema.Constants.DI_IsCombustible}]=(0) AND [{UNDGDataItemSchema.Constants.DI_DGFlashPoint}]<>(0)")
						.GetInfo();
				}
				return indexProvider;
			}
		}

		#endregion
	}
}
