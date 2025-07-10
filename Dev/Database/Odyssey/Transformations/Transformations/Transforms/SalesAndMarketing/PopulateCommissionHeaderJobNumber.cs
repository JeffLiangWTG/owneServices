using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.SalesAndMarketing
{
	public class PopulateCommissionHeaderJobNumber : DataTransformation
	{
		public override string UserDescription => "Populate Job Number on Commission Header";

		protected override void OfflinePostUpgradeTransform()
		{
			Db.Connection.ExecuteNonQuery(@"UPDATE AccCommissionHeader
	SET 
	CH0_JobNumber = JH_JobNum,
	CH0_SystemLastEditTimeUtc = SYSUTCDATETIME(),
	CH0_SystemLastEditUser = '~BP'
	FROM AccCommissionHeader
	INNER JOIN JobHeader ON CH0_GroupingSourceId = JH_PK
	WHERE CH0_JobNumber = ''");
		}
	}
}
