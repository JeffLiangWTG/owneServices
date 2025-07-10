using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public class ImportInvoiceLineLayouts : IPanelLayoutProvider
	{
		public PanelLayout Layout
		{
			get
			{
				var builder = new ImportInvoiceLineLayoutBuilder();
				var jpImportBag = builder.JPImportBag;
				var commonBag = builder.CommonBag;
				var jpCommonBag = builder.JPCommonBag;
				builder.AddControlBag(jpImportBag);
				builder.AddControlBag(jpCommonBag);

				builder.AddColumn();
				builder.Add(jpCommonBag.EntryInstructionGuidDropEdit, ControlWidthClass.Auto);
				builder.Add(jpImportBag.TariffFindBox, ControlWidthClass.Auto);
				builder.Add(jpCommonBag.NACCSCodeDropEdit, ControlWidthClass.Long);
				builder.Add(jpCommonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
				builder.Add(jpCommonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
				builder.Add(jpImportBag.CertificateOfOriginPanel, ControlWidthClass.Auto);
				builder.Add(jpImportBag.DutyRateTextBox, ControlWidthClass.Auto);
				builder.Add(jpImportBag.ProcedureTextBox, ControlWidthClass.Auto);
				builder.Add(jpImportBag.StorageTypeDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.BondedWHSOrderNumberTextBox, ControlWidthClass.Auto);
				builder.Add(commonBag.BondedWHSOrderLineNumberCalcEdit, ControlWidthClass.Auto);

				builder.AddColumn();
				builder.Add(commonBag.DescriptionLongTextControl, ControlWidthClass.Auto);
				builder.Add(jpCommonBag.VolumeCalcDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.CommodityCodeFindBox, ControlWidthClass.Auto);
				builder.Add(commonBag.CountryOfOriginCodeFindBox, ControlWidthClass.Auto);
				builder.Add(jpCommonBag.WeightCalcDropEdit, ControlWidthClass.Auto);
				builder.Add(jpCommonBag.UnitPriceCalcEdit, ControlWidthClass.Auto);
				builder.Add(jpCommonBag.InvoiceQuantityCalcDropEdit, ControlWidthClass.Auto);
				builder.Add(jpCommonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);

				return builder.Build();
			}
		}
	}
}
