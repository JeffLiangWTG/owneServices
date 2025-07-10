using System.Collections.Generic;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

[TestedType(typeof(CusGoodsLocationLayout))]
sealed class CusGoodsLocationLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.QualifierDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.TypeDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.OrganisationFindBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.AuthorizationCodeFindBox, ControlWidthClass.Long);
			yield return (CusGoodsLocationControlBag.Instance.AdditionalIdentifierDropEdit, ControlWidthClass.Long);
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new CusGoodsLocationLayoutBuilder<Business.CusGoodsLocation>();
}
