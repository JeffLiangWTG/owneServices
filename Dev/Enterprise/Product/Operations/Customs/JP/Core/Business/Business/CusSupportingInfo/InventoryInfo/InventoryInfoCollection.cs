using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;

namespace Enterprise.Customs.JP.Business
{
	public class InventoryInfoCollection(CusEntryInstruction parent, ZGuid supportingInfoPk) : CusSupportingInfoCollection<InventoryInfo>(parent, CusSupportingInfoTypeList.Codes.Inventory, supportingInfoPk)
	{
		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (child is InventoryInfo inventoryInfo && inventoryInfo.ParentMoveInDestination is MoveInDestination inventoryInfoParent)
			{
				inventoryInfo.CSI_Quantity = Math.Max(inventoryInfoParent.CSI_Quantity - TotalQuantity, 0);
			}
		}

		internal ZDecimal TotalQuantity => this.Cast<InventoryInfo>().Sum(x => x.CSI_Quantity);
	}
}
