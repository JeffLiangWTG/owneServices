using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(InvoiceLineCertificateOfOriginControlBag))]
	sealed class InvoiceLineCertificateOfOriginControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return (nameof(InvoiceLineCertificateOfOriginControlBag.COOIndicatorDropEdit));
				yield return (nameof(InvoiceLineCertificateOfOriginControlBag.COODeterminationRuleDropEdit));
				yield return (nameof(InvoiceLineCertificateOfOriginControlBag.COOLabelLocationDropEdit));
				yield return (nameof(InvoiceLineCertificateOfOriginControlBag.FTATypeDropEdit));
			}
		}

		protected override ControlBag GetControlBagForTesting() => InvoiceLineCertificateOfOriginControlBag.Instance;
	}
}
