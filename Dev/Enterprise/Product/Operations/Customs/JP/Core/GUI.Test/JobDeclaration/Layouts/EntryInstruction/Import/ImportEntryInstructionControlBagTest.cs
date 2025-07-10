using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(ImportEntryInstructionControlBag))]
	sealed class ImportEntryInstructionControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				return new[]
				{
					nameof(ImportEntryInstructionControlBag.BeforePermitApplicationReasonDropEdit),
					nameof(ImportEntryInstructionControlBag.BondedLocationCodeFindBox),
					nameof(ImportEntryInstructionControlBag.BondedLocationNameTextBox),
					nameof(ImportEntryInstructionControlBag.ContentInspectionResultDropEdit),
					nameof(ImportEntryInstructionControlBag.DutyDrawbackDropEdit),
					nameof(ImportEntryInstructionControlBag.SpecialDeclarationOfficeGroupBox),
					nameof(ImportEntryInstructionControlBag.DeclarationCargoTypeDropEdit),
					nameof(ImportEntryInstructionControlBag.SpecialDeclarationTypeDropEdit),
				};
			}
		}

		protected override ControlBag GetControlBagForTesting() => ImportEntryInstructionControlBag.Instance;
	}
}
