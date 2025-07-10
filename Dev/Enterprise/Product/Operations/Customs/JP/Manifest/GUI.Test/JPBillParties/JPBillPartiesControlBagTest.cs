using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.GUI.Testing
{
	[TestedType(typeof(JPBillPartiesControlBag))]
	sealed class JPBillPartiesControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(JPBillPartiesControlBag.ConsigneeRegNoPanel);
				yield return nameof(JPBillPartiesControlBag.ShipperRegNoPanel);
				yield return nameof(JPBillPartiesControlBag.NotifyPartyRegNoPanel);
			}
		}

		protected override ControlBag GetControlBagForTesting() => JPBillPartiesControlBag.Instance;
	}
}
