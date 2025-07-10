using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.GUI.Testing
{
	[TestedType(typeof(GoodsItemDetailsControlBag))]
	sealed class GoodsItemDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(GoodsItemDetailsControlBag.GoodsItemDetailsItemNoPlusMainPackUserControl);
			}
		}
		protected override ControlBag GetControlBagForTesting() => GoodsItemDetailsControlBag.Instance;
	}
}
