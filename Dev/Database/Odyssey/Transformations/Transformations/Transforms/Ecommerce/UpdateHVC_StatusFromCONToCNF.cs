using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Ecommerce
{
	class UpdateHVC_StatusFromCONToCNF : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update HVC_Status from CON to CNF.";

		protected override void OfflinePreUpgradeTransform()
		{
			var connection = Db.Connection;
			var sql = @"
ALTER TABLE dbo.HVLVConsignment DROP CONSTRAINT IF EXISTS Constraint_HVC_Status

UPDATE dbo.HVLVConsignment
SET
	HVC_Status = 'CNF',
	HVC_SystemLastEditTimeUtc = GETUTCDATE(),
	HVC_SystemLastEditUser = '~BP'
WHERE HVC_Status = 'CON'
";

			if (DbObjectCreator.TableExists(connection, HVLVConsignmentSchema.Constants.TableName)
				&& DbObjectCreator.ColumnExists(connection, HVLVConsignmentSchema.Constants.TableName, HVLVConsignmentSchema.Constants.HVC_Status))
			{
				connection.ExecuteNonQuery(sql);
			}
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);

				indexProvider.New(HVLVConsignmentSchema.Instance)
					.Key(HVLVConsignmentSchema.Constants.HVC_Status)
					.Where($"{HVLVConsignmentSchema.Constants.HVC_Status.QuoteName()}='CON'")
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
