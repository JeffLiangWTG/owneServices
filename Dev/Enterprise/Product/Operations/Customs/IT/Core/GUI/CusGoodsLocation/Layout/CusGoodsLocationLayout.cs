using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using QualifierList = Enterprise.Customs.Business.CusGoodsLocationQualifierList;

namespace Enterprise.Customs.IT.GUI;

sealed class CusGoodsLocationLayout : IPanelLayoutProviderWithExtensions
{
	public CusGoodsLocationLayout()
	{
		Layout = CreateCusGoodsLocationLayout();
	}

	PanelLayout Layout { get; }

	#region IPanelLayoutProviderWithExtensions

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	IReadOnlyCollection<ILayoutExtension> IPanelLayoutProviderWithExtensions.Extensions => new[]
	{
		new CusGoodsLocationWebAddressValidationExtension()
	};

	#endregion

	PanelLayout CreateCusGoodsLocationLayout()
	{
		var builder = new CusGoodsLocationLayoutBuilder<CusGoodsLocation>();
		var commonBag = builder.CommonBag;
		var itBag = CusGoodsLocationControlBag.Instance;

		builder.AddControlBag(itBag);
		builder.AddColumn();
		builder.Add(commonBag.QualifierDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.TypeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.OrganisationFindBox, ControlWidthClass.Long);
		builder.Add(itBag.OrganizationAddressControl, ControlWidthClass.Long);
		builder.Add(itBag.OverrideCheckBox, ControlWidthClass.Long);
		builder.Add(commonBag.EoriNumberTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.AuthorizationCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.AdditionalIdentifierTextBox, ControlWidthClass.Long);
		builder.Add(itBag.AdditionalIdentifierDropEdit, ControlWidthClass.Long);
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

		builder.SetControlVisibility(commonBag.AdditionalIdentifierTextBox, QualifierList.Codes.PostcodeAddress, QualifierList.Codes.EoriNumber);
		builder.SetControlVisibility(itBag.OrganizationAddressControl, QualifierList.Codes.Address);
		builder.SetControlVisibility(itBag.OverrideCheckBox, QualifierList.Codes.Address);
		HideControlForQualifiers(builder, commonBag.ContactTextBox, QualifierList.Codes.CustomsOfficeIdentifier);
		HideControlForQualifiers(builder, commonBag.PhoneTextBox, QualifierList.Codes.CustomsOfficeIdentifier);
		HideControlForQualifiers(builder, commonBag.EmailTextBox, QualifierList.Codes.CustomsOfficeIdentifier);

		builder.SetVisibility(itBag.AdditionalIdentifierDropEdit, l => l.IsPlaceCodeAvailable, l => l.CGL_QualifierInfo, l => l.CGL_TypeInfo);

		return builder.Build();
	}

	void HideControlForQualifiers(CusGoodsLocationLayoutBuilder<CusGoodsLocation> builder, ControlReference control, params ZString[] hiddenForQualifiers)
	{
		builder.SetVisibility(control, l => !l.CGL_Qualifier.IsEmpty && !l.CGL_Qualifier.In(hiddenForQualifiers), l => l.CGL_QualifierInfo);
	}
}
