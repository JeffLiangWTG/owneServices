using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ValuationEntryDetailsControlBag))]
	sealed class ValuationEntryDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ValuationEntryDetailsControlBag.Instance.EntryNumberTextBox);
				yield return nameof(ValuationEntryDetailsControlBag.Instance.MessageStatusDropEdit);
				yield return nameof(ValuationEntryDetailsControlBag.Instance.EntryStatusDropEdit);
				yield return nameof(ValuationEntryDetailsControlBag.Instance.EntrySubmittedDateDateEdit);
				yield return nameof(ValuationEntryDetailsControlBag.Instance.AcceptedDateDateEdit);
				yield return nameof(ValuationEntryDetailsControlBag.Instance.ApprovalDateDateEdit);
				yield return nameof(ValuationEntryDetailsControlBag.Instance.EffectiveToDateDateEdit);
				yield return nameof(ValuationEntryDetailsControlBag.Instance.ApprovalNumberTextBox);
				yield return nameof(ValuationEntryDetailsControlBag.Instance.ResultReasonTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ValuationEntryDetailsControlBag.Instance;
	}
}
