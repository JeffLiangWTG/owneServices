using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(TemporaryStorageUserControlBag))]
	sealed class TemporaryStorageUserControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TemporaryStorageUserControlBag.ManifestTypeDropEdit);
				yield return nameof(TemporaryStorageUserControlBag.CustomsOfficeofLodgementCodeFindBox);
				yield return nameof(TemporaryStorageUserControlBag.CustomsOfficeOfFirstEntryCodeFindBox);
				yield return nameof(TemporaryStorageUserControlBag.RepresentativeStatusDropEdit);
				yield return nameof(TemporaryStorageUserControlBag.BorderTransportTypeDropEdit);
				yield return nameof(TemporaryStorageUserControlBag.BorderTransportIDTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TemporaryStorageUserControlBag.Instance;
	}
}
