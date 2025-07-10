using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(TransportDepartureControlBag))]
	sealed class TransportDepartureControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TransportDepartureControlBag.InlandTransportModeDropEdit);
				yield return nameof(TransportDepartureControlBag.TransportAtDepartureTextBox);
				yield return nameof(TransportDepartureControlBag.TransportAtDepartureCountryCodeFindBox);
				yield return nameof(TransportDepartureControlBag.TransportAtDepartureTrailer1RegNoTextBox);
				yield return nameof(TransportDepartureControlBag.TransportAtDepartureTrailer1NationalityCodeFindBox);
				yield return nameof(TransportDepartureControlBag.TransportAtDepartureTrailer2RegNoTextBox);
				yield return nameof(TransportDepartureControlBag.TransportAtDepartureTrailer2NationalityCodeFindBox);
				yield return nameof(TransportDepartureControlBag.TransportAtDepartureTypeDropEdit);
				yield return nameof(TransportDepartureControlBag.AircraftIDAtDepartureTextBox);
				yield return nameof(TransportDepartureControlBag.VesselCodeFindBox);
				yield return nameof(TransportDepartureControlBag.VesselCountryCodeFindBox);
				yield return nameof(TransportDepartureControlBag.AdditionalWagonNumbersButton);
				yield return nameof(TransportDepartureControlBag.PlaceHolderLabel);
				yield return nameof(TransportDepartureControlBag.PlaceHolder2Label);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TransportDepartureControlBag.Instance;
	}
}
