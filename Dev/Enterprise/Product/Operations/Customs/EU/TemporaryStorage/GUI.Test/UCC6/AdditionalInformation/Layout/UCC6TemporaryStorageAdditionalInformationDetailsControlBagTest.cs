using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageAdditionalInformationDetailsControlBag))]

	public class UCC6TemporaryStorageAdditionalInformationDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(UCC6TemporaryStorageAdditionalInformationDetailsUserControl.KindDropEdit);
				yield return nameof(UCC6TemporaryStorageAdditionalInformationDetailsUserControl.FullTypeCodeFindBox);
				yield return nameof(UCC6TemporaryStorageAdditionalInformationDetailsUserControl.ReferenceTextBox);
				yield return nameof(UCC6TemporaryStorageAdditionalInformationDetailsUserControl.DescriptionTextBox);
				yield return nameof(UCC6TemporaryStorageAdditionalInformationDetailsUserControl.DetailTextBox);
				yield return nameof(UCC6TemporaryStorageAdditionalInformationDetailsUserControl.CurrencyDropEdit);
				yield return nameof(UCC6TemporaryStorageAdditionalInformationDetailsUserControl.AmountCalcEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => UCC6TemporaryStorageAdditionalInformationDetailsControlBag.Instance;
	}
}
