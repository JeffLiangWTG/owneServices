using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Business;

public class MoveInDestinationCollection(CusEntryInstruction entryInstruction) : CusSupportingInfoCollection<MoveInDestination>(entryInstruction, CusSupportingInfoTypeList.Codes.ExpectedMoveInDestination)
{
	public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(MoveInDestination);

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);
		if (Master is CusEntryInstruction entryInstruction && child is MoveInDestination moveInDestination && entryInstruction.JobDeclaration is JobDeclaration declration)
		{
			var address = declration.WarehouseDocAddress.Address ?? declration.ContainerYardDocAddress.Address;
			moveInDestination.CSI_Code = address?.CustomsCodes.GetCustomsRegNoMatching(OrgCusCode.CodeTypes.ControlledPremisesID) ?? ZString.Empty;

			if (Count == 0)
			{
				moveInDestination = child as MoveInDestination;
				moveInDestination.CSI_Quantity = entryInstruction.CEI_CargoQuantity;
				moveInDestination.CSI_Quantity2 = entryInstruction.CEI_CustomsWeight;
				moveInDestination.CSI_Quantity3 = entryInstruction.CEI_CustomsVolume;
			}
		}
	}

	internal ZDecimal TotalCargoQuantity => SumQuantity(x => x.CSI_Quantity);

	internal ZDecimal TotalCustomsWeight => SumQuantity(x => x.CSI_Quantity2);

	internal ZDecimal TotalCustomsVolume => SumQuantity(x => x.CSI_Quantity3);

	ZDecimal SumQuantity(Func<MoveInDestination, decimal> selector) => this.Sum(selector);
}
