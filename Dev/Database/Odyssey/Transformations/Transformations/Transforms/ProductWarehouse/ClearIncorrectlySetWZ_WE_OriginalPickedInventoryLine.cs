using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	class ClearIncorrectlySetWZ_WE_OriginalPickedInventoryLine : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Clear Incorrectly Set WZ_WE_OriginalPickedInventoryLine";

		protected override void OfflinePostUpgradeTransform()
		{
			base.OfflinePostUpgradeTransform();

			Db.Connection.ExecuteNonQuery(@"
IF EXISTS(SELECT NULL FROM sys.triggers WHERE name = 'TG_WhsPickLine_PreventChangingOriginalPickedInventory')
BEGIN
	DISABLE TRIGGER TG_WhsPickLine_PreventChangingOriginalPickedInventory ON dbo.WhsPickLine
END

EXEC dbo.SuspendTrigger 'TG_WhsPickLine_TransactionAndPickedQtyIsCorrect'
EXEC dbo.SuspendTrigger 'TG_WhsPickLine_StockOnHandIsBalanced'

UPDATE dbo.WhsPickLine
SET
	WZ_WE_OriginalPickedInventoryLine = NULL,
	WZ_SystemLastEditTimeUtc = GetUtcDate(),
	WZ_SystemLastEditUser = '~BP'
WHERE
	WZ_WE_OriginalOrderLine IS NOT NULL AND WZ_WE_OriginalPickedInventoryLine IS NOT NULL

EXEC dbo.ResumeTrigger 'TG_WhsPickLine_TransactionAndPickedQtyIsCorrect'
EXEC dbo.ResumeTrigger 'TG_WhsPickLine_StockOnHandIsBalanced'

IF EXISTS(SELECT NULL FROM sys.triggers WHERE name = 'TG_WhsPickLine_PreventChangingOriginalPickedInventory')
BEGIN
	ENABLE TRIGGER TG_WhsPickLine_PreventChangingOriginalPickedInventory ON dbo.WhsPickLine
END
");
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(WhsPickLineSchema.Instance)
					.Key(WhsPickLineSchema.Constants.WZ_WE_OriginalOrderLine, WhsPickLineSchema.Constants.WZ_WE_OriginalPickedInventoryLine)
					.Where("[WZ_WE_OriginalOrderLine] IS NOT NULL AND [WZ_WE_OriginalPickedInventoryLine] IS NOT NULL")
					.GetInfo();
				return indexProvider;
			}
		}
	}
}
