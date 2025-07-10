using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common.HelperClasses;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse
{
	public class ClearCIN750Note : DataTransformation
	{
		public override string UserDescription => "Remove CIN 750 Notes except for RCN and DCN";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var lastProcessedChunkPKName = "ClearCIN750Note.LastProcessedChunkPKName";
			var approximateRowCount = Math.Max(1, DataUtils.GetApproximateRowCountForTable(Db.Connection, StmNoteSchema.Constants.TableName));

			var chunkingHelper = new GuidChunkingOperation(manager, 10000, approximateRowCount, ProcessChunk, lastProcessedChunkPKName, token);
			chunkingHelper.DoChunking();
		}

		void ProcessChunk(Guid lowerBound, Guid upperBound)
		{
			var sqlText = @$"DELETE dbo.StmNote
WHERE ST_ParentID BETWEEN @lowerBound AND @upperBound
AND ST_Description = 'CIN 750 Message Notes'
AND ST_Table <> 'WhsItemReceiveConsignment'
AND ST_Table <> 'WhsItemDispatchConsignment'
";

			Db.Connection.ExecuteNonQuery(sqlText, cmd =>
			{
				cmd.AddParameter("@lowerBound", SqlDbType.UniqueIdentifier, lowerBound);
				cmd.AddParameter("@upperBound", SqlDbType.UniqueIdentifier, upperBound);
			});
		}
	}
}
