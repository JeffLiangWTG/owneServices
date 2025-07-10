using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStoragePackedItemDetailsControlBag))]
	public class UCC6TemporaryStoragePackedItemDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(UCC6TemporaryStoragePackedItemDetailsControl.SeqCalcEdit);
				yield return nameof(UCC6TemporaryStoragePackedItemDetailsControl.TariffCodeFindBox);
				yield return nameof(UCC6TemporaryStoragePackedItemDetailsControl.GoodsDescriptionTextBox);
				yield return nameof(UCC6TemporaryStoragePackedItemDetailsControl.GrossWeightCalcDropEdit);
				yield return nameof(UCC6TemporaryStoragePackedItemDetailsControl.NetWeightCalcDropEdit);
				yield return nameof(UCC6TemporaryStoragePackedItemDetailsControl.CusCodeFindBox);
				yield return nameof(UCC6TemporaryStoragePackedItemDetailsControl.CountryOfOriginDropEdit);
				yield return nameof(UCC6TemporaryStoragePackedItemDetailsControl.CustomsValueCalcDropEdit);
				yield return nameof(UCC6TemporaryStoragePackedItemDetailsControl.SupplementaryUnitsCalcDropEdit);
				yield return nameof(UCC6TemporaryStoragePackedItemDetailsControl.CustomsSecondQuantityDropEdit);
				yield return nameof(UCC6TemporaryStoragePackedItemDetailsControl.CustomsThirdQuantityDropEdit);
				yield return nameof(UCC6TemporaryStoragePackedItemDetailsControl.AdditionalSupplementaryCodesUserControl);
				yield return nameof(UCC6TemporaryStoragePackedItemDetailsControl.DutiesAndTaxesLabel);
				yield return nameof(UCC6TemporaryStoragePackedItemDetailsControl.DutiesAndTaxesGrid);
			}
		}

		protected override ControlBag GetControlBagForTesting() => UCC6TemporaryStoragePackedItemDetailsControlBag.Instance;
	}
}
