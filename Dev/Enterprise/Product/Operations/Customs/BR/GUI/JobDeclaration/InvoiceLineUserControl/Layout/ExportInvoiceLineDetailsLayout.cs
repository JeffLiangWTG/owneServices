using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public sealed class ExportInvoiceLineDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new InvoiceLineDetailsLayoutBuilder();
			var commonBag = builder.CommonBag;
			var brBag = InvoiceLineDetailsControlBag.Instance;
			builder.AddControlBag(brBag);

			builder.AddColumn();
			builder.Add(brBag.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.WithDescriptionTariffFindBox, ControlWidthClass.Long);
			builder.Add(brBag.FullGoodsDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(brBag.ComplementaryDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(brBag.BRNFENumberTextBox, ControlWidthClass.Long);
			builder.Add(brBag.BRNFEItemNumberTextBox, ControlWidthClass.Medium);
			builder.Add(brBag.CargoPriorityDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfOriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(brBag.CountryDestinationCodeFindBox, ControlWidthClass.Long);
			builder.Add(brBag.CPCGroupBox, ControlWidthClass.LongNoCaption);

			builder.AddColumn();
			builder.Add(commonBag.InvoiceQuantityCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
			builder.Add(brBag.NFeLinePriceCalcEdit, ControlWidthClass.Medium);

			builder.AddColumn();
			builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CommodityCodeFindBox, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
