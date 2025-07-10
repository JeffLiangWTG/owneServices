using System;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;

sealed class EnsureCusTempStorageRegLineTransactionGrossWeightFitPrecision19Scale6 : DataTransformation, ITransformationIndexProvider
{
	public override string UserDescription => "Ensure CusTempStorageRegLineTransaction.SRT_GrossWeight fits type DECIMAL(19, 6)";

	protected override void OfflinePreUpgradeTransform()
	{
		EnsureQuantityFieldFits13IntegerDigits(CusTempStorageRegLineTransactionSchema.SRT_GrossWeight);
	}

	static void EnsureQuantityFieldFits13IntegerDigits(SchemaDecimalColumn quantityField)
	{
		if (DbObjectCreator.ColumnExists(Db.Connection, quantityField.TableName, quantityField.Name))
		{
			var sql = FormattableString.Invariant($@"
					UPDATE dbo.{quantityField.TableName}
						SET {quantityField.Name} = 9999999999999,
						SRT_SystemLastEditTimeUtc = GETUTCDATE(),
						SRT_SystemLastEditUser = '~BP'
						WHERE {quantityField.Name} > 9999999999999");

			Db.Connection.ExecuteNonQuery(sql);
		}
	}

	#region ITransformationIndexProvider

	TransformationIndexProvider ITransformationIndexProvider.IndexProvider
	{
		get
		{
			var indexProvider = new TransformationIndexProvider(this);
			CreateIndexForColumnIfExists(indexProvider, CusTempStorageRegLineTransactionSchema.SRT_GrossWeight);
			return indexProvider;
		}
	}

	static void CreateIndexForColumnIfExists(TransformationIndexProvider indexProvider, SchemaDecimalColumn quantityField)
	{
		if (DbObjectCreator.ColumnExists(Db.Connection, quantityField.TableName, quantityField.Name))
		{
			indexProvider.New(CusTempStorageRegLineTransactionSchema.Instance)
				.Key(quantityField.Name)
				.Where($"[{quantityField.Name}]>(9999999999999.)")
				.GetInfo();
		}
	}

	#endregion
}
