using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.GUI.Testing;

[TestedType(typeof(TransportDetailsUserControlBag))]
sealed class TransportDetailsUserControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(TransportDetailsUserControlBag.FlightAndNationalityUserControl);
		}
	}

	protected override ControlBag GetControlBagForTesting() => TransportDetailsUserControlBag.Instance;
}
