
namespace Enterprise.DbUpgrader.Transformation.Common
{
	public static class WhsTransformationHelper
	{
		// Constants can be used both in DataModification and PreUpgrade

		#region FixDocketLineLocationsForIntraWarehouse

		public const string FixDocketLineLocationsForIntraWarehouse = @"
UPDATE 
	SourceRow
SET
	SourceRow.WE_WL = DestinationRow.WE_WL
From
	dbo.WhsDocketLine SourceRow
	Join dbo.WhsDocketLine DestinationRow ON SourceRow.WE_PK = DestinationRow.WE_WE_ParentDocketLine
	Join dbo.WhsDocket ON WD_PK = SourceRow.WE_WD AND WD_DocketType = 'TFR' AND WD_DocketSubType IN ('IWS')	
WHERE
	DestinationRow.WE_WL <> SourceRow.WE_WL
";

		#endregion

		#region FixDocketLineLocationsForSameWarehouse

		public const string FixDocketLineLocationsForSameWarehouse = @"
UPDATE Main
SET
	Main.WE_WL = Child.WE_WL,
	Main.WE_WL_TransferFrom = Child.WE_WL_TransferFrom
from
       dbo.WhsDocketLine as Main
       join dbo.WhsDocket MainDocket on MainDocket.WD_PK = Main.WE_WD
       join dbo.WhsLocation as L1 on L1.WL_PK = Main.WE_WL
       join dbo.WhsRow as R1 on R1.WR_PK = L1.WL_WR
       join dbo.WhsLocation as L2 on L2.WL_PK = Main.WE_WL_TransferFrom
       join dbo.WhsRow as R2 on R2.WR_PK = L2.WL_WR
       join dbo.WhsDocketLine as Child on Child.WE_WE_ParentDocketLine = Main.WE_PK
	   join dbo.WhsDocket ChildDocket on ChildDocket.WD_PK = Child.WE_WD
where
       (MainDocket.WD_DocketSubType = 'IWD' or MainDocket.WD_DocketSubType = 'IWS') 
	   and R1.WR_WW_Whs = R2.WR_WW_Whs 
	   and (Main.WE_WL != Child.WE_WL or Main.WE_WL_TransferFrom != Child.WE_WL_TransferFrom)
";

		#endregion
	}
}
