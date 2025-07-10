using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(G5V1TemporaryStoragePreviousDocumentsDetailsControlBag))]
	sealed class G5V1TemporaryStoragePreviousDocumentsDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(G5V1TemporaryStoragePreviousDocumentsDetailsControlBag.ReferenceNumber2TextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => G5V1TemporaryStoragePreviousDocumentsDetailsControlBag.Instance;
	}
}
