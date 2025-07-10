using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(EntryDetailsControlBag))]
sealed class EntryDetailsControlBagTest : ControlBagAbstractTest
{
	protected override ControlBag GetControlBagForTesting() => EntryDetailsControlBag.Instance;

	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(EntryDetailsControlBag.EntryTypeTextBox);
			yield return nameof(EntryDetailsControlBag.ReferenceNumberTextBox);
			yield return nameof(EntryDetailsControlBag.IssueDateDateEdit);
			yield return nameof(EntryDetailsControlBag.IncotermTextBox);
			yield return nameof(EntryDetailsControlBag.GrossWeightUserControl);
			yield return nameof(EntryDetailsControlBag.NetWeightUserControl);
			yield return nameof(EntryDetailsControlBag.CustomsQuantityUserControl);
			yield return nameof(EntryDetailsControlBag.InvoiceAmountUserControl);
			yield return nameof(EntryDetailsControlBag.FreightAdjustmentCalcEdit);
			yield return nameof(EntryDetailsControlBag.MessageStatusUserControl);
			yield return nameof(EntryDetailsControlBag.ControlChannelDropEdit);
			yield return nameof(EntryDetailsControlBag.MessageTypeDropEdit);
			yield return nameof(EntryDetailsControlBag.WarehouseStatusDropEdit);
			yield return nameof(EntryDetailsControlBag.RegistrationNumberTextBox);
			yield return nameof(EntryDetailsControlBag.CustomsOfficeTextBox);
			yield return nameof(EntryDetailsControlBag.ReleaseCodeTextBox);
			yield return nameof(EntryDetailsControlBag.A93Grid);
			yield return nameof(EntryDetailsControlBag.ExitDateDateEdit);
			yield return nameof(EntryDetailsControlBag.ExitOfficeUserControl);
			yield return nameof(EntryDetailsControlBag.ExitStatusUserControl);
			yield return nameof(EntryDetailsControlBag.ReferenceLabel);
			yield return nameof(EntryDetailsControlBag.TotalsLabel);
			yield return nameof(EntryDetailsControlBag.StatusLabel);
			yield return nameof(EntryDetailsControlBag.CustomsLabel);
			yield return nameof(EntryDetailsControlBag.A93Label);
			yield return nameof(EntryDetailsControlBag.ExitLabel);
			yield return nameof(EntryDetailsControlBag.SubmittedDateDateEdit);
			yield return nameof(EntryDetailsControlBag.MRNTextBox);
			yield return nameof(EntryDetailsControlBag.ReleaseDateDateEdit);
			yield return nameof(EntryDetailsControlBag.EntryStatusDropEdit);
		}
	}
}
