using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse
{
	public class DeleteEmptyReferenceNumberAddOnValue : DataTransformation
	{
		public override string UserDescription => "Delete empty SourceType add on value for CusEntryNumber";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
			=> Db.Connection.ExecuteNonQuery(@"
			DELETE dbo.GenCustomAddOnValue WHERE XV_ParentTableCode='CE' AND XV_Data = '' AND XV_Name = 'SourceType';");
	}
}
