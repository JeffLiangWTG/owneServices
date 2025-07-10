using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5GoodsItemDifferencesDetailsControlBag))]
	sealed class Phase5GoodsItemDifferencesDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(Phase5GoodsItemDifferencesDetailsControlBag.SequenceNumberTextBox);
				yield return nameof(Phase5GoodsItemDifferencesDetailsControlBag.ItemNumberTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => Phase5GoodsItemDifferencesDetailsControlBag.Instance;
	}
}
