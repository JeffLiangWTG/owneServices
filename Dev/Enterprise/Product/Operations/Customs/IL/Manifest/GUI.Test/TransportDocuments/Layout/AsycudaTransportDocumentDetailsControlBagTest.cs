using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	[TestedType(typeof(AsycudaTransportDocumentDetailsControlBag))]
	public sealed class AsycudaTransportDocumentDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(AsycudaTransportDocumentDetailsControl.typeDropEdit);
				yield return nameof(AsycudaTransportDocumentDetailsControl.referenceTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => AsycudaTransportDocumentDetailsControlBag.Instance;
	}
}
