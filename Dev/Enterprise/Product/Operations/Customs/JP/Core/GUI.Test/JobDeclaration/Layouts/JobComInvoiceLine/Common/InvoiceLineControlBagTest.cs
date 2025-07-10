using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing;

[TestedType(typeof(InvoiceLineControlBag))]
sealed class InvoiceLineControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(InvoiceLineControlBag.NACCSCodeDropEdit);
			yield return nameof(InvoiceLineControlBag.CustomsQuantityCalcDropEdit);
			yield return nameof(InvoiceLineControlBag.CustomsSecondQuantityCalcDropEdit);
			yield return nameof(InvoiceLineControlBag.EntryInstructionGuidDropEdit);
			yield return nameof(InvoiceLineControlBag.UnitPriceCalcEdit);
			yield return nameof(InvoiceLineControlBag.VolumeCalcDropEdit);
			yield return nameof(InvoiceLineControlBag.WeightCalcDropEdit);
			yield return nameof(InvoiceLineControlBag.InvoiceQuantityCalcDropEdit);
			yield return nameof(InvoiceLineControlBag.LinePriceCurrencyCalcFindBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => InvoiceLineControlBag.Instance;
}
