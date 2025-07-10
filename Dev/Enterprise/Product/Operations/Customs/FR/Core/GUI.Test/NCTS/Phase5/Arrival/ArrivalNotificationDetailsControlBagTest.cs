using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	[TestedType(typeof(ArrivalNotificationDetailsControlBag))]
	sealed class ArrivalNotificationDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ArrivalNotificationDetailsUserControl.ExpectedNextCustomsProcedureDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ArrivalNotificationDetailsControlBag.Instance;
	}
}
