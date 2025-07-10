using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(G5V1TemporaryStoragePackedItemDetailsControlBag))]
	public class G5V1TemporaryStoragePackedItemDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(G5V1TemporaryStoragePackedItemDetailsControlBag.UCRTextBox);
				yield return nameof(G5V1TemporaryStoragePackedItemDetailsControlBag.PresentationDateEdit);
				yield return nameof(G5V1TemporaryStoragePackedItemDetailsControlBag.MissingCheckBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => G5V1TemporaryStoragePackedItemDetailsControlBag.Instance;
	}
}
