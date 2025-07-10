using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(HouseConsignmentDetailsControlBag))]
public sealed class HouseConsignmentDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(HouseConsignmentDetailsControlBag.CustomsStatusUserControl);
		}
	}
	protected override ControlBag GetControlBagForTesting() => HouseConsignmentDetailsControlBag.Instance;
}
