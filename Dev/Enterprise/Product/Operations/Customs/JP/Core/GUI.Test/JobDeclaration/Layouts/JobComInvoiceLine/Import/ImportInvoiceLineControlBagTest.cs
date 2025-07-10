using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(ImportInvoiceLineControlBag))]
	sealed class ImportInvoiceLineControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ImportInvoiceLineControlBag.TariffFindBox);
				yield return nameof(ImportInvoiceLineControlBag.CertificateOfOriginPanel);
				yield return nameof(ImportInvoiceLineControlBag.DutyRateTextBox);
				yield return nameof(ImportInvoiceLineControlBag.ProcedureTextBox);
				yield return nameof(ImportInvoiceLineControlBag.StorageTypeDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ImportInvoiceLineControlBag.Instance;
	}
}
