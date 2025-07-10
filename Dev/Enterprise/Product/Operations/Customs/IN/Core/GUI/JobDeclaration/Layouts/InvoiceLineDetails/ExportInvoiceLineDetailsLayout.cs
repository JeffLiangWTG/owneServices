using Enterprise.Customs.GUI;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public class ExportInvoiceLineDetailsLayout : IPanelLayoutProvider
{
	public PanelLayout Layout
	{
		get
		{
			var builder = new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();
			var commonBag = builder.CommonBag;
			var inBag = InvoiceLineDetailsControlBag.Instance;
			builder.AddControlBag(inBag);

			builder.AddColumn();
			builder.Add(commonBag.DescriptionLongTextControl, ControlWidthClass.Long);
			builder.Add(commonBag.InvoiceQuantityCalcDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Long);
			builder.Add(inBag.UnitPriceCalcFindBox, ControlWidthClass.Long);
			builder.Add(inBag.PMVFieldsUserControl, ControlWidthClass.Long);
			builder.Add(inBag.AccessoryStatusDropEdit, ControlWidthClass.Long);
			builder.Add(inBag.RewardItemDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.WithDescriptionTariffFindBox, ControlWidthClass.Long);
			builder.Add(inBag.IGSTPaymentGroupBox, ControlWidthClass.LongNoCaption);

			builder.AddColumn();
			builder.Add(commonBag.CommodityCodeFindBox, ControlWidthClass.Long);
			builder.Add(inBag.UnitQuantityCalcDropEdit, ControlWidthClass.Long);
			builder.Add(inBag.TotalPMVCalcFindBox, ControlWidthClass.Long);
			builder.Add(inBag.TransitCountryDropEdit, ControlWidthClass.Long);
			builder.Add(inBag.EndUseCodeFindBox, ControlWidthClass.Long);
			builder.Add(inBag.AccessoryDescriptionLongTextBox, ControlWidthClass.Long, inBag.AccessoryStatusDropEdit);

			builder.AddColumn();
			builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Long, alignToControl: commonBag.InvoiceQuantityCalcDropEdit);
			builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.StateOrRegionOfOriginDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfOriginCodeFindBox, ControlWidthClass.Long);

			builder.AddControlBehaviour<ZCalcDropEdit>(commonBag.InvoiceQuantityCalcDropEdit, (control, _) =>
			{
				control.MaxValue = 99999999.999m;
				control.Decimals = 3;
			});
			return builder.Build();
		}
	}
}
