using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AR.Manifest.GUI.Testing
{
	[TestedType(typeof(ARBillControlBag))]
	sealed class ARBillControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ARBillControlBag.IsInformedToRenarCheckBox);
				yield return nameof(ARBillControlBag.IsMonitoredTransitCheckBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ARBillControlBag.Instance;
	}
}
