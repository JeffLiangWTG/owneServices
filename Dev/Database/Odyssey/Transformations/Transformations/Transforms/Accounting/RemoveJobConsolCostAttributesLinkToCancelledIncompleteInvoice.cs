using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;
namespace Enterprise.DbUpgrader.Transformations.Transforms.Accounting
{
	public class RemoveJobConsolCostAttributesLinkToCancelledIncompleteInvoice : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Remove Job Consol Cost Attributes that link to a cancelled transaction header";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
DELETE JC6
FROM dbo.JobConsolCostAttrib AS JC6
INNER JOIN dbo.AccTransactionHeader AS ATH ON TRY_CONVERT(uniqueidentifier, JC6.E6A_Value) = ATH.AH_PK
WHERE JC6.E6A_Name = 'INV' AND ATH.AH_IsCancelled = '1' AND ATH.AH_Ledger = 'IN';
";

			Db.Connection.ExecuteNonQuery(sql);
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(JobConsolCostAttribSchema.Instance)
					.Key(JobConsolCostAttribSchema.Constants.E6A_Name)
					.Where($"[{JobConsolCostAttribSchema.Constants.E6A_Name}]='INV'")
					.Include(JobConsolCostAttribSchema.Constants.E6A_Value)
					.GetInfo();
				return indexProvider;
			}
		}
	}
}
