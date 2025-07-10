using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using CusGoodsLocation = Enterprise.Customs.EU.Business.CusGoodsLocation;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class CusGoodsLocationLayout : IPanelLayoutProviderWithExtensions
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
			new CusGoodsLocationWebAddressValidationExtension()
		};

		#endregion

		PanelLayout CreateCusGoodsLocationLayout()
		{
			var builder = new CusGoodsLocationLayoutBuilder<CusGoodsLocation>();
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

			return builder.Build();
		}
	}
}
