using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class ArrivalNotificationDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, ArrivalNotificationDetailsControlBag> where T : Business.NctsHeader
	{
		public override ArrivalNotificationDetailsControlBag CommonBag => ArrivalNotificationDetailsControlBag.Instance;

		protected override int MaxColumns => 2;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();

			SetVisibility(CommonBag.OverrideFreightDetailsCheckBox, x => x.IsPluggedIn);
			AddControlBehaviour(CommonBag.NumberCodeFindBox, new ArrivalAuthorisationNumberCasingBehaviour());
		}
	}
}
