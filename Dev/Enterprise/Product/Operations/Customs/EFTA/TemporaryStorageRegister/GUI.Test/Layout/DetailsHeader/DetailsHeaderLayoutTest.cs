using System.Collections.Generic;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Testing;

[TestedType(typeof(DetailsHeaderLayout))]
sealed class DetailsHeaderLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
			yield return ThirdColumnControls;
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (DetailsHeaderControlBag.Instance.ATBNumberTextBox, ControlWidthClass.Long);
			yield return (DetailsHeaderControlBag.Instance.PreviousReferenceTypeDropEdit, ControlWidthClass.Long);
			yield return (DetailsHeaderControlBag.Instance.StatusDropEdit, ControlWidthClass.Long);
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (DetailsHeaderControlBag.Instance.ArrvialDateEdit, ControlWidthClass.Long);
			yield return (DetailsHeaderControlBag.Instance.PresentationDateEdit, ControlWidthClass.Long);
			yield return (DetailsHeaderControlBag.Instance.PreviousReferenceNumberTextBox, ControlWidthClass.Long);
			yield return (DetailsHeaderControlBag.Instance.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (DetailsHeaderControlBag.Instance.CustomerReferenceTextBox, ControlWidthClass.Long);
		}
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new DetailsHeaderCommonLayoutBuilder<CusTempStorageRegHeader>();
}
