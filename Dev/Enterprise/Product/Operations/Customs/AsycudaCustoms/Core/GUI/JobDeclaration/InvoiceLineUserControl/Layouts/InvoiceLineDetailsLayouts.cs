using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public sealed class InvoiceLineDetailsLayouts : IPanelLayoutProvider
	{
		PanelLayout InvoiceLineDetails { get; }

		PanelLayout IPanelLayoutProvider.Layout => InvoiceLineDetails;

		public InvoiceLineDetailsLayouts()
		{
			InvoiceLineDetails = CreateInvoiceLineDetailsLayouts();
		}

		PanelLayout CreateInvoiceLineDetailsLayouts()
		{
			var builder = new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();
			var commonBag = builder.CommonBag;
			var specificBag = InvoiceLineDetailsControlBag.Instance;
			builder.AddControlBag(specificBag);

			builder.AddColumn();
			builder.Add(commonBag.EntryInstructionGuidDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.PartNoCodeFindBox, ControlWidthClass.Medium);
			builder.Add(commonBag.ProcedureCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.TariffFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.DescriptionLongTextControl, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfOriginCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.PrimaryPreferenceDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CommodityCodeFindBox, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(commonBag.InvoiceQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.BondedWhsQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.TaxTypeDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(specificBag.PreviousEntryNumberTextBox, ControlWidthClass.Auto);
			builder.Add(specificBag.PreviousEntryLineNumberCalcEdit, ControlWidthClass.Auto);

			builder.SetVisibility(specificBag.PreviousEntryLineNumberCalcEdit, x => x.HasOutOfWarehouseProcedure, x => x.JI_ProcedureInfo);
			builder.SetVisibility(specificBag.PreviousEntryNumberTextBox, x => x.HasOutOfWarehouseProcedure, x => x.JI_ProcedureInfo);

			return builder.Build();
		}
	}
}
