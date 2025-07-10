using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(RCREntryInstructionControlBag))]
	sealed class RCREntryInstructionControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				return new[]
				{
					nameof(RCREntryInstructionControlBag.RCRActionDropEdit),
					nameof(RCREntryInstructionControlBag.PreviousBillNumberTextBox),
					nameof(RCREntryInstructionControlBag.ViaLocationCodeFindBox),
				};
			}
		}

		protected override ControlBag GetControlBagForTesting() => RCREntryInstructionControlBag.Instance;
	}
}
