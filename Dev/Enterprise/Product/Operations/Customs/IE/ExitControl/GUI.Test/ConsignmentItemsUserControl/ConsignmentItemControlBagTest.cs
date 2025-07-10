using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.GUI.Testing
{
	[TestedType(typeof(ConsignmentItemControlBag))]
	sealed class ConsignmentItemControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return (nameof(ConsignmentItemUserControlDetails.MainUserControl));
			}
		}

		protected override ControlBag GetControlBagForTesting() => ConsignmentItemControlBag.Instance;
	}
}
