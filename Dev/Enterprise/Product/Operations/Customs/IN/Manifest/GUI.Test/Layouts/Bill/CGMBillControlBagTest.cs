using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.GUI.Testing;

[TestedType(typeof(CGMBillControlBag))]
sealed class CGMBillControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(CGMBillControlBag.BondDetailsUserControl);
			yield return nameof(CGMBillControlBag.FinalDestinationDetailsUserControl);
			yield return nameof(CGMBillControlBag.TransshipmentDetailsUserControl);
		}
	}

	protected override ControlBag GetControlBagForTesting() => CGMBillControlBag.Instance;
}
