using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public sealed class ImportInvoiceLineDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();
			var commonBag = builder.CommonBag;
			var euBag = EU.GUI.InvoiceLineDetailsControlBag.Instance;
			var ieBag = ImportInvoiceLineDetailsControlBag.Instance;
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
			builder.Add(ieBag.CountryOfOriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(ieBag.CountryOfSupplyCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.RegionOfDestinationDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.WithDescriptionPrimaryPreferenceDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.QuotaDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.ValuationCodeDropEdit, ControlWidthClass.Auto);
			builder.SetVisibility(euBag.RegionOfDestinationDropEdit, i => i.Declaration?.IsUCC6AndIsImport ?? false);

			builder.AddColumn();
			builder.Add(commonBag.InvoiceQuantityCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Medium);
			builder.Add(commonBag.TaxTypeDropEdit, ControlWidthClass.Medium);
			builder.Add(euBag.SupplementaryCode1DropEdit, ControlWidthClass.Medium);
			builder.Add(euBag.SupplementaryCode2DropEdit, ControlWidthClass.Medium);
			builder.Add(euBag.AdditionalSupplementaryCodesUserControl, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CommodityCodeFindBox, ControlWidthClass.Medium);
			builder.Add(euBag.DestinationCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.DispatchCodeFindBox, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
