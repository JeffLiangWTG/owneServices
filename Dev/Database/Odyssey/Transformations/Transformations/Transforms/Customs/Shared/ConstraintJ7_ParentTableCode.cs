using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared
{
	class ConstraintJ7_ParentTableCode : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Cleanup data for new constraint on column J7_ParentTableCode";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = FormattableString.Invariant($@"
DELETE FROM dbo.JobComInvHeaderCharge
WHERE J7_ParentTableCode NOT IN (
	'JD', -- JobOrderHeader
	'JI', -- JobComInvoiceLine
	'JO', -- JobOrderLine
	'JZ' -- JobComInvoiceHeader
)");

			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(JobComInvHeaderChargeSchema.Instance)
					.Key(JobComInvHeaderChargeSchema.Constants.J7_ParentID)
					.Include(JobComInvHeaderChargeSchema.Constants.J7_SystemCreateTimeUtc)
					.Include(JobComInvHeaderChargeSchema.Constants.J7_SystemLastEditTimeUtc)
					.Where(@"[J7_ParentTableCode]<>'JD' AND [J7_ParentTableCode]<>'JI' AND [J7_ParentTableCode]<>'JO' AND [J7_ParentTableCode]<>'JZ'")
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
