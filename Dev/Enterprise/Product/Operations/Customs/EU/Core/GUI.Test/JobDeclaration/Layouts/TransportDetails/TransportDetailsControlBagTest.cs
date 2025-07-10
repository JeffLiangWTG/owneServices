using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(TransportDetailsControlBag))]
	sealed class TransportDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TransportDetailsControlBag.InlandTransportDetailsUserControl);
				yield return nameof(TransportDetailsControlBag.AircraftRegistrationNumberTextBox);
				yield return nameof(TransportDetailsControlBag.FlightAndNationalityUserControl);
				yield return nameof(TransportDetailsControlBag.TransportIDAndNationalityInlandWaterwayENIUserControl);
				yield return nameof(TransportDetailsControlBag.TransportIDAndNationalityInlandWaterwayUserControl);
				yield return nameof(TransportDetailsControlBag.TransportIDAndNationalityRailUserControl);
				yield return nameof(TransportDetailsControlBag.TransportIDAndNationalityUserControl);
				yield return nameof(TransportDetailsControlBag.Instance.AdditionalWagonNumbersUserControl);
				yield return nameof(TransportDetailsControlBag.Instance.TransportNationalityCodeFindBox);
				yield return nameof(TransportDetailsControlBag.VesselUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TransportDetailsControlBag.Instance;
	}
}
