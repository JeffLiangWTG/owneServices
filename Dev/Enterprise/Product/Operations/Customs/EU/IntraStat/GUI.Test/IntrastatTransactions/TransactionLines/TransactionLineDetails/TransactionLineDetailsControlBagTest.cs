
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.GUI.Testing
{
	[TestedType(typeof(TransactionLineDetailsControlBag))]
	sealed class TransactionLineDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(TransactionLineDetailsControlBag.MassDropEdit);
				yield return nameof(TransactionLineDetailsControlBag.CountryOfOriginDropEdit);
				yield return nameof(TransactionLineDetailsControlBag.DescriptionOfGoodsTextBox);
				yield return nameof(TransactionLineDetailsControlBag.InvoiceValueDropEdit);
				yield return nameof(TransactionLineDetailsControlBag.RegionDropEdit);
				yield return nameof(TransactionLineDetailsControlBag.StatisticalValueDropEdit);
				yield return nameof(TransactionLineDetailsControlBag.SupplementaryUnitsCalcDropEdit);
				yield return nameof(TransactionLineDetailsControlBag.TariffFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => TransactionLineDetailsControlBag.Instance;
	}
}
