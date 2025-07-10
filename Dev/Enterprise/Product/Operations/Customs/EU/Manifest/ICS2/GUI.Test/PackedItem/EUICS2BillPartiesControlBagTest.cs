using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	[TestedType(typeof(EUICS2PackedItemDetailsControlBag))]
	sealed class EUICS2PackedItemDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(EUICS2PackedItemDetailsControlBag.CusCodeFindBox);
				yield return nameof(EUICS2PackedItemDetailsControlBag.PostalValueCalcFindBox);
				yield return nameof(EUICS2PackedItemDetailsControlBag.TypeOfGoodsDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => EUICS2PackedItemDetailsControlBag.Instance;
	}
}
