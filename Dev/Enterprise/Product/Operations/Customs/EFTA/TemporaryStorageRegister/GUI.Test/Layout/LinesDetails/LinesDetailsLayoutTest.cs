using System.Collections.Generic;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Testing;

[TestedType(typeof(LinesDetailsLayout))]
sealed class LinesDetailsLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (LinesDetailsControlBag.Instance.LineNumberCalcEdit, ControlWidthClass.Long);
			yield return (LinesDetailsControlBag.Instance.LocationofGoodsTextBox, ControlWidthClass.Long);
			yield return (LinesDetailsControlBag.Instance.OwnerReferenceNumberTextBox, ControlWidthClass.Long);
			yield return (LinesDetailsControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Long);
			yield return (LinesDetailsControlBag.Instance.CustodianEORIBranchUserControl, ControlWidthClass.Long);
			yield return (LinesDetailsControlBag.Instance.DisposalEntitledTraderEORIBranchUserControl, ControlWidthClass.Long);
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (LinesDetailsControlBag.Instance.LimitDateEdit, ControlWidthClass.Auto);
			yield return (LinesDetailsControlBag.Instance.PackagesRemainingCalcEdit, ControlWidthClass.Auto);
			yield return (LinesDetailsControlBag.Instance.PackageTypeDropEdit, ControlWidthClass.Auto);
			yield return (LinesDetailsControlBag.Instance.OwnerReferenceTypeDropEdit, ControlWidthClass.Auto);
			yield return (LinesDetailsControlBag.Instance.CustomsStatusDropEdit, ControlWidthClass.Auto);
			yield return (LinesDetailsControlBag.Instance.UnionStatusDropEdit, ControlWidthClass.Auto);
		}
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new LinesDetailsCommonLayoutBuilder<CusTempStorageRegHeader>();
}
