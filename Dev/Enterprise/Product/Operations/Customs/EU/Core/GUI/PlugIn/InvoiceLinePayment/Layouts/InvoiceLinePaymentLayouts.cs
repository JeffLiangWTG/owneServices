using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI.Plugin;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class InvoiceLinePaymentLayouts : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new InvoiceLinePaymentLayoutBuilder<JobComInvoiceLine>();
			var euBag = InvoiceLinePaymentControlBag.Instance;
			builder.AddControlBag(euBag);

			builder.AddColumn();
			builder.Add(euBag.CommercialPaymentCodeDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.PaymentAmountCalcEdit, ControlWidthClass.Long);
			builder.Add(euBag.PaymentNoTextBox, ControlWidthClass.Long);
			builder.Add(euBag.PaymentDateEdit, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
