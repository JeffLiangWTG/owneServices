using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7BillPartiesControlBag))]
	sealed class EUH7BillPartiesControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(EUH7BillPartiesControlBag.ImporterSeparatorUserControl);
				yield return nameof(EUH7BillPartiesControlBag.ExporterSeparatorUserControl);
				yield return nameof(EUH7BillPartiesControlBag.SellerSeparatorUserControl);
				yield return nameof(EUH7BillPartiesControlBag.IdentificationNoTextBox);
				yield return nameof(EUH7BillPartiesControlBag.ImporterIdentificationTypeDropEdit);
				yield return nameof(EUH7BillPartiesControlBag.CountryOfImportDropEdit);
				yield return nameof(EUH7BillPartiesControlBag.CountryOfExportDropEdit);
				yield return nameof(EUH7BillPartiesControlBag.CountryOfSellerDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => EUH7BillPartiesControlBag.Instance;
	}
}
