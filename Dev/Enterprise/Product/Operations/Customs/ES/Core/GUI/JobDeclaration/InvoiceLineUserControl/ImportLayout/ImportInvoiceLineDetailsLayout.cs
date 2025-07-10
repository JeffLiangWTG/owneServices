using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public sealed class ImportInvoiceLineDetailsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout => CreateLayout();

	static PanelLayout CreateLayout()
	{
		var builder = new ExportInvoiceLineDetailsLayoutBuilder();
		var commonBag = builder.CommonBag;
		var euBag = EU.GUI.InvoiceLineDetailsControlBag.Instance;
		builder.AddControlBag(euBag);
		var esBag = InvoiceLineDetailsControlBag.Instance;
		builder.AddControlBag(esBag);

		builder.AddColumn();
		builder.Add(commonBag.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.CommodityCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.CountryOfOriginDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.FormattedWithDescriptionTariffFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.DescriptionLongTextControl, ControlWidthClass.Long);
		builder.Add(euBag.SupplementaryCode1DropEdit, ControlWidthClass.Medium);
		builder.Add(euBag.SupplementaryCode2DropEdit, ControlWidthClass.Medium);
		builder.Add(euBag.AdditionalSupplementaryCodesAndGDMUserControl, ControlWidthClass.Long);
		builder.Add(euBag.PreferenceCodeDropEdit, ControlWidthClass.Long);
		builder.Add(euBag.FormattedProcedureCodeFindBox, ControlWidthClass.Long);
		builder.Add(euBag.AdditionalProcedureCodesUserControl, ControlWidthClass.Long);
		builder.Add(euBag.QuotaWithCheckLinkUserControl, ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(esBag.T2LItemNumberCalcEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Medium);
		builder.Add(commonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.CustomsFourthQuantityCalcDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.InvoiceQuantityCalcDropEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Medium);
		builder.Add(esBag.CommercialReferenceTextBox, ControlWidthClass.Long);
		builder.Add(euBag.CountryOfSupplyCodeFindBox, ControlWidthClass.Long);
		builder.Add(euBag.DispatchCodeFindBox, ControlWidthClass.Long);
		builder.Add(esBag.CountryOfDestinationCodeFindBox, ControlWidthClass.Auto);
		builder.Add(esBag.RegionOfDestinationCodeFindBox, ControlWidthClass.Long);
		builder.Add(euBag.RegionOfDestinationDropEdit, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(esBag.MethodOfPaymentDropEdit, ControlWidthClass.Auto);
		builder.Add(esBag.MethodOfPayment2DropEdit, ControlWidthClass.Auto);
		builder.Add(esBag.VATIGICTypeDropEdit, ControlWidthClass.Auto);
		builder.Add(esBag.AIEMTypeDropEdit, ControlWidthClass.Auto);
		builder.Add(esBag.ExciseExemptionDropEdit, ControlWidthClass.Auto);
		builder.Add(esBag.ExciseCodeDropEdit, ControlWidthClass.Auto);
		builder.Add(esBag.GlobalWarmingPotentialCalcEdit, ControlWidthClass.Auto);
		builder.Add(esBag.PVPCalcFindBox, ControlWidthClass.Auto);
		builder.Add(esBag.REAProductCodeDropEdit, ControlWidthClass.Auto);
		builder.Add(esBag.READirectConsumptionCheckBox, ControlWidthClass.Auto);
		builder.Add(euBag.CusNumberCodeFindBox, ControlWidthClass.Auto);
		builder.Add(esBag.HasNonRecycledPlasticsCheckBox, ControlWidthClass.Auto);
		builder.Add(commonBag.ValuationCodeDropEdit, ControlWidthClass.Long);

		builder.SetVisibility(esBag.REAProductCodeDropEdit, i => i.Declaration?.DestinationStateIsCanaryIsland ?? false, i => i.Declaration?.ZG_DestinationStateInfo);
		builder.SetVisibility(esBag.READirectConsumptionCheckBox, i => i.Declaration?.DestinationStateIsCanaryIsland ?? false, i => i.Declaration?.ZG_DestinationStateInfo);
		builder.SetVisibility(esBag.AIEMTypeDropEdit, i => i.Declaration?.DestinationStateIsCanaryIsland ?? false, i => i.Declaration?.ZG_DestinationStateInfo);
		builder.SetVisibility(esBag.MethodOfPayment2DropEdit, i => i.Declaration?.DestinationStateIsCanaryIsland ?? false, i => i.Declaration?.ZG_DestinationStateInfo);
		builder.SetVisibility(esBag.T2LItemNumberCalcEdit, i => i.EntryInstruction?.IsT2C ?? false, i => i.JI_CEIInfo, i => i.EntryInstruction?.CEI_SubStyleInfo);
		builder.SetVisibility(esBag.PVPCalcFindBox, i => i.IsPVPApplicable, i => i.ZG_ExciseCodeInfo);
		builder.SetVisibility(esBag.GlobalWarmingPotentialCalcEdit, i => i.IsPCAApplicable, i => i.ZG_ExciseCodeInfo);
		builder.SetVisibility(esBag.RegionOfDestinationCodeFindBox, i => (i.Declaration?.IsUCC6AndIsImport ?? false) && !i.IsCountryOfDestinationESOrXCOrXLOrEmpty, i => i.Declaration?.JE_MessageTypeInfo, i => i.ZG_CountryOfDestinationInfo);
		builder.SetVisibility(euBag.DispatchCodeFindBox, i => i.Declaration?.IsUCC6 ?? false);
		builder.SetVisibility(euBag.RegionOfDestinationDropEdit, i => (i.Declaration?.IsUCC6AndIsImport ?? false) && i.IsCountryOfDestinationESOrXCOrXLOrEmpty, i => i.Declaration?.JE_MessageTypeInfo, i => i.ZG_CountryOfDestinationInfo);
		builder.SetVisibility(commonBag.ValuationCodeDropEdit, i => i.Declaration?.IsUCC6 ?? false);

		builder.SetCaption(esBag.VATIGICTypeDropEdit, i => i.VATIGICTypeCaption, i => i.Declaration?.ZG_DestinationStateInfo);

		return builder.Build();
	}
}
