using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.GUI.Testing
{
	[TestedType(typeof(EntryInstructionDetailsControlBag))]
	class EntryInstructionDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(EntryInstructionDetailsControlBag.UCRNumberTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => EntryInstructionDetailsControlBag.Instance;
	}
}
