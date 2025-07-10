using CargoWise.Common;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing;

[TestedType(typeof(CusGoodsLocationLayoutBuilder))]
sealed class CusGoodsLocationLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<CusGoodsLocationLayoutBuilder, CusGoodsLocation, EU.GUI.CusGoodsLocationControlBag>
{
	protected override CusGoodsLocationLayoutBuilder GetColumnLayoutBuilderForTesting() => new CusGoodsLocationLayoutBuilder();

	protected override int ExpectedMaxColumns => 1;

	public void TestAuthorizationCodeFindBoxVisibility()
	{
		var location = Factory.New<CusGoodsLocation>();
		var layout = ((IPanelLayoutProvider)new Phase5GoodsLocationLayout()).Layout;
		var control = EU.GUI.CusGoodsLocationControlBag.Instance.AuthorizationCodeFindBox;
		var visibleForQualifiers = new[] { Customs.Business.CusGoodsLocationQualifierList.Codes.AuthorizationNumber };
		var visibleForTypes = new[] { Customs.Business.CusGoodsLocationTypeList.Codes.DesignatedLocation, Customs.Business.CusGoodsLocationTypeList.Codes.ApprovedPlace, Customs.Business.CusGoodsLocationTypeList.Codes.Other };

		foreach (var qualifier in new Customs.Business.CusGoodsLocationQualifierList().GetAllCodes())
		{
			foreach (var type in new Customs.Business.CusGoodsLocationTypeList().GetAllCodes())
			{
				location.CGL_Qualifier = qualifier;
				location.CGL_Type = type;
				AssertEquals($"CGL_Qualifier = '{qualifier}' and CGL_Type = '{type}'", qualifier.In(visibleForQualifiers) && type.In(visibleForTypes), layout.IsVisible(control, location));
			}
		}
	}

	public void TestAuthorizationDropEditVisibility()
	{
		var location = Factory.New<CusGoodsLocation>();
		var layout = ((IPanelLayoutProvider)new Phase5GoodsLocationLayout()).Layout;
		var control = EU.GUI.CusGoodsLocationControlBag.Instance.AuthorizationDropEdit;

		var visibleForQualifiers = new[] { Customs.Business.CusGoodsLocationQualifierList.Codes.AuthorizationNumber };
		var visibleForTypes = new[] { Customs.Business.CusGoodsLocationTypeList.Codes.AuthorizedPlace };

		foreach (var qualifier in new Customs.Business.CusGoodsLocationQualifierList().GetAllCodes())
		{
			foreach (var type in new Customs.Business.CusGoodsLocationTypeList().GetAllCodes())
			{
				location.CGL_Qualifier = qualifier;
				location.CGL_Type = type;
				AssertEquals($"CGL_Qualifier = '{qualifier}' and CGL_Type = '{type}'", qualifier.In(visibleForQualifiers) && type.In(visibleForTypes), layout.IsVisible(control, location));
			}
		}
	}
}
