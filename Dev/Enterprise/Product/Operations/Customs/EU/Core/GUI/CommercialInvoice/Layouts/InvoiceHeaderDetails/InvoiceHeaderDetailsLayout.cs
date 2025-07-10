using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.CommercialInvoice
{
	public sealed class InvoiceHeaderDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();
		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new Customs.GUI.CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
			var baseBag = Customs.GUI.CommercialInvoice.InvoiceHeaderDetailsControlBag.Instance;
			var euBag = EUInvoiceHeaderDetailsControlBag.Instance;

			builder.AddControlBag(baseBag);
			builder.AddControlBag(euBag);

			builder.AddColumn();
			builder.Add(baseBag.InvoiceNumberBoundTextBox, ControlWidthClass.Auto);
			builder.Add(baseBag.InvoiceDateEdit, ControlWidthClass.Auto);
			builder.Add(baseBag.InvoiceAmountCalcFindBox, ControlWidthClass.Auto);
			builder.Add(baseBag.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
			builder.Add(baseBag.IncotermAndIncotermPlaceUserControl, ControlWidthClass.Auto);
			builder.Add(euBag.AgreedPlaceCodeFindBox, ControlWidthClass.Auto);
			builder.Add(baseBag.ValuationCodeDropEdit, ControlWidthClass.Auto);
			builder.Add(baseBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(baseBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(baseBag.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto);
			builder.Add(baseBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
