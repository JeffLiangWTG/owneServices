using System;
using System.Data;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Transformation.DataModification.AddInfoTransformationBase;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing
{
	[TestsSubclassesOf(typeof(OnlineCopyAddInfoToOtherTableRealColumn<>))]
	abstract class OnlineCopyAddInfoToOtherTableRealColumnTest<T, TOnline> : OnlineCopyAddInfoToRealColumnTest<T, TOnline>
		where T : CopyAddInfoToOtherTableRealColumn
		where TOnline : OnlineCopyAddInfoToOtherTableRealColumn<T>
	{
		protected void AssertNotExists(Guid objPK)
		{
			AssertEquals(0, GetRecordCount(objPK, OfflineTransformation.TargetForeignKeyColumn));
		}

		protected void AssertDeletingSourceWillDeleteTarget(Guid pk)
		{
			AssertDeletingSourceWillDeleteTarget(pk, OfflineTransformation.SourceTablePK, OfflineTransformation.TargetForeignKeyColumn);
		}

		protected void AssertDeletingSourceWillDeleteTarget(Guid pk, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn)
		{
			DeleteData(pk, primaryKeyColumn);
			AssertTargetWasDeleted(pk, foreignKeyColumn);
		}

		protected void DeleteData(Guid pk, SchemaGuidColumn primaryKeyColumn)
		{
			string deleteSourceSql = $@"DELETE {primaryKeyColumn.TableName.QuoteName()} WHERE {primaryKeyColumn.Name.QuoteName()} = @pk";
			var cmd = Db.Connection.Command(deleteSourceSql);
			cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
			cmd.ExecuteNonQuery();
		}

		protected void AssertTargetWasDeleted(Guid pk, SchemaGuidColumn foreignKeyColumn)
		{
			AssertEquals(foreignKeyColumn.TableName + " should be deleted as parent got deleted", 0, GetRecordCount(pk, foreignKeyColumn));
		}

		protected string GetDeleteSourceTriggerName(TOnline onlineTransformation) => OnlineCopyAddInfoToOtherTableRealColumn<T>.GetInsteadOfDeleteTriggerName(onlineTransformation.OfflineTransformation.SourceTableName);
	}
}
