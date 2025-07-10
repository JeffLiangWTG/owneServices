using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ContainerYard;

class PopulateUnitLineItemForGatedInYardUnit : DataTransformation
{
	public override string UserDescription => "Add and populate CYDUnitLineItem for gated in yard unit";

	protected override void OfflinePostUpgradeTransform()
	{
		const string sql = """
Declare @YardUnitToLineItemMap TABLE (
    YUS_PK UNIQUEIDENTIFIER NOT NULL,
    YLI_PK UNIQUEIDENTIFIER NOT NULL
);

INSERT INTO @YardUnitToLineItemMap (
    YUS_PK,
    YLI_PK
)
SELECT
    Y.YUS_PK,
    NEWID()
FROM
    dbo.CYDYardUnitState Y
    LEFT JOIN dbo.CYDTransportationUnit T ON Y.YUS_YTU_ReceiveTransportationUnit = T.YTU_PK
WHERE
    Y.YUS_YLI_UnitLineItem IS NULL AND
    Y.YUS_YDL_Delivery IS NOT NULL AND
    T.YTU_GateInTime IS NOT NULL;

INSERT INTO dbo.[CYDUnitLineItem] (
    [YLI_PK],
    [YLI_Type],
    [YLI_ManufactureDate],
    [YLI_RC_ContainerType],
    [YLI_YMI_MachineryLineItem],
    [YLI_SealNumber],
    [YLI_Quantity],
    [YLI_IsEmpty],
    [YLI_IsDamaged],
    [YLI_TareWeight],
    [YLI_GrossWeight],
    [YLI_UnitOfWeight],
    [YLI_REG_Grade],
    [YLI_SystemCreateTimeUtc],
    [YLI_SystemCreateUser],
    [YLI_SystemLastEditTimeUtc],
    [YLI_SystemLastEditUser]
)
SELECT
    M.YLI_PK,
    L.YLI_Type,
    L.YLI_ManufactureDate,
    L.YLI_RC_ContainerType,
    L.YLI_YMI_MachineryLineItem,
    L.YLI_SealNumber,
    L.YLI_Quantity,
    L.YLI_IsEmpty,
    L.YLI_IsDamaged,
    L.YLI_TareWeight,
    L.YLI_GrossWeight,
    L.YLI_UnitOfWeight,
    L.YLI_REG_Grade,
    GETUTCDATE(),
    '~BP',
    GETUTCDATE(),
    '~BP'
FROM
    @YardUnitToLineItemMap M
    LEFT JOIN dbo.CYDYardUnitState Y ON M.YUS_PK = Y.YUS_PK
    LEFT JOIN dbo.CYDDelivery D ON Y.YUS_YDL_Delivery = D.YDL_PK
    LEFT JOIN dbo.CYDUnitLineItem L ON D.YDL_YLI_UnitLineItem = L.YLI_PK;

UPDATE dbo.[CYDYardUnitState]
SET
    YUS_YLI_UnitLineItem = M.YLI_PK,
	YUS_SystemLastEditTimeUtc = GETUTCDATE(),
	YUS_SystemLastEditUser = '~BP'
FROM
    @YardUnitToLineItemMap M
    LEFT JOIN dbo.CYDYardUnitState Y ON M.YUS_PK = Y.YUS_PK;
""";
		Db.Connection.ExecuteNonQuery(sql);
	}
}
