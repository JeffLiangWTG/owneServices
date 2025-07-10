using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(EventControlBag))]
	sealed class EventUserControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(EventControlBag.Phase5EventTabUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => EventControlBag.Instance;
	}
}
