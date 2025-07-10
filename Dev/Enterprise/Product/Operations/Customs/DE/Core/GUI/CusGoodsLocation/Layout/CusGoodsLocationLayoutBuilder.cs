using CargoWise.Types;
using Enterprise.Customs.Business;
using CusGoodsLocation = Enterprise.Customs.DE.Business.CusGoodsLocation;

namespace Enterprise.Customs.DE.GUI
{
	public class CusGoodsLocationLayoutBuilder : EU.GUI.CusGoodsLocationLayoutBuilder<CusGoodsLocation>
	{
		readonly EU.GUI.CusGoodsLocationControlBag euBag = EU.GUI.CusGoodsLocationControlBag.Instance;

		readonly CusGoodsLocationControlBag deBag = CusGoodsLocationControlBag.Instance;

		protected override int MaxColumns => 1;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();

			SetVisibility(euBag.UnlocoCodeFindBox, i => i.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.UnLocode, i => i.CGL_QualifierInfo);
			SetVisibility(euBag.GeoLocationLatitudeTextBox, i => i.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.GnssCoordinates, i => i.CGL_QualifierInfo);
			SetVisibility(euBag.GeoLocationLongitudeTextBox, i => i.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.GnssCoordinates, i => i.CGL_QualifierInfo);

			SetVisibility(deBag.AdditionalIdentifierDropEdit, i => i.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber, i => i.CGL_QualifierInfo);

			SetVisibility(deBag.LoadingPlaceTextBox, i => i.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.Address, i => i.CGL_QualifierInfo);
			SetVisibility(euBag.StreetAndNumberTextBox, i => i.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.Address, i => i.CGL_QualifierInfo);
			SetVisibility(euBag.CityTextBox, i => i.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.Address, i => i.CGL_QualifierInfo);
			SetVisibility(euBag.PostcodeTextBox, i => i.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.Address, i => i.CGL_QualifierInfo);
			SetVisibility(euBag.CountryCodeFindBox, i => i.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.Address, i => i.CGL_QualifierInfo);

			SetControlVisibility(euBag.ContactTextBox, commonQualifiers);
			SetControlVisibility(euBag.PhoneTextBox, commonQualifiers);
			SetControlVisibility(euBag.EmailTextBox, commonQualifiers);
		}

		readonly ZString[] commonQualifiers = new ZString[] { CusGoodsLocationQualifierList.Codes.UnLocode, CusGoodsLocationQualifierList.Codes.GnssCoordinates, CusGoodsLocationQualifierList.Codes.AuthorizationNumber, CusGoodsLocationQualifierList.Codes.Address };
	}
}
