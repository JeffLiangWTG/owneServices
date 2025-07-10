using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(G5V1TemporaryStorageDetailsUserControlBag))]
	sealed class G5V1TemporaryStorageDetailsUserControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(G5V1TemporaryStorageDetailsUserControl.LRNTextBox);
				yield return nameof(G5V1TemporaryStorageDetailsUserControl.MRNTextBox);
				yield return nameof(G5V1TemporaryStorageDetailsUserControl.CustomsStatusDropEdit);
				yield return nameof(G5V1TemporaryStorageDetailsUserControl.MessageStatusDropEdit);
				yield return nameof(G5V1TemporaryStorageDetailsUserControl.CircuitTextBox);
				yield return nameof(G5V1TemporaryStorageDetailsUserControl.AcceptanceDateDateEdit);
				yield return nameof(G5V1TemporaryStorageDetailsUserControl.ClearanceNumberTextBox);
				yield return nameof(G5V1TemporaryStorageDetailsUserControl.DsdtSdFormatHasUrlUserControl);
				yield return nameof(G5V1TemporaryStorageDetailsUserControl.DsdtSdFormatNoUrlUserControl);
				yield return nameof(G5V1TemporaryStorageDetailsUserControl.DsdtMrnBindingMemberUserControl);
				yield return nameof(G5V1TemporaryStorageDetailsUserControl.DsdtMrnNumberTextBox);
				yield return nameof(G5V1TemporaryStorageDetailsUserControl.LAMEEntryNumberTextBox);
				yield return nameof(G5V1TemporaryStorageDetailsUserControl.LAMEEntryDateDateEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => G5V1TemporaryStorageDetailsUserControlBag.Instance;
	}
}
