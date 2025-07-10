using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(SEDDetailsGroupBoxControlBag))]
	sealed class SEDDetailsGroupBoxControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(SEDDetailsGroupBoxControlBag.CertificateOfOriginGroupBox);
				yield return nameof(SEDDetailsGroupBoxControlBag.ManufacturerGroupBox);
				yield return nameof(SEDDetailsGroupBoxControlBag.ImporterGroupBox);
				yield return nameof(SEDDetailsGroupBoxControlBag.SupplierGroupBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => SEDDetailsGroupBoxControlBag.Instance;
	}
}
