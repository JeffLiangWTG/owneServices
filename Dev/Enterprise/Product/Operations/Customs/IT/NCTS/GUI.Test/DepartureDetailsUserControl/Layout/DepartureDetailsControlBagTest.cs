using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(DepartureDetailsControlBag))]
sealed class DepartureDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(DepartureDetailsControlBag.LocationOfGoodsUserControl);
		}
	}
	protected override ControlBag GetControlBagForTesting() => DepartureDetailsControlBag.Instance;
}
