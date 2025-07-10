using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(ECREntryInstructionControlBag))]
	sealed class ECREntryInstructionControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				return new[]
				{
					nameof(ECREntryInstructionControlBag.CusEntryInstructionNSITextBox),
					nameof(ECREntryInstructionControlBag.ECRNotesTextBox),
					nameof(ECREntryInstructionControlBag.ECRCargoTypeDropEdit),
					nameof(ECREntryInstructionControlBag.VolumeCalcDropEdit),
					nameof(ECREntryInstructionControlBag.CustomsVolumeCalcDropEdit),
					nameof(ECREntryInstructionControlBag.SpecialCargoCodeFindBox),
				};
			}
		}

		protected override ControlBag GetControlBagForTesting() => ECREntryInstructionControlBag.Instance;
	}
}
