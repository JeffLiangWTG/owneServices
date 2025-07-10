using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(TransportBorderControlBag))]
	sealed class TransportBorderControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TransportBorderControlBag.BorderTransportModeDropEdit);
				yield return nameof(TransportBorderControlBag.BorderTransportTypeOfIdDropEdit);
				yield return nameof(TransportBorderControlBag.BorderTransportIdAndNationalityUserControl);
				yield return nameof(TransportBorderControlBag.BorderConveyanceNumberTextBox);
				yield return nameof(TransportBorderControlBag.BorderOfficeDropEdit);
				yield return nameof(TransportBorderControlBag.AdditionalTransportBorderUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TransportBorderControlBag.Instance;
	}
}
