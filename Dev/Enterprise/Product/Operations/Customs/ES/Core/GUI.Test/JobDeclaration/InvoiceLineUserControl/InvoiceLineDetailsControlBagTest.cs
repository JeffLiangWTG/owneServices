using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(InvoiceLineDetailsControlBag))]
sealed class InvoiceLineDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(InvoiceLineDetailsControlBag.CommercialReferenceTextBox);
			yield return nameof(InvoiceLineDetailsControlBag.T2LItemNumberCalcEdit);
			yield return nameof(InvoiceLineDetailsControlBag.CountryOfDestinationCodeFindBox);
			yield return nameof(InvoiceLineDetailsControlBag.RegionOfDestinationCodeFindBox);
			yield return nameof(InvoiceLineDetailsControlBag.MethodOfPaymentDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.MethodOfPayment2DropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.VATIGICTypeDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.AIEMTypeDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.ExciseExemptionDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.ExciseCodeDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.GlobalWarmingPotentialCalcEdit);
			yield return nameof(InvoiceLineDetailsControlBag.PVPCalcFindBox);
			yield return nameof(InvoiceLineDetailsControlBag.REAProductCodeDropEdit);
			yield return nameof(InvoiceLineDetailsControlBag.READirectConsumptionCheckBox);
			yield return nameof(InvoiceLineDetailsControlBag.HasNonRecycledPlasticsCheckBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => InvoiceLineDetailsControlBag.Instance;
}
