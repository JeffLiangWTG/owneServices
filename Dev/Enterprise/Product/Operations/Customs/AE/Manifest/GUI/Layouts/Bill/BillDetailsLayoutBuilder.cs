using Enterprise.Customs.AE.Manifest.Business;
using Enterprise.Customs.ASYCUDA.GUI;

namespace Enterprise.Customs.AE.Manifest.GUI;

public class BillDetailsLayoutBuilder : BillLayoutBuilder<AsycudaBill>
{
	protected override void SetDefaultVisibilities()
	{
		base.SetDefaultVisibilities();

		var aeBag = BillDetailsControlBag.Instance;
		SetVisibility(aeBag.SplitBillNumberCodeFindBox, x => x.ABL_SplitBill, x => x.ABL_SplitBillInfo);
	}
}
