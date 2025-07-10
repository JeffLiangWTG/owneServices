using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(TransportAndPackagingControlBag))]
	sealed class TransportAndPackagingControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TransportAndPackagingUserControl.TransportMethodOfPaymentDropEdit);
				yield return nameof(TransportAndPackagingUserControl.CarrierDocAddressControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TransportAndPackagingControlBag.Instance;
	}
}
