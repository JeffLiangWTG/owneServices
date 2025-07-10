using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStoragePreviousDocumentsDetailsControlBag))]
	sealed class UCC6TemporaryStoragePreviousDocumentsDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(UCC6TemporaryStoragePreviousDocumentsDetailsControlBag.TypeCodeFindBox);
				yield return nameof(UCC6TemporaryStoragePreviousDocumentsDetailsControlBag.ReferenceNumberTextBox);
				yield return nameof(UCC6TemporaryStoragePreviousDocumentsDetailsControlBag.GoodItemIdentifierCalcEdit);
				yield return nameof(UCC6TemporaryStoragePreviousDocumentsDetailsControlBag.PackageCalcDropEdit);
				yield return nameof(UCC6TemporaryStoragePreviousDocumentsDetailsControlBag.QuantityCalcDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => UCC6TemporaryStoragePreviousDocumentsDetailsControlBag.Instance;
	}
}
