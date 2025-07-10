using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(EntryInstructionControlBag))]
	sealed class EntryInstructionControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				return new[]
				{
					nameof(EntryInstructionControlBag.TradeTypePanel),
					nameof(EntryInstructionControlBag.CustomsInspectionCodeTextBox),
					nameof(EntryInstructionControlBag.CargoQuantityCalcDropEdit),
					nameof(EntryInstructionControlBag.WeightzCalcDropEdit),
					nameof(EntryInstructionControlBag.ContainerCountCalcEdit),
					nameof(EntryInstructionControlBag.CustomsWeightCalcDropEdit),
					nameof(EntryInstructionControlBag.ValueTypeDropEdit),
					nameof(EntryInstructionControlBag.DeclarationTypeDropEdit),
					nameof(EntryInstructionControlBag.AdditionalDeclarationTypeDropEdit),
				};
			}
		}

		protected override ControlBag GetControlBagForTesting() => EntryInstructionControlBag.Instance;
	}
}
