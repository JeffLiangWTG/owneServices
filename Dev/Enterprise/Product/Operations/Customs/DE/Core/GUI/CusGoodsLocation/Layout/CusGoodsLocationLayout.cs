using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
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
			new EU.GUI.CusGoodsLocationWebAddressValidationExtension()
		};

		#endregion

		PanelLayout CreateCusGoodsLocationLayout()
		{
			var builder = new CusGoodsLocationLayoutBuilder();
			var euBag = EU.GUI.CusGoodsLocationControlBag.Instance;
			builder.AddControlBag(euBag);
			var deBag = CusGoodsLocationControlBag.Instance;
			builder.AddControlBag(deBag);

			builder.AddColumn();
			builder.Add(euBag.QualifierDropEdit, ControlWidthClass.Long);
			builder.Add(deBag.AdditionalIdentifierDropEdit, ControlWidthClass.Long);
			builder.Add(deBag.LoadingPlaceTextBox, ControlWidthClass.Long);
			builder.Add(euBag.StreetAndNumberWithAddressValidationUserControl, ControlWidthClass.Long);
			builder.Add(euBag.CityTextBox, ControlWidthClass.Long);
			builder.Add(euBag.PostcodeTextBox, ControlWidthClass.Long);
			builder.Add(euBag.CountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.UnlocoCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.GeoLocationLatitudeTextBox, ControlWidthClass.Long);
			builder.Add(euBag.GeoLocationLongitudeTextBox, ControlWidthClass.Long);
			builder.Add(euBag.ContactTextBox, ControlWidthClass.Long);
			builder.Add(euBag.PhoneTextBox, ControlWidthClass.Long);
			builder.Add(euBag.EmailTextBox, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
