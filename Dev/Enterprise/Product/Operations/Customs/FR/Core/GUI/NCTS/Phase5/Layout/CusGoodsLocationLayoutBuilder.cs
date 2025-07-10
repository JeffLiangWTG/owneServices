using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.GUI.NCTS;

public class CusGoodsLocationLayoutBuilder : EU.GUI.CusGoodsLocationLayoutBuilder<Business.NCTS.CusGoodsLocation>
{
	protected override void SetAuthorisationNumberControlsVisibility()
	{
		SetVisibility(CommonBag.AuthorizationCodeFindBox, location => location.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber && location.CGL_Type != CusGoodsLocationTypeList.Codes.AuthorizedPlace, l => l.CGL_QualifierInfo, l => l.CGL_TypeInfo);
		SetVisibility(CommonBag.AuthorizationDropEdit, location => location.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber && location.CGL_Type == CusGoodsLocationTypeList.Codes.AuthorizedPlace, l => l.CGL_QualifierInfo, l => l.CGL_TypeInfo);
	}
}
