using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(TransportDetailsControlBag))]
	sealed class TransportDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TransportDetailsControlBag.Instance.CargoArrivalDocUtilizationDropEdit);
				yield return nameof(TransportDetailsControlBag.Instance.VesselAndCountryUserControl);
				yield return nameof(TransportDetailsControlBag.Instance.PlateTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TransportDetailsControlBag.Instance;
	}
}
