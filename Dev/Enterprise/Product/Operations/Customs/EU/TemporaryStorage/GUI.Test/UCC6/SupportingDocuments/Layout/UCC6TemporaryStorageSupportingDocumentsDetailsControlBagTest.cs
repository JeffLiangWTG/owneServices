using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageSupportingDocumentsDetailsControlBag))]
	public class UCC6TemporaryStorageSupportingDocumentsDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(UCC6TemporaryStorageSupportingDocumentsDetailsUserControl.CodeCodeFindBox);
				yield return nameof(UCC6TemporaryStorageSupportingDocumentsDetailsUserControl.ReferenceNumberTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => UCC6TemporaryStorageSupportingDocumentsDetailsControlBag.Instance;
	}
}
