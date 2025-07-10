using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(CDB01EntryInstructionControlBag))]
	sealed class CDB01EntryInstructionControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				return new[]
				{
					nameof(CDB01EntryInstructionControlBag.CDB01BillNumberUserControl),
					nameof(CDB01EntryInstructionControlBag.DateForDutyDateEdit),
					nameof(CDB01EntryInstructionControlBag.CDB01CargoTypeDropEdit),
					nameof(CDB01EntryInstructionControlBag.CDB01PermitNumberTextBox),
					nameof(CDB01EntryInstructionControlBag.CDB01MoveInUserControl),
					nameof(CDB01EntryInstructionControlBag.MAWBTextBox),
					nameof(CDB01EntryInstructionControlBag.PortOfLoadingPanel),
					nameof(CDB01EntryInstructionControlBag.FinalDestinationPanel),
					nameof(CDB01EntryInstructionControlBag.ExternalBrokerGroupBox),
					nameof(CDB01EntryInstructionControlBag.AirCargoAgentGroupBox),
					nameof(CDB01EntryInstructionControlBag.ForwarderGroupBox),
					nameof(CDB01EntryInstructionControlBag.CarrierGroupBox),
			};
			}
		}

		protected override ControlBag GetControlBagForTesting() => CDB01EntryInstructionControlBag.Instance;
	}
}
