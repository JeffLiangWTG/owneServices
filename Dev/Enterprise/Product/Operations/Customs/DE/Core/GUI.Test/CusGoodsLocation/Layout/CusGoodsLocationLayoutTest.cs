using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing;

[TestedType(typeof(CusGoodsLocationLayout))]
sealed class CusGoodsLocationLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 2;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
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
			yield return (CusGoodsLocationControlBag.Instance.AdditionalIdentifierDropEdit, ControlWidthClass.Long);
			yield return (CusGoodsLocationControlBag.Instance.LoadingPlaceTextBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.StreetAndNumberWithAddressValidationUserControl, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.CityTextBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.PostcodeTextBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.CountryCodeFindBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.UnlocoCodeFindBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.GeoLocationLatitudeTextBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.GeoLocationLongitudeTextBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.ContactTextBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.PhoneTextBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.EmailTextBox, ControlWidthClass.Long);
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new CusGoodsLocationLayoutBuilder();
}
