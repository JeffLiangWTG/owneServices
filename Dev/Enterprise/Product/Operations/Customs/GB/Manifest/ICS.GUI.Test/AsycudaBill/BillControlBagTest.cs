using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.ICS.GUI.Testing
{
	[TestedType(typeof(BillControlBag))]
	sealed class BillControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(BillControlBag.SpecialMentionsDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => BillControlBag.Instance;
	}
}
