using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(SWProductionDetailsControlBag))]
sealed class SWProductionDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(SWProductionDetailsControlBag.BatchIDTextBox);
			yield return nameof(SWProductionDetailsControlBag.BatchQuantityDropEdit);
			yield return nameof(SWProductionDetailsControlBag.ManufacturingDateEdit);
			yield return nameof(SWProductionDetailsControlBag.ExpiryDateEdit);
			yield return nameof(SWProductionDetailsControlBag.BestBeforeDateTimeOffsetEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => SWProductionDetailsControlBag.Instance;
}

