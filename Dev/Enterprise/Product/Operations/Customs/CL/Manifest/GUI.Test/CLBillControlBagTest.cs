using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.GUI.Testing
{
	[TestedType(typeof(CLBillControlBag))]
	sealed class CLBillControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CLBillControlBag.RoRoCheckBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CLBillControlBag.Instance;
	}
}
