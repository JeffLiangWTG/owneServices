using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class PlaceOfUseOrProcessingLayout : IPanelLayoutProviderWithExtensions
	{
		public PlaceOfUseOrProcessingLayout()
		{
			Layout = CreatePlaceOfUseOrProcessingLayout();
		}

		PanelLayout Layout { get; }

		#region IPanelLayoutProvider

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		IReadOnlyCollection<ILayoutExtension> IPanelLayoutProviderWithExtensions.Extensions => new[]
		{
			new CusGoodsLocationWebAddressValidationExtension()
		};

		#endregion

		PanelLayout CreatePlaceOfUseOrProcessingLayout()
		{
			var builder = new CusGoodsLocationLayoutBuilder<PlaceOfUseOrProcessing>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.QualifierDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.OrganisationFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.EoriNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.AuthorizationCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.AdditionalIdentifierTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.StreetAndNumberWithAddressValidationUserControl, ControlWidthClass.Long);
			builder.Add(commonBag.CityTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.PostcodeTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.CountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.UnlocoCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.GeoLocationLatitudeTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.GeoLocationLongitudeTextBox, ControlWidthClass.Long);

			SetControlVisibility(builder, commonBag.AdditionalIdentifierTextBox,
				CusGoodsLocationQualifierList.Codes.PostcodeAddress,
				CusGoodsLocationQualifierList.Codes.EoriNumber,
				CusGoodsLocationQualifierList.Codes.AuthorizationNumber,
				CusGoodsLocationQualifierList.Codes.UnLocode,
				CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier,
				CusGoodsLocationQualifierList.Codes.Address);
			SetControlVisibility(builder, commonBag.PostcodeTextBox,
				CusGoodsLocationQualifierList.Codes.PostcodeAddress,
				CusGoodsLocationQualifierList.Codes.Address);
			SetControlVisibility(builder, commonBag.CountryCodeFindBox,
				CusGoodsLocationQualifierList.Codes.PostcodeAddress,
				CusGoodsLocationQualifierList.Codes.Address);
			SetControlVisibility(builder, commonBag.UnlocoCodeFindBox, CusGoodsLocationQualifierList.Codes.UnLocode);
			SetControlVisibility(builder, commonBag.CustomsOfficeCodeFindBox, CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier);
			SetControlVisibility(builder, commonBag.GeoLocationLatitudeTextBox, CusGoodsLocationQualifierList.Codes.GnssCoordinates);
			SetControlVisibility(builder, commonBag.GeoLocationLongitudeTextBox, CusGoodsLocationQualifierList.Codes.GnssCoordinates);
			SetControlVisibility(builder, commonBag.OrganisationFindBox,
				CusGoodsLocationQualifierList.Codes.EoriNumber,
				CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
			SetControlVisibility(builder, commonBag.EoriNumberTextBox, CusGoodsLocationQualifierList.Codes.EoriNumber);
			SetControlVisibility(builder, commonBag.AuthorizationCodeFindBox, CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
			SetControlVisibility(builder, commonBag.StreetAndNumberWithAddressValidationUserControl, CusGoodsLocationQualifierList.Codes.Address);
			SetControlVisibility(builder, commonBag.CityTextBox, CusGoodsLocationQualifierList.Codes.Address);

			builder.SetCaption(commonBag.AdditionalIdentifierTextBox, GetAdditionalIdentifierTextBoxCaption, l => l.CGL_QualifierInfo);

			return builder.Build();
		}

		void SetControlVisibility(CusGoodsLocationLayoutBuilder<PlaceOfUseOrProcessing> builder, ControlReference control, params ZString[] visibleForQualifiers)
		{
			builder.SetVisibility(control, l => l.CGL_Qualifier.In(visibleForQualifiers), l => l.CGL_QualifierInfo);
		}

		ResourceStringData GetAdditionalIdentifierTextBoxCaption(PlaceOfUseOrProcessing location)
		{
			switch (location.CGL_Qualifier)
			{
				case CusGoodsLocationQualifierList.Codes.PostcodeAddress:
					return Res.GetData("05F37DB7-B50F-4942-B29F-A990B885EB58", "House Number");
				case CusGoodsLocationQualifierList.Codes.EoriNumber:
				case CusGoodsLocationQualifierList.Codes.AuthorizationNumber:
					return Res.GetData("D1D8289F-04FA-40BA-981E-B5A6699E31B1", "Additional Identifier");
				default:
					return new ResourceStringData();
			}
		}
	}
}
