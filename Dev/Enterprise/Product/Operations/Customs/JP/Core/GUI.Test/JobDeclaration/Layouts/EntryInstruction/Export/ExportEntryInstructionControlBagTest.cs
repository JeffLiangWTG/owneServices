using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(ExportEntryInstructionControlBag))]
	sealed class ExportEntryInstructionControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				return new[]
				{
					nameof(ExportEntryInstructionControlBag.AwbOrBillNumberTextBox),
					nameof(ExportEntryInstructionControlBag.ExportControlNumberTextBox),
					nameof(ExportEntryInstructionControlBag.PreInspectedCargoDropEdit),
					nameof(ExportEntryInstructionControlBag.LoadingConfirmationIsRequiredCheckBox),
					nameof(ExportEntryInstructionControlBag.VanningLocationsGroupBox),
					nameof(ExportEntryInstructionControlBag.DeclarationCargoTypeDropEdit),
					nameof(ExportEntryInstructionControlBag.GoodsDescriptionTextBox),
				};
			}
		}

		protected override ControlBag GetControlBagForTesting() => ExportEntryInstructionControlBag.Instance;
	}
}
