using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.Business;

public class InventoryInfoValidation(InventoryInfo parent) : CusSupportingInfoValidation(parent)
{
	new InventoryInfo Parent => (InventoryInfo)base.Parent;

	CusEntryInstruction EntryInstruction => Parent.Parent;

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();
		if (EntryInstruction.IsECR)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_CodeInfo);
		}
	}

	protected override void CheckCSI_Quantity()
	{
		base.CheckCSI_Quantity();
		var parent = Parent;
		var info = parent.CSI_QuantityInfo;
		if (parent.ParentCollection.TotalQuantity > parent.ParentMoveInDestination.CSI_Quantity)
		{
			info.AddMessageError(Res.GetString("79C31C16-1C55-439C-A3FB-C1C40D1B6F79", "The total quantity (Inventory) must not exceed the quantity (Move-In)."));
		}
		if (EntryInstruction.IsECR)
		{
			MandatoryValidation.MessageErrorIfNotEntered(info);
		}
	}
}
