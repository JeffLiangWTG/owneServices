using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStoragePackageDetailsControlBag))]
	public class UCC6TemporaryStoragePackageDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(UCC6TemporaryStoragePackageDetailsControl.containerPKGuidDropEditWithFixedWidth);
				yield return nameof(UCC6TemporaryStoragePackageDetailsControl.packUQDropEditWithFixedWidth);
				yield return nameof(UCC6TemporaryStoragePackageDetailsControl.packQtyCalcEdit);
				yield return nameof(UCC6TemporaryStoragePackageDetailsControl.marksAndNumbersTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => UCC6TemporaryStoragePackageDetailsControlBag.Instance;
	}
}
