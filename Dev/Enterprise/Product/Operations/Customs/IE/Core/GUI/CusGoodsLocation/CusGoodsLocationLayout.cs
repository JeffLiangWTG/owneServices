using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public class CusGoodsLocationLayout : IPanelLayoutProviderWithExtensions
	{
		public CusGoodsLocationLayout()
		{
			Layout = CreateCusGoodsLocationLayout();
		}

		PanelLayout Layout { get; }

		#region IPanelLayoutProvider

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		IReadOnlyCollection<ILayoutExtension> IPanelLayoutProviderWithExtensions.Extensions => new[]
		{
			new EU.GUI.CusGoodsLocationWebAddressValidationExtension()
		};

		#endregion

		PanelLayout CreateCusGoodsLocationLayout()
		{
			var builder = new EU.GUI.CusGoodsLocationLayoutBuilder<CusGoodsLocation>();
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
			builder.Add(commonBag.ContactTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.PhoneTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.EmailTextBox, ControlWidthClass.Long);

			builder.SetVisibility(commonBag.ContactTextBox, ContactDetailControlsVisible, ContactDetailControlsVisibleDependingOn);
			builder.SetVisibility(commonBag.PhoneTextBox, ContactDetailControlsVisible, ContactDetailControlsVisibleDependingOn);
			builder.SetVisibility(commonBag.EmailTextBox, ContactDetailControlsVisible, ContactDetailControlsVisibleDependingOn);

			builder.SetControlVisibility(commonBag.PostcodeTextBox,
				Customs.Business.CusGoodsLocationQualifierList.Codes.UnLocode,
				Customs.Business.CusGoodsLocationQualifierList.Codes.PostcodeAddress,
				Customs.Business.CusGoodsLocationQualifierList.Codes.Address);
			builder.SetControlVisibility(commonBag.StreetAndNumberWithAddressValidationUserControl,
				Customs.Business.CusGoodsLocationQualifierList.Codes.UnLocode,
				Customs.Business.CusGoodsLocationQualifierList.Codes.Address);
			builder.SetControlVisibility(commonBag.CityTextBox,
				Customs.Business.CusGoodsLocationQualifierList.Codes.UnLocode,
				Customs.Business.CusGoodsLocationQualifierList.Codes.Address);
			builder.SetControlVisibility(commonBag.AdditionalIdentifierTextBox,
				Customs.Business.CusGoodsLocationQualifierList.Codes.UnLocode);

			return builder.Build();
		}

		static bool ContactDetailControlsVisible(CusGoodsLocation goodsLocation) => goodsLocation.UCCVersionProvider is ICusGoodsLocationProviderWithUCCVersion uCCVersionProvider && !uCCVersionProvider.IsUCC5;

		static ZPropertyInfo ContactDetailControlsVisibleDependingOn(CusGoodsLocation goodsLocation) => goodsLocation.UCCVersionProvider?.IsUCC5Info;
	}
}
