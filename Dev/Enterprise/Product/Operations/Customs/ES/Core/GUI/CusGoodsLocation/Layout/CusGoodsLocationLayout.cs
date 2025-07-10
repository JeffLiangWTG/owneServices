using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;
using CusGoodsLocation = Enterprise.Customs.ES.Business.CusGoodsLocation;

namespace Enterprise.Customs.ES.GUI
{
	public sealed class CusGoodsLocationLayout : IPanelLayoutProviderWithExtensions
	{
		public CusGoodsLocationLayout()
		{
			Layout = CreateCusGoodsLocationLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		#region IPanelLayoutProviderWithExtensions

		IReadOnlyCollection<ILayoutExtension> IPanelLayoutProviderWithExtensions.Extensions => new[]
		{
			new CusGoodsLocationWebAddressValidationExtension()
		};

		#endregion

		PanelLayout CreateCusGoodsLocationLayout()
		{
			var builder = new CusGoodsLocationLayoutBuilder<CusGoodsLocation>();
			var commonBag = builder.CommonBag;
			var esBag = CusGoodsLocationControlBag.Instance;
			builder.AddControlBag(esBag);

			builder.AddColumn();
			builder.Add(commonBag.QualifierDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.OrganisationFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.EoriNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.AuthorizationCodeFindBox, ControlWidthClass.Long);
			builder.Add(esBag.ESAuthorizationCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.AdditionalIdentifierTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.StreetAndNumberWithAddressValidationUserControl, ControlWidthClass.Long);
			builder.Add(commonBag.CityTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.PostcodeTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.CountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.UnlocoCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.GeoLocationLatitudeTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.GeoLocationLongitudeTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ContactTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.PhoneTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.EmailTextBox, ControlWidthClass.Long);

			SetAuthorizationVisibility(builder, commonBag.AuthorizationCodeFindBox, esBag.ESAuthorizationCodeFindBox);
			SetControlVisibleContactPhoneEmail(builder, commonBag.ContactTextBox);
			SetControlVisibleContactPhoneEmail(builder, commonBag.PhoneTextBox);
			SetControlVisibleContactPhoneEmail(builder, commonBag.EmailTextBox);

			return builder.Build();
		}

		void SetControlVisibleContactPhoneEmail(CusGoodsLocationLayoutBuilder<CusGoodsLocation> builder, ControlReference control)
		{
			builder.SetVisibility(control, l => !l.CGL_Qualifier.IsEmpty && l.NamePhoneAndEmailVisible, l => l.CGL_QualifierInfo, l => l.CGL_TypeInfo, l => l.Address.AuthorisationNumberInfo);
		}

		void SetAuthorizationVisibility(CusGoodsLocationLayoutBuilder<CusGoodsLocation> builder, ControlReference controlCodeFindBox, ControlReference controlDropEdit)
		{
			builder.SetVisibility(controlCodeFindBox, l => l.CGL_Qualifier.Equals(CusGoodsLocationQualifierList.Codes.AuthorizationNumber) && !l.CGL_Type.Equals(CusGoodsLocationTypeList.Codes.AuthorizedPlace), l => l.CGL_QualifierInfo, l => l.CGL_TypeInfo);
			builder.SetVisibility(controlDropEdit, l => l.IsQualifierYAndTypeB, l => l.CGL_QualifierInfo, l => l.CGL_TypeInfo);
		}
	}
}
