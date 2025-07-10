using Enterprise.Customs.GUI;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	sealed class InvoiceLineDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout Layout { get; } = CreateLayout();

		static PanelLayout CreateLayout()
		{
			var builder = new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();
			var commonBag = builder.CommonBag;
			var ilBag = InvoiceLineDetailsControlBag.Instance;
			builder.AddControlBag(ilBag);

			builder.AddColumn();
			builder.Add(ilBag.InvoiceNumberDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.PartNoCodeFindBox, ControlWidthClass.Medium);
			builder.Add(commonBag.TariffFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.InvoiceQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.CountryOfOriginDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.PrimaryPreferenceDropEdit, ControlWidthClass.Long);
			builder.Add(ilBag.PreferenceDocNumberTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.BondedWhsQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.TaxTypeDropEdit, ControlWidthClass.Auto);

			builder.SetVisibility(commonBag.BondedWhsQuantityCalcDropEdit, i => i.IsImport, i => i.Declaration?.JE_MessageTypeInfo);
			return builder.Build();
		}
	}
}
