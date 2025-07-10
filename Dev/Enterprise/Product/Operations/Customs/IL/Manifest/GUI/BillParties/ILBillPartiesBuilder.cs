using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.IL.Manifest.Business;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public class ILBillPartiesBuilder : BillPartiesLayoutBuilder<AsycudaBill>
	{
		protected override void SetDefaultCaptions()
		{
			base.SetDefaultCaptions();
			SetCaption(CommonBag.ConsigneeRegoNoTextBox, h => h.ConsigneeRegoNoCaption, h => h.Header.AMA_NatureInfo);
		}
	}
}
