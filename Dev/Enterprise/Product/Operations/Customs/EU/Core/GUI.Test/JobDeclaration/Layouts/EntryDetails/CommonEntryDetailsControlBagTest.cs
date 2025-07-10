using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(CommonEntryDetailsControlBag))]
	sealed class CommonEntryDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CommonEntryDetailsControlBag.TotalsLabel);
				yield return nameof(CommonEntryDetailsControlBag.CustomsLabel);
				yield return (nameof(CommonEntryDetailsControlBag.NoPacksCalcEdit));
				yield return (nameof(CommonEntryDetailsControlBag.GrossWeightCalcDropEdit));
				yield return (nameof(CommonEntryDetailsControlBag.NetWeightCalcDropEdit));
				yield return (nameof(CommonEntryDetailsControlBag.CustomsQuantityCalcDropEdit));
				yield return (nameof(CommonEntryDetailsControlBag.DutyCalcEdit));
				yield return (nameof(CommonEntryDetailsControlBag.VatCalcEdit));
				yield return (nameof(CommonEntryDetailsControlBag.EntryLinesCountCalcEdit));
				yield return (nameof(CommonEntryDetailsControlBag.ReferenceNumberTextBox));
				yield return (nameof(CommonEntryDetailsControlBag.SubmittedDateDateEdit));
				yield return (nameof(CommonEntryDetailsControlBag.MRNTextBox));
				yield return (nameof(CommonEntryDetailsControlBag.ReleaseDateDateEdit));
				yield return (nameof(CommonEntryDetailsControlBag.EntryStatusDropEdit));
				yield return nameof(CommonEntryDetailsControlBag.AcceptanceDateDateEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CommonEntryDetailsControlBag.Instance;
	}
}
