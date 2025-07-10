using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public sealed class ImportLicenseInvoiceLineDetailsLayout : IPanelLayoutProvider
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
			builder.Add(commonBag.WithDescriptionTariffFindBox, ControlWidthClass.Auto);
			builder.Add(brBag.FullGoodsDescriptionTextBox, ControlWidthClass.Long);
			builder.Add(brBag.NaladiHsCodeFindBox, ControlWidthClass.Auto);
			builder.Add(brBag.GoodsConditionSeparatorUserControl, ControlWidthClass.Long);
			builder.Add(brBag.UsedMaterialRegimeDropEdit, ControlWidthClass.Long);
			builder.Add(brBag.GoodsConditionOperationTypeDropEdit, ControlWidthClass.Long);
			builder.Add(brBag.BrandGoodsConditionTextBox, ControlWidthClass.Long);
			builder.Add(brBag.ModelGoodsConditionTextBox, ControlWidthClass.Long);
			builder.Add(brBag.SerialNumberGoodsConditionTextBox, ControlWidthClass.Long);
			builder.Add(brBag.YearGoodsConditionTextBox, ControlWidthClass.Medium);

			builder.AddColumn();
			builder.Add(commonBag.InvoiceQuantityCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Medium);
			builder.Add(brBag.DutyTaxRegimeSeparatorUserControl, ControlWidthClass.Medium);
			builder.Add(brBag.DutyTaxRegimeDropEdit, ControlWidthClass.Medium);
			builder.Add(brBag.DutyLegalBaseDropEdit, ControlWidthClass.Medium);
			builder.Add(brBag.TariffAgreementSeparatorUserControl, ControlWidthClass.Medium);
			builder.Add(brBag.TariffAgreementDropEdit, ControlWidthClass.Medium);

			builder.AddColumn();
			builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CommodityCodeFindBox, ControlWidthClass.Auto);

			builder.SetVisibility(brBag.GoodsConditionOperationTypeDropEdit, l => l.UsedMaterialRegimeIsNationalization, l => l.JI_UsedMaterialRegimeInfo);
			builder.SetVisibility(brBag.ModelGoodsConditionTextBox, l => l.UsedMaterialRegimeIsNationalization, l => l.JI_UsedMaterialRegimeInfo);
			builder.SetVisibility(brBag.YearGoodsConditionTextBox, l => l.UsedMaterialRegimeIsNationalization, l => l.JI_UsedMaterialRegimeInfo);
			builder.SetVisibility(brBag.SerialNumberGoodsConditionTextBox, l => l.UsedMaterialRegimeIsNationalization, l => l.JI_UsedMaterialRegimeInfo);
			builder.SetVisibility(brBag.BrandGoodsConditionTextBox, l => l.UsedMaterialRegimeIsNationalization, l => l.JI_UsedMaterialRegimeInfo);

			return builder.Build();
		}
	}
}
