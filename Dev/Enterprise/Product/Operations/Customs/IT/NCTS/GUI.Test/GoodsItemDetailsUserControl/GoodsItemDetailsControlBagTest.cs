using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(GoodsItemDetailsControlBag))]
sealed class GoodsItemDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(GoodsItemDetailsControlBag.CustomsStatusUserControl);
		}
	}

	protected override ControlBag GetControlBagForTesting() => GoodsItemDetailsControlBag.Instance;
}
