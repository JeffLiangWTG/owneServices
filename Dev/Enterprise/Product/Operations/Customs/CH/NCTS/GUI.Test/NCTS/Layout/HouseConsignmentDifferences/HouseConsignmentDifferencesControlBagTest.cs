using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(HouseConsignmentDifferencesControlBag))]
sealed class HouseConsignmentDifferencesControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(HouseConsignmentDifferencesControlBag.UnloadingRemarkCodeDropEdit);
			yield return nameof(HouseConsignmentDifferencesControlBag.UnloadingRemarkTextTextBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => HouseConsignmentDifferencesControlBag.Instance;
}
