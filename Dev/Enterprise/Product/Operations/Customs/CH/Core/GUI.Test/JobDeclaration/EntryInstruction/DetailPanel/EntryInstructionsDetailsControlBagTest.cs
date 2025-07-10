using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(EntryInstructionsDetailsControlBag))]
class EntryInstructionsDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(EntryInstructionDetailPanelUserControl.DeclarationReasonDropEdit);
			yield return nameof(EntryInstructionDetailPanelUserControl.PartialDeliveryCheckBox);
			yield return nameof(EntryInstructionDetailPanelUserControl.TransportChargesMethodOfPaymentDropEdit);
			yield return nameof(EntryInstructionDetailPanelUserControl.ProcedureCodeDropEdit);
			yield return nameof(EntryInstructionDetailPanelUserControl.NextProcedureDropEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => EntryInstructionsDetailsControlBag.Instance;
}
