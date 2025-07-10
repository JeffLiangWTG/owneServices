using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.TemporaryStorage.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStoragePackedItemWithGridLayout))]
	public class UCC6TemporaryStoragePackedItemWithGridLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumn;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumn
		{
			get
			{
				yield return (UCC6TemporaryStoragePackedItemDetailsControlBag.Instance.SeqTextBox, ControlWidthClass.Medium);
				yield return (UCC6TemporaryStoragePackedItemDetailsControlBag.Instance.TariffCodeFindBox, ControlWidthClass.Long);
				yield return (UCC6TemporaryStoragePackedItemDetailsControlBag.Instance.GoodDescriptionTextBox, ControlWidthClass.Long);
				yield return (UCC6TemporaryStoragePackedItemDetailsControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (UCC6TemporaryStoragePackedItemDetailsControlBag.Instance.CusCodeCodeFindBox, ControlWidthClass.Long);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new UCC6TemporaryStoragePackedItemDetailsBuilder<TemporaryStorageHeader>();
		protected override Type ExpectedGridUserControlType => typeof(UCC6TemporaryStoragePackedItemGridControl);
	}
}
