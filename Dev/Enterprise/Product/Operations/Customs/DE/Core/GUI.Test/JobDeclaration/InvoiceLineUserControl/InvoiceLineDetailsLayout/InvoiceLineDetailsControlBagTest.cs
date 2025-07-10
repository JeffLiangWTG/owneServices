using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(InvoiceLineDetailsControlBag))]
	sealed class InvoiceLineDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(InvoiceLineDetailsControlBag.CessionManagementFlagDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.SupplementaryInformationTextBox);
				yield return nameof(InvoiceLineDetailsControlBag.OriginFederalStateDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.InvoiceNumberDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.QuotaQtyCalcDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.TobaccoStampTextBox);
				yield return nameof(InvoiceLineDetailsControlBag.IsMainPackCheckBox);
				yield return nameof(InvoiceLineDetailsControlBag.UsualReplacementCheckBox);
				yield return nameof(InvoiceLineDetailsControlBag.ReimportDateEdit);
				yield return nameof(InvoiceLineDetailsControlBag.ExportCountryCodeFindBox);
				yield return nameof(InvoiceLineDetailsControlBag.DecisiveDateEdit);
				yield return nameof(InvoiceLineDetailsControlBag.OutwardMRNTextBox);
				yield return nameof(InvoiceLineDetailsControlBag.OutwardDecisiveDateEdit);
				yield return nameof(InvoiceLineDetailsControlBag.NetPriceCurrencyCalcFindBox);
				yield return nameof(InvoiceLineDetailsControlBag.DescriptionLongTextControl);
				yield return nameof(InvoiceLineDetailsControlBag.DgSubstanceUserControl);
				yield return nameof(InvoiceLineDetailsControlBag.FixedMaxLengthEntryInstructionGuidDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.FixedMaxLengthWithDescriptionTariffFindBox);
				yield return nameof(InvoiceLineDetailsControlBag.FixedMaxLengthInvoiceNumberDropEdit);
				yield return nameof(InvoiceLineDetailsControlBag.BondedWHSOrderNumberTextBox);
				yield return nameof(InvoiceLineDetailsControlBag.BondedWHSOrderLineNumberCalcEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => InvoiceLineDetailsControlBag.Instance;
	}
}
