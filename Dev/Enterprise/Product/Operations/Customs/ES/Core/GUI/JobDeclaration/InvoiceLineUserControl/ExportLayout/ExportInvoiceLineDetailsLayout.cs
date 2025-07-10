using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public sealed class ExportInvoiceLineDetailsLayout : IPanelLayoutProvider
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
			builder.Add(commonBag.OriginStateDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.FormattedWithDescriptionTariffFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.DescriptionLongTextControl, ControlWidthClass.Long);
			builder.Add(euBag.SupplementaryCode1DropEdit, ControlWidthClass.Medium);
			builder.Add(euBag.SupplementaryCode2DropEdit, ControlWidthClass.Medium);
			builder.Add(euBag.AdditionalSupplementaryCodesAndGDMUserControl, ControlWidthClass.Long);
			builder.Add(euBag.FormattedProcedureCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.AdditionalProcedureCodesUserControl, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(euBag.CusNumberCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.InvoiceQuantityCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(esBag.CommercialReferenceTextBox, ControlWidthClass.Long);

			builder.AddControlBehaviour(commonBag.FormattedWithDescriptionTariffFindBox, new TariffBoxNomenclatureSelectionModeBehaviour(), i => i.EntryInstruction?.CEI_SubStyleInfo);

			return builder.Build();
		}
	}
}
