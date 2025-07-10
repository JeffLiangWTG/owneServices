using System.Threading;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.MasterData
{
	public class RemovePatternMatchingDataForDummyContacts : DataTransformation
	{
		public override string UserDescription => "Delete all the pattern matching name records of the 'DUMMY CONTACT TO SUPPRESS DOCS'";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var sql = @$"
DELETE FROM [dbo].[PatternMatchingName]
WHERE [PMN_HashedValue] = '1566845912'
AND [PMN_ParentTableCode] IN ('OC', 'PER')";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
