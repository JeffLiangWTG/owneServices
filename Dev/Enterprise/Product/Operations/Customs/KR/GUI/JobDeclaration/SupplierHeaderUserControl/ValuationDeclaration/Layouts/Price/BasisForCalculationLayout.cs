using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class BasisForCalculationLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var common = PriceControlBag.InstanceForDeclaration;
			var layout = new PanelLayout();

			layout.RegisterControlBag(common);

			var ruler1 = layout.CreateRuler(160);
			var ruler2 = layout.CreateRuler(580);

			layout.Include(ruler1, common.PaymentAmountConvertToLocalCurrencyControl, ruler2, common.ExchangeRateCalcEdit);
			layout.Include(ruler1, common.IndirectAmountCalcEdit, ruler2, common.PaymentAmountCalcEdit);

			return layout;
		}
	}
}
