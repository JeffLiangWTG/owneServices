using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Manifest.GUI
{
	public sealed class JPTemporaryLandingLayouts : IPanelLayoutProvider
	{
		PanelLayout TemporaryLanding { get; }

		PanelLayout IPanelLayoutProvider.Layout => TemporaryLanding;

		public JPTemporaryLandingLayouts()
		{
			TemporaryLanding = CreateBillLayout();
		}

		PanelLayout CreateBillLayout()
		{
			var builder = new JPTemporaryLandingLayoutBuilder<AsycudaBill>();
			var common = builder.CommonBag;
			builder.AddColumn();
			builder.Add(common.TemporaryLandingReasonDropEdit, ControlWidthClass.Auto);
			builder.Add(common.TemporaryLandingPeriodDaysCalcEdit, ControlWidthClass.Auto);
			builder.Add(common.TemporaryLandingStartDateEdit, ControlWidthClass.Auto);
			builder.Add(common.TemporaryLandingEndDateEdit, ControlWidthClass.Auto);
			builder.Add(common.TemporaryLandingBondedTransportCodeDropEdit, ControlWidthClass.Auto);
			builder.Add(common.GoodsLocationCodeFindBox, ControlWidthClass.Auto);
			return builder.Build();
		}
	}
}
