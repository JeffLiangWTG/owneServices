using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class UnloadingRemarksValidation : CusCodeDataValidation
{
	public UnloadingRemarksValidation(UnloadingRemarks parent)
		: base(parent)
	{
	}

	public new UnloadingRemarks Parent => (UnloadingRemarks)base.Parent;

	protected override void CheckCY_Code()
	{
	}

	readonly string[] requiredUnloadingRemarksStates = new[] { NctsUnloadedStateList.Codes.MIS, NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.DIF };

	protected override void CheckCY_Data()
	{
		base.CheckCY_Data();
		var cusSeal = Parent.Parent;
		if (cusSeal != null && cusSeal.UnloadingRemarksText.IsEmpty && requiredUnloadingRemarksStates.ToList().Contains(cusSeal.BK_UnloadingState))
		{
			Parent.CY_DataInfo.AddMessageError(Res.GetString("597B9077-F6A4-4E9D-A0EC-53F061B81107", "Unloading Remarks are required"));
		}
	}
}
