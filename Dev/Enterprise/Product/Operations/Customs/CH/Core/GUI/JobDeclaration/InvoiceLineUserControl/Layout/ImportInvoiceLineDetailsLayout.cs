using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class ImportInvoiceLineDetailsLayout : IPanelLayoutProvider
{
	PanelLayout ImportInvoiceLineDetails { get; }

	public PanelLayout Layout => ImportInvoiceLineDetails;

	public ImportInvoiceLineDetailsLayout()
	{
		ImportInvoiceLineDetails = CreateLayout();
	}

	static PanelLayout CreateLayout()
	{
		var builder = new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();
		var commonBag = builder.CommonBag;
		builder.AddControlBag(commonBag);
		var chBag = InvoiceLineDetailsControlBag.Instance;
		builder.AddControlBag(chBag);

		builder.AddColumn();
		builder.Add(commonBag.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.ProcedureCodeFindBox, ControlWidthClass.Long);
		builder.Add(chBag.NonCommercialGoodsCheckBox, ControlWidthClass.Long);
		builder.Add(commonBag.FormattedWithDescriptionTariffFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.DescriptionLongTextControl, ControlWidthClass.Auto);
		builder.Add(commonBag.CountryOfOriginCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.WithDescriptionPrimaryPreferenceDropEdit, ControlWidthClass.Long);
		builder.Add(chBag.PermitObligationDropEdit, ControlWidthClass.Long);
		builder.Add(chBag.NonCustomsLawObligationDropEdit, ControlWidthClass.Long);
		builder.Add(chBag.StorageTypeDropEdit, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(commonBag.InvoiceQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
		builder.Add(chBag.VATCodeUserControl, ControlWidthClass.Long);
		builder.Add(chBag.RateFormulaDescriptionTextBox, ControlWidthClass.Auto);
		builder.Add(chBag.RateOverrideCheckBox, ControlWidthClass.Auto);
		builder.Add(chBag.OverriddenRateCalcEdit, ControlWidthClass.Auto);
		builder.Add(chBag.DutyRateUserControl, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.CommodityCodeFindBox, ControlWidthClass.Medium);
		builder.Add(chBag.NetDutyCheckBox, ControlWidthClass.Medium);
		builder.Add(chBag.CustomNetWeightCalcDropEdit, ControlWidthClass.Medium);
		builder.Add(chBag.TareSupplementUserControl, ControlWidthClass.Auto);
		builder.Add(chBag.CalculatedGrossMassCalcDropEdit, ControlWidthClass.Medium);
		builder.Add(chBag.ConfirmationCodesSeparatorUserControl, ControlWidthClass.Medium);
		builder.Add(chBag.GrossMassConfirmationCheckBox, ControlWidthClass.Medium);
		builder.Add(chBag.NetMassConfirmationCheckBox, ControlWidthClass.Medium);
		builder.Add(chBag.AdditionalUnitConfirmationCheckBox, ControlWidthClass.Auto);
		builder.Add(chBag.StatisticalValueConfirmationCheckBox, ControlWidthClass.Medium);
		builder.Add(chBag.VATValueConfirmationCheckBox, ControlWidthClass.Medium);

		builder.SetVisibility(chBag.CustomNetWeightCalcDropEdit, l => l.NetDuty, l => l.NetDutyInfo);
		builder.SetVisibility(chBag.TareSupplementUserControl, l => l.NetDuty, l => l.NetDutyInfo);
		builder.SetVisibility(chBag.CalculatedGrossMassCalcDropEdit, l => l.NetDuty, l => l.NetDutyInfo);
		builder.SetVisibility(chBag.DutyRateUserControl, l => !l.IsFixedRate, l => l.DutyRateAdditionalCodeInfo);
		builder.SetVisibility(chBag.OverriddenRateCalcEdit, l => l.JI_RateOverride, l => l.JI_RateOverrideInfo);

		return builder.Build();
	}
}
