using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
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
				yield return nameof(TransportAndPackagingUserControl.PortOfPresentationCodeFindBox);
				yield return nameof(TransportAndPackagingUserControl.ChargePaymentOrDestinationIDDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TransportAndPackagingControlBag.Instance;
	}
}
