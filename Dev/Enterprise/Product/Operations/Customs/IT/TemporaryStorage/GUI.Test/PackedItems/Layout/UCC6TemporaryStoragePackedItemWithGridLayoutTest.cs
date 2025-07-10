using System;
using System.Collections.Generic;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

[TestedType(typeof(UCC6TemporaryStoragePackedItemWithGridLayout))]
sealed class UCC6TemporaryStoragePackedItemWithGridLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumn;
			yield return SecondColumn;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumn
	{
		get
		{
			yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStoragePackedItemDetailsControlBag.Instance.SeqTextBox, ControlWidthClass.Medium);
			yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStoragePackedItemDetailsControlBag.Instance.TariffCodeFindBox, ControlWidthClass.Long);
			yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStoragePackedItemDetailsControlBag.Instance.GoodDescriptionTextBox, ControlWidthClass.Long);
			yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStoragePackedItemDetailsControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStoragePackedItemDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Long);
			yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStoragePackedItemDetailsControlBag.Instance.CustomsSecondQuantityDropEdit, ControlWidthClass.Long);
			yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStoragePackedItemDetailsControlBag.Instance.CusCodeCodeFindBox, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumn
	{
		get
		{
			yield return (UCC6TemporaryStoragePackedItemDetailsControlBag.Instance.RegistrationNoTextBox, ControlWidthClass.Long);
			yield return (UCC6TemporaryStoragePackedItemDetailsControlBag.Instance.ReleaseDateEdit, ControlWidthClass.Long);
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.TemporaryStorage.GUI.UCC6TemporaryStoragePackedItemDetailsBuilder<TemporaryStorageHeader>();
	protected override Type ExpectedGridUserControlType => typeof(EU.TemporaryStorage.GUI.UCC6TemporaryStoragePackedItemGridControl);
}
