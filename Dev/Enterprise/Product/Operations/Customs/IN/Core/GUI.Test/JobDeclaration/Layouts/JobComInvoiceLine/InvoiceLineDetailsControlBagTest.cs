using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(InvoiceLineDetailsControlBag))]
sealed class InvoiceLineDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(InvoiceLineDetailsControlBag.UnitQuantityCalcDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.UnitPriceCalcFindBox);
			yield return nameof(InvoiceLineDetailsControlBag.AccessoryStatusDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.EndUseCodeFindBox);
			yield return nameof(InvoiceLineDetailsControlBag.IGSTPaymentGroupBox);
			yield return nameof(InvoiceLineDetailsControlBag.RewardItemDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.TotalPMVCalcFindBox);
			yield return nameof(InvoiceLineDetailsControlBag.PMVFieldsUserControl);
			yield return nameof(InvoiceLineDetailsControlBag.TransitCountryDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.AccessoryDescriptionLongTextBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => InvoiceLineDetailsControlBag.Instance;
}
