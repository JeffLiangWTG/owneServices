using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.GUI
{
	public sealed class ExportInvoiceLineDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new InvoiceLineDetailsLayoutBuilder();
			var commonBag = builder.CommonBag;
			var mxBag = InvoiceLineDetailsControlBag.Instance;
			builder.AddControlBag(mxBag);

			builder.AddColumn();
			builder.Add(mxBag.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfExportCodeFindBox, ControlWidthClass.Long);
			builder.Add(mxBag.ObservationsTextBox, ControlWidthClass.Long);
			builder.Add(mxBag.VehicleDetailsUserControl, ControlWidthClass.LongNoCaption);

			builder.AddColumn();
			builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Medium);

			return builder.Build();
		}
	}
}
