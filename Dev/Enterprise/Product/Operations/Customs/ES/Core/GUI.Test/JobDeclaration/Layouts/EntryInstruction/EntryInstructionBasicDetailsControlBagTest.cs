using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(EntryInstructionDetailsBasicUserControlBag))]
sealed class EntryInstructionBasicDetailsControlBagTest : ControlBagAbstractTest
{
	protected override ControlBag GetControlBagForTesting() => EntryInstructionDetailsBasicUserControlBag.Instance;

	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(EntryInstructionDetailsBasicUserControlBag.LocationOfGoodsUserControl);
			yield return nameof(EntryInstructionDetailsBasicUserControlBag.ActivateByOperatorCheckBox);
			yield return nameof(EntryInstructionDetailsBasicUserControlBag.IncludeRoutingSecurityDataCheckBox);
			yield return nameof(EntryInstructionDetailsBasicUserControlBag.BondHolderOrganisationControl);
			yield return nameof(EntryInstructionDetailsBasicUserControlBag.NewOwnerOrganisationControl);
			yield return nameof(EntryInstructionDetailsBasicUserControlBag.RemoverOrganisationControl);
			yield return nameof(EntryInstructionDetailsBasicUserControlBag.RequestLabel);
			yield return nameof(EntryInstructionDetailsBasicUserControlBag.RequestTypeDropEdit);
			yield return nameof(EntryInstructionDetailsBasicUserControlBag.IndirectTypeDropEdit);
			yield return nameof(EntryInstructionDetailsBasicUserControlBag.NationalCheckBox);
			yield return nameof(EntryInstructionDetailsBasicUserControlBag.NumberOfDaysCalcEdit);
			yield return nameof(EntryInstructionDetailsBasicUserControlBag.JustificationTextBox);
		}
	}
}
