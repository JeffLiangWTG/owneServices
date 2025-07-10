using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(TransportDetailsControlBag))]
	sealed class TransportDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TransportDetailsControlBag.MasterBillAndIATAUserControl);
				yield return nameof(TransportDetailsControlBag.TransportInlandRailUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TransportDetailsControlBag.Instance;
	}
}
