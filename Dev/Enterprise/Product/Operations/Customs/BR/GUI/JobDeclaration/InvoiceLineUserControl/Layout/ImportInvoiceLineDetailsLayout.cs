using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public sealed class ImportInvoiceLineDetailsLayout : IPanelLayoutProvider
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
			builder.Add(brBag.GoodsApplicationDropEdit, ControlWidthClass.Long);
			builder.Add(brBag.GoodsConditionDropEdit, ControlWidthClass.Long);
			builder.Add(brBag.ManufacturerIndicatorDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ManufacturerAddressControl, ControlWidthClass.Auto);
			builder.Add(commonBag.CountryOfOriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(brBag.NaladiNccaCodeFindBox, ControlWidthClass.Auto);
			builder.Add(brBag.NaladiHsCodeFindBox, ControlWidthClass.Auto);
			builder.Add(brBag.ImportLicenseNumberTextBox, ControlWidthClass.Medium);
			builder.Add(brBag.RequiresImportLicenseCheckBox, ControlWidthClass.Long);
			builder.Add(brBag.ImportLicenseFineSeparatorUserControl, ControlWidthClass.LongNoCaption);
			builder.Add(brBag.ImportLicenseTypeDropEdit, ControlWidthClass.Medium);
			builder.Add(brBag.ImportLicenseAuthorizationDateTextBox, ControlWidthClass.Medium);
			builder.Add(brBag.ImportLicenseFeeTypeDropEdit, ControlWidthClass.Medium);

			builder.AddColumn();
			builder.Add(commonBag.InvoiceQuantityCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Medium);

			builder.AddColumn();
			builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.CommodityCodeFindBox, ControlWidthClass.Auto);

			builder.SetVisibility(brBag.ImportLicenseNumberTextBox, l => l.IsImportSiscomex, l => l.Declaration?.JE_MessageTypeInfo);
			builder.SetVisibility(brBag.RequiresImportLicenseCheckBox, l => l.IsImportSiscomex || (l.IsImport && !l.IsAttachedToPersistentDeclaration), l => l.Declaration?.JE_MessageTypeInfo);
			builder.SetVisibility(brBag.ImportLicenseFineSeparatorUserControl, l => l.IsImportSiscomex, l => l.Declaration?.JE_MessageTypeInfo);
			builder.SetVisibility(brBag.ImportLicenseTypeDropEdit, l => l.IsImportSiscomex, l => l.Declaration?.JE_MessageTypeInfo);
			builder.SetVisibility(brBag.ImportLicenseAuthorizationDateTextBox, l => l.IsImportSiscomex, l => l.Declaration?.JE_MessageTypeInfo);
			builder.SetVisibility(brBag.ImportLicenseFeeTypeDropEdit, l => l.IsImportSiscomex, l => l.Declaration?.JE_MessageTypeInfo);
			builder.SetVisibility(brBag.NaladiNccaCodeFindBox, l => l.IsImportSiscomex, l => l.Declaration?.JE_MessageTypeInfo);
			builder.SetVisibility(brBag.NaladiHsCodeFindBox, l => l.IsImportSiscomex, l => l.Declaration?.JE_MessageTypeInfo);
			builder.SetVisibility(brBag.ComplementaryDescriptionTextBox, l => l.IsImportOnly, l => l.Declaration?.JE_MessageTypeInfo);

			return builder.Build();
		}
	}
}
