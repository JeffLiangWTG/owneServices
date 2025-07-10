using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class ImportInvoiceLineDetailsControlBag : ControlBag
	{
		public ImportInvoiceLineDetailsControlBag()
		{
			ProductCodeFindBox = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.ProductCodeFindBox));
			LotNumberTextBox = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.LotNumberTextBox));
			TariffFindBox = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.TariffFindBox));
			GoodsDescriptionLongTextControl = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.GoodsDescriptionLongTextControl));
			BrandCodeFindBox = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.BrandCodeFindBox));
			BrandNameTextBox = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.BrandNameTextBox));
			ModelTradeNameTextBox = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.ModelTradeNameTextBox));
			IngredientLongTextControl = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.IngredientLongTextControl));

			CustomsQtyCalcDropEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.CustomsQtyCalcDropEdit));
			DrawBackQtyCalcDropEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.DrawBackQtyCalcDropEdit));
			NetWeightCalcDropEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.NetWeightCalcDropEdit));
			GrossWeightCalcDropEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.GrossWeightCalcDropEdit));
			CustomsUnitPriceCalcEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.CustomsUnitPriceCalcEdit));
			InvoiceQtyCalcDropEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.InvoiceQtyCalcDropEdit));
			UnitPriceCalcEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.UnitPriceCalcEdit));
			PriceCalcFindBox = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.PriceCalcFindBox));

			DutyRateTypeDropEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.DutyRateTypeDropEdit));
			MinMaxDutyDropEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.MinMaxDutyDropEdit));
			PreferenceCodeDropEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.PreferenceCodeDropEdit));
			DutyRateCalcEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.DutyRateCalcEdit));
			DutyCodeDropEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.DutyCodeDropEdit));
			AddDutyRateCalcEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.AddDutyRateCalcEdit));
			DutyReductionCodeFindBox = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.DutyReductionCodeFindBox));
			DutyReductionRateCalcEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.DutyReductionRateCalcEdit));
			InstalmentCodeFindBox = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.InstalmentCodeFindBox));
			SpecificUsePermitNoTextBox = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.SpecificUsePermitNoTextBox));
			SpecificUseCheckBox = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.SpecificUseCheckBox));
			DomesticTaxCodeFindBox = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.DomesticTaxCodeFindBox));
			DomesticTaxTypeDropEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.DomesticTaxTypeDropEdit));
			DomesticTaxRateCalcEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.DomesticTaxRateCalcEdit));
			DomesticTaxExemptionCodeFindBox = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.DomesticTaxExemptionCodeFindBox));
			DomesticTaxBaseQtyOrPriceCalcEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.DomesticTaxBaseQtyOrPriceCalcEdit));
			VATTypeDropEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.VATTypeDropEdit));
			VATReductionCodeFindBox = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.VATReductionCodeFindBox));
			EducationTaxTypeDropEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.EducationTaxTypeDropEdit));
			AgricultureTaxTypeDropEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.AgricultureTaxTypeDropEdit));

			CustomsSecondQuantityCalcDropEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.CustomsSecondQuantityCalcDropEdit));
			CustomsThirdQuantityCalcDropEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.CustomsThirdQuantityCalcDropEdit));
			CustomsFourthQuantityCalcDropEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.CustomsFourthQuantityCalcDropEdit));
			InstallationCostCalcEdit = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.InstallationCostCalcEdit));
		}

		public static ImportInvoiceLineDetailsControlBag Instance => instance ?? (instance = new ImportInvoiceLineDetailsControlBag());

		[ThreadStatic]
		static ImportInvoiceLineDetailsControlBag instance;

		public ControlReference ProductCodeFindBox { get; }
		public ControlReference LotNumberTextBox { get; }
		public ControlReference TariffFindBox { get; }
		public ControlReference GoodsDescriptionLongTextControl { get; }
		public ControlReference BrandCodeFindBox { get; }
		public ControlReference BrandNameTextBox { get; }
		public ControlReference ModelTradeNameTextBox { get; }
		public ControlReference IngredientLongTextControl { get; }

		public ControlReference CustomsQtyCalcDropEdit { get; }
		public ControlReference DrawBackQtyCalcDropEdit { get; }
		public ControlReference NetWeightCalcDropEdit { get; }
		public ControlReference GrossWeightCalcDropEdit { get; }
		public ControlReference CustomsUnitPriceCalcEdit { get; }
		public ControlReference InvoiceQtyCalcDropEdit { get; }
		public ControlReference UnitPriceCalcEdit { get; }
		public ControlReference PriceCalcFindBox { get; }

		public ControlReference DutyRateTypeDropEdit { get; }
		public ControlReference MinMaxDutyDropEdit { get; }
		public ControlReference PreferenceCodeDropEdit { get; }
		public ControlReference DutyRateCalcEdit { get; }
		public ControlReference DutyCodeDropEdit { get; }
		public ControlReference AddDutyRateCalcEdit { get; }
		public ControlReference DutyReductionCodeFindBox { get; }
		public ControlReference DutyReductionRateCalcEdit { get; }
		public ControlReference InstalmentCodeFindBox { get; }
		public ControlReference SpecificUsePermitNoTextBox { get; }
		public ControlReference SpecificUseCheckBox { get; }
		public ControlReference DomesticTaxCodeFindBox { get; }
		public ControlReference DomesticTaxTypeDropEdit { get; }
		public ControlReference DomesticTaxRateCalcEdit { get; }
		public ControlReference DomesticTaxExemptionCodeFindBox { get; }
		public ControlReference DomesticTaxBaseQtyOrPriceCalcEdit { get; }
		public ControlReference VATTypeDropEdit { get; }
		public ControlReference VATReductionCodeFindBox { get; }
		public ControlReference EducationTaxTypeDropEdit { get; }
		public ControlReference AgricultureTaxTypeDropEdit { get; }
		public ControlReference CustomsSecondQuantityCalcDropEdit { get; }
		public ControlReference CustomsThirdQuantityCalcDropEdit { get; }
		public ControlReference CustomsFourthQuantityCalcDropEdit { get; }
		public ControlReference InstallationCostCalcEdit { get; }

		protected override Control CreateTemplate() => new ImportInvoiceLineDetailsUserControl();
	}
}
