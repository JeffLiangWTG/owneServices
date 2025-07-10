using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	[TestedType(typeof(EUICS2BillControlBag))]
	sealed class EUICS2BillControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(EUICS2BillControlBag.TransportDocumentTypeDropEdit);
				yield return nameof(EUICS2BillControlBag.FreightValueAndCurrencyCalcFindBox);
				yield return nameof(EUICS2BillControlBag.ReceptacleIdTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => EUICS2BillControlBag.Instance;
	}
}
