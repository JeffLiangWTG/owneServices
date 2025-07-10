using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Accounting
{
	public class RemoveJobChargeAttributesLinkToCancelledIncompleteInvoice : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Remove Job Charge Attributes that link to a cancelled transaction header";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
DELETE JCA
FROM dbo.JobChargeAttrib AS JCA
INNER JOIN dbo.AccTransactionHeader AS ATH ON TRY_CONVERT(uniqueidentifier, JCA.EC_Value) = ATH.AH_PK
WHERE JCA.EC_Name = 'INV' AND ATH.AH_IsCancelled = '1' AND ATH.AH_Ledger = 'IN';
";

			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(JobChargeAttribSchema.Instance)
					.Key(JobChargeAttribSchema.Constants.EC_Name)
					.Where($"[{JobChargeAttribSchema.Constants.EC_Name}]='INV'")
					.Include(JobChargeAttribSchema.Constants.EC_Value, JobChargeAttribSchema.Constants.EC_SystemCreateTimeUtc, JobChargeAttribSchema.Constants.EC_SystemLastEditTimeUtc)
					.GetInfo();
				return indexProvider;
			}
		}
	}
}
