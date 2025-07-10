using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core
{
	public class UpdateEQ_DateReceivedColumnType : DataTransformation
	{
		public override string UserDescription => "Empty Transformation UpdateEQ_DateReceivedColumnType";

		protected override void OnlinePreUpgradeTransform()
		{
			ExtProperty.Database.Delete(Db.Connection, "ConvertDateTimeToDateTimeOffsetTransform.EQ_DateReceived.FromTime");
			ExtProperty.Database.Delete(Db.Connection, "ConvertDateTimeToDateTimeOffsetTransform.EQ_DateReceived.ToTime");
			ExtProperty.Database.Delete(Db.Connection, "ConvertDateTimeToDateTimeOffsetTransform.EQ_DateReceived.Status");
			DbObjectCreator.DropTriggerIfExists(Db.Connection, "TG_JobRequiredDocument_KeepEQ_DateReceivedInSync");

			if (DbObjectCreator.TableExists(Db.Connection, JobRequiredDocumentSchema.Constants.TableName))
			{
				var tempColumnName = Invariant($"{ColumnSynchroniser.ConvertDateTimeToDateTimeOffsetColumnPrefix}{JobRequiredDocumentSchema.Constants.EQ_DateReceived}");
				new DbColumnDependencyRemover(JobRequiredDocumentSchema.Constants.SqlSchemaName, JobRequiredDocumentSchema.Constants.TableName, tempColumnName).DropRelatedIndexes(Db.Connection);
				Db.Connection.ExecuteNonQuery(Invariant($"ALTER TABLE dbo.JobRequiredDocument DROP COLUMN IF EXISTS {tempColumnName}"));
			}

			base.OnlinePreUpgradeTransform();
		}
	}
}
