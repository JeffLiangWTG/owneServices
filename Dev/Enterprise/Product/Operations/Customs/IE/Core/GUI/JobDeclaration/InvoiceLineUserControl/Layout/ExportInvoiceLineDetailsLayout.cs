using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public sealed class ExportInvoiceLineDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();
			var commonBag = builder.CommonBag;
			var euBag = EU.GUI.InvoiceLineDetailsControlBag.Instance;
			var ieBag = ExportInvoiceLineDetailsControlBag.Instance;
			builder.AddControlBag(euBag);
			builder.AddControlBag(ieBag);

			builder.AddColumn();
			builder.Add(commonBag.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.PartNoCodeFindBox, ControlWidthClass.Auto);
			builder.Add(euBag.FormattedProcedureCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.AdditionalProcedureCodesUserControl, ControlWidthClass.Long);
			builder.Add(euBag.CusNumberCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.WithDescriptionTariffFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.DescriptionLongTextControl, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfOriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfExportCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.DestinationCodeFindBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.InvoiceQuantityCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Medium);
			builder.Add(euBag.SupplementaryCode1DropEdit, ControlWidthClass.Medium);
			builder.Add(euBag.SupplementaryCode2DropEdit, ControlWidthClass.Medium);
			builder.Add(euBag.AdditionalSupplementaryCodesUserControl, ControlWidthClass.Long);
			builder.Add(euBag.NationalAdditionalCode1DropEdit, ControlWidthClass.Medium);
			builder.Add(euBag.NationalAdditionalCode2DropEdit, ControlWidthClass.Medium);
			builder.Add(euBag.NationalAdditionalCodesUserControl, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(ieBag.IsMainPackCheckBox, ControlWidthClass.Medium);
			builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CommodityCodeFindBox, ControlWidthClass.Medium);

			return builder.Build();
		}
	}
}
