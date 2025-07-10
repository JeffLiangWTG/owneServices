using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(RefundForCancelControlBag))]
	sealed class RefundForCancelControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(RefundForCancelControlBag.CancelReasonDropEdit);
				yield return nameof(RefundForCancelControlBag.DisposalNumberTextBox);
				yield return nameof(RefundForCancelControlBag.DisposalDateEdit);
				yield return nameof(RefundForCancelControlBag.GoodsLocationDescriptionLongTextControl);
				yield return nameof(RefundForCancelControlBag.ResidualSubstanceDescriptionLongTextControl);
				yield return nameof(RefundForCancelControlBag.DamageSituationLongTextControl);
				yield return nameof(RefundForCancelControlBag.ExportEntryNumberTextBox);
				yield return nameof(RefundForCancelControlBag.ExportEntryLineNumberTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => RefundForCancelControlBag.Instance;
	}
}
