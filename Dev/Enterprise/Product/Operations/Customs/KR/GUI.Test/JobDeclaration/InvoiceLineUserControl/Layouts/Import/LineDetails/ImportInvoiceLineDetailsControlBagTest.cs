using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ImportInvoiceLineDetailsControlBag))]
	sealed class ImportInvoiceLineDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.ProductCodeFindBox);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.LotNumberTextBox);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.TariffFindBox);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.GoodsDescriptionLongTextControl);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.BrandCodeFindBox);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.BrandNameTextBox);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.ModelTradeNameTextBox);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.IngredientLongTextControl);

				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.CustomsQtyCalcDropEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.DrawBackQtyCalcDropEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.NetWeightCalcDropEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.GrossWeightCalcDropEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.CustomsUnitPriceCalcEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.InvoiceQtyCalcDropEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.UnitPriceCalcEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.PriceCalcFindBox);

				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.DutyRateTypeDropEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.MinMaxDutyDropEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.PreferenceCodeDropEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.DutyRateCalcEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.DutyCodeDropEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.AddDutyRateCalcEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.DutyReductionCodeFindBox);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.DutyReductionRateCalcEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.InstalmentCodeFindBox);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.SpecificUsePermitNoTextBox);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.SpecificUseCheckBox);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.DomesticTaxCodeFindBox);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.DomesticTaxTypeDropEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.DomesticTaxRateCalcEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.DomesticTaxExemptionCodeFindBox);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.DomesticTaxBaseQtyOrPriceCalcEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.VATTypeDropEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.VATReductionCodeFindBox);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.EducationTaxTypeDropEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.AgricultureTaxTypeDropEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.CustomsSecondQuantityCalcDropEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.CustomsThirdQuantityCalcDropEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.CustomsFourthQuantityCalcDropEdit);
				yield return nameof(ImportInvoiceLineDetailsControlBag.Instance.InstallationCostCalcEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ImportInvoiceLineDetailsControlBag.Instance;
	}
}
