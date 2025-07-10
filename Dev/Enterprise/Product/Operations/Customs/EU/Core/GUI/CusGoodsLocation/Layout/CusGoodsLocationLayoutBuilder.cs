using CargoWise.Common;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using CusGoodsLocation = Enterprise.Customs.EU.Business.CusGoodsLocation;

namespace Enterprise.Customs.EU.GUI
{
	public class CusGoodsLocationLayoutBuilder<T> : ColumnLayoutBuilder<T, CusGoodsLocationControlBag> where T : CusGoodsLocation
	{
		public override CusGoodsLocationControlBag CommonBag => CusGoodsLocationControlBag.Instance;

		protected override int MaxColumns => 1;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();

			SetControlVisibility(CommonBag.AdditionalIdentifierTextBox,
				CusGoodsLocationQualifierList.Codes.PostcodeAddress,
				CusGoodsLocationQualifierList.Codes.EoriNumber,
				CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
			SetControlVisibility(CommonBag.PostcodeTextBox,
				CusGoodsLocationQualifierList.Codes.PostcodeAddress,
				CusGoodsLocationQualifierList.Codes.Address);
			SetControlVisibility(CommonBag.CountryCodeFindBox,
				CusGoodsLocationQualifierList.Codes.PostcodeAddress,
				CusGoodsLocationQualifierList.Codes.Address);
			SetControlVisibility(CommonBag.UnlocoCodeFindBox, CusGoodsLocationQualifierList.Codes.UnLocode);
			SetControlVisibility(CommonBag.CustomsOfficeCodeFindBox, CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier);
			SetControlVisibility(CommonBag.GeoLocationLatitudeTextBox, CusGoodsLocationQualifierList.Codes.GnssCoordinates);
			SetControlVisibility(CommonBag.GeoLocationLongitudeTextBox, CusGoodsLocationQualifierList.Codes.GnssCoordinates);
			SetControlVisibility(CommonBag.OrganisationFindBox,
				CusGoodsLocationQualifierList.Codes.EoriNumber,
				CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
			SetControlVisibility(CommonBag.EoriNumberTextBox, CusGoodsLocationQualifierList.Codes.EoriNumber);
			SetAuthorisationNumberControlsVisibility();
			SetControlVisibility(CommonBag.StreetAndNumberWithAddressValidationUserControl, CusGoodsLocationQualifierList.Codes.Address);
			SetControlVisibility(CommonBag.CityTextBox, CusGoodsLocationQualifierList.Codes.Address);
			SetControlVisibleWhenQualifierIsNotEmpty(CommonBag.ContactTextBox);
			SetControlVisibleWhenQualifierIsNotEmpty(CommonBag.PhoneTextBox);
			SetControlVisibleWhenQualifierIsNotEmpty(CommonBag.EmailTextBox);
		}

		protected virtual void SetAuthorisationNumberControlsVisibility()
		{
			SetControlVisibility(CommonBag.AuthorizationCodeFindBox, CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
			SetControlVisibility(CommonBag.AuthorizationDropEdit, CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
		}

		public void SetControlVisibility(ControlReference control, params ZString[] visibleForQualifiers)
		{
			SetVisibility(control, l => l.CGL_Qualifier.In(visibleForQualifiers), l => l.CGL_QualifierInfo);
		}

		public void SetControlVisibleWhenQualifierIsNotEmpty(ControlReference control)
		{
			SetVisibility(control,
				isVisible: l => l.ContactPersonDataVisible,
				dependencies: l => l.CGL_QualifierInfo);
		}

		protected override void SetDefaultCaptions()
		{
			base.SetDefaultCaptions();
			SetCaption(CommonBag.AdditionalIdentifierTextBox, GetAdditionalIdentifierTextBoxCaption, l => l.CGL_QualifierInfo);
		}

		ResourceStringData GetAdditionalIdentifierTextBoxCaption(CusGoodsLocation location)
		{
			switch (location.CGL_Qualifier)
			{
				case CusGoodsLocationQualifierList.Codes.PostcodeAddress:
					return Res.GetData("6A61C8D3-250B-4AE4-BA39-42D13C212D57", "House Number");
				case CusGoodsLocationQualifierList.Codes.EoriNumber:
				case CusGoodsLocationQualifierList.Codes.AuthorizationNumber:
					return Res.GetData("A754FD51-3C20-43CB-B50C-592F7F4F7A66", "Additional Identifier");
				default:
					return new ResourceStringData();
			}
		}
	}
}
