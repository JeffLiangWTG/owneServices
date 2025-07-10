using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7PackDetailsControlBag))]
	sealed class EUH7PackDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(EUH7PackDetailsControlBag.GoodsDescriptionTextBox);
				yield return nameof(EUH7PackDetailsControlBag.PackUQDropEdit);
				yield return nameof(EUH7PackDetailsControlBag.PackQtyCalcEdit);
				yield return nameof(EUH7PackDetailsControlBag.MarksAndNumbersTextBox);
				yield return nameof(EUH7PackDetailsControlBag.WeightCalcDropEdit);
				yield return nameof(EUH7PackDetailsControlBag.VolumeCalcDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => EUH7PackDetailsControlBag.Instance;
	}
}
