using System.Collections.Generic;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing
{
	[TestedType(typeof(CusGoodsLocationLayout))]
	sealed class CusGoodsLocationLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

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
				yield return (CusGoodsLocationControlBag.Instance.QualifierDropEdit, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.TypeDropEdit, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.OrganisationFindBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.EoriNumberTextBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.AuthorizationCodeFindBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.StreetAndNumberWithAddressValidationUserControl, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.CityTextBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.PostcodeTextBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.CountryCodeFindBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.UnlocoCodeFindBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.GeoLocationLatitudeTextBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.GeoLocationLongitudeTextBox, ControlWidthClass.Long);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CusGoodsLocationLayoutBuilder<ES.Business.CusGoodsLocation>();
	}
}
