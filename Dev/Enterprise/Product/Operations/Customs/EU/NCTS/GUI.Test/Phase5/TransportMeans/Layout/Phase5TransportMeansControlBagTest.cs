using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5TransportMeansControlBag))]
	sealed class Phase5TransportMeansControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(Phase5TransportMeansControlBag.TransportAtDepartureTypeDropEdit);
				yield return nameof(Phase5TransportMeansControlBag.TransportAtDepartureIDTextBox);
				yield return nameof(Phase5TransportMeansControlBag.TransportAtDepartureNationalityCodeFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => Phase5TransportMeansControlBag.Instance;
	}
}
