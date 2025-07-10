using System.Collections.Generic;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.H7.GUI.Testing
{
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
				yield return (CusGoodsLocationControlBag.Instance.QualifierDropEdit, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.StreetAndNumberWithAddressValidationUserControl, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.CountryCodeFindBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.PostcodeTextBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.CityTextBox, ControlWidthClass.Long);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CusGoodsLocationLayoutBuilder<CusGoodsLocation>();
	}
}
