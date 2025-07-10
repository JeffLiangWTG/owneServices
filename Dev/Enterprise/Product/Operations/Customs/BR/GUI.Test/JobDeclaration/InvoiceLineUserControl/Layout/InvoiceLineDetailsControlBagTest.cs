using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(InvoiceLineDetailsControlBag))]
	sealed class InvoiceLineDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(InvoiceLineDetailsControlBag.CPCGroupBox);
				yield return nameof(InvoiceLineDetailsControlBag.ComplementaryDescriptionTextBox);
				yield return nameof(InvoiceLineDetailsControlBag.BRNFEItemNumberTextBox);
				yield return nameof(InvoiceLineDetailsControlBag.BRNFENumberTextBox);
				yield return nameof(InvoiceLineDetailsControlBag.CargoPriorityDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.CountryDestinationCodeFindBox);
				yield return nameof(InvoiceLineDetailsControlBag.NFeLinePriceCalcEdit);
				yield return nameof(InvoiceLineDetailsControlBag.NaladiNccaCodeFindBox);
				yield return nameof(InvoiceLineDetailsControlBag.NaladiHsCodeFindBox);
				yield return nameof(InvoiceLineDetailsControlBag.ImportLicenseNumberTextBox);
				yield return nameof(InvoiceLineDetailsControlBag.RequiresImportLicenseCheckBox);
				yield return nameof(InvoiceLineDetailsControlBag.GoodsConditionSeparatorUserControl);
				yield return nameof(InvoiceLineDetailsControlBag.UsedMaterialRegimeDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.GoodsConditionOperationTypeDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.ManufacturerIndicatorDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.SerialNumberGoodsConditionTextBox);
				yield return nameof(InvoiceLineDetailsControlBag.YearGoodsConditionTextBox);
				yield return nameof(InvoiceLineDetailsControlBag.ModelGoodsConditionTextBox);
				yield return nameof(InvoiceLineDetailsControlBag.BrandGoodsConditionTextBox);
				yield return nameof(InvoiceLineDetailsControlBag.DutyTaxRegimeSeparatorUserControl);
				yield return nameof(InvoiceLineDetailsControlBag.DutyTaxRegimeDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.DutyLegalBaseDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.ImportLicenseFineSeparatorUserControl);
				yield return nameof(InvoiceLineDetailsControlBag.ImportLicenseTypeDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.ImportLicenseAuthorizationDateTextBox);
				yield return nameof(InvoiceLineDetailsControlBag.ImportLicenseFeeTypeDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.TariffAgreementSeparatorUserControl);
				yield return nameof(InvoiceLineDetailsControlBag.TariffAgreementDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.GoodsApplicationDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.GoodsConditionDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.FullGoodsDescriptionTextBox);
				yield return nameof(InvoiceLineDetailsControlBag.EntryInstructionGuidDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => InvoiceLineDetailsControlBag.Instance;
	}
}
